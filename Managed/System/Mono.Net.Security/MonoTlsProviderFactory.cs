using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mono.Btls;
using Mono.Unity;

namespace Mono.Net.Security;

internal static class MonoTlsProviderFactory
{
	private static object locker = new object();

	private static bool initialized;

	private static MobileTlsProvider defaultProvider;

	private static Dictionary<string, Tuple<Guid, string>> providerRegistration;

	private static Dictionary<Guid, MobileTlsProvider> providerCache;

	private static bool enableDebug;

	internal static readonly Guid UnityTlsId = new Guid("06414A97-74F6-488F-877B-A6CA9BBEB82E");

	internal static readonly Guid AppleTlsId = new Guid("981af8af-a3a3-419a-9f01-a518e3a17c1c");

	internal static readonly Guid BtlsId = new Guid("432d18c9-9348-4b90-bfbf-9f2a10e1f15b");

	internal static bool IsInitialized
	{
		get
		{
			lock (locker)
			{
				return initialized;
			}
		}
	}

	internal static MobileTlsProvider GetProviderInternal()
	{
		lock (locker)
		{
			InitializeInternal();
			return defaultProvider;
		}
	}

	internal static void InitializeInternal()
	{
		lock (locker)
		{
			if (!initialized)
			{
				SystemDependencyProvider.Initialize();
				InitializeProviderRegistration();
				MobileTlsProvider mobileTlsProvider;
				try
				{
					mobileTlsProvider = CreateDefaultProviderImpl();
				}
				catch (Exception innerException)
				{
					throw new NotSupportedException("TLS Support not available.", innerException);
				}
				if (mobileTlsProvider == null)
				{
					throw new NotSupportedException("TLS Support not available.");
				}
				if (!providerCache.ContainsKey(mobileTlsProvider.ID))
				{
					providerCache.Add(mobileTlsProvider.ID, mobileTlsProvider);
				}
				defaultProvider = mobileTlsProvider;
				initialized = true;
			}
		}
	}

	internal static void InitializeInternal(string provider)
	{
		lock (locker)
		{
			if (initialized)
			{
				throw new NotSupportedException("TLS Subsystem already initialized.");
			}
			SystemDependencyProvider.Initialize();
			defaultProvider = LookupProvider(provider, throwOnError: true);
			initialized = true;
		}
	}

	private static Type LookupProviderType(string name, bool throwOnError)
	{
		lock (locker)
		{
			InitializeProviderRegistration();
			if (!providerRegistration.TryGetValue(name, out var value))
			{
				if (throwOnError)
				{
					throw new NotSupportedException($"No such TLS Provider: `{name}'.");
				}
				return null;
			}
			Type type = Type.GetType(value.Item2, throwOnError: false);
			if (type == null && throwOnError)
			{
				throw new NotSupportedException($"Could not find TLS Provider: `{value.Item2}'.");
			}
			return type;
		}
	}

	private static MobileTlsProvider LookupProvider(string name, bool throwOnError)
	{
		lock (locker)
		{
			InitializeProviderRegistration();
			if (!providerRegistration.TryGetValue(name, out var value))
			{
				if (throwOnError)
				{
					throw new NotSupportedException($"No such TLS Provider: `{name}'.");
				}
				return null;
			}
			if (providerCache.TryGetValue(value.Item1, out var value2))
			{
				return value2;
			}
			Type type = Type.GetType(value.Item2, throwOnError: false);
			if (type == null && throwOnError)
			{
				throw new NotSupportedException($"Could not find TLS Provider: `{value.Item2}'.");
			}
			try
			{
				value2 = (MobileTlsProvider)Activator.CreateInstance(type, nonPublic: true);
			}
			catch (Exception innerException)
			{
				throw new NotSupportedException($"Unable to instantiate TLS Provider `{type}'.", innerException);
			}
			if (value2 == null)
			{
				if (throwOnError)
				{
					throw new NotSupportedException($"No such TLS Provider: `{name}'.");
				}
				return null;
			}
			providerCache.Add(value.Item1, value2);
			return value2;
		}
	}

	[Conditional("MONO_TLS_DEBUG")]
	private static void InitializeDebug()
	{
		if (Environment.GetEnvironmentVariable("MONO_TLS_DEBUG") != null)
		{
			enableDebug = true;
		}
	}

	[Conditional("MONO_TLS_DEBUG")]
	internal static void Debug(string message, params object[] args)
	{
		if (enableDebug)
		{
			Console.Error.WriteLine(message, args);
		}
	}

	private static void InitializeProviderRegistration()
	{
		lock (locker)
		{
			if (providerRegistration == null)
			{
				providerRegistration = new Dictionary<string, Tuple<Guid, string>>();
				providerCache = new Dictionary<Guid, MobileTlsProvider>();
				if (UnityTls.IsSupported)
				{
					PopulateUnityProviders();
				}
				else
				{
					PopulateProviders();
				}
			}
		}
	}

	private static void PopulateUnityProviders()
	{
		Tuple<Guid, string> value = new Tuple<Guid, string>(UnityTlsId, "Mono.Unity.UnityTlsProvider");
		providerRegistration.Add("default", value);
		providerRegistration.Add("unitytls", value);
	}

	private static void PopulateProviders()
	{
		object obj = null;
		Tuple<Guid, string> tuple = null;
		if (IsBtlsSupported())
		{
			tuple = new Tuple<Guid, string>(BtlsId, typeof(MonoBtlsProvider).FullName);
			providerRegistration.Add("btls", tuple);
		}
		if (obj == null)
		{
			obj = tuple;
		}
		Tuple<Guid, string> tuple2 = (Tuple<Guid, string>)obj;
		if (tuple2 != null)
		{
			providerRegistration.Add("default", tuple2);
			providerRegistration.Add("legacy", tuple2);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool IsBtlsSupported();

	private static MobileTlsProvider CreateDefaultProviderImpl()
	{
		string text = Environment.GetEnvironmentVariable("MONO_TLS_PROVIDER");
		if (string.IsNullOrEmpty(text))
		{
			text = "default";
		}
		switch (text)
		{
		case "default":
		case "legacy":
			if (!UnityTls.IsSupported)
			{
				if (!IsBtlsSupported())
				{
					throw new NotSupportedException("TLS Support not available.");
				}
				goto case "btls";
			}
			goto case "unitytls";
		case "btls":
			return new MonoBtlsProvider();
		case "unitytls":
			return new UnityTlsProvider();
		default:
			return LookupProvider(text, throwOnError: true);
		}
	}

	internal static MobileTlsProvider GetProvider()
	{
		return GetProviderInternal();
	}

	internal static bool IsProviderSupported(string name)
	{
		lock (locker)
		{
			InitializeProviderRegistration();
			return providerRegistration.ContainsKey(name);
		}
	}

	internal static MobileTlsProvider GetProvider(string name)
	{
		return LookupProvider(name, throwOnError: false);
	}

	internal static void Initialize()
	{
		InitializeInternal();
	}

	internal static void Initialize(string provider)
	{
		InitializeInternal(provider);
	}
}
