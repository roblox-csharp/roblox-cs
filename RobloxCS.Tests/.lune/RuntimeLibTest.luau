local CS = require("../../RobloxCS/Include/RuntimeLib")

-- CS.is()
assert(true, CS.is("abc", "string"))
assert(true, CS.is(123, "number"))
assert(true, CS.is(false, "boolean"))
assert(true, CS.is(nil, "nil"))
assert(true, CS.is(function() end, "function"))

local MyClass: CS.Class = { __className = "MyClass" }
local myClass = { __className = "MyClass" }
assert(true, CS.is(myClass, MyClass))

local parent = {}
parent.__index = parent
local child = setmetatable({}, parent)
assert(true, CS.is(child, parent))

-- CS.defineGlobal() / CS.getGlobal()
CS.defineGlobal("MyValue", 69420)
assert(69420, CS.getGlobal("MyValue"))

