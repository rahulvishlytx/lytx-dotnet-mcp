import { access, readFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

export const WORKSPACE_ROOT = path.resolve(__dirname, '../../..');
export const CATALOG_PATH = path.join(WORKSPACE_ROOT, 'mcp', 'knowledge', 'standards', 'catalog.json');

function normalizeText(value) {
  return String(value ?? '').toLowerCase();
}

function tokenize(value) {
  return normalizeText(value)
    .split(/[^a-z0-9.#_-]+/i)
    .map(token => token.trim())
    .filter(token => token.length > 1);
}

function uniqueBy(items, selector) {
  const seen = new Set();
  return items.filter(item => {
    const key = selector(item);
    if (seen.has(key)) {
      return false;
    }

    seen.add(key);
    return true;
  });
}

export async function loadCatalog() {
  return JSON.parse(await readFile(CATALOG_PATH, 'utf8'));
}

export function resolveWorkspacePath(relativePath) {
  return path.resolve(WORKSPACE_ROOT, relativePath);
}

export async function ensureWorkspacePath(relativePath) {
  await access(resolveWorkspacePath(relativePath));
}

export async function readWorkspaceFile(relativePath) {
  return readFile(resolveWorkspacePath(relativePath), 'utf8');
}

export async function listDocuments() {
  const catalog = await loadCatalog();
  return [...catalog.documents].sort((left, right) => (left.order ?? 0) - (right.order ?? 0));
}

export async function getDocumentById(id) {
  const catalog = await loadCatalog();
  const document = catalog.documents.find(entry => entry.id === id);

  if (!document) {
    return null;
  }

  const content = await readWorkspaceFile(document.path);
  return { ...document, content };
}

export async function getCategoryBundle(categoryId) {
  const catalog = await loadCatalog();
  const category = catalog.categories.find(entry => entry.id === categoryId);

  if (!category) {
    return null;
  }

  const documents = await Promise.all(
    catalog.documents
      .filter(entry => entry.category === categoryId)
      .sort((left, right) => (left.order ?? 0) - (right.order ?? 0))
      .map(async entry => ({ ...entry, content: await readWorkspaceFile(entry.path) }))
  );

  const markdown = [`# ${category.title}`, '', category.description, ''];

  for (const document of documents) {
    markdown.push(`## ${document.title}`, '', document.content.trim(), '');
  }

  return {
    category,
    documents,
    markdown: markdown.join('\n')
  };
}

export async function getRawInstructionSource(sourceId = 'legacy-copilot-instructions') {
  const catalog = await loadCatalog();
  const source = catalog.rawInstructionSources.find(entry => entry.id === sourceId);

  if (!source) {
    return null;
  }

  const content = await readWorkspaceFile(source.path);
  return { ...source, content };
}

export async function buildCatalogSummary() {
  const catalog = await loadCatalog();

  return {
    version: catalog.version,
    title: catalog.title,
    description: catalog.description,
    categories: catalog.categories.map(category => ({
      ...category,
      documentCount: catalog.documents.filter(document => document.category === category.id).length
    })),
    documents: catalog.documents
      .slice()
      .sort((left, right) => (left.order ?? 0) - (right.order ?? 0))
      .map(document => ({
        id: document.id,
        title: document.title,
        category: document.category,
        tags: document.tags,
        uri: `standards://document/${document.id}`
      })),
    rawInstructionSources: catalog.rawInstructionSources.map(source => ({
      id: source.id,
      title: source.title,
      uri: `standards://source/${source.id}`
    }))
  };
}

function scoreDocument(queryTokens, document, content) {
  if (queryTokens.length === 0) {
    return 0;
  }

  const title = normalizeText(document.title);
  const category = normalizeText(document.category);
  const tags = (document.tags ?? []).map(normalizeText);
  const body = normalizeText(content);

  let score = 0;
  for (const token of queryTokens) {
    if (title.includes(token)) {
      score += 8;
    }

    if (category.includes(token)) {
      score += 5;
    }

    if (tags.some(tag => tag.includes(token))) {
      score += 4;
    }

    if (body.includes(token)) {
      score += 2;
    }
  }

  return score;
}

function buildExcerpt(content, queryTokens) {
  const lines = content.split(/\r?\n/);
  const matchIndex = lines.findIndex(line => queryTokens.some(token => normalizeText(line).includes(token)));

  if (matchIndex === -1) {
    return lines.slice(0, 8).join('\n').trim();
  }

  const start = Math.max(0, matchIndex - 2);
  const end = Math.min(lines.length, matchIndex + 4);
  return lines.slice(start, end).join('\n').trim();
}

export async function searchDocuments({ query, category, limit = 5 }) {
  const catalog = await loadCatalog();
  const queryTokens = tokenize(query);

  const filtered = category
    ? catalog.documents.filter(document => document.category === category)
    : catalog.documents;

  const matches = [];

  for (const document of filtered) {
    const content = await readWorkspaceFile(document.path);
    const score = scoreDocument(queryTokens, document, content);

    if (score <= 0) {
      continue;
    }

    matches.push({
      id: document.id,
      title: document.title,
      category: document.category,
      tags: document.tags,
      uri: `standards://document/${document.id}`,
      score,
      excerpt: buildExcerpt(content, queryTokens)
    });
  }

  return matches
    .sort((left, right) => right.score - left.score || left.title.localeCompare(right.title))
    .slice(0, limit);
}

export async function buildInstructionBundle({ story, techStack, categories = [], maxDocuments = 5 }) {
  const explicitCategories = uniqueBy(categories.filter(Boolean), category => category);
  const catalog = await loadCatalog();
  const selected = [];

  if (explicitCategories.length > 0) {
    for (const category of explicitCategories) {
      const docs = catalog.documents
        .filter(document => document.category === category)
        .sort((left, right) => (left.order ?? 0) - (right.order ?? 0));

      selected.push(...docs);
    }
  }

  const searchQuery = [story, techStack].filter(Boolean).join(' ');
  if (searchQuery.trim()) {
    const matches = await searchDocuments({ query: searchQuery, limit: maxDocuments });
    for (const match of matches) {
      const doc = catalog.documents.find(document => document.id === match.id);
      if (doc) {
        selected.push(doc);
      }
    }
  }

  const limitedDocs = uniqueBy(selected, document => document.id).slice(0, maxDocuments);
  const documents = await Promise.all(
    limitedDocs.map(async document => ({
      ...document,
      content: await readWorkspaceFile(document.path)
    }))
  );

  const sections = [];
  for (const document of documents) {
    sections.push(`## ${document.title}\n\n${document.content.trim()}`);
  }

  const text = [
    '# Company Standards Instruction Bundle',
    '',
    story ? `## Jira Story\n${story}` : null,
    techStack ? `## Tech Stack\n${techStack}` : null,
    '## Relevant Standards',
    documents.length > 0 ? sections.join('\n\n') : 'No direct standard match found. Fall back to catalog review and the raw instruction source.',
    '',
    '## Working Rules',
    '- Prefer existing company patterns before introducing new abstractions.',
    '- Keep acceptance criteria and tests aligned with the standard.',
    '- If no standard exists, document the gap and propose a new standard in a follow-up PR.'
  ]
    .filter(Boolean)
    .join('\n');

  return {
    story,
    techStack,
    documents: documents.map(document => ({
      id: document.id,
      title: document.title,
      category: document.category,
      uri: `standards://document/${document.id}`
    })),
    text
  };
}
