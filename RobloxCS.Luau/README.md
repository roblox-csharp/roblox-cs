# Luau AST

## To-do
- Generate blocks that are direct children of other blocks (or the compilation unit) as a `do..end` statement
- If using temporary variables and a regular variable with the same name is created, append temporary names with `_n` where `n` is the index of the temporary variable

## Roadmap

In no particular order:
- [ ] save navigation (`a?.b?.c`)
- [ ] emit comments
- [ ] method overloading
- [ ] patterns
	- [ ] `is`
	- [ ] `not`
	- [ ] type
	- [ ] declaration
	- [ ] relational
- [ ] control flow
	- [x] if
	- [x] while
	- [x] for
	- [x] foreach
	- [x] do
	- [ ] switch statements
	- [ ] switch expressions
- [ ] types
	- [x] map primitives to luau
	- [ ] generics
- [ ] namespaces
- [ ] enums
- [ ] interfaces
- [ ] partial classes/structs/interfaces
- [ ] classes
	- [ ] constructors
	- [ ] destructors/finalizers?
	- [ ] fields
	- [ ] properties
	- [ ] methods
	- [ ] operator overloading
	- [ ] inheritance
- [ ] reflection
	- [x] nameof
	- [ ] `Type` class

## Will not be supported
- Structs
- Records
- Events (via the `event` keyword, not no Roblox events)
- Any unsafe context (pointers, `unsafe` keyword, etc.)
- `ref` keyword