namespace TypeGenerator
{
    internal static class Constants
    {
        public const string ROOT_CLASS_NAME = "<<<ROOT>>>";
        public static readonly List<string> BAD_NAME_CHARS = new List<string> { " ", "/", "\"" };

        public static readonly Dictionary<string, Dictionary<string, APITypes.Security>> SECURITY_OVERRIDES = new Dictionary<string, Dictionary<string, APITypes.Security>>
        {
            ["StarterGui"] = new Dictionary<string, APITypes.Security>
            {
                ["ShowDevelopmentGui"] = new APITypes.Security
                {
                    Read = "PluginSecurity",
                    Write = "PluginSecurity"
                }
            }
        };

        public static readonly HashSet<string> PARTIAL_INTERFACES = new HashSet<string>
        {
            "AnimationClipProvider",
            "Animator",
            "AssetService",
            "KeyframeSequenceProvider",
            "KeyframeSequence",
            "Instance",
            "StarterGui",
            "GameSettings",
            "ClipEvaluator",
            "Terrain",
            "WorldRoot",
            "Workspace",
            "Player",
            "Players",
            "PluginManagerInterface",
            "RunService",
            "ScriptContext",
            "ScriptDocument",
            "ScriptEditorService",
            "ScriptProfilerService",
            "DataModel",
            "SoundService",
            "TerrainRegion",
            "NetworkPeer",
            "NetworkClient",
            "NetworkServer",
            "BasePart",
            "ServiceProvider"
        };

        public static readonly HashSet<string> CREATABLE_BLACKLIST = new HashSet<string>
        {
            "UserSettings",
            "DebugSettings",
            "Studio",
            "GameSettings",
            "ParabolaAdornment",
            "LuaSettings",
            "PhysicsSettings",
            "Player",
            "DebuggerWatch",
            "Tween",
            "UserGameSettings"
        };

        public static readonly HashSet<string> PLUGIN_ONLY_CLASSES = new HashSet<string>
        {
            "ABTestService",
            "ChangeHistoryService",
            "CoreGui",
            "DataModelSession",
            "DebuggerBreakpoint",
            "DebuggerManager",
            "DebuggerWatch",
            "DebugSettings",
            "File",
            "GameSettings",
            "GlobalSettings",
            "LuaSettings",
            "MemStorageConnection",
            "MultipleDocumentInterfaceInstance",
            "NetworkPeer",
            "NetworkReplicator",
            "NetworkSettings",
            "PackageService",
            "PhysicsSettings",
            "Plugin",
            "PluginAction",
            "PluginDebugService",
            "PluginDragEvent",
            "PluginGui",
            "PluginGuiService",
            "PluginMenu",
            "PluginMouse",
            "PluginToolbar",
            "PluginToolbarButton",
            "RenderingTest",
            "RenderSettings",
            "RobloxPluginGuiService",
            "ScriptDebugger",
            "Selection",
            "StatsItem",
            "Studio",
            "StudioData",
            "StudioService",
            "StudioTheme",
            "TaskScheduler",
            "TestService",
            "VersionControlService"
        };

        public static readonly HashSet<string> CLASS_BLACKLIST = new HashSet<string>
        {
            // Classes which Roblox leverages internally/in the CoreScripts but serve no purpose to developers
            "AnalysticsSettings",
            "BinaryStringValue",
            "BrowserService",
            "CacheableContentProvider",
            "ClusterPacketCache",
            "CookiesService",
            "CorePackages",
            "CoreScript",
            "CoreScriptSyncService",
            "DraftsService",
            "FlagStandService",
            "FlyweightService",
            "FriendService",
            "Geometry",
            "GoogleAnalyticsConfiguration",
            "GuidRegistryService",
            "HttpRbxApiService",
            "HttpRequest",
            "KeyboardService",
            "LocalStorageService",
            "LuaWebService",
            "MemStorageService",
            "MouseService",
            "PartOperationAsset",
            "PermissionsService",
            "PhysicsPacketCache",
            "PlayerEmulatorService",
            "ReflectionMetadataItem",
            "RobloxReplicatedStorage",
            "RuntimeScriptService",
            "SpawnerService",
            "StandalonePluginScripts",
            "StopWatchReporter",
            "ThirdPartyUserService",
            "TimerService",
            "TouchInputService",
            "VirtualInputManager",
            "Visit",

            // never implemented
            "AdvancedDragger",
            "LoginService",
            "NotificationService",
            "ScriptService",
            "Status",

            // super deprecated:
            "AdService",
            "FunctionalTest",
            "PluginManager",
            "VirtualUser",

            //"BevelMesh",
            "CustomEvent",
            "CustomEventReceiver",
            //"CylinderMesh",
            //"DoubleConstrainedValue",
            "Flag",
            "FlagStand",
            //"FloorWire",
            //"Glue",
            "GuiMain",
            //"Hat",
            "Hint",
            //"Hole",
            "Hopper",
            "HopperBin",
            //"IntConstrainedValue",
            //"JointsService",
            "Message",
            //"MotorFeature",
            "PointsService",
            //"SelectionPartLasso",
            //"SelectionPointLasso",
            //"SkateboardPlatform",
            "Skin",

            "ReflectionMetadata",
            "ReflectionMetadataCallbacks",
            "ReflectionMetadataClasses",
            "ReflectionMetadataEnums",
            "ReflectionMetadataEvents",
            "ReflectionMetadataFunctions",
            "ReflectionMetadataProperties",
            "ReflectionMetadataYieldFunctions",

            "Studio",

            // unused
            "UGCValidationService",
            "RbxAnalyticsService"
        };

        public static readonly Dictionary<string, HashSet<string>> MEMBER_BLACKLIST = new Dictionary<string, HashSet<string>>
        {
            { "Workspace", ["FilteringEnabled"] },
            { "Players", ["FilteringEnabled", "LocalPlayer"] }, // defined in Roblox.cs
            { "CollectionService", ["GetCollection"] },
            { "Instance", ["children", "Remove", "IsA", "FindFirstChildOfClass", "FindFirstChildWhichIsA", "FindFirstAncestorOfClass", "FindFirstAncestorWhichIsA", "Clone", "IsAncestorOf", "IsDescendantOf", "GetAttribute", "GetAttributes", "GetDescendants", "GetTags", "WaitForChild", "clone", "isDescendantOf", "AncestryChanged", "AttributeChanged", "Changed", "ChildAdded", "ChildRemoved", "DescendantAdded", "DescendantRemoving", "Destroying", "childAdded"] }, // defined in Roblox.cs
            { "BodyGyro", ["cframe"] },
            { "BodyAngularVelocity", ["FilteringEnabled"] },
            { "BodyPosition", ["FilteringEnabled"] },
            { "DataStoreService", ["FilteringEnabled"] },
            { "Debris", ["FilteringEnabled"] },
            { "LayerCollector", ["FilteringEnabled"] },
            { "GuiBase3d", ["FilteringEnabled"] },
            { "Model", ["FilteringEnabled"] },
            { "ServiceProvider", ["FilteringEnabled", "GetService", "FindService", "service"] },
            { "DataModel", ["FilteringEnabled", "Workspace", "lighting"] }
        };

        public static readonly Dictionary<string, List<string>> EXPECTED_EXTRA_MEMBERS = new Dictionary<string, List<string>>
        {
            { "Player", new List<string> { "Name" } },
            { "ValueBase", new List<string> { "Value", "Changed" } },
            { "DataStore", new List<string> { "GetAsync", "IncrementAsync", "SetAsync", "UpdateAsync", "RemoveAsync" } },
            { "OrderedDataStore", new List<string> { "GetAsync", "IncrementAsync", "SetAsync", "UpdateAsync", "RemoveAsync" } }
        };

        public static readonly HashSet<string> ABSTRACT_CLASSES = new HashSet<string>
        {
            "BackpackItem",
            "BasePart",
            "BasePlayerGui",
            "BaseScript",
            "BevelMesh",
            "BodyMover",
            "CharacterAppearance",
            "Clothing",
            "Constraint",
            "Controller",
            "DataModelMesh",
            "DynamicRotate",
            "FaceInstance",
            "Feature",
            "FormFactorPart",
            "GenericSettings",
            "GuiBase",
            "GuiBase2d",
            "GuiBase3d",
            "GuiButton",
            "GuiLabel",
            "GuiObject",
            "HandleAdornment",
            "HandlesBase",
            "Instance",
            "JointInstance",
            "LayerCollector",
            "Light",
            "LuaSourceContainer",
            "ManualSurfaceJointInstance",
            "NetworkPeer",
            "NetworkReplicator",
            "Pages",
            "PartAdornment",
            "PluginGui",
            "PostEffect",
            "PVAdornment",
            "PVInstance",
            "SelectionLasso",
            "ServiceProvider",
            "SlidingBallConstraint",
            "SoundEffect",
            "StatsItem",
            "TriangleMeshPart",
            "TweenBase",
            "UIBase",
            "UIComponent",
            "UIConstraint",
            "UIGridStyleLayout",
            "UILayout",
            "ValueBase",
            "WorldRoot"
        };

        public static readonly Dictionary<string, string> RENAMEABLE_AUTO_TYPES = new Dictionary<string, string>
        {
            { "Part", "BasePart" },
            { "Script", "LuaSourceContainer" },
            { "Character", "Model" },
            { "Input", "InputObject" }
        };

        public static readonly Dictionary<string, string> PROP_TYPE_MAP = new Dictionary<string, string>();
        public static readonly Dictionary<string, string> VALUE_TYPE_MAP = new Dictionary<string, string>
        {
            { "Array", "object[]" },
            { "BinaryString", "string" },
            { "SharedString", "string" },
            { "Connection", "ScriptConnection" },
            { "Content", "string" },
            { "CoordinateFrame", "CFrame" },
            { "EventInstance", "ScriptSignal" },
            { "Function", "Action" },
            { "int", "int" },
            { "int64", "long" },
            { "Dictionary", "object" },
            { "Map", "object" },
            { "RBXScriptSignal", "ScriptSignal" },
            { "RBXScriptConnection", "ScriptConnection" },
            // { "Instance", "Instance?" },
            { "Object", "Instance" },
            { "Objects", "Instance[]" },
            { "Property", "string" },
            { "OptionalCoordinateFrame", "CFrame?" },
            { "ProtectedString", "string" },
            { "Rect2D", "Rect" },
            { "Tuple", "object[]" },
            { "Variant", "object" },
            { "Color3uint8", "Color3" },
            { "any", "object" },
            { "Array<any>", "object[]" }
        };

        public static readonly Dictionary<string, string> RETURN_TYPE_MAP = new Dictionary<string, string>
        {
            { "null", "void" }
        };

        public static readonly Dictionary<string, string> ARG_NAME_MAP = new Dictionary<string, string>
        {
            { "debugger", "debug" },
            { "old", "oldValue" },
            { "new", "newValue" },
            { "params", "parameters" },
            { "override", "_override" },
            { "string", "str" },
            { "object", "obj" }
        };
    }
}