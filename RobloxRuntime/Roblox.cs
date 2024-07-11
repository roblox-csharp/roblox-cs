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

        /// <summary>Returns an orthonormalized copy of the <see cref="CFrame"/>. The <see cref="Classes.BasePart.CFrame"/> property automatically applies orthonormalization, but other APIs which take <see cref="CFrame"/>s do not, so this method is occasionally necessary when incrementally updating a <see cref="CFrame"/> and using it with them.</summary>
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

    /// <summary></summary>
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
    /// <para>The UDim2 data type represents a two-dimensional value where each dimension is composed of a relative scale and an absolute offset.</para>
    /// <para>It is a combination of two UDim representing the X and Y dimensions. The most common usages of <see cref="UDim2"/> objects are setting the Size and Position of <see cref="Classes.GuiObject"/>s.</para>
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
    /// <para>The number values are expressed using the <see cref="NumberSequenceKeypoint"/> type. This type is used in properties such as <see cref="Classes.ParticleEmitter.Size"/> and <see cref="Classes.Beam.Transparency"/> to define a numerical change over time.</para>
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