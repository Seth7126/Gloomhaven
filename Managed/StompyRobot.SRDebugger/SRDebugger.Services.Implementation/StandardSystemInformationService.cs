using System;
using System.Collections.Generic;
using System.Linq;
using SRF;
using SRF.Service;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SRDebugger.Services.Implementation;

[Service(typeof(ISystemInformationService))]
public class StandardSystemInformationService : ISystemInformationService
{
	private readonly Dictionary<string, IList<InfoEntry>> _info = new Dictionary<string, IList<InfoEntry>>();

	public StandardSystemInformationService()
	{
		CreateDefaultSet();
	}

	public IEnumerable<string> GetCategories()
	{
		return _info.Keys;
	}

	public IList<InfoEntry> GetInfo(string category)
	{
		if (!_info.TryGetValue(category, out var value))
		{
			Debug.LogError("[SystemInformationService] Category not found: {0}".Fmt(category));
			return new InfoEntry[0];
		}
		return value;
	}

	public void Add(InfoEntry info, string category = "Default")
	{
		if (!_info.TryGetValue(category, out var value))
		{
			value = new List<InfoEntry>();
			_info.Add(category, value);
		}
		if (value.Any((InfoEntry p) => p.Title == info.Title))
		{
			throw new ArgumentException("An InfoEntry object with the same title already exists in that category.", "info");
		}
		value.Add(info);
	}

	public Dictionary<string, Dictionary<string, object>> CreateReport(bool includePrivate = false)
	{
		Dictionary<string, Dictionary<string, object>> dictionary = new Dictionary<string, Dictionary<string, object>>(_info.Count);
		foreach (KeyValuePair<string, IList<InfoEntry>> item in _info)
		{
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>(item.Value.Count);
			foreach (InfoEntry item2 in item.Value)
			{
				if (!item2.IsPrivate || includePrivate)
				{
					dictionary2.Add(item2.Title, item2.Value);
				}
			}
			dictionary.Add(item.Key, dictionary2);
		}
		return dictionary;
	}

	private void CreateDefaultSet()
	{
		_info.Add("System", new InfoEntry[7]
		{
			InfoEntry.Create("Operating System", SystemInfo.operatingSystem),
			InfoEntry.Create("Device Name", SystemInfo.deviceName, isPrivate: true),
			InfoEntry.Create("Device Type", SystemInfo.deviceType),
			InfoEntry.Create("Device Model", SystemInfo.deviceModel),
			InfoEntry.Create("CPU Type", SystemInfo.processorType),
			InfoEntry.Create("CPU Count", SystemInfo.processorCount),
			InfoEntry.Create("System Memory", SRFileUtil.GetBytesReadable((long)SystemInfo.systemMemorySize * 1024L * 1024))
		});
		if (SystemInfo.batteryStatus != BatteryStatus.Unknown)
		{
			_info.Add("Battery", new InfoEntry[2]
			{
				InfoEntry.Create("Status", SystemInfo.batteryStatus),
				InfoEntry.Create("Battery Level", SystemInfo.batteryLevel)
			});
		}
		_info.Add("Unity", new InfoEntry[12]
		{
			InfoEntry.Create("Version", Application.unityVersion),
			InfoEntry.Create("Debug", Debug.isDebugBuild),
			InfoEntry.Create("Unity Pro", Application.HasProLicense()),
			InfoEntry.Create("Genuine", "{0} ({1})".Fmt(Application.genuine ? "Yes" : "No", Application.genuineCheckAvailable ? "Trusted" : "Untrusted")),
			InfoEntry.Create("System Language", Application.systemLanguage),
			InfoEntry.Create("Platform", Application.platform),
			InfoEntry.Create("Install Mode", Application.installMode),
			InfoEntry.Create("Sandbox", Application.sandboxType),
			InfoEntry.Create("IL2CPP", "No"),
			InfoEntry.Create("Application Version", Application.version),
			InfoEntry.Create("Application Id", Application.identifier),
			InfoEntry.Create("SRDebugger Version", "1.12.1")
		});
		_info.Add("Display", new InfoEntry[5]
		{
			InfoEntry.Create("Resolution", () => Screen.width + "x" + Screen.height),
			InfoEntry.Create("DPI", () => Screen.dpi),
			InfoEntry.Create("Fullscreen", () => Screen.fullScreen),
			InfoEntry.Create("Fullscreen Mode", () => Screen.fullScreenMode),
			InfoEntry.Create("Orientation", () => Screen.orientation)
		});
		_info.Add("Runtime", new InfoEntry[4]
		{
			InfoEntry.Create("Play Time", () => Time.unscaledTime),
			InfoEntry.Create("Level Play Time", () => Time.timeSinceLevelLoad),
			InfoEntry.Create("Current Level", delegate
			{
				Scene activeScene = SceneManager.GetActiveScene();
				return "{0} (Index: {1})".Fmt(activeScene.name, activeScene.buildIndex);
			}),
			InfoEntry.Create("Quality Level", () => QualitySettings.names[QualitySettings.GetQualityLevel()] + " (" + QualitySettings.GetQualityLevel() + ")")
		});
		TextAsset textAsset = (TextAsset)Resources.Load("UnityCloudBuildManifest.json");
		Dictionary<string, object> dictionary = ((textAsset != null) ? (Json.Deserialize(textAsset.text) as Dictionary<string, object>) : null);
		if (dictionary != null)
		{
			List<InfoEntry> list = new List<InfoEntry>(dictionary.Count);
			foreach (KeyValuePair<string, object> item in dictionary)
			{
				if (item.Value != null)
				{
					string value = item.Value.ToString();
					list.Add(InfoEntry.Create(GetCloudManifestPrettyName(item.Key), value));
				}
			}
			_info.Add("Build", list);
		}
		_info.Add("Features", new InfoEntry[5]
		{
			InfoEntry.Create("Location", SystemInfo.supportsLocationService),
			InfoEntry.Create("Accelerometer", SystemInfo.supportsAccelerometer),
			InfoEntry.Create("Gyroscope", SystemInfo.supportsGyroscope),
			InfoEntry.Create("Vibration", SystemInfo.supportsVibration),
			InfoEntry.Create("Audio", SystemInfo.supportsAudio)
		});
		_info.Add("Graphics - Device", new InfoEntry[5]
		{
			InfoEntry.Create("Device Name", SystemInfo.graphicsDeviceName),
			InfoEntry.Create("Device Vendor", SystemInfo.graphicsDeviceVendor),
			InfoEntry.Create("Device Version", SystemInfo.graphicsDeviceVersion),
			InfoEntry.Create("Graphics Memory", SRFileUtil.GetBytesReadable((long)SystemInfo.graphicsMemorySize * 1024L * 1024)),
			InfoEntry.Create("Max Tex Size", SystemInfo.maxTextureSize)
		});
		_info.Add("Graphics - Features", new InfoEntry[25]
		{
			InfoEntry.Create("UV Starts at top", SystemInfo.graphicsUVStartsAtTop),
			InfoEntry.Create("Shader Level", SystemInfo.graphicsShaderLevel),
			InfoEntry.Create("Multi Threaded", SystemInfo.graphicsMultiThreaded),
			InfoEntry.Create("Hidden Service Removal (GPU)", SystemInfo.hasHiddenSurfaceRemovalOnGPU),
			InfoEntry.Create("Uniform Array Indexing (Fragment Shaders)", SystemInfo.hasDynamicUniformArrayIndexingInFragmentShaders),
			InfoEntry.Create("Shadows", SystemInfo.supportsShadows),
			InfoEntry.Create("Raw Depth Sampling (Shadows)", SystemInfo.supportsRawShadowDepthSampling),
			InfoEntry.Create("Motion Vectors", SystemInfo.supportsMotionVectors),
			InfoEntry.Create("Cubemaps", SystemInfo.supportsRenderToCubemap),
			InfoEntry.Create("Image Effects", SystemInfo.supportsImageEffects),
			InfoEntry.Create("3D Textures", SystemInfo.supports3DTextures),
			InfoEntry.Create("2D Array Textures", SystemInfo.supports2DArrayTextures),
			InfoEntry.Create("3D Render Textures", SystemInfo.supports3DRenderTextures),
			InfoEntry.Create("Cubemap Array Textures", SystemInfo.supportsCubemapArrayTextures),
			InfoEntry.Create("Copy Texture Support", SystemInfo.copyTextureSupport),
			InfoEntry.Create("Compute Shaders", SystemInfo.supportsComputeShaders),
			InfoEntry.Create("Instancing", SystemInfo.supportsInstancing),
			InfoEntry.Create("Hardware Quad Topology", SystemInfo.supportsHardwareQuadTopology),
			InfoEntry.Create("32-bit index buffer", SystemInfo.supports32bitsIndexBuffer),
			InfoEntry.Create("Sparse Textures", SystemInfo.supportsSparseTextures),
			InfoEntry.Create("Render Target Count", SystemInfo.supportedRenderTargetCount),
			InfoEntry.Create("Separated Render Targets Blend", SystemInfo.supportsSeparatedRenderTargetsBlend),
			InfoEntry.Create("Multisampled Textures", SystemInfo.supportsMultisampledTextures),
			InfoEntry.Create("Texture Wrap Mirror Once", SystemInfo.supportsTextureWrapMirrorOnce),
			InfoEntry.Create("Reversed Z Buffer", SystemInfo.usesReversedZBuffer)
		});
	}

	private static string GetCloudManifestPrettyName(string name)
	{
		return name switch
		{
			"scmCommitId" => "Commit", 
			"scmBranch" => "Branch", 
			"cloudBuildTargetName" => "Build Target", 
			"buildStartTime" => "Build Date", 
			_ => name.Substring(0, 1).ToUpper() + name.Substring(1), 
		};
	}
}
