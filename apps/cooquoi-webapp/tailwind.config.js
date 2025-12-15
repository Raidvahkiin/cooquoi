// const { createGlobPatternsForDependencies } = require('@nx/next/tailwind');

// The above utility import will not work if you are using Next.js' --turbo.
// Instead you will have to manually add the dependent paths to be included.
// For example
// ../libs/buttons/**/*.{ts,tsx,js,jsx,html}',                 <--- Adding a shared lib
// !../libs/buttons/**/*.{stories,spec}.{ts,tsx,js,jsx,html}', <--- Skip adding spec/stories files from shared lib

// If you are **not** using `--turbo` you can uncomment both lines 1 & 19.
// A discussion of the issue can be found: https://github.com/nrwl/nx/issues/26510

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './{src,pages,components,app}/**/*.{ts,tsx,js,jsx,html}',
    '!./{src,pages,components,app}/**/*.{stories,spec}.{ts,tsx,js,jsx,html}',
//     ...createGlobPatternsForDependencies(__dirname)
  ],
  theme: {
    extend: {
    fontFamily: {
      sans: [
        'var(--ui-font-sans)',
        'ui-sans-serif',
        'system-ui',
        '-apple-system',
        'BlinkMacSystemFont',
        'Segoe UI',
        'Helvetica Neue',
        'Arial',
        'Noto Sans',
        'sans-serif',
      ],
    },
    colors: {
      ui: {
        surface: 'rgb(var(--ui-surface) / <alpha-value>)',
        surfaceMuted: 'rgb(var(--ui-surface-muted) / <alpha-value>)',
        text: 'rgb(var(--ui-text) / <alpha-value>)',
        textMuted: 'rgb(var(--ui-text-muted) / <alpha-value>)',
        border: 'rgb(var(--ui-border) / <alpha-value>)',
        primary: 'rgb(var(--ui-primary) / <alpha-value>)',
        primaryHover: 'rgb(var(--ui-primary-hover) / <alpha-value>)',
        primaryRing: 'rgb(var(--ui-primary-ring) / <alpha-value>)',
        danger: 'rgb(var(--ui-danger) / <alpha-value>)',
        dangerHover: 'rgb(var(--ui-danger-hover) / <alpha-value>)',
        dangerRing: 'rgb(var(--ui-danger-ring) / <alpha-value>)',
      },
    },
    borderRadius: {
      ui: 'var(--ui-radius-md)',
      'ui-lg': 'var(--ui-radius-lg)',
    },
    spacing: {
      'ui-2': 'var(--ui-space-2)',
      'ui-3': 'var(--ui-space-3)',
      'ui-4': 'var(--ui-space-4)',
      'ui-6': 'var(--ui-space-6)',
    },
  },
  },
  plugins: [],
};
