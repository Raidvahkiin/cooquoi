import { themePalette } from '@astryxdesign/core/palette';
import { defineTheme } from '@astryxdesign/core/theme';

// Source of the `cooquoi` theme: warm neutrals, paprika accent.
// Built with `pnpm nx run @cooquoi/webapp:theme-build` into cooquoi.{css,js,d.ts} (committed).
// Hex is allowed here only: these are the theme seeds.
export const cooquoiTheme = defineTheme({
  name: 'cooquoi',
  palette: themePalette({
    accent: '#B9531F',
    neutral: '#7A6F66',
    // Crimson, not the default red: keeps errors apart from the paprika accent.
    danger: '#C2185B',
    recipe: 'astryx-oklch-v2',
  }),
  typography: {
    // next/font sets --font-be-vietnam-pro (Figtree has no Vietnamese subset).
    body: {
      family: 'var(--font-be-vietnam-pro)',
      fallbacks: '-apple-system, system-ui, sans-serif',
    },
  },
});
