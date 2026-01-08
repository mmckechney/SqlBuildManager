

You are an AI assistant working on a .NET 8 C# solution that currently uses legacy ADO.NET data structures based on `System.Data`, including:

- `System.Data.DataSet`, `System.Data.DataTable`, `System.Data.DataColumn`, and `System.Data.DataRow`
- Inherited, strongly-typed variants of these objects (e.g., typed `DataSet`, typed `DataTable`, typed `DataRow` classes generated from XSD schemas or design-time tools)

These strongly-typed ADO.NET types expose domain-specific properties and members while still fundamentally relying on the `System.Data` model.

## Goal

Modernize this solution by:
1. Replacing usages of `DataSet`, `DataTable`, `DataRow`, and `DataColumn` — including their strongly-typed, inherited variants — with strongly-typed C# POCO classes and collections.
2. Applying these changes consistently across ALL projects in the solution.
3. Ensuring behavior is preserved and validated by unit tests.
4. Keeping a running, human-readable log of your plan, tasks, and status updates as you work.

Assume the target runtime is .NET 8.0 and use modern C# language features (nullable reference types, async/await, pattern matching, etc.) where appropriate.

---

## Context & Constraints

- The solution may have multiple projects (e.g., UI, services, domain, data access).
- Existing code relies heavily on ADO.NET `System.Data` types, often using patterns like:
  - Raw `DataSet`, `DataTable`, `DataRow`, `DataColumn` usage
  - Strongly-typed `DataSet`/`DataTable`/`DataRow` subclasses generated from XSDs or design-time tools
  - Property-based access on typed rows (e.g., `customerRow.FirstName`) that map to underlying columns
  - Indexer-based access (e.g., `row["ColumnName"]`) and manual casting/conversion between `object` and concrete types
- We want to move toward clearly defined domain models (POCOs) and strongly typed collections, while introducing minimal breaking changes.
- Prefer incremental, safe refactoring with continuous validation via unit tests.
- Do not introduce new external dependencies (ORMs, libraries) unless explicitly asked; work with what’s already in the repo.

You MUST consider both:
- Direct usages of base `System.Data` types, and
- Indirect usages through their strongly-typed, inherited classes.

If there is any ambiguity about design choices (e.g., naming, nullability, or schema assumptions), propose options and briefly justify your recommendations before applying broad changes.

---

## High-Level Workflow

Follow this workflow for the entire refactoring effort:

1. **Repository & Impact Analysis**
   - Scan the entire solution for usages of:
     - `System.Data.DataSet`
     - `System.Data.DataTable`
     - `System.Data.DataRow`
     - `System.Data.DataColumn`
     - Any strongly-typed subclasses of these types (e.g., typed `CustomerDataSet`, `OrdersDataTable`, `CustomerRow`)
   - Identify:
     - Where `System.Data` is used directly
     - Where strongly-typed, inherited datasets/tables/rows are used
   - Group findings by:
     - Project
     - Namespace / layer (e.g., API, business logic, data access)
     - Conceptual data models (e.g., “Customer”, “Order”, “Invoice”)
   - Produce a concise summary of:
     - Where these types are used
     - How they are used (DTOs, query results, in-memory data, UI binding, etc.)
   - Record this analysis in the log (see “Running Log” section below).

2. **Plan & Task Breakdown**
   - Based on the analysis, propose a plan that:
     - Identifies logical refactoring units (e.g., “Customer module”, “Reporting module”, “Invoice typed dataset”)
     - Defines tasks for each unit (e.g., “Define POCOs for Customer”, “Replace typed DataTable in CustomerService”, “Update tests for Invoice reporting”)
   - Assign each task:
     - A unique ID
     - A short description
     - A status (`Planned`, `In Progress`, `Done`, `Blocked`)
   - Write this plan to the log and keep it up to date as you work.

3. **POCO Design & Mapping Strategy**

   For both raw and strongly-typed ADO.NET types:

   - For each `DataTable`/`DataSet`/typed dataset usage, infer the underlying data model from:
     - Column names and types
     - Typed `DataRow` properties (e.g., `CustomerRow.FirstName`, `OrderRow.OrderDate`)
     - How data is consumed (properties accessed, joins, filters, etc.)
   - Define appropriate POCO classes:
     - Use descriptive names (e.g., `Customer`, `OrderItem`, `InvoiceSummary`)
     - Use correct C# types and nullability annotations (`string?`, `int?`, etc.)
     - Prefer immutable or minimally mutable models where reasonable (e.g., init-only setters).
   - Introduce mapping utilities where needed:
     - Extension methods or helper classes to convert:
       - Typed `DataRow` → POCO
       - `DataRow` → POCO
       - `DataTable` (or typed table) → `List<POCO>`
     - These help incrementally migrate call sites and reduce duplicated mapping logic.
   - When a typed dataset or typed row class is effectively acting as a domain model, design the POCO so that:
     - It preserves the same conceptual data
     - It has clear, strongly-typed properties replacing the strongly-typed ADO.NET row properties
   - Document global design decisions (e.g., naming conventions, nullability strategy, how typed datasets map to POCO aggregates) in the log.

4. **Incremental Refactoring Across the Solution**

   For each identified module / area:

   - Replace method signatures and internal logic that expose or depend on:
     - `DataSet`, `DataTable`, `DataRow`, `DataColumn`
     - Their strongly-typed, inherited variants (typed datasets, typed tables, typed rows)
   - Prefer return types such as:
     - `List<MyPoco>`
     - `IEnumerable<MyPoco>`
     - A single POCO (for single-row results)
     - POCO aggregates or view models instead of typed datasets
   - Inside each method:
     - Replace indexer-based access like `row["ColumnName"]` with strongly typed property access on POCOs.
     - Replace property-based access on typed rows (e.g., `customerRow.FirstName`) with POCO property access.
     - Carefully handle type conversions and nullability.
   - When full replacement is too risky in one step:
     - Use interim mapping from typed datasets/tables/rows to POCOs internally while keeping external APIs stable.
   - Update any consumers of these methods so they operate on POCOs instead of `System.Data` types or their typed subclasses.

5. **Unit Testing & Validation**
   - Before modifying a given module:
     - Try to locate and run existing tests that cover that module, including tests that rely on typed datasets or typed rows.
     - Record test command and results in the log.
   - After refactoring:
     - Update existing unit tests to work with POCOs instead of typed datasets/tables/rows.
     - If tests are missing or too thin, propose and implement additional tests focusing on:
       - Data mapping correctness (typed ADO.NET types → POCOs)
       - Business logic behavior equivalence
   - Run the relevant tests after each major change:
     - Fix or call out any failing tests.
     - Record test runs and outcomes in the log (command used, projects run, and summary of results).

6. **Safety & Review**
   - Minimize breaking changes to public APIs unless explicitly requested.
   - Whenever a breaking change is unavoidable:
     - Note it clearly in the log.
     - Provide a brief migration note or example of how callers should be updated.
   - Keep changes cohesive and logically grouped so that they can be reviewed and merged incrementally.

---

## Running Log Requirements

Maintain a running log in a Markdown file at the repo root named:

`Refactor-SystemData-to-POCO-Log.md`

For the duration of this effort:

- If the file doesn’t exist, create it with sections:
  - `# Refactor System.Data to POCO – Work Log`
  - `## Goals`
  - `## Global Decisions`
  - `## Task List`
  - `## Progress Updates`
- Under **Task List**, maintain a table with fields like:

  | Task ID | Area / Module | Description | Status | Notes |
  |--------|----------------|-------------|--------|-------|

- Under **Progress Updates**, append timestamped entries as you:
  - Complete analysis
  - Start/finish tasks
  - Run tests (include command and summary)
  - Encounter blockers or important design decisions

Keep this log updated as you progress so a human reviewer can easily follow the plan, status, and rationale.

---

## How to Interact with Me

- Start by:
  1. Summarizing your understanding of the repository’s current use of `System.Data` and its strongly-typed, inherited ADO.NET types.
  2. Presenting the initial plan and task list in both:
     - The chat
     - The `Refactor-SystemData-to-POCO-Log.md` file.
- Then proceed iteratively:
  - Before large or risky changes, briefly describe what you’re going to do.
  - After each batch of changes:
    - Update the log (tasks and progress updates).
    - Run relevant unit tests and report the results.
- If at any point assumptions are unclear (schema inference, naming, nullability, how a typed dataset maps to domain POCOs, etc.), ask clarifying questions or present recommended options before moving forward at scale.

=====================================================================

You are an AI developer working in a C#/.NET solution that is being modernized.

# Goal
Complete the migration away from legacy ADO.NET DataSet-based APIs to C# POCO-based models, removing usage of:
- System.Data.DataSet and strongly typed DataSets
- DataTable
- DataRow
- DataColumn

and then **delete the ADO.NET-based classes and files from the solution** once they are no longer referenced, while preserving existing behavior and keeping all tests passing.

# Context
- POCO classes have already been created to replace the strongly typed DataSet/Table/Column/Row objects.
- Mapping helpers exist to map between the old DataSet-based structures and the new POCO models.
- Unit tests have been created or updated to validate the new POCO-based behavior.
- A dedicated feature branch has already been created and checked out for this work.
- You have access to the full repository, can edit files, run tests, and make local git commits.

Assume this is a typical .NET solution. When you need to run tests, use:
- `dotnet test` from the solution root (adjust if you detect a more appropriate test command or solution file).

# High-Level Responsibilities
1. **Analyze usage of ADO.NET DataSet-based types**
   - Find all remaining usages of:
     - `System.Data.DataSet`
     - `System.Data.DataTable`
     - `System.Data.DataRow`
     - `System.Data.DataColumn`
     - Any **strongly typed DataSet classes** and related ADO.NET-based data classes (e.g., designer-generated dataset classes).
   - Identify the files, projects, and layers (e.g., data access, service, UI, tests) that depend on these types.
   - Produce a short, structured plan (with milestones) describing how you will:
     - Replace these usages with the new POCO models and mapping helpers.
     - Update all impacted unit tests.
     - Delete the obsolete ADO.NET-based classes and their files.

2. **Plan and progress log**
   - Create or update a markdown file to track the migration, for example:
     - `docs/dataset-migration-plan.md`
   - In this file, maintain:
     - A brief overview of the goal.
     - A checklist of milestones (e.g., by module, project, or feature area).
     - For each milestone: description, impacted components, and test coverage notes.
     - A running log of what has been completed, including relevant file paths and key changes.

3. **Refactor code and tests in milestones**
   For each milestone in your plan:

   - **Implementation code**
     - Replace DataSet/DataTable/DataRow/DataColumn (and typed DataSet types) with the appropriate POCO classes and mapping helpers.
     - Update any methods, services, repositories, or UI code that previously depended on DataSet-based objects to use the POCO models instead.
     - Ensure that public APIs and external contracts remain backward compatible unless a breaking change is explicitly required.

   - **Unit tests and other tests**
     - Update or add unit tests so they:
       - Use POCOs instead of DataSet-based objects.
       - Validate that behavior, input/output contracts, and edge cases remain equivalent.
     - Remove or refactor any test helpers or fixtures that construct or assert against DataSet/DataTable/DataRow/DataColumn or typed DataSet types.

   - **Local verification for the milestone**
     - Run the relevant tests (preferably `dotnet test` at the appropriate scope).
     - Fix any test failures caused by the migration as part of that milestone.
     - Update the migration markdown file with:
       - The milestone status.
       - Key files changed.
       - Tests run and outcomes.

4. **Deletion of ADO.NET-based classes and files**
   - After you have migrated all usages of a given ADO.NET-based type or typed DataSet class:
     - Confirm there are **no remaining references** in the codebase (including tests).
     - Remove the corresponding ADO.NET-based classes **and their source files** from the solution.
       - This includes any designer-generated `.cs` files and related `.xsd` files for typed DataSets.
     - Update project files (e.g., `.csproj`) to ensure these files are no longer compiled or included.
   - Document, in the migration markdown file:
     - Which classes and files were deleted.
     - Where their functionality is now handled in terms of POCOs/mapping helpers.

5. **Final cleanup and verification milestone**
   - Add a final milestone to your plan for cleanup and verification that:
     - There are **no remaining references** to:
       - `System.Data.DataSet`
       - `System.Data.DataTable`
       - `System.Data.DataRow`
       - `System.Data.DataColumn`
       - Any typed DataSet classes or DataSet-backed “table/row” types.
     - Any remaining necessary uses (e.g., at external integration boundaries that cannot yet be changed) are:
       - Clearly documented in the migration markdown file, and
       - Isolated to well-defined boundary components.
   - Verify:
     - All affected projects build successfully.
     - `dotnet test` (or the appropriate test command) passes successfully for the solution.
   - Document the final state in the migration markdown, including:
     - Completed milestones.
     - Any exceptions and TODOs.

6. **Git workflow**
   - After successfully completing each milestone (including passing tests for that milestone):
     - Stage the related changes.
     - Commit to the **current local branch** with a clear, descriptive message.
       - Use a consistent pattern, for example:
         - `refactor: replace DataSet with POCO in <module-name>`
         - `test: update unit tests for <module-name> POCO migration`
         - `chore: remove legacy DataSet classes from <project-name>`
     - Do not push to remote; only make local commits.

7. **Design decisions and when to stop**
   - You may proceed through your milestones without waiting for my input.
   - However, **pause and ask for my guidance before proceeding** if:
     - You need to change a public API or external contract in a way that might be breaking.
     - You are unsure how to model a shape or behavior with POCOs (e.g., complex hierarchies, dynamic schemas).
     - You encounter an area where performance, concurrency, or transaction semantics might be impacted by the refactor.
     - You find code that relies on DataSet-specific features (e.g., DataRelations, constraints, row state tracking) that are not clearly covered by the existing POCO/mapping design.
   - When you pause, summarize:
     - The options you see.
     - Pros/cons of each.
     - Your recommended approach.
   - Wait for my response before making changes in that area.

8. **Documentation and communication**
   - Throughout the process, keep the migration markdown file updated with:
     - Current plan and status (e.g., “Milestone 2 of 5 complete”).
     - Short bullet points summarizing changes per milestone.
     - Any technical debt, follow-up items, or TODOs you discover.
   - In your responses to me:
     - First, show the updated plan checklist with completed/remaining milestones.
     - Then summarize what changed in the last milestone, including:
       - Key files touched.
       - Behavioral equivalence notes.
       - Test commands run and their outcomes.
       - Commit message(s) used.

# Constraints and Quality Bar
- Preserve behavior and data semantics; do not simplify away important validation or edge cases even if the legacy code looks messy.
- Keep public interfaces stable where possible. If you must introduce a breaking change, clearly highlight it and explain why it is necessary.
- Avoid large, cross-cutting changes in a single commit. Prefer smaller, logically grouped commits per milestone or feature area.
- Ensure there are no remaining references to DataSet/DataTable/DataRow/DataColumn (and typed DataSet classes) in the application code and tests once the migration is complete, except where absolutely necessary (e.g., external library boundaries you cannot change). Document any such exceptions as part of the final milestone.
- Ensure that all obsolete ADO.NET-based classes and their associated files are removed from the solution and its projects once their usages have been fully migrated to POCOs.
You do not need to wait for my response between milestones/tasks; you may proceed through your plan autonomously. Only stop and request input if you encounter a design decision as described above.
