# Team Strategy & Tasks — Project Plan

**Version:** 0.3  
**Date:** 2026-03-23  
**Status:** Draft

---

## 1. Vision

Build a web-based team tool that lets organizations establish, visualize, and manage a four-level strategic hierarchy — from broad business objectives down to individual actionable tasks. The system exists to close the gap between executive intent and day-to-day execution by making the chain of accountability explicit and navigable for every team member.

---

## 2. The Hierarchy Model

The domain model is a four-level hierarchy with **many-to-many relationships between every adjacent pair of levels**. A Process can simultaneously support multiple Objectives; an Initiative can be shared across multiple Processes; a Task can contribute to multiple Initiatives. This reflects real business reality where a single initiative (e.g., implementing 5S) may serve both a cost-reduction objective and a quality objective.

```
Business Objectives  ◄──┐
         │   (M:M)      │
         ▼              │
Business Processes  ◄──┐│
         │   (M:M)     ││
         ▼             ││
    Initiatives  ◄──┐  ││
         │  (M:M)   │  ││
         ▼          │  ││
       Tasks ───────┘  ││
                       ││
 (cross-linked)  ───────┘│
                         │
 (cross-linked)  ─────────┘
```

| Level | Description | Examples |
|---|---|---|
| **Objective** | Broad, measurable business goal | Increase revenue, Reduce internal costs, Improve delivered quality |
| **Process** | An ongoing organizational process that influences one or more Objectives | Sales development plan, Internal quality improvement program, QA procedures |
| **Initiative** | A time-boxed effort to build or improve one or more Processes | Reorganize sales team & refresh goals, Implement mfg. process improvement system, Standardize customer requirements evaluation |
| **Task** | A concrete, assignable unit of work that advances one or more Initiatives | Write cold contact sales script, Implement 5S, Conduct MSA on final inspect method |

### Hierarchy Rules
- An **Objective** may link to many **Processes**; a **Process** may link to many **Objectives** (M:M).
- A **Process** may link to many **Initiatives**; an **Initiative** may link to many **Processes** (M:M).
- An **Initiative** may link to many **Tasks**; a **Task** may link to many **Initiatives** (M:M).
- No node may be left fully unlinked (orphaned) once it participates in any relationship.
- Completion rolls up through links: an Initiative's progress is the mean completion of its Tasks; a Process's progress is the mean of its linked Initiatives; an Objective's progress is the mean of its linked Processes.
- When a node participates in multiple parents, its progress contribution is counted once per parent relationship — not deduplicated.

---

## 3. Goals & Success Criteria

| Goal | Success Criterion |
|---|---|
| Establish strategic clarity | Every task in the system traces to a named Objective |
| Improve execution visibility | Teams can see at a glance what % of tasks are complete for any Initiative |
| Reduce planning overhead | A new Initiative can be created and linked to a Process in under 2 minutes |
| Enable accountability | Each node at every level has a designated owner |
| Support reporting | A manager can export or view a full rollup of status from Objective to Task in one screen |

---

## 3a. Dual-Mode UX Philosophy

The system serves two fundamentally different interaction needs, and these should not share the same interface surface. Conflating them produces a tool that is simultaneously too noisy for leadership reviews and too constrained for day-to-day execution work.

### Workbench Mode — *Where strategy is built*

The workbench is where practitioners (strategy owners, initiative leads, contributors) develop and maintain the hierarchy. The key UX principle is **contextual filtering**: a user picks a parent node — an Objective, a Process — and the rest of the screen reflects only that branch. The M:M relationships that exist in the data are intentionally hidden here; a practitioner building out the Processes under "Reduce Cost of Poor Quality" does not need to see that one of those processes is also linked to "Improve Delivered Quality" at the moment they are creating tasks.

**Design principle:** Show one branch at a time. If you can see the whole strategy at once, you are in the wrong mode.

Workbench surfaces:
- **Unified Strategy page** (`/strategy`) — a single tabbed page (Objectives · Processes · Initiatives) that replaces the former three separate list pages. Each tab has its own search bar, status filter, and saved-filter support. Cross-level context filtering (e.g., "show Processes for this Objective") is exposed via a compact picker within each tab. Tasks remains a dedicated page (`/tasks`) because its interaction model differs — it is assignee-driven with due dates and Kanban, not a contextual hierarchy drill-down.
- Strategy Tree — collapsible hierarchy rooted at a chosen Objective; primary view for navigating and editing a single strategic thread

### Communication View — *Where strategy is explained*

The communication view is designed to be presented on a projector, shared in a meeting, or included in a leadership report. It shows the full strategy on one screen: all Objectives, all linked Processes, all Initiatives — with M:M crossing links visible as connection lines and status colours indicating health at a glance. It must be self-explanatory within 30 seconds. Editing is hidden or disabled; this is a read-optimized surface.

**Design principle:** The audience should understand the health and direction of the whole strategy without clicking into anything.

Communication surfaces:
- Strategy Overview — structured card-per-objective summary with progress bars; boardroom-ready, exportable
- Strategy Map — visual network diagram with nodes and crossing connection lines; reflects the whiteboard diagram that leadership teams naturally draw when discussing strategy

### Why Both Are Necessary

The M:M data model is a strength, not a liability — but it looks like a liability when you show it all at once to someone building a task list, or hide it entirely from someone reviewing strategic alignment. Each mode reveals a different true shape of the same data.

| Dimension | Workbench | Communication View |
|---|---|---|
| **Scope** | One branch at a time | Full strategy on one screen |
| **Primary audience** | Practitioners, team leads | Senior leaders, executives |
| **Interaction model** | Create, edit, link, assign | Read, scan, present |
| **M:M visibility** | Hidden (filtered to one parent) | Explicit — crossing lines and badges |
| **Typical update cadence** | Daily / continuous | Weekly / monthly review |
| **Editing allowed** | Yes — everything | No — read-only by default |

---

## 4. Stakeholder & User Roles

The system enforces **at least two distinct access levels** for non-administrator users:

- **Team Lead tier** — users who own and shape the strategy. They can create and manage Objectives, Processes, and Initiatives, and can also create and assign Tasks. Corresponds to the **Strategy Owner** and **Initiative Lead** named roles.
- **Team Member tier** — users who contribute to execution. They can create and assign Tasks *in support of existing Initiatives* only; they cannot create, edit, or archive Objectives, Processes, or Initiatives. Corresponds to the **Contributor** named role.

| Role | Tier | Description | Key Permissions |
|---|---|---|---|
| **Administrator** | — | Configures the system, manages users and teams | Full CRUD on all levels; user management |
| **Strategy Owner** | Team Lead | Typically an executive or director; defines and owns Objectives and Processes | Create/edit Objectives and Processes; read all |
| **Initiative Lead** | Team Lead | Mid-level manager or team lead; owns Initiatives | Create/edit Initiatives and Tasks under their Processes; read all |
| **Contributor** | Team Member | Individual team member | Create and assign Tasks in support of Initiatives; read the full hierarchy |
| **Viewer** | — | Read-only stakeholder | Read all levels |

---

## 5. Functional Requirements

### 5.1 Hierarchy Management
- FR-01: Create, read, update, archive/delete nodes at all four levels.
- FR-02: Enforce parent-child relationships; prevent orphan nodes.
- FR-03: Reorder nodes within a level via drag-and-drop or manual ordering.
- FR-04: Move an Initiative to a different Process (with confirmation and audit log entry).

### 5.2 Node Detail
- FR-05: Every node stores: title, description (rich text), owner (user), status, target date, created date, last modified date.
- FR-06: Tasks additionally store: assignee (may differ from owner), estimated effort, actual effort, completion date.
- FR-07: Objectives and Processes additionally store: a measurable success metric and a target value.
- FR-08: Any node can have file attachments and threaded comments.

### 5.3 Status & Progress
- FR-09: Task statuses: `Not Started`, `In Progress`, `Blocked`, `Done`, `Cancelled`.
- FR-10: Initiative/Process/Objective statuses: `Not Started`, `Active`, `On Track`, `At Risk`, `Complete`, `Archived`.
- FR-11: Computed progress (%) rolls up automatically from Tasks to Initiatives and from Initiatives to Processes.
- FR-12: Highlight nodes that are `At Risk` or `Blocked` on all tree views.

### 5.4 Views

#### Workbench Views
Optimized for practitioners who are building and maintaining the strategy hierarchy day-to-day. Editing is always available. One branch of the hierarchy is in focus at a time.

- FR-13: **Strategy Tree (Workbench)** — collapsible hierarchy rooted at a user-selected Objective. Shows Processes → Initiatives → Tasks in an indented tree. Each node displays its status badge, owner, and computed progress percentage. Nodes that appear under multiple parents show a cross-link badge ("also in 2 other Objectives") to make shared ownership discoverable without being overwhelming. This is the primary workbench for navigating and editing a single strategic thread end-to-end.
- FR-14: **Board View** — Kanban board at the Task level, filterable by Initiative or Assignee. Drag-and-drop cards to change Task status. Designed for day-to-day execution tracking.
- FR-15: **Dashboard** — per-user summary of all nodes the current user owns or is assigned to, overdue items, and items due this week across all levels. Primary landing page after login. Links directly into the workbench views for each item.
- FR-35: **Orphan Triage** — a panel or page that surfaces nodes at any level that have no parent links. Shows a count of unlinked Processes on the Objectives page, unlinked Initiatives on the Processes page, and so on. Users can link or archive orphaned nodes from this view. Prevents strategy gaps from silently accumulating in the filtered workbench views.
- FR-38: **Unified Strategy Page** — replaces the former separate Objectives, Processes, and Initiatives list pages with a single `/strategy` page that uses tabs (Objectives · Processes · Initiatives). Each tab provides its own search bar, status/owner filter, saved-filter support (using the tab name as the PageKey), and context picker (e.g., "filter Processes by Objective"). Navigation collapses from three items to one "Strategy" entry. Individual detail routes (`/objectives/{id}`, `/processes/{id}`, `/initiatives/{id}`) are unchanged. Tasks keeps its own dedicated page because it is assignee-driven (due dates, Kanban) rather than a hierarchy drill-down.

#### Communication Views
Optimized for communicating strategy to senior leadership. Editing is hidden or disabled. Designed to be legible on a projector or large monitor without clicking into any node.

- FR-16: **Strategy Overview** — a structured summary page presenting all active Objectives as cards. Each card shows the Objective title, owner, status, linked Process count, total Initiative count, and a computed progress bar rolled up from Tasks. The full strategy health is scannable in under 30 seconds. Suitable for a weekly leadership check-in and exportable to PDF as a leadership report.
- FR-36: **Strategy Map** — a visual network diagram presenting Objectives, Processes, and Initiatives as labelled nodes with connection lines showing the M:M links between levels. Node border colour indicates status (on track / at risk / blocked). Mirrors the whiteboard diagram that leadership teams draw when aligning on strategy. Read-only by default; an advanced "Edit Links" toggle unlocks the ability to add or remove M:M relationships directly on the diagram for users with Strategy Owner or Administrator role.
- FR-37: **Presentation Mode** — any Communication View can enter Presentation Mode via a "Present" button. Hides the application navigation, toolbars, user avatar, and all action buttons. Enlarges node labels and progress text. Suitable for full-screen projection. Pressing Escape or a "Exit Presentation" overlay button returns to normal mode.

### 5.5 Search & Filter
- FR-17: Full-text search across all node titles and descriptions.
- FR-18: Filter any view by: owner, assignee, status, target date range, level.
- FR-19: Saved filters per user.

### 5.6 Notifications
- FR-20: In-app notifications when a node is assigned to/from the current user.
- FR-21: In-app notification when a comment is posted on a node the user owns or is @mentioned in.
- FR-22: Daily digest email of overdue items (opt-in per user).

### 5.7 Audit & History
- FR-23: Full field-level change history for every node.
- FR-24: Revert a node to a previous state (restricted to Administrator and owner).

### 5.8 Import / Export
- FR-25: Export any subtree to CSV or PDF for reporting.
- FR-26: Import Tasks from CSV (for bulk entry under a specified Initiative).

### 5.9 Role-Based Access Control

Two minimum access tiers must be enforced in the system (see Section 4 for full role mapping):

- FR-39: **Team Lead — Strategy Owner access** — users holding the Strategy Owner role are permitted to create, edit, and archive Objectives and Processes, and may also create and assign Tasks.
- FR-40: **Team Lead — Initiative Lead access** — users holding the Initiative Lead role are permitted to create, edit, and archive Initiatives and Tasks under Processes they own; they may not create or modify Objectives or Processes.
- FR-41: **Team Member access** — users holding the Contributor role are permitted to create and assign Tasks in support of existing Initiatives; they may not create, edit, or archive Objectives, Processes, or Initiatives.
- FR-42: All actions restricted by role must be hidden or disabled in the UI for users who do not hold the required role; server-side authorization must independently reject unauthorized mutations regardless of UI state.

### 5.10 Suggestion Library
The system ships with a built-in, read-only library of suggested hierarchy content. Suggestions are curated by the development team and seeded into the database. They are never user-editable.

- FR-27: The suggestion library contains pre-defined Objective templates, each linked to suggested Process templates, which link to suggested Initiative templates, which link to suggested Task templates — mirroring the same M:M hierarchy structure as the live data.
- FR-28: When a user creates a new Objective, the system presents matching Objective suggestions. The user may adopt one (copies title/description into the new node) or dismiss and proceed with a blank form.
- FR-29: When a user adds a Process to an Objective, the system presents the suggested Processes associated with that Objective's template (if one was adopted). Suggestions are also available when no template was adopted, filtered by keyword match.
- FR-30: The same suggestion flow applies when adding Initiatives to Processes and Tasks to Initiatives.
- FR-31: Accepting a suggestion at any level optionally cascades: the user may choose to also bring in the suggested children of that suggestion (e.g., adopt a suggested Initiative and simultaneously add its suggested Tasks as starting points).
- FR-32: Suggestions are displayed as a dismissible side panel alongside the create/edit form — never blocking the form.
- FR-33: A user may mark a suggestion as "not relevant" to suppress it from appearing again for that session.
- FR-34: Administrators can view and manage the suggestion library content through an admin interface (create, edit, deactivate suggestions and their relationships).

#### Seed Suggestion Content (initial library)

The following represents the initial seeded content. This list will grow over time.

**Objective: Reduce Internal Costs**
- Process: Operational Efficiency Program
  - Initiative: Implement 5S Workplace Organization
    - Task: Conduct initial workplace audit (Sort phase)
    - Task: Define and label storage locations (Set in Order)
    - Task: Establish cleaning standards and schedule (Shine)
    - Task: Create 5S audit checklist and assign ownership (Standardize)
    - Task: Schedule recurring 5S audits (Sustain)
  - Initiative: Implement Value Stream Mapping
    - Task: Identify target value stream
    - Task: Map current-state VSM
    - Task: Identify waste and bottlenecks
    - Task: Design future-state VSM
    - Task: Build and execute improvement action plan
  - Initiative: Reduce Unplanned Downtime
    - Task: Audit equipment failure history
    - Task: Implement preventive maintenance schedule
    - Task: Train operators on basic maintenance checks
- Process: Procurement Optimization
  - Initiative: Consolidate Supplier Base
    - Task: Identify redundant suppliers by category
    - Task: Evaluate and score preferred suppliers
    - Task: Negotiate consolidated contracts
  - Initiative: Implement Spend Analytics
    - Task: Export and clean purchase order history
    - Task: Categorize spend by supplier and commodity
    - Task: Identify top cost-reduction opportunities

**Objective: Improve Delivered Quality**
- Process: Quality Assurance Procedures
  - Initiative: Standardize Customer Requirements Review
    - Task: Create a customer requirements intake form
    - Task: Define a requirements review checklist
    - Task: Assign a requirements owner per product line
    - Task: Train sales and engineering on the intake process
  - Initiative: Establish Measurement System Assurance
    - Task: Inventory all inspection gauges and methods
    - Task: Conduct Gauge R&R (MSA) on critical inspection methods
    - Task: Document approved measurement methods
    - Task: Set calibration schedule for all gauges
  - Initiative: Implement Corrective Action System
    - Task: Select and configure a CAPA tracking tool
    - Task: Define CAPA process flow and ownership rules
    - Task: Train team on root cause analysis methods (5-Why, Ishikawa)
    - Task: Establish CAPA review cadence
- Process: Internal Quality Improvement
  - Initiative: Implement SPC on Key Processes
    - Task: Identify critical-to-quality characteristics
    - Task: Define control charts for each CTC
    - Task: Train operators on SPC data collection
    - Task: Establish out-of-control response plan

**Objective: Increase Revenue**
- Process: Sales Development Plan
  - Initiative: Reorganize Sales Team and Refresh Goals
    - Task: Conduct sales team skills assessment
    - Task: Define new territory or vertical assignments
    - Task: Write or refresh role-specific OKRs
    - Task: Establish weekly pipeline review cadence
  - Initiative: Develop New Cold Outreach Strategy
    - Task: Write new cold contact sales script
    - Task: Build target account list by vertical
    - Task: Define outreach sequence (calls, email, LinkedIn)
    - Task: Set up CRM pipeline stages for outbound
- Process: Customer Retention Program
  - Initiative: Launch Customer Success Reviews
    - Task: Define QBR (quarterly business review) template
    - Task: Schedule initial QBRs with top 10 accounts
    - Task: Build customer health scorecard
  - Initiative: Implement Customer Feedback Loop
    - Task: Design post-delivery satisfaction survey
    - Task: Define escalation process for low scores
    - Task: Report survey results monthly to leadership

### 5.10 Teams
- FR-39: The system shall support a **Team** entity with a name and a **description / mandate** field that explicitly states the team's scope of responsibility and what the team is authorized to act on and decide within the strategy hierarchy. This field is the canonical, system-of-record answer to "what is this team here to do?"
- FR-40: Users may be members of one or more Teams. A Team can have multiple members.
- FR-41: Nodes at any level (Objective, Process, Initiative, Task) may be associated with a responsible Team, making ownership and decision-making authority explicit alongside the individual owner field.
- FR-42: A Team's description / mandate is visible on the Team detail page and surfaced as a tooltip or side-panel wherever the Team name appears in the hierarchy views, so that any user can immediately understand the scope of the team responsible for a given node.

### 5.11 Node Dependencies
- FR-43: Any node at any level may declare a dependency on any other node of the same or adjacent level (`blocks` / `blocked-by`). Dependencies are stored in a `NodeDependency` join table (`blocker_type`, `blocker_id`, `blocked_type`, `blocked_id`, `dependency_type: FinishToStart | StartToStart`).
- FR-43a: The Strategy Tree renders dependency arrows or badges so that blocked nodes are visually distinguishable from organically-blocked status.
- FR-43b: When a blocker node is not yet `Done`, the system automatically suggests escalating the blocked node's status to `Blocked`; the status write-back service applies this propagation in the same pass as progress rollup.
- FR-43c: A circular-dependency check runs on creation; the system rejects and reports any cycle.

### 5.12 Key Results on Objectives
- FR-44: Each Objective may have one or more **Key Results** (`title`, `current_value`, `target_value`, `unit`, `owner_id`, `updated_at`). Unit is user-defined (e.g., %, $, days, count, score).
- FR-44a: Each Key Result displays a progress percentage (`current_value / target_value × 100`) and a status badge that mirrors the Objective status vocabulary (`On Track`, `At Risk`, `Complete`).
- FR-44b: An Objective's progress bar on the Strategy Overview and Strategy Tree reflects the mean of its Key Result percentages when Key Results are present, alongside (not replacing) the task-completion rollup. Both signals are labelled and visually distinct.
- FR-44c: Key Result history is captured in the audit log. Users with Objective owner or Strategy Owner role may record updates.

### 5.13 Milestones within Initiatives
- FR-45: Each Initiative may have one or more **Milestones** (`title`, `due_date`, `status: Pending | Reached | Missed`, `completed_at`, `notes`). Milestones are lighter than Tasks — they have no assignee, no effort fields, and are not part of the M:M hierarchy.
- FR-45a: Milestones render as date-markers in the Initiative detail view and on the Timeline / Roadmap view (FR-50).
- FR-45b: A Milestone whose `due_date` has passed and whose status is still `Pending` is automatically flagged as `Missed` by the progress write-back service and triggers a notification to the Initiative owner.

### 5.14 Risk Register
- FR-46: Any node at any level may have one or more **Risks** associated with it (`title`, `description`, `probability: Low | Medium | High`, `impact: Low | Medium | High`, `mitigation`, `owner_id`, `status: Open | Mitigated | Accepted`, `raised_at`, `resolved_at`).
- FR-46a: A computed `severity` field (probability × impact, mapped to a 1–9 scale) is displayed as a colour-coded badge alongside each risk.
- FR-46b: Any node with one or more Open risks whose severity is High/High (9) automatically has its status suggested for elevation to `At Risk` — identical behavior to the dependency propagation in FR-43b.
- FR-46c: An `/admin/risks` page lists all Open risks across the entire hierarchy, sortable by severity, node, and owner. Accessible to Strategy Owner and Administrator roles.
- FR-46d: Risk history is captured in the audit log.

### 5.15 Workload & Capacity View
- FR-47: A `/workload` page presents a per-user and per-team capacity summary, derived from existing node data (no new effort-tracking fields required).
- FR-47a: Per-user rows show: total active nodes owned or assigned, count overdue, count due this week, and aggregate task-completion percentage across all assigned tasks.
- FR-47b: Per-team rows aggregate the same metrics across all team members.
- FR-47c: Rows are sortable and link directly into the Dashboard for the relevant user. Accessible to Strategy Owner and Administrator roles.

### 5.16 Decision Log
- FR-48: A **Decision** entity (`title`, `context`, `rationale`, `alternatives_considered`, `made_by`, `made_at`, `status: Open | Superseded`) may be linked to one or more nodes at any level.
- FR-48a: Decisions are accessible from a `Decisions` tab on any node detail view, and from a global `/decisions` page sorted by date.
- FR-48b: Decisions are distinct from comments (automated, ephemeral) and audit log entries (field-level, automated). They represent conscious, documented strategic choices.
- FR-48c: A Decision may be superseded by linking it to a newer Decision, automatically setting its status to `Superseded` and cross-referencing both.

### 5.17 Timeline / Roadmap View
- FR-50: A `/roadmap` page renders a quarterly swimlane timeline. Rows are grouped by Objective; each row contains horizontal bars for Initiatives (and optionally Milestones as point-markers) plotted against their `target_date` and `created_at` / start date.
- FR-50a: Bars are colour-coded by Initiative status. Hovering a bar shows a tooltip with owner, progress %, and linked Process.
- FR-50b: The view is read-only by default. Users with Initiative Lead or higher may click a bar to open the Initiative detail panel without leaving the timeline.
- FR-50c: Timeline is exportable to PDF using the same export infrastructure as the Strategy Overview (FR-16).
- FR-50d: Implementation uses custom SVG rendering (consistent with the Strategy Map approach) rather than a third-party Gantt library, avoiding external dependencies.

### 5.18 RACI Assignments
- FR-49: Any node at any level may have one or more **RACI assignments** (`user_id`, `raci_role: Responsible | Accountable | Consulted | Informed`). Multiple users may hold the same role on the same node.
- FR-49a: A generated RACI matrix view is available per Initiative and per Process: rows are nodes (Initiatives or Tasks), columns are users, cells show R/A/C/I badges.
- FR-49b: RACI assignments are visible in the node detail view alongside the existing `owner` field. The `owner` field is treated as equivalent to `Accountable` for display consistency.
- FR-49c: RACI changes are captured in the audit log.

### 5.19 Public Read-Only Share Links
- FR-51: Users with Strategy Owner or Administrator role may generate a **share link** for any Communication View (Strategy Overview, Strategy Map, a specific Objective subtree). Share links are token-authenticated, require no login, and render the target view in a read-only, chrome-free layout.
- FR-51a: Share links store: `token` (random 256-bit), `view_type`, `target_id`, `created_by`, `created_at`, `expires_at` (nullable — no expiry if null), `is_revoked`.
- FR-51b: Accessing a revoked or expired token returns a 410 Gone response with a descriptive message.
- FR-51c: Share link accesses are logged (token, timestamp, IP) for audit purposes.

### 5.20 OData / Power BI Connector
- FR-52: The REST API layer is extended with an **OData v4 endpoint** at `/odata` exposing the core hierarchy entities (Objectives, Processes, Initiatives, Tasks, Teams) as queryable OData feeds.
- FR-52a: Supports standard OData query options: `$filter`, `$select`, `$expand`, `$orderby`, `$top`, `$skip`, `$count`.
- FR-52b: Authentication uses the existing JWT bearer scheme (same as the REST API). Read-only — no mutations via OData.
- FR-52c: A documented Power BI connection guide (in the project README) walks through connecting Power BI Desktop to the OData feed using the `/api/auth/token` endpoint to obtain a bearer token.

### 5.21 AI-Generated Suggestions
- FR-53: The suggestion system gains an **LLM-backed mode** alongside the existing curated seed library. When enabled, a new `AiSuggestionService` implementation calls a configured AI model (Azure OpenAI or OpenAI-compatible endpoint) using the existing `ISuggestionService` abstraction — no UI changes required.
- FR-53a: A feature flag (`Ai:Enabled`) in `appsettings.json` switches between the seeded service and the AI service. The seeded library is always available as the graceful fallback when AI is disabled or the API call fails.
- FR-53b: The AI call receives the node title and description as context and returns typed suggestions (Objectives / Processes / Initiatives / Tasks) conforming to the existing DTO shapes via JSON-mode structured output.
- FR-53c: A `Ai:Endpoint`, `Ai:ApiKey`, and `Ai:Model` configuration block maps to the chosen provider. Azure Key Vault is the recommended secret store for deployed environments.
- FR-53d: LLM call latency is hidden behind the existing `_loading` indicator pattern already present in `SuggestionPanel.razor`; no UI-layer changes are needed beyond a spinner state.
- FR-53e: All AI suggestion calls are logged to the audit log with the model version used, for traceability.

### 5.22 Shared Values Page (Culture Surface)
- FR-54: The system shall provide a dedicated **Shared Values** surface where each value is stored as a short word or phrase (`name`) plus a definition/explanation (`definition`).
- FR-54a: Shared Values are managed in an Administrator-authorized editor (create, edit, reorder, archive).
- FR-54b: The display view (`/shared-values`) must render all active values and their definitions on a **single portrait page** suitable for leadership communication and onboarding.
- FR-54c: A print layout is required for U.S. Letter portrait (`8.5in x 11in`) with margins and typography tuned so all active values and definitions print legibly on one sheet when the configured maximum number of values is respected.
- FR-54d: The page must include a print action that opens browser print with print CSS optimized for portrait orientation and no application chrome.
- FR-54e: Validation rules enforce concise content limits (for example, max title length and max definition length) to preserve one-page printability.

### 5.23 Quality Engineering Module (Separated Domain)
This is intentionally treated as a **separate product area** from strategic planning/workbench surfaces. It should have its own navigation root (`/quality`), data model, permissions, and roadmap so quality records are not blended into day-to-day strategy hierarchy pages.

- FR-55: Introduce a new **Quality Engineering module** aligned to AS9100 compliance needs, with bounded-context separation from the strategy module.
- FR-55a: Provide an **AS9100 Clause Conformance Evidence** submodule where each auditable clause can be recorded, assigned, statused, and linked to objective evidence artifacts (documents, records, links, notes, owners, review dates).
- FR-55b: Clause records must support conformance state tracking (`Conforming`, `Partially Conforming`, `Nonconforming`, `Not Assessed`) and retain audit-ready history of status and evidence changes.
- FR-55c: Provide **PFMEA** management (process/item, failure mode, effects, causes, controls, severity/occurrence/detection scoring, recommended actions, and action closure).
  - **Future enhancement (defer to FR-56 integration):** When work instructions are introduced in the Process Engineering module, the PFMEA editor should be able to populate the *Process Step* field by selecting from the defined steps of a linked work instruction, rather than requiring free-text entry. This would eliminate duplication and ensure PFMEA failure modes stay in sync with the authoritative operation sequence. Implement as a nullable `WorkInstructionStepId` foreign reference on `PfmeaFailureMode`, resolved lazily via a cross-module lookup service to preserve bounded-context separation.
- FR-55d: Provide **Control Plan** management linked to process characteristics, control methods, reaction plans, sampling/inspection instructions, and revision history.
- FR-55e: Quality-module reporting must include an auditor-facing clause checklist view and evidence packet export, independent from strategy exports.
- FR-55f: Security model must allow quality-specific roles (for example, Quality Manager, Internal Auditor, Process Owner) without coupling to strategy role assumptions.
- FR-55g: Navigation and IA must keep this module visually and structurally distinct from Strategy pages while preserving shared shell/authentication.
- FR-55h: The application shell shall implement a **workspace model** with two first-class workspaces: `Strategic Planning` and `Quality Engineering`. This must not be a one-time hard fork after login; users with permissions to both can switch contexts at any time.
- FR-55i: Post-login routing shall be role-aware and preference-aware: users with access to one workspace land directly there; users with access to both land in their last-used workspace (or a workspace chooser when no preference exists).
- FR-55j: A persistent workspace switcher in the top-level shell shall allow in-session context switching without re-authentication.
- FR-55k: Deep links (`/strategy/*`, `/quality/*`) must open directly to their target workspace and not force intermediate chooser screens.
- FR-55l: Search, saved filters, dashboards, and exports shall be workspace-scoped by default to prevent accidental blending of strategic and quality records.
- FR-55m: Shared platform services (authentication, audit log, attachments, notifications, background jobs) may be reused, while domain models and application services remain separated by bounded-context rules.

### 5.24 Quality Engineering Module Blueprint (IA + Domain)
The Quality module is delivered as a major product area under `/quality` with independent information architecture, explicit role boundaries, and phased rollout.

#### 5.24.1 Workspace & Entry UX Blueprint
- BP-55-UX1: Preferred pattern is **persistent workspace switcher + smart default routing**, not a mandatory chooser page every login.
- BP-55-UX2: If a chooser screen is shown (first login or no preference), present two cards/buttons: `Strategic Planning` and `Quality Engineering`.
- BP-55-UX3: Store `last_workspace` per user to reduce friction for frequent users.
- BP-55-UX4: Keep visual identity distinct per workspace (menu taxonomy, iconography, terminology), while preserving a shared app shell and account/session model.

#### 5.24.2 Quality IA Blueprint (`/quality`)
- BP-55-IA1: `Overview` — compliance posture, upcoming audits, open CAPAs, overdue actions.
- BP-55-IA2: `AS9100 Conformance` — clause library, clause status, evidence links, review cadence.
- BP-55-IA3: `PFMEA` — item/process, failure modes, effects/causes, S/O/D, RPN/AP, recommended actions.
- BP-55-IA4: `Control Plans` — process characteristics, methods, frequency, reaction plans, revision history.
- BP-55-IA5: `Audits & Findings` — internal/external audits, findings, containment/correction/corrective action tracking.
- BP-55-IA6: `CAPA` — root cause workflow, action owners, due dates, effectiveness verification.
- BP-55-IA7: `Reports & Evidence Packets` — clause checklist, evidence export, open-risk trend and closure metrics.
- BP-55-IA8: `Admin` — templates, scoring scales, role mapping, retention and document policy settings.
- BP-55-IA9: `RCA Library` — searchable knowledge base of completed root cause analyses, supporting interactive 5-Why branching trees and Ishikawa fishbone diagrams, with tagging, process-area classification, and a recurring-root-cause roll-up view.

#### 5.24.3 Role & Permission Blueprint
- BP-55-R1: `Quality Manager` — full quality-module admin rights, release approvals, reporting.
- BP-55-R2: `Internal Auditor` — audit planning/execution, findings management, read access to quality records.
- BP-55-R3: `Process Owner` — owns PFMEA/control plan rows and action closure for assigned processes.
- BP-55-R4: `Contributor` — evidence upload, comments, and assigned action updates.
- BP-55-R5: `Observer` — read-only quality visibility.
- BP-55-R6: Permission sets are workspace-scoped; a user may be admin in Strategy and contributor in Quality (or vice versa).

#### 5.24.4 Core Data Blueprint (Initial)
- BP-55-D1: `QualityClause`, `ClauseAssessment`, `ClauseEvidenceItem`, `ClauseReviewEvent`.
- BP-55-D2: `PfmeaRecord`, `FailureMode`, `PfmeaAction`, `SeverityScale`, `OccurrenceScale`, `DetectionScale`.
- BP-55-D3: `ControlPlan`, `ControlPlanCharacteristic`, `ReactionPlan`, `ControlPlanRevision`.
- BP-55-D4: `Audit`, `AuditChecklistItem`, `AuditFinding`, `CapaCase`, `CapaAction`, `EffectivenessCheck`.
- BP-55-D5: Shared references are link-based (`strategy_node_reference`) rather than ownership-based joins, preserving domain separation.
- BP-55-D6: `RcaCase`, `RcaCaseTag`, `FiveWhyNode`, `IshikawaCause`. `RcaCase` carries type (FiveWhys | Ishikawa), status, links to `CapaCase` and/or `AuditFinding`, process area, part family, user-defined tags, and a curated `root_cause_summary` written by the analyst on approval. `FiveWhyNode` implements a self-referencing tree (`parent_id`) supporting branching — any node may have multiple child nodes. `IshikawaCause` stores a 6M category (Man | Machine | Material | Method | Measurement | Environment) plus optional `parent_cause_id` for sub-bones.

#### 5.24.5 Workflow Blueprint
- BP-55-W1: Clause conformance lifecycle: `Not Assessed -> Partially Conforming -> Conforming` with `Nonconforming` exception path.
- BP-55-W2: Finding/CAPA lifecycle: `Open -> Containment -> Root Cause -> Corrective Action -> Effectiveness Verification -> Closed`.
- BP-55-W3: PFMEA action loop: score, prioritize, assign, close, re-score.
- BP-55-W4: Control plan revision workflow: draft, review, approve, supersede with immutable revision history.
- BP-55-W5: RCA lifecycle: `Drafting → InReview → Approved → Archived`. Only Approved RCAs enter the searchable library. An Approved RCA may be re-opened (no new record created; re-open event logged to the audit log).

#### 5.24.6 Reporting & Export Blueprint
- BP-55-RPT1: Clause checklist report with status, owner, due date, and evidence completeness.
- BP-55-RPT2: Audit evidence packet export including clause mappings, findings, CAPA status, and referenced artifacts.
- BP-55-RPT3: PFMEA risk trend reporting (pre/post action score deltas, overdue action aging).
- BP-55-RPT4: Control plan change log and effective-date traceability.

#### 5.24.7 Delivery Blueprint (Phased)
- BP-55-P1: Workspace shell + switcher + `/quality` home + roles/permissions baseline.
- BP-55-P2: AS9100 clause conformance + evidence capture + checklist report.
- BP-55-P3: PFMEA module (scales, scoring, actions, re-score flow).
- BP-55-P4: Control Plan module + revision management + reaction plans.
- BP-55-P5: Audits/Findings/CAPA + evidence packet export + operational hardening.
- BP-55-P6: RCA Library — 5-Why branching tree editor, Ishikawa fishbone diagram editor, RCA list/search page (`/quality/rca`), recurring root cause roll-up view, CAPA integration (surface linked RCA in CAPA detail view), Evidence Packet export additions (5-Why as indented outline; Ishikawa as structured 6M table).

### 5.28 Root Cause Analysis Library (Quality Engineering Extension)

The RCA Library gives quality engineers two interactive investigation tools — a branching **5 Whys tree** and an **Ishikawa fishbone diagram** — alongside a long-lived, searchable organisational knowledge base. As RCA cases are completed and approved, the library accumulates institutional knowledge about manufacturing process weaknesses, recurring failure modes, and proven corrective patterns. Over time this collection becomes a self-service reference that shortens future investigations and reveals systemic gaps.

#### 5.28.1 Core Concepts

- **RCA Case** — the root container. Carries a title, problem statement, type (`FiveWhys | Ishikawa`), status, an optional link to an existing `CapaCase` or `AuditFinding`, `process_area`, `part_family` / product line (optional), a human-readable `root_cause_summary` authored by the analyst on approval, and user-defined tags.
- **5 Whys Tree** — a `FiveWhyNode` entity with `why_question`, `because_answer`, `parent_id` (self-referencing, nullable for the root node), `display_order`, and an `is_root_cause` flag. The structure is inherently n-ary: any node may have multiple child nodes, enabling branching when a single "why?" reveals more than one contributing cause.
- **Ishikawa Diagram** — an `IshikawaCause` entity with a 6M `category` (`Man | Machine | Material | Method | Measurement | Environment`), `cause_text`, `parent_cause_id` (self-referencing, nullable for primary bones), `display_order`, and an `is_root_cause` flag. Primary bones attach to the diagram spine; sub-bones attach to parent cause nodes.
- **RCA Library** — the collection of all `Approved` RCA cases, searchable and browsable across the organisation.

#### 5.28.2 Functional Requirements

- **FR-55-RCA-1**: An RCA Case may be created standalone or linked to an existing `CapaCase` or `AuditFinding`. When linked to a CAPA case, the RCA summary and a link to the full RCA are surfaced in the CAPA detail view alongside the existing analysis fields.
- **FR-55-RCA-2**: The **5 Whys editor** presents an interactive tree view. The analysis starts with an initial "Why?" node pinned to the problem statement. Each node supports branching: any answer node may spawn 1–N child "Why?" questions. Individual nodes may be marked `Is Root Cause`. The tree persists incrementally without a full page reload.
- **FR-55-RCA-3**: The **Ishikawa editor** presents a two-column fishbone-style layout with six 6M category lanes. Within each lane, causes may be added at the primary level and sub-causes nested under any primary cause. Each cause item supports an `Is Root Cause` toggle.
- **FR-55-RCA-4**: RCA status workflow: `Drafting → InReview → Approved`. Only `Approved` RCAs enter the searchable library. An Approved RCA may be re-opened (no new record is created; the re-open event is written to the audit log with a required reason). The analyst completes revisions and re-submits for approval.
- **FR-55-RCA-5**: The **library search** page (`/quality/rca`) supports:
  - Full-text search across `root_cause_summary`, cause/node text, and tags.
  - Filter by RCA type (5 Whys | Ishikawa).
  - Filter by process area and part family.
  - Filter by date range (initiated or approved).
  - Filter by linked CAPA or finding.
  - Each result card shows: title, type badge, process area, approved date, linked CAPA/finding number, and a truncated root cause summary.
- **FR-55-RCA-6**: A **Recurring Root Cause** roll-up view (`/quality/rca/recurring`) groups approved RCAs by the text of their designated root cause nodes. Groups with count ≥ 2 are surfaced as recurring root cause clusters, ordered by cluster size descending, to highlight the highest-frequency process gaps. This view is the primary tool for identifying systemic issues across incidents.
- **FR-55-RCA-7**: Tags are free-form strings on each RCA Case. The tag input autocompletes from existing tags in the database to encourage vocabulary reuse over fragmentation.
- **FR-55-RCA-8**: Users with `Internal Auditor` role or higher may view and search the full RCA library and approve RCA cases. `Contributor` role users may create and edit cases in `Drafting` or `InReview` state but may not approve.
- **FR-55-RCA-9**: 5-Why and Ishikawa data are included in the Evidence Packet export (BP-55-RPT2). A 5-Why tree exports as an indented outline; an Ishikawa diagram exports as a structured table grouped by 6M category with root cause nodes highlighted.

#### 5.28.3 Data Blueprint

Entities added to the Quality bounded context (supplement to BP-55-D6):

| Entity | Key Fields |
|---|---|
| `RcaCase` | `id`, `title`, `problem_statement (text)`, `type (enum)`, `status (enum)`, `linked_capa_case_id (FK, nullable)`, `linked_finding_id (FK, nullable)`, `process_area (varchar 200)`, `part_family (varchar 200, nullable)`, `root_cause_summary (text, nullable)`, `initiated_by_id`, `initiated_at`, `approved_by_id (nullable)`, `approved_at (nullable)`, `is_archived` |
| `RcaCaseTag` | `rca_case_id (FK)`, `tag (varchar 100)` — many tags per case |
| `FiveWhyNode` | `id`, `rca_case_id (FK)`, `parent_id (FK self-ref, nullable)`, `display_order`, `why_question (text)`, `because_answer (text, nullable)`, `is_root_cause (bool)` |
| `IshikawaCause` | `id`, `rca_case_id (FK)`, `category (enum: Man\|Machine\|Material\|Method\|Measurement\|Environment)`, `parent_cause_id (FK self-ref, nullable)`, `display_order`, `cause_text (varchar 500)`, `is_root_cause (bool)` |

#### 5.28.4 UI Blueprint

| Page / Component | Route | Purpose |
|---|---|---|
| `RcaLibrary.razor` | `/quality/rca` | Searchable list of all Approved RCAs; filter/search bar; type and process-area chips; create button for new case |
| `RcaDetail.razor` | `/quality/rca/{id:guid}` | Case header (status, type, problem statement); tabbed editor: **5 Whys** tab and **Ishikawa** tab; tags panel; linked CAPA/finding chip; root cause summary field (editable when in Drafting/InReview); status advance actions |
| `FiveWhysEditor.razor` | (embedded component) | Interactive tree: add Why node, branch from any existing node, mark root causes, reorder; rendered as a collapsible indented tree |
| `IshikawaEditor.razor` | (embedded component) | Six-lane layout; add primary cause and sub-cause per lane; mark root causes; rendered as a structured vertical list grouped by 6M category |
| `RcaRecurring.razor` | `/quality/rca/recurring` | Recurring root cause clusters; ordered by frequency; each cluster expandable to list contributing cases |

#### 5.28.5 Integration Points

- **CAPA Detail** — when a `CapaCase` has one or more linked `RcaCase` records, the CAPA detail page renders an **RCA** chip/link section showing the RCA type badge, title, and current status. Clicking navigates to the full RCA detail.
- **Audit Finding Detail** — same pattern: linked RCAs appear as a chip strip on the Finding detail view.
- **Evidence Packet** — BP-55-RPT2 export is extended to include any linked Approved RCA data (both 5-Why and Ishikawa formats) as appendix sections.
- **Recurring Root Cause ↔ CAPA** — the recurring roll-up view links each cluster entry directly to the CAPA cases that reference those RCAs, creating a closed feedback loop from incident-to-analysis-to-systemic-pattern.

---

### 5.25 Process Engineering Module (Future Placeholder)
This module is a future manufacturing-facing capability focused on designing and executing standardized work instructions and in-process data collection.

- FR-56: Introduce a **Process Engineering module** where engineers design manufacturing process definitions, operation steps, work instructions, and data-collection plans.
- FR-56a: Provide an **engineering-facing design UI** for authoring process templates: operation sequence, required tools, parameter limits, required measurements, inspection checkpoints, and evidence requirements.
- FR-56b: Provide an **operator-facing execute UI** that renders released instructions as guided workflows with step-by-step execution, required inputs, pass/fail criteria, and mandatory data capture.
- FR-56c: Support typed data capture for inspections and process checks (for example numeric measurements, categorical outcomes, comments, attachments, timestamp, operator identity).
- FR-56d: Support process and instruction revision/version control with explicit release states (`Draft`, `In Review`, `Released`, `Superseded`) and full audit history.
- FR-56e: Enable controlled data sharing with Quality Engineering artifacts (notably PFMEA and Control Plan references) without collapsing bounded contexts.
- FR-56f: Allow process execution records to be linked to quality events (findings/CAPA) for traceability when nonconformances occur.

### 5.26 Production Tracking Module (Future Placeholder)
This module is a future operations-facing capability for tracking workpieces and batches through manufacturing using process execution and collected data.

- FR-57: Introduce a **Production Tracking module** to track unit-level or batch-level flow through factory operations.
- FR-57a: Track lifecycle state for each workpiece/batch (for example `Queued`, `In Process`, `Waiting Inspection`, `Rework`, `Completed`, `Scrapped`).
- FR-57b: Use Process Engineering execution events and data-collection outputs as first-class production trace records.
- FR-57c: Provide genealogy/traceability across material lot, operation, operator, timestamp, measurement results, and quality disposition.
- FR-57d: Provide WIP visibility by line/cell/operation with queue aging and bottleneck indicators.
- FR-57e: Provide hold/release and rework routing logic tied to quality outcomes and disposition decisions.
- FR-57f: Support production reporting for throughput, first-pass yield, rework rate, and cycle-time distribution.
- FR-57g: Preserve interoperability with Quality Engineering and Process Engineering while keeping production runtime concerns in a dedicated module boundary.

### 5.27 Cross-Module Context Map (Planning Placeholder)
This context map defines intended bounded-context edges and integration style. It is a planning artifact to prevent accidental domain blending as additional modules are introduced.

```text
                              Shared Platform Services
             (Auth, Audit Log, Attachments, Notifications, Jobs, Reporting)
                                           |
                                           |
  +-----------------------+                |                +-------------------------+
  | Strategic Planning    |<-----ref-------+------ref------>| Quality Engineering     |
  | (/strategy)           |                               +->| (/quality)             |
  | Objectives/Processes/ |                               |  | AS9100, PFMEA, CAPA,   |
  | Initiatives/Tasks     |                               |  | Control Plans          |
  +-----------------------+                               |  +-------------------------+
                                                           \
                                                            \  controlled references
                                                             \
                        +-------------------------------------+-----------------------+
                        |                                                             |
                        v                                                             v
               +--------------------------+                                  +--------------------------+
               | Process Engineering      |----------execution events------->| Production Tracking      |
               | (/process-engineering)   |                                  | (/production)            |
               | Work instructions,       |----quality link refs------------>| WIP, genealogy, flow,    |
               | data-collection design,  |<---PFMEA/control plan refs------| throughput, disposition   |
               | operator execute UI      |                                  | and trace records         |
               +--------------------------+                                  +--------------------------+
```

Context rules:
- CM-01: Each module owns its own aggregate roots and persistence schema; cross-module joins are avoided in write paths.
- CM-02: Cross-module relationships are represented by reference IDs and integration events, not ownership transfer.
- CM-03: Quality is the system of record for PFMEA, CAPA, and Control Plan governance artifacts.
- CM-04: Process Engineering is the system of record for instruction definitions and execution template semantics.
- CM-05: Production Tracking is the system of record for runtime workpiece/batch state, genealogy, and flow history.
- CM-06: Strategic Planning may reference quality/process/production KPIs, but does not own operational execution data.
- CM-07: Shared platform services remain common infrastructure and must not be treated as a domain boundary bypass.

Integration contract placeholders:
- CM-IC1: `ProcessInstructionReleased` event published by Process Engineering for operator-executable revisions.
- CM-IC2: `ProcessExecutionRecorded` event published by Process Engineering and consumed by Production Tracking and Quality.
- CM-IC3: `NonconformanceRaised` event published by Quality and consumed by Production Tracking for hold/rework routing.
- CM-IC4: `DispositionDecided` event published by Quality and consumed by Production Tracking for release/scrap transitions.
- CM-IC5: `ProductionKpiSnapshot` event published by Production Tracking and referenced by Strategic dashboards.

---

## 6. Non-Functional Requirements

| ID | Category | Requirement |
|---|---|---|
| NFR-01 | Performance | Tree views load within 2 seconds for hierarchies up to 5,000 nodes |
| NFR-02 | Scalability | Support up to 500 concurrent users (single-org deployment) |
| NFR-03 | Availability | 99.5% uptime |
| NFR-04 | Security | Role-based access control; all data encrypted in transit (TLS 1.3) and at rest (AES-256) |
| NFR-05 | Accessibility | WCAG 2.1 AA conformance for all UI surfaces |
| NFR-06 | Auditability | All mutations recorded with timestamp and user identity |
| NFR-07 | Portability | Containerized deployment (Docker/Compose); cloud-agnostic |
| NFR-08 | Browser Support | Latest two versions of Chrome, Edge, Firefox, Safari |
| NFR-09 | Database Portability | The application must support both **PostgreSQL** and **SQL Server** as the backing database, selectable at deployment time via configuration. The choice of database must not require any application rebuild — only a connection string and a provider flag in `appsettings.json` (or environment variable). Migrations must be maintained for both providers. |

---

## 7. Tech Stack

### Frontend
| Concern | Choice | Rationale |
|---|---|---|
| Framework | **Blazor Server** (.NET 8 LTS) | Full C# stack; eliminates a JS/API boundary for internal tooling; real-time updates via SignalR |
| Component Library | **MudBlazor** | Material Design components for Blazor; accessible, well-documented, actively maintained |
| Rich Text | **Blazor Quill** / custom Quill interop | Quill.js wrapped for Blazor; adequate for description fields |
| Charts / Progress | **ApexCharts for Blazor** (Blazor-ApexCharts) | Good chart variety; Blazor-native wrapper |

### Backend
| Concern | Choice | Rationale |
|---|---|---|
| Runtime | **ASP.NET Core 8** (C#, .NET 8 LTS) | Same process as Blazor Server; strongly typed throughout |
| ORM | **Entity Framework Core 8** | First-class .NET ORM; migrations, LINQ queries, M:M navigation properties |
| Auth | ASP.NET Core Identity + cookie auth | Integrated with EF Core 8; well-understood; extensible to Entra ID later |
| Background Jobs | **Hangfire** (PostgreSQL storage) | Persistent background jobs (email digests, exports) without Redis dependency |
| Validation | **FluentValidation** | Declarative validation rules; integrates with Blazor EditForm |

### Data
| Concern | Choice | Rationale |
|---|---|---|
| Primary DB | **PostgreSQL 16** | ACID; EF Core 8 provider (Npgsql) is mature; handles M:M join tables cleanly |
| File Storage | S3-compatible (local filesystem for dev, MinIO for self-hosted prod) | Attachments |

### Infrastructure
| Concern | Choice |
|---|---|
| Containerization | Docker + Docker Compose (dev and prod) |
| CI/CD | GitHub Actions |
| Monitoring | Application Insights or OpenTelemetry → Seq (self-hosted) |
| .NET Version | .NET 8 LTS (net8.0) |

---

## 8. High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Client (Browser)                      │
│   Blazor Server Components  ←── SignalR (WebSocket) ──┐  │
└────────────────────────────────────────────────────┬──┘  │
                                                     │      │
┌────────────────────────────────────────────────────▼─────┐
│              ASP.NET Core 8 Application Server            │
│                                                           │
│  ┌──────────────┐  ┌───────────────┐  ┌───────────────┐  │
│  │  Blazor Hub  │  │  Auth (Identity)│  │  Hangfire     │  │
│  │  (SignalR)   │  │  + Cookie Auth │  │  (bg jobs)    │  │
│  └──────────────┘  └───────────────┘  └───────────────┘  │
│                                                           │
│             EF Core 9  (Npgsql provider)                  │
└────────────────────────────┬──────────────────────────────┘
                             │
              ┌──────────────▼──────────────┐
              │       PostgreSQL 16          │
              │  (hierarchy, users,          │
              │   suggestions, comments,     │
              │   audit, hangfire jobs)      │
              └─────────────────────────────┘
```

**Why Blazor Server?** For an internal single-org tool, the server-side rendering model simplifies architecture significantly — there is no separate API layer to maintain. All business logic, data access, and UI state live in the same .NET process. SignalR keeps the UI in sync with server state in real time.

### Data Model Sketch (PostgreSQL / EF Core)

Many-to-many relationships are modeled with explicit join entities so that relationship metadata (e.g., display order, date linked) can be added later without a breaking migration.

```
── Live Hierarchy ──────────────────────────────────────────

objectives          id, title, description, owner_id, status,
                    metric, target_value, target_date,
                    created_at, updated_at

processes           id, title, description, owner_id, status,
                    metric, target_value, target_date,
                    created_at, updated_at

objective_processes objective_id FK → objectives,      ← M:M join
                    process_id   FK → processes,
                    display_order, linked_at

initiatives         id, title, description, owner_id, status,
                    target_date, created_at, updated_at

process_initiatives process_id    FK → processes,       ← M:M join
                    initiative_id FK → initiatives,
                    display_order, linked_at

tasks               id, title, description, owner_id, assignee_id,
                    status, est_effort, actual_effort,
                    completion_date, created_at, updated_at

initiative_tasks    initiative_id FK → initiatives,     ← M:M join
                    task_id       FK → tasks,
                    display_order, linked_at

── Supporting ──────────────────────────────────────────────

users               id, display_name, email, role, is_active, ...
teams               id, name, description (mandate / scope of
                    responsibility), created_at, updated_at
team_members        team_id  FK → teams, user_id FK → users,
                    joined_at                              ← M:M join
comments            id, node_type (enum), node_id, author_id,
                    body, parent_comment_id, created_at
attachments         id, node_type (enum), node_id, uploader_id,
                    filename, storage_key, created_at
audit_log           id, node_type (enum), node_id, user_id,
                    field_name, old_value, new_value, occurred_at

── Suggestion Library (read-only, seeded) ──────────────────

suggestion_objectives    id, title, description, is_active
suggestion_processes     id, title, description, is_active
suggestion_obj_processes suggestion_objective_id,       ← M:M join
                         suggestion_process_id
suggestion_initiatives   id, title, description, is_active
suggestion_proc_inits    suggestion_process_id,          ← M:M join
                         suggestion_initiative_id
suggestion_tasks         id, title, description, is_active
suggestion_init_tasks    suggestion_initiative_id,       ← M:M join
                         suggestion_task_id
```

The suggestion library tables mirror the live hierarchy's M:M structure exactly. The `is_active` flag on each suggestion node allows admins to deactivate suggestions without deleting them (preserving audit history).

---

## 9. Phased Delivery Plan

### Phase 0 — Foundation (Pre-Development)
- [x] Write project plan (this document)
- [x] Write style guide
- [x] Finalize tech stack (Blazor Server / EF Core / PostgreSQL)
- [x] Resolve hierarchy model (many-to-many at all levels)
- [x] Resolve org model (single organization)
- [x] Set up repo, Docker Compose dev environment
- [x] Define and review database schema (ERD)
- [ ] Create wireframes for Strategy Tree, Strategy Map, and node detail panel

### Phase 1 — Core Hierarchy & Workbench (MVP)
**Goal:** Practitioners can build and navigate the full 4-level strategy hierarchy in the workbench. The system is usable end-to-end by the people doing the work.

**In progress:**
- [x] User authentication (email/password via ASP.NET Core Identity)
- [x] CRUD for all four levels (Objective → Process → Initiative → Task)
- [x] M:M linking UI: separate Objectives, Processes, and Initiatives list pages (each with a parent-picker for contextual filtering) — **to be consolidated into the unified Strategy page (FR-38)**
- [x] Suggestion Library — side panel during node creation with cascade adoption flow; manufacturing-focused seed data
- [x] Basic status field on all nodes
- [x] Role enforcement (Admin, Strategy Owner, Initiative Lead, Contributor) — implements the two-tier Team Lead / Team Member access model defined in Section 4 and FR-39–FR-42

**Remaining:**
- [x] **Team management** — create and manage Teams, each with a name and a mandatory description / mandate field that defines the team's scope of responsibility and decision-making authority; assign users to Teams (FR-39, FR-40, FR-41, FR-42)
- [x] **Unified Strategy page** (`/strategy`) — consolidate the Objectives, Processes, and Initiatives list pages into a single tabbed workbench page with per-tab search, filters, saved filters, and context pickers; update nav to a single "Strategy" item (FR-38)
- [x] Strategy Tree (Workbench) — collapsible hierarchy per Objective with status badges, progress %, and cross-link badges (FR-13)
- [x] Node detail view with full field editing (no comments or attachments yet)
- [x] Orphan Triage panel — surface unlinked nodes per level (FR-35)

**Exit criteria:** A team can model their real strategic hierarchy end-to-end, navigate it via the Strategy Tree, and spot any nodes that haven't been linked yet.

### Phase 2 — Communication Views & Progress Tracking
**Goal:** Leadership can review the full strategy on one screen without needing to navigate the workbench. Progress data is computed and displayed automatically.

- [x] Computed progress rollup — Task % → Initiative → Process → Objective (averaged across M:M links, counted once per parent relationship) (FR-11)
- [x] Strategy Overview — per-Objective cards with rolled-up progress bars; exportable to PDF (FR-16)
- [x] Strategy Map — visual network diagram with M:M crossing links; status colour-coding; read-only by default with Edit Links toggle for Strategy Owners (FR-36)
- [x] Presentation Mode — full-screen, chrome-free mode for any Communication View (FR-37)
- [x] Dashboard — per-user owned items, overdue, and due-this-week across all levels (FR-15)

**Exit criteria:** A monthly leadership review can be run directly from the tool without exporting to slides. Objective health is visible at a glance from the Strategy Map. A practitioner's daily view and a leader's monthly review use the same data but completely different surfaces.

### Phase 3 — Collaboration & Workflow
**Goal:** Teams can communicate within the tool and track work in real time.

- [x] Computed progress write-back (update Initiative/Process/Objective status from rollup thresholds)
- [x] Board View (Kanban) for Tasks (FR-14)
- [x] Comments — threaded, on all node types, with @mention (FR-08)
- [x] File attachments — on all node types (FR-08)
- [x] In-app notifications — assignment changes and @mentions (FR-20, FR-21)
- [x] Highlight At Risk and Blocked nodes in all views (FR-12)

**Exit criteria:** A weekly team review can be run entirely within the tool: status updates, blockers called out, comments logged.

### Phase 4 — Intelligence & Automation
**Goal:** Reduce friction, surface insights, and give administrators control over the suggestion library.

- [x] Full-text search across all node titles and descriptions (FR-17)
- [x] Saved filters per user (FR-19)
- [x] Email digest notifications for overdue items (Hangfire jobs) (FR-22)
- [x] CSV import for Tasks (FR-26)
- [x] CSV / PDF export for any subtree (FR-25)
- [x] Audit log viewer — UI for browsing field-level change history (FR-23)
- [x] Node history & revert (FR-24)
- [x] Admin UI for managing suggestion library content (FR-34)

### Phase 5 — Scale & Integration
**Goal:** Fit into the enterprise toolkit.

- [x] SSO / OAuth 2.0 (Microsoft Entra ID priority) (FR-Phase5)
- [x] Webhook events on status changes
- [x] Accessibility audit and remediation (WCAG 2.1 AA — NFR-05)
- [x] REST API layer for external integrations
- [x] Multi-database provider support — PostgreSQL and SQL Server selectable via `appsettings.json`/environment variable, no rebuild required (NFR-09).
  - Swap `Npgsql.EntityFrameworkCore.PostgreSQL` → `Microsoft.EntityFrameworkCore.SqlServer` based on `Database:Provider` config key.
  - Maintain separate migrations folders (`Migrations/Postgres/` and `Migrations/SqlServer/`) or use a shared migration approach that both providers can run without conflict.
  - `UseNpgsql` / `UseSqlServer` branch in `AppDbContext` registration.
  - Replace any PostgreSQL-specific EF functions (e.g., `EF.Functions.ILike`) with provider-neutral equivalents or conditional branches (e.g., `EF.Functions.Like` for SQL Server).
  - Validate that Hangfire PostgreSQL storage switches to `Hangfire.SqlServer` accordingly.
  - Docker Compose dev environment updated to offer a SQL Server container option alongside the existing PostgreSQL container.

### Phase 6 — Depth & Ecosystem
**Goal:** Make strategy actionable at a finer grain, give risk and accountability first-class representation, and open the data to the broader enterprise tooling ecosystem.

- [x] Node dependencies — `blocks` / `blocked-by` relationships with cycle detection and automatic `Blocked` status propagation (FR-43)
- [x] Key Results on Objectives — measurable KRs with `current_value / target_value` progress, dual-signal progress bar alongside task rollup (FR-44)
- [x] Milestones within Initiatives — lightweight date-markers with `Missed` auto-detection and owner notification (FR-45)
- [x] Risk Register — per-node risks with probability × impact severity scoring, auto status elevation, global risk dashboard (FR-46)
- [x] Workload & Capacity View — per-user and per-team active node counts, overdue, and completion % at `/workload` (FR-47)
- [x] Decision Log — structured decision records linked to nodes, with supersession chain; distinct from comments and audit log (FR-48)
- [x] Timeline / Roadmap View — quarterly swimlane at `/roadmap`, Initiative bars grouped by Objective, Milestone markers, PDF export; custom SVG (FR-50)
- [ ] RACI Assignments — per-node R/A/C/I roles with generated matrix view per Initiative or Process (FR-49)
- [ ] Public read-only share links — token-authenticated, expiry-optional, chrome-free Communication View rendering for external stakeholders (FR-51)
- [ ] OData v4 endpoint + Power BI connection guide — read-only queryable feed over the hierarchy at `/odata`, JWT authenticated (FR-52)
- [ ] AI-generated suggestions — LLM-backed `AiSuggestionService` behind a feature flag, Azure OpenAI / OpenAI-compatible, graceful fallback to seed library (FR-53)

**Exit criteria:** Strategic risks are visible and tracked. Every objective has measurable Key Results. The full hierarchy is consumable by BI tools without writing custom queries.

### Phase 7 — Culture & Quality Domains (Parallel Track)
**Goal:** Add two organization-level capabilities that are adjacent to strategy but intentionally separated in UX and domain boundaries: Shared Values communication and AS9100-focused quality engineering.

- [ ] Shared Values page and admin editor with one-page portrait print layout on 8.5x11 (FR-54)
- [ ] Workspace model with persistent switcher (`Strategic Planning` / `Quality Engineering`) and role-aware default routing (FR-55h, FR-55i, FR-55j, FR-55k)
- [ ] Quality module shell (`/quality`) and role model separated from strategy surfaces (FR-55, FR-55f, FR-55g, FR-55l, FR-55m)
- [ ] AS9100 clause conformance evidence tracking (FR-55a, FR-55b)
- [ ] PFMEA module (FR-55c)
- [ ] Control Plan module (FR-55d)
- [ ] Auditor checklist/evidence packet reporting (FR-55e)
- [ ] Blueprint-driven IA rollout (`/quality` overview, audits/findings/CAPA, reports, admin) (Section 5.24)

**Exit criteria:** The organization can print and post a concise one-page Shared Values sheet, and can run internal/external audit preparation for AS9100 using a dedicated quality workspace without mixing records into strategic planning pages.

### Phase 7.1 — Implementation Sequencing Table (Execution Blueprint)
**Goal:** Provide an execution-grade sequence for Quality Engineering delivery with explicit dependencies, sizing, and quality gates.

Sizing legend:
- **S**: 2-5 dev-days
- **M**: 1-3 dev-weeks
- **L**: 3-6 dev-weeks

| Seq | Workstream | Scope / Deliverable | Effort | Depends On | Test Gates (minimum) |
|---|---|---|---|---|---|
| 7.1.1 | Workspace foundation | Persistent workspace switcher, role-aware landing, `last_workspace` preference, direct deep-link behavior (`/strategy/*`, `/quality/*`) | M | Existing auth/session shell | Unit tests for route/role selection; integration test for deep links; manual UX pass for switch persistence |
| 7.1.2 | Quality shell + authorization | `/quality` root, module-specific nav, workspace-scoped search/filter defaults, Quality roles (`Quality Manager`, `Internal Auditor`, `Process Owner`, `Contributor`, `Observer`) | M | 7.1.1 | Authorization integration tests per role; regression for Strategy access boundaries; audit-log verification for role changes |
| 7.1.3 | Shared Values production hardening | Finalize FR-54 acceptance checks (one-page print validation, content-length guardrails, admin UX polish) | S | Existing Shared Values baseline | Print snapshot/manual print QA on Letter portrait; unit tests for validation limits; smoke test for admin create/edit/reorder/archive |
| 7.1.4 | AS9100 conformance module | Clause library, clause assessment workflow, evidence linking, review cadence reminders, clause checklist report v1 | L | 7.1.2 | Integration tests for clause lifecycle transitions; attachment/evidence linking tests; report output verification |
| 7.1.5 | PFMEA module | PFMEA records, S/O/D scoring scales, prioritization, action tracking, re-score loop and trend metrics | L | 7.1.2 | Unit tests for scoring logic and ranking; integration tests for action closure + re-score; regression test on data visibility by role |
| 7.1.6 | Control Plan module | Control plan records, characteristics, control methods, reaction plans, immutable revision history | L | 7.1.2 | Integration tests for revision immutability; unit tests for effective-date/version rules; export validation for control-plan traceability |
| 7.1.7 | Audits, findings, CAPA | Audit planning/execution, findings, CAPA workflow, effectiveness checks, closure governance | L | 7.1.4, 7.1.5, 7.1.6 | Workflow integration tests for CAPA state machine; role-based action authorization tests; overdue/escalation job tests |
| 7.1.8 | Reporting & evidence packets | Auditor-facing evidence packet export, quality dashboard KPIs, aging and closure trend reports | M | 7.1.4, 7.1.7 | Golden-file tests for report payloads; export integrity checks; performance checks on representative datasets |
| 7.1.9 | Operational hardening | Retention policies, migration/backfill scripts, observability dashboards, runbooks, support SOPs | M | 7.1.1-7.1.8 | Migration dry-run in staging; backup/restore rehearsal; synthetic monitoring + alert validation |

Execution guidance:
- Sequence 7.1.1 and 7.1.2 are release blockers for all Quality-domain feature work.
- Sequence 7.1.4 through 7.1.7 may run in parallel by separate squads once quality shell + authorization is stable.
- Each sequence requires a demo checklist and sign-off artifact before advancing to the next dependency tier.
- Release slices should stay vertical (UI + service + persistence + test gates) to avoid accumulating untestable partial layers.

### Phase 7.2 — Ownership & Timeline Table (Planning Baseline)
**Goal:** Assign accountable squads, estimate sprint windows, and define go/no-go criteria for each sequence in Phase 7.1.

Assumptions:
- Two-week sprints.
- Parallel execution begins after workspace and authorization foundations are stable.
- `Target Sprint Window` is a planning baseline and may be adjusted based on team capacity.

| Seq | Suggested Owner Squad | Target Sprint Window | Go / No-Go Criteria |
|---|---|---|---|
| 7.1.1 Workspace foundation | Platform + Web UX | Sprint 1-2 | Go when switcher, role-aware landing, and deep-link routing all pass integration tests. No-go if context switching causes auth/session regressions. |
| 7.1.2 Quality shell + authorization | Platform + Security | Sprint 2-3 | Go when role matrix is enforced end-to-end and strategy/quality access boundaries are validated. No-go if privilege leakage is detected. |
| 7.1.3 Shared Values hardening | Web UX + Product QA | Sprint 2 | Go when one-page print acceptance and admin CRUD/reorder/archive checks are complete. No-go if print layout is unstable across target browsers. |
| 7.1.4 AS9100 conformance module | Quality Domain Squad A | Sprint 3-5 | Go when clause lifecycle and evidence linkage are complete and checklist report is accepted by quality stakeholders. No-go if audit trail is incomplete. |
| 7.1.5 PFMEA module | Quality Domain Squad B | Sprint 3-5 | Go when scoring, ranking, action loop, and re-score logic pass defined tests. No-go if scoring outputs are not deterministic or explainable. |
| 7.1.6 Control Plan module | Quality Domain Squad C | Sprint 3-5 | Go when revision immutability and reaction-plan workflows are verified. No-go if revision history can be overwritten or bypassed. |
| 7.1.7 Audits, findings, CAPA | Quality Domain Squad A + B | Sprint 5-7 | Go when CAPA state machine and effectiveness verification are operational. No-go if closure can occur without required verification evidence. |
| 7.1.8 Reporting & evidence packets | Data/Reporting + Quality QA | Sprint 6-7 | Go when packet exports are complete, consistent, and accepted by internal audit stakeholders. No-go if report completeness or traceability fails. |
| 7.1.9 Operational hardening | Platform + DevOps + Support | Sprint 7-8 | Go when migration dry-run, backup/restore rehearsal, monitoring, and runbooks are all signed off. No-go if operational recovery objectives are unmet. |

Release-readiness checkpoints:
- **Checkpoint A (Foundation Ready):** 7.1.1 and 7.1.2 complete.
- **Checkpoint B (Core Quality Ready):** 7.1.4, 7.1.5, 7.1.6 complete.
- **Checkpoint C (Audit Ready):** 7.1.7 and 7.1.8 complete.
- **Checkpoint D (Production Ready):** 7.1.9 complete and runbook sign-off recorded.

### Phase 8 — Manufacturing Operations Expansion (Future Placeholder)
**Goal:** Extend beyond strategic and quality planning into engineering-defined execution and production flow intelligence.

- [ ] Process Engineering module foundation (FR-56, FR-56a, FR-56d)
- [ ] Engineering design UI and operator execute UI (FR-56a, FR-56b)
- [ ] In-process inspection and measurement data collection framework (FR-56c)
- [ ] Quality linkage model for PFMEA/Control Plan references and nonconformance traceability (FR-56e, FR-56f)
- [ ] Production Tracking module for unit/batch flow visibility (FR-57, FR-57a, FR-57d)
- [ ] Genealogy and quality disposition traceability across operations (FR-57c, FR-57e)
- [ ] Throughput, FPY, rework, and cycle-time reporting (FR-57f)

**Exit criteria:** Engineering can publish executable work instructions with governed revisions, operators can execute and capture required inspection data, and production leadership can trace and monitor unit/batch flow across the factory.

---

## 10. Resolved Decisions

| Decision | Resolution |
|---|---|
| Multi-org / multi-tenant? | **Single organization.** No multi-tenancy in scope. |
| Many-to-many relationships? | **Yes.** All adjacent levels use M:M. Explicit join entities. |
| Tech stack | **Blazor Server + ASP.NET Core 8 + EF Core 8 + PostgreSQL 16.** |
| SSO priority | Not required for Phase 1. Entra ID targeted for Phase 5. |
| Dual-mode UX | **Yes.** Workbench (filtered, edit-optimized) and Communication View (full strategy, read-optimized) are distinct surfaces built on the same data model. This is not negotiable — merging them produces a tool that serves neither audience well. |
| Strategy Tree in Phase 1 | **Yes.** The Strategy Tree is the primary Workbench navigation surface and must ship with the MVP. The Strategy Map (executive view) is Phase 2. |
| M:M display in Strategy Tree | Each node is shown under every parent it belongs to. Cross-link badges make shared ownership visible without tangled arrows. Under a given parent context, a node appears once even if it has multiple siblings under the same parent. |
| Unified Strategy page | **Adopted.** The three separate workbench list pages (Objectives, Processes, Initiatives) are consolidated into a single `/strategy` page with three tabs. Tabs share the same contextual-filter and saved-filter infrastructure, with `PageKey` scoped per tab. Tasks keeps a dedicated page — its interaction model (assignee, due dates, Kanban) is distinct from the hierarchy drill-down. Navigation collapses from three items to one "Strategy" entry. Individual detail routes (`/objectives/{id}`, etc.) are unchanged. |

## 10a. Still Open

1. **Effort units:** Should task effort be tracked as hours, story points, or user-defined? Recommend hours for Phase 1 with user-defined label added in Phase 4.
2. **Hosting target:** Self-hosted on-premise, private cloud, or internal server?
3a. **Database provider at deploy time:** When multi-DB support is implemented (Phase 5), what is the expected default for new deployments — PostgreSQL or SQL Server? This affects which migrations folder is treated as authoritative and which Docker Compose profile is the default.
3. **Suggestion library expansion:** Is the manufacturing-focused seed library sufficient long-term, or will the organization contribute domain-specific additions via the admin UI?
4. **Strategy Map technology:** Rendering the M:M network diagram requires a graph layout library. Options: (a) MudBlazor + custom SVG — lightweight, full control, no extra dependency; (b) Blazor wrapper around D3.js — powerful but adds JS interop complexity; (c) a dedicated Blazor diagram library (e.g., Blazor Diagrams). Recommend starting with custom SVG for a fixed three-level layout (Objective row → Process row → Initiative row with curved SVG lines), which avoids the general graph layout problem entirely and matches the whiteboard-style diagram the user already thinks in.
5. **Presentation Mode entry point:** Should "Present" be a button visible only on Communication Views, or accessible as a keyboard shortcut (e.g., `F11` / `P`) from anywhere in the app?


---

## 11. Out of Scope (v1)

- Integration with project management tools (Jira, Asana, etc.) — Phase 4+
- Mobile native apps
- Custom workflow/approval chains
- Financial/budget tracking against Initiatives
- Multi-tenancy

---

## 12. Developer Testing Notes

### 12.1 Test Layers

- **Unit tests** validate isolated business logic (for example, bootstrap role selection logic).
- **Integration tests** validate real endpoint wiring and infrastructure behavior using a test host and PostgreSQL testcontainers.

### 12.2 Local vs CI Behavior

- Local development:
  - Integration tests attempt to start Docker testcontainers.
  - If Docker is unavailable, tests log an explicit message and return without failing local developer flow.
- CI environments (`CI=true`, `GITHUB_ACTIONS=true`, or `TF_BUILD=True`):
  - Docker availability is required for integration tests.
  - If Docker is unavailable, integration tests fail fast to enforce pipeline coverage.

### 12.3 Recommended Commands

- Run targeted bootstrap unit tests:
  - `dotnet test tests/TeamStrategyAndTasks.Web.Tests/TeamStrategyAndTasks.Web.Tests.csproj --filter "FullyQualifiedName~RoleBootstrapHelperTests"`
- Run targeted bootstrap integration test:
  - `dotnet test tests/TeamStrategyAndTasks.Integration.Tests/TeamStrategyAndTasks.Integration.Tests.csproj --filter "FullyQualifiedName~RegisterBootstrapIntegrationTests"`
- Run full test suite:
  - `./scripts/run-tests.ps1`

### 12.4 Docker Prerequisite for Integration Tests

- Start Docker Desktop before running integration tests locally.
- Ensure `docker compose up -d postgres` succeeds in the repository root when validating database-dependent flows.
