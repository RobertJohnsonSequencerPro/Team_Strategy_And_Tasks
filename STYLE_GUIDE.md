# Team Strategy & Tasks — Style Guide

**Version:** 0.2  
**Date:** 2026-03-22  
**Status:** Draft

This document governs the visual design language, UX patterns, and code conventions for the project. All contributors should treat it as the source of truth. Update this guide when patterns are intentionally changed — never let the code silently diverge from it.

---

## Table of Contents

1. [Design Principles](#1-design-principles)
2. [Color System](#2-color-system)
3. [Typography](#3-typography)
4. [Spacing & Layout](#4-spacing--layout)
5. [Iconography](#5-iconography)
6. [Component Patterns](#6-component-patterns)
7. [The Hierarchy: Visual Language](#7-the-hierarchy-visual-language)
8. [Status & Progress Conventions](#8-status--progress-conventions)
9. [Motion & Animation](#9-motion--animation)
10. [Accessibility](#10-accessibility)
11. [Code Conventions — General](#11-code-conventions--general)
12. [Code Conventions — C# / Blazor](#12-code-conventions--c--blazor)
13. [Code Conventions — ASP.NET Core Services](#13-code-conventions--aspnet-core-services)
14. [Code Conventions — Database (PostgreSQL / EF Core)](#14-code-conventions--database-postgresql--ef-core)
15. [Testing Conventions](#15-testing-conventions)
16. [File & Folder Structure](#16-file--folder-structure)

---

## 1. Design Principles

These principles are the "why" behind every specific rule in this guide. When two rules seem to conflict, refer back here.

| Principle | What it means in practice |
|---|---|
| **Hierarchy is the product** | The four-level tree is not a chrome element — it is the primary UI surface. Make it immediately legible at every zoom level. |
| **Clarity over cleverness** | Use plain language in labels, tooltips, and error messages. Avoid jargon specific to any one methodology (Agile, OKR, PDCA…). |
| **Information density with breathing room** | Business hierarchies grow large. Compact layouts enable overview; generous whitespace enables focus. Provide both — don't collapse them into one mode. |
| **Status at a glance** | A user opening any view should understand what is `At Risk` or `Blocked` in under 5 seconds without reading body text. |
| **Progressive disclosure** | Show summary information by default; detailed information on demand (expand, drawer, modal). Never bury the tree under required clicks. |
| **Accessible by default** | Accessibility is not a final-phase audit item. Every new component ships meeting WCAG 2.1 AA. |

---

## 2. Color System

The palette uses a neutral base with one primary brand color and semantic colors for statuses. All values are defined as CSS custom properties so they can be overridden per-tenant.

### 2.1 Primitive Palette (CSS Custom Properties)

```css
/* ── Neutrals ─────────────────────────────────────────── */
--color-neutral-0:   #FFFFFF;
--color-neutral-50:  #F8F9FA;
--color-neutral-100: #F1F3F5;
--color-neutral-200: #E9ECEF;
--color-neutral-300: #DEE2E6;
--color-neutral-400: #CED4DA;
--color-neutral-500: #ADB5BD;
--color-neutral-600: #6C757D;
--color-neutral-700: #495057;
--color-neutral-800: #343A40;
--color-neutral-900: #212529;
--color-neutral-1000: #0D0F10;

/* ── Primary (Corporate Blue) ─────────────────────────── */
--color-primary-50:  #EEF2FF;
--color-primary-100: #E0E7FF;
--color-primary-200: #C7D2FE;
--color-primary-300: #A5B4FC;
--color-primary-400: #818CF8;
--color-primary-500: #6366F1;   /* ← default brand color */
--color-primary-600: #4F46E5;
--color-primary-700: #4338CA;
--color-primary-800: #3730A3;
--color-primary-900: #312E81;

/* ── Semantic: Success ────────────────────────────────── */
--color-success-100: #DCFCE7;
--color-success-500: #22C55E;
--color-success-700: #15803D;

/* ── Semantic: Warning ────────────────────────────────── */
--color-warning-100: #FEF9C3;
--color-warning-500: #EAB308;
--color-warning-700: #A16207;

/* ── Semantic: Danger ─────────────────────────────────── */
--color-danger-100:  #FEE2E2;
--color-danger-500:  #EF4444;
--color-danger-700:  #B91C1C;

/* ── Semantic: Info ───────────────────────────────────── */
--color-info-100:    #DBEAFE;
--color-info-500:    #3B82F6;
--color-info-700:    #1D4ED8;
```

### 2.2 Semantic Tokens (use these in components, not primitives)

```css
--color-bg-page:          var(--color-neutral-50);
--color-bg-surface:       var(--color-neutral-0);
--color-bg-elevated:      var(--color-neutral-100);
--color-bg-overlay:       rgba(0, 0, 0, 0.40);

--color-border-default:   var(--color-neutral-200);
--color-border-strong:    var(--color-neutral-400);
--color-border-focus:     var(--color-primary-500);

--color-text-primary:     var(--color-neutral-900);
--color-text-secondary:   var(--color-neutral-600);
--color-text-placeholder: var(--color-neutral-400);
--color-text-disabled:    var(--color-neutral-400);
--color-text-on-primary:  var(--color-neutral-0);
--color-text-link:        var(--color-primary-600);
--color-text-link-hover:  var(--color-primary-700);

--color-interactive:      var(--color-primary-500);
--color-interactive-hover:var(--color-primary-600);
--color-interactive-active:var(--color-primary-700);
```

### 2.3 Dark Mode

Dark mode is in scope but not priority in Phase 1. Implement using the `prefers-color-scheme` media query by aliasing the semantic tokens. Do not hard-code light-mode values in component CSS.

---

## 3. Typography

Font stack: **Inter** (variable font, loaded via Google Fonts or self-hosted). Fallback: `system-ui, -apple-system, sans-serif`.

### 3.1 Scale

| Token | Size | Line Height | Weight | Usage |
|---|---|---|---|---|
| `--text-xs` | 11px | 16px | 400 | Timestamps, meta labels |
| `--text-sm` | 13px | 20px | 400 | Secondary body, table cells, helper text |
| `--text-base` | 15px | 24px | 400 | Primary body, form inputs |
| `--text-md` | 17px | 26px | 500 | Emphasized body, card titles |
| `--text-lg` | 20px | 30px | 600 | Section headings (H3) |
| `--text-xl` | 24px | 32px | 700 | Page headings (H2) |
| `--text-2xl` | 30px | 40px | 700 | Primary page title (H1) |

### 3.2 Rules
- Never use font sizes outside the scale. Add a token if a new size is genuinely needed.
- Line length: keep readable prose to 60–80 characters wide (`max-width: 72ch`).
- Do not use bold weight (`700`) for body paragraphs. Use `500` (medium) for inline emphasis.
- Node titles in the tree — use `--text-md` at Objective/Process levels, `--text-base` at Initiative/Task levels.

---

## 4. Spacing & Layout

The base unit is **4px**. All spacing values must be multiples of 4.

### 4.1 Spacing Scale

| Token | Value | Common Use |
|---|---|---|
| `--space-1` | 4px | Icon-to-label gap, tight internal padding |
| `--space-2` | 8px | Card internal padding (tight), icon button padding |
| `--space-3` | 12px | Form field internal padding |
| `--space-4` | 16px | Standard component padding, list item gap |
| `--space-5` | 20px | Section internal gap |
| `--space-6` | 24px | Card padding, panel padding |
| `--space-8` | 32px | Between cards, major section gaps |
| `--space-10` | 40px | Between primary sections |
| `--space-12` | 48px | Page-level vertical rhythm |

### 4.2 Layout Grid

- **Desktop:** 12-column grid, 24px gutters, 1280px max content width.
- **Tablet (768px–1279px):** 8-column grid, 16px gutters.
- **Mobile (<768px):** 4-column grid, 16px gutters.

The left sidebar (navigation + tree) is fixed at **240px** on desktop. The main content panel takes the remaining width. A detail drawer opens at **420px** from the right, overlapping main content on <1024px viewports.

### 4.3 Border Radius

| Token | Value | Usage |
|---|---|---|
| `--radius-sm` | 4px | Inputs, tags, small badges |
| `--radius-md` | 8px | Cards, panels, dropdowns |
| `--radius-lg` | 12px | Modals, large surface sheets |
| `--radius-full` | 9999px | Avatars, pill badges |

---

## 5. Iconography

Use **MudBlazor's built-in Material icon set** (`MudIcon`) for all icons. Do not mix icon libraries or import separate icon packages.

### Rules
- Icon size: `Size.Small` inline with text, `Size.Medium` for standalone action buttons, `Size.Large` for navigation.
- Icons must never be the sole communication of status or action — always pair with a text label or `Title` / `aria-label`.
- Set `aria-hidden="true"` (via `UserAttributes`) on decorative icons; set `Title` on interactive icon-only buttons for screen reader text.
- Do not create custom SVG icons without design review. Find the closest Material icon equivalent first.

### Icon → Concept Mapping

| Concept | Icon (MudBlazor / Material) |
|---|---|
| Objective | `@Icons.Material.Rounded.TrackChanges` |
| Process | `@Icons.Material.Rounded.AccountTree` |
| Initiative | `@Icons.Material.Rounded.RocketLaunch` |
| Task | `@Icons.Material.Rounded.CheckBox` |
| Add / Create | `@Icons.Material.Rounded.Add` |
| Edit | `@Icons.Material.Rounded.Edit` |
| Delete / Remove | `@Icons.Material.Rounded.Delete` |
| Archive | `@Icons.Material.Rounded.Archive` |
| Expand tree node | `@Icons.Material.Rounded.ChevronRight` |
| Collapse tree node | `@Icons.Material.Rounded.ExpandMore` |
| Blocked | `@Icons.Material.Rounded.Block` |
| At Risk | `@Icons.Material.Rounded.Warning` |
| On Track / Done | `@Icons.Material.Rounded.CheckCircle` |
| Comment | `@Icons.Material.Rounded.ChatBubble` |
| Attachment | `@Icons.Material.Rounded.AttachFile` |
| Owner / Assigned | `@Icons.Material.Rounded.AccountCircle` |
| Due Date | `@Icons.Material.Rounded.CalendarToday` |
| Settings | `@Icons.Material.Rounded.Settings` |
| Search | `@Icons.Material.Rounded.Search` |
| Suggestion / Template | `@Icons.Material.Rounded.Lightbulb` |

---

## 6. Component Patterns

### 6.1 Buttons

Four variants, two sizes.

| Variant | Use |
|---|---|
| `primary` | Single primary action per view (Save, Create) |
| `secondary` | Secondary actions (Cancel, Back) |
| `ghost` | Inline actions in tables or tree rows (Edit, Archive) |
| `danger` | Destructive actions (Delete) — always require confirmation |

Sizes: `sm` (28px height) and `md` (36px height, default).

- Disabled buttons must have `disabled` attribute — not just visual styling.
- Loading state: replace label with a spinner; keep button width stable.

### 6.2 Forms

- Every input has a visible `<label>` — no placeholder-only labelling.
- Validation runs on blur (not on keystroke) and on submit.
- Error messages appear below the field, in `--color-danger-700`, at `--text-sm`.
- Required fields are marked with an asterisk (*) in the label; explain the convention once per form.
- Use `<fieldset>` and `<legend>` for related groups of inputs.

### 6.3 Cards

Cards are the standard surface for node summaries (e.g. on the Dashboard and Board View).

- Background: `--color-bg-surface`
- Border: 1px `--color-border-default`
- Radius: `--radius-md`
- Padding: `--space-6`
- Hover state: box-shadow lift (`0 4px 12px rgba(0,0,0,0.08)`); no color change.
- Cards must not contain interactive elements that are only discoverable on hover on touch devices.

### 6.4 Tree Nodes

The tree is the most important component in the application.

- Each node row is `40px` tall minimum.
- Indentation per level: `--space-6` (24px). Do not use deeper indentation — four levels is the maximum; indentation stays manageable.
- Expand/collapse chevron is on the left of the title.
- Node title is a link to the detail view — opens a right-side drawer, not a full navigation.
- The level type badge (Objective / Process / Initiative / Task) appears to the left of the title icon.
- Status badge appears to the right of the title, only if the status is not `Not Started` or `Active`.
- Progress bar (inline, 64px wide) appears to the right of the status if there are child nodes.
- Owner avatar appears at the far right.

### 6.5 Status Badges

Status is communicated with a colored dot + label. Do not rely on color alone.

| Status | Dot Color | Label |
|---|---|---|
| Not Started | `--color-neutral-400` | Not Started |
| In Progress / Active | `--color-info-500` | In Progress |
| On Track | `--color-success-500` | On Track |
| At Risk | `--color-warning-500` | At Risk |
| Blocked | `--color-danger-500` | Blocked |
| Done / Complete | `--color-success-700` | Done |
| Cancelled | `--color-neutral-400` | Cancelled (strikethrough text) |
| Archived | `--color-neutral-500` | Archived |

### 6.6 Modals & Drawers

- **Modal:** Used for confirmations (delete, archive, move) and short forms only. Max width: `480px`.
- **Drawer:** Used for node detail view. Width: `420px` on desktop; full-screen on mobile. Opens from the right.
- Always trap focus within open modal/drawer. Close on Escape key. Provide a visible close button.
- Never stack two modals. Use a drawer inside a modal only if unavoidable — prefer navigating away instead.

### 6.7 Empty States

Every view that can be empty must have a purposeful empty state:
- An illustration or large icon (not generic)
- A 1–2 sentence explanation of what belongs here
- A primary CTA (e.g., "Create your first Objective")

Do not show generic messages like "No data found."

---

## 7. The Hierarchy: Visual Language

Each of the four levels has a distinct visual identity to make scanning easy.

| Level | Color | Icon | Badge Label Color |
|---|---|---|---|
| Objective | `--color-primary-600` (Indigo) | `Target` | `--color-primary-700` bg, white text |
| Process | `#0891B2` (Cyan-700) | `GitBranch` | Cyan bg, white text |
| Initiative | `#7C3AED` (Violet-700) | `Rocket` | Violet bg, white text |
| Task | `--color-neutral-700` (Slate) | `CheckSquare` | Neutral bg, white text |

These level colors are used only for the level badge and the left-border accent on tree nodes. Do not flood large areas with these colors.

---

## 8. Status & Progress Conventions

### Progress Calculation
- Task completion is binary: a Task is either `Done`/`Cancelled` or it isn't.
- Initiative progress = `(done_tasks + cancelled_tasks) / total_tasks` × 100, rounded to nearest integer.
- Process progress = mean of progress across all its Initiatives.
- Cancelled Tasks are included in the denominator (they were scope). If you want to exclude cancelled, display a separate "N cancelled" note.

### Progress Bar Display
- Show as a thin (4px) inline bar with a percentage label.
- Color: green (`--color-success-500`) for ≥70%, amber (`--color-warning-500`) for 30–69%, red (`--color-danger-500`) for <30%.
- Do not show progress bars on nodes with zero children.

---

## 9. Motion & Animation

Keep animation purposeful and subtle. This is a productivity tool, not a marketing site.

| Context | Duration | Easing |
|---|---|---|
| Tree node expand/collapse | 150ms | `ease-out` |
| Drawer slide in/out | 200ms | `ease-in-out` |
| Modal fade in | 150ms | `ease-out` |
| Toast appear | 200ms | `ease-out` |
| Progress bar fill | 600ms (on mount only) | `ease-out` |
| Hover state transitions | 100ms | `ease-out` |

- All animations must respect `prefers-reduced-motion`. When it is `reduce`, disable transitions and animations entirely.
- Never use animation to delay displaying content. Loader states, not motion.

---

## 10. Accessibility

- **Keyboard navigation:** All interactive elements reachable and operable via keyboard. Tab order must follow visual order.
- **Focus indicators:** Visible focus ring using `outline: 2px solid var(--color-border-focus)` with `outline-offset: 2px`. Never suppress focus rings without providing a custom alternative.
- **Color contrast:** Text on backgrounds must meet 4.5:1 (normal text) or 3:1 (large text / UI components).
- **Screen reader:** Use semantic HTML (`<main>`, `<nav>`, `<section>`, `<article>`, `<aside>`, heading hierarchy). Add `aria-*` attributes only when semantic HTML is insufficient.
- **Tree widget:** Implement the ARIA tree pattern (`role="tree"`, `role="treeitem"`, `aria-expanded`, `aria-selected`).
- **Live regions:** Status changes (e.g., save confirmed) announced via `aria-live="polite"`.
- **Error messages:** Associate error text with inputs via `aria-describedby`.
- **Skip link:** A "Skip to main content" link is the first focusable element on every page.

---

## 11. Code Conventions — General

### Naming
- Use **English** for all identifiers, comments, and documentation.
- Be descriptive; prefer `initiativeProgress` over `pct` or `val`.
- Boolean variables/props: prefix with `is`, `has`, or `can` (`isLoading`, `hasChildren`, `canEdit`).
- Event handlers: prefix with `handle` in components, `on` in props (`handleSave`, `onSave`).

### Comments
- Write comments to explain **why**, not **what**. Code should be self-describing for the what.
- Leave a comment with a ticket/issue reference for any temporary workaround: `// TODO(#42): remove once API returns sorted results`.
- No commented-out code committed to main. Use version history.

### Commits
Format: `<type>(<scope>): <summary>` (Conventional Commits)

| Type | When |
|---|---|
| `feat` | New feature |
| `fix` | Bug fix |
| `refactor` | Refactoring without behavior change |
| `style` | Formatting, whitespace only |
| `test` | Adding or fixing tests |
| `docs` | Documentation only |
| `chore` | Build, deps, tooling |
| `perf` | Performance improvement |

Examples:
```
feat(tree): add drag-and-drop reordering for initiative nodes
fix(auth): refresh token not invalidated on logout
docs(style-guide): add motion conventions section
```

---

## 12. Code Conventions — C# / Blazor

### C# General
- Target **C# 12** (.NET 8) with nullable reference types enabled (`<Nullable>enable</Nullable>` in every project file). Treat nullable warnings as errors.
- Follow Microsoft's [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).
- Use `PascalCase` for types, methods, properties, and public fields. Use `camelCase` for local variables and parameters. Prefix private fields with `_` (`_dbContext`, `_logger`).
- Prefer `record` types for immutable DTOs and value objects. Use `class` for entities and services.
- Use C# primary constructors for services and components where appropriate (supported in C# 12 / .NET 8).
- Prefer expression-bodied members only when the body is a single, readable expression. Do not compress multi-step logic.
- `async`/`await` throughout. Suffix async method names with `Async` (`GetInitiativesAsync`). Never use `.Result` or `.Wait()` on Tasks.
- Never swallow exceptions silently. Always log before re-throwing or converting to a user-facing error.

### Blazor Components
- One component per `.razor` file. Name files in `PascalCase` matching the component type name.
- Place the `@code { }` block at the bottom of the file.
- Use code-behind `.razor.cs` partial classes when the `@code` block exceeds ~60 lines or contains non-trivial logic.
- Separate UI concerns from data concerns: components should receive data via parameters or injected services — do not write EF Core queries inside `.razor` files.
- Use `[Parameter]` for inputs; use `[CascadingParameter]` sparingly and only for app-wide state (e.g., current user context).
- Use `EventCallback<T>` for child-to-parent communication. Prefer strongly typed callbacks over generic `Action` delegates.
- Use `StateHasChanged()` only when Blazor cannot detect the change automatically (e.g., after a background thread update). Document why it is needed.
- Dispose `IDisposable` and `IAsyncDisposable` resources properly in `@implements` / `Dispose()` or `DisposeAsync()`.

### Component File Naming
- Pages (routable): `PascalCase.razor` in `Pages/` folder (`ObjectiveList.razor`, `TaskDetail.razor`).
- Shared components: `PascalCase.razor` in `Components/` or a relevant subfolder.
- Layout components: `PascalCase.razor` in `Layout/` (`MainLayout.razor`, `NavMenu.razor`).
- Code-behind: `SameNameAsRazor.razor.cs` in the same folder.

### Dependency Injection
- Register all services in `Program.cs` using the appropriate lifetime:
  - `AddScoped` for services that hold per-request/per-circuit state (EF Core `DbContext`, user session services).
  - `AddTransient` for lightweight, stateless services.
  - `AddSingleton` only for truly application-wide shared state (e.g., suggestion library cache).
- Never resolve services manually via `IServiceProvider` in application code. Use constructor injection everywhere.

### Using Directives
- Place global `using` directives in `GlobalUsings.cs` at the project root for namespaces used across 5+ files.
- Within a file, order `using` directives: System namespaces first, then Microsoft namespaces, then third-party, then project-local. Enforce with an `.editorconfig` rule.

---

## 13. Code Conventions — ASP.NET Core Services

### Service Layer
- Business logic lives in service classes under `Services/`. Services are injected into Blazor components — never access `AppDbContext` directly from a component.
- Service method signatures return `Task<T>` or `Task`. Void-returning fire-and-forget operations are forbidden in services (use Hangfire background jobs instead).
- Use the **repository pattern** lightly: `AppDbContext` is the repository. Create dedicated query objects or service methods for complex queries rather than a formal `IRepository<T>` abstraction over EF Core (which duplicates the Unit of Work that EF Core already provides).

### Error Handling
- Use custom exception types (`NotFoundException`, `ValidationException`, `ForbiddenException`) defined in `Core/Exceptions/`.
- Blazor component `@code` blocks must catch service exceptions and present user-friendly messages — never let a raw exception reach the UI.
- Log all exceptions via `ILogger<T>` with structured logging properties: `UserId`, `EntityType`, `EntityId`, `Operation`.
- Never log sensitive data (passwords, tokens, PII).

### Authorization
- Use ASP.NET Core's authorization policies defined in `Program.cs`. Apply `[Authorize(Policy = "...")]` attributes on page components.
- Check row-level ownership in the service layer (e.g., a Contributor may only edit tasks they own), not in the component.
- Never rely on UI hiding alone for access control — enforce permissions server-side in the service.

### Background Jobs (Hangfire)
- Define job methods as `public` methods on registered services — Hangfire needs to resolve them via DI.
- All job methods are idempotent where possible.
- Use `[AutomaticRetry(Attempts = 3)]` for transient-failure-tolerant jobs (email, export). Mark destructive or non-idempotent jobs as `[AutomaticRetry(Attempts = 0)]`.

### Security
- Validate all user-supplied input with **FluentValidation** before it reaches the service layer. Wire validators into Blazor `EditForm` via a custom `FluentValidationValidator` component.
- Use EF Core parameterized queries exclusively. Never concatenate user input into raw SQL strings.
- Apply `AntiforgeryToken` on all mutating forms (Blazor Server provides this automatically via the circuit; confirm it is not disabled).
- Store passwords via ASP.NET Core Identity's default `PasswordHasher` (PBKDF2). Never store plaintext or weakly hashed passwords.

---

## 14. Code Conventions — Database (PostgreSQL / EF Core)

### Entity & DbContext Design
- Entity class names: **PascalCase, singular** (`Objective`, `AuditLog`, `InitiativeTask`).
- EF Core configures table names via `modelBuilder` in `AppDbContext.OnModelCreating` using the **snake_case, plural** convention (`objectives`, `audit_logs`). Use a naming convention helper (e.g., `EFCore.NamingConventions` package with `UseSnakeCaseNamingConvention()`) rather than annotating every property.
- Every entity inherits from `BaseEntity` which provides: `Id` (Guid, `HasDefaultValueSql("gen_random_uuid()")`), `CreatedAt` (DateTimeOffset, `HasDefaultValueSql("now()")`), and `UpdatedAt` (DateTimeOffset, updated via a `SavingChanges` interceptor).
- Boolean properties: prefix with `Is` or `Has` in C# (`IsArchived`, `HasAttachment`). EF snake_case naming maps these to `is_archived`, `has_attachment`.
- Navigate through relationships using **navigation properties**, not manual FK lookups in application code.

### Many-to-Many (M:M) Join Entities
- Model all M:M relationships with an explicit join entity (`ObjectiveProcess`, `ProcessInitiative`, `InitiativeTask`), not EF Core's implicit skip-navigation. This allows join table metadata (display order, linked date) to be added later.
- Configure M:M relationships in `OnModelCreating`:
  ```csharp
  modelBuilder.Entity<ObjectiveProcess>()
      .HasKey(op => new { op.ObjectiveId, op.ProcessId });
  modelBuilder.Entity<ObjectiveProcess>()
      .HasOne(op => op.Objective).WithMany(o => o.ObjectiveProcesses)
      .HasForeignKey(op => op.ObjectiveId).OnDelete(DeleteBehavior.Cascade);
  // mirror for ProcessId side
  ```
- Deleting a node removes its join table entries (cascade delete). Deleting a join entry does not delete the node on either side.

### Migrations
- All schema changes via EF Core migrations (`dotnet ef migrations add <Name>`).
- Migration names are descriptive and imperative: `AddObjectiveProcessJoinTable`, `AddIsActiveToSuggestions`.
- Never edit a migration file after it has been applied to any shared environment (dev, staging, prod).
- Every migration is reviewed by at least one other engineer before merging (included in the PR).
- Destructive migrations (drop column, drop table) require a `// BREAKING:` comment in the migration and explicit team sign-off before merging to main.
- Suggestion library seed data is applied via a dedicated EF Core data seeder class (`SuggestionLibrarySeeder`), not inside migration files.

### Queries
- Use EF Core LINQ queries via `AppDbContext` in service methods. Avoid raw SQL except for complex aggregate reports.
- For hierarchy traversal that requires recursion, use `FormattableString` interpolated raw SQL with `FromSqlInterpolated` (parameterized automatically by EF Core). Encapsulate in a dedicated service method.
- Eager-load navigation properties explicitly via `.Include()` / `.ThenInclude()`. Do not rely on lazy loading (it is disabled).
- Avoid N+1 queries. When loading a tree level with its children, use a single query with `Include`.
- Index all FK columns. Index any column used in `WHERE` clauses for tables expected to exceed ~10,000 rows. Define indexes in `OnModelCreating` via `HasIndex()`.

---

## 15. Testing Conventions

### Coverage Targets (minimum)
| Layer | Target |
|---|---|
| Service classes (business logic) | 80% line coverage |
| Service integration tests (DB) | Key paths covered (happy path + not found + authorization failure) |
| Blazor components (bUnit) | Critical interactive components (tree, forms, suggestion panel) |
| Utilities / helper methods | 100% |

### Testing Libraries
| Layer | Library |
|---|---|
| Unit tests (services, helpers) | **xUnit** |
| Mocking | **Moq** (or NSubstitute — pick one and be consistent) |
| Assertion library | **FluentAssertions** |
| Blazor component tests | **bUnit** |
| Integration tests (DB) | xUnit + **Testcontainers for .NET** (spins up a real PostgreSQL container) |
| E2E | **Playwright** (.NET binding) |

### Rules
- Test projects live in a `tests/` folder at the solution root, mirroring the `src/` structure: `tests/TeamStrategyAndTasks.Services.Tests/`, `tests/TeamStrategyAndTasks.Web.Tests/`.
- Test class names match the class under test: `ObjectiveServiceTests`, `TreeNodeTests`.
- Test method names follow: `MethodName_StateUnderTest_ExpectedBehavior` — e.g., `GetInitiativesAsync_WhenProcessHasNoInitiatives_ReturnsEmptyList`.
- Do not test implementation details (private methods, internal state). Test observable behavior and outputs.
- Unit tests mock the `AppDbContext` via an in-memory provider or mock the service's repository abstraction. **Do not use the EF Core In-Memory provider for tests that rely on relational constraints** — use Testcontainers instead.
- Integration tests run against a real PostgreSQL instance (Testcontainers). Each test wraps its work in a transaction that is rolled back after the test, leaving the DB clean.
- `[Fact(Skip = "...")]` tests must include a reason and a linked issue. Never commit a skipped test without explanation.

---

## 16. File & Folder Structure

```
Team_Strategy_And_Tasks/              ← Solution root
├── PROJECT_PLAN.md
├── STYLE_GUIDE.md
├── TeamStrategyAndTasks.sln
├── src/
│   ├── TeamStrategyAndTasks.Web/         # Blazor Server application
│   │   ├── Components/
│   │   │   ├── Layout/               # MainLayout.razor, NavMenu.razor, NavBreadcrumb.razor
│   │   │   ├── Shared/               # Reusable UI components (StatusBadge, ProgressBar, …)
│   │   │   ├── Tree/                 # HierarchyTree.razor, TreeNode.razor
│   │   │   ├── Nodes/                # ObjectiveCard, ProcessRow, InitiativeRow, TaskRow
│   │   │   └── Suggestions/          # SuggestionPanel.razor, SuggestionCard.razor
│   │   ├── Pages/                    # Routable page components
│   │   │   ├── Dashboard.razor
│   │   │   ├── Objectives/           # ObjectiveList.razor, ObjectiveDetail.razor
│   │   │   ├── Processes/
│   │   │   ├── Initiatives/
│   │   │   ├── Tasks/
│   │   │   ├── Tree/                 # HierarchyTreePage.razor
│   │   │   ├── Admin/                # SuggestionLibrary.razor, UserManagement.razor
│   │   │   └── Settings/
│   │   ├── wwwroot/
│   │   │   ├── css/
│   │   │   │   ├── tokens.css            # All CSS custom properties
│   │   │   │   └── app.css               # Global styles, imports tokens.css
│   │   │   └── fonts/
│   │   ├── Program.cs
│   │   └── App.razor
│   │
│   ├── TeamStrategyAndTasks.Core/        # Domain + application layer (no UI, no EF)
│   │   ├── Entities/                 # Objective.cs, Process.cs, Initiative.cs, Task.cs
│   │   │                             # Join entities: ObjectiveProcess.cs, etc.
│   │   │                             # Suggestion entities: SuggestionObjective.cs, etc.
│   │   ├── Enums/                    # NodeStatus.cs, UserRole.cs, NodeType.cs
│   │   ├── Exceptions/               # NotFoundException.cs, ForbiddenException.cs, etc.
│   │   ├── Interfaces/               # IObjectiveService.cs, ISuggestionService.cs, etc.
│   │   ├── DTOs/                     # Request/response records (CreateObjectiveRequest, etc.)
│   │   └── Validators/               # FluentValidation validators
│   │
│   ├── TeamStrategyAndTasks.Infrastructure/  # EF Core, DB, jobs, file storage
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/       # IEntityTypeConfiguration<T> per entity
│   │   │   ├── Migrations/           # EF Core migration files (auto-generated)
│   │   │   └── Seeders/              # SuggestionLibrarySeeder.cs
│   │   ├── Services/              # Implementations of Core interfaces
│   │   │   ├── ObjectiveService.cs
│   │   │   ├── ProcessService.cs
│   │   │   ├── InitiativeService.cs
│   │   │   ├── TaskService.cs
│   │   │   ├── SuggestionService.cs
│   │   │   └── AuditService.cs
│   │   ├── Jobs/                  # Hangfire job classes (EmailDigestJob.cs, etc.)
│   │   └── Storage/               # File storage abstraction + implementation
│   │
│   └── TeamStrategyAndTasks.Web.csproj   ← references Core + Infrastructure
│
├── tests/
│   ├── TeamStrategyAndTasks.Core.Tests/    # xUnit unit tests (services, validators)
│   ├── TeamStrategyAndTasks.Web.Tests/     # bUnit component tests
│   └── TeamStrategyAndTasks.Integration.Tests/  # Testcontainers DB integration tests
│
├── docker-compose.yml                  # Dev environment (PostgreSQL only)
└── .github/
    └── workflows/                        # CI/CD pipelines (build, test, migrate)
```

### Solution Conventions
- Three projects: `Web` (Blazor Server host), `Core` (domain/application, no external dependencies), `Infrastructure` (EF Core, Hangfire, storage).
- `Core` has zero external package dependencies. `Infrastructure` depends on `Core`. `Web` depends on both.
- Run `dotnet ef migrations add <Name> --project src/TeamStrategyAndTasks.Infrastructure --startup-project src/TeamStrategyAndTasks.Web` for all migrations (uses .NET 8 / EF Core 8).
- Common `dotnet` scripts are defined as PowerShell scripts in `scripts/` at the solution root: `dev-run.ps1`, `run-tests.ps1`, `run-migrations.ps1`.
