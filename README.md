# roblox-cs

Roblox CSharp to Lua compiler

# To-do
- Unit tests
- Referencing the full path of values from imported libraries (i.e. `RobloxRuntime.Classes.Part` vs `Part` with `using RobloxRuntime.Classes`)
- Collection literals
- Tuple literals
- String interpolation
- `for`/`foreach`/`while` loops
- `using name = value` expressions (equivalent to `:=` operator in other languages)
- Safe navigation
- Ternary operator
- Property declarations
- Macro IEnumerable<T> (or just List<T>) methods
- Classes/structs/interfaces nested in classes
- Restrict `volatile` and `partial` usage (`partial` maybe supported in the future)
- Polyfills:
	- The rest of the `Math` library
