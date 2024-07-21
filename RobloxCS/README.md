# RobloxCS
This project includes the compiler and transformers.

## To-do
- Inheritance chains 🤮
- String & table macros or extensions
- Macro bitwise operators to `bit32` library methods
- Macro `IEnumerable<T>` (or `Array`?) methods
- Classes/structs/interfaces nested in classes/structs/interfaces
- Some sort of `LuaTuple` object
- Transform operator methods into a RuntimeLib method call
- Transform parameterized class declarations (i.e `class Vector4(float x = 0, float y = 0, float z = 0, float w = 0)`) into regular class declarations with a constructor

## Will maybe be supported
- `out` keyword
- `partial` keyword
- `using Name = Type` expressions (type aliases)
- Destructuring

## Will not be supported
- `using name = value` expressions (equivalent to `:=` operator in other languages)
- `volatile` and `unsafe` keywords