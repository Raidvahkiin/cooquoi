import nxPlugin from '@nx/eslint-plugin';
import tseslint, { type ConfigArray } from 'typescript-eslint';

const config: ConfigArray = [
  ...tseslint.configs.recommended,
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
              sourceTag: 'scope:domain-layer',
              onlyDependOnLibsWithTags: ['scope:core', 'scope:utility'],
            },
            {
              sourceTag: 'scope:application-layer',
              onlyDependOnLibsWithTags: [
                'scope:core',
                'scope:utility',
                'scope:domain-layer',
              ],
            },
            {
              sourceTag: 'scope:infrastructure-layer',
              onlyDependOnLibsWithTags: [
                'scope:core',
                'scope:utility',
                'scope:domain-layer',
                'scope:application-layer',
              ],
            },

            {
              sourceTag: 'scope:presentation-layer',
              onlyDependOnLibsWithTags: [
                'scope:core',
                'scope:utility',
                'scope:domain-layer',
                'scope:application-layer',
                'scope:infrastructure-layer',
              ],
            },
            {
              sourceTag: 'scope:consumer',
              onlyDependOnLibsWithTags: [
                'scope:core',
                'scope:utility',
                'scope:presentation-layer',
              ],
            },
            {
              sourceTag: 'platform:node',
              onlyDependOnLibsWithTags: ['platform:node'],
            },
            {
              sourceTag: 'platform:nestjs',
              onlyDependOnLibsWithTags: ['platform:nestjs', 'platform:node'],
            },
            {
              sourceTag: 'platform:nextjs',
              onlyDependOnLibsWithTags: [
                'platform:nextjs',
                'platform:node',
                'platform:react',
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
