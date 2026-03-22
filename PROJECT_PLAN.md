# Team Strategy & Tasks — Project Plan

**Version:** 0.2  
**Date:** 2026-03-22  
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

## 4. Stakeholder & User Roles

| Role | Description | Key Permissions |
|---|---|---|
| **Administrator** | Configures the system, manages users and teams | Full CRUD on all levels; user management |
| **Strategy Owner** | Typically an executive or director; defines and owns Objectives and Processes | Create/edit Objectives and Processes; read all |
| **Initiative Lead** | Mid-level manager or team lead; owns Initiatives | Create/edit Initiatives and Tasks under their Processes; read all |
| **Contributor** | Individual team member | Create/edit/complete Tasks assigned to them; read the full hierarchy |
| **Viewer** | Read-only stakeholder | Read all levels |

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
- FR-13: **Tree View** — collapsible full hierarchy from any starting level.
- FR-14: **Board View** — Kanban board at the Task level, filtered by Initiative or Assignee.
- FR-15: **Dashboard** — per-user summary of owned items, overdue items, and items due this week.
- FR-16: **Objective Rollup** — single-page summary showing all Objectives, their child Processes, and rolled-up progress bars.

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

### 5.9 Suggestion Library
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
- [ ] Set up repo, CI/CD skeleton, Docker Compose dev environment
- [ ] Define and review database schema (ERD)
- [ ] Create wireframes for Tree View, Dashboard, and node detail panel

### Phase 1 — Core Hierarchy (MVP)
**Goal:** Users can create and browse the full 4-level many-to-many hierarchy, with suggestion library available during creation.

- User authentication (email/password via ASP.NET Core Identity)
- CRUD for all four levels (Objective → Process → Initiative → Task)
- M:M linking UI: add/remove links between levels
- Tree View (collapsible, renders M:M relationships as a DAG)
- Node detail view (no comments or attachments yet)
- Role enforcement (Admin, Strategy Owner, Initiative Lead, Contributor)
- Basic status field on all nodes
- **Suggestion Library** — suggestion side panel during node creation at all four levels; cascade adoption flow

**Exit criteria:** A team can model their real strategic hierarchy end-to-end, guided by the suggestion library.

### Phase 2 — Collaboration & Visibility
**Goal:** Teams can track progress and communicate within the tool.

- Computed progress rollup (Task % → Initiative → Process → Objective, averaged across M:M links)
- Board View (Kanban) for Tasks
- Comments (threaded) on all node types
- File attachments
- Dashboard (owned items, overdue, due this week)
- Objective Rollup report view
- In-app notifications (SignalR-pushed)

**Exit criteria:** A weekly team review can be run entirely within the tool.

### Phase 3 — Intelligence & Automation
**Goal:** Reduce friction and surface insights.

- Full-text search
- Saved filters
- Email digest notifications (Hangfire jobs)
- CSV import for Tasks
- CSV / PDF export
- Audit log viewer (UI)
- Node history & revert
- Admin UI for managing suggestion library content

### Phase 4 — Scale & Integration
**Goal:** Fit into the enterprise toolkit.

- SSO / OAuth 2.0 (Microsoft Entra ID priority)
- Webhook events on status changes
- Accessibility audit + remediation
- REST API layer for external integrations

---

## 10. Resolved Decisions

| Decision | Resolution |
|---|---|
| Multi-org / multi-tenant? | **Single organization.** No multi-tenancy in scope. |
| Many-to-many relationships? | **Yes.** All adjacent levels use M:M. Explicit join entities model these relationships. |
| Tech stack | **Blazor Server + ASP.NET Core 9 + EF Core 9 + PostgreSQL 16.** |
| SSO priority | Not required for Phase 1. Entra ID targeted for Phase 4. |

## 10a. Still Open

1. **Effort units:** Should task effort be tracked as hours, story points, or user-defined?
2. **Hosting target:** Self-hosted on-premise, private cloud, or internal server?
3. **Suggestion library expansion:** Will the organization contribute domain-specific suggestions, or is the initial seed library sufficient long-term?
4. **M:M display in Tree View:** When a node appears under multiple parents, does the tree show it once (canonical parent) or under every parent? (Both modes have merit — recommend showing under every parent with a visual indicator that the node is shared.)

---

## 11. Out of Scope (v1)

- Gantt chart / timeline view
- AI-generated (LLM-based) suggestions — the suggestion library is curated/seeded, not AI-generated
- Integration with project management tools (Jira, Asana, etc.) — Phase 4+
- Mobile native apps
- Custom workflow/approval chains
- Financial/budget tracking against Initiatives
- Multi-tenancy
