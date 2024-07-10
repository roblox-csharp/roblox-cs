namespace RobloxRuntime
{
    using HttpHeaders = IDictionary<string, string>;

    namespace Classes
    {
        public partial interface Instance
        {
            public static T Create<T>() where T : Instance
            {
                return (T)(object)null!;
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
    
    public sealed class Vector2int16(short x = 0, short y = 0)
    {
        public readonly short X = x;
        public readonly short Y = y;

        public static Vector2int16 operator -(Vector2int16 a, Vector2int16 b)
        {
            return null!;
        }

        public static Vector2int16 operator *(Vector2int16 a, Vector2int16 b)
        {
            return null!;
        }

        public static Vector2int16 operator /(Vector2int16 a, Vector2int16 b)
        {
            return null!;
        }

        public static Vector2int16 operator *(Vector2int16 a, float b)
        {
            return null!;
        }

        public static Vector2int16 operator /(Vector2int16 a, float b)
        {
            return null!;
        }
    }

    public sealed class Vector2(float x = 0, float y = 0)
    {
        public static readonly Vector2 zero = null!;
        public static readonly Vector2 one = null!;
        public static readonly Vector2 xAxis = null!;
        public static readonly Vector2 yAxis = null!;

        public readonly float X = x;
        public readonly float Y = y;
        public readonly float Magnitude;
        public readonly Vector2 Unit = null!;

        public Vector2 Abs()
        {
            return null!;
        }
        public Vector2 Ceil()
        {
            return null!;
        }

        public Vector2 Floor()
        {
            return null!;
        }

        public Vector2 Sign()
        {
            return null!;
        }
        public Vector2 Cross(Vector2 other)
        {
            return null!;
        }

        public float Angle(Vector2 other, bool isSigned)
        {
            return default;
        }

        public float Dot(Vector2 other)
        {
            return default;
        }

        public bool FuzzyEq(Vector2 other, double? epsilon)
        {
            return default;
        }

        public Vector2 Lerp(Vector2 goal, float alpha)
        {
            return null!;
        }

        public Vector2 Max(Vector2 vector)
        {
            return null!;
        }

        public Vector2 Min(Vector2 vector)
        {
            return null!;
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return null!;
        }

        public static Vector2 operator *(Vector2 a, Vector2 b)
        {
            return null!;
        }

        public static Vector2 operator /(Vector2 a, Vector2 b)
        {
            return null!;
        }

        public static Vector2 operator *(Vector2 a, float b)
        {
            return null!;
        }

        public static Vector2 operator /(Vector2 a, float b)
        {
            return null!;
        }
    }
    
    public sealed class Vector3int16(short x = 0, short y = 0, short z = 0)
    {
        public readonly short X = x;
        public readonly short Y = y;
        public readonly short Z = z;

        public static Vector3int16 operator +(Vector3int16 a, Vector3int16 b)
        {
            return null!;
        }

        public static Vector3int16 operator -(Vector3int16 a, Vector3int16 b)
        {
            return null!;
        }

        public static Vector3int16 operator *(Vector3int16 a, Vector3int16 b)
        {
            return null!;
        }

        public static Vector3int16 operator /(Vector3int16 a, Vector3int16 b)
        {
            return null!;
        }

        public static Vector3int16 operator *(Vector3int16 a, float b)
        {
            return null!;
        }

        public static Vector3int16 operator /(Vector3int16 a, float b)
        {
            return null!;
        }
    }

    public sealed class Vector3(float x = 0, float y = 0, float z = 0)
    {
        public static readonly Vector3 zero = null!;
        public static readonly Vector3 one = null!;
        public static readonly Vector3 xAxis = null!;
        public static readonly Vector3 yAxis = null!;
        public static readonly Vector3 zAxis = null!;

        public readonly float X = x;
        public readonly float Y = y;
        public readonly float Z = z;
        public readonly float Magnitude;
        public readonly Vector3 Unit = null!;

        public static Vector3 FromNormalId(Enum.NormalId normal)
        {
            return null!;
        }
        public static Vector3 FromAxis(Enum.Axis axis)
        {
            return null!;
        }

        public Vector3 Abs()
        {
            return null!;
        }
        public Vector3 Ceil()
        {
            return null!;
        }

        public Vector3 Floor()
        {
            return null!;
        }

        public Vector3 Sign()
        {
            return null!;
        }
        public Vector3 Cross(Vector3 other)
        {
            return null!;
        }

        public float Angle(Vector3 other, Vector3 axis)
        {
            return default;
        }

        public float Dot(Vector3 other)
        {
            return default;
        }

        public bool FuzzyEq(Vector3 other, double? epsilon)
        {
            return default;
        }

        public Vector3 Lerp(Vector3 goal, float alpha)
        {
            return null!;
        }

        public Vector3 Max(Vector3 vector)
        {
            return null!;
        }

        public Vector3 Min(Vector3 vector)
        {
            return null!;
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return null!;
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return null!;
        }

        public static Vector3 operator *(Vector3 a, Vector3 b)
        {
            return null!;
        }

        public static Vector3 operator /(Vector3 a, Vector3 b)
        {
            return null!;
        }

        public static Vector3 operator *(Vector3 a, float b)
        {
            return null!;
        }

        public static Vector3 operator /(Vector3 a, float b)
        {
            return null!;
        }
    }

    public sealed class CFrame
    {
        public readonly CFrame identity = null!;
        public readonly Vector3 Position = null!;
        public readonly CFrame Rotation = null!;
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public readonly Vector3 LookVector = null!;
        public readonly Vector3 RightVector = null!;
        public readonly Vector3 UpVector = null!;
        public readonly Vector3 XVector = null!;
        public readonly Vector3 YVector = null!;
        public readonly Vector3 ZVector = null!;

        public static CFrame lookAt(Vector3 at, Vector3 lookAt, Vector3? up)
        {
            return null!;
        }

        public static CFrame lookAlong(Vector3 at, Vector3 direction, Vector3? up)
        {
            return null!;
        }

        public static CFrame fromEulerAngles(float x, float y, float z, Enum.RotationOrder? order)
        {
            return null!;
        }

        public static CFrame fromEulerAnglesXYZ(float x, float y, float z)
        {
            return null!;
        }

        public static CFrame fromEulerAnglesYXZ(float x, float y, float z)
        {
            return null!;
        }

        public static CFrame Angles(float x, float y, float z)
        {
            return null!;
        }

        public static CFrame fromOrientation(float x, float y, float z)
        {
            return null!;
        }

        public static CFrame fromAxisAngle(Vector3 vector, float rotation)
        {
            return null!;
        }

        public static CFrame fromMatrix(Vector3 pos, Vector3 vX, Vector3 vY, Vector3 vZ)
        {
            return null!;
        }

        public CFrame Inverse()
        {
            return null!;
        }

        public CFrame Lerp(CFrame goal, float alpha)
        {
            return null!;
        }

        public CFrame Orthonormalize()
        {
            return null!;
        }

        public CFrame ToWorldSpace(CFrame cf)
        {
            return null!;
        }

        public CFrame ToObjectSpace(CFrame cf)
        {
            return null!;
        }

        public CFrame PointToWorldSpace(Vector3 v3)
        {
            return null!;
        }

        public CFrame PointToObjectSpace(Vector3 v3)
        {
            return null!;
        }

        public CFrame VectorToWorldSpace(Vector3 v3)
        {
            return null!;
        }

        public CFrame VectorToObjectSpace(Vector3 v3)
        {
            return null!;
        }

        public (float, float, float, float, float, float, float, float, float, float, float, float) GetComponents()
        {
            return default;
        }

        public (float, float, float) ToEulerAngles(Enum.RotationOrder order)
        {
            return default;
        }

        public (float, float, float) ToEulerAnglesXYZ()
        {
            return default;
        }

        public (float, float, float) ToEulerAnglesYXZ()
        {
            return default;
        }
        public (float, float, float) ToOrientation()
        {
            return default;
        }

        public (Vector3, float) ToAxisAngle()
        {
            return default;
        }

        public bool FuzzyEq(CFrame other, double? epsilon)
        {
            return default;
        }

        public (float, float, float, float, float, float, float, float, float, float, float, float) components()
        {
            return default;
        }

        public static CFrame operator +(CFrame a, Vector3 b)
        {
            return null!;
        }

        public static CFrame operator -(CFrame a, Vector3 b)
        {
            return null!;
        }

        public static CFrame operator *(CFrame a, CFrame b)
        {
            return null!;
        }

        public static Vector3 operator *(CFrame a, Vector3 b)
        {
            return null!;
        }
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