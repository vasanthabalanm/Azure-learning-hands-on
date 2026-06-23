# Angular Coding Standards - Matching Architecture

## Purpose
This document defines mandatory coding standards for Angular applications that follow the architecture style.

Use this file as the implementation contract for:
- Developers creating new features
- Reviewers validating pull requests
- AI agents generating or modifying code

If any rule in this document conflicts with project-specific constraints, the project architecture document is the source of truth.

---

## 1. Non-Negotiable Principles

1. Components are view-only renderers.
2. Business logic must not live in components.
3. State is managed through NgRx with immutable updates.
4. Side effects are handled in effects only.
5. Components interact with state through facades.
6. UI binds to a single typed model object.
7. All feature code is organized by module and lazy-loaded where applicable.

---

## 2. Architecture Guardrails

### MUST
- Use NgModule architecture (not standalone component architecture).
- Use feature modules with lazy loading.
- Use a shared module for common UI, directives, pipes, and Angular Material re-exports.
- Keep a single app state contract with one slice per feature.
- Enforce component-level facade providers.

### MUST NOT
- Call APIs directly from components.
- Inject HttpClient directly into components or effects.
- Put business workflows in templates.
- Mutate NgRx state directly.
- Add raw route strings when route enums/constants exist.

---

## 3. Module Standards

### Root Module
- Configure top-level routes.
- Register global interceptors.
- Initialize root NgRx store and root effects.
- Import extension files once at app startup.

### Authorized Shell Module
- Host authenticated layout and protected route tree.
- Register shared core reducer/effects used across protected features.

### Feature Module
- Register feature reducer using StoreModule.forFeature.
- Register feature effects using EffectsModule.forFeature.
- Import SharedModule.
- Declare feature component(s) and internal wiring only.

---

## 4. Routing Standards

### MUST
- Define route fragments with enums/constants.
- Keep authenticated route mapping centralized.
- Use guards for authentication and feature access control.
- Use resolvers for required pre-load data on public or guarded routes.

### MUST NOT
- Hardcode route segment strings across random files.
- Trigger complex navigation decisions directly in components.

---

## 5. NgRx Standards

### State
- Define typed state interface per feature slice.
- Define default state values in a dedicated default class/object.

### Actions
- Use createAction with typed props.
- Follow action name convention: [Feature] Event Name.
- Use explicit success and reject/failure actions.

### Reducers
- Keep reducers pure and synchronous.
- Always use immutable updates with object spread.
- Never call services, APIs, or helper methods with side effects.

### Selectors
- Build memoized selectors with createSelector.
- Construct feature model objects in selectors, not in components.
- Pull shared data (for example localization, current user) from core state selectors.

### Effects
- Handle async operations, navigation, dialogs, and backend communication.
- Call services, then map responses to success/failure actions.
- Use catchError in pipelines for expected request failures.

---

## 6. Facade Standards

### MUST
- Facade extends a shared facade base where possible.
- Facade exposes Model stream and current model access.
- Public facade methods dispatch actions and return void.
- Facade is provided at component level.

### MUST NOT
- Keep private reactive graphs in facade that bypass store selectors.
- Return mutable state objects to components.

---

## 7. Model-Based UI Standards

### MUST
- Each feature has a typed model class.
- Model has a static Create factory that maps state to view data.
- Templates consume a single model instance from async pipe.
- Derivations and formatting happen in model creation using pure helpers.

### MUST NOT
- Compute business-level derived data in component templates.
- Store view model objects in NgRx state.

---

## 8. Component Standards

### MUST
- Use ChangeDetectionStrategy.OnPush.
- Extend shared base component classes when available.
- Keep event handlers as pass-through calls to facade.
- Use trackBy with list rendering.
- Keep files in kebab-case.

### MUST NOT
- Subscribe directly to store inside components.
- Place domain decisions in click handlers.

---

## 9. API and Service Layer Standards

### API Layer
- Encapsulate HTTP primitives in base API classes.
- Keep typed API methods per domain.
- Keep request URLs centralized in constants/enums.

### Service Layer
- Services wrap API classes and include caching/composition logic.
- Effects depend on services, not API classes.

### MUST NOT
- Duplicate endpoint strings inline across code.
- Spread transport-level concerns into effects/components.

---

## 10. Generated Artifact Rules

### DTO Contracts
- Treat generated DTO files as read-only.
- Regenerate from backend OpenAPI/Swagger source.

### Localization Contracts
- Treat generated localization interfaces as read-only.
- Load localization into core state and flow through selectors to models.

---

## 11. Shared Library Standards

### Components
- Shared components are input/output driven and store-agnostic.
- Inputs must be typed and explicit.

### Pipes and Formatters
- Use pipes for template-only transformations.
- Use formatter/helper classes for model/effect transformations.

### Directives
- Keep directives generic, reusable, and independent of feature state.

### Helpers
- Helpers must be pure and stateless.
- No DI, no subscriptions, no hidden side effects.

### Extensions
- Global prototype extensions are imported once in root startup modules.
- Avoid adding new extensions unless clearly justified and reviewed.

---

## 12. Interceptor, Guard, and Resolver Standards

### Interceptors
- Implement token attachment, request metadata, loading activity, and HTTP error translation.
- Keep order deterministic and documented.

### Guards
- Use guards for authentication, authorization, and route validity checks.

### Resolvers
- Use resolvers where data must exist before route activation.

---

## 13. Dependency Injection and Config Standards

### MUST
- Use root scope only for true global singletons.
- Provide feature-specific services at feature module scope.
- Provide facades at component scope.
- Read runtime config from centralized config namespace/object.

### MUST NOT
- Overuse root-scoped providers for feature logic.
- Duplicate runtime config resolution logic across features.

---

## 14. Styling Standards

1. Use LESS for component styles.
2. Keep global style tokens and mixins centralized.
3. Avoid inline style attributes in templates.
4. Respect shared theme tokens and spacing scale.

---

## 15. Testing Standards

### Unit Testing
- Use Jest test framework.
- Co-locate spec files beside source files.
- Add tests for facades, models, reducers, selectors, and effects.

### Minimum Required Assertions
- Facade dispatch contract is correct.
- Model.Create maps state to view model correctly.
- Reducers update state immutably.
- Selectors derive expected values.
- Effects dispatch expected success/reject actions for API outcomes.

---

## 16. Naming and File Conventions

### File naming
- Use kebab-case for file names.

### Type naming
- Use PascalCase for classes, interfaces, and enums.

### Feature file set (expected)
- feature.component.ts
- feature.component.html
- feature.component.less
- feature.facade.ts
- feature.model.ts
- feature.module.ts
- actions/feature.actions.ts
- entities/feature.state.ts
- reducers/feature.reducer.ts
- selectors/feature.selectors.ts
- effects/feature.effect.ts
- services/feature.service.ts
- matching spec files

---

## 17. Pull Request Review Checklist

Approve only when all items pass:

1. Component is OnPush and facade-driven.
2. No business logic in component/template.
3. Feature has complete NgRx set: actions, reducer, selectors, effects, state.
4. Model factory is used for state-to-view mapping.
5. Effects call services and handle errors correctly.
6. Routes use constants/enums, not hardcoded strings.
7. API and service boundaries are respected.
8. Generated files were not edited manually.
9. Tests cover model/facade/reducer/effect behavior.
10. File naming and folder structure match conventions.

---

## 18. AI Code Generation Contract

When AI generates code for this project style, it must:

1. Produce NgModule-based feature code, not standalone-only patterns.
2. Generate facade + model + selector flow for all feature screens.
3. Keep component code minimal and delegation-based.
4. Place async and side effects in effects.
5. Use typed payloads and typed action props.
6. Follow kebab-case naming and co-located specs.
7. Avoid introducing architecture deviations unless explicitly requested.

---

## 19. Definition of Done

A feature is done only if:

1. Architecture alignment is validated against this standards file.
2. State, facade, model, and UI integration are complete.
3. Unit tests are added and passing.
4. No forbidden shortcuts are present.
5. Code review checklist is fully satisfied.

---

## 20. Optional Project Addendum Template

For each new project, add an addendum section with:

- Route enums file location
- Core state shape and mandatory slices
- Required interceptors and order
- API base classes and request contract
- Required generated files and regen commands
- Project-specific naming exceptions
- Performance and security constraints

This keeps the standards reusable while preserving project-specific precision.