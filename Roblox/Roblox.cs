namespace Roblox
{
    using HttpHeaders = IDictionary<string, string>;

    public static partial class Globals
    {
        public static DataModel game { get; } = null!;
        public static LuaSourceContainer script { get; } = null!;
        public static PluginClasses.Plugin plugin { get; } = null!;

        public static void warn(params string[] messages)
        {
        }

        public static void error(string message, int? level = null)
        {
        }

        public static object require(ModuleScript module)
        {
            return null!;
        }

        public static float tick()
        {
            return default;
        }

        public static float ToNumber(string str)
        {
            return default;
        }

        public static float ToFloat(string str)
        {
            return default;
        }

        public static double ToDouble(string str)
        {
            return default;
        }

        public static int ToInt(string str)
        {
            return default;
        }

        public static uint ToUInt(string str)
        {
            return default;
        }

        public static short ToShort(string str)
        {
            return default;
        }

        public static ushort ToUShort(string str)
        {
            return default;
        }

        public static byte ToByte(string str)
        {
            return default;
        }

        public static sbyte ToSByte(string str)
        {
            return default;
        }

        public static string TypeOf(object obj)
        {
            return null!;
        }

        public static IEnumerable<(TKey, TValue)> pairs<TKey, TValue>(Dictionary<TKey, TValue> table) where TKey : notnull
        {
            return null!;
        }

        public static object getmetatable(object obj)
        {
            return null!;
        }

        public static void setmetatable(object obj, object meta)
        {
        }

        public static object rawget(object obj, object index)
        {
            return null!;
        }

        public static object rawset(object obj, object index, object value)
        {
            return null!;
        }

        public static uint rawlen(object obj)
        {
            return default;
        }

        public static bool raweq(object a, object b)
        {
            return default;
        }

        public static void assert(object obj, string? errorMessage = null)
        { 
        }

        public static object newproxy(bool addMetatable)
        {
            return null!;
        }

        public static object loadstring(string str, string? chunkName = null)
        {
            return null!;
        }

        public static string version()
        {
            return null!;
        }

        public static UserSettings UserSettings()
        {
            return null!;
        }

        public static PluginClasses.GlobalSettings settings()
        {
            return null!;
        }

        public static uint gcinfo()
        {
            return default;
        }
    }

    public interface ClipEvaluator : Instance
    {
    }

    public interface SystemAddress : Instance
    {
    }

    public interface OpenCloudModel : Instance
    {
    }

    public interface HSRDataContentProvider : Instance
    {
    }

    public interface MeshContentProvider : Instance
    {
    }

    public interface SolidModelContentProvider : Instance
    {
    }

    public interface CSGDictionaryService : Instance
    {
    }

    public interface NonReplicatedCSGDictionaryService : Instance
    {
    }

    public interface AppStorageService : Instance
    {
    }

    public interface UserStorageService : Instance
    {
    }

    public partial interface NetworkPeer : Instance
    {
        public void SetOutgoingKBPSLimit(int limit);
    }

    public partial interface NetworkClient : NetworkPeer
    {
    }

    public partial interface NetworkServer : NetworkPeer
    {
    }

    public partial interface Players
    {
        public Player LocalPlayer { get; }
    }

    public partial interface BasePart
    {
        public Vector3 Position { get; set; }
        public Vector3 Orientation { get; set; }
    }

    public partial interface DataModel : Instance
    {
        public Workspace Workspace { get; }
        public Lighting Lighting { get; }
        public T GetService<T>() where T : IServiceInstance;
    }

    public interface IServiceInstance : Instance
    {
    }

    public interface ICreatableInstance : Instance
    {
    }

    public partial interface Instance
    {
        public static sealed T Create<T>(Instance? parent = null) where T : ICreatableInstance
        {
            return default!;
        }

        public Instance Clone();
        public bool IsA<T>() where T : Instance;
        public bool IsAncestorOf(Instance descendant);
        public bool IsDescendantOf(Instance ancestor);
        public object? GetAttribute(string attribute);
        public Dictionary<string, object> GetAttributes();
        public Instance[] GetDescendants();
        public T? FindFirstAncestorOfClass<T>() where T : Instance;
        public T? FindFirstAncestorWhichIsA<T>() where T : Instance;
        public T? FindFirstChildOfClass<T>() where T : Instance;
        public T? FindFirstChildWhichIsA<T>(bool? recursive) where T : Instance;
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

    /// <summary>The <see cref="EnumItem"/> data type represents an individual item in a Roblox enum.</summary>
    public interface EnumItem
    {
        /// <summary>The name of the <see cref="EnumItem"/>.</summary>
        public string Name { get; }
        /// <summary>The integral value assigned to the <see cref="EnumItem"/>.</summary>
        public uint Value { get; }
        /// <summary>A reference to the parent <see cref="IEnum">Enum</see> of the <see cref="EnumItem"/>.</summary>
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

    /// <summary>
    /// <para>The <see cref="Vector2int16"/> data type represents a vector in 2D space with a signed 16-bit integer for its components.</para>
    /// <para>It is similar to <see cref="Vector2"/> in that it allows for the same arithmetic operations, but it lacks commonly used vector functions.</para>
    /// <listheader><see cref="Vector2int16"/> should not be confused with:</listheader>
    /// <list type="bullet"><see cref="Vector2"/>, a <b>more precise</b> and complete implementation for 2D vectors.</list>
    /// <list type="bullet"><see cref="Vector3int16"/>, a similar implementation for 3D vectors.</list>
    /// <listheader>For each component:</listheader>
    /// <list type="bullet">The <b>lower</b> bound is -2¹⁵, or <b>-32,768</b></list>
    /// <list type="bullet">The <b>upper</b> bound is 2¹⁵ - 1, or <b>32,767</b></list>
    /// </summary>
    /// 
    /// <summary>Returns a <see cref="Vector2int16"/> from the given x and y components.</summary>
    /// <param name="x">The x-coordinate of the <see cref="Vector2int16"/>.</param>
    /// <param name="y">The y-coordinate of the <see cref="Vector2int16"/>.</param>
    public sealed class Vector2int16(short x = 0, short y = 0)
    {
        /// <summary>The x-coordinate of the <see cref="Vector2int16"/>.</summary>
        public readonly short X = x;
        /// <summary>The y-coordinate of the <see cref="Vector2int16"/>.</summary>
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
        /// <summary>A <see cref="Vector2"/> with a magnitude of zero.</summary>
        public static readonly Vector2 zero = null!;
        /// <summary>A <see cref="Vector2"/> with a value of 1 on every axis.</summary>
        public static readonly Vector2 one = null!;
        /// <summary>A <see cref="Vector2"/> with a value of 1 on the X axis.</summary>
        public static readonly Vector2 xAxis = null!;
        /// <summary>A <see cref="Vector2"/> with a value of 1 on the Y axis.</summary>
        public static readonly Vector2 yAxis = null!;

        /// <summary>The x-coordinate of the <see cref="Vector2"/></summary>
        public readonly float X = x;
        /// <summary>The y-coordinate of the <see cref="Vector2"/></summary>
        public readonly float Y = y;
        /// <summary>The length of the <see cref="Vector2"/></summary>
        public readonly float Magnitude;
        /// <summary>A normalized copy of the <see cref="Vector2"/> - one that has the same direction as the original but a magnitude of 1.</summary>
        public readonly Vector2 Unit = null!;

        /// <summary>Returns a new vector from the absolute values of the original's components. For example, a vector of (-2, 4, -6) returns a vector of (2, 4, 6).</summary>
        public Vector2 Abs()
        {
            return null!;
        }

        /// <summary>Returns a new vector from the ceiling of the original's components. For example, a vector of (-2.6, 5.1, 8.8) returns a vector of (-2, 6, 9).</summary>
        public Vector2 Ceil()
        {
            return null!;
        }

        /// <summary>Returns a new vector from the floor of the original's components. For example, a vector of (-2.6, 5.1, 8.8) returns a vector of (-3, 5, 8).</summary>
        public Vector2 Floor()
        {
            return null!;
        }

        /// <summary>Returns a new vector from the sign (-1, 0, or 1) of the original's components. For example, a vector of (-2.6, 5.1, 0) returns a vector of (-1, 1, 0).</summary>
        public Vector2 Sign()
        {
            return null!;
        }

        /// <summary>Returns the cross product of the two vectors.</summary>
        public Vector2 Cross(Vector2 other)
        {
            return null!;
        }

        /// <summary>Returns the angle in radians between the two vectors. If you provide an axis, it determines the sign of the angle.</summary>
        public float Angle(Vector2 other, bool isSigned)
        {
            return default;
        }

        /// <summary>Returns a scalar dot product of the two vectors.</summary>
        public float Dot(Vector2 other)
        {
            return default;
        }

        /// <summary>Returns true if the X, Y, and Z components of the other <see cref="Vector2"/> are within epsilon units of each corresponding component of this <see cref="Vector2"/>.</summary>
        public bool FuzzyEq(Vector2 other, float? epsilon)
        {
            return default;
        }

        /// <summary>
        /// <para>Returns a <see cref="Vector2"/> linearly interpolated between this Vector3 and the given goal <see cref="Vector2"/> by the fraction alpha.</para>
        /// <para>Note: the alpha value is not limited to the range [0, 1].</para>
        /// </summary>
        public Vector2 Lerp(Vector2 goal, float alpha)
        {
            return null!;
        }

        /// <summary>Returns a <see cref="Vector2"/> with each component as the highest among the respective components of both provided <see cref="Vector2"/> objects.</summary>
        public Vector2 Max(Vector2 vector)
        {
            return null!;
        }

        /// <summary>Returns a <see cref="Vector2"/> with each component as the lowest among the respective components of both provided <see cref="Vector2"/> objects.</summary>
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

    /// <summary>
    /// <para>The <see cref="Vector3int16"/> data type represents a vector in 3D space with a signed 16-bit integer for its components.</para>
    /// <para>It is similar to <see cref="Vector3"/> in that it allows for the same arithmetic operations, but it lacks commonly used vector functions.</para>
    /// <listheader><see cref="Vector3int16"/> should not be confused with:</listheader>
    /// <list type="bullet"><see cref="Vector3"/>, a <b>more precise</b> and complete implementation for 3D vectors.</list>
    /// <list type="bullet"><see cref="Vector2int16"/>, a similar implementation for 2D vectors.</list>
    /// <listheader>For each component:</listheader>
    /// <list type="bullet">The <b>lower</b> bound is -2¹⁵, or <b>-32,768</b></list>
    /// <list type="bullet">The <b>upper</b> bound is 2¹⁵ - 1, or <b>32,767</b></list>
    /// </summary>
    /// 
    /// <summary>Returns a <see cref="Vector3int16"/> from the given x and y components.</summary>
    /// <param name="x">The x-coordinate of the <see cref="Vector3int16"/>.</param>
    /// <param name="y">The y-coordinate of the <see cref="Vector3int16"/>.</param>
    /// <param name="z">The z-coordinate of the <see cref="Vector3int16"/>.</param>
    public sealed class Vector3int16(short x = 0, short y = 0, short z = 0)
    {
        /// <summary>The x-coordinate of the <see cref="Vector3int16"/>.</summary>
        public readonly short X = x;
        /// <summary>The y-coordinate of the <see cref="Vector3int16"/>.</summary>
        public readonly short Y = y;
        /// <summary>The z-coordinate of the <see cref="Vector3int16"/>.</summary>
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

    /// <summary>
    /// <para>The <see cref="Vector3"/> data type represents a vector in 3D space, typically usually used as a point in 3D space or the dimensions of a rectangular prism.</para>
    /// <para><see cref="Vector3"/> supports basic component-based arithmetic operations (sum, difference, product, and quotient) and these operations can be applied on the left or right hand side to either another <see cref="Vector3"/> or a number.</para>
    /// <para>It also features methods for common vector operations, such as Cross() and Dot().</para>
    /// </summary>
    /// 
    /// Returns a new <see cref="Vector3"/> from the given x, y, and z components.
    /// <param name="x">The x-coordinate of the <see cref="Vector3"/>.</param>
    /// <param name="y">The y-coordinate of the <see cref="Vector3"/>.</param>
    /// <param name="z">The z-coordinate of the <see cref="Vector3"/>.</param>
    public sealed class Vector3(float x = 0, float y = 0, float z = 0)
    {
        /// <summary>A <see cref="Vector3"/> with a magnitude of zero.</summary>
        public static readonly Vector3 zero = null!;
        /// <summary>A <see cref="Vector3"/> with a value of 1 on every axis.</summary>
        public static readonly Vector3 one = null!;
        /// <summary>A <see cref="Vector3"/> with a value of 1 on the X axis.</summary>
        public static readonly Vector3 xAxis = null!;
        /// <summary>A <see cref="Vector3"/> with a value of 1 on the Y axis.</summary>
        public static readonly Vector3 yAxis = null!;
        /// <summary>A <see cref="Vector3"/> with a value of 1 on the Z axis.</summary>
        public static readonly Vector3 zAxis = null!;

        /// <summary>The x-coordinate of the <see cref="Vector3"/>.</summary>
        public readonly float X = x;
        /// <summary>The y-coordinate of the <see cref="Vector3"/>.</summary>
        public readonly float Y = y;
        /// <summary>The z-coordinate of the <see cref="Vector3"/>.</summary>
        public readonly float Z = z;
        /// <summary>The length of the Vector3.</summary>
        public readonly float Magnitude;
        /// <summary>A normalized copy of the <see cref="Vector3"/> - one that has the same direction as the original but a magnitude of 1.</summary>
        public readonly Vector3 Unit = null!;

        /// <summary>Returns a new <see cref="Vector3"/> in the given direction.</summary>
        public static Vector3 FromNormalId(Enum.NormalId normal)
        {
            return null!;
        }

        /// <summary>Returns a new <see cref="Vector3"/> for the given axis.</summary>
        public static Vector3 FromAxis(Enum.Axis axis)
        {
            return null!;
        }

        /// <summary>Returns a new vector from the absolute values of the original's components.</summary>
        public Vector3 Abs()
        {
            return null!;
        }

        /// <summary>Returns a new vector from the ceiling of the original's components.</summary>
        public Vector3 Ceil()
        {
            return null!;
        }

        /// <summary>Returns a new vector from the floor of the original's components.</summary>
        public Vector3 Floor()
        {
            return null!;
        }

        /// <summary>Returns a new vector from the sign (-1, 0, or 1) of the original's components.</summary>
        public Vector3 Sign()
        {
            return null!;
        }

        /// <summary>Returns the cross product of the two vectors.</summary>
        public Vector3 Cross(Vector3 other)
        {
            return null!;
        }

        /// <summary>Returns the angle in radians between the two vectors. If you provide an axis, it determines the sign of the angle.</summary>
        public float Angle(Vector3 other, Vector3? axis)
        {
            return default;
        }

        /// <summary>Returns a scalar dot product of the two vectors.</summary>
        public float Dot(Vector3 other)
        {
            return default;
        }

        /// <summary>Returns true if the X, Y, and Z components of the other <see cref="Vector3"/> are within epsilon units of each corresponding component of this <see cref="Vector3"/>.</summary>
        public bool FuzzyEq(Vector3 other, float? epsilon)
        {
            return default;
        }

        /// <summary>
        /// <para>Returns a <see cref="Vector2"/> linearly interpolated between this Vector3 and the given goal <see cref="Vector2"/> by the fraction alpha.</para>
        /// <para>Note: the alpha value is not limited to the range [0, 1].</para>
        /// </summary>
        public Vector3 Lerp(Vector3 goal, float alpha)
        {
            return null!;
        }

        /// <summary>Returns a <see cref="Vector3"/> with each component as the highest among the respective components of both provided <see cref="Vector3"/> objects.</summary>
        public Vector3 Max(Vector3 vector)
        {
            return null!;
        }

        /// <summary>Returns a <see cref="Vector3"/> with each component as the lowest among the respective components of both provided <see cref="Vector3"/> objects.</summary>
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

    /// <summary>
    /// <para>The <see cref="CFrame"/> data type, short for <b>coordinate frame</b>, describes a 3D position and orientation.</para>
    /// <para>It is made up of a <b>positional</b> component and a <b>rotational</b> component and includes essential arithmetic operations for working with 3D data on Roblox.</para>
    /// </summary>
    public sealed class CFrame
    {
        /// <summary>An identity <see cref="CFrame"/> with no translation or rotation.</summary>
        public readonly CFrame identity = null!;
        /// <summary>The 3D position of the <see cref="CFrame"/>.</summary>
        public readonly Vector3 Position = null!;
        /// <summary>A copy of the <see cref="CFrame"/> with no translation.</summary>
        public readonly CFrame Rotation = null!;
        /// <summary>The X coordinate of the position.</summary>
        public readonly float X;
        /// <summary>The Y coordinate of the position.</summary>
        public readonly float Y;
        /// <summary>The Z coordinate of the position.</summary>
        public readonly float Z;
        /// <summary>The forward-direction component of the <see cref="CFrame"/> object's orientation, equivalent to the negated <see cref="ZVector"/> or the negated third column of the rotation matrix.</summary>
        public readonly Vector3 LookVector = null!;
        /// <summary>The right-direction component of the <see cref="CFrame"/> object's orientation. Equivalent to <see cref="XVector"/> or the first column of the rotation matrix.</summary>
        public readonly Vector3 RightVector = null!;
        /// <summary>The up-direction component of the <see cref="CFrame"/> object's orientation. Equivalent to <see cref="YVector"/> or the second column of the rotation matrix.</summary>
        public readonly Vector3 UpVector = null!;
        /// <summary>The X component of the <see cref="CFrame"/> object's orientation. Equivalent to <see cref="RightVector"/> or the first column of the rotation matrix.</summary>
        public readonly Vector3 XVector = null!;
        /// <summary>The Y component of the <see cref="CFrame"/> object's orientation. Equivalent to <see cref="UpVector"/> or the second column of the rotation matrix.</summary>
        public readonly Vector3 YVector = null!;
        /// <summary>The Z component of the <see cref="CFrame"/> object's orientation. Equivalent to the negated <see cref="LookVector"/> or the third column of the rotation matrix.</summary>
        public readonly Vector3 ZVector = null!;

        /// <summary>Returns a <see cref="CFrame"/> with no rotation with the position of the provided <see cref="Vector3"/>.</summary>
        public CFrame(Vector3 pos)
        { 
        }

        /// <summary>
        /// <para>Returns a new <see cref="CFrame"/> located at pos and facing towards lookAt, assuming that (0, 1, 0) is considered "up" in world space.</para>
        /// <para>This constructor overload has been replaced by <see cref="lookAt(Vector3, Vector3, Vector3?)"/>, which accomplishes a similar goal. It remains for the sake of backward compatibility.</para>
        /// <para>At high pitch angles (around 82 degrees), you may experience numerical instability. If this is an issue, or if you require a different "up" vector, use <see cref="fromMatrix(Vector3, Vector3, Vector3, Vector3)"/> to more accurately construct the <see cref="CFrame"/>. Additionally, if lookAt is directly above pos (pitch angle of 90 degrees), the "up" vector switches to the X axis.</para>
        /// </summary>
        public CFrame(Vector3 pos, Vector3 lookAt)
        {
        }

        /// <summary>Returns a <see cref="CFrame"/> with a position comprised of the provided x, y, and z components.</summary>
        public CFrame(float x, float y, float z)
        {
        }

        /// <summary>Returns a <see cref="CFrame"/> from position (x, y, z) and quaternion (qX, qY, qZ, qW). The quaternion is expected to be of unit length to represent a valid rotation. If this isn't the case, the quaternion will be normalized.</summary>
        public CFrame(float x, float y, float z, float qX, float qY, float qZ, float qW)
        {
        }

        /// <summary>
        /// Creates a <see cref="CFrame"/> from position (x, y, z) with an orientation specified by the rotation matrix.
        /// <code>[[R00 R01 R02] [R10 R11 R12] [R20 R21 R22]]</code>
        /// </summary>
        public CFrame(float x, float y, float z, float r00, float r01, float r02, float r10, float r11, float r12, float r20, float r21, float r22)
        {
        }

        /// <summary>Returns a new <see cref="CFrame"/> with the position of at and facing towards lookAt, optionally specifying the upward direction (up) with a default of (0, 1, 0).</summary>
        public static CFrame lookAt(Vector3 at, Vector3 lookAt, Vector3? up)
        {
            return null!;
        }

        /// <summary>
        /// <para>Returns a new <see cref="CFrame"/> with the position of at and facing along direction, optionally specifying the upward direction (up) with a default of (0, 1, 0).</para>
        /// <para>This constructor is equivalent to CFrame.lookAt(at, at + direction).</para>
        /// </summary>
        public static CFrame lookAlong(Vector3 at, Vector3 direction, Vector3? up)
        {
            return null!;
        }

        /// <summary>Returns a rotated <see cref="CFrame"/> from angles rx, ry, and rz in radians. Rotations are applied in the optional <see cref="Enum.RotationOrder"/> with a default of XYZ.</summary>
        public static CFrame fromEulerAngles(float x, float y, float z, Enum.RotationOrder? order)
        {
            return null!;
        }

        /// <summary>Returns a rotated <see cref="CFrame"/> from angles rx, ry, and rz in radians using <see cref="Enum.RotationOrder.XYZ"/>.</summary>
        public static CFrame fromEulerAnglesXYZ(float x, float y, float z)
        {
            return null!;
        }

        /// <summary>Returns a rotated <see cref="CFrame"/> from angles rx, ry, and rz in radians using <see cref="Enum.RotationOrder.YXZ"/>.</summary>
        public static CFrame fromEulerAnglesYXZ(float x, float y, float z)
        {
            return null!;
        }

        /// <summary>Equivalent to <see cref="fromEulerAnglesXYZ(float, float, float)"/>.</summary>
        public static CFrame Angles(float x, float y, float z)
        {
            return null!;
        }

        /// <summary>Equivalent to <see cref="fromEulerAnglesYXZ(float, float, float)"/>.</summary>
        public static CFrame fromOrientation(float x, float y, float z)
        {
            return null!;
        }

        /// <summary>Returns a rotated <see cref="CFrame"/> from a unit <see cref="Vector3"/> and a rotation in radians.</summary>
        public static CFrame fromAxisAngle(Vector3 vector, float rotation)
        {
            return null!;
        }

        /// <summary>Returns a <see cref="CFrame"/> from a translation and the columns of a rotation matrix. If vZ is excluded, the third column is calculated as vX:Cross(vY).Unit.</summary>
        public static CFrame fromMatrix(Vector3 pos, Vector3 vX, Vector3 vY, Vector3 vZ)
        {
            return null!;
        }

        /// <summary>Returns the inverse of the <see cref="CFrame"/>.</summary>
        public CFrame Inverse()
        {
            return null!;
        }

        /// <summary>Returns a <see cref="CFrame"/> interpolated between itself and goal by the fraction alpha.</summary>
        public CFrame Lerp(CFrame goal, float alpha)
        {
            return null!;
        }

        /// <summary>Returns an orthonormalized copy of the <see cref="CFrame"/>. The <see cref="BasePart.CFrame"/> property automatically applies orthonormalization, but other APIs which take <see cref="CFrame"/>s do not, so this method is occasionally necessary when incrementally updating a <see cref="CFrame"/> and using it with them.</summary>
        public CFrame Orthonormalize()
        {
            return null!;
        }

        /// <summary>
        /// Returns one or more <see cref="CFrame"/> objects transformed from object to world space. Equivalent to:
        /// <code>CFrame * cf</code>
        /// </summary>
        public CFrame ToWorldSpace(CFrame cf)
        {
            return null!;
        }

        /// <summary>
        /// Returns one or more <see cref="CFrame"/> objects transformed from world to object space. Equivalent to:
        /// <code>CFrame:Inverse() * cf</code>
        /// </summary>
        public CFrame ToObjectSpace(CFrame cf)
        {
            return null!;
        }

        /// <summary>
        /// Returns one or more <see cref="Vector3"/> objects transformed from object to world space. Equivalent to:
        /// <code>CFrame * v3</code>
        /// </summary>
        public CFrame PointToWorldSpace(Vector3 v3)
        {
            return null!;
        }

        /// <summary>
        /// Returns one or more <see cref="Vector3"/> objects transformed from world to object space. Equivalent to:
        /// <code>CFrame:Inverse() * v3</code>
        /// </summary>
        public CFrame PointToObjectSpace(Vector3 v3)
        {
            return null!;
        }

        /// <summary>
        /// Returns one or more <see cref="Vector3"/> objects rotated from object to world space. Equivalent to:
        /// <code>(CFrame - CFrame.Position) * v3</code>
        /// </summary>
        public CFrame VectorToWorldSpace(Vector3 v3)
        {
            return null!;
        }

        /// <summary>
        /// Returns one or more <see cref="Vector3"/> objects rotated from world to object space. Equivalent to:
        /// <code>(CFrame:Inverse() - CFrame:Inverse().Position) * v3</code>
        /// </summary>
        public CFrame VectorToObjectSpace(Vector3 v3)
        {
            return null!;
        }

        /// <summary>Returns the values x, y, z, R00, R01, R02, R10, R11, R12, R20, R21, and R22, where x y z represent the position of the <see cref="CFrame"/> and R00‑R22 represent its 3×3 rotation matrix.</summary>
        public (float, float, float, float, float, float, float, float, float, float, float, float) GetComponents()
        {
            return default;
        }

        /// <summary>Returns approximate angles that could be used to generate the <see cref="CFrame"/> using the optional <see cref="Enum.RotationOrder"/>. If you don't provide order, the method uses <see cref="Enum.RotationOrder.XYZ"/>.</summary>
        public (float, float, float) ToEulerAngles(Enum.RotationOrder order)
        {
            return default;
        }

        /// <summary>Returns approximate angles that could be used to generate the <see cref="CFrame"/> using <see cref="Enum.RotationOrder.XYZ"/>.</summary>
        public (float, float, float) ToEulerAnglesXYZ()
        {
            return default;
        }

        /// <summary>Returns approximate angles that could be used to generate the <see cref="CFrame"/> using <see cref="Enum.RotationOrder.YXZ"/>.</summary>
        public (float, float, float) ToEulerAnglesYXZ()
        {
            return default;
        }

        /// <summary>Equivalent to <see cref="ToEulerAnglesYXZ()"/>.</summary>
        public (float, float, float) ToOrientation()
        {
            return default;
        }

        /// <summary>Returns a tuple of a <see cref="Vector3"/> and a <see cref="float"/> which represent the rotation of the <see cref="CFrame"/> in the axis-angle representation.</summary>
        public (Vector3, float) ToAxisAngle()
        {
            return default;
        }

        /// <summary>
        /// <para>Returns true if the other <see cref="CFrame"/> is sufficiently close to this <see cref="CFrame"/> in both position and rotation.</para>
        /// <para>The epsilon value is used to control the tolerance for this similarity. This value is optional and should be a small positive value if provided.</para>
        /// <para>The similarity for position is component-wise, and for rotation uses a fast approximation of the angle difference.</para>
        /// </summary>
        public bool FuzzyEq(CFrame other, float? epsilon)
        {
            return default;
        }

        /// <summary>Equivalent to <see cref="GetComponents()"/>.</summary>
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

    /// <summary>
    /// <para>Not to be confused with <see cref="Region3"/>, a separate class that fulfills a different purpose.</para>
    /// <para>The <see cref="Region3int16"/> data type represents a volume in 3D space similar to an axis-aligned rectangular prism.</para>
    /// <para>It uses two <see cref="Vector3int16"/> to store the volume's bounds in the <see cref="Min"/> and <see cref="Max"/> properties. It is constructed using <see cref="Region3int16"/>.new(Min, Max), given the two <see cref="Vector3int16"/> bounds.</para>
    /// <para>This data type features no functions or operations.</para>
    /// </summary>
    public sealed class Region3int16
    {
        /// <summary>The lower bound of the <see cref="Region3int16"/>.</summary>
        public readonly Vector3int16 Min = null!;
        /// <summary>The upper bound of the <see cref="Region3int16"/>.</summary>
        public readonly Vector3int16 Max = null!;

        public Region3int16(Vector3int16 min, Vector3int16 max)
        { 
        }
    }

    /// <summary>
    /// <para>The <see cref="Region3"/> data type describes a volume in 3D space similar to an axis-aligned rectangular prism.</para>
    /// <para>It is commonly used with <see cref="Terrain"/> functions and functions that detect parts within a volume, such as <see cref="WorldRoot.FindPartsInRegion3(Region3, Instance?, int?)"/>.</para>
    /// <para>The prism's center is accessible using the <see cref="CFrame"/> property and the prism's size is accessible through the <see cref="Size"/> property. Note that the components of this property may be negative.</para>
    /// <para>The <see cref="ExpandToGrid()"/> method returns a new <see cref="Region3"/> whose bounds comply with a provided resolution value. The resulting volume may be equal to or greater than the original volume, but never smaller.</para>
    /// <listheader>See also:</listheader>
    /// <list type="bullet"><see cref="Region3int16"/></list>
    /// </summary>
    public sealed class Region3
    {
        /// <summary>The center location and rotation of the <see cref="Region3"/>.</summary>
        public readonly CFrame CFrame = null!;
        /// <summary>The 3D size of the <see cref="Region3"/>.</summary>
        public readonly Vector3 Size = null!;

        /// <summary>Returns a new <see cref="Region3"/> using the provided vectors as boundaries.</summary>
        public Region3(Vector3 min, Vector3 max)
        {
        }

        /// <summary>Expands the <see cref="Region3"/> based on the provided resolution and returns the expanded <see cref="Region3"/> aligned to the <see cref="Terrain"/> voxel grid.</summary>
        public Region3 ExpandToGrid(float resolution)
        {
            return null!;
        }
    }

    /// <summary>
    /// <para>The <see cref="Color3"/> data type describes a color using red, green, and blue components in the range of 0 to 1.</para>
    /// <para>Unlike the <see cref="BrickColor"/> data type which describes named colors, <see cref="Color3"/> is used for precise coloring of objects on screen through properties like <see cref="BasePart.Color"/> and <see cref="GuiObject.BackgroundColor3"/>.</para>
    /// </summary>
    public sealed class Color3()
    {
        /// <summary>The red value of the color.</summary>
        public readonly float R;
        /// <summary>The green value of the color.</summary>
        public readonly float G;
        /// <summary>The blue value of the color.</summary>
        public readonly float B;

        /// <summary>Returns a <see cref="Color3"/> with the given red, green, and blue values.</summary>
        public static Color3 fromRGB(float r = 0, float g = 0, float b = 0)
        {
            return null!;
        }

        /// <summary>Returns a <see cref="Color3"/> from given components within the range of 0 to 255.</summary>
        public static Color3 fromRGB(byte r = 0, byte g = 0, byte b = 0)
        {
            return null!;
        }

        /// <summary>Returns a <see cref="Color3"/> from the given hue, saturation, and value components.</summary>
        public static Color3 fromHSV(float h, float s, float v)
        {
            return null!;
        }

        /// <summary>Returns a <see cref="Color3"/> from a given hex value.</summary>
        public static Color3 fromHex(string hex)
        {
            return null!;
        }

        /// <summary>Returns a <see cref="Color3"/> interpolated between two colors.</summary>
        public Color3 Lerp(Color3 color, float alpha)
        {
            return null!;
        }

        /// <summary>Returns the hue, saturation, and value of a <see cref="Color3"/>.</summary>
        public (float, float, float) ToHSV()
        {
            return default;
        }

        /// <summary>Returns the hex code of a <see cref="Color3"/>.</summary>
        public string ToHex()
        {
            return null!;
        }
    }

    /// <summary></summary>
    public sealed class BrickColor
    {
        /// <summary>The unique number that identifies the <see cref="BrickColor"/>.</summary>
        public readonly uint Number;
        /// <summary>The red component of the <see cref="BrickColor"/> (between 0 and 1).</summary>
        public readonly float r;
        /// <summary>The green component of the <see cref="BrickColor"/> (between 0 and 1).</summary>
        public readonly float g;
        /// <summary>The blue component of the <see cref="BrickColor"/> (between 0 and 1).</summary>
        public readonly float b;
        /// <summary>The name associated with the <see cref="BrickColor"/>.</summary>
        public readonly string Name = null!;
        /// <summary>The <see cref="Color3"/> associated with the <see cref="BrickColor"/>.</summary>
        public readonly Color3 Color = null!;

        /// <summary>Constructs a <see cref="BrickColor"/> from its numerical index.</summary>
        public BrickColor(uint index)
        { 
        }

        /// <summary>Constructs the closest <see cref="BrickColor"/> that can be matched to the specified RGB components, each between 0 and 1.</summary>
        public BrickColor(float r, float g, float b)
        {
        }

        /// <summary>Constructs a <see cref="BrickColor"/> from its name.</summary>
        public BrickColor(string name)
        {
        }

        /// <summary>Constructs the closest <see cref="BrickColor"/> that can be matched to the specified <see cref="Color3"/>.</summary>
        public BrickColor(Color3 color)
        {
        }

        /// <summary>Constructs a <see cref="BrickColor"/> from its palette index.</summary>
        public static BrickColor palette(uint paletteIndex)
        {
            return null!;
        }

        /// <summary>Returns a random <see cref="BrickColor"/>.</summary>
        public static BrickColor random()
        {
            return null!;
        }

        /// <summary>Returns a random <see cref="BrickColor"/>.</summary>
        public static BrickColor Random()
        {
            return null!;
        }

        /// <summary>Returns the "White" <see cref="BrickColor"/>.</summary>
        public static BrickColor White()
        {
            return null!;
        }

        /// <summary>Returns the "Medium stone grey" <see cref="BrickColor"/>.</summary>
        public static BrickColor Gray()
        {
            return null!;
        }

        /// <summary>Returns the "Dark stone grey" <see cref="BrickColor"/>.</summary>
        public static BrickColor DarkGray()
        {
            return null!;
        }

        /// <summary>Returns the "Black" <see cref="BrickColor"/>.</summary>
        public static BrickColor Black()
        {
            return null!;
        }

        /// <summary>Returns the "Bright Red" <see cref="BrickColor"/>.</summary>
        public static BrickColor Red()
        {
            return null!;
        }

        /// <summary>Returns the "Bright Yellow" <see cref="BrickColor"/>.</summary>
        public static BrickColor Yellow()
        {
            return null!;
        }

        /// <summary>Returns the "Dark Green" <see cref="BrickColor"/>.</summary>
        public static BrickColor Green()
        {
            return null!;
        }

        /// <summary>Returns the "Bright Blue" <see cref="BrickColor"/>.</summary>
        public static BrickColor Blue()
        {
            return null!;
        }
    }

    /// <summary>
    /// <para>The <see cref="Ray"/> data type represents a half-line, finite in one direction but infinite in the other.</para>
    /// <para>It can be defined by a 3D point, where the line originates from, and a direction vector, which is the direction it goes in.</para>
    /// </summary>
    public sealed class Ray
    {
        /// <summary>The <see cref="Ray"/> with a normalized direction (the direction has a magnitude of 1).</summary>
        public readonly Ray Unit = null!;
        /// <summary>The position of the origin.</summary>
        public readonly Vector3 Origin = null!;
        /// <summary>The direction vector of the <see cref="Ray"/>.</summary>
        public readonly Vector3 Direction = null!;

        /// <summary>Returns a <see cref="Ray"/> with the given Origin and Direction.</summary>
        public Ray(Vector3 Origin, Vector3 Direction)
        {
        }

        /// <summary>Returns a <see cref="Vector3"/> projected onto the ray so that it is within the <see cref="Ray"/> line of sight.</summary>
        public Vector3 ClosestPoint(Vector3 point)
        {
            return null!;
        }

        /// <summary>Returns the distance between the given point and the closest point on the <see cref="Ray"/>.</summary>
        public float Distance(Vector3 point)
        {
            return default;
        }
    }

    /// <summary>
    /// <para>The <see cref="RaycastResult"/> data type stores the result of a successful raycasting operation performed by <see cref="WorldRoot.Raycast(Vector3, Vector3, RaycastParams?)"/>. It contains the properties listed below.</para>
    /// <para>This object should not be confused with the similarly-named <see cref="RaycastParams"/> which is used to perform a raycast.</para>
    /// </summary>
    public sealed class RaycastResult
    {
        /// <summary>The distance between the ray origin and the intersection point.</summary>
        public readonly float Distance;
        /// <summary>The <see cref="BasePart"/> or <see cref="Terrain"/> cell that the ray intersected.</summary>
        public readonly Instance? Instance;
        /// <summary>The <see cref="Enum.Material"/> at the intersection point.</summary>
        public readonly Enum.Material Material;
        /// <summary>The position of the intersection between the ray and the part.</summary>
        public readonly Vector3 Position = null!;
        /// <summary>The normal vector of the intersected face.</summary>
        public readonly Vector3 Normal = null!;
    }

    /// <summary>
    /// <para>The <see cref="RaycastParams"/> data type stores parameters for <see cref="WorldRoot.Raycast(Vector3, Vector3, RaycastParams?)"/> operations.</para>
    /// <para>The <see cref="FilterDescendantsInstances"/> property stores an array of objects to use as either an inclusion or exclusion list based on the <see cref="FilterType"/> enum.</para>
    /// <para>If desired, the <see cref="IgnoreWater"/> property can be used to ignore Terrain water, and the <see cref="CollisionGroup"/> property can specify a collision group for the raycasting operation.</para>
    /// <para>This object is different from the similarly named <see cref="RaycastResult"/> which provides the results of a raycast.</para>
    /// <para>Unlike most data types in Luau, you can change all of the members of <see cref="RaycastParams"/> without creating a new object, allowing you to reuse the same object repeatedly.</para>
    /// </summary>
    public sealed class RaycastParams
    {
        /// <summary>An array of objects whose descendants are used in filtering raycasting candidates.</summary>
        public Instance[]? FilterDescendantsInstances;
        /// <summary>Determines how the <see cref="FilterDescendantsInstances"/> array is used.</summary>
        public Enum.RaycastFilterType? FilterType;
        /// <summary>Determines whether the water material is considered when raycasting against <see cref="Terrain"/>.</summary>
        public bool? IgnoreWater;
        /// <summary>The collision group used for the operation.</summary>
        public string? CollisionGroup;
        /// <summary>Determines whether the raycast operation considers a part's <see cref="BasePart.CanCollide"/> property value over its <see cref="BasePart.CanQuery"/> value.</summary>
        public bool? RespectCanCollide;
        /// <summary>When enabled, the query will ignore all part collision properties and perform a brute-force check on every part.</summary>
        public bool? BruteForceAllSlow;

        /// <summary>Returns a blank <see cref="RaycastParams"/>.</summary>
        public RaycastParams()
        {
        }

        /// <summary>
        /// <para>For efficiency and simplicity, this method is the preferred way to add instances to the filter.</para>
        /// <para>It has the additional advantage that it allows <see cref="FilterDescendantsInstances"/> to be updated from a parallel context.</para>
        /// </summary>
        /// <param name="instance">An instance to add.</param>
        public void AddToFilter(Instance instance)
        {
        }

        /// <summary>
        /// <para>For efficiency and simplicity, this method is the preferred way to add instances to the filter.</para>
        /// <para>It has the additional advantage that it allows <see cref="FilterDescendantsInstances"/> to be updated from a parallel context.</para>
        /// </summary>
        /// <param name="instances">An array containing instances to add.</param>
        public void AddToFilter(Instance[] instances)
        {
        }
    }

    /// <summary>
    /// <para>The <see cref="OverlapParams"/> data type stores parameters for use with <see cref="WorldRoot"/> boundary-querying functions, in particular <see cref="WorldRoot.GetPartBoundsInBox(CFrame, Vector3, OverlapParams?)"/>, <see cref="WorldRoot.GetPartBoundsInRadius(Vector3, float, OverlapParams?)"/> and <see cref="WorldRoot.GetPartsInPart(BasePart, OverlapParams?)"/>.</para>
    /// <para>The <see cref="FilterDescendantsInstances"/> property stores an array of objects to use as either an inclusion or exclusion list based on the <see cref="FilterType"/> enum, and the <see cref="CollisionGroup"/> property can specify a collision group for the boundary query operation.</para>
    /// <para>Unlike most data types in Luau, you can change all of the members of <see cref="OverlapParams"/> without creating a new object, allowing you to reuse the same object repeatedly.</para>
    /// </summary>
    public sealed class OverlapParams
    {
        /// <summary>An array of objects whose descendants is used in filtering candidates.</summary>
        public Instance[]? FilterDescendantsInstances;
        /// <summary>Determines how the <see cref="FilterDescendantsInstances"/> list is used.</summary>
        public Enum.RaycastFilterType? FilterType;
        /// <summary>The maximum amount of parts to be returned by the query.</summary>
        public uint MaxParts;
        /// <summary>The collision group used for the operation.</summary>
        public string? CollisionGroup;
        /// <summary>Determines whether the boundary-querying operation considers a part's <see cref="BasePart.CanCollide"/> property value over its <see cref="BasePart.CanQuery"/> value.</summary>
        public bool? RespectCanCollide;
        /// <summary>When enabled, the query will ignore all part collision properties and perform a brute-force check on every part.</summary>
        public bool? BruteForceAllSlow;

        /// <summary>Returns a blank <see cref="OverlapParams"/> object.</summary>
        public OverlapParams()
        {
        }

        public void AddToFilter(Instance instance)
        {
        }

        public void AddToFilter(Instance[] instances)
        {
        }
    }

    /// <summary>The <see cref="UDim"/> data type represents a one-dimensional value with two components, a relative scale and an absolute offset.</summary>
    public sealed class UDim
    {
        /// <summary>The relative scale component of the <see cref="UDim"/>.</summary>
        public readonly float Scale;
        /// <summary>The absolute offset component of the <see cref="UDim"/>.</summary>
        public readonly uint Offset;

        /// <summary>Returns a <see cref="UDim"/> from the given components.</summary>
        public UDim(float Scale, uint Offset)
        {
        }

        public static UDim operator +(UDim a, UDim b)
        {
            return null!;
        }

        public static UDim operator -(UDim a, UDim b)
        {
            return null!;
        }
    }

    /// <summary>
    /// <para>The <see cref="UDim2"/> data type represents a two-dimensional value where each dimension is composed of a relative scale and an absolute offset.</para>
    /// <para>It is a combination of two <see cref="UDim"/> representing the X and Y dimensions. The most common usages of <see cref="UDim2"/> objects are setting the Size and Position of <see cref="GuiObject"/>s.</para>
    /// </summary>
    public sealed class UDim2
    {
        /// <summary>The X dimension scale and offset of the <see cref="UDim2"/>.</summary>
        public readonly UDim X = null!;
        /// <summary>The Y dimension scale and offset of the <see cref="UDim2"/>.</summary>
        public readonly UDim Y = null!;
        /// <summary>The X dimension scale and offset of the <see cref="UDim2"/>.</summary>
        public readonly UDim Width = null!;
        /// <summary>The Y dimension scale and offset of the <see cref="UDim2"/>.</summary>
        public readonly UDim Height = null!;

        /// <summary>Returns a new <see cref="UDim2"/> given the coordinates of the two <see cref="UDim"/> components representing each axis.</summary>
        public UDim2(float xScale = 0, uint xOffset = 0, float yScale = 0, uint yOffset = 0)
        {
        }

        /// <summary>Returns a new <see cref="UDim2"/> from the given <see cref="UDim"/> objects representing the X and Y dimensions, respectively.</summary>
        public UDim2(UDim x, UDim y)
        {
        }

        /// <summary>Returns a new <see cref="UDim2"/> with the given scalar coordinates and no offsets.</summary>
        public static UDim2 fromScale(float xScale = 0, float yScale = 0)
        {
            return null!;
        }

        /// <summary>Returns a new <see cref="UDim2"/> with the given offset coordinates and no scales.</summary>
        public static UDim2 fromOffset(uint xOffset = 0, uint yOffset = 0)
        {
            return null!;
        }

        public UDim2 Lerp(UDim2 goal, float alpha)
        {
            return null!;
        }

        public static UDim2 operator +(UDim2 a, UDim2 b)
        {
            return null!;
        }

        public static UDim2 operator -(UDim2 a, UDim2 b)
        {
            return null!;
        }
    }

    /// <summary>
    /// The <see cref="Random"/> data type generates pseudorandom numbers and directions.
    /// </summary>
    public sealed class Random
    {
        /// <summary>Returns a new pseudorandom generator using an optional seed.</summary>
        public Random(long seed)
        {
        }

        /// <summary>Returns a pseudorandom number uniformly distributed over [0, 1].</summary>
        public int NextInteger(int min, int max)
        {
            return default;
        }

        /// <summary>Returns a pseudorandom number uniformly distributed over [min, max].</summary>
        public int NextNumber()
        {
            return default;
        }

        public int NextNumber(int min, int max)
        {
            return default;
        }

        /// <summary>Uniformly shuffles a table in-place.</summary>
        public void Shuffle(object[] tb)
        {
        }

        /// <summary>Returns a unit vector with a pseudorandom direction.</summary>
        public int NextUnitVector()
        {
            return default;
        }

        /// <summary>Returns a new <see cref="Random"/> object with the same state as the original.</summary>
        public Random Clone()
        {
            return null!;
        }
    }

    public interface time_t
    {
        public uint year { get; }
        public uint month { get; }
        public uint day { get; }
        public uint wday { get; }
        public uint yday { get; }
        public uint hour { get; }
        public uint min { get; }
        public uint sec { get; }
        public bool isdst { get; }
    }

    public interface TimeTable
    {
        public uint Year { get; }
        public uint Month { get; }
        public uint Day { get; }
        public uint Hour { get; }
        public uint Minute { get; }
        public uint Second { get; }
        public uint Millisecond { get; }
    }

    /// <summary>
    /// <para>The <see cref="DateTime"/> data type represents a moment in time using a Unix timestamp.</para>
    /// <para>It can be used to easily format dates and times in specific locales.</para>
    /// <para>When converted to a string, a string conversion of the stored timestamp integer is returned.</para>
    /// <para>They don't store timezone values. Instead, timezones are considered when constructing and using <see cref="DateTime"/> objects.</para>
    /// <para><see cref="DateTime"/> objects are equal if and only if their <see cref="UnixTimestampMillis"/> properties are equal.</para>
    /// </summary>
    public interface DateTime
    {
        public ulong UnixTimestamp { get; }
        public ulong UnixTimestampMillis { get; }

        /// <summary>Returns a <see cref="DateTime"/> representing the current moment in time.</summary>
        public static DateTime now()
        {
            return null!;
        }

        /// <summary>Returns a <see cref="DateTime"/> representing the given Unix timestamp.</summary>
        public static DateTime fromUnixTimestamp(float unixTimestamp)
        {
            return null!;
        }

        /// <summary>Returns a <see cref="DateTime"/> representing the given Unix timestamp in milliseconds.</summary>
        public static DateTime fromUnixTimestampMillis(float unixTimestampMillis)
        {
            return null!;
        }

        /// <summary>Returns a new <see cref="DateTime"/> using the given units from a UTC time.</summary>
        public static DateTime fromUniversalTime(uint year, byte month, byte day, byte hour, byte minute, byte second, ushort millisecond)
        {
            return null!;
        }

        /// <summary>Returns a new <see cref="DateTime"/> using the given units from a local time.summary>
        public static DateTime fromLocalTime(uint year, byte month, byte day, byte hour, byte minute, byte second, ushort millisecond)
        {
            return null!;
        }

        /// <summary>Returns a <see cref="DateTime"/> from an ISO 8601 date-time string (in UTC).</summary>
        public static DateTime fromIsoDate(string isoDate)
        {
            return null!;
        }

        /// <summary>Converts the value of this <see cref="DateTime"/> object to Universal Coordinated Time (UTC).</summary>
        public TimeTable ToUniversalTime()
        {
            return null!;
        }

        /// <summary>Converts the value of this <see cref="DateTime"/> object to local time.</summary>
        public TimeTable ToLocalTime()
        {
            return null!;
        }

        /// <summary>
        /// <para>Formats a date as a ISO 8601 date-time string.</para>
        /// <para>The value returned by this function could be passed to <see cref="fromIsoDate(string)"/> to produce the original <see cref="DateTime"/> object.</para>
        /// <para>An example ISO 8601 date-time string would be 2020-01-02T10:30:45Z, which represents January 2nd 2020 at 10:30 AM, 45 seconds.</para>
        /// </summary>
        public string ToIsoDate()
        {
            return null!;
        }

        /// <summary>
        /// <para>Generates a string from the <see cref="DateTime"/> value interpreted as Universal Coordinated Time (UTC) and a format string.</para>
        /// <para>The format string should contain tokens, which will replace to certain date/time values described by the <see cref="DateTime"/> object.</para>
        /// </summary>
        public string FormatUniversalTime(string format, string locale)
        {
            return null!;
        }

        /// <summary>
        /// <para>Generates a string from the <see cref="DateTime"/> value interpreted as local time and a format string.</para>
        /// <para>The format string should contain tokens, which will replace to certain date/time values described by the <see cref="DateTime"/> object.</para>
        /// </summary>
        public string FormatLocalTime(string format, string locale)
        {
            return null!;
        }
    }

    /// <summary>The <see cref="NumberRange"/> represents a range of numbers.</summary>
    public sealed class NumberRange
    {
        /// <summary>The minimum value of the <see cref="NumberRange"/>.</summary>
        public readonly float Min;
        /// <summary>The maximum value of the <see cref="NumberRange"/>.</summary>
        public readonly float Max;

        /// <summary>Returns a new <see cref="NumberRange"/> with the minimum and maximum set to the value.</summary>
        public NumberRange(float number)
        {
        }

        /// <summary>Returns a new <see cref="NumberRange"/> with the provided minimum and maximum.</summary>
        public NumberRange(float minimum, float maximum)
        {
        }
    }

    /// <summary>
    /// <para>The <see cref="NumberSequence"/> data type represents a series of number values from 0 to 1.</para>
    /// <para>The number values are expressed using the <see cref="NumberSequenceKeypoint"/> type. This type is used in properties such as <see cref="ParticleEmitter.Size"/> and <see cref="Beam.Transparency"/> to define a numerical change over time.</para>
    /// </summary>
    public sealed class NumberSequence
    {
        /// <summary>An array of <see cref="NumberSequenceKeypoint"/> values in ascending order.</summary>
        public readonly NumberSequenceKeypoint[] Keypoints = null!;

        /// <summary>Returns a <see cref="NumberSequence"/> with the start and end values set to the provided n.</summary>
        public NumberSequence(float n)
        {
        }

        /// <summary>Returns a <see cref="NumberSequence"/> of two keypoints with n0 as the start value and n1 as the end value.</summary>
        public NumberSequence(float n0, float n1)
        {
        }

        /// <summary>Returns a <see cref="NumberSequence"/> from an array of <see cref="NumberSequenceKeypoint"/>s.</summary>
        public NumberSequence(NumberSequenceKeypoint[] Keypoints)
        {
        }
    }

    /// <summary>
    /// The <see cref="NumberSequenceKeypoint"/> data type represents keypoints within a <see cref="NumberSequence"/> with a particular time, value, and envelope size.
    /// </summary>
    public sealed class NumberSequenceKeypoint
    {
        /// <summary>The amount of variance allowed from the value.</summary>
        public readonly float Envelope;
        /// <summary>The relative time at which the keypoint is positioned.</summary>
        public readonly float Time;
        /// <summary>The base value of the keypoint.</summary>
        public readonly float Value;

        /// <summary>Returns a keypoint with the specified time and value.</summary>
        public NumberSequenceKeypoint(float time, float value)
        { 
        }

        /// <summary>Returns a keypoint with the specified time, value, and envelope.</summary>
        public NumberSequenceKeypoint(float time, float value, float envelope)
        {
        }
    }

    /// <summary>
    /// <para>The <see cref="ColorSequence"/> data type represents a gradient of color values from 0 to 1.</para>
    /// <para>The color values are expressed using the <see cref="ColorSequenceKeypoint"/> type. This type is used in various properties of <see cref="ParticleEmitter"/>, <see cref="Trail"/>, <see cref="Beam"/>, and other objects that use color gradients.</para>
    /// </summary>
    public sealed class ColorSequence
    {
        /// <summary>An array of <see cref="ColorSequenceKeypoint"/> values in ascending order.</summary>
        public readonly ColorSequenceKeypoint[] Keypoint = null!;

        /// <summary>Returns a new <see cref="ColorSequence"/> that is entirely the specified color.</summary>
        public ColorSequence(Color3 c)
        {
        }

        /// <summary>Returns a new <see cref="ColorSequence"/> with c0 as the start value and c1 as the end value.</summary>
        public ColorSequence(Color3 c0, Color3 c1)
        {
        }

        /// <summary>Returns a new <see cref="ColorSequence"/> from an array of ColorSequenceKeypoints.</summary>
        public ColorSequence(ColorSequenceKeypoint[] keypoints)
        {
        }
    }

    public sealed class ColorSequenceKeypoint
    {
        /// <summary>The relative time at which the keypoint is located.</summary>
        public readonly float Time;
        /// <summary>The <see cref="Color3"/> value of the keypoint.</summary>
        public readonly Color3 Value = null!;

        /// <summary>Creates a <see cref="ColorSequenceKeypoint"/> with a specified time and color.</summary>
        public ColorSequenceKeypoint(float time, Color3 color)
        {
        }
    }

    public sealed class Rect
    {
        /// <summary>The width of the <see cref="Rect"/> in pixels.</summary>
        public readonly float Width;
        /// <summary>The height of the <see cref="Rect"/> in pixels.</summary>
        public readonly float Height;
        /// <summary>The top-left corner.</summary>
        public readonly Vector2 Min = null!;
        /// <summary>The bottom-right corner.</summary>
        public readonly Vector2 Max = null!;

        /// <summary>Returns a new <see cref="Rect"/> with zero <see cref="Vector2"/> positions.</summary>
        public Rect()
        { 
        }

        /// <summary>Returns a new <see cref="Rect"/> from the given <see cref="Vector2"/> positions.</summary>
        public Rect(Vector2 min, Vector2 max)
        {
        }

        /// <summary>Returns a new <see cref="Rect"/> using the first and last two arguments as coordinates for corners.</summary>
        public Rect(float minX, float minY, float maxX, float maxY)
        {
        }
    }

    /// <summary>
    /// The <see cref="Axes"/> data type is for the <see cref="ArcHandles"/> class to control which rotation axes are currently enabled.
    /// </summary>
    public class Axes
    {
        /// <summary>Whether the X axis is enabled.</summary>
        public readonly bool X;
        /// <summary>Whether the Y axis is enabled.</summary>
        public readonly bool Y;
        /// <summary>Whether the Z axis is enabled.</summary>
        public readonly bool Z;
        /// <summary>Whether the top face is included.</summary>
        public readonly bool Top;
        /// <summary>Whether the bottom face is included.</summary>
        public readonly bool Bottom;
        /// <summary>Whether the left face is included.</summary>
        public readonly bool Left;
        /// <summary>Whether the right face is included.</summary>
        public readonly bool Right;
        /// <summary>Whether the back face is included.</summary>
        public readonly bool Back;
        /// <summary>Whether the front face is included.</summary>
        public readonly bool Front;

        /// <summary>Create an empty <see cref="Axes"/></summary>
        public Axes()
        { 
        }

        /// <summary>Creates a new <see cref="Axes"/> using list of axes.</summary>
        public Axes((Enum.Axis, Enum.Axis?, Enum.Axis?) axes)
        {
        }

        /// <summary>Creates a new <see cref="Axes"/> using list of faces. NormalIds (faces) are converted to the corresponding axes.</summary>
        public Axes((Enum.NormalId, Enum.NormalId?, Enum.NormalId?, Enum.NormalId?, Enum.NormalId?, Enum.NormalId?) axes)
        {
        }
    }

    /// <summary>
    /// <para>The <see cref="Faces"/> data type contains six booleans representing whether a feature is enabled for each face (<see cref="Enum.NormalId"/>) of a <see cref="Part"/>.</para>
    /// <para>In other words, this contains a boolean for each axes (X/Y/Z) in both directions (positive/negative). The <see cref="Handles"/> object uses this data type to enable whether a direction has a visible handle on a <see cref="Part"/>'s face.</para>
    /// <para>Like most data types on Roblox, the <see cref="Faces"/> data type is immutable: you cannot assign to its properties once created.</para>
    /// </summary>
    public sealed class Faces
    {
        /// <summary>Whether the top face is included.</summary>
        public readonly bool Top;
        /// <summary>Whether the bottom face is included.</summary>
        public readonly bool Bottom;
        /// <summary>Whether the left face is included.</summary>
        public readonly bool Left;
        /// <summary>Whether the right face is included.</summary>
        public readonly bool Right;
        /// <summary>Whether the back face is included.</summary>
        public readonly bool Back;
        /// <summary>Whether the front face is included.</summary>
        public readonly bool Front;

        /// <summary>Create an empty <see cref="Faces"/></summary>
        public Faces()
        {
        }

        /// <summary>
        /// <para>Creates a new <see cref="Faces"/> given some number of <see cref="Enum.NormalId"/> as arguments.</para>
        /// <para>Each NormalId provided indicates the property of the same name in the new <see cref="Faces"/> will be true.</para>
        /// <para>Passing values that are not a <see cref="Enum.NormalId"/> will do nothing; they are ignored silently.</para>
        /// </summary>
        public Faces((Enum.NormalId, Enum.NormalId?, Enum.NormalId?, Enum.NormalId?, Enum.NormalId?, Enum.NormalId?) axes)
        {
        }
    }

    /// <summary>
    /// <para>The <see cref="PhysicalProperties"/> data type describes several physical properties of a <see cref="BasePart"/>: <see cref="BasePart.Density"/>, <see cref="BasePart.Elasticity"/>, and <see cref="BasePart.Friction"/>.</para>
    /// <para>It is used in the similarly-named <see cref="BasePart.CustomPhysicalProperties"/> property.</para>
    /// </summary>
    public sealed class PhysicalProperties
    {
        /// <summary>The mass per unit volume of the part.</summary>
        public float Density;
        /// <summary>The deceleration of the part when rubbing against another part.</summary>
        public float Friction;
        /// <summary>The amount of energy retained when colliding with another part.</summary>
        public float Elasticity;
        /// <summary>The importance of the part's <see cref="BasePart.Friction"/> property when calculating the friction with the colliding part.</summary>
        public float FrictionWeight;
        /// <summary>The importance of the part's <see cref="BasePart.Elasticity"/> property when calculating the elasticity with the colliding part.</summary>
        public float ElasticityWeight;

        /// <summary>Returns a <see cref="PhysicalProperties"/> container, with the density, friction, and elasticity specified for this Material.</summary>
        public PhysicalProperties(Enum.Material material)
        {
        }

        /// <summary>Returns a <see cref="PhysicalProperties"/> container, with the specified density, friction, and elasticity.</summary>
        public PhysicalProperties(float density, float friction, float elasticity)
        {
        }

        /// <summary>Creates a <see cref="PhysicalProperties"/> container with the specified density, friction, elasticity, weight of friction, and weight of elasticity.</summary>
        public PhysicalProperties(float density, float friction, float elasticity, float frictionWeight, float elasticityWeight)
        {
        }
    }

    /// <summary>
    /// The <see cref="Path2DControlPoint"/> data type represents a single control point used with the <see cref="Path2D"/> instance.
    /// </summary>
    public sealed class Path2DControlPoint
    {
        /// <summary>The position of the <see cref="Path2DControlPoint"/>.</summary>
        public readonly UDim2 Position = null!;
        /// <summary>The left tangent of the <see cref="Path2DControlPoint"/>.</summary>
        public readonly UDim2 LeftTangent = null!;
        /// <summary>The right tangent of the <see cref="Path2DControlPoint"/>.</summary>
        public readonly UDim2 RightTangent = null!;

        /// <summary>Returns an empty <see cref="Path2DControlPoint"/>.</summary>
        public Path2DControlPoint()
        { 
        }

        /// <summary>Returns an empty <see cref="Path2DControlPoint"/>.</summary>
        public Path2DControlPoint(UDim2 position)
        {
        }

        /// <summary>Returns an empty <see cref="Path2DControlPoint"/>.</summary>
        public Path2DControlPoint(UDim2 position, UDim2 leftTangent, UDim2 rightTangent)
        {
        }
    }


    /// <summary>
    /// <para>A time-value pair used with <see cref="RotationCurve"/> instances.</para>
    /// <para>The <see cref="Interpolation"/> property dictates the interpolation mode for the segment started by this key and ended by the next key on the curve. Each segment may use a different interpolation mode.</para>
    /// <para>The <see cref="LeftTangent"/> and <see cref="RightTangent"/> properties apply to the cubic interpolation mode and define the desired tangent (slope) at the key. Different left and right values can be used to encode discontinuities in slope at the key.</para>
    /// <para>Attempting to set a <see cref="RightTangent"/> value on a key that doesn't use the cubic interpolation mode will result in a runtime error. It is possible to set the <see cref="LeftTangent"/> property on any key, as it will be used should the preceding segment use cubic interpolation.</para>
    /// </summary>
    public sealed class RotationCurveKey
    {
        /// <summary>The key interpolation mode for the segment started by this <see cref="RotationCurveKey"/>.</summary
        public readonly Enum.KeyInterpolationMode Interpolation;
        /// <summary>The time position of this <see cref="RotationCurveKey"/>.</summary>
        public readonly float Time;
        /// <summary>The value of this <see cref="RotationCurveKey"/>.</summary>
        public readonly CFrame Value = null!;
        /// <summary>The tangent to the right of this <see cref="RotationCurveKey"/>.</summary>
        public readonly float RightTangent;
        /// <summary>The tangent to the left of this <see cref="RotationCurveKey"/>.</summary>
        public readonly float LeftTangent;

        /// <summary>
        /// <para>Creates a new <see cref="RotationCurveKey"/> at a given time with a given <see cref="CFrame"/>.</para>
        /// <para><see cref="LeftTangent"/> and <see cref="RightTangent"/> are left uninitialized and, if not initialized, tangent values of 0 will be used when evaluating the curve.</para>
        /// </summary>
        /// <param name="time">Time at which to create the new <see cref="RotationCurveKey"/>.</param>
        /// <param name="value">CFrame of the new <see cref="RotationCurveKey"/>.</param>
        /// <param name="interpolation"></param>
        public RotationCurveKey(float time, CFrame cframe, Enum.KeyInterpolationMode interpolation)
        {
        }
    }

    /// <summary>
    /// <para>A time-value pair used with <see cref="FloatCurve"/> instances.</para>
    /// <para>The <see cref="Interpolation"/> property dictates the interpolation mode for the segment started by this key and ended by the next key on the curve. Each segment may use a different interpolation mode.</para>
    /// <para>The <see cref="LeftTangent"/> and <see cref="RightTangent"/> properties apply to the cubic interpolation mode and define the desired tangent (slope) at the key. Different left and right values can be used to encode discontinuities in slope at the key.</para>
    /// <para>Attempting to set a <see cref="RightTangent"/> value on a key that doesn't use the cubic interpolation mode will result in a runtime error. It is possible to set the <see cref="LeftTangent"/> property on any key, as it will be used should the preceding segment use cubic interpolation.</para>
    /// </summary>
    public sealed class FloatCurveKey
    {
        /// <summary>The key interpolation mode for the segment started by this <see cref="FloatCurveKey"/>.</summary
        public readonly Enum.KeyInterpolationMode Interpolation;
        /// <summary>The time position of this <see cref="FloatCurveKey"/>.</summary>
        public readonly float Time;
        /// <summary>The value of this <see cref="FloatCurveKey"/>.</summary>
        public readonly float Value;
        /// <summary>The tangent to the right of this <see cref="FloatCurveKey"/>.</summary>
        public readonly float RightTangent;
        /// <summary>The tangent to the left of this <see cref="FloatCurveKey"/>.</summary>
        public readonly float LeftTangent;

        /// <summary>
        /// <para>Creates a new <see cref="FloatCurveKey"/> at a given time and value.</para>
        /// <para><see cref="LeftTangent"/> and <see cref="RightTangent"/> are left uninitialized and, if not initialized, tangent values of 0 will be used when evaluating the curve.</para>
        /// </summary>
        /// <param name="time">Time at which to create the new <see cref="FloatCurveKey"/>.</param>
        /// <param name="value">Value of the new <see cref="FloatCurveKey"/>.</param>
        /// <param name="interpolation"></param>
        public FloatCurveKey(float time, float value, Enum.KeyInterpolationMode interpolation)
        { 
        }
    }

    /// <summary>
    /// <para>Describes the font used to render text. Every font consists of a font family (like Source Sans Pro), a weight like <see cref="Enum.FontWeight.Bold"/>, and a style like <see cref="Enum.FontStyle.Italic"/>.</para>
    /// <para>Font families are a type of asset, like images or meshes. Each font family contains a number of font faces, and each face has a different weight and style.</para>
    /// <para><see cref="Font"/> is used by the <see cref="TextLabel.FontFace"/>, <see cref="TextButton.FontFace"/>, and <see cref="TextBox.FontFace"/> properties.</para>
    /// </summary>
    public sealed class Font
    {
        /// <summary>The asset ID for the font family. These start with either rbxasset:// or rbxassetid://.</summary>
        public readonly string Family = null!;
        /// <summary>
        /// <para>How thick the text is. The default value is <see cref="Enum.FontWeight.Regular"/>.</para>
        /// <para>When set, <see cref="Font.Bold"/> is updated. Bold is true if the weight is <see cref="Enum.FontWeight.SemiBold"/> or thicker.</para>
        /// </summary>
        public readonly Enum.FontWeight Weight;
        /// <summary>
        /// <para>Whether the font is italic. The default value is <see cref="Enum.FontStyle.Normal"/>.</para>
        /// <para>The font can be made italic (like this) using <see cref="Enum.FontStyle.Italic"/>.</para>
        /// </summary>
        public readonly Enum.FontStyle Style;
        /// <summary>
        /// Whether the font is bold. Sets <see cref="Font.Weight"/> to <see cref="Enum.FontWeight.Bold"/> when true, and <see cref="Enum.FontWeight.Regular"/> otherwise.
        /// </summary>
        public readonly bool Bold;

        /// <summary>Creates a new <see cref="Font"/>.</summary>
        /// <param name="family">The asset ID for the font family, starting with rbxasset:// or rbxassetid://.</param>
        /// <param name="weight">How thick the text is.</param>
        /// <param name="style">Whether the text is normal or italic.</param>
        public Font(string family, Enum.FontWeight? weight, Enum.FontStyle? style)
        {
        }

        /// <summary>
        /// <para>Creates a <see cref="Font"/> from an <see cref="Enum.Font"/> value. Throws an error when called with <see cref="Enum.Font.Unknown"/>.</para>
        /// <para>The following table indicates the family, weight, and style associated with each <see cref="Enum.Font"/>.</para>
        /// </summary>
        /// <param name="font">The enum value of the font to use.</param>
        public static Font fromEnum(Enum.Font font)
        {
            return null!;
        }

        /// <summary>
        /// <para>This is a convenience method for creating fonts from the content folder. The name you pass in will be converted into an asset ID like "rbxasset://fonts/families/YourFontNameHere.json".</para>
        /// <para>The name can only contain alphabetical characters, digits, _ (underscore), and - (hyphen). It can't contain any spaces.</para>
        /// </summary>
        /// <param name="name">The name of the font.</param>
        /// <param name="weight">How thick the text is.</param>
        /// <param name="style">Whether the text is normal or italic.</param>
        public static Font fromName(string name, Enum.FontWeight? weight, Enum.FontStyle? style)
        {
            return null!;
        }

        /// <summary>This is a convenience method for creating fonts from an asset ID number.</summary>
        /// <param name="id">The asset ID of the font as a number.</param>
        /// <param name="weight">How thick the text is.</param>
        /// <param name="style">Whether the text is normal or italic.</param>
        public static Font fromId(ulong id, Enum.FontWeight? weight, Enum.FontStyle? style)
        {
            return null!;
        }
    }

    /// <summary>
    /// <para>The <see cref="Secret"/> data type stores the secret content returned by <see cref="HttpService.GetSecret(string)"/>.</para>
    /// <para>It cannot be printed or logged, but can be modified using built-in functions, as demonstrated by the code block below.</para>
    /// </summary>
    public sealed class Secret
    {
        /// <summary>Prepends a string to the secret content.</summary>
        public Secret AddPrefix(string prefix)
        {
            return null!;
        }

        /// <summary>Appends a string to the secret content.</summary>
        public Secret AddSuffix(string prefix)
        {
            return null!;
        }
    }

    /// <summary>
    /// <para>The <see cref="DockWidgetPluginGuiInfo"/> data type describes details for a <see cref="DockWidgetPluginGui"/>.</para>
    /// <para>This data type is used when constructing a <see cref="PluginGui"/> via the plugin's <see cref="PluginClasses.Plugin.CreateDockWidgetPluginGui(string, DockWidgetPluginGuiInfo)"/> method.</para>
    /// </summary>
    public sealed class DockWidgetPluginGuiInfo
    {
        /// <summary>
        /// <para>The initial enabled state of a PluginGui created using this DockWidgetPluginGuiInfo.</para>
        /// <para>If a PluginGui with the same ''pluginGuiId'' has previously been created in an earlier session of Roblox Studio, then it will reload that saved enabled state (unless <see cref="InitialEnabledShouldOverrideRestore"/> is true).</para>
        /// </summary>
        public readonly bool InitialEnabled;
        /// <summary>
        /// <para>If true, the value of <see cref="InitialEnabled"/> will override the previously saved enabled state of a <see cref="PluginClasses.PluginGui"/> being created with this <see cref="DockWidgetPluginGuiInfo"/>.</para>
        /// <para>The previously saved enabled state is loaded based on the pluginGuiId argument of <see cref="PluginClasses.Plugin.CreateDockWidgetPluginGui(string, DockWidgetPluginGuiInfo)"/>.</para>
        /// </summary>
        public readonly bool InitialEnabledShouldOverrideRestore;
        /// <summary>The initial pixel width of a <see cref="PluginClasses.PluginGui"/> created using this <see cref="DockWidgetPluginGuiInfo"/>, when the <see cref="Enum.InitialDockState"/> is set to <see cref="Enum.InitialDockState.Float"/>.</summary>
        public readonly float FloatingXSize;
        /// <summary>The initial pixel height of a <see cref="PluginClasses.PluginGui"/> created using this <see cref="DockWidgetPluginGuiInfo"/>, when the <see cref="Enum.InitialDockState"/> is set to <see cref="Enum.InitialDockState.Float"/>.</summary>
        public readonly float FloatingYSize;
        /// <summary>
        /// <para>The minimum width of a <see cref="PluginClasses.PluginGui"/> created using this <see cref="DockWidgetPluginGuiInfo"/>, in pixels.</para>
        /// <para>Each platform has its own absolute minimum that Roblox will enforce. These variations exist to account for the contents of the title bar (which varies by platform) when the widget is floating. For example, on a Mac, the width can never be less than ~80 pixels to accommodate the close/minimize/maximize buttons.</para>
        /// </summary>
        public readonly float MinWidth;
        /// <summary>The minimum height of a <see cref="PluginClasses.PluginGui"/> created using this <see cref="DockWidgetPluginGuiInfo"/>, in pixels.</summary>
        public readonly float MinHeight;

        /// <summary>Returns a new <see cref="DockWidgetPluginGuiInfo"/> object.</summary>
        public DockWidgetPluginGuiInfo(
            Enum.InitialDockState? initDockState,
            bool? initEnabled,
            bool? overrideEnabledRestore,
            float? floatXSize,
            float? floatYSize,
            float? minWidth,
            float? minHeight
        )
        { 
        }
    }

    /// <summary>
    /// <para>The <see cref="CatalogSearchParams"/> data type stores the parameters of a catalog search via <see cref="AvatarEditorService.SearchCatalog(CatalogSearchParams)"/>.</para>
    /// <para>When accessing the value of the <see cref="BundleTypes"/> or <see cref="AssetTypes"/> property the returned table will be read-only to avoid confusion when not directly accessing the <see cref="CatalogSearchParams"/> instance.</para>
    /// </summary>
    public sealed class CatalogSearchParams
    {
        /// <summary>The keyword to search for catalog results with.</summary>
        public string? SearchKeyword;
        /// <summary>The minimum item price to search for.</summary>
        public ulong? MinPrice;
        /// <summary>The maximum item price to search for.</summary>
        public ulong? MaxPrice;
        /// <summary>The order in which to sort the results.</summary>
        public Enum.CatalogSortType? SortType;
        /// <summary>The time period to use to aggregate the sort results.</summary>
        public Enum.CatalogSortAggregation? SortAggregation;
        /// <summary>The category to filter the search by.</summary>
        public Enum.CatalogCategoryFilter? CategoryFilter;
        /// <summary>The sales type filter the search by.</summary>
        public Enum.SalesTypeFilter? SalesTypeFilter;
        /// <summary>An array containing <see cref="Enum.BundleType"/> values to filter the search by.</summary>
        public Enum.BundleType[]? BundleTypes;
        /// <summary>An array containing <see cref="Enum.AvatarAssetType"/> values to filter the search by.</summary>
        public Enum.AvatarAssetType[]? AssetTypes;
        /// <summary>Whether off sale items should be included in the results.</summary>
        public bool? IncludeOffSale;
        /// <summary>Search for items with the given creator.</summary>
        public string? CreatorName;
        /// <summary>Specifies the number of items to return. Accepts 10, 28, 30, 60, and 120. Defaults to 30.</summary>
        public byte? Limit;
    }

    /// <summary>
    /// <para>The <see cref="TweenInfo"/> data type stores parameters for <see cref="TweenService.Create(Instance?, TweenInfo, object)"/> to specify the behavior of the tween.</para>
    /// <para>The properties of a <see cref="TweenInfo"/> cannot be written to after its creation.</para>
    /// </summary>
    public sealed class TweenInfo
    {
        /// <summary>The style in which the tween executes.</summary>
        public readonly Enum.EasingStyle EasingStyle;
        /// <summary>The direction in which the EasingStyle executes.</summary>
        public readonly Enum.EasingDirection EasingDirection;
        /// <summary>The amount of time the tween takes in seconds.</summary>
        public readonly float Time;
        /// <summary>The amount of time that elapses before tween starts in seconds.</summary>
        public readonly float DelayTime;
        /// <summary>The number of times the tween repeats after tweening once.</summary>
        public readonly ushort RepeatCount;
        /// <summary>Whether or not the tween does the reverse tween once the initial tween completes.</summary>
        public readonly bool Reverses;

        /// <summary>Creates a new <see cref="TweenInfo"/> from the provided parameters.</summary>
        public TweenInfo(float? time, Enum.EasingStyle? easingStyle, Enum.EasingDirection? easingDirection, ushort? repeatCount, float? delayTime, bool? reverses)
        {
        }
    }

    /// <summary>
    /// <para>Represents a table-like data structure that can be shared across execution contexts.</para>
    /// <para>While it can be used for various sorts of general data storage, it is designed especially for use with Parallel Luau, where it can be used to share state across scripts parented under different <see cref="Actor"/> instances.</para>
    /// <para>There are a couple idiomatic ways to communicate shared tables between scripts. One method is to store and retrieve <see cref="SharedTable"/> objects in the <see cref="SharedTableRegistry"/>. The registry lets any script in the same data model get or set a <see cref="SharedTable"/> by name. Another method is to use <see cref="Actor.SendMessage(string, object[])"/> to send a shared table to another <see cref="Actor"/> inside a message.</para>
    /// </summary>
    public sealed class SharedTable : Dictionary<string, object>
    {
        public static void clear(SharedTable st)
        { 
        }

        public static SharedTable clone(SharedTable st, bool? deep)
        {
            return null!;
        }

        public static SharedTable cloneAndFreeze(SharedTable st, bool? deep)
        {
            return null!;
        }

        public static float increment(SharedTable st, string key, float delta)
        {
            return default;
        }

        public static bool isFrozen(SharedTable st)
        {
            return default;
        }

        public static ushort size(SharedTable st)
        {
            return default;
        }

        public static void update(SharedTable st, string key, Func<object> f)
        {
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
        public ulong Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
    }

    public interface GroupInfo
    {
        interface GroupOwner
        {
            public ulong Id { get; set; }
            public string Name { get; set; }
        }

        interface GroupRole
        {
            public string Name { get; set; }
            public byte Rank { get; set; }
        }

        public ulong Id { get; set; }
        public string Name { get; set; }
        public GroupOwner Owner { get; set; }
        public string EmblemUrl { get; set; }
        public string Description { get; set; }
        public GroupRole[] Roles { get; set; }
    }

    public interface GetGroupsAsyncResult
    {
        public ulong Id { get; set; }
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