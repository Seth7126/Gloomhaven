using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SM.Utils;
using UnityEngine;

namespace Gloomhaven;

[Serializable]
public class GraphicProfile : ISerializable
{
	protected bool OverridePixelLight;

	public int PixelLight;

	public TextureQualityRender TextureQuality;

	public AnisotropicFiltering AnisotropicTexture;

	public AntialiasingRender AntiAliasing;

	public bool SoftParticles;

	public bool RealtimeReflectionProbes;

	public ShadowQuality Shadows;

	public ShadowResolution ShadowResolution;

	public SkinWeights SkinWeights;

	public VSyncQuality VSync;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("PixelLight", PixelLight);
		info.AddValue("TextureQuality", TextureQuality);
		info.AddValue("AnisotropicTexture", AnisotropicTexture);
		info.AddValue("PPAA", AntiAliasing);
		info.AddValue("RealtimeReflectionProbes", RealtimeReflectionProbes);
		info.AddValue("Shadows", Shadows);
		info.AddValue("ShadowResolution", ShadowResolution);
		info.AddValue("SkinWeights", SkinWeights);
		info.AddValue("VSync", VSync);
		info.AddValue("SoftParticles", SoftParticles);
	}

	public GraphicProfile()
	{
	}

	public GraphicProfile(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "PixelLight":
					PixelLight = info.GetInt32("PixelLight");
					break;
				case "TextureQuality":
					TextureQuality = (TextureQualityRender)info.GetValue("TextureQuality", typeof(TextureQualityRender));
					if (!Enum.IsDefined(typeof(TextureQualityRender), TextureQuality))
					{
						TextureQuality = TextureQualityRender.EIGHTHEN;
					}
					break;
				case "SoftParticles":
					SoftParticles = info.GetBoolean("SoftParticles");
					break;
				case "RealtimeReflectionProbes":
					RealtimeReflectionProbes = info.GetBoolean("RealtimeReflectionProbes");
					break;
				case "AnisotropicTexture":
					AnisotropicTexture = (AnisotropicFiltering)info.GetValue("AnisotropicTexture", typeof(AnisotropicFiltering));
					break;
				case "PPAA":
					AntiAliasing = (AntialiasingRender)info.GetValue("PPAA", typeof(AntialiasingRender));
					break;
				case "Shadows":
					Shadows = (ShadowQuality)info.GetValue("Shadows", typeof(ShadowQuality));
					break;
				case "ShadowResolution":
					ShadowResolution = (ShadowResolution)info.GetValue("ShadowResolution", typeof(ShadowResolution));
					break;
				case "SkinWeights":
					SkinWeights = (SkinWeights)info.GetValue("SkinWeights", typeof(SkinWeights));
					break;
				case "VSync":
					VSync = (VSyncQuality)info.GetValue("VSync", typeof(VSyncQuality));
					break;
				case "AA":
					AntiAliasing = AntialiasingRender.TAA;
					break;
				}
			}
			catch (Exception ex)
			{
				LogUtils.LogError("Exception while trying to deserialize GraphicProfile entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public void Setup()
	{
		QualitySettings.anisotropicFiltering = AnisotropicTexture;
		QualitySettings.pixelLightCount = PixelLight;
		QualitySettings.shadowResolution = ShadowResolution;
		QualitySettings.shadows = Shadows;
		QualitySettings.realtimeReflectionProbes = RealtimeReflectionProbes;
		QualitySettings.softParticles = SoftParticles;
		QualitySettings.skinWeights = SkinWeights;
		QualitySettings.masterTextureLimit = (int)TextureQuality;
		QualitySettings.vSyncCount = (int)VSync;
		GraphicSettings.s_Antialiasing = AntiAliasing;
	}

	public void LoadCurrentValues(AntialiasingRender antialiasing)
	{
		AnisotropicTexture = QualitySettings.anisotropicFiltering;
		PixelLight = QualitySettings.pixelLightCount;
		ShadowResolution = QualitySettings.shadowResolution;
		Shadows = QualitySettings.shadows;
		RealtimeReflectionProbes = QualitySettings.realtimeReflectionProbes;
		SoftParticles = QualitySettings.softParticles;
		SkinWeights = QualitySettings.skinWeights;
		TextureQuality = Enum.GetValues(typeof(TextureQualityRender)).Cast<TextureQualityRender>().FirstOrDefault((TextureQualityRender it) => it == (TextureQualityRender)QualitySettings.masterTextureLimit);
		VSync = Enum.GetValues(typeof(VSyncQuality)).Cast<VSyncQuality>().First((VSyncQuality it) => it == (VSyncQuality)QualitySettings.vSyncCount);
		AntiAliasing = Enum.GetValues(typeof(AntialiasingRender)).Cast<AntialiasingRender>().First((AntialiasingRender it) => it == antialiasing);
	}
}
