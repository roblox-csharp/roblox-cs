# RobloxCS
This project includes the compiler and transformers.

## To-do
- Table & (the rest of) string macros or extensions
- Macro `IEnumerable<T>` (or `Array`?) methods
- Classes/structs/interfaces nested in classes/structs/interfaces
- Some sort of `LuaTuple` object
- Compile operator methods as regular method declarations but onto `mt`
- Transform parameterized class declarations (i.e `class Vector4(float x = 0, float y = 0, float z = 0, float w = 0)`) into regular class declarations with a constructor\
- Switch expressions
- `do` statement
- Destructuring/parenthesized variable designation (i.e. `var (value1, value2) = tuple;`)
- Interfaces to Luau `export type`
- Anonymous types (e.g. `var x = new { };`)
- Type hoisting when outside of namespace
- Macro `Equals` function to `__eq`
- Macro `GetType` function to the name of the type as a string
- Async/await
- Overloaded methods

## Will maybe be supported
- [Class finalizers (destructors)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/finalizers)
- Custom get/set methods
- `yield` keyword
- `out` keyword
- `partial` keyword
- `using Name = Type` expressions (type aliases)

## Will not be supported
- `using name = value` expressions (equivalent to `:=` operator in other languages)
- Any unsafe context (pointers, `volatile`, `fixed`, etc.)