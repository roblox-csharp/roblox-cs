# roblox-cs

![Tests](https://github.com/R-unic/roblox-cs/actions/workflows/tests.yml/badge.svg)  
Roblox CSharp to Lua compiler

# To-do
- Imports
- Default values (via `default` keyword)
- Inheritance chains 🤮
- Classes outside of namespaces
- Macro `IEnumerable<T>` (or just `List<T>`) methods
- Classes/structs/interfaces nested in classes/structs/interfaces
- Restrict `volatile` and `partial` usage (`partial` maybe supported in the future)
- `using name = value` expressions (equivalent to `:=` operator in other languages) (MAYBE)
- Polyfills:
	- The rest of the `Math` library