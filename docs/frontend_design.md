# Frontend Design

## Overview

The frontend is a Single Page Application (SPA) built with React and Vite. It emphasizes security, performance, and a premium user experience.

## Technology Stack

- **Framework**: React 18+ with TypeScript
- **Build Tool**: Vite
- **Styling**: Tailwind CSS
- **State Management**: React Context + TanStack Query (React Query)
- **Routing**: React Router v6
- **Internationalization**: react-i18next
- **Testing**: Vitest + jsdom (for service/algorithm tests only)

## Project Structure

```
src/
├── assets/              # Static assets (images, fonts)
├── components/          # Reusable UI components
│   └── shared/          # Shared components used across pages
│       └── ComponentName/
│           ├── ComponentName.tsx
│           └── index.ts
├── pages/               # Page components (flat structure, no grouping)
│   ├── LoginPage/
│   │   ├── LoginPage.tsx
│   │   └── index.ts
│   ├── RegisterPage/
│   │   ├── RegisterPage.tsx
│   │   └── index.ts
│   └── DashboardPage/
│       ├── DashboardPage.tsx
│       └── index.ts
├── services/            # Business logic and API clients
│   ├── api/             # Per-endpoint API modules
│   │   ├── register/
│   │   │   ├── models.ts    # Request & Response types
│   │   │   ├── api.ts       # export async function invoke()
│   │   │   └── index.ts     # export * as registerApi from './api'
│   │   └── login/
│   │       ├── models.ts
│   │       ├── api.ts
│   │       └── index.ts
│   └── passwordService/ # Business logic services (password hashing, validation)
│       ├── index.ts
│       └── index.test.ts   # Colocated algorithm tests
├── i18n/                # Internationalization
│   ├── config.ts
│   └── locales/
│       ├── en-US.json
│       └── fr-FR.json
└── App.tsx
```

## Architecture Principles

### 1. Modular Component Structure

- Each component lives in its own folder with an `index.ts` for clean imports
- No barrel files at the `shared/` level - import directly from component modules
- Example: `import { LanguageSelector } from '../../components/shared/LanguageSelector'`

### 2. Flat Page Structure

- All pages are at the root of `pages/` directory (no grouping folders)
- Each page is a module with its own folder containing `PageName.tsx` and `index.ts`
- Import pages directly: `import LoginPage from './pages/LoginPage'`

### 3. Per-Endpoint API Modules

- Each API endpoint has its own folder under `services/api/`
- Structure per endpoint:
  - `models.ts`: Request, Response, and ApiError types (one pair per endpoint)
  - `api.ts`: Single `invoke(request): Promise<Response>` function
  - `index.ts`: Re-exports as namespace: `export * as endpointApi from './api'`
- Usage: `await registerApi.invoke({ username, passwordHash })`

### 4. Business Logic Services

- Services folder contains reusable business logic (not just API calls)
- Example: `passwordService` for password hashing and validation
- Algorithm tests are colocated: `serviceName/index.test.ts`
- Only algorithm/service tests - no UI component tests

### 5. No Legacy Code

- No "legacy", "deprecated", or "compatibility" shims
- All imports point directly to their source modules
- Clean, intentional architecture from the start

## Key Design Patterns

### Component Module Pattern

Each component is self-contained:

```typescript
// components/shared/Button/Button.tsx
export function Button({ children, ...props }) { ... }

// components/shared/Button/index.ts
export { Button } from './Button';

// Usage in pages
import { Button } from '../../components/shared/Button';
```

### API Endpoint Pattern

Each endpoint is isolated with typed models:

```typescript
// services/api/register/models.ts
export type Request = { username: string; passwordHash: string };
export type Response = { success: boolean; user?: User };
export type ApiError = { code: string; message?: string };

// services/api/register/api.ts
import type { Request, Response, ApiError } from './models';
export async function invoke(req: Request): Promise<Response> { ... }

// services/api/register/index.ts
export * as registerApi from './api';
export * from './models';

// Usage in pages
import { registerApi } from '../../services/api/register';
const response = await registerApi.invoke({ username, passwordHash });
```

### Service Pattern

Business logic services with colocated tests:

```typescript
// services/passwordService/index.ts
export async function hash(input: string): Promise<string> { ... }
export function validateStrength(password: string): StrengthResult { ... }

// services/passwordService/index.test.ts
describe('passwordService', () => {
  it('hash is deterministic', async () => { ... });
});
```

## Testing Strategy

- **Unit tests**: Only for algorithms and business logic in `services/`
- **Test location**: Colocated with implementation as `*.test.ts`
- **No UI tests**: Focus on pure functions and service logic
- **Test example**: Password hashing determinism, validation rules

## Internationalization (i18n)

- **Library**: react-i18next
- **Supported languages**: English (en-US), French (fr-FR)
- **Structure**:
  - Configuration: `i18n/config.ts`
  - Translations: `i18n/locales/{language}.json`
- **Features**:
  - URL query override: `?lang=fr-FR`
  - localStorage persistence: `afina_lang`
  - Error code mapping: Backend returns codes, frontend translates

## Security Practices

### Client-Side Password Hashing

- Passwords are hashed with SHA-256 before transmission
- Server receives only the hash, never plaintext
- Server applies BCrypt to the received hash for storage
- This provides defense-in-depth against network interception

## UI/UX Guidelines

- **Theme**: Dark mode by default, high contrast for readability.
- **Responsiveness**: Mobile-first design using Tailwind breakpoints.
- **Feedback**: Loading skeletons, toast notifications for actions.
