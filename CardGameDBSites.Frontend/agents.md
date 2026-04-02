# Agents Documentation

## Project Overview

- **Type**: Nuxt 3 Vue.js Frontend Application
- **Purpose**: Card game database website with deck building, collection management, and account features
- **Tech Stack**: Vue 3, Nuxt 3, Pinia (state management), TailwindCSS, TypeScript

## Commands

| Command | Description |
|---------|-------------|
| `npm run dev` | Start development server |
| `npm run build` | Build for production (includes type checking) |
| `npx nuxi typecheck` | Run TypeScript type check without building |
| `npm run generate` | Static site generation |
| `npm run preview` | Preview production build |
| `npm run generateUmbracoApi` | Generate API client from Umbraco delivery API |
| `npm run generateDefaultApi` | Generate API client from Umbraco default API |

## Key Directories

- `components/pageTypes/` - Page component definitions
- `components/popups/` - Popup dialog components
- `components/shared/` - Reusable UI components
- `services/` - Business logic services (DeckService, CardService, SetService, etc.)
- `stores/` - Pinia state stores (CardStore, CollectionStore, AccountStore, MemberStore)
- `server/api/` - Server-side API routes
- `api/umbraco/` - Generated Umbraco API client

## Code Style

- TypeScript is required
- Vue 3 Composition API with `<script setup>`
- TailwindCSS for styling
- ESLint with Vue plugin enabled

## Development Server

- Runs on `https://aidalon.local:3000`
- Uses custom HTTPS certificates (AidalonDB+2.pem, AidalonDB+2-key.pem)
- Port: 3000, Host: aidalon.local

## Environment Variables

- `NUXT_PUBLIC_API_BASE_URL` - Base URL for API calls (configured in nuxt.config.ts)

## Monorepo Structure

The frontend lives in a larger .NET + Nuxt monorepo at the repo root:

| Directory | Purpose |
|-----------|---------|
| `CardGameDBSites.API/` | ASP.NET Core (Umbraco) backend API |
| `CardGameDBSites.Frontend/` | Nuxt 3 frontend (this project) |
| `SkytearHorde.Business/` | Business logic layer (.NET) |
| `SkytearHorde.Entities/` | Domain models & Umbraco generated models (.NET) |
| `SkytearHorde.Website/` | Legacy Umbraco-rendered website (has `.cshtml` views) |
| `Vue/SkytearHordeDB/` | Older Vue SPA (Vite-based, no Nuxt) |

The backend exposes an OpenAPI spec; frontend API clients in `api/default/` and `api/umbraco/` are auto-generated from it.

## Routing & Page Types

Routing is CMS-driven via Umbraco. A catch-all route in `pages/[...slug].vue` maps Umbraco content types to Vue components. Key page type mappings:

| Content Type | Component |
|-------------|-----------|
| `card` | `CardDetailPage.vue` |
| `cardOverview` | `CardOverviewPage.vue` |
| `deckDetail` | `DeckDetail.vue` |

Site settings (including `cardSections`, navigation, colors) are loaded from `/api/settings/site` via `SiteService.ts` and cached statically.

## API Communication

There are two ways the frontend talks to the backend:

1. **Direct calls** via `DoFetch()` in `helpers/RequestsHelper.ts` — calls `${API_BASE_URL}/api/...` directly. Used by services/composables.
2. **Server proxy** via `/api/proxy/[...slug].ts` — forwards requests from the Nuxt server to the backend, attaching JWT from cookies. Used by `DeckAction.vue` exports and `DoServerFetch()`.

The proxy always attaches `Authorization: Bearer ${jwt}` if the `cardgamesdb` cookie exists.

## Card Detail & Tags

- `CardDetailPage.vue` renders card attributes using `siteSettings.cardSections` from the CMS.
- Each section is rendered by `CardDetailAbility.vue`, which handles two modes:
  - **Plain text**: comma-joined values via `SpecialTextFormatter`
  - **Tags** (`showAsTags: true`): styled pills, optionally linking to a card overview page with a filter query param (`?{ability}={value}`)
- `CardSectionApiModel` fields: `ability` (filter alias), `showAsTags`, `overviewPageUrl`, `namePosition`, `isDivider`.

## Card Overview & Filtering

- `CardOverviewPage.vue` builds `OverviewFilterModel[]` from CMS config and passes to `CardOverview.vue`.
- `Overview.vue` manages filter state, search, and pagination. Filters are synced to URL query params.
- Filter alias = query param key, filter value = query param value (e.g. `?Type=Warrior`).
- `BaseCardOverview.vue` converts selected filters into `CardsQueryFilterClauseApiModel[]` for the API query.
- Cards are loaded via `useCards().queryCards()` which calls the backend `/api/cards/query` endpoint.

## Deck Building — Resource Requirements

The deck builder enforces resource constraints via `ResourceRequirement` (frontend: `services/requirements/ResourceRequirement.ts`, backend: `SkytearHorde.Entities/Requirements/ResourceRequirement.cs`). Resources are values on cards (e.g. "Energy", "Mana") where main cards (leaders) provide resources and other cards require them.

Three modes are available via the `ResourceMode` enum (`services/requirements/ResourceMode.ts`):

| Mode | Enum Value | Rule | Example (pool: `{A, B}`) |
|------|-----------|------|--------------------------|
| Single Resource | `ContainsAny` | Card needs at least one resource from the pool | `{A}`, `{A,C}` pass; `{C}` fails |
| Budget | `Budget` | Card's new resources (not in pool) must fit within remaining budget | Pool has 2 of 6 budget → 4 remaining. `{A,C,D}` passes (2 new); `{A,C,D,E,F}` fails (5 new) |
| Subset | `Subset` | All of card's resources must exist in the pool with sufficient quantity | `{A}`, `{A,B}` pass; `{A,C}` fails; `{A,A}` fails if pool only has 1×A |

**Backward compatibility**: The old `singleResourceMode` boolean (CMS property) maps to `Subset` (true) or `Budget` (false). The new `resourceMode` string property takes precedence when present.

Config keys (from `ResourceSquadRequirementConfig.GetConfig()`):
- `mainAbility` — attribute name on main cards (e.g. "Provides")
- `ability` — attribute name on other cards (e.g. "Requires")
- `mainCardsCondition` — conditions to identify main cards
- `resourceMode` — the `ResourceMode` enum value
- `singleResourceMode` — legacy boolean (still emitted for backward compat)
- `possibleValues` — all known resource values (used for `ToFilters`)
- `totalBudget` — total resource budget for `Budget` mode (default: 6)

## Deck Building — Restriction Types

Requirements have a `RestrictionType` (`CardGameDBSites.API/Models/Requirements/RestrictionType.cs`) that controls how they behave in the deck builder:

| Type | Validation | Filtering | Description |
|------|-----------|-----------|-------------|
| `Hard` | Enforced | Applied when "Show cards that fit" is ON | Strict restriction — blocks card addition and pre-filters the card list |
| `Passive` | Enforced when "Show cards" is ON | Applied when "Show cards that fit" is ON | Soft restriction — can be toggled off via "Only show cards that fit in the deck" checkbox |
| `Filter` | Never enforced | Always applied | Pure filtering — pre-filters the card list but never blocks adding cards |

The `Filter` type is designed for cases like SW-Unlimited aspects, where certain card combinations are penalized but not forbidden. The requirement's `ToFilters` output is always included in the card query regardless of the "Show cards" toggle, but `IsValid` is never called during validation.

**Flow in `RequirementService.ts`**:
- `GetInvalidRequirements()`: Skips `Filter` type entirely (always valid). Skips `Passive` when `ignorePassive=true`.
- `GetFilters()`: Always includes `Filter` type. Skips `Passive` when `ignorePassive=true`.

CMS configuration: The restriction type is set per requirement in Umbraco via the "Restriction Type" dropdown (`uSync/v9/DataTypes/DropdownRestrictionType.config`). Valid values: `Hard`, `Passive`, `Filter`.

## Site Differentiation

The frontend has **no hardcoded site-specific logic**. All differentiation (colors, card sections, filters, navigation, keyword images) comes from the backend via `/api/settings/site`. The `wrangler.jsonc` config sets the API base URL for Cloudflare Pages deployment.

## Key Data Models

- `CardDetailApiModel`: `baseId`, `displayName`, `imageUrl` (ImageCropsApiModel), `backImageUrl`, `attributes` (Record<string, string[]>), `price`, `variants`
- `CardSectionApiModel`: `ability`, `showAsTags`, `overviewPageUrl`, `namePosition`, `isDivider`
- `SiteSettingsApiModel`: `mainColor`, `hoverMainColor`, `borderColor`, `siteName`, `showLogin`, `showPrices`, `navigation`, `cardSections`, `keywordImages`, `footerText`, `footerLinks`

## Legacy Reference

The `SkytearHorde.Website/` directory contains the old server-rendered Umbraco views (`.cshtml`). Key reference files:
- `Views/CardDetail.cshtml` — old card detail page
- `Views/Partials/cardAbilities/multiTextAbilityValue.cshtml` — old tag rendering (shows correct filter URL pattern: `@Model.OverviewPageUrl?@value.Ability.Name=@item`)
- `Views/DeckDetail.cshtml` — old deck detail page
