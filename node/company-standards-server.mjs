import { McpServer, ResourceTemplate, StdioServerTransport } from '@modelcontextprotocol/server';
import * as z from 'zod/v4';
import {
  buildCatalogSummary,
  buildInstructionBundle,
  getCategoryBundle,
  getDocumentById,
  getRawInstructionSource,
  listDocuments,
  loadCatalog,
  searchDocuments
} from './lib/standards-catalog.mjs';

export function createCompanyStandardsServer() {
  const server = new McpServer({
    name: 'company-standards',
    version: '0.1.0'
  });

  server.registerResource(
    'standards-catalog',
    'standards://catalog',
    {
      title: 'Company standards catalog',
      description: 'Catalog of segmented company standards documents.',
      mimeType: 'application/json'
    },
    async uri => ({
      contents: [
        {
          uri: uri.href,
          text: JSON.stringify(await buildCatalogSummary(), null, 2)
        }
      ]
    })
  );

  server.registerResource(
    'legacy-instructions',
    'standards://source/legacy-copilot-instructions',
    {
      title: 'Legacy copilot instructions',
      description: 'Raw instruction source provided by engineering.',
      mimeType: 'text/markdown'
    },
    async uri => {
      const source = await getRawInstructionSource();
      return {
        contents: [
          {
            uri: uri.href,
            text: source?.content ?? 'Legacy source not found.'
          }
        ]
      };
    }
  );

  server.registerResource(
    'standard-document',
    new ResourceTemplate('standards://document/{id}', {
      list: async () => {
        const documents = await listDocuments();
        return {
          resources: documents.map(document => ({
            uri: `standards://document/${document.id}`,
            name: document.title,
            mimeType: 'text/markdown',
            description: `${document.category} standard`
          }))
        };
      }
    }),
    {
      title: 'Company standard document',
      description: 'Single segmented standard document.',
      mimeType: 'text/markdown'
    },
    async (uri, { id }) => {
      const document = await getDocumentById(id);
      return {
        contents: [
          {
            uri: uri.href,
            text: document?.content ?? `No standard document found for '${id}'.`
          }
        ]
      };
    }
  );

  server.registerResource(
    'category-bundle',
    new ResourceTemplate('standards://category/{category}', {
      list: async () => {
        const catalog = await loadCatalog();
        return {
          resources: catalog.categories.map(category => ({
            uri: `standards://category/${category.id}`,
            name: category.title,
            mimeType: 'text/markdown',
            description: category.description
          }))
        };
      }
    }),
    {
      title: 'Company standards category bundle',
      description: 'All standards within a category.',
      mimeType: 'text/markdown'
    },
    async (uri, { category }) => {
      const bundle = await getCategoryBundle(category);
      return {
        contents: [
          {
            uri: uri.href,
            text: bundle?.markdown ?? `No category bundle found for '${category}'.`
          }
        ]
      };
    }
  );

  server.registerTool(
    'list_categories',
    {
      title: 'List categories',
      description: 'List the available company standards categories.'
    },
    async () => {
      const catalog = await loadCatalog();
      return {
        content: [
          {
            type: 'text',
            text: catalog.categories.map(category => `- ${category.id}: ${category.title} — ${category.description}`).join('\n')
          }
        ],
        structuredContent: {
          categories: catalog.categories
        }
      };
    }
  );

  server.registerTool(
    'search_standards',
    {
      title: 'Search standards',
      description: 'Search the segmented company standards by story, technology, or keyword.',
      inputSchema: z.object({
        query: z.string().min(2).describe('Search terms from Jira story, code area, or architecture context.'),
        category: z.string().optional().describe('Optional category filter.'),
        limit: z.number().int().min(1).max(10).default(5).describe('Maximum number of matches.')
      })
    },
    async ({ query, category, limit }) => {
      const matches = await searchDocuments({ query, category, limit });
      return {
        content: [
          {
            type: 'text',
            text: matches.length > 0
              ? matches.map(match => `# ${match.title}\nCategory: ${match.category}\nURI: ${match.uri}\n\n${match.excerpt}`).join('\n\n---\n\n')
              : 'No matching standards found.'
          }
        ],
        structuredContent: {
          matches
        }
      };
    }
  );

  server.registerTool(
    'get_standard',
    {
      title: 'Get standard',
      description: 'Return the full text of a standard document by id.',
      inputSchema: z.object({
        id: z.string().describe('Document id from the catalog.')
      })
    },
    async ({ id }) => {
      const document = await getDocumentById(id);
      return {
        content: [
          {
            type: 'text',
            text: document?.content ?? `No standard document found for '${id}'.`
          }
        ],
        structuredContent: {
          document: document
            ? {
                id: document.id,
                title: document.title,
                category: document.category,
                path: document.path,
                uri: `standards://document/${document.id}`
              }
            : null
        }
      };
    }
  );

  server.registerTool(
    'build_instruction_bundle',
    {
      title: 'Build instruction bundle',
      description: 'Create a context bundle for implementing or reviewing a Jira story against company standards.',
      inputSchema: z.object({
        story: z.string().min(5).describe('Jira story summary, acceptance criteria, or implementation notes.'),
        techStack: z.string().optional().describe('Optional stack hint such as .NET API, Kafka, SQS, PostgreSQL.'),
        categories: z.array(z.string()).optional().describe('Optional standard categories to force include.'),
        maxDocuments: z.number().int().min(1).max(10).default(5).describe('Maximum number of standards to include.')
      })
    },
    async ({ story, techStack, categories = [], maxDocuments }) => {
      const bundle = await buildInstructionBundle({ story, techStack, categories, maxDocuments });
      return {
        content: [
          {
            type: 'text',
            text: bundle.text
          }
        ],
        structuredContent: bundle
      };
    }
  );

  server.registerPrompt(
    'plan-jira-story-with-standards',
    {
      title: 'Plan Jira story with company standards',
      description: 'Generate an implementation plan that respects the company standards catalog.',
      argsSchema: z.object({
        story: z.string().describe('Jira story summary and acceptance criteria.'),
        techStack: z.string().optional().describe('Optional tech stack hint.'),
        categories: z.array(z.string()).optional().describe('Optional categories to force include.')
      })
    },
    async ({ story, techStack, categories = [] }) => {
      const bundle = await buildInstructionBundle({ story, techStack, categories, maxDocuments: 5 });
      return {
        messages: [
          {
            role: 'user',
            content: {
              type: 'text',
              text: [
                'Create an implementation plan for the Jira story using the company standards below.',
                '',
                bundle.text,
                '',
                'Required output:',
                '1. impacted files or components',
                '2. standards that apply',
                '3. implementation steps',
                '4. tests and validation',
                '5. PR checklist before opening review'
              ].join('\n')
            }
          }
        ]
      };
    }
  );

  server.registerPrompt(
    'review-pr-with-standards',
    {
      title: 'Review PR with company standards',
      description: 'Review a planned or drafted change against company standards before raising a PR.',
      argsSchema: z.object({
        changeSummary: z.string().describe('Summary of the proposed change or PR.'),
        affectedAreas: z.string().optional().describe('Optional affected subsystems or technologies.')
      })
    },
    async ({ changeSummary, affectedAreas }) => {
      const bundle = await buildInstructionBundle({
        story: changeSummary,
        techStack: affectedAreas,
        categories: [],
        maxDocuments: 5
      });

      return {
        messages: [
          {
            role: 'user',
            content: {
              type: 'text',
              text: [
                'Review this change against company standards before opening or updating a PR.',
                '',
                bundle.text,
                '',
                'Required output:',
                '1. violated standards',
                '2. missing registration or wiring',
                '3. missing validation or tests',
                '4. risk items',
                '5. single consolidated review summary'
              ].join('\n')
            }
          }
        ]
      };
    }
  );

  return server;
}

export async function main() {
  const server = createCompanyStandardsServer();
  const transport = new StdioServerTransport();
  await server.connect(transport);
  console.error('Company Standards MCP server running on stdio');
}

if (process.argv[1] && new URL(`file://${process.argv[1].replace(/\\/g, '/')}`).href === import.meta.url) {
  main().catch(error => {
    console.error('Fatal error in Company Standards MCP server:', error);
    process.exit(1);
  });
}
