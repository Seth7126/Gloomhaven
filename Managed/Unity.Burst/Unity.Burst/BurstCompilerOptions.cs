using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Jobs.LowLevel.Unsafe;

namespace Unity.Burst;

public sealed class BurstCompilerOptions
{
	private const string DisableCompilationArg = "--burst-disable-compilation";

	private const string ForceSynchronousCompilationArg = "--burst-force-sync-compilation";

	internal const string DefaultLibraryName = "lib_burst_generated";

	internal const string BurstInitializeName = "burst.initialize";

	internal const string BurstInitializeExternalsName = "burst.initialize.externals";

	internal const string BurstInitializeStaticsName = "burst.initialize.statics";

	internal const string OptionGroup = "group";

	internal const string OptionPlatform = "platform=";

	internal const string OptionBackend = "backend=";

	internal const string OptionGlobalSafetyChecksSetting = "global-safety-checks-setting=";

	internal const string OptionDisableSafetyChecks = "disable-safety-checks";

	internal const string OptionDisableOpt = "disable-opt";

	internal const string OptionFastMath = "fastmath";

	internal const string OptionTarget = "target=";

	internal const string OptionOptLevel = "opt-level=";

	internal const string OptionOptForSize = "opt-for-size";

	internal const string OptionFloatPrecision = "float-precision=";

	internal const string OptionFloatMode = "float-mode=";

	internal const string OptionDisableWarnings = "disable-warnings=";

	internal const string OptionCompilationDefines = "compilation-defines=";

	internal const string OptionDump = "dump=";

	internal const string OptionFormat = "format=";

	internal const string OptionDebugTrap = "debugtrap";

	internal const string OptionDisableVectors = "disable-vectors";

	internal const string OptionDebug = "debug=";

	internal const string OptionDebugMode = "debugMode";

	internal const string OptionStaticLinkage = "generate-static-linkage-methods";

	internal const string OptionJobMarshalling = "generate-job-marshalling-methods";

	internal const string OptionTempDirectory = "temp-folder=";

	internal const string OptionEnableDirectExternalLinking = "enable-direct-external-linking";

	internal const string OptionLinkerOptions = "linker-options=";

	internal const string OptionEnableAutoLayoutFallbackCheck = "enable-autolayout-fallback-check";

	internal const string OptionGenerateLinkXml = "generate-link-xml=";

	internal const string OptionCacheDirectory = "cache-directory=";

	internal const string OptionJitDisableFunctionCaching = "disable-function-caching";

	internal const string OptionJitDisableAssemblyCaching = "disable-assembly-caching";

	internal const string OptionJitEnableAssemblyCachingLogs = "enable-assembly-caching-logs";

	internal const string OptionJitEnableSynchronousCompilation = "enable-synchronous-compilation";

	internal const string OptionJitCompilationPriority = "compilation-priority=";

	internal const string OptionJitLogTimings = "log-timings";

	internal const string OptionJitIsForFunctionPointer = "is-for-function-pointer";

	internal const string OptionJitManagedFunctionPointer = "managed-function-pointer=";

	internal const string OptionJitManagedDelegateHandle = "managed-delegate-handle=";

	internal const string OptionEnableInterpreter = "enable-interpreter";

	internal const string OptionAotAssemblyFolder = "assembly-folder=";

	internal const string OptionRootAssembly = "root-assembly=";

	internal const string OptionIncludeRootAssemblyReferences = "include-root-assembly-references=";

	internal const string OptionAotMethod = "method=";

	internal const string OptionAotType = "type=";

	internal const string OptionAotAssembly = "assembly=";

	internal const string OptionAotOutputPath = "output=";

	internal const string OptionAotKeepIntermediateFiles = "keep-intermediate-files";

	internal const string OptionAotNoLink = "nolink";

	internal const string OptionAotPatchedAssembliesOutputFolder = "patch-assemblies-into=";

	internal const string OptionAotPinvokeNameToPatch = "pinvoke-name=";

	internal const string OptionAotExecuteMethodNameToFind = "execute-method-name=";

	internal const string OptionAotOnlyStaticMethods = "only-static-methods";

	internal const string OptionMethodPrefix = "method-prefix=";

	internal const string OptionAotNoNativeToolchain = "no-native-toolchain";

	internal const string OptionAotEmitLlvmObjects = "emit-llvm-objects";

	internal const string OptionAotKeyFolder = "key-folder=";

	internal const string OptionAotDecodeFolder = "decode-folder=";

	internal const string OptionVerbose = "verbose";

	internal const string OptionValidateExternalToolChain = "validate-external-tool-chain";

	internal const string OptionCompilerThreads = "threads=";

	internal const string OptionChunkSize = "chunk-size=";

	internal const string OptionPrintLogOnMissingPInvokeCallbackAttribute = "print-monopinvokecallbackmissing-message";

	internal const string OptionOutputMode = "output-mode=";

	internal const string OptionAlwaysCreateOutput = "always-create-output=";

	internal const string OptionAotPdbSearchPaths = "pdb-search-paths=";

	internal const string OptionSafetyChecks = "safety-checks";

	internal const string CompilerCommandShutdown = "$shutdown";

	internal const string CompilerCommandCancel = "$cancel";

	internal const string CompilerCommandEnableCompiler = "$enable_compiler";

	internal const string CompilerCommandDisableCompiler = "$disable_compiler";

	internal const string CompilerCommandSetDefaultOptions = "$set_default_options";

	internal const string CompilerCommandTriggerSetupRecompilation = "$trigger_setup_recompilation";

	internal const string CompilerCommandIsCurrentCompilationDone = "$is_current_compilation_done";

	internal const string CompilerCommandTriggerRecompilation = "$trigger_recompilation";

	internal const string CompilerCommandDomainReload = "$domain_reload";

	internal const string CompilerCommandVersionNotification = "$version";

	internal const string CompilerCommandSetProfileCallbacks = "$set_profile_callbacks";

	internal const string CompilerCommandUnloadBurstNatives = "$unload_burst_natives";

	internal const string CompilerCommandIsNativeApiAvailable = "$is_native_api_available";

	internal const string CompilerCommandILPPCompilation = "$ilpp_compilation";

	internal const string CompilerCommandIsArmTestEnv = "$is_arm_test_env";

	internal const string CompilerCommandNotifyAssemblyCompilationFinished = "$notify_assembly_compilation_finished";

	internal const string CompilerCommandNotifyCompilationStarted = "$notify_compilation_started";

	internal const string CompilerCommandNotifyCompilationFinished = "$notify_compilation_finished";

	internal const string CompilerCommandRequestSetProtocolVersionEditor = "$request_set_protocol_version_editor";

	internal const string CompilerCommandSetProtocolVersionBurst = "$set_protocol_version_burst";

	internal static readonly bool ForceDisableBurstCompilation;

	private static readonly bool ForceBurstCompilationSynchronously;

	internal static readonly bool IsSecondaryUnityProcess;

	private bool _enableBurstCompilation;

	private bool _enableBurstCompileSynchronously;

	private bool _enableBurstSafetyChecks;

	private bool _enableBurstTimings;

	private bool _enableBurstDebug;

	private bool _forceEnableBurstSafetyChecks;

	private bool IsGlobal { get; }

	public bool IsEnabled
	{
		get
		{
			if (EnableBurstCompilation)
			{
				return !ForceDisableBurstCompilation;
			}
			return false;
		}
	}

	public bool EnableBurstCompilation
	{
		get
		{
			return _enableBurstCompilation;
		}
		set
		{
			if (IsGlobal && ForceDisableBurstCompilation)
			{
				value = false;
			}
			bool num = _enableBurstCompilation != value;
			if (num && value)
			{
				MaybePreventChangingOption();
			}
			_enableBurstCompilation = value;
			if (IsGlobal)
			{
				JobsUtility.JobCompilerEnabled = value;
				BurstCompiler._IsEnabled = value;
			}
			if (num)
			{
				OnOptionsChanged();
			}
		}
	}

	public bool EnableBurstCompileSynchronously
	{
		get
		{
			return _enableBurstCompileSynchronously;
		}
		set
		{
			bool num = _enableBurstCompileSynchronously != value;
			_enableBurstCompileSynchronously = value;
			if (num)
			{
				OnOptionsChanged();
			}
		}
	}

	public bool EnableBurstSafetyChecks
	{
		get
		{
			return _enableBurstSafetyChecks;
		}
		set
		{
			bool num = _enableBurstSafetyChecks != value;
			if (num)
			{
				MaybePreventChangingOption();
			}
			_enableBurstSafetyChecks = value;
			if (num)
			{
				OnOptionsChanged();
				MaybeTriggerRecompilation();
			}
		}
	}

	public bool ForceEnableBurstSafetyChecks
	{
		get
		{
			return _forceEnableBurstSafetyChecks;
		}
		set
		{
			bool num = _forceEnableBurstSafetyChecks != value;
			if (num)
			{
				MaybePreventChangingOption();
			}
			_forceEnableBurstSafetyChecks = value;
			if (num)
			{
				OnOptionsChanged();
				MaybeTriggerRecompilation();
			}
		}
	}

	public bool EnableBurstDebug
	{
		get
		{
			return _enableBurstDebug;
		}
		set
		{
			bool num = _enableBurstDebug != value;
			if (num)
			{
				MaybePreventChangingOption();
			}
			_enableBurstDebug = value;
			if (num)
			{
				OnOptionsChanged();
				MaybeTriggerRecompilation();
			}
		}
	}

	[Obsolete("This property is no longer used and will be removed in a future major release")]
	public bool DisableOptimizations
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	[Obsolete("This property is no longer used and will be removed in a future major release. Use the [BurstCompile(FloatMode = FloatMode.Fast)] on the method directly to enable this feature")]
	public bool EnableFastMath
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	internal bool EnableBurstTimings
	{
		get
		{
			return _enableBurstTimings;
		}
		set
		{
			bool num = _enableBurstTimings != value;
			_enableBurstTimings = value;
			if (num)
			{
				OnOptionsChanged();
			}
		}
	}

	internal bool RequiresSynchronousCompilation
	{
		get
		{
			if (!EnableBurstCompileSynchronously)
			{
				return ForceBurstCompilationSynchronously;
			}
			return true;
		}
	}

	internal Action OptionsChanged { get; set; }

	private BurstCompilerOptions()
		: this(isGlobal: false)
	{
	}

	internal BurstCompilerOptions(bool isGlobal)
	{
		IsGlobal = isGlobal;
		EnableBurstCompilation = true;
		EnableBurstSafetyChecks = true;
	}

	internal BurstCompilerOptions Clone()
	{
		return new BurstCompilerOptions
		{
			EnableBurstCompilation = EnableBurstCompilation,
			EnableBurstCompileSynchronously = EnableBurstCompileSynchronously,
			EnableBurstSafetyChecks = EnableBurstSafetyChecks,
			EnableBurstTimings = EnableBurstTimings,
			EnableBurstDebug = EnableBurstDebug,
			ForceEnableBurstSafetyChecks = ForceEnableBurstSafetyChecks
		};
	}

	private static bool TryGetAttribute(MemberInfo member, out BurstCompileAttribute attribute)
	{
		attribute = null;
		if (member == null)
		{
			return false;
		}
		attribute = GetBurstCompileAttribute(member);
		if (attribute == null)
		{
			return false;
		}
		return true;
	}

	private static bool TryGetAttribute(Assembly assembly, out BurstCompileAttribute attribute)
	{
		if (assembly == null)
		{
			attribute = null;
			return false;
		}
		attribute = assembly.GetCustomAttribute<BurstCompileAttribute>();
		return attribute != null;
	}

	private static BurstCompileAttribute GetBurstCompileAttribute(MemberInfo memberInfo)
	{
		BurstCompileAttribute customAttribute = memberInfo.GetCustomAttribute<BurstCompileAttribute>();
		if (customAttribute != null)
		{
			return customAttribute;
		}
		foreach (Attribute customAttribute2 in memberInfo.GetCustomAttributes())
		{
			if (customAttribute2.GetType().FullName == "Burst.Compiler.IL.Tests.TestCompilerAttribute")
			{
				List<string> list = new List<string>();
				return new BurstCompileAttribute(FloatPrecision.Standard, FloatMode.Default)
				{
					CompileSynchronously = true,
					Options = list.ToArray()
				};
			}
		}
		return null;
	}

	internal static bool HasBurstCompileAttribute(MemberInfo member)
	{
		if (member == null)
		{
			throw new ArgumentNullException("member");
		}
		BurstCompileAttribute attribute;
		return TryGetAttribute(member, out attribute);
	}

	internal static void MergeAttributes(ref BurstCompileAttribute memberAttribute, in BurstCompileAttribute assemblyAttribute)
	{
		if (memberAttribute.FloatMode == FloatMode.Default)
		{
			memberAttribute.FloatMode = assemblyAttribute.FloatMode;
		}
		if (memberAttribute.FloatPrecision == FloatPrecision.Standard)
		{
			memberAttribute.FloatPrecision = assemblyAttribute.FloatPrecision;
		}
		if (memberAttribute.OptimizeFor == OptimizeFor.Default)
		{
			memberAttribute.OptimizeFor = assemblyAttribute.OptimizeFor;
		}
		if (!memberAttribute._compileSynchronously.HasValue && assemblyAttribute._compileSynchronously.HasValue)
		{
			memberAttribute._compileSynchronously = assemblyAttribute._compileSynchronously;
		}
		if (!memberAttribute._debug.HasValue && assemblyAttribute._debug.HasValue)
		{
			memberAttribute._debug = assemblyAttribute._debug;
		}
		if (!memberAttribute._disableDirectCall.HasValue && assemblyAttribute._disableDirectCall.HasValue)
		{
			memberAttribute._disableDirectCall = assemblyAttribute._disableDirectCall;
		}
		if (!memberAttribute._disableSafetyChecks.HasValue && assemblyAttribute._disableSafetyChecks.HasValue)
		{
			memberAttribute._disableSafetyChecks = assemblyAttribute._disableSafetyChecks;
		}
	}

	internal bool TryGetOptions(MemberInfo member, bool isJit, out string flagsOut, bool isForILPostProcessing = false)
	{
		flagsOut = null;
		if (!TryGetAttribute(member, out var attribute))
		{
			return false;
		}
		if (TryGetAttribute(member.Module.Assembly, out var attribute2))
		{
			MergeAttributes(ref attribute, in attribute2);
		}
		flagsOut = GetOptions(isJit, attribute, isForILPostProcessing);
		return true;
	}

	internal string GetOptions(bool isJit, BurstCompileAttribute attr = null, bool isForILPostProcessing = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (isJit && ((attr != null && attr.CompileSynchronously) || RequiresSynchronousCompilation))
		{
			AddOption(stringBuilder, GetOption("enable-synchronous-compilation"));
		}
		if (isForILPostProcessing)
		{
			AddOption(stringBuilder, GetOption("compilation-priority=", CompilationPriority.ILPP));
		}
		if (attr != null)
		{
			if (attr.FloatMode != FloatMode.Default)
			{
				AddOption(stringBuilder, GetOption("float-mode=", attr.FloatMode));
			}
			if (attr.FloatPrecision != FloatPrecision.Standard)
			{
				AddOption(stringBuilder, GetOption("float-precision=", attr.FloatPrecision));
			}
			if (attr.DisableSafetyChecks)
			{
				AddOption(stringBuilder, GetOption("disable-safety-checks"));
			}
			if (attr.Options != null)
			{
				string[] options = attr.Options;
				foreach (string text in options)
				{
					if (!string.IsNullOrEmpty(text))
					{
						AddOption(stringBuilder, text);
					}
				}
			}
			switch (attr.OptimizeFor)
			{
			case OptimizeFor.Default:
			case OptimizeFor.Balanced:
				AddOption(stringBuilder, GetOption("opt-level=", 2));
				break;
			case OptimizeFor.Performance:
				AddOption(stringBuilder, GetOption("opt-level=", 3));
				break;
			case OptimizeFor.Size:
				AddOption(stringBuilder, GetOption("opt-for-size"));
				AddOption(stringBuilder, GetOption("opt-level=", 3));
				break;
			case OptimizeFor.FastCompilation:
				AddOption(stringBuilder, GetOption("opt-level=", 1));
				break;
			}
		}
		if (ForceEnableBurstSafetyChecks)
		{
			AddOption(stringBuilder, GetOption("global-safety-checks-setting=", GlobalSafetyChecksSettingKind.ForceOn));
		}
		else if (EnableBurstSafetyChecks)
		{
			AddOption(stringBuilder, GetOption("global-safety-checks-setting=", GlobalSafetyChecksSettingKind.On));
		}
		else
		{
			AddOption(stringBuilder, GetOption("global-safety-checks-setting=", GlobalSafetyChecksSettingKind.Off));
		}
		if (isJit && EnableBurstTimings)
		{
			AddOption(stringBuilder, GetOption("log-timings"));
		}
		if (EnableBurstDebug || (attr != null && attr.Debug))
		{
			AddOption(stringBuilder, GetOption("debugMode"));
		}
		return stringBuilder.ToString();
	}

	private static void AddOption(StringBuilder builder, string option)
	{
		if (builder.Length != 0)
		{
			builder.Append('\n');
		}
		builder.Append(option);
	}

	internal static string GetOption(string optionName, object value = null)
	{
		if (optionName == null)
		{
			throw new ArgumentNullException("optionName");
		}
		return "--" + optionName + (value ?? string.Empty);
	}

	private void OnOptionsChanged()
	{
		OptionsChanged?.Invoke();
	}

	private void MaybeTriggerRecompilation()
	{
	}

	private void MaybePreventChangingOption()
	{
	}

	static BurstCompilerOptions()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		foreach (string text in commandLineArgs)
		{
			if (!(text == "--burst-disable-compilation"))
			{
				if (text == "--burst-force-sync-compilation")
				{
					ForceBurstCompilationSynchronously = true;
				}
			}
			else
			{
				ForceDisableBurstCompilation = true;
			}
		}
		if (CheckIsSecondaryUnityProcess())
		{
			ForceDisableBurstCompilation = true;
			IsSecondaryUnityProcess = true;
		}
	}

	private static bool CheckIsSecondaryUnityProcess()
	{
		return false;
	}
}
