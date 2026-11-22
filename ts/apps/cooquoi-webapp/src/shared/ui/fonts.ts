import { Be_Vietnam_Pro } from 'next/font/google';

// Figtree (the Astryx default) has no Vietnamese subset; the `cooquoi` theme reads this variable.
export const bodyFont = Be_Vietnam_Pro({
  subsets: ['latin', 'latin-ext', 'vietnamese'],
  weight: ['400', '500', '600', '700'],
  display: 'swap',
  variable: '--font-be-vietnam-pro',
});
