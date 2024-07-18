# RobloxCS
This project includes the compiler and transformers.

## To-do
- Inheritance chains 🤮
- String & table macros or extensions
- Macro bitwise operators to `bit32` library methods
- Macro `IEnumerable<T>` (or `Array`?) methods
- Classes/structs/interfaces nested in classes/structs/interfaces
- Transform operator methods into a RuntimeLib method call
- Some sort of `LuaTuple` object

## Will maybe be supported
- `out` keyword
- `partial` keyword
- `using Name = Type` expressions (type aliases)
- Destructuring

## Will not be supported
- `using name = value` expressions (equivalent to `:=` operator in other languages)
- `volatile` and `unsafe` keywords