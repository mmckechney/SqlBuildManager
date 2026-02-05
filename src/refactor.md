You are assisting me with refactoring and reorganizing an existing C# project: /SqlSync.SqlBuild/SqlSync.SqlBuild.csproj.

GOAL
Review this C# project and propose a more logical, consistent, and readable structure while preserving all existing functionality and behavior. I want the project to still compile AND all existing unit tests in /SqlSync.SqlBuild.Dependent.UnitTest/SqlSync.SqlBuild.Dependent.UnitTest.csproj and /SqlSync.SqlBuild.UnitTest/SqlSync.SqlBuild.UnitTest.csproj to pass with no behavior changes.

When you have completed the changes to the /SqlSync.SqlBuild/SqlSync.SqlBuild.csproj, please ensure the entire solution SqlBuildManager-console.sln builds 
CONTEXT
- I want to improve:
  - File and folder organization
  - Class and namespace organization
  - Readability and understandability (naming, small methods, clarity)
  - Separation of concerns and layering (e.g., domain, application, infrastructure, UI/API)
- I do NOT want:
  - Any behavior changes
  - Any changes to observable behavior verified by existing unit tests
  - Any public API or contract changes unless explicitly called out
  - Any changes that prevent the project from building successfully with `dotnet build` or cause unit tests to fail with `dotnet test`

- Assume there are unit tests in this solution. Treat them as a safety net:
  - Your refactoring suggestions must be compatible with the existing tests.
  - Do not remove or weaken tests.
  - Only suggest changes to tests if they are clearly broken by purely structural moves (e.g., namespace or file moves), and explain why the test change is safe and does not relax coverage.

SOURCE
Assume the current working directory is the root of the C# solution. You can inspect files, suggest edits, and propose a reorganized structure based on what you see here.

EXPECTATIONS
Work in an iterative, safe way that preserves functionality and keeps tests passing:

1. **Initial Assessment**
   - Inspect the solution and project files (e.g., `.sln`, `.csproj`).
   - Summarize the current structure: projects, layers, and main responsibilities.
   - Identify the main pain points in organization, naming, and structure.
   - Call out where tests are located ( `*.UnitTest` projects) and how they appear to map to the main code.

2. **Proposed Architecture & Structure**
   - Propose a clearer high-level architecture (e.g., layers such as Domain, Application, Infrastructure, API/UI).
   - Suggest an improved folder and namespace structure.
   - Provide a summary table or bullet list of:
     - Current structure → Proposed structure
     - Rationale for each major change.
   - For each significant move or rename, briefly note how it might affect test references (e.g., namespaces, class names).

3. **Refactoring Plan (Step-by-Step)**
   - Provide a concrete, ordered plan of refactor steps that can be applied safely, for example:
     - Step 1: Introduce new folders/namespaces.
     - Step 2: Move specific classes/files.
     - Step 3: Rename specific classes/methods for clarity.
     - Step 4: Extract interfaces or abstractions where appropriate.
   - For each step, clearly state:
     - What files/classes are affected.
     - Any corresponding updates that tests would require (e.g., updated using statements, namespaces).
     - The intended outcome.
   - Favor changes that do NOT require modifying tests. When tests do need adjustments (because of mechanical moves/renames only), explicitly explain:
     - What needs to be changed in the tests.
     - Why this does not change test intent or coverage.

4. **Code-Level Improvements (Without Behavior Change)**
   - For selected representative files, propose refactored versions that:
     - Improve naming, method extraction, and clarity.
     - Improve separation of concerns where safe.
     - Do not change behavior or business rules.
   - When suggesting changes, show **before vs after** snippets and explain:
     - Why the refactor is safe.
     - Why it should not affect unit test expectations.
   - Avoid changing method signatures or public contracts unless absolutely necessary and clearly justified.

5. **Build & Safety Checks**
   - After each major set of suggested changes, remind me to:
     - Run `dotnet build` to validate compilation.
     - Run `dotnet test` to ensure all unit tests still pass.
   - Only suggest changes that you believe will keep the build passing and tests green. If something is risky, call that out explicitly and mark it as optional.
   - If you propose any test file changes, clearly explain:
     - The exact edits.
     - Why the change is mechanical (e.g., namespace, type name) and does not reduce test strictness.

CONSTRAINTS
- Do not introduce new third-party dependencies unless I explicitly request it.
- Prefer minimal, incremental refactors over massive rewrites.
- Maintain all public method signatures and public contracts unless you clearly explain the impact and benefits.
- Preserve existing unit tests and their intent. Do not remove or relax assertions. Only adjust tests when required by structural changes (e.g., namespaces, moved types) and explain the rationale.

Now:
1. Start by inspecting the solution and project structure (including test projects) and summarizing what you see.
2. Then propose a reorganized folder/namespace structure and a safe, step-by-step refactoring plan that keeps `dotnet build` and `dotnet test` passing.
3. After that, we can go file-by-file where you propose concrete refactors that preserve behavior and test outcomes.
``