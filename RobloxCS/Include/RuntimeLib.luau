--!strict
--!native
local CS = {}

type GlobalNamespace = { [string]: NamespaceMember }
type IEnum<K, V> = { [K & string]: V & number }
type Enum = IEnum<string, number>

export type Namespace = { [string]: any }
type NamespaceMember = Namespace | Class | Enum

CS.globalNamespace = {} :: GlobalNamespace

function CS.defineGlobal(name: string, value: NamespaceMember): ()
	CS.globalNamespace[name] = value
end

function CS.getGlobal(name: string): NamespaceMember
	return CS.globalNamespace[name]
end

type IClass<T> = {
	__className: string; -- prob temporary
	new: T;
}

export type Class = IClass<() -> {}>

function CS.is(object: any, class: Class | string): boolean
	if typeof(class) == "table" and typeof(class.__className) == "string" then
		return typeof(object) == "table" and typeof(object.__className) == "string" and object.__className == class.__className
	end

	-- metatable check
	if typeof(object) == "table" then
		object = getmetatable(object)

		while object ~= nil do
			if object == class then
				return true
			end

			local mt = getmetatable(object) :: { __index: {} }
			object = if mt then mt.__index else nil
		end
	end

	if typeof(class) == "string" then
		return if typeof(object) == "Instance" then
			object:IsA(class)
			else
			typeof(object) == class
	end

	return false
end

-- Enumerator class
export type EnumeratorClass<T> = IClass<(items: { T }) -> IEnumerator<T>>;
export type IEnumerator<T> = EnumeratorClass<T> & {
	_index: number;
	read _isAdvanced: boolean;
	Current: T;
	MoveNext: (IEnumerator<T>) -> ();
}
type Enumerator = IEnumerator<unknown>

local Enumerator
do
	local className = "Enumerator"
	Enumerator = setmetatable({}, {
		__tostring = function(): string
			return className
		end
	}) :: Enumerator
	Enumerator.__index = Enumerator
	Enumerator.__className = className

	function Enumerator.new<T>(items: { T } | (() -> { () -> T })): IEnumerator<T>
		local isAdvanced = typeof(items) == "function"
		return (setmetatable({
			_items = if isAdvanced then items() else items,
			_isAdvanced = isAdvanced,
			_index = 0
		}, Enumerator) :: any) :: IEnumerator<T>
	end

	function Enumerator:MoveNext(): boolean
		self._index += 1
		local gotValue = self._index <= #self._items
		local item = if gotValue then self._items[self._index] else nil
		self.Current = if self._isAdvanced and typeof(item) == "function" then
				item(function()
					self._index = #self._items + 1
				end)
			else
				item
		return gotValue
	end

	function Enumerator:Reset(): ()
		self._index = 0
		self.Current = nil :: any
	end

	function Enumerator:Dispose(): ()
		self._items = nil
		setmetatable(self, nil)
	end
end
CS.defineGlobal("Enumerator", Enumerator)
CS.Enumerator = Enumerator

-- Exception class
type ExceptionClass = Class & {
	__index: ExceptionClass;
	new: (message: string?) -> Exception;
}
export type Exception = ExceptionClass & {
	Message: string;
	Throw: (Exception) -> ();
}

local Exception
do
	Exception = setmetatable({}, {
		__tostring = function(self: Exception): string
			return `{self.__className}: {self.Message}`
		end
	}) :: Exception
	Exception.__index = Exception
	Exception.__className = "Exception"

	function Exception.new(message: string?): Exception
		message = message or "An error occurred"

		return (setmetatable({ Message = message }, Exception) :: any) :: Exception
	end

	function Exception:Throw(): ()
		error(self)
	end
end
CS.defineGlobal("Exception", Exception)
CS.Exception = Exception

type CatchBlock = {
	exceptionClass: string;
	block: (ex: Exception?, rethrow: () -> ()) -> ();
}

function CS.try(block: () -> nil, finallyBlock: (() -> ())?, catchBlocks: { CatchBlock }): ()
	local success: boolean, result: Exception | string | nil = pcall(block)
	if not success then
		if typeof(result) == "string" then
			result = if Exception == nil --[[ never ]] then nil :: any else Exception.new(result)
		end
		for _, catchBlock in catchBlocks do
			if catchBlock.exceptionClass ~= result.__className then continue end
			catchBlock.block(result :: Exception, result.Throw)
		end
	end
	if finallyBlock ~= nil then
		finallyBlock()
	end
end

return CS