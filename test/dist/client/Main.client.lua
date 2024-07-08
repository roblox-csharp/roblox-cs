local TestGame = {}
TestGame.Client = {} do
  TestGame.Client.Game = {} do
    local Game = TestGame.Client.Game

    function Game.Main()
      Console.WriteLine("hello world")
    end

    Game.Main()
  end
end
