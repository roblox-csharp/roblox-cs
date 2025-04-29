namespace RobloxCS.Shared;

public sealed class AnalysisResult
{
    public TypeClassInfo TypeClassInfo { get; } = new();
    public MemberClassInfo MemberClassInfo { get; } = new();
    public MethodBaseClassInfo MethodBaseClassInfo { get; } = new();
    public AssemblyClassInfo AssemblyClassInfo { get; } = new();
    public ModuleClassInfo ModuleClassInfo { get; } = new();
    public PropertyClassInfo PropertyClassInfo { get; } = new();
    public CustomAttributeDataClassInfo CustomAttributeDataClassInfo { get; } = new();
}

public abstract class BaseClassInfo
{
    public HashSet<string> MemberUses { get; } = [];
}

// temporary or something idk
public sealed class TypeClassInfo : BaseClassInfo;
public sealed class MemberClassInfo : BaseClassInfo;
public sealed class MethodBaseClassInfo : BaseClassInfo;
public sealed class AssemblyClassInfo : BaseClassInfo;
public sealed class ModuleClassInfo : BaseClassInfo;
public sealed class PropertyClassInfo : BaseClassInfo;
public sealed class CustomAttributeDataClassInfo : BaseClassInfo;