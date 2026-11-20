# ADR 0008 — Web UI: Astryx design system

- **Status**: Accepted — setup details confirmed by a spike on one real page (pantry list)
- **Date**: 2026-11-20

## Context

The webapp (Next.js 16, React 19, currently Tailwind v4) uses hand-written UI components in `@utils/react`, which is slow to build and maintain. The UI will be redesigned from scratch, must work well on phones (PWA first, see the mobile strategy ADR), and the developer is not a UI specialist — the design system must carry most design decisions.

Options considered: **Astryx** (Meta, MIT), shadcn/ui, HeroUI, Mantine, daisyUI, Ark/Park UI.

## Decision

Use **[Astryx](https://astryx.atmeta.com/)** (`@astryxdesign/core`, MIT) as the design system, and use as much of it as possible: components, theme, tokens, typography, spacing, radius, elevation, motion and responsive adaptations.

Why Astryx:

- Installed package — no component code to write or maintain.
- 150+ accessible components, including those Cooquoi needs early: AppShell, MobileNav, BottomSheet, Table, date inputs, Typeahead, Toast, and **Chat** components for AI suggestions.
- Complete token system with light/dark mode, themes built from a few seeds, and a theme CLI.
- Agent-ready (CLI and agent docs).

Fallback if the spike finds a blocker: **shadcn/ui**.

## Rules

### 1. Components first

- Use an Astryx component whenever one exists. Write a custom component only by **composing** Astryx components.
- `@utils/react` shrinks to Cooquoi-specific composites (e.g. `StockItemCard`) built on Astryx; its hand-written primitives (button, input, card, modal…) are removed.
- In Server Components, import each component from its own entry: `import { Button } from '@astryxdesign/core/Button'`.

### 2. Tokens only — no hard-coded values

| Need | Use | Never |
|---|---|---|
| Spacing | Component props (`<Stack gap={4}>`, `padding={4}`), or `var(--spacing-*)` in CSS Modules | `16px`, `margin: 13px` |
| Text | `<Heading level={…} type=…>`, `<Text type="body \| large \| label \| supporting \| code">` | `font-size: 15px` |
| Colour | Semantic tokens (`--color-text-primary`, `--color-background-surface`, `--color-accent`, `--color-success`…) | Hex values in components |
| Radius | `--radius-inner \| element \| container \| page \| full` | `border-radius: 10px` |
| Elevation | `elevation="low \| med \| high"`, `--shadow-*` | Custom `box-shadow` |
| Control size | `--size-element-sm \| md \| lg` | Fixed heights |

Spacing scale is 4 px based (`--spacing-1` = 0.25rem … `--spacing-12` = 3rem). Hex values appear **only** in the Cooquoi theme definition.

### 3. Responsive

- Breakpoints come from the theme's `adaptations.widthBreakpoints` (defaults: `sm` 640, `md` 768, `lg` 1024, `xl` 1280, `2xl` 1536) — no ad-hoc media queries.
- Prefer **intrinsic layouts** over breakpoints: `<Grid columns={{ minWidth: …, max: … }}>` reflows by itself; avoid fixed `columns={n}` for content lists.
- Layout regions: 3 above `lg`, 2 from `md`, 1 below (Astryx layout guidance).
- App shell: `AppShell` with `mobileNav={{ breakpoint: 'md' }}`; `BottomSheet` instead of dialogs for mobile-first actions.
- Use `adaptations` rules for `pointer: 'coarse'` (touch) and `motion: 'reduce'` rather than custom detection.

### 4. Theme

- A single **`cooquoi` theme** created with `defineTheme({ name: 'cooquoi', extends: neutralTheme, color: { accent, neutralStyle, contrast }, typography, radius })`.
- Built with `npx @astryxdesign/cli theme build` and committed — **built themes are required for SSR** (runtime themes flash on hydration).
- Colour scheme: see [Colour scheme](#colour-scheme) — chosen through the Astryx theme editor, then frozen in the theme file.
- Status colours (info, success, warning, error) keep Astryx defaults unless they clash with the accent.
- Light and dark modes both supported (`<Theme mode="system">`); dark first paint via `<html data-theme>`.
- Fonts are loaded by the app (`next/font`), Astryx only references them.

### 5. Styling custom code — no Tailwind

Astryx is authored in StyleX but ships **prebuilt CSS**: consumers need neither StyleX nor Tailwind. **Tailwind is removed** (together with the hand-written components, during the redesign — not before, as the current UI depends on it).

Custom styling, in order of preference:

1. **Astryx props** — `Layout`, `Section`, `Stack`/`HStack`/`VStack`, `Grid` with `gap`, `padding`, `columns`, `elevation`.
2. **CSS Modules** referencing Astryx tokens (`padding: var(--spacing-4); color: var(--color-text-secondary);`) — native in Next.js, no plugin.
3. **StyleX via `xstyle`** — only if a real need appears; requires adding the StyleX compiler (`@stylexswc/nextjs-plugin`) to the build.

Removed with Tailwind: `tailwindcss`, `@tailwindcss/postcss`, `@tailwindcss/vite`, `tailwind-merge`, `class-variance-authority`, and the PostCSS config. `clsx` may stay for conditional class names.

CSS setup in the root layout:

```css
@import '@astryxdesign/core/reset.css';
@import '@astryxdesign/core/astryx.css';
/* + the built cooquoi theme CSS */
```

### 6. Tooling cautions

- `npx @astryxdesign/cli init` writes `AGENTS.md` / `CLAUDE.md`: run it in a scratch folder and merge the useful parts manually into the project's own files.
- Avoid a Babel-based StyleX setup (it disables SWC and `next/font`); only needed if authoring StyleX source, then use `@stylexswc/nextjs-plugin`.

## Colour scheme

Defined by **one accent seed + a neutral style**; Astryx derives surfaces, text, borders and both light/dark values. Candidates are evaluated in the theme editor (`npx @astryxdesign/cli theme editor new cooquoi --like neutral`) for contrast in light and dark mode, and against status colours.

**Chosen (2026-11-21): warm neutral + paprika accent.**

```ts
defineTheme({
  name: 'cooquoi',
  extends: neutralTheme,
  color: { accent: '#C4562E', neutralStyle: 'warm', contrast: 'standard' },
});
```

- Paprika conveys home cooking and appetite; warm neutrals (creamy greys) support the "daily comfort" feel.
- Watch-out: paprika is close to the error red. In the theme editor, check that accent buttons and error states (`--color-error`, `--color-text-error`) stay clearly distinct in light and dark mode; switch `contrast` to `'high'` or adjust the seed slightly if they don't.
- The exact seed may be fine-tuned in the theme editor during the spike; the final value is the one committed in the theme file.

## Consequences

- Little UI code to own; consistent spacing, typography and colour by construction.
- Redesigning the look means editing one theme file, not components.
- Design tokens (CSS variables) are the bridge to a future native app.
- **Astryx is in beta** (public v0 announced 2026-11-14): expect breaking changes; pin versions and use `npx @astryxdesign/cli upgrade` deliberately.
- Fewer community answers than mainstream libraries; rely on Astryx's own docs and agent docs.
