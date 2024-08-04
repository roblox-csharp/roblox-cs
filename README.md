# RobloxCS (rewrite)

## To-do
- Generate blocks that are direct children of other blocks or the compilation unit as a `do ... end` statement

## Roadmap

In no particular order:
- [x] built-in attributes (aka `@native`)
- [x] local methods
- [x] anonymous methods 
- [x] returning
- [x] calling
- [ ] lambdas 
- [ ] method overloading
- [ ] control flow
	- [x] if
	- [x] while
	- [ ] for
	- [ ] foreach
	- [ ] do
- [ ] types
	- [x] map primitives to luau
	- [ ] generics
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
	- [ ] nameof
	- [ ] `Type` class

## Will not be supported
- Structs
- Records
- Events (via the `event` keyword, not no Roblox events)
- Any unsafe context (pointers, `unsafe` keyword, etc.)