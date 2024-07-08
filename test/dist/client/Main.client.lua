local TestGame = {}
TestGame.Client = {} do
  TestGame.Client.Program = {} do
    local Program = TestGame.Client.Program
    Program.__index = Program

    function Program.new()
      local self = setmetatable({}, Program)
      self._abc = 5
      return self
    end
  end
end