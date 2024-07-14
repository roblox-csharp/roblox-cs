# roblox-cs

![Tests](https://github.com/R-unic/roblox-cs/actions/workflows/tests.yml/badge.svg)  
Roblox CSharp to Lua compiler

# Contributing
1. [Fork it](https://github.com/R-unic/roblox-cs/fork)
2. Commit your changes (`git commit -m 'feat: add some feature'`)
3. Test your code (`dotnet test` or Ctrl + R, A in Visual Studio)
4. Push your changes (`git push`)
5. Create a pull request

# To-do
- Macro `ServiceProvider.GetService<T>()` to `ServiceProvider:GetService("T")`
- Imports
- Inheritance chains 🤮
- Copy `RuntimeLib.lua` into `<project-dir>/include` on compile
- Default values (via `default` keyword)
- Macro `IEnumerable<T>` methods
- Classes/structs/interfaces nested in classes/structs/interfaces
- Macro operator methods into metamethods
- Polyfills:
	- The rest of the `Math` library

# Will maybe be supported
- `partial` keyword

# Will not be supported
- `using name = value` expressions (equivalent to `:=` operator in other languages)
- `volatile` and `unsafe` keywords