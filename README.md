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
- [x] lambdas 
- [ ] method overloading
- [ ] using postfix unary ops as expressions (`doSomething(i++);`)
- [ ] using assignments as expressions (`var x = i = 2;`)
- [ ] control flow
	- [x] if
	- [x] while
	- [x] for
	- [ ] foreach
	- [x] do
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
	- [x] nameof
	- [ ] `Type` class

## Will not be supported
- Structs
- Records
- Events (via the `event` keyword, not no Roblox events)
- Any unsafe context (pointers, `unsafe` keyword, etc.)