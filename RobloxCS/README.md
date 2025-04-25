# RobloxCS

## To-do

Key:

- `?` -> will maybe be added

In no particular order:

- [ ] disallow method grouping with methods from objects/classes
- [ ] only generate interface declarations & inherit from interfaces if they have methods with implementations
- [ ] map `dynamic` to no type at all
- [ ] a LOT more testing
    - [ ] generation tests (started)
    - [ ] luau rendering tests (mostly done)
    - [ ] transformer tests
        - [ ] main transformer (started)
    - [ ] runtime library tests (started)
    - [ ] utility tests
        - [ ] standard utility (started)
        - [ ] ast utility (started)
        - [ ] file utility
- [ ] save navigation (`a?.b?.c`)
- [ ] macro `ToNumber()`, `ToUInt()`, `ToFloat()`, etc. (defined in Roblox.cs in RobloxCS.Types) to `tonumber()`
    - this may be replaced with the task below
- [ ] macro primitive type static properties/methods (e.g. `int.TryParse()`, `int.MaxValue`, `string.Empty`)
- [ ] prefix increment/decrement (`++a`, `--a`)
- [ ] transform primary constructors (e.g. `class Vector4(float x = 0, float y = 0, float z = 0, float w = 0)`) into
  regular class declarations with a constructor
- [ ] member access or qualified names under the Roblox namespace should be unqualified (e.g. `Roblox.Enum.KeyCode.Z` ->
  `Enum.KeyCode.Z`)
- [ ] emit comments?
- [ ] method overloading
- [ ] patterns
    - [ ] `is`
    - [x] `not`
    - [x] type
        - [ ] nested types
    - [ ] declaration
    - [x] relational
- [ ] control flow
    - [x] if
    - [x] while
    - [x] for
    - [x] foreach
    - [x] do
    - [x] switch statements
    - [ ] switch expressions
    - [x] `return`
    - [x] `break`
    - [x] `continue`
- [x] types
    - [x] map primitives to luau
    - [x] generics
- [x] type hoisting
- [x] namespaces
- [x] enums
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
    - [ ] constructor overloading
    - [ ] operator overloading
    - [ ] inheritance
- [ ] reflection
    - [x] nameof
    - [ ] typeof (mostly done!)

## Will not be supported

- Structs
- `object.GetType()` (sorry, we can't access C# types during runtime!)
- Any unsafe context (pointers, `unsafe` keyword, `stackalloc`, etc.)
