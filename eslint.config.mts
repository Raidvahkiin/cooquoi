import { dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import nxPlugin from '@nx/eslint-plugin';
import tseslint, { type ConfigArray } from 'typescript-eslint';

const __dirname = dirname(fileURLToPath(import.meta.url));

const config: ConfigArray = [
  ...tseslint.configs.recommended,
  {
    languageOptions: {
      parserOptions: {
        tsconfigRootDir: __dirname,
      },
    },
  },
  {
    plugins: {
      '@nx': nxPlugin,
    },
  },
  {
    files: ['**/*.ts', '**/*.tsx', '**/*.js', '**/*.jsx'],
    rules: {
      '@nx/enforce-module-boundaries': [
        'error',
        {
          depConstraints: [
            {
              sourceTag: 'type:lib',
              onlyDependOnLibsWithTags: ['type:lib'],
            },
            {
              sourceTag: 'type:app',
              onlyDependOnLibsWithTags: ['type:lib', 'type:app'],
            },
            {
              sourceTag: 'scope:core',
              onlyDependOnLibsWithTags: ['scope:core', 'type:app'],
            },
            {
              sourceTag: 'scope:utility',
              onlyDependOnLibsWithTags: ['scope:utility', 'scope:core'],
            },

            {
              sourceTag: 'scope:bounded-context',
              onlyDependOnLibsWithTags: ['scope:core', 'scope:utility'],
            },
            {
              sourceTag: 'scope:consumer',
              onlyDependOnLibsWithTags: [
                'scope:core',
                'scope:utility',
                'scope:bounded-context',
              ],
            },
            {
              sourceTag: 'platform:node',
              onlyDependOnLibsWithTags: ['platform:node'],
            },
            {
              sourceTag: 'platform:nestjs',
              onlyDependOnLibsWithTags: ['platform:nestjs', 'platform:node', 'platform:all'],
            },
            {
              sourceTag: 'platform:nextjs',
              onlyDependOnLibsWithTags: [
                'platform:nextjs',
                'platform:node',
                'platform:react',
                'platform:all',
              ],
            },
          ],
        },
      ],
    },
  },
  {
    files: ['**/*.spec.ts', '**/*.spec.tsx', '**/*.test.ts', '**/*.test.tsx'],
    rules: {
      '@typescript-eslint/no-explicit-any': 'off',
    },
  },
  {
    ignores: [
      '**/dist',
      '**/node_modules',
      '**/out-tsc',
      '**/test-output',
      '**/.next',
      '**/coverage',
      '**/next.config.js',
      '**/postcss.config.js',
      '**/tailwind.config.js',
    ],
  },
];

export default config;
