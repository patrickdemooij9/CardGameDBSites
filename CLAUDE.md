# CLAUDE.md

Solution-wide conventions. Applies to every project here: `SkytearHorde.*`, `CardGameDBSites.*`, `AdServer`.

## Comments

**Keep comments minimal. No comments is usually the right amount.** Write code that explains itself
instead — clear names, small functions, obvious structure.

When something genuinely needs explaining, **one line is the budget**:

- a non-obvious workaround or a subtle trap (a reactivity gotcha, a SQL join that double-counts
  without a guard, a value that must stay in sync with another file)
- *why* a decision was made, when the alternative looks more obvious than it is

Do not write:

- comments that restate what the code already says
- XML doc blocks on DTOs, models, or self-evident properties
- multi-line narrative blocks explaining design rationale — that belongs in the PR description
- section banners and decorative dividers

If a comment is longer than the code it describes, delete it.

## C# file layout

**One type per file**, named after the type. A class, enum, interface, or record gets its own file -
no bundling several related models into a shared `*Models.cs`, and no trailing helper/row classes at
the bottom of a repository or service file.

Put them in a folder that matches the namespace.

## Per-project notes

- `CardGameDBSites.Frontend/agents.md` — Nuxt/Vue conventions, commands, directory layout
- `SkytearHorde.Tests/AGENTS.md` — NUnit + Moq patterns, test placement, `CardTestHelper`
