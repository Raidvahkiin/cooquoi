---
name: webapp-ui
description: Build or change UI in the Cooquoi webapp with the Astryx design system — component choice, layout, tokens instead of pixels, responsive behaviour, theming, CSS Modules. Use whenever creating or modifying a screen, component or style in the webapp.
---

# Webapp UI with Astryx

Decision: [ADR 0008](../../../docs/architecture/adr/0008-web-ui-astryx.md). Astryx is new: **check its docs before assuming an API** — https://astryx.atmeta.com/docs/getting-started and the component pages under https://astryx.atmeta.com/components. When unsure of a prop, read the component page; never invent props.

## Rules

1. **Astryx component first.** Write a custom component only by composing Astryx components. Common picks:

   | Need | Astryx |
   |---|---|
   | App frame, navigation | `AppShell`, `SideNav`, `TopNav`, `MobileNav` (`mobileNav={{ breakpoint: 'md' }}`) |
   | Page layout | `Layout`, `Section`, `Stack` / `HStack` / `VStack`, `Grid` |
   | Lists and data | `List`, `Table`, `Card`, `EmptyState`, `Skeleton`, `Pagination` |
   | Forms | `Field`, `TextInput`, `NumberInput`, `DateInput`, `Selector`, `Typeahead`, `Switch`, `Button` |
   | Feedback | `Toast`, `Banner`, `Badge`, `StatusDot`, `ProgressBar`, `Spinner` |
   | Overlays | `Dialog`, `AlertDialog`, `BottomSheet` (mobile-first actions), `Popover`, `Tooltip` |
   | AI suggestions | `Chat*` components |
   | Text | `Heading level={…}`, `Text type="body | large | label | supporting | code"` |

2. **Tokens only — never hard-coded values.**

   | Need | Use | Never |
   |---|---|---|
   | Spacing | `gap={4}`, `padding={4}`, `var(--spacing-*)` | `16px`, `1rem` literals |
   | Colour | `--color-text-*`, `--color-background-*`, `--color-accent`, status tokens | hex, rgb, named colours |
   | Radius | `--radius-inner | element | container | page | full` | `border-radius: 10px` |
   | Elevation | `elevation="low | med | high"`, `--shadow-*` | custom `box-shadow` |
   | Control size | `--size-element-sm | md | lg` | fixed heights |
   | Typography | `Heading`, `Text` types | `font-size`, `font-weight` |

3. **Responsive by construction.**
   - Prefer intrinsic layouts: `<Grid columns={{ minWidth: …, max: … }}>`; avoid fixed `columns={n}` for content lists.
   - Breakpoints come from the theme (`sm` 640, `md` 768, `lg` 1024, `xl` 1280, `2xl` 1536). No ad-hoc media queries.
   - Regions: 3 above `lg`, 2 from `md`, 1 below.
   - Touch: rely on the theme's `pointer: 'coarse'` adaptation, not custom detection.

4. **Styling custom code.** Order: Astryx props → CSS Modules using Astryx tokens → StyleX only with an approved need. No Tailwind in new code.

5. **Server Components.** Import each component from its own entry: `import { Button } from '@astryxdesign/core/Button'`. Add `'use client'` only on the interactive leaf that needs it.

6. **Theme.** Use the `cooquoi` theme (warm neutral, paprika accent). Never override tokens locally to "fix" a colour — change the theme, or use the right semantic token.

7. **Text.** All strings through next-intl. Leave room for longer French and Vietnamese labels (no fixed widths on text).

8. **Accessibility.** Keep Astryx semantics: labels on every field (`Field`), accessible names on icon buttons, headings in order, no click handlers on non-interactive elements.

## Checklist

- [ ] Only Astryx components or compositions of them
- [ ] No px, rem literals, hex, Tailwind classes or custom shadows
- [ ] Layout reflows from mobile to desktop without horizontal scroll
- [ ] Works in light and dark mode (semantic tokens only)
- [ ] No literal strings; long translations do not break layout
- [ ] `'use client'` only where needed
