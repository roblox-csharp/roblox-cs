using RobloxRuntime;
using RobloxRuntime.Classes;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            ComponentRunner.AttachTag("Lava", instance => new LavaComponent((Part)instance));
        }
    }

    public class LavaComponent : GameComponent<Part>
    {
        public new Part Instance { get; set; }

        public LavaComponent(Part instance)
            : base(instance)
        {
            Instance = instance;
            Console.WriteLine($"lava component created with {instance}");
        }

        public override void Start()
        {
            Console.WriteLine("lava component started");
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
        public static void AttachTag<TComponent>(string tag, Func<Instance, TComponent> attachComponent) where TComponent : GameComponent
        {
            var attached = false;
            var instances = Services.CollectionService.GetTagged(tag);
            Services.CollectionService.TagAdded.Connect(tag =>
            {
                if (attached) return;
                var instance = Services.CollectionService.GetTagged(tag)[0];
                Console.WriteLine(instance);
                Run(attachComponent(instance));
                attached = true;
            });

            foreach (var instance in instances)
            {
                if (attached) continue;
                Run(attachComponent(instance));
                attached = true;
            }
        }

        public static void Run(GameComponent component)
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
        public TInstance Instance { get; }

        protected GameComponent(TInstance instance)
        { 
            Instance = instance;
        }
    }
}