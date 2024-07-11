// THIS FILE IS AUTOMATICALLY GENERATED AND SHOULD NOT BE EDITED MANUALLY!

namespace RobloxRuntime.Plugin
{
	// GENERATED ROBLOX INSTANCE CLASSES
	using RobloxRuntime.Classes;
	
	public partial interface Instance
	{
		public bool RobloxLocked { get; set; }
		public string GetDebugId(int? scopeLength);
	}
	
	public partial interface KeyframeSequence : AnimationClip
	{
		public float AuthoredHipHeight { get; set; }
	}
	
	public partial interface AnimationClipProvider : Instance
	{
		public AnimationClip GetAnimationClip(string assetId);
		public AnimationClip GetAnimationClipById(long assetId, bool useCache);
	}
	
	public partial interface Animator : Instance
	{
		public void StepAnimations(float deltaTime);
	}
	
	public partial interface AssetService : Instance
	{
		public MeshPart CreateMeshPartAsync(string meshId, object? options);
	}
	
	public interface CoreGui : BasePlayerGui
	{
		public int Version { get; set; }
	}
	
	public partial interface StarterGui : BasePlayerGui
	{
		public bool ProcessUserInput { get; set; }
		public bool ShowDevelopmentGui { get; set; }
	}
	
	public interface ChangeHistoryService : Instance
	{
		public void FinishRecording(string identifier, Enum.FinishRecordingOperation operation, object? finalOptions);
		public object[] GetCanRedo();
		public object[] GetCanUndo();
		public bool IsRecordingInProgress(string? identifier);
		public void Redo();
		public void ResetWaypoints();
		public void SetEnabled(bool state);
		public void SetWaypoint(string name);
		public string? TryBeginRecording(string name, string? displayName);
		public void Undo();
		public ScriptSignal<string, string?, string?, Enum.FinishRecordingOperation, object?> OnRecordingFinished { get; }
		public ScriptSignal<string, string?> OnRecordingStarted { get; }
		public ScriptSignal<string> OnRedo { get; }
		public ScriptSignal<string> OnUndo { get; }
	}
	
	public interface DataModelSession : Instance
	{
	}
	
	public interface DebugSettings : Instance
	{
		public int DataModel { get; set; }
		public int InstanceCount { get; set; }
		public bool IsScriptStackTracingEnabled { get; set; }
		public int JobCount { get; set; }
		public int PlayerCount { get; set; }
		public bool ReportSoundWarnings { get; set; }
		public string RobloxVersion { get; set; }
		public Enum.TickCountSampleMethod TickCountPreciseOverride { get; set; }
	}
	
	public interface DebuggerBreakpoint : Instance
	{
		public string Condition { get; set; }
		public bool ContinueExecution { get; set; }
		public bool IsEnabled { get; set; }
		public int Line { get; set; }
		public string LogExpression { get; set; }
		public bool isContextDependentBreakpoint { get; set; }
		public int line { get; set; }
	}
	
	public interface DebuggerManager : Instance
	{
		public bool DebuggingEnabled { get; set; }
		public Instance? AddDebugger(Instance? script);
		public Instance[] GetDebuggers();
		public void Resume();
		public void StepIn();
		public void StepOut();
		public void StepOver();
		public ScriptSignal<Instance?> DebuggerAdded { get; }
		public ScriptSignal<Instance?> DebuggerRemoved { get; }
	}
	
	public interface DebuggerWatch : Instance
	{
		public string Expression { get; set; }
	}
	
	public interface FaceControls : Instance
	{
		public float ChinRaiser { get; set; }
		public float ChinRaiserUpperLip { get; set; }
		public float Corrugator { get; set; }
		public float EyesLookDown { get; set; }
		public float EyesLookLeft { get; set; }
		public float EyesLookRight { get; set; }
		public float EyesLookUp { get; set; }
		public float FlatPucker { get; set; }
		public float Funneler { get; set; }
		public float JawDrop { get; set; }
		public float JawLeft { get; set; }
		public float JawRight { get; set; }
		public float LeftBrowLowerer { get; set; }
		public float LeftCheekPuff { get; set; }
		public float LeftCheekRaiser { get; set; }
		public float LeftDimpler { get; set; }
		public float LeftEyeClosed { get; set; }
		public float LeftEyeUpperLidRaiser { get; set; }
		public float LeftInnerBrowRaiser { get; set; }
		public float LeftLipCornerDown { get; set; }
		public float LeftLipCornerPuller { get; set; }
		public float LeftLipStretcher { get; set; }
		public float LeftLowerLipDepressor { get; set; }
		public float LeftNoseWrinkler { get; set; }
		public float LeftOuterBrowRaiser { get; set; }
		public float LeftUpperLipRaiser { get; set; }
		public float LipPresser { get; set; }
		public float LipsTogether { get; set; }
		public float LowerLipSuck { get; set; }
		public float MouthLeft { get; set; }
		public float MouthRight { get; set; }
		public float Pucker { get; set; }
		public float RightBrowLowerer { get; set; }
		public float RightCheekPuff { get; set; }
		public float RightCheekRaiser { get; set; }
		public float RightDimpler { get; set; }
		public float RightEyeClosed { get; set; }
		public float RightEyeUpperLidRaiser { get; set; }
		public float RightInnerBrowRaiser { get; set; }
		public float RightLipCornerDown { get; set; }
		public float RightLipCornerPuller { get; set; }
		public float RightLipStretcher { get; set; }
		public float RightLowerLipDepressor { get; set; }
		public float RightNoseWrinkler { get; set; }
		public float RightOuterBrowRaiser { get; set; }
		public float RightUpperLipRaiser { get; set; }
		public float TongueDown { get; set; }
		public float TongueOut { get; set; }
		public float TongueUp { get; set; }
		public float UpperLipSuck { get; set; }
	}
	
	public interface File : Instance
	{
		public long Size { get; set; }
		public string GetBinaryContents();
		public string GetTemporaryId();
	}
	
	public partial interface GameSettings : Instance
	{
		public bool VideoCaptureEnabled { get; set; }
	}
	
	public interface PluginGui : LayerCollector
	{
		public string Title { get; set; }
		public void BindToClose(Action? function);
		public Vector2 GetRelativeMousePosition();
		public ScriptSignal<object> PluginDragDropped { get; }
		public ScriptSignal<object> PluginDragEntered { get; }
		public ScriptSignal<object> PluginDragLeft { get; }
		public ScriptSignal<object> PluginDragMoved { get; }
		public ScriptSignal WindowFocusReleased { get; }
		public ScriptSignal WindowFocused { get; }
	}
	
	public interface DockWidgetPluginGui : PluginGui
	{
	}
	
	public interface QWidgetPluginGui : PluginGui
	{
	}
	
	public partial interface KeyframeSequenceProvider : Instance
	{
		public Instance? GetKeyframeSequence(string assetId);
		public Instance? GetKeyframeSequenceById(long assetId, bool useCache);
	}
	
	public interface LuaSettings : Instance
	{
	}
	
	public interface Script : BaseScript
	{
		public string Source { get; set; }
	}
	
	public interface ModuleScript : LuaSourceContainer
	{
		public string Source { get; set; }
	}
	
	public interface MaterialVariant : Instance
	{
		public string ColorMap { get; set; }
		public string MetalnessMap { get; set; }
		public string NormalMap { get; set; }
		public string RoughnessMap { get; set; }
	}
	
	public interface MemStorageConnection : Instance
	{
		public void Disconnect();
	}
	
	public interface PluginMouse : Mouse
	{
		public ScriptSignal<Instance[]> DragEnter { get; }
	}
	
	public interface MultipleDocumentInterfaceInstance : Instance
	{
	}
	
	public interface NetworkPeer : Instance
	{
		public void SetOutgoingKBPSLimit(int limit);
	}
	
	public interface NetworkClient : NetworkPeer
	{
	}
	
	public interface NetworkServer : NetworkPeer
	{
	}
	
	public interface NetworkReplicator : Instance
	{
		public Instance? GetPlayer();
	}
	
	public interface ClientReplicator : NetworkReplicator
	{
	}
	
	public interface ServerReplicator : NetworkReplicator
	{
	}
	
	public interface NetworkSettings : Instance
	{
		public int EmulatedTotalMemoryInMB { get; set; }
		public float FreeMemoryMBytes { get; set; }
		public bool HttpProxyEnabled { get; }
		public string HttpProxyURL { get; }
		public double IncomingReplicationLag { get; set; }
		public bool PrintJoinSizeBreakdown { get; set; }
		public bool PrintPhysicsErrors { get; set; }
		public bool PrintStreamInstanceQuota { get; set; }
		public bool RandomizeJoinInstanceOrder { get; set; }
		public bool RenderStreamedRegions { get; set; }
		public bool ShowActiveAnimationAsset { get; set; }
	}
	
	public partial interface Terrain : BasePart
	{
		public void ConvertToSmooth();
	}
	
	public interface Model : PVInstance
	{
		public Enum.ModelLevelOfDetail LevelOfDetail { get; set; }
	}
	
	public partial interface WorldRoot : Model
	{
		public void IKMoveTo(BasePart part, CFrame target, float? translateStiffness, float? rotateStiffness, Enum.IKCollisionsMode? collisionsMode);
		public void StepPhysics(float dt, Instance[]? parts);
	}
	
	public partial interface Workspace : WorldRoot
	{
		public void BreakJoints(Instance[] objects);
		public void MakeJoints(Instance[] objects);
		public void ZoomToExtents();
	}
	
	public interface PackageService : Instance
	{
	}
	
	public interface PhysicsSettings : Instance
	{
		public bool AllowSleep { get; set; }
		public bool AreAnchorsShown { get; set; }
		public bool AreAssembliesShown { get; set; }
		public bool AreAwakePartsHighlighted { get; set; }
		public bool AreBodyTypesShown { get; set; }
		public bool AreContactIslandsShown { get; set; }
		public bool AreContactPointsShown { get; set; }
		public bool AreJointCoordinatesShown { get; set; }
		public bool AreMechanismsShown { get; set; }
		public bool AreModelCoordsShown { get; set; }
		public bool AreNonAnchorsShown { get; set; }
		public bool AreOwnersShown { get; set; }
		public bool ArePartCoordsShown { get; set; }
		public bool AreRegionsShown { get; set; }
		public bool AreTerrainReplicationRegionsShown { get; set; }
		public bool AreUnalignedPartsShown { get; set; }
		public bool AreWorldCoordsShown { get; set; }
		public bool DisableCSGv2 { get; set; }
		public bool DisableCSGv3ForPlugins { get; set; }
		public bool ForceCSGv2 { get; set; }
		public bool IsInterpolationThrottleShown { get; set; }
		public bool IsReceiveAgeShown { get; set; }
		public bool IsTreeShown { get; set; }
		public Enum.EnviromentalPhysicsThrottle PhysicsEnvironmentalThrottle { get; set; }
		public bool ShowDecompositionGeometry { get; set; }
		public double ThrottleAdjustTime { get; set; }
		public bool UseCSGv2 { get; set; }
	}
	
	public partial interface Player : Instance
	{
		public void SetAccountAge(int accountAge);
		public void SetSuperSafeChat(bool value);
	}
	
	public partial interface Players : Instance
	{
		public void Chat(string message);
		public void SetChatStyle(Enum.ChatStyle? style);
		public void TeamChat(string message);
	}
	
	public interface Plugin : Instance
	{
		public bool CollisionEnabled { get; set; }
		public float GridSize { get; set; }
		public void Activate(bool exclusiveMouse);
		public PluginAction CreatePluginAction(string actionId, string text, string statusTip, string iconName, bool? allowBinding);
		public PluginMenu CreatePluginMenu(string id, string title, string icon);
		public PluginToolbar CreateToolbar(string name);
		public void Deactivate();
		public Enum.JointCreationMode GetJoinMode();
		public PluginMouse GetMouse();
		public Enum.RibbonTool GetSelectedRibbonTool();
		public object? GetSetting(string key);
		public long GetStudioUserId();
		public Instance? Intersect(Instance[] objects);
		public bool IsActivated();
		public bool IsActivatedWithExclusiveMouse();
		public Instance[] Negate(Instance[] objects);
		public void OpenScript(LuaSourceContainer script, int? lineNumber);
		public void OpenWikiPage(string url);
		public void SaveSelectedToRoblox();
		public void SelectRibbonTool(Enum.RibbonTool tool, UDim2 position);
		public Instance[] Separate(Instance[] objects);
		public void SetSetting(string key, object? value);
		public void StartDrag(object dragData);
		public Instance? Union(Instance[] objects);
		public DockWidgetPluginGui CreateDockWidgetPluginGui(string pluginGuiId, DockWidgetPluginGuiInfo dockWidgetPluginGuiInfo);
		public Instance? ImportFbxAnimation(Instance? rigModel, bool? isR15);
		public Instance? ImportFbxRig(bool? isR15);
		public long PromptForExistingAssetId(string assetType);
		public bool PromptSaveSelection(string suggestedFileName);
		public ScriptSignal Deactivation { get; }
		public ScriptSignal Unloading { get; }
	}
	
	public interface PluginAction : Instance
	{
		public string ActionId { get; set; }
		public bool AllowBinding { get; set; }
		public string StatusTip { get; set; }
		public string Text { get; set; }
		public ScriptSignal Triggered { get; }
	}
	
	public interface PluginDebugService : Instance
	{
	}
	
	public interface PluginDragEvent : Instance
	{
		public string Data { get; set; }
		public string MimeType { get; set; }
		public Vector2 Position { get; set; }
		public string Sender { get; set; }
	}
	
	public interface PluginGuiService : Instance
	{
	}
	
	public partial interface PluginManagerInterface : Instance
	{
		public Instance? CreatePlugin();
		public void ExportPlace(string filePath);
		public void ExportSelection(string filePath);
	}
	
	public interface PluginMenu : Instance
	{
		public string Icon { get; set; }
		public string Title { get; set; }
		public void AddAction(Instance? action);
		public void AddMenu(Instance? menu);
		public Instance? AddNewAction(string actionId, string text, string icon);
		public void AddSeparator();
		public void Clear();
		public Instance? ShowAsync();
	}
	
	public interface PluginToolbar : Instance
	{
		public PluginToolbarButton CreateButton(string buttonId, string tooltip, string iconname, string text);
	}
	
	public interface PluginToolbarButton : Instance
	{
		public bool ClickableWhenViewportHidden { get; set; }
		public bool Enabled { get; set; }
		public string Icon { get; set; }
		public void SetActive(bool active);
		public ScriptSignal Click { get; }
	}
	
	public interface RenderSettings : Instance
	{
		public int AutoFRMLevel { get; set; }
		public bool EagerBulkExecution { get; set; }
		public Enum.QualityLevel EditQualityLevel { get; set; }
		public bool EnableVRMode { get; set; }
		public bool EnableFRM { get; set; }
		public bool ExportMergeByMaterial { get; set; }
		public Enum.FramerateManagerMode FrameRateManager { get; set; }
		public Enum.GraphicsMode GraphicsMode { get; set; }
		public int MeshCacheSize { get; set; }
		public Enum.MeshPartDetailLevel MeshPartDetailLevel { get; set; }
		public Enum.QualityLevel QualityLevel { get; set; }
		public bool ReloadAssets { get; set; }
		public bool RenderCSGTrianglesDebug { get; set; }
		public bool ShowBoundingBoxes { get; set; }
		public Enum.ViewMode ViewMode { get; set; }
		public int GetMaxQualityLevel();
	}
	
	public interface RenderingTest : Instance
	{
		public CFrame CFrame { get; set; }
		public int ComparisonDiffThreshold { get; set; }
		public Enum.RenderingTestComparisonMethod ComparisonMethod { get; set; }
		public float ComparisonPsnrThreshold { get; set; }
		public string Description { get; set; }
		public float FieldOfView { get; set; }
		public Vector3 Orientation { get; set; }
		public bool PerfTest { get; set; }
		public Vector3 Position { get; set; }
		public bool QualityAuto { get; set; }
		public int QualityLevel { get; set; }
		public int RenderingTestFrameCount { get; set; }
		public bool ShouldSkip { get; set; }
		public string Ticket { get; set; }
		public int Timeout { get; set; }
		public void RenderdocTriggerCapture();
	}
	
	public interface RobloxPluginGuiService : Instance
	{
	}
	
	public partial interface RunService : Instance
	{
		public Enum.RunState RunState { get; set; }
		public bool IsEdit();
		public void Pause();
		public void Reset();
		public void Run();
		public void Stop();
	}
	
	public partial interface ScriptContext : Instance
	{
		public void SetTimeout(double seconds);
	}
	
	public interface ScriptDebugger : Instance
	{
		public string CoreScriptIdentifier { get; set; }
		public int CurrentLine { get; set; }
		public bool IsDebugging { get; set; }
		public bool IsPaused { get; set; }
		public Instance? Script { get; set; }
		public string ScriptGuid { get; set; }
		public Instance? AddWatch(string expression);
		public Instance[] GetBreakpoints();
		public object GetGlobals(int? stackFrame);
		public object GetLocals(int? stackFrame);
		public object[] GetStack();
		public object GetUpvalues(int? stackFrame);
		public object? GetWatchValue(Instance? watch);
		public Instance[] GetWatches();
		public Instance? SetBreakpoint(int line, bool isContextDependentBreakpoint);
		public void SetGlobal(string name, object? value, int stackFrame);
		public void SetLocal(string name, object? value, int? stackFrame);
		public void SetUpvalue(string name, object? value, int? stackFrame);
		public ScriptSignal<Instance?> BreakpointAdded { get; }
		public ScriptSignal<Instance?> BreakpointRemoved { get; }
		public ScriptSignal<int, Enum.BreakReason> EncounteredBreak { get; }
		public ScriptSignal Resuming { get; }
		public ScriptSignal<Instance?> WatchAdded { get; }
		public ScriptSignal<Instance?> WatchRemoved { get; }
	}
	
	public partial interface ScriptDocument : Instance
	{
		public string GetLine(int? lineIndex);
		public int GetLineCount();
		public LuaSourceContainer GetScript();
		public string GetSelectedText();
		public object[] GetSelection();
		public object[] GetSelectionEnd();
		public object[] GetSelectionStart();
		public string GetText(int? startLine, int? startCharacter, int? endLine, int? endCharacter);
		public object[] GetViewport();
		public bool HasSelectedText();
		public bool IsCommandBar();
		public object[] CloseAsync();
		public object[] EditTextAsync(string newText, int startLine, int startCharacter, int endLine, int endCharacter);
		public object[] ForceSetSelectionAsync(int cursorLine, int cursorCharacter, int? anchorLine, int? anchorCharacter);
		public object[] RequestSetSelectionAsync(int cursorLine, int cursorCharacter, int? anchorLine, int? anchorCharacter);
		public ScriptSignal<long, long, long, long> SelectionChanged { get; }
		public ScriptSignal<long, long> ViewportChanged { get; }
	}
	
	public partial interface ScriptEditorService : Instance
	{
		public void DeregisterAutocompleteCallback(string name);
		public void DeregisterScriptAnalysisCallback(string name);
		public ScriptDocument FindScriptDocument(LuaSourceContainer script);
		public string GetEditorSource(LuaSourceContainer script);
		public Instance[] GetScriptDocuments();
		public void RegisterAutocompleteCallback(string name, int priority, Action callbackFunction);
		public void RegisterScriptAnalysisCallback(string name, int priority, Action callbackFunction);
		public object[] OpenScriptDocumentAsync(LuaSourceContainer script);
		public void UpdateSourceAsync(LuaSourceContainer script, Action callback);
		public ScriptSignal<ScriptDocument, object?> TextDocumentDidChange { get; }
		public ScriptSignal<ScriptDocument> TextDocumentDidClose { get; }
		public ScriptSignal<ScriptDocument> TextDocumentDidOpen { get; }
	}
	
	public partial interface ScriptProfilerService : Instance
	{
		public void ClientRequestData(Player player);
		public void ClientStart(Player player, int? frequency);
		public void ClientStop(Player player);
		public object DeserializeJSON(string? jsonString);
		public void ServerRequestData();
		public void ServerStart(int? frequency);
		public void ServerStop();
		public ScriptSignal<Player, string> OnNewData { get; }
	}
	
	public interface Selection : Instance
	{
		public float SelectionThickness { get; set; }
		public void Add(Instance[] instancesToAdd);
		public Instance[] Get();
		public void Remove(Instance[] instancesToRemove);
		public void Set(Instance[] selection);
		public ScriptSignal SelectionChanged { get; }
	}
	
	public partial interface DataModel : ServiceProvider
	{
		public object[] GetJobsInfo();
		public Instance[] GetObjects(string url);
		public void SetPlaceId(long placeId);
		public void SetUniverseId(long universeId);
	}
	
	public interface GlobalSettings : GenericSettings
	{
		public bool GetFFlag(string name);
		public string GetFVariable(string name);
	}
	
	public partial interface SoundService : Instance
	{
		public void OpenAttenuationCurveEditor(Instance[] selectedCurveObjects);
	}
	
	public interface StatsItem : Instance
	{
		public string DisplayName { get; set; }
		public double GetValue();
		public string GetValueString();
	}
	
	public interface RunningAverageItemDouble : StatsItem
	{
	}
	
	public interface RunningAverageItemInt : StatsItem
	{
	}
	
	public interface RunningAverageTimeIntervalItem : StatsItem
	{
	}
	
	public interface TotalCountTimeIntervalItem : StatsItem
	{
	}
	
	public interface StudioData : Instance
	{
	}
	
	public interface StudioService : Instance
	{
		public Instance? ActiveScript { get; set; }
		public bool DraggerSolveConstraints { get; set; }
		public bool DrawConstraintsOnTop { get; set; }
		public float GridSize { get; set; }
		public float RotateIncrement { get; set; }
		public bool ShowConstraintDetails { get; set; }
		public string StudioLocaleId { get; set; }
		public bool UseLocalSpace { get; set; }
		public object GetClassIcon(string className);
		public long GetUserId();
		public RaycastResult GizmoRaycast(Vector3 origin, Vector3 direction, RaycastParams? raycastParams);
		public Instance? PromptImportFile(object[]? fileTypeFilter);
		public Instance[] PromptImportFiles(object[]? fileTypeFilter);
	}
	
	public interface StudioTheme : Instance
	{
		public Color3 GetColor(Enum.StudioStyleGuideColor styleguideitem, Enum.StudioStyleGuideModifier? modifier);
	}
	
	public interface SurfaceAppearance : Instance
	{
		public Enum.AlphaMode AlphaMode { get; set; }
		public string ColorMap { get; set; }
		public string MetalnessMap { get; set; }
		public string NormalMap { get; set; }
		public string RoughnessMap { get; set; }
	}
	
	public interface TaskScheduler : Instance
	{
		public double SchedulerDutyCycle { get; set; }
		public double SchedulerRate { get; set; }
		public Enum.ThreadPoolConfig ThreadPoolConfig { get; set; }
		public int ThreadPoolSize { get; set; }
	}
	
	public interface TerrainDetail : Instance
	{
		public string ColorMap { get; set; }
		public string MetalnessMap { get; set; }
		public string NormalMap { get; set; }
		public string RoughnessMap { get; set; }
	}
	
	public partial interface TerrainRegion : Instance
	{
		public void ConvertToSmooth();
	}
	
	public interface TestService : Instance
	{
		public bool AutoRuns { get; set; }
		public string Description { get; set; }
		public int ErrorCount { get; set; }
		public bool ExecuteWithStudioRun { get; set; }
		public bool Is30FpsThrottleEnabled { get; set; }
		public bool IsPhysicsEnvironmentalThrottled { get; set; }
		public bool IsSleepAllowed { get; set; }
		public int NumberOfPlayers { get; set; }
		public double SimulateSecondsLag { get; set; }
		public int TestCount { get; set; }
		public double Timeout { get; set; }
		public int WarnCount { get; set; }
		public void Check(bool condition, string description, Instance? source, int? line);
		public void Checkpoint(string text, Instance? source, int? line);
		public void Done();
		public void Error(string description, Instance? source, int? line);
		public void Fail(string description, Instance? source, int? line);
		public void Message(string text, Instance? source, int? line);
		public void Require(bool condition, string description, Instance? source, int? line);
		public object ScopeTime();
		public void Warn(bool condition, string description, Instance? source, int? line);
		public bool isFeatureEnabled(string name);
		public void Run();
		public ScriptSignal<bool, string, Instance?, int> ServerCollectConditionalResult { get; }
		public ScriptSignal<string, Instance?, int> ServerCollectResult { get; }
	}
	
	public interface VersionControlService : Instance
	{
	}
	
	public interface VoiceChatService : Instance
	{
		public bool EnableDefaultVoice { get; set; }
		public Enum.AudioApiRollout UseAudioApi { get; set; }
	}
	
}