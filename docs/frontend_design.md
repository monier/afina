# Frontend Design

## Overview
The frontend is a Single Page Application (SPA) built with React and Vite. It emphasizes security, performance, and a premium user experience.

## Technology Stack
*   **Framework**: React (Latest)
*   **Build Tool**: Vite
*   **Styling**: Tailwind CSS (Utility) + Styled-Components (Component encapsulation)
*   **State Management**: React Context + TanStack Query (React Query)
*   **Routing**: React Router
*   **Cryptography**: Web Crypto API (Native browser support)

## Project Structure
```
src/
├── assets/          # Static assets
├── components/      # Shared UI components (Button, Input, Modal)
├── features/        # Feature-based modules
│   ├── auth/        # Login, Register, Key Derivation
│   ├── vault/       # Vault list, Item details, Crypto logic
│   ├── tenant/      # Tenant management, Members
│   └── audit/       # Audit log viewer
├── hooks/           # Shared hooks (useAuth, useTheme)
├── layouts/         # Page layouts (AuthLayout, DashboardLayout)
├── pages/           # Route components
├── services/        # API clients and Crypto utilities
│   ├── api.ts
│   └── crypto.ts    # CRITICAL: All client-side encryption logic
├── styles/          # Global styles, theme definitions
└── App.tsx
```

## Key Components
*   **CryptoService**: Handles PBKDF2/Argon2 key derivation, RSA key generation, and AES-GCM encryption/decryption.
*   **VaultGrid**: Displays vault items with type-specific icons.
*   **SecureInput**: Input field that prevents clipboard copying for sensitive data (optional).

## UI/UX Guidelines
*   **Theme**: Dark mode by default, high contrast for readability.
*   **Responsiveness**: Mobile-first design using Tailwind breakpoints.
*   **Feedback**: Loading skeletons, toast notifications for actions.
