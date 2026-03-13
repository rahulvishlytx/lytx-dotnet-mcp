import {
  buildCatalogSummary,
  ensureWorkspacePath,
  getRawInstructionSource,
  loadCatalog,
  readWorkspaceFile
} from '../lib/standards-catalog.mjs';

async function main() {
  const catalog = await loadCatalog();
  const ids = new Set();

  for (const document of catalog.documents) {
    if (ids.has(document.id)) {
      throw new Error(`Duplicate document id '${document.id}' in catalog.`);
    }

    ids.add(document.id);
    await ensureWorkspacePath(document.path);
    const content = await readWorkspaceFile(document.path);

    if (!content.trim()) {
      throw new Error(`Document '${document.id}' is empty.`);
    }
  }

  for (const source of catalog.rawInstructionSources) {
    await ensureWorkspacePath(source.path);
  }

  const source = await getRawInstructionSource();
  if (!source?.content?.trim()) {
    throw new Error('Legacy instruction source is empty or missing.');
  }

  const summary = await buildCatalogSummary();
  console.log(`Validated ${summary.documents.length} standards across ${summary.categories.length} categories.`);
}

main().catch(error => {
  console.error(error.message || error);
  process.exit(1);
});
