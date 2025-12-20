import js from '@eslint/js';
import tseslint from 'typescript-eslint';
import nxPlugin from '@nx/eslint-plugin';

export default [
	js.configs.recommended,
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
							sourceTag: 'layer:domain',
							onlyDependOnLibsWithTags: ['layer:standalone'],
						},
						{
							sourceTag: 'layer:application',
							onlyDependOnLibsWithTags: ['layer:domain', 'layer:standalone'],
						},
						{
							sourceTag: 'layer:infrastructure',
							onlyDependOnLibsWithTags: [
								'layer:application',
								'layer:domain',
								'layer:standalone',
							],
						},
						{
							sourceTag: 'layer:presentation',
							onlyDependOnLibsWithTags: [
								'layer:domain',
								'layer:application',
								'layer:infrastructure',
								'layer:standalone',
							],
						},
						{
							sourceTag: 'platform:backend',
							onlyDependOnLibsWithTags: ['platform:backend', 'platform:shared'],
						},
						{
							sourceTag: 'platform:frontend',
							onlyDependOnLibsWithTags: [
								'platform:frontend',
								'platform:shared',
							],
						},
						{
							sourceTag: 'platform:shared',
							onlyDependOnLibsWithTags: ['platform:shared'],
						},
						{
							sourceTag: 'scope:core',
							onlyDependOnLibsWithTags: ['scope:core'],
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
