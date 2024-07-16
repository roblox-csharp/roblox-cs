# RobloxCS
This project includes the compiler and transformers.

## To-do
- Inheritance chains 🤮
- Omit `require` if only types are used from the import
- Add definitions for the rest of the Roblox globals
- Macro `ServiceProvider.GetService<T>()` to `ServiceProvider:GetService("T")`
- Copy `RuntimeLib.lua` into `<project-dir>/include` on compile
- Default values (via `default` keyword)
- Macro `IEnumerable<T>` methods
- Classes/structs/interfaces nested in classes/structs/interfaces
- Macro operator methods into metamethods
- Polyfills:
	- The rest of the `Math` library

## Will maybe be supported
- `partial` keyword

## Will not be supported
- `using name = value` expressions (equivalent to `:=` operator in other languages)
- `volatile` and `unsafe` keywords