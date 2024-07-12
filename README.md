# roblox-cs

![Tests](https://github.com/R-unic/roblox-cs/actions/workflows/tests.yml/badge.svg)  
Roblox CSharp to Lua compiler

# To-do
- Unit tests
- Referencing the full path of values from imported libraries (i.e. `RobloxRuntime.Classes.Part` vs `Part` with `using RobloxRuntime.Classes`)
- Tuple literals (create a table and macro `obj.ItemN` to `obj[N]`)
- Imports
- Inheritance chains 🤮
- `for`/`while` loops
- `using name = value` expressions (equivalent to `:=` operator in other languages)
- Property declarations
- Macro `IEnumerable<T>` (or just `List<T>`) methods
- Classes/structs/interfaces nested in classes/structs/interfaces
- Restrict `volatile` and `partial` usage (`partial` maybe supported in the future)
- Polyfills:
	- The rest of the `Math` library
