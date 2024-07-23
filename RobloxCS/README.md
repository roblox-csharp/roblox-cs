# RobloxCS
This project includes the compiler and transformers.

## To-do
- String & table macros or extensions
- Macro `IEnumerable<T>` (or `Array`?) methods
- Classes/structs/interfaces nested in classes/structs/interfaces
- Some sort of `LuaTuple` object
- Compile operator methods as regular method declarations but onto `self.mt`
- Transform parameterized class declarations (i.e `class Vector4(float x = 0, float y = 0, float z = 0, float w = 0)`) into regular class declarations with a constructor

## Will maybe be supported
- [Class finalizers (destructors)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/finalizers)
- Deconstructing (i.e. `var (value1, value2) = tuple`)
- Custom get/set methods
- `out` keyword
- `partial` keyword
- `using Name = Type` expressions (type aliases)

## Will not be supported
- `using name = value` expressions (equivalent to `:=` operator in other languages)
- `volatile` and `unsafe` keywords