using RobloxRuntime;
using RobloxRuntime.Classes;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            ComponentRunner.AttachTag<Lava>("Lava");
        }
    }

    public class Lava : GameComponent<Part>
    {
        public override void Start()
        {
            Instance.Touched.Connect(hit =>
            {
                var model = (Model?)hit.FindFirstAncestorOfClass("Model");
                var humanoid = (Humanoid?)model?.FindFirstChildOfClass("Humanoid");
                if (humanoid == null) return;

                humanoid.TakeDamage(10);
            });
        }

        public override void Update(double dt)
        {

        }

        public override void Destroy()
        {

        }
    }

    public static class ComponentRunner
    {
        public static void AttachTag<TComponent>(string tag) where TComponent : GameComponent, new()
        {
            var instances = Services.CollectionService.GetTagged(tag);
            foreach (var instance in instances)
            {
                var component = (new TComponent() as GameComponent<Instance>)!;
                component.Instance = instance;
                Run(component);
            }
        }

        public static void Run<TComponent>(TComponent component) where TComponent : GameComponent
        {
            component.Start();

            var updateEvent = GetUpdateEvent(component);
            updateEvent.Connect(component.Update);
        }

        private static ScriptSignal<double> GetUpdateEvent(GameComponent component)
        {
            if (component.UpdateMethod == "RenderStepped")
            {
                return Services.RunService.RenderStepped;
            }
            else if (component.UpdateMethod == "Heartbeat")
            {
                return Services.RunService.Heartbeat;
            }
            else
            {
                return Services.RunService.Heartbeat;
            }
        }
    }

    public abstract class GameComponent
    {
        public string UpdateMethod { get; set; } = Services.RunService.IsClient() ? "RenderStepped" : "Heartbeat";

        public abstract void Start();
        public abstract void Update(double dt);
        public abstract void Destroy();
    }

    public abstract class GameComponent<TInstance> : GameComponent where TInstance : Instance
    {
        public TInstance Instance { get; set; } = default!;
    }
}