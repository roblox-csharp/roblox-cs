# Luau AST

## To-do
- Generate blocks that are direct children of other blocks (or the compilation unit) as `ScopedBlock`s
- Only generate interface declarations & inherit from interfaces if they have methods with implementations
- Map `dynamic` to no type at all

## Roadmap

Key:
- `?` -> will maybe be added

In no particular order:
- [ ] save navigation (`a?.b?.c`)
- [ ] emit comments?
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
	- [x] `return`
	- [x] `break`
	- [x] `continue`
- [ ] types
	- [x] map primitives to luau
	- [ ] generics
- [ ] type hoisting
- [x] namespaces
- [ ] enums
- [ ] interfaces
- [ ] partial classes/structs/interfaces
- [ ] classes
	- [x] `new`
	- [x] constructors
	- [ ] destructors/finalizers?
	- [x] fields
	- [x] basic properties
	- [ ] property getters
	- [ ] property setters
	- [x] methods
	- [ ] operator overloading
	- [ ] inheritance
- [ ] reflection
	- [x] nameof
	- [ ] typeof (partially done!)
	- [ ] `object.GetType()`

## Will not be supported
- Structs
- Records
- Events (via the `event` keyword, not Roblox events)
- Any unsafe context (pointers, `unsafe` keyword, `stackalloc`, etc.)
- `ref` keyword