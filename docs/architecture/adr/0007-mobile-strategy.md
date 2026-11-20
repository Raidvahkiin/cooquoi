# ADR 0007 — Mobile strategy: PWA first, Flutter as the future native target

- **Status**: Accepted
- **Date**: 2026-11-21

## Context

Cooquoi starts as a webapp, with mobile use expected shortly after (checking the fridge in the kitchen, snapping a receipt in the shop). A native mobile app is a goal for the future, and **Flutter** is the chosen technology for it ([ADR 0001](0001-toolchain-folder-layout.md) reserves `mobile/`). The web UI uses Astryx ([ADR 0008](0008-web-ui-astryx.md)), which is web-only, so a native app has its own UI whatever the technology.

Options considered for mobile:

| Option | Shares with web | Extra cost |
|---|---|---|
| **PWA**: the Next.js app installable on the phone | Everything | Near zero |
| **Flutter**: native apps in Dart | API contract (generated Dart client), design tokens | High: a third language and a separate UI |
| **Expo (React Native)**: native apps in TypeScript/React | Logic, API client, types, design tokens | Medium: a second UI, still TypeScript |

## Decision

1. **Now: PWA**, built into the Next.js webapp.
   - Web app manifest via Next.js's built-in `app/manifest.ts` (name, icons, theme colour from the Cooquoi theme, `display: standalone`).
   - Service worker (e.g. Serwist) for app-shell caching and offline read of the pantry.
   - Camera through the browser (`<input type="file" accept="image/*" capture>` or `getUserMedia`) for receipt photos.
   - Web push for expiry reminders where supported (on iOS only once the app is installed to the home screen).
   - Mobile-first layouts with Astryx `AppShell`, `MobileNav` and `BottomSheet`.
2. **Later: a native Flutter app** in `mobile/`, when native capabilities matter: reliable background notifications, home-screen widgets, deeper camera/scanner integration, or app-store presence.
   - API access through a Dart client generated from the API contract (TypeSpec → OpenAPI).
   - Look and feel from the same design tokens as the web (Cooquoi theme exported to Flutter `ThemeData`).
   - Orchestrated by Nx like every other folder.
3. Expo was considered as a cheaper native path (TypeScript-only) but is not the chosen target.

## Consequences

- One codebase serves desktop and mobile for the MVP; Flutter work starts only when the PWA's limits are actually hit.
- iOS PWA limitations are accepted for now: push only when installed, no background sync, storage may be evicted if unused. The server remains the source of truth.
- **Keep Flutter possible from day one**:
  - all client-facing behaviour goes through the public API (no logic only reachable through Next.js server actions);
  - the API contract is the shared artifact, so TypeSpec contract-first becomes more valuable;
  - design tokens are kept in a form that can be exported to Dart.
- A native Flutter app means a third language and a second UI to maintain.
