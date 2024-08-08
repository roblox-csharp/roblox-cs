# Luau AST

## To-do
- Generate blocks that are direct children of other blocks (or the compilation unit) as `ScopedBlock`s
- Only generate interface declarations & inherit from interfaces if they have methods with implementations
- Map `dynamic` to no type at all

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
- [x] namespaces
- [ ] enums
- [ ] interfaces
- [ ] partial classes/structs/interfaces
- [ ] classes
	- [x] constructors
	- [ ] destructors/finalizers?
	- [ ] fields
	- [ ] properties
	- [x] methods
	- [ ] operator overloading
	- [ ] inheritance
- [ ] reflection
	- [x] nameof
	- [ ] typeof
	- [ ] `Type` class

## Will not be supported
- Structs
- Records
- Events (via the `event` keyword, not Roblox events)
- Any unsafe context (pointers, `unsafe` keyword, `stackalloc`, etc.)
- `ref` keyword