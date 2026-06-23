# Angular Coding Standards - Angular 17+ (Standalone and Stateless First)

## Purpose
This document defines mandatory coding standards for Angular 17+ applications.

It is designed for:
- Developers implementing new features
- Reviewers validating pull requests
- AI agents generating or modifying code

This standard is updated for modern Angular expectations:
- Standalone architecture by default
- Stateless components by default
- Signal-first reactivity
- Route-level lazy loading

If a project has a strict legacy constraint, keep a separate project addendum that explicitly documents exceptions.

---

## 1. Core Principles

1. Components are stateless view renderers by default.
2. Business logic lives in domain services, feature stores, or facades.
3. Standalone APIs are default. NgModules are legacy compatibility only.
4. State is immutable and explicit.
5. Side effects are isolated from template and component presentation logic.
6. Feature boundaries are route-driven.
7. Type safety is mandatory across UI, domain, and API contracts.

---

## 2. Angular 17+ Baseline

### MUST
- Bootstrap with bootstrapApplication.
- Configure providers in app config using provideRouter, provideHttpClient, and other platform providers.
- Use standalone components, directives, and pipes by default.
- Use modern control flow syntax where practical: @if, @for, @switch.
- Use defer blocks for heavy or secondary UI rendering when beneficial.

### MUST NOT
- Create new NgModule-based feature architecture unless a documented legacy constraint requires it.
- Put business decision trees in templates.
- Depend on mutable shared objects for cross-feature state.

---

## 3. Architecture Guardrails

### Presentation Layer
- Smart container components coordinate state and orchestration.
- Dumb/presentational components receive typed input and emit typed output.
- Presentational components remain stateless and side-effect free.

### Orchestration Layer
- Use feature stores, facades, or use-case services to mediate UI and domain.
- Keep orchestration logic outside templates.

### Data Layer
- Services and repositories own network and persistence concerns.
- API contracts are typed and centralized.

---

## 4. Routing and Feature Composition

### MUST
- Use route configuration as the feature composition backbone.
- Lazy load features with loadChildren for route arrays or loadComponent for single-entry features.
- Keep route definitions centralized by feature domain.
- Use route guards and resolvers only for access and data prerequisites.

### MUST NOT
- Scatter route strings throughout random files.
- Perform complex route intent logic directly in template click handlers.

---

## 5. State Management Standards

Angular 17+ does not remove state management. It removes mandatory NgModule ceremony. State still exists and must be managed intentionally.

### Recommended patterns
1. Signals for local and feature-level reactive state.
2. NgRx Signal Store or NgRx Store for complex workflows and large teams.
3. ComponentStore for scoped state where appropriate.

### MUST
- Keep component state minimal and view-focused.
- Keep domain and shared state in stores/services, not ad hoc component fields.
- Use immutable updates.
- Keep selectors/computed state pure.

### MUST NOT
- Mutate arrays/objects in place when they represent shared state.
- Mix transport state, domain state, and ephemeral UI state without boundaries.

---

## 6. Stateless Component Contract

### MUST
- Prefer input/output plus computed signals for rendering.
- Use OnPush change detection for all feature components.
- Keep templates declarative and simple.
- Use track expressions with @for and trackBy patterns for large lists.

### MUST NOT
- Call HttpClient directly from components.
- Subscribe imperatively in components unless there is a clear lifecycle reason.
- Hide side effects inside getters or template helpers.

---

## 7. Signals and RxJS Usage Rules

### Signals
- Use signal for writable local state.
- Use computed for deterministic derivations.
- Use effect only for explicit side effects and cleanup-aware operations.

### RxJS
- Keep streams in services/stores/facades for async and external data workflows.
- Convert between observables and signals at boundaries only.
- Use takeUntilDestroyed or equivalent lifecycle-safe patterns when subscribing.

---

## 8. API and Service Layer Standards

### MUST
- Keep HTTP access encapsulated in dedicated API or data services.
- Use typed request and response contracts.
- Centralize endpoint paths and query key constants.
- Normalize error handling in a single strategy (interceptors plus typed domain errors).

### MUST NOT
- Duplicate endpoint literals across multiple files.
- Leak HTTP details into presentational components.

---

## 9. Generated Files and Contracts

### DTO and Client Generation
- Treat generated files as read-only.
- Regenerate from source contracts (OpenAPI or equivalent).
- Never hand-edit generated outputs.

### Localization
- Keep localization contract typed.
- Flow localization through state or injection boundary, not ad hoc globals.

---

## 10. Error Handling, Security, and Performance

### Error Handling
- Handle known request failures in service/store orchestration.
- Surface user-facing errors through a unified notification mechanism.

### Security
- Enforce token and auth handling through interceptors or central auth services.
- Validate and encode route/query inputs where relevant.

### Performance
- Use defer blocks for non-critical content.
- Avoid expensive template expressions.
- Apply memoized computed/selectors for derived data.
- Prefer SSR plus hydration for large apps when applicable.

---

## 11. Styling Standards

1. Keep design tokens centralized.
2. Avoid inline styles in templates.
3. Co-locate feature styles with components.
4. Keep responsive behavior first-class.
5. Maintain accessibility contrast and keyboard navigation support.

---

## 12. Testing Standards

### MUST
- Co-locate tests with implementation files.
- Unit test components, services, stores, guards, and route behavior.
- Validate stateless component contracts: input in, output out.
- Validate computed state derivations and async side effects.

### Minimum assertions
1. Rendering reflects input and signal state correctly.
2. User events emit expected typed outputs.
3. Store/service updates follow immutable patterns.
4. Error and loading states are rendered correctly.
5. Route-level lazy loading and guard behavior are correct.

---

## 13. Naming and Folder Conventions

### File naming
- Use kebab-case for files.

### Type naming
- Use PascalCase for classes, interfaces, and enums.

### Example feature structure (standalone-first)
- feature.routes.ts
- feature-shell.component.ts
- feature-shell.component.html
- feature-shell.component.less
- components/
- services/
- store/ or state/
- models/
- api/
- guards/
- resolvers/
- test files alongside source

---

## 14. Pull Request Review Checklist

Merge only when all items pass:

1. Standalone-first architecture is used.
2. Components are stateless by default and OnPush.
3. Business logic is outside presentational components.
4. State boundaries are explicit and immutable.
5. Routes are lazy-loaded appropriately.
6. API boundaries and typed contracts are respected.
7. Error/loading/access states are implemented.
8. No generated contract file was manually edited.
9. Tests cover rendering, events, state, and side effects.
10. Naming and folder standards are respected.

---

## 15. AI Code Generation Contract

When AI generates code under this standard, it must:

1. Default to standalone components and route-based composition.
2. Keep components stateless unless local transient UI state is required.
3. Place business logic in services/stores/facades.
4. Use typed models, payloads, and API contracts.
5. Keep side effects out of templates and presentational components.
6. Follow immutable state patterns.
7. Generate matching tests for new logic.

---

## 16. Definition of Done

A feature is done only when:

1. Angular 17+ standards in this file are satisfied.
2. Stateless component contract is respected.
3. State and side-effect boundaries are clear and tested.
4. Accessibility, error, and loading behavior are complete.
5. PR checklist is fully satisfied.

---

## 17. Optional Legacy Compatibility Addendum

Use this section only if your project still carries NgModule constraints.

Document explicitly:
- Why standalone cannot be used for that scope
- Which modules remain and retirement plan
- Which features are still module-scoped
- Migration steps and target dates

Without this addendum, the default expectation is Angular 17+ standalone-first implementation.