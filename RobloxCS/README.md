# RobloxCS
This project includes the compiler and transformers.

## To-do
- Inheritance chains 🤮
- Destructuring
- The rest of the built-in libraries
- Omit `require` if only types are used from the import
- Macro number `Parse()` methods to `tonumber()`
- Default values (via `default` keyword)
- Macro `IEnumerable<T>` (or `Array`?) methods
- Classes/structs/interfaces nested in classes/structs/interfaces
- Transform operator methods into a RuntimeLib method call

## Will maybe be supported
- `partial` keyword
- `using Name = Type` expressions (type aliases)

## Will not be supported
- `using name = value` expressions (equivalent to `:=` operator in other languages)
- `volatile` and `unsafe` keywords