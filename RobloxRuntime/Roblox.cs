namespace RobloxRuntime
{
    using HttpHeaders = IDictionary<string, string>;

    namespace Classes
    {
        public partial interface Instance
        {
            public static Instance Create(string name)
            {
                return null!;
            }

            public Instance Clone();
            public bool IsAncestorOf(Instance descendant);
            public bool IsDescendantOf(Instance ancestor);
            public object? GetAttribute(string attribute);
            public Dictionary<string, object> GetAttributes();
            public Instance[] GetDescendants();
            public string[] GetTags();
            public Instance WaitForChild(string name);
            public Instance? WaitForChild(string name, float timeout);
            public Instance clone();
            public bool isDescendantOf(Instance ancestor);
            public ScriptSignal<Instance, Instance> AncestryChanged { get; }
            public ScriptSignal<string> AttributeChanged { get; }
            public ScriptSignal<string> Changed { get; }
            public ScriptSignal<Instance> ChildAdded { get; }
            public ScriptSignal<Instance> ChildRemoved { get; }
            public ScriptSignal<Instance> DescendantAdded { get; }
            public ScriptSignal<Instance> DescendantRemoving { get; }
            public ScriptSignal Destroying { get; }
            public ScriptSignal<Instance> childAdded { get; }
        }
    }

    public interface EnumItem
    {
        public string Name { get; }
        public uint Value { get; }
        public string EnumType { get; }
    }

    public interface IScriptSignal<TAction>
    {
        public ScriptConnection Connect(TAction func);
        public ScriptConnection ConnectParellel(TAction func);
        public ScriptConnection Once(TAction func);
        public void Wait();
    }

    public interface ScriptSignal : IScriptSignal<Action>
    {
    }

    public interface ScriptSignal<T> : IScriptSignal<Action<T>>
    {
        public new T Wait();
    }

    public interface ScriptSignal<T1, T2> : IScriptSignal<Action<T1, T2>>
    {
        public new (T1, T2) Wait();
    }

    public interface ScriptSignal<T1, T2, T3> : IScriptSignal<Action<T1, T2, T3>>
    {
        public new (T1, T2, T3) Wait();
    }

    public interface ScriptSignal<T1, T2, T3, T4> : IScriptSignal<Action<T1, T2, T3, T4>>
    {
        public new (T1, T2, T3, T4) Wait();
    }

    public interface ScriptSignal<T1, T2, T3, T4, T5> : IScriptSignal<Action<T1, T2, T3, T4, T5>>
    {
        public new (T1, T2, T2, T3, T4, T5) Wait();
    }

    public interface ScriptSignal<T1, T2, T3, T4, T5, T6> : IScriptSignal<Action<T1, T2, T3, T4, T5, T6>>
    {
        public new (T1, T2, T2, T3, T4, T5, T6) Wait();
    }

    public interface ScriptSignal<T1, T2, T3, T4, T5, T6, T7> : IScriptSignal<Action<T1, T2, T3, T4, T5, T6, T7>>
    {
        public new (T1, T2, T3, T4, T5, T6, T7) Wait();
    }

    public interface ScriptSignal<T1, T2, T3, T4, T5, T6, T7, T8> : IScriptSignal<Action<T1, T2, T3, T4, T5, T6, T7, T8>>
    {
        public new (T1, T2, T3, T4, T5, T6, T7, T8) Wait();
    }

    public interface ScriptSignal<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IScriptSignal<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>>
    {
        public new (T1, T2, T3, T4, T5, T6, T7, T8, T9) Wait();
    }

    public interface ScriptSignal<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IScriptSignal<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>
    {
        public new (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) Wait();
    }

    public interface ScriptConnection
    {
        public bool Connected { get; set; }

        public void Disconnect();
    }

    public interface EmoteDictionary : IDictionary<string, int[]>
    {
    }

    public interface EquippedEmote
    {
        public enum EmoteSlot
        {
            Slot1 = 1,
            Slot2,
            Slot3,
            Slot4,
            Slot5,
            Slot6,
            Slot7,
            Slot8
        }

        public string Name { get; set; }
        public EmoteSlot Slot { get; set; }
    }

    public interface UserInfo
    {
        public uint Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
    }

    public interface GroupInfo
    {
        interface GroupOwner
        {
            public uint Id { get; set; }
            public string Name { get; set; }
        }

        interface GroupRole
        {
            public string Name { get; set; }
            public byte Rank { get; set; }
        }

        public uint Id { get; set; }
        public string Name { get; set; }
        public GroupOwner Owner { get; set; }
        public string EmblemUrl { get; set; }
        public string Description { get; set; }
        public GroupRole[] Roles { get; set; }
    }

    public interface GetGroupsAsyncResult
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string EmblemUrl { get; set; }
        public string Description { get; set; }
        public byte Rank { get; set; }
	    public string Role { get; set; }
	    public bool IsPrimary { get; set; }
        public bool IsInClan { get; set; }
    }

    public interface RequestAsyncRequest
    {
        public string Url { get; set; }
        public string? Method { get; set; }
        public string? Body { get; set; }
        public HttpHeaders? Headers { get; set; }
        // public Enum.HttpCompression? Compress { get; set; }
    }
}