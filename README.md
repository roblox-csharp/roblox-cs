# roblox-cs

![Tests](https://github.com/R-unic/roblox-cs/actions/workflows/tests.yml/badge.svg)  
Roblox CSharp to Lua compiler

# To-do
- Test `DebugTransformer`
- Macro any method that takes an instance name and returns that instance (`FindFirstChild`, etc.)
- Imports
- Inheritance chains 🤮
- Append ` = null` to generated nullable properties
- Copy `RuntimeLib.lua` into `<project-dir>/include` on compile
- Default values (via `default` keyword)
- Macro `IEnumerable<T>` (or just `List<T>`) methods
- Classes/structs/interfaces nested in classes/structs/interfaces
- Polyfills:
	- The rest of the `Math` library

# Will maybe be supported
- `partial` keyword

# Will not be supported
- `using name = value` expressions (equivalent to `:=` operator in other languages)
- `volatile` and `unsafe` keywords