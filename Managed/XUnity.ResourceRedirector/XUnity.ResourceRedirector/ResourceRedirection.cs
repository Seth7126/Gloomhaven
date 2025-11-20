using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;
using XUnity.ResourceRedirector.Hooks;
using XUnity.ResourceRedirector.Properties;

namespace XUnity.ResourceRedirector;

public static class ResourceRedirection
{
	private static readonly List<PrioritizedCallback<Action<AssetLoadedContext>>> PostfixRedirectionsForAssetsPerCall = new List<PrioritizedCallback<Action<AssetLoadedContext>>>();

	private static readonly List<PrioritizedCallback<Action<AssetLoadedContext>>> PostfixRedirectionsForAssetsPerResource = new List<PrioritizedCallback<Action<AssetLoadedContext>>>();

	private static readonly List<PrioritizedCallback<Action<ResourceLoadedContext>>> PostfixRedirectionsForResourcesPerCall = new List<PrioritizedCallback<Action<ResourceLoadedContext>>>();

	private static readonly List<PrioritizedCallback<Action<ResourceLoadedContext>>> PostfixRedirectionsForResourcesPerResource = new List<PrioritizedCallback<Action<ResourceLoadedContext>>>();

	private static readonly List<PrioritizedCallback<Delegate>> PrefixRedirectionsForAssetsPerCall = new List<PrioritizedCallback<Delegate>>();

	private static readonly List<PrioritizedCallback<Delegate>> PrefixRedirectionsForAsyncAssetsPerCall = new List<PrioritizedCallback<Delegate>>();

	private static readonly List<PrioritizedCallback<Delegate>> PrefixRedirectionsForAssetBundles = new List<PrioritizedCallback<Delegate>>();

	private static readonly List<PrioritizedCallback<Delegate>> PrefixRedirectionsForAsyncAssetBundles = new List<PrioritizedCallback<Delegate>>();

	private static readonly List<PrioritizedCallback<Action<AssetBundleLoadedContext>>> PostfixRedirectionsForAssetBundles = new List<PrioritizedCallback<Action<AssetBundleLoadedContext>>>();

	private static Action<AssetBundleLoadingContext> _emulateAssetBundles;

	private static Action<AsyncAssetBundleLoadingContext> _emulateAssetBundlesAsync;

	private static Action<AssetBundleLoadingContext> _redirectionMissingAssetBundlesToEmpty;

	private static Action<AsyncAssetBundleLoadingContext> _redirectionMissingAssetBundlesToEmptyAsync;

	private static bool _enabledRandomizeCabIfConflict = false;

	private static Action<AssetBundleLoadingContext> _enableCabRandomizationPrefix;

	private static Action<AsyncAssetBundleLoadingContext> _enableCabRandomizationPrefixAsync;

	private static Action<AssetBundleLoadedContext> _enableCabRandomizationPostfix;

	private static bool _initialized = false;

	private static bool _initializedSyncOverAsyncEnabled = false;

	private static bool _logAllLoadedResources = false;

	private static bool _isFiringAssetBundle;

	private static bool _isFiringResource;

	private static bool _isFiringAsset;

	private static bool _isRecursionDisabledPermanently;

	internal static bool RecursionEnabled = true;

	internal static bool SyncOverAsyncEnabled = false;

	public static bool LogAllLoadedResources
	{
		get
		{
			return _logAllLoadedResources;
		}
		set
		{
			if (value)
			{
				Initialize();
			}
			_logAllLoadedResources = value;
		}
	}

	public static bool LogCallbackOrder { get; set; }

	public static void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			HookingHelper.PatchAll(ResourceAndAssetHooks.GeneralHooks, forceExternHooks: false);
		}
	}

	public static void EnableSyncOverAsyncAssetLoads()
	{
		Initialize();
		if (!_initializedSyncOverAsyncEnabled)
		{
			_initializedSyncOverAsyncEnabled = true;
			SyncOverAsyncEnabled = true;
			HookingHelper.PatchAll(ResourceAndAssetHooks.SyncOverAsyncHooks, forceExternHooks: false);
		}
	}

	public static void DisableRecursionPermanently()
	{
		_isRecursionDisabledPermanently = true;
	}

	public static void EnableEmulateAssetBundles(int priority, string emulationDirectory)
	{
		if (_emulateAssetBundles == null && _emulateAssetBundlesAsync == null)
		{
			_emulateAssetBundles = delegate(AssetBundleLoadingContext ctx)
			{
				HandleAssetBundleEmulation<AssetBundleLoadingContext>(ctx, SetBundle);
			};
			_emulateAssetBundlesAsync = delegate(AsyncAssetBundleLoadingContext ctx)
			{
				HandleAssetBundleEmulation<AsyncAssetBundleLoadingContext>(ctx, SetRequest);
			};
			RegisterAssetBundleLoadingHook(priority, _emulateAssetBundles);
			RegisterAsyncAssetBundleLoadingHook(priority, _emulateAssetBundlesAsync);
		}
		void HandleAssetBundleEmulation<T>(T context, Action<T, string> changeBundle) where T : IAssetBundleLoadingContext
		{
			if (context.Parameters.LoadType == AssetBundleLoadType.LoadFromFile)
			{
				string normalizedPath = context.GetNormalizedPath();
				string text = Path.Combine(emulationDirectory, normalizedPath);
				if (File.Exists(text))
				{
					changeBundle(context, text);
					context.Complete(skipRemainingPrefixes: true, true);
					XuaLogger.ResourceRedirector.Debug("Redirected asset bundle: '" + context.Parameters.Path + "' => '" + text + "'");
				}
			}
		}
		static void SetBundle(AssetBundleLoadingContext context, string path)
		{
			context.Bundle = AssetBundle.LoadFromFile(path, context.Parameters.Crc, context.Parameters.Offset);
		}
		static void SetRequest(AsyncAssetBundleLoadingContext context, string path)
		{
			context.Request = AssetBundle.LoadFromFileAsync(path, context.Parameters.Crc, context.Parameters.Offset);
		}
	}

	public static void DisableEmulateAssetBundles()
	{
		if (_emulateAssetBundles != null && _emulateAssetBundlesAsync != null)
		{
			UnregisterAssetBundleLoadingHook(_emulateAssetBundles);
			UnregisterAsyncAssetBundleLoadingHook(_emulateAssetBundlesAsync);
			_emulateAssetBundles = null;
			_emulateAssetBundlesAsync = null;
		}
	}

	public static void EnableRedirectMissingAssetBundlesToEmptyAssetBundle(int priority)
	{
		if (_redirectionMissingAssetBundlesToEmpty == null && _redirectionMissingAssetBundlesToEmptyAsync == null)
		{
			_redirectionMissingAssetBundlesToEmpty = delegate(AssetBundleLoadingContext ctx)
			{
				HandleMissingBundle<AssetBundleLoadingContext>(ctx, SetBundle);
			};
			_redirectionMissingAssetBundlesToEmptyAsync = delegate(AsyncAssetBundleLoadingContext ctx)
			{
				HandleMissingBundle<AsyncAssetBundleLoadingContext>(ctx, SetRequest);
			};
			RegisterAssetBundleLoadingHook(priority, _redirectionMissingAssetBundlesToEmpty);
			RegisterAsyncAssetBundleLoadingHook(priority, _redirectionMissingAssetBundlesToEmptyAsync);
		}
		static void HandleMissingBundle<TContext>(TContext context, Action<TContext, byte[]> changeBundle) where TContext : IAssetBundleLoadingContext
		{
			if (context.Parameters.LoadType == AssetBundleLoadType.LoadFromFile && !File.Exists(context.Parameters.Path))
			{
				byte[] empty = Resources.empty;
				CabHelper.RandomizeCab(empty);
				changeBundle(context, empty);
				context.Complete(skipRemainingPrefixes: true, true);
				XuaLogger.ResourceRedirector.Warn("Tried to load non-existing asset bundle: " + context.Parameters.Path);
			}
		}
		static void SetBundle(AssetBundleLoadingContext context, byte[] assetBundleData)
		{
			AssetBundle bundle = AssetBundle.LoadFromMemory(assetBundleData);
			context.Bundle = bundle;
		}
		static void SetRequest(AsyncAssetBundleLoadingContext context, byte[] assetBundleData)
		{
			AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(assetBundleData);
			context.Request = request;
		}
	}

	public static void EnableRandomizeCabIfConflict(int priority, bool forceRandomizeWhenInMemory)
	{
		if (_enabledRandomizeCabIfConflict)
		{
			return;
		}
		_enabledRandomizeCabIfConflict = true;
		if (forceRandomizeWhenInMemory)
		{
			_enableCabRandomizationPrefix = delegate(AssetBundleLoadingContext ctx)
			{
				HandleCabRandomizePrefix(ctx);
			};
			_enableCabRandomizationPrefixAsync = delegate(AsyncAssetBundleLoadingContext ctx)
			{
				HandleCabRandomizePrefix(ctx);
			};
			RegisterAssetBundleLoadingHook(priority, _enableCabRandomizationPrefix);
			RegisterAsyncAssetBundleLoadingHook(priority, _enableCabRandomizationPrefixAsync);
		}
		_enableCabRandomizationPostfix = delegate(AssetBundleLoadedContext ctx)
		{
			HandleCabRandomizePostfix(ctx);
		};
		RegisterAssetBundleLoadedHook(priority, _enableCabRandomizationPostfix);
		void HandleCabRandomizePostfix(AssetBundleLoadedContext context)
		{
			if (context.Parameters.LoadType == AssetBundleLoadType.LoadFromFile)
			{
				if ((Object)(object)context.Bundle == (Object)null && File.Exists(context.Parameters.Path))
				{
					XuaLogger.ResourceRedirector.Warn("The asset bundle '" + context.Parameters.Path + "' could not be loaded likely due to conflicting CAB-string. Retrying in-memory with randomized CAB-string.");
					byte[] array;
					using (FileStream fileStream = new FileStream(context.Parameters.Path, FileMode.Open, FileAccess.Read))
					{
						long length = fileStream.Length;
						long offset = (long)context.Parameters.Offset;
						long num = length - offset;
						fileStream.Seek(offset, SeekOrigin.Begin);
						array = fileStream.ReadFully((int)num);
					}
					if (!forceRandomizeWhenInMemory)
					{
						CabHelper.RandomizeCabWithAnyLength(array);
					}
					AssetBundle val = AssetBundle.LoadFromMemory(array, 0u);
					if ((Object)(object)val != (Object)null)
					{
						context.Bundle = val;
						context.Complete();
					}
				}
			}
			else if (context.Parameters.LoadType == AssetBundleLoadType.LoadFromMemory)
			{
				if ((Object)(object)context.Bundle == (Object)null && !forceRandomizeWhenInMemory)
				{
					string text = AssetBundleHelper.PathForLoadedInMemoryBundle ?? "Unnamed";
					XuaLogger.ResourceRedirector.Warn("Could not load an in-memory asset bundle (" + text + ") likely due to conflicting CAB-string. Retrying with randomized CAB-string.");
					CabHelper.RandomizeCabWithAnyLength(context.Parameters.Binary);
					AssetBundle val2 = AssetBundle.LoadFromMemory(context.Parameters.Binary, 0u);
					if ((Object)(object)val2 != (Object)null)
					{
						context.Bundle = val2;
						context.Complete();
					}
				}
			}
			else if (context.Parameters.LoadType == AssetBundleLoadType.LoadFromStream && (Object)(object)context.Bundle == (Object)null)
			{
				string text2 = AssetBundleHelper.PathForLoadedInMemoryBundle ?? "Unnamed";
				XuaLogger.ResourceRedirector.Warn("Could not load a stream asset bundle (" + text2 + ") likely due to conflicting CAB-string. Retrying with randomized CAB-string.");
				byte[] array2 = context.Parameters.Stream.ReadFully(0);
				if (!forceRandomizeWhenInMemory)
				{
					CabHelper.RandomizeCabWithAnyLength(array2);
				}
				AssetBundle val3 = AssetBundle.LoadFromMemory(array2, 0u);
				if ((Object)(object)val3 != (Object)null)
				{
					context.Bundle = val3;
					context.Complete();
				}
			}
		}
		static void HandleCabRandomizePrefix(IAssetBundleLoadingContext context)
		{
			if (context.Parameters.LoadType == AssetBundleLoadType.LoadFromMemory)
			{
				CabHelper.RandomizeCabWithAnyLength(context.Parameters.Binary);
			}
		}
	}

	public static void DisableRandomizeCabIfConflict()
	{
		if (_enabledRandomizeCabIfConflict)
		{
			_enabledRandomizeCabIfConflict = false;
			if (_enableCabRandomizationPrefix != null)
			{
				UnregisterAssetBundleLoadingHook(_enableCabRandomizationPrefix);
				_enableCabRandomizationPrefix = null;
			}
			if (_enableCabRandomizationPrefixAsync != null)
			{
				UnregisterAsyncAssetBundleLoadingHook(_enableCabRandomizationPrefixAsync);
				_enableCabRandomizationPrefixAsync = null;
			}
			if (_enableCabRandomizationPostfix != null)
			{
				UnregisterAssetBundleLoadedHook(_enableCabRandomizationPostfix);
				_enableCabRandomizationPostfix = null;
			}
		}
	}

	public static void DisableRedirectMissingAssetBundlesToEmptyAssetBundle()
	{
		if (_redirectionMissingAssetBundlesToEmpty != null && _redirectionMissingAssetBundlesToEmptyAsync != null)
		{
			UnregisterAssetBundleLoadingHook(_redirectionMissingAssetBundlesToEmpty);
			UnregisterAsyncAssetBundleLoadingHook(_redirectionMissingAssetBundlesToEmptyAsync);
			_redirectionMissingAssetBundlesToEmpty = null;
			_redirectionMissingAssetBundlesToEmptyAsync = null;
		}
	}

	internal static bool TryGetAssetBundleLoadInfo(AssetBundleRequest request, out AsyncAssetLoadInfo result)
	{
		result = request.GetExtensionData<AsyncAssetLoadInfo>();
		return result != null;
	}

	internal static bool TryGetAssetBundle(AssetBundleCreateRequest request, out AsyncAssetBundleLoadInfo result)
	{
		result = request.GetExtensionData<AsyncAssetBundleLoadInfo>();
		return result != null;
	}

	internal static bool ShouldBlockAsyncOperationMethods(AssetBundleRequest operation)
	{
		if (TryGetAssetBundleLoadInfo(operation, out var result))
		{
			return result.ResolveType == AsyncAssetLoadingResolve.ThroughAssets;
		}
		return false;
	}

	internal static bool ShouldBlockAsyncOperationMethods(AssetBundleCreateRequest operation)
	{
		if (TryGetAssetBundle(operation, out var result))
		{
			return result.ResolveType == AsyncAssetBundleLoadingResolve.ThroughBundle;
		}
		return false;
	}

	internal static bool ShouldBlockAsyncOperationMethods(AsyncOperation operation)
	{
		if (SyncOverAsyncEnabled)
		{
			if (!operation.TryCastTo<AssetBundleRequest>(out var castedObject) || !ShouldBlockAsyncOperationMethods(castedObject))
			{
				if (operation.TryCastTo<AssetBundleCreateRequest>(out var castedObject2))
				{
					return ShouldBlockAsyncOperationMethods(castedObject2);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	internal static AssetBundleLoadingContext Hook_AssetBundleLoading_Prefix(AssetBundleLoadingParameters parameters, out AssetBundle bundle)
	{
		AssetBundleLoadingContext assetBundleLoadingContext = new AssetBundleLoadingContext(parameters);
		if (_isFiringAssetBundle && (_isRecursionDisabledPermanently || !RecursionEnabled))
		{
			bundle = null;
			return assetBundleLoadingContext;
		}
		try
		{
			_isFiringAssetBundle = true;
			if (_logAllLoadedResources)
			{
				XuaLogger.ResourceRedirector.Debug("Loading Asset Bundle: (" + assetBundleLoadingContext.GetNormalizedPath() + ").");
			}
			List<PrioritizedCallback<Delegate>> prefixRedirectionsForAssetBundles = PrefixRedirectionsForAssetBundles;
			int count = prefixRedirectionsForAssetBundles.Count;
			for (int i = 0; i < count; i++)
			{
				PrioritizedCallback<Delegate> prioritizedCallback = prefixRedirectionsForAssetBundles[i];
				if (prioritizedCallback.IsBeingCalled)
				{
					continue;
				}
				try
				{
					prioritizedCallback.IsBeingCalled = true;
					if (prioritizedCallback.Callback is Action<AssetBundleLoadingContext> action)
					{
						action(assetBundleLoadingContext);
					}
					else if (prioritizedCallback.Callback is Action<IAssetBundleLoadingContext> action2)
					{
						action2(assetBundleLoadingContext);
					}
					if (assetBundleLoadingContext.SkipRemainingPrefixes)
					{
						break;
					}
				}
				catch (Exception e)
				{
					XuaLogger.ResourceRedirector.Error(e, "An error occurred while invoking AssetBundleLoading event.");
				}
				finally
				{
					RecursionEnabled = true;
					prioritizedCallback.IsBeingCalled = false;
				}
			}
		}
		catch (Exception e2)
		{
			XuaLogger.ResourceRedirector.Error(e2, "An error occurred while invoking AssetBundleLoading event.");
		}
		finally
		{
			_isFiringAssetBundle = false;
		}
		bundle = assetBundleLoadingContext.Bundle;
		return assetBundleLoadingContext;
	}

	internal static AssetBundleLoadedContext Hook_AssetBundleLoaded_Postfix(AssetBundleLoadingParameters parameters, ref AssetBundle bundle)
	{
		AssetBundleLoadedContext assetBundleLoadedContext = new AssetBundleLoadedContext(parameters, bundle);
		if (_isFiringAssetBundle && (_isRecursionDisabledPermanently || !RecursionEnabled))
		{
			bundle = null;
			return assetBundleLoadedContext;
		}
		try
		{
			_isFiringAssetBundle = true;
			List<PrioritizedCallback<Action<AssetBundleLoadedContext>>> postfixRedirectionsForAssetBundles = PostfixRedirectionsForAssetBundles;
			int count = postfixRedirectionsForAssetBundles.Count;
			for (int i = 0; i < count; i++)
			{
				PrioritizedCallback<Action<AssetBundleLoadedContext>> prioritizedCallback = postfixRedirectionsForAssetBundles[i];
				if (prioritizedCallback.IsBeingCalled)
				{
					continue;
				}
				try
				{
					prioritizedCallback.IsBeingCalled = true;
					prioritizedCallback.Callback(assetBundleLoadedContext);
					if (assetBundleLoadedContext.SkipRemainingPostfixes)
					{
						break;
					}
				}
				catch (Exception e)
				{
					XuaLogger.ResourceRedirector.Error(e, "An error occurred while invoking AssetBundleLoaded event.");
				}
				finally
				{
					RecursionEnabled = true;
					prioritizedCallback.IsBeingCalled = false;
				}
			}
		}
		catch (Exception e2)
		{
			XuaLogger.ResourceRedirector.Error(e2, "An error occurred while invoking AssetBundleLoaded event.");
		}
		finally
		{
			_isFiringAssetBundle = false;
		}
		bundle = assetBundleLoadedContext.Bundle;
		return assetBundleLoadedContext;
	}

	internal static AsyncAssetBundleLoadingContext Hook_AssetBundleLoading_Prefix(AssetBundleLoadingParameters parameters, out AssetBundleCreateRequest request)
	{
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		AsyncAssetBundleLoadingContext asyncAssetBundleLoadingContext = new AsyncAssetBundleLoadingContext(parameters);
		if (_isFiringAssetBundle && (_isRecursionDisabledPermanently || !RecursionEnabled))
		{
			request = null;
			return asyncAssetBundleLoadingContext;
		}
		try
		{
			_isFiringAssetBundle = true;
			if (_logAllLoadedResources)
			{
				XuaLogger.ResourceRedirector.Debug("Loading Asset Bundle (async): (" + asyncAssetBundleLoadingContext.GetNormalizedPath() + ").");
			}
			List<PrioritizedCallback<Delegate>> prefixRedirectionsForAsyncAssetBundles = PrefixRedirectionsForAsyncAssetBundles;
			int count = prefixRedirectionsForAsyncAssetBundles.Count;
			for (int i = 0; i < count; i++)
			{
				PrioritizedCallback<Delegate> prioritizedCallback = prefixRedirectionsForAsyncAssetBundles[i];
				if (prioritizedCallback.IsBeingCalled)
				{
					continue;
				}
				try
				{
					prioritizedCallback.IsBeingCalled = true;
					if (prioritizedCallback.Callback is Action<AsyncAssetBundleLoadingContext> action)
					{
						action(asyncAssetBundleLoadingContext);
					}
					else if (prioritizedCallback.Callback is Action<IAssetBundleLoadingContext> action2)
					{
						action2(asyncAssetBundleLoadingContext);
					}
					if (asyncAssetBundleLoadingContext.SkipRemainingPrefixes)
					{
						break;
					}
				}
				catch (Exception e)
				{
					XuaLogger.ResourceRedirector.Error(e, "An error occurred while invoking AssetBundleLoading event.");
				}
				finally
				{
					RecursionEnabled = true;
					prioritizedCallback.IsBeingCalled = false;
				}
			}
		}
		catch (Exception e2)
		{
			XuaLogger.ResourceRedirector.Error(e2, "An error occurred while invoking AsyncAssetBundleLoading event.");
		}
		finally
		{
			_isFiringAssetBundle = false;
		}
		if (asyncAssetBundleLoadingContext.ResolveType == AsyncAssetBundleLoadingResolve.ThroughRequest)
		{
			request = asyncAssetBundleLoadingContext.Request;
		}
		else
		{
			if (asyncAssetBundleLoadingContext.ResolveType != AsyncAssetBundleLoadingResolve.ThroughBundle)
			{
				throw new InvalidOperationException("Found invalid ResolveType on context: " + asyncAssetBundleLoadingContext.ResolveType);
			}
			request = new AssetBundleCreateRequest();
			if (!asyncAssetBundleLoadingContext.SkipOriginalCall)
			{
				XuaLogger.ResourceRedirector.Warn("Resolving sync over async asset load, but 'SkipOriginalCall' was not set to true. Forcing it to true.");
				asyncAssetBundleLoadingContext.SkipOriginalCall = true;
			}
		}
		return asyncAssetBundleLoadingContext;
	}

	internal static void Hook_AssetBundleLoading_Postfix(AsyncAssetBundleLoadingContext context, AssetBundleCreateRequest request)
	{
		request?.SetExtensionData(new AsyncAssetBundleLoadInfo(context.Parameters, context.Bundle, context.SkipAllPostfixes, context.ResolveType));
	}

	internal static void Hook_AssetLoading_Postfix(AsyncAssetLoadingContext context, AssetBundleRequest request)
	{
		request?.SetExtensionData(new AsyncAssetLoadInfo(context.Parameters, context.Bundle, context.SkipAllPostfixes, context.ResolveType, context.Assets));
	}

	internal static AssetLoadingContext Hook_AssetLoading_Prefix(AssetLoadingParameters parameters, AssetBundle parentBundle, ref Object asset)
	{
		Object[] assets = null;
		AssetLoadingContext result = Hook_AssetLoading_Prefix(parameters, parentBundle, ref assets);
		if (assets == null || assets.Length == 0)
		{
			asset = null;
			return result;
		}
		if (assets.Length > 1)
		{
			XuaLogger.ResourceRedirector.Warn("Illegal behaviour by redirection handler in AssetLoadeding event. Returned more than one asset to call requiring only a single asset.");
			asset = assets[0];
			return result;
		}
		if (assets.Length == 1)
		{
			asset = assets[0];
		}
		return result;
	}

	internal static AssetLoadingContext Hook_AssetLoading_Prefix(AssetLoadingParameters parameters, AssetBundle bundle, ref Object[] assets)
	{
		AssetLoadingContext assetLoadingContext = new AssetLoadingContext(parameters, bundle);
		try
		{
			if (_isFiringAsset && (_isRecursionDisabledPermanently || !RecursionEnabled))
			{
				return assetLoadingContext;
			}
			_isFiringAsset = true;
			List<PrioritizedCallback<Delegate>> prefixRedirectionsForAssetsPerCall = PrefixRedirectionsForAssetsPerCall;
			int count = prefixRedirectionsForAssetsPerCall.Count;
			for (int i = 0; i < count; i++)
			{
				PrioritizedCallback<Delegate> prioritizedCallback = prefixRedirectionsForAssetsPerCall[i];
				if (prioritizedCallback.IsBeingCalled)
				{
					continue;
				}
				try
				{
					prioritizedCallback.IsBeingCalled = true;
					if (prioritizedCallback.Callback is Action<AssetLoadingContext> action)
					{
						action(assetLoadingContext);
					}
					else if (prioritizedCallback.Callback is Action<IAssetLoadingContext> action2)
					{
						action2(assetLoadingContext);
					}
					if (assetLoadingContext.SkipRemainingPrefixes)
					{
						break;
					}
				}
				catch (Exception e)
				{
					XuaLogger.ResourceRedirector.Error(e, "An error occurred while invoking AssetLoading event.");
				}
				finally
				{
					RecursionEnabled = true;
					prioritizedCallback.IsBeingCalled = false;
				}
			}
			assets = assetLoadingContext.Assets;
		}
		catch (Exception e2)
		{
			XuaLogger.ResourceRedirector.Error(e2, "An error occurred while invoking AssetLoading event.");
		}
		finally
		{
			_isFiringAsset = false;
		}
		return assetLoadingContext;
	}

	internal static AsyncAssetLoadingContext Hook_AsyncAssetLoading_Prefix(AssetLoadingParameters parameters, AssetBundle bundle, ref AssetBundleRequest request)
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		AsyncAssetLoadingContext asyncAssetLoadingContext = new AsyncAssetLoadingContext(parameters, bundle);
		try
		{
			if (_isFiringAsset && (_isRecursionDisabledPermanently || !RecursionEnabled))
			{
				return asyncAssetLoadingContext;
			}
			_isFiringAsset = true;
			List<PrioritizedCallback<Delegate>> prefixRedirectionsForAsyncAssetsPerCall = PrefixRedirectionsForAsyncAssetsPerCall;
			int count = prefixRedirectionsForAsyncAssetsPerCall.Count;
			for (int i = 0; i < count; i++)
			{
				PrioritizedCallback<Delegate> prioritizedCallback = prefixRedirectionsForAsyncAssetsPerCall[i];
				if (prioritizedCallback.IsBeingCalled)
				{
					continue;
				}
				try
				{
					prioritizedCallback.IsBeingCalled = true;
					if (prioritizedCallback.Callback is Action<AsyncAssetLoadingContext> action)
					{
						action(asyncAssetLoadingContext);
					}
					else if (prioritizedCallback.Callback is Action<IAssetLoadingContext> action2)
					{
						action2(asyncAssetLoadingContext);
					}
					if (asyncAssetLoadingContext.SkipRemainingPrefixes)
					{
						break;
					}
				}
				catch (Exception e)
				{
					XuaLogger.ResourceRedirector.Error(e, "An error occurred while invoking AsyncAssetLoading event.");
				}
				finally
				{
					RecursionEnabled = true;
					prioritizedCallback.IsBeingCalled = false;
				}
			}
			if (asyncAssetLoadingContext.ResolveType == AsyncAssetLoadingResolve.ThroughRequest)
			{
				request = asyncAssetLoadingContext.Request;
			}
			else
			{
				if (asyncAssetLoadingContext.ResolveType != AsyncAssetLoadingResolve.ThroughAssets)
				{
					throw new InvalidOperationException("Found invalid ResolveType on context: " + asyncAssetLoadingContext.ResolveType);
				}
				request = new AssetBundleRequest();
				if (!asyncAssetLoadingContext.SkipOriginalCall)
				{
					XuaLogger.ResourceRedirector.Warn("Resolving sync over async asset load, but 'SkipOriginalCall' was not set to true. Forcing it to true.");
					asyncAssetLoadingContext.SkipOriginalCall = true;
				}
			}
		}
		catch (Exception e2)
		{
			XuaLogger.ResourceRedirector.Error(e2, "An error occurred while invoking AsyncAssetLoading event.");
		}
		finally
		{
			_isFiringAsset = false;
		}
		return asyncAssetLoadingContext;
	}

	internal static void Hook_AssetLoaded_Postfix(AssetLoadingParameters parameters, AssetBundle parentBundle, ref Object asset)
	{
		Object[] assets = (Object[])(object)((!(asset == (Object)null)) ? new Object[1] { asset } : new Object[0]);
		Hook_AssetLoaded_Postfix(parameters, parentBundle, ref assets);
		if (assets == null || assets.Length == 0)
		{
			asset = null;
		}
		else if (assets.Length > 1)
		{
			XuaLogger.ResourceRedirector.Warn("Illegal behaviour by redirection handler in AssetLoaded event. Returned more than one asset to call requiring only a single asset.");
			asset = assets[0];
		}
		else if (assets.Length == 1)
		{
			asset = assets[0];
		}
	}

	internal static void Hook_AssetLoaded_Postfix(AssetLoadingParameters parameters, AssetBundle bundle, ref Object[] assets)
	{
		FireAssetLoadedEvent(parameters.ToAssetLoadedParameters(), bundle, ref assets);
	}

	internal static void Hook_ResourceLoaded_Postfix(ResourceLoadedParameters parameters, ref Object asset)
	{
		Object[] assets = (Object[])(object)((!(asset == (Object)null)) ? new Object[1] { asset } : new Object[0]);
		Hook_ResourceLoaded_Postfix(parameters, ref assets);
		if (assets == null || assets.Length == 0)
		{
			asset = null;
		}
		else if (assets.Length > 1)
		{
			XuaLogger.ResourceRedirector.Warn("Illegal behaviour by redirection handler in ResourceLoaded event. Returned more than one asset to call requiring only a single asset.");
			asset = assets[0];
		}
		else if (assets.Length == 1)
		{
			asset = assets[0];
		}
	}

	internal static void Hook_ResourceLoaded_Postfix(ResourceLoadedParameters parameters, ref Object[] assets)
	{
		FireResourceLoadedEvent(parameters, ref assets);
	}

	internal static void FireAssetLoadedEvent(AssetLoadedParameters parameters, AssetBundle assetBundle, ref Object[] assets)
	{
		Object[] array = assets?.ToArray();
		try
		{
			AssetLoadedContext assetLoadedContext = new AssetLoadedContext(parameters, assetBundle, assets);
			if (_isFiringAsset && (_isRecursionDisabledPermanently || !RecursionEnabled))
			{
				return;
			}
			_isFiringAsset = true;
			if (_logAllLoadedResources && assets != null)
			{
				for (int i = 0; i < assets.Length; i++)
				{
					Object val = assets[i];
					if (val != (Object)null)
					{
						string uniqueFileSystemAssetPath = assetLoadedContext.GetUniqueFileSystemAssetPath(val);
						XuaLogger.ResourceRedirector.Debug("Loaded Asset: '" + val.GetUnityType().FullName + "', Load Type: '" + parameters.LoadType.ToString() + "', Unique Path: (" + uniqueFileSystemAssetPath + ").");
					}
				}
			}
			List<PrioritizedCallback<Action<AssetLoadedContext>>> postfixRedirectionsForAssetsPerCall = PostfixRedirectionsForAssetsPerCall;
			int count = postfixRedirectionsForAssetsPerCall.Count;
			for (int j = 0; j < count; j++)
			{
				PrioritizedCallback<Action<AssetLoadedContext>> prioritizedCallback = postfixRedirectionsForAssetsPerCall[j];
				if (prioritizedCallback.IsBeingCalled)
				{
					continue;
				}
				try
				{
					prioritizedCallback.IsBeingCalled = true;
					prioritizedCallback.Callback(assetLoadedContext);
					if (assetLoadedContext.SkipRemainingPostfixes)
					{
						break;
					}
				}
				catch (Exception e)
				{
					XuaLogger.ResourceRedirector.Error(e, "An error occurred while invoking AssetLoaded event.");
				}
				finally
				{
					RecursionEnabled = true;
					prioritizedCallback.IsBeingCalled = false;
				}
			}
			assets = assetLoadedContext.Assets;
			if (assetLoadedContext.SkipRemainingPostfixes || assets == null)
			{
				return;
			}
			int num = assets.Length;
			for (int k = 0; k < num; k++)
			{
				Object val2 = assets[k];
				if (val2 != (Object)null)
				{
					AssetLoadedContext assetLoadedContext2 = new AssetLoadedContext(parameters, assetBundle, val2);
					List<PrioritizedCallback<Action<AssetLoadedContext>>> postfixRedirectionsForAssetsPerResource = PostfixRedirectionsForAssetsPerResource;
					int count2 = postfixRedirectionsForAssetsPerResource.Count;
					for (int l = 0; l < count2; l++)
					{
						PrioritizedCallback<Action<AssetLoadedContext>> prioritizedCallback2 = postfixRedirectionsForAssetsPerResource[l];
						if (prioritizedCallback2.IsBeingCalled)
						{
							continue;
						}
						try
						{
							prioritizedCallback2.IsBeingCalled = true;
							prioritizedCallback2.Callback(assetLoadedContext2);
							if (assetLoadedContext2.Asset != (Object)null)
							{
								assets[k] = assetLoadedContext2.Asset;
							}
							else
							{
								XuaLogger.ResourceRedirector.Warn($"Illegal behaviour by redirection handler in AssetLoaded event. You must not remove an asset reference when hooking with behaviour {HookBehaviour.OneCallbackPerResourceLoaded}.");
							}
							if (assetLoadedContext2.SkipRemainingPostfixes)
							{
								break;
							}
						}
						catch (Exception e2)
						{
							XuaLogger.ResourceRedirector.Error(e2, "An error occurred while invoking AssetLoaded event.");
						}
						finally
						{
							RecursionEnabled = true;
							prioritizedCallback2.IsBeingCalled = false;
						}
					}
				}
				else
				{
					XuaLogger.ResourceRedirector.Error("Found unexpected null asset during AssetLoaded event.");
				}
			}
		}
		catch (Exception e3)
		{
			XuaLogger.ResourceRedirector.Error(e3, "An error occurred while invoking AssetLoaded event.");
		}
		finally
		{
			_isFiringAsset = false;
			if (array != null)
			{
				Object[] array2 = array;
				for (int m = 0; m < array2.Length; m++)
				{
					array2[m].GetOrCreateExtensionData<ResourceExtensionData>().HasBeenRedirected = true;
				}
			}
		}
	}

	internal static void FireResourceLoadedEvent(ResourceLoadedParameters parameters, ref Object[] assets)
	{
		Object[] array = assets?.ToArray();
		try
		{
			ResourceLoadedContext resourceLoadedContext = new ResourceLoadedContext(parameters, assets);
			if (_isFiringResource && (_isRecursionDisabledPermanently || !RecursionEnabled))
			{
				return;
			}
			_isFiringResource = true;
			if (_logAllLoadedResources && assets != null)
			{
				for (int i = 0; i < assets.Length; i++)
				{
					Object val = assets[i];
					if (val != (Object)null)
					{
						string uniqueFileSystemAssetPath = resourceLoadedContext.GetUniqueFileSystemAssetPath(val);
						XuaLogger.ResourceRedirector.Debug("Loaded Asset: '" + val.GetUnityType().FullName + "', Load Type: '" + parameters.LoadType.ToString() + "', Unique Path: (" + uniqueFileSystemAssetPath + ").");
					}
				}
			}
			List<PrioritizedCallback<Action<ResourceLoadedContext>>> postfixRedirectionsForResourcesPerCall = PostfixRedirectionsForResourcesPerCall;
			int count = postfixRedirectionsForResourcesPerCall.Count;
			for (int j = 0; j < count; j++)
			{
				PrioritizedCallback<Action<ResourceLoadedContext>> prioritizedCallback = postfixRedirectionsForResourcesPerCall[j];
				if (prioritizedCallback.IsBeingCalled)
				{
					continue;
				}
				try
				{
					prioritizedCallback.IsBeingCalled = true;
					prioritizedCallback.Callback(resourceLoadedContext);
					if (resourceLoadedContext.SkipRemainingPostfixes)
					{
						break;
					}
				}
				catch (Exception e)
				{
					XuaLogger.ResourceRedirector.Error(e, "An error occurred while invoking ResourceLoaded event.");
				}
				finally
				{
					RecursionEnabled = true;
					prioritizedCallback.IsBeingCalled = false;
				}
			}
			assets = resourceLoadedContext.Assets;
			if (resourceLoadedContext.SkipRemainingPostfixes || assets == null)
			{
				return;
			}
			int num = assets.Length;
			for (int k = 0; k < num; k++)
			{
				Object val2 = assets[k];
				if (val2 != (Object)null)
				{
					ResourceLoadedContext resourceLoadedContext2 = new ResourceLoadedContext(parameters, val2);
					List<PrioritizedCallback<Action<ResourceLoadedContext>>> postfixRedirectionsForResourcesPerResource = PostfixRedirectionsForResourcesPerResource;
					int count2 = postfixRedirectionsForResourcesPerResource.Count;
					for (int l = 0; l < count2; l++)
					{
						PrioritizedCallback<Action<ResourceLoadedContext>> prioritizedCallback2 = postfixRedirectionsForResourcesPerResource[l];
						if (prioritizedCallback2.IsBeingCalled)
						{
							continue;
						}
						try
						{
							prioritizedCallback2.IsBeingCalled = true;
							prioritizedCallback2.Callback(resourceLoadedContext2);
							if (resourceLoadedContext2.Asset != (Object)null)
							{
								assets[k] = resourceLoadedContext2.Asset;
							}
							else
							{
								XuaLogger.ResourceRedirector.Warn($"Illegal behaviour by redirection handler in ResourceLoaded event. You must not remove an asset reference when hooking with behaviour {HookBehaviour.OneCallbackPerResourceLoaded}.");
							}
							if (resourceLoadedContext2.SkipRemainingPostfixes)
							{
								break;
							}
						}
						catch (Exception e2)
						{
							XuaLogger.ResourceRedirector.Error(e2, "An error occurred while invoking ResourceLoaded event.");
						}
						finally
						{
							RecursionEnabled = true;
							prioritizedCallback2.IsBeingCalled = false;
						}
					}
				}
				else
				{
					XuaLogger.ResourceRedirector.Error("Found unexpected null asset during ResourceLoaded event.");
				}
			}
		}
		catch (Exception e3)
		{
			XuaLogger.ResourceRedirector.Error(e3, "An error occurred while invoking ResourceLoaded event.");
		}
		finally
		{
			_isFiringResource = false;
			if (array != null)
			{
				Object[] array2 = array;
				for (int m = 0; m < array2.Length; m++)
				{
					array2[m].GetOrCreateExtensionData<ResourceExtensionData>().HasBeenRedirected = true;
				}
			}
		}
	}

	private static void LogEventRegistration(string eventType, IEnumerable callbacks)
	{
		XuaLogger.ResourceRedirector.Debug("Registered new callback for " + eventType + ".");
		LogNewCallbackOrder(eventType, callbacks);
	}

	private static void LogEventUnregistration(string eventType, IEnumerable callbacks)
	{
		XuaLogger.ResourceRedirector.Debug("Unregistered callback for " + eventType + ".");
		LogNewCallbackOrder(eventType, callbacks);
	}

	private static void LogNewCallbackOrder(string eventType, IEnumerable callbacks)
	{
		if (!LogCallbackOrder)
		{
			return;
		}
		XuaLogger.ResourceRedirector.Debug("New callback order for " + eventType + ":");
		foreach (object callback in callbacks)
		{
			XuaLogger.ResourceRedirector.Debug(callback.ToString());
		}
	}

	public static void RegisterAssetBundleLoadedHook(int priority, Action<AssetBundleLoadedContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PrioritizedCallback<Action<AssetBundleLoadedContext>> item = PrioritizedCallback.Create(action, priority);
		if (PostfixRedirectionsForAssetBundles.Contains(item))
		{
			throw new ArgumentException("This callback has already been registered.", "action");
		}
		Initialize();
		PostfixRedirectionsForAssetBundles.BinarySearchInsert(item);
		LogEventRegistration("AssetBundleLoaded", PostfixRedirectionsForAssetBundles);
	}

	public static void RegisterAssetBundleLoadedHook(Action<AssetBundleLoadedContext> action)
	{
		RegisterAssetBundleLoadedHook(0, action);
	}

	public static void UnregisterAssetBundleLoadedHook(Action<AssetBundleLoadedContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PostfixRedirectionsForAssetBundles.RemoveAll((PrioritizedCallback<Action<AssetBundleLoadedContext>> x) => object.Equals(x.Callback, action));
		LogEventUnregistration("AssetBundleLoaded", PostfixRedirectionsForAssetBundles);
	}

	public static void RegisterAssetLoadingHook(int priority, Action<AssetLoadingContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PrioritizedCallback<Delegate> item = PrioritizedCallback.Create((Delegate)action, priority);
		if (PrefixRedirectionsForAssetsPerCall.Contains(item))
		{
			throw new ArgumentException("This callback has already been registered.", "action");
		}
		Initialize();
		PrefixRedirectionsForAssetsPerCall.BinarySearchInsert(item);
		LogEventRegistration("AssetLoading", PrefixRedirectionsForAssetsPerCall);
	}

	public static void RegisterAssetLoadingHook(Action<AssetLoadingContext> action)
	{
		RegisterAssetLoadingHook(0, action);
	}

	public static void UnregisterAssetLoadingHook(Action<AssetLoadingContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PrefixRedirectionsForAssetsPerCall.RemoveAll((PrioritizedCallback<Delegate> x) => object.Equals(x.Callback, action));
		LogEventUnregistration("AssetLoading", PrefixRedirectionsForAssetsPerCall);
	}

	public static void RegisterAsyncAssetLoadingHook(int priority, Action<AsyncAssetLoadingContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PrioritizedCallback<Delegate> item = PrioritizedCallback.Create((Delegate)action, priority);
		if (PrefixRedirectionsForAsyncAssetsPerCall.Contains(item))
		{
			throw new ArgumentException("This callback has already been registered.", "action");
		}
		Initialize();
		PrefixRedirectionsForAsyncAssetsPerCall.BinarySearchInsert(item);
		LogEventRegistration("AsyncAssetLoading", PrefixRedirectionsForAsyncAssetsPerCall);
	}

	public static void RegisterAsyncAssetLoadingHook(Action<AsyncAssetLoadingContext> action)
	{
		RegisterAsyncAssetLoadingHook(0, action);
	}

	public static void UnregisterAsyncAssetLoadingHook(Action<AsyncAssetLoadingContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PrefixRedirectionsForAsyncAssetsPerCall.RemoveAll((PrioritizedCallback<Delegate> x) => object.Equals(x.Callback, action));
		LogEventUnregistration("AsyncAssetLoading", PrefixRedirectionsForAsyncAssetsPerCall);
	}

	public static void RegisterAsyncAndSyncAssetLoadingHook(int priority, Action<IAssetLoadingContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PrioritizedCallback<Delegate> item = PrioritizedCallback.Create((Delegate)action, priority);
		if (PrefixRedirectionsForAsyncAssetsPerCall.Contains(item))
		{
			throw new ArgumentException("This callback has already been registered.", "action");
		}
		Initialize();
		PrefixRedirectionsForAsyncAssetsPerCall.BinarySearchInsert(item);
		LogEventRegistration("AsyncAssetLoading", PrefixRedirectionsForAsyncAssetsPerCall);
		PrefixRedirectionsForAssetsPerCall.BinarySearchInsert(item);
		LogEventRegistration("AssetLoading", PrefixRedirectionsForAssetsPerCall);
	}

	public static void RegisterAsyncAndSyncAssetLoadingHook(Action<IAssetLoadingContext> action)
	{
		RegisterAsyncAndSyncAssetLoadingHook(0, action);
	}

	public static void UnregisterAsyncAndSyncAssetLoadingHook(Action<IAssetLoadingContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PrefixRedirectionsForAsyncAssetsPerCall.RemoveAll((PrioritizedCallback<Delegate> x) => object.Equals(x.Callback, action));
		LogEventUnregistration("AsyncAssetLoading", PrefixRedirectionsForAsyncAssetsPerCall);
		PrefixRedirectionsForAssetsPerCall.RemoveAll((PrioritizedCallback<Delegate> x) => object.Equals(x.Callback, action));
		LogEventUnregistration("AssetLoading", PrefixRedirectionsForAssetsPerCall);
	}

	public static void RegisterAssetLoadedHook(HookBehaviour behaviour, int priority, Action<AssetLoadedContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PrioritizedCallback<Action<AssetLoadedContext>> item = PrioritizedCallback.Create(action, priority);
		if (PostfixRedirectionsForAssetsPerCall.Contains(item) || PostfixRedirectionsForAssetsPerResource.Contains(item))
		{
			throw new ArgumentException("This callback has already been registered.", "action");
		}
		Initialize();
		switch (behaviour)
		{
		case HookBehaviour.OneCallbackPerLoadCall:
			PostfixRedirectionsForAssetsPerCall.BinarySearchInsert(item);
			LogEventRegistration("AssetLoaded (" + behaviour.ToString() + ")", PostfixRedirectionsForAssetsPerCall);
			break;
		case HookBehaviour.OneCallbackPerResourceLoaded:
			PostfixRedirectionsForAssetsPerResource.BinarySearchInsert(item);
			LogEventRegistration("AssetLoaded (" + behaviour.ToString() + ")", PostfixRedirectionsForAssetsPerResource);
			break;
		}
	}

	public static void RegisterAssetLoadedHook(HookBehaviour behaviour, Action<AssetLoadedContext> action)
	{
		RegisterAssetLoadedHook(behaviour, 0, action);
	}

	public static void UnregisterAssetLoadedHook(Action<AssetLoadedContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (PostfixRedirectionsForAssetsPerCall.RemoveAll((PrioritizedCallback<Action<AssetLoadedContext>> x) => x.Callback == action) > 0)
		{
			LogEventRegistration("AssetLoaded (" + HookBehaviour.OneCallbackPerLoadCall.ToString() + ")", PostfixRedirectionsForAssetsPerCall);
		}
		if (PostfixRedirectionsForAssetsPerResource.RemoveAll((PrioritizedCallback<Action<AssetLoadedContext>> x) => x.Callback == action) > 0)
		{
			LogEventRegistration("AssetLoaded (" + HookBehaviour.OneCallbackPerResourceLoaded.ToString() + ")", PostfixRedirectionsForAssetsPerResource);
		}
	}

	public static void RegisterAssetBundleLoadingHook(int priority, Action<AssetBundleLoadingContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PrioritizedCallback<Delegate> item = PrioritizedCallback.Create((Delegate)action, priority);
		if (PrefixRedirectionsForAssetBundles.Contains(item))
		{
			throw new ArgumentException("This callback has already been registered.", "action");
		}
		Initialize();
		PrefixRedirectionsForAssetBundles.BinarySearchInsert(item);
		LogEventRegistration("AssetBundleLoading", PrefixRedirectionsForAssetBundles);
	}

	public static void RegisterAssetBundleLoadingHook(Action<AssetBundleLoadingContext> action)
	{
		RegisterAssetBundleLoadingHook(0, action);
	}

	public static void UnregisterAssetBundleLoadingHook(Action<AssetBundleLoadingContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PrefixRedirectionsForAssetBundles.RemoveAll((PrioritizedCallback<Delegate> x) => object.Equals(x.Callback, action));
		LogEventUnregistration("AssetBundleLoading", PrefixRedirectionsForAssetBundles);
	}

	public static void RegisterAsyncAssetBundleLoadingHook(int priority, Action<AsyncAssetBundleLoadingContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PrioritizedCallback<Delegate> item = PrioritizedCallback.Create((Delegate)action, priority);
		if (PrefixRedirectionsForAsyncAssetBundles.Contains(item))
		{
			throw new ArgumentException("This callback has already been registered.", "action");
		}
		Initialize();
		PrefixRedirectionsForAsyncAssetBundles.BinarySearchInsert(item);
		LogEventRegistration("AsyncAssetBundleLoading", PrefixRedirectionsForAsyncAssetBundles);
	}

	public static void RegisterAsyncAssetBundleLoadingHook(Action<AsyncAssetBundleLoadingContext> action)
	{
		RegisterAsyncAssetBundleLoadingHook(0, action);
	}

	public static void UnregisterAsyncAssetBundleLoadingHook(Action<AsyncAssetBundleLoadingContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PrefixRedirectionsForAsyncAssetBundles.RemoveAll((PrioritizedCallback<Delegate> x) => object.Equals(x.Callback, action));
		LogEventUnregistration("AsyncAssetBundleLoading", PrefixRedirectionsForAsyncAssetBundles);
	}

	public static void RegisterAsyncAndSyncAssetBundleLoadingHook(int priority, Action<IAssetBundleLoadingContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PrioritizedCallback<Delegate> item = PrioritizedCallback.Create((Delegate)action, priority);
		if (PrefixRedirectionsForAssetBundles.Contains(item))
		{
			throw new ArgumentException("This callback has already been registered.", "action");
		}
		Initialize();
		PrefixRedirectionsForAssetBundles.BinarySearchInsert(item);
		LogEventRegistration("AssetBundleLoading", PrefixRedirectionsForAssetBundles);
		PrefixRedirectionsForAsyncAssetBundles.BinarySearchInsert(item);
		LogEventRegistration("AsyncAssetBundleLoading", PrefixRedirectionsForAsyncAssetBundles);
	}

	public static void RegisterAsyncAndSyncAssetBundleLoadingHook(Action<IAssetBundleLoadingContext> action)
	{
		RegisterAsyncAndSyncAssetBundleLoadingHook(0, action);
	}

	public static void UnregisterAsyncAndSyncAssetBundleLoadingHook(Action<IAssetBundleLoadingContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PrefixRedirectionsForAssetBundles.RemoveAll((PrioritizedCallback<Delegate> x) => object.Equals(x.Callback, action));
		LogEventUnregistration("AssetBundleLoading", PrefixRedirectionsForAssetBundles);
		PrefixRedirectionsForAsyncAssetBundles.RemoveAll((PrioritizedCallback<Delegate> x) => object.Equals(x.Callback, action));
		LogEventUnregistration("AsyncAssetBundleLoading", PrefixRedirectionsForAsyncAssetBundles);
	}

	public static void RegisterResourceLoadedHook(HookBehaviour behaviour, int priority, Action<ResourceLoadedContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		PrioritizedCallback<Action<ResourceLoadedContext>> item = PrioritizedCallback.Create(action, priority);
		if (PostfixRedirectionsForResourcesPerCall.Contains(item) || PostfixRedirectionsForResourcesPerResource.Contains(item))
		{
			throw new ArgumentException("This callback has already been registered.", "action");
		}
		Initialize();
		switch (behaviour)
		{
		case HookBehaviour.OneCallbackPerLoadCall:
			PostfixRedirectionsForResourcesPerCall.BinarySearchInsert(item);
			LogEventRegistration("ResourceLoaded (" + behaviour.ToString() + ")", PostfixRedirectionsForResourcesPerCall);
			break;
		case HookBehaviour.OneCallbackPerResourceLoaded:
			PostfixRedirectionsForResourcesPerResource.BinarySearchInsert(item);
			LogEventRegistration("ResourceLoaded (" + behaviour.ToString() + ")", PostfixRedirectionsForResourcesPerResource);
			break;
		}
	}

	public static void RegisterResourceLoadedHook(HookBehaviour behaviour, Action<ResourceLoadedContext> action)
	{
		RegisterResourceLoadedHook(behaviour, 0, action);
	}

	public static void UnregisterResourceLoadedHook(Action<ResourceLoadedContext> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (PostfixRedirectionsForResourcesPerCall.RemoveAll((PrioritizedCallback<Action<ResourceLoadedContext>> x) => x.Callback == action) > 0)
		{
			LogEventRegistration("ResourceLoaded (" + HookBehaviour.OneCallbackPerLoadCall.ToString() + ")", PostfixRedirectionsForResourcesPerCall);
		}
		if (PostfixRedirectionsForResourcesPerResource.RemoveAll((PrioritizedCallback<Action<ResourceLoadedContext>> x) => x.Callback == action) > 0)
		{
			LogEventRegistration("ResourceLoaded (" + HookBehaviour.OneCallbackPerResourceLoaded.ToString() + ")", PostfixRedirectionsForResourcesPerResource);
		}
	}
}
