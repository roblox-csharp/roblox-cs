# RobloxCS (rewrite)

## To-do
- Generate blocks that are direct children of other blocks or the compilation unit as a `do ... end` statement

## Roadmap

In no particular order:
- [x] built-in attributes (aka `@native`)
- [x] variables 
- [x] local methods
- [x] anonymous methods 
- [x] anonymous object creation
- [x] returning
- [x] calling
- [x] lambdas 
- [x] ternary operator (`condition ? whenTrue : whenFalse`)
- [ ] save navigation (`a?.b?.c`)
- [ ] if using temporary variables and a regular variable with the same name is created, append temporary names with `_n` where `n` is the index of the temporary variable
- [ ] emit comments
- [ ] method overloading
- [ ] using postfix unary ops as expressions (`doSomething(i++);`)
- [ ] using assignments as expressions (`var x = i = 2;`)
- [x] control flow
	- [x] if
	- [x] while
	- [x] for
	- [x] foreach
	- [x] do
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