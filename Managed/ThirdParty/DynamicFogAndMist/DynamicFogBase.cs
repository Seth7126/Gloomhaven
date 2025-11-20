using System;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicFogAndMist;

[AddComponentMenu("")]
public class DynamicFogBase : MonoBehaviour
{
	[SerializeField]
	private FOG_TYPE _effectType = FOG_TYPE.DesktopFogPlusWithSkyHaze;

	[SerializeField]
	private FOG_PRESET _preset = FOG_PRESET.Mist;

	[SerializeField]
	private DynamicFogProfile _profile;

	[SerializeField]
	private bool _profileSync;

	[SerializeField]
	private bool _useFogVolumes;

	[SerializeField]
	private bool _enableDithering;

	[SerializeField]
	[Range(0f, 0.3f)]
	private float _ditherStrength = 0.03f;

	[SerializeField]
	[Range(0f, 1f)]
	protected float _alpha = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _noiseStrength = 0.5f;

	[SerializeField]
	[Range(0.01f, 1f)]
	private float _noiseScale = 0.1f;

	[SerializeField]
	[Range(0f, 0.999f)]
	private float _distance = 0.1f;

	[SerializeField]
	[Range(0.0001f, 5f)]
	private float _distanceFallOff = 0.01f;

	[SerializeField]
	[Range(0f, 1.2f)]
	private float _maxDistance = 0.999f;

	[SerializeField]
	[Range(0.0001f, 0.5f)]
	private float _maxDistanceFallOff;

	[SerializeField]
	[Range(0f, 500f)]
	private float _height = 1f;

	[SerializeField]
	[Range(0f, 500f)]
	private float _maxHeight = 100f;

	[SerializeField]
	[Range(0.0001f, 1f)]
	private float _heightFallOff = 0.1f;

	[SerializeField]
	private float _baselineHeight;

	[SerializeField]
	private bool _clipUnderBaseline;

	[SerializeField]
	[Range(0f, 15f)]
	private float _turbulence = 0.1f;

	[SerializeField]
	[Range(0f, 5f)]
	private float _speed = 0.1f;

	[SerializeField]
	private Vector3 _windDirection = new Vector3(1f, 0f, 1f);

	[SerializeField]
	private Color _color = Color.white;

	[SerializeField]
	private Color _color2 = Color.gray;

	[SerializeField]
	[Range(0f, 500f)]
	private float _skyHaze = 50f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _skySpeed = 0.3f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _skyNoiseStrength = 0.1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _skyAlpha = 1f;

	[SerializeField]
	private GameObject _sun;

	[SerializeField]
	private bool _fogOfWarEnabled;

	[SerializeField]
	private Vector3 _fogOfWarCenter;

	[SerializeField]
	private Vector3 _fogOfWarSize = new Vector3(1024f, 0f, 1024f);

	[SerializeField]
	private int _fogOfWarTextureSize = 256;

	[SerializeField]
	protected bool _useSinglePassStereoRenderingMatrix;

	[SerializeField]
	private bool _useXZDistance;

	[SerializeField]
	[Range(0f, 1f)]
	private float _scattering = 0.7f;

	[SerializeField]
	private Color _scatteringColor = new Color(1f, 1f, 0.8f);

	private Material fogMatAdv;

	private Material fogMatFogSky;

	private Material fogMatOnlyFog;

	private Material fogMatVol;

	private Material fogMatSimple;

	private Material fogMatBasic;

	private Material fogMatOrthogonal;

	private Material fogMatDesktopPlusOrthogonal;

	[SerializeField]
	protected Material fogMat;

	private float initialFogAlpha;

	private float targetFogAlpha;

	private float initialSkyHazeAlpha;

	private float targetSkyHazeAlpha;

	private bool targetFogColors;

	private Color initialFogColor1;

	private Color targetFogColor1;

	private Color initialFogColor2;

	private Color targetFogColor2;

	private float transitionDuration;

	private float transitionStartTime;

	private float currentFogAlpha;

	private float currentSkyHazeAlpha;

	private bool transitionAlpha;

	private bool transitionColor;

	private bool transitionProfile;

	private DynamicFogProfile initialProfile;

	private DynamicFogProfile targetProfile;

	private Color currentFogColor1;

	private Color currentFogColor2;

	protected Camera currentCamera;

	private Texture2D fogOfWarTexture;

	private Color32[] fogOfWarColorBuffer;

	private Light sunLight;

	private Vector3 sunDirection = Vector3.zero;

	private Color sunColor = Color.white;

	private float sunIntensity = 1f;

	private static DynamicFog _fog;

	private List<string> shaderKeywords;

	protected bool matOrtho;

	protected bool shouldUpdateMaterialProperties;

	public FOG_TYPE effectType
	{
		get
		{
			return _effectType;
		}
		set
		{
			if (value != _effectType)
			{
				_effectType = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public FOG_PRESET preset
	{
		get
		{
			return _preset;
		}
		set
		{
			if (value != _preset)
			{
				_preset = value;
				UpdateMaterialPropertiesNow();
			}
		}
	}

	public DynamicFogProfile profile
	{
		get
		{
			return _profile;
		}
		set
		{
			if (value != _profile)
			{
				_profile = value;
				if (_profile != null)
				{
					_profile.Load(this);
					_preset = FOG_PRESET.Custom;
					UpdateMaterialProperties();
				}
			}
		}
	}

	public bool profileSync
	{
		get
		{
			return _profileSync;
		}
		set
		{
			if (value != _profileSync)
			{
				_profileSync = value;
			}
		}
	}

	public bool useFogVolumes
	{
		get
		{
			return _useFogVolumes;
		}
		set
		{
			if (value != _useFogVolumes)
			{
				_useFogVolumes = value;
			}
		}
	}

	public bool enableDithering
	{
		get
		{
			return _enableDithering;
		}
		set
		{
			if (value != _enableDithering)
			{
				_enableDithering = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float ditherStrength
	{
		get
		{
			return _ditherStrength;
		}
		set
		{
			if (value != _ditherStrength)
			{
				_ditherStrength = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float alpha
	{
		get
		{
			return _alpha;
		}
		set
		{
			if (value != _alpha)
			{
				_alpha = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public float noiseStrength
	{
		get
		{
			return _noiseStrength;
		}
		set
		{
			if (value != _noiseStrength)
			{
				_noiseStrength = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public float noiseScale
	{
		get
		{
			return _noiseScale;
		}
		set
		{
			if (value != _noiseScale)
			{
				_noiseScale = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public float distance
	{
		get
		{
			return _distance;
		}
		set
		{
			if (value != _distance)
			{
				_distance = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public float distanceFallOff
	{
		get
		{
			return _distanceFallOff;
		}
		set
		{
			if (value != _distanceFallOff)
			{
				_distanceFallOff = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public float maxDistance
	{
		get
		{
			return _maxDistance;
		}
		set
		{
			if (value != _maxDistance)
			{
				_maxDistance = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public float maxDistanceFallOff
	{
		get
		{
			return _maxDistanceFallOff;
		}
		set
		{
			if (value != _maxDistanceFallOff)
			{
				_maxDistanceFallOff = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public float height
	{
		get
		{
			return _height;
		}
		set
		{
			if (value != _height)
			{
				_height = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public float maxHeight
	{
		get
		{
			return _maxHeight;
		}
		set
		{
			if (value != _maxHeight)
			{
				_maxHeight = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public float heightFallOff
	{
		get
		{
			return _heightFallOff;
		}
		set
		{
			if (value != _heightFallOff)
			{
				_heightFallOff = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public float baselineHeight
	{
		get
		{
			return _baselineHeight;
		}
		set
		{
			if (value != _baselineHeight)
			{
				_baselineHeight = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool clipUnderBaseline
	{
		get
		{
			return _clipUnderBaseline;
		}
		set
		{
			if (value != _clipUnderBaseline)
			{
				_clipUnderBaseline = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float turbulence
	{
		get
		{
			return _turbulence;
		}
		set
		{
			if (value != _turbulence)
			{
				_turbulence = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public float speed
	{
		get
		{
			return _speed;
		}
		set
		{
			if (value != _speed)
			{
				_speed = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public Vector3 windDirection
	{
		get
		{
			return _windDirection;
		}
		set
		{
			if (value != _windDirection)
			{
				_windDirection = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color color
	{
		get
		{
			return _color;
		}
		set
		{
			if (value != _color)
			{
				_color = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public Color color2
	{
		get
		{
			return _color2;
		}
		set
		{
			if (value != _color2)
			{
				_color2 = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public float skyHaze
	{
		get
		{
			return _skyHaze;
		}
		set
		{
			if (value != _skyHaze)
			{
				_skyHaze = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public float skySpeed
	{
		get
		{
			return _skySpeed;
		}
		set
		{
			if (value != _skySpeed)
			{
				_skySpeed = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public float skyNoiseStrength
	{
		get
		{
			return _skyNoiseStrength;
		}
		set
		{
			if (value != _skyNoiseStrength)
			{
				_skyNoiseStrength = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public float skyAlpha
	{
		get
		{
			return _skyAlpha;
		}
		set
		{
			if (value != _skyAlpha)
			{
				_skyAlpha = value;
				_preset = FOG_PRESET.Custom;
				UpdateMaterialProperties();
			}
		}
	}

	public GameObject sun
	{
		get
		{
			return _sun;
		}
		set
		{
			if (value != _sun)
			{
				_sun = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool fogOfWarEnabled
	{
		get
		{
			return _fogOfWarEnabled;
		}
		set
		{
			if (value != _fogOfWarEnabled)
			{
				_fogOfWarEnabled = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Vector3 fogOfWarCenter
	{
		get
		{
			return _fogOfWarCenter;
		}
		set
		{
			if (value != _fogOfWarCenter)
			{
				_fogOfWarCenter = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Vector3 fogOfWarSize
	{
		get
		{
			return _fogOfWarSize;
		}
		set
		{
			if (value != _fogOfWarSize)
			{
				_fogOfWarSize = value;
				UpdateMaterialProperties();
			}
		}
	}

	public int fogOfWarTextureSize
	{
		get
		{
			return _fogOfWarTextureSize;
		}
		set
		{
			if (value != _fogOfWarTextureSize)
			{
				_fogOfWarTextureSize = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool useSinglePassStereoRenderingMatrix
	{
		get
		{
			return _useSinglePassStereoRenderingMatrix;
		}
		set
		{
			if (value != _useSinglePassStereoRenderingMatrix)
			{
				_useSinglePassStereoRenderingMatrix = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool useXZDistance
	{
		get
		{
			return _useXZDistance;
		}
		set
		{
			if (value != _useXZDistance)
			{
				_useXZDistance = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float scattering
	{
		get
		{
			return _scattering;
		}
		set
		{
			if (value != _scattering)
			{
				_scattering = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color scatteringColor
	{
		get
		{
			return _scatteringColor;
		}
		set
		{
			if (value != _scatteringColor)
			{
				_scatteringColor = value;
				UpdateMaterialProperties();
			}
		}
	}

	public static DynamicFog instance
	{
		get
		{
			if (_fog == null)
			{
				Camera[] allCameras = Camera.allCameras;
				for (int i = 0; i < allCameras.Length; i++)
				{
					_fog = allCameras[i].GetComponent<DynamicFog>();
					if (_fog != null)
					{
						break;
					}
				}
			}
			return _fog;
		}
	}

	public Camera fogCamera => currentCamera;

	public string GetCurrentPresetName()
	{
		return Enum.GetName(typeof(FOG_PRESET), preset);
	}

	private void OnEnable()
	{
		Init();
		UpdateMaterialPropertiesNow();
	}

	private void Reset()
	{
		UpdateMaterialPropertiesNow();
	}

	private void OnDestroy()
	{
		fogMat = null;
		if (fogMatVol != null)
		{
			UnityEngine.Object.DestroyImmediate(fogMatVol);
			fogMatVol = null;
			if (fogMatDesktopPlusOrthogonal != null)
			{
				UnityEngine.Object.DestroyImmediate(fogMatDesktopPlusOrthogonal);
				fogMatDesktopPlusOrthogonal = null;
			}
		}
		if (fogMatAdv != null)
		{
			UnityEngine.Object.DestroyImmediate(fogMatAdv);
			fogMatAdv = null;
		}
		if (fogMatFogSky != null)
		{
			UnityEngine.Object.DestroyImmediate(fogMatFogSky);
			fogMatFogSky = null;
		}
		if (fogMatOnlyFog != null)
		{
			UnityEngine.Object.DestroyImmediate(fogMatOnlyFog);
			fogMatOnlyFog = null;
		}
		if (fogMatSimple != null)
		{
			UnityEngine.Object.DestroyImmediate(fogMatSimple);
			fogMatSimple = null;
		}
		if (fogMatBasic != null)
		{
			UnityEngine.Object.DestroyImmediate(fogMatBasic);
			fogMatBasic = null;
		}
		if (fogMatOrthogonal != null)
		{
			UnityEngine.Object.DestroyImmediate(fogMatOrthogonal);
			fogMatOrthogonal = null;
		}
		if (fogMatDesktopPlusOrthogonal != null)
		{
			UnityEngine.Object.DestroyImmediate(fogMatDesktopPlusOrthogonal);
			fogMatOrthogonal = null;
		}
		if (fogOfWarTexture != null)
		{
			UnityEngine.Object.DestroyImmediate(fogOfWarTexture);
			fogOfWarTexture = null;
		}
	}

	private void Init()
	{
		targetFogAlpha = -1f;
		targetSkyHazeAlpha = -1f;
		currentCamera = GetComponent<Camera>();
		UpdateFogOfWarTexture();
		if (_profileSync && _profile != null)
		{
			_profile.Load(this);
		}
	}

	private void Update()
	{
		if (fogMat == null)
		{
			return;
		}
		if (transitionProfile)
		{
			float num = (Time.time - transitionStartTime) / transitionDuration;
			if (num > 1f)
			{
				num = 1f;
			}
			DynamicFogProfile.Lerp(initialProfile, targetProfile, num, this);
			if (num >= 1f)
			{
				transitionProfile = false;
			}
		}
		if (transitionAlpha)
		{
			if (targetFogAlpha >= 0f)
			{
				if (targetFogAlpha != currentFogAlpha || targetSkyHazeAlpha != currentSkyHazeAlpha)
				{
					if (transitionDuration > 0f)
					{
						currentFogAlpha = Mathf.Lerp(initialFogAlpha, targetFogAlpha, (Time.time - transitionStartTime) / transitionDuration);
						currentSkyHazeAlpha = Mathf.Lerp(initialSkyHazeAlpha, targetSkyHazeAlpha, (Time.time - transitionStartTime) / transitionDuration);
					}
					else
					{
						currentFogAlpha = targetFogAlpha;
						currentSkyHazeAlpha = targetSkyHazeAlpha;
						transitionAlpha = false;
					}
					fogMat.SetFloat("_FogAlpha", currentFogAlpha);
					SetSkyData();
				}
			}
			else if (currentFogAlpha != alpha || targetSkyHazeAlpha != currentSkyHazeAlpha)
			{
				if (transitionDuration > 0f)
				{
					currentFogAlpha = Mathf.Lerp(initialFogAlpha, alpha, (Time.time - transitionStartTime) / transitionDuration);
					currentSkyHazeAlpha = Mathf.Lerp(initialSkyHazeAlpha, alpha, (Time.time - transitionStartTime) / transitionDuration);
				}
				else
				{
					currentFogAlpha = alpha;
					currentSkyHazeAlpha = skyAlpha;
					transitionAlpha = false;
				}
				fogMat.SetFloat("_FogAlpha", currentFogAlpha);
				SetSkyData();
			}
		}
		if (transitionColor)
		{
			if (targetFogColors)
			{
				if (targetFogColor1 != currentFogColor1 || targetFogColor2 != currentFogColor2)
				{
					if (transitionDuration > 0f)
					{
						currentFogColor1 = Color.Lerp(initialFogColor1, targetFogColor1, (Time.time - transitionStartTime) / transitionDuration);
						currentFogColor2 = Color.Lerp(initialFogColor2, targetFogColor2, (Time.time - transitionStartTime) / transitionDuration);
					}
					else
					{
						currentFogColor1 = targetFogColor1;
						currentFogColor2 = targetFogColor2;
						transitionColor = false;
					}
					fogMat.SetColor("_FogColor", currentFogColor1);
					fogMat.SetColor("_FogColor2", currentFogColor2);
				}
			}
			else if (currentFogColor1 != color || currentFogColor2 != color2)
			{
				if (transitionDuration > 0f)
				{
					currentFogColor1 = Color.Lerp(initialFogColor1, color, (Time.time - transitionStartTime) / transitionDuration);
					currentFogColor2 = Color.Lerp(initialFogColor2, color2, (Time.time - transitionStartTime) / transitionDuration);
				}
				else
				{
					currentFogColor1 = color;
					currentFogColor2 = color2;
					transitionColor = false;
				}
				fogMat.SetColor("_FogColor", currentFogColor1);
				fogMat.SetColor("_FogColor2", currentFogColor2);
			}
		}
		if (sun != null)
		{
			bool flag = false;
			if (sun.transform.forward != sunDirection)
			{
				flag = true;
			}
			if (sunLight != null && (sunLight.color != sunColor || sunLight.intensity != sunIntensity))
			{
				flag = true;
			}
			if (flag)
			{
				UpdateFogColor();
			}
		}
	}

	private void OnDidApplyAnimationProperties()
	{
		shouldUpdateMaterialProperties = true;
	}

	public void CheckPreset()
	{
		if (_preset != FOG_PRESET.Custom)
		{
			_effectType = FOG_TYPE.DesktopFogWithSkyHaze;
		}
		switch (preset)
		{
		case FOG_PRESET.Clear:
			alpha = 0f;
			break;
		case FOG_PRESET.Mist:
			alpha = 0.75f;
			skySpeed = 0.11f;
			skyHaze = 15f;
			skyNoiseStrength = 1f;
			skyAlpha = 0.33f;
			distance = 0f;
			distanceFallOff = 0.07f;
			height = 4.4f;
			heightFallOff = 1f;
			turbulence = 0f;
			noiseStrength = 0.6f;
			speed = 0.01f;
			color = new Color(0.89f, 0.89f, 0.89f, 1f);
			color2 = color;
			maxDistance = 0.999f;
			maxDistanceFallOff = 0f;
			break;
		case FOG_PRESET.WindyMist:
			alpha = 0.75f;
			skySpeed = 0.3f;
			skyHaze = 35f;
			skyNoiseStrength = 0.32f;
			skyAlpha = 0.33f;
			distance = 0f;
			distanceFallOff = 0.07f;
			height = 2f;
			heightFallOff = 1f;
			turbulence = 2f;
			noiseStrength = 0.6f;
			speed = 0.06f;
			color = new Color(0.89f, 0.89f, 0.89f, 1f);
			color2 = color;
			maxDistance = 0.999f;
			maxDistanceFallOff = 0f;
			break;
		case FOG_PRESET.GroundFog:
			alpha = 1f;
			skySpeed = 0.3f;
			skyHaze = 35f;
			skyNoiseStrength = 0.32f;
			skyAlpha = 0.33f;
			distance = 0f;
			distanceFallOff = 0f;
			height = 1f;
			heightFallOff = 1f;
			turbulence = 0.4f;
			noiseStrength = 0.7f;
			speed = 0.005f;
			color = new Color(0.89f, 0.89f, 0.89f, 1f);
			color2 = color;
			maxDistance = 0.999f;
			maxDistanceFallOff = 0f;
			break;
		case FOG_PRESET.Fog:
			alpha = 0.96f;
			skySpeed = 0.3f;
			skyHaze = 155f;
			skyNoiseStrength = 0.6f;
			skyAlpha = 0.93f;
			distance = (effectType.isPlus() ? 0.2f : 0.01f);
			distanceFallOff = 0.04f;
			height = 20f;
			heightFallOff = 1f;
			turbulence = 0.4f;
			noiseStrength = 0.4f;
			speed = 0.005f;
			color = new Color(0.89f, 0.89f, 0.89f, 1f);
			color2 = color;
			maxDistance = 0.999f;
			maxDistanceFallOff = 0f;
			break;
		case FOG_PRESET.HeavyFog:
			alpha = 1f;
			skySpeed = 0.05f;
			skyHaze = 350f;
			skyNoiseStrength = 0.8f;
			skyAlpha = 0.97f;
			distance = (effectType.isPlus() ? 0.1f : 0f);
			distanceFallOff = 0.045f;
			height = 35f;
			heightFallOff = 0.88f;
			turbulence = 0.4f;
			noiseStrength = 0.24f;
			speed = 0.003f;
			color = new Color(0.86f, 0.847f, 0.847f, 1f);
			color2 = color;
			maxDistance = 0.999f;
			maxDistanceFallOff = 0f;
			break;
		case FOG_PRESET.SandStorm:
			alpha = 1f;
			skySpeed = 0.49f;
			skyHaze = 333f;
			skyNoiseStrength = 0.72f;
			skyAlpha = 0.97f;
			distance = (effectType.isPlus() ? 0.15f : 0f);
			distanceFallOff = 0.028f;
			height = 83f;
			heightFallOff = 0f;
			turbulence = 15f;
			noiseStrength = 0.45f;
			speed = 0.2f;
			color = new Color(0.364f, 0.36f, 0.36f, 1f);
			color2 = color;
			maxDistance = 0.999f;
			maxDistanceFallOff = 0f;
			break;
		}
	}

	private void OnPreCull()
	{
		if (currentCamera != null)
		{
			currentCamera.depthTextureMode |= DepthTextureMode.Depth;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (fogMat == null || _alpha == 0f || currentCamera == null)
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (shouldUpdateMaterialProperties)
		{
			shouldUpdateMaterialProperties = false;
			UpdateMaterialPropertiesNow();
		}
		if (currentCamera.orthographic)
		{
			if (!matOrtho)
			{
				ResetMaterial();
			}
			fogMat.SetVector("_ClipDir", currentCamera.transform.forward);
		}
		else if (matOrtho)
		{
			ResetMaterial();
		}
		fogMat.SetMatrix("_ClipToWorld", currentCamera.cameraToWorldMatrix * currentCamera.projectionMatrix.inverse);
		Graphics.Blit(source, destination, fogMat);
	}

	protected void ResetMaterial()
	{
		fogMat = null;
		fogMatAdv = null;
		fogMatFogSky = null;
		fogMatOnlyFog = null;
		fogMatSimple = null;
		fogMatBasic = null;
		fogMatVol = null;
		fogMatDesktopPlusOrthogonal = null;
		fogMatOrthogonal = null;
		UpdateMaterialProperties();
	}

	public void UpdateMaterialProperties()
	{
		if (Application.isPlaying)
		{
			shouldUpdateMaterialProperties = true;
		}
		else
		{
			UpdateMaterialPropertiesNow();
		}
	}

	protected void UpdateMaterialPropertiesNow()
	{
		CheckPreset();
		CopyTransitionValues();
		if (currentCamera == null)
		{
			currentCamera = GetComponent<Camera>();
		}
		switch (effectType)
		{
		case FOG_TYPE.MobileFogOnlyGround:
			if (fogMatOnlyFog == null)
			{
				string path;
				if (currentCamera.orthographic)
				{
					matOrtho = true;
					path = "Materials/DFOOnlyFog";
				}
				else
				{
					matOrtho = false;
					path = "Materials/DFGOnlyFog";
				}
				fogMatOnlyFog = UnityEngine.Object.Instantiate(Resources.Load<Material>(path));
				fogMatOnlyFog.hideFlags = HideFlags.DontSave;
			}
			fogMat = fogMatOnlyFog;
			break;
		case FOG_TYPE.MobileFogWithSkyHaze:
			if (fogMatFogSky == null)
			{
				string path;
				if (currentCamera.orthographic)
				{
					matOrtho = true;
					path = "Materials/DFOWithSky";
				}
				else
				{
					matOrtho = false;
					path = "Materials/DFGWithSky";
				}
				fogMatFogSky = UnityEngine.Object.Instantiate(Resources.Load<Material>(path));
				fogMatFogSky.hideFlags = HideFlags.DontSave;
			}
			fogMat = fogMatFogSky;
			break;
		case FOG_TYPE.DesktopFogPlusWithSkyHaze:
			if (fogMatVol == null)
			{
				string path;
				if (currentCamera.orthographic)
				{
					matOrtho = true;
					path = "Materials/DFODesktopPlus";
				}
				else
				{
					matOrtho = false;
					path = "Materials/DFGDesktopPlus";
				}
				fogMatVol = UnityEngine.Object.Instantiate(Resources.Load<Material>(path));
				fogMatVol.hideFlags = HideFlags.DontSave;
			}
			fogMat = fogMatVol;
			break;
		case FOG_TYPE.MobileFogSimple:
			if (fogMatSimple == null)
			{
				string path;
				if (currentCamera.orthographic)
				{
					matOrtho = true;
					path = "Materials/DFOSimple";
				}
				else
				{
					matOrtho = false;
					path = "Materials/DFGSimple";
				}
				fogMatSimple = UnityEngine.Object.Instantiate(Resources.Load<Material>(path));
				fogMatSimple.hideFlags = HideFlags.DontSave;
			}
			fogMat = fogMatSimple;
			break;
		case FOG_TYPE.MobileFogBasic:
			if (fogMatBasic == null)
			{
				string path;
				if (currentCamera.orthographic)
				{
					matOrtho = true;
					path = "Materials/DFOBasic";
				}
				else
				{
					matOrtho = false;
					path = "Materials/DFGBasic";
				}
				fogMatBasic = UnityEngine.Object.Instantiate(Resources.Load<Material>(path));
				fogMatBasic.hideFlags = HideFlags.DontSave;
			}
			fogMat = fogMatBasic;
			break;
		case FOG_TYPE.MobileFogOrthogonal:
			if (fogMatOrthogonal == null)
			{
				string path;
				if (currentCamera.orthographic)
				{
					matOrtho = true;
					path = "Materials/DFOOrthogonal";
				}
				else
				{
					matOrtho = false;
					path = "Materials/DFGOrthogonal";
				}
				fogMatOrthogonal = UnityEngine.Object.Instantiate(Resources.Load<Material>(path));
				fogMatOrthogonal.hideFlags = HideFlags.DontSave;
			}
			fogMat = fogMatOrthogonal;
			break;
		case FOG_TYPE.DesktopFogPlusOrthogonal:
			if (fogMatDesktopPlusOrthogonal == null)
			{
				string path;
				if (currentCamera.orthographic)
				{
					matOrtho = true;
					path = "Materials/DFODesktopPlusOrthogonal";
				}
				else
				{
					matOrtho = false;
					path = "Materials/DFGDesktopPlusOrthogonal";
				}
				fogMatDesktopPlusOrthogonal = UnityEngine.Object.Instantiate(Resources.Load<Material>(path));
				fogMatDesktopPlusOrthogonal.hideFlags = HideFlags.DontSave;
			}
			fogMat = fogMatDesktopPlusOrthogonal;
			break;
		default:
			if (fogMatAdv == null)
			{
				string path;
				if (currentCamera.orthographic)
				{
					matOrtho = true;
					path = "Materials/DFODesktop";
				}
				else
				{
					matOrtho = false;
					path = "Materials/DFGDesktop";
				}
				fogMatAdv = UnityEngine.Object.Instantiate(Resources.Load<Material>(path));
				fogMatAdv.hideFlags = HideFlags.DontSave;
			}
			fogMat = fogMatAdv;
			break;
		}
		if (fogMat == null)
		{
			return;
		}
		float num = ((effectType == FOG_TYPE.DesktopFogPlusWithSkyHaze) ? (_speed * 5f) : _speed);
		fogMat.SetVector("_FogSpeed", -_windDirection.normalized * num);
		Vector4 value = new Vector4(_noiseStrength, _turbulence, currentCamera.farClipPlane * 15f / 1000f, _noiseScale);
		fogMat.SetVector("_FogNoiseData", value);
		Vector4 value2 = new Vector4(_height + 0.001f, _baselineHeight, _clipUnderBaseline ? (-0.01f) : (-10000f), _heightFallOff);
		if (_effectType == FOG_TYPE.MobileFogOrthogonal || _effectType == FOG_TYPE.DesktopFogPlusOrthogonal)
		{
			value2.z = maxHeight;
		}
		fogMat.SetVector("_FogHeightData", value2);
		fogMat.SetFloat("_FogAlpha", currentFogAlpha);
		Vector4 value3 = new Vector4(_distance, _distanceFallOff, _maxDistance, _maxDistanceFallOff);
		if (effectType.isPlus())
		{
			value3.x = currentCamera.farClipPlane * _distance;
			value3.y = distanceFallOff * value3.x + 0.0001f;
			value3.z *= currentCamera.farClipPlane;
		}
		fogMat.SetVector("_FogDistance", value3);
		UpdateFogColor();
		SetSkyData();
		if (shaderKeywords == null)
		{
			shaderKeywords = new List<string>();
		}
		else
		{
			shaderKeywords.Clear();
		}
		if (fogOfWarEnabled)
		{
			if (fogOfWarTexture == null)
			{
				UpdateFogOfWarTexture();
			}
			fogMat.SetTexture("_FogOfWar", fogOfWarTexture);
			fogMat.SetVector("_FogOfWarCenter", _fogOfWarCenter);
			fogMat.SetVector("_FogOfWarSize", _fogOfWarSize);
			Vector3 vector = fogOfWarCenter - 0.5f * _fogOfWarSize;
			fogMat.SetVector("_FogOfWarCenterAdjusted", new Vector3(vector.x / _fogOfWarSize.x, 1f, vector.z / _fogOfWarSize.z));
			shaderKeywords.Add("FOG_OF_WAR_ON");
		}
		if (_enableDithering)
		{
			fogMat.SetFloat("_FogDither", _ditherStrength * 0.1f);
			shaderKeywords.Add("DITHER_ON");
		}
		fogMat.shaderKeywords = shaderKeywords.ToArray();
	}

	private void CopyTransitionValues()
	{
		currentFogAlpha = _alpha;
		currentSkyHazeAlpha = _skyAlpha;
		currentFogColor1 = _color;
		currentFogColor2 = _color2;
	}

	private void SetSkyData()
	{
		Vector4 value = new Vector4(_skyHaze, _skySpeed, _skyNoiseStrength, currentSkyHazeAlpha);
		fogMat.SetVector("_FogSkyData", value);
	}

	private void UpdateFogColor()
	{
		if (fogMat == null)
		{
			return;
		}
		if (_sun != null)
		{
			if (sunLight == null)
			{
				sunLight = _sun.GetComponent<Light>();
			}
			if (sunLight != null && sunLight.transform != _sun.transform)
			{
				sunLight = _sun.GetComponent<Light>();
			}
			sunDirection = _sun.transform.forward;
			if (sunLight != null)
			{
				sunColor = sunLight.color;
				sunIntensity = sunLight.intensity;
			}
		}
		float num = sunIntensity * Mathf.Clamp01(1f - sunDirection.y);
		fogMat.SetColor("_FogColor", num * currentFogColor1 * sunColor);
		fogMat.SetColor("_FogColor2", num * currentFogColor2 * sunColor);
		Color color = num * scatteringColor;
		fogMat.SetColor("_SunColor", new Vector4(color.r, color.g, color.b, scattering));
		fogMat.SetVector("_SunDir", -sunDirection);
	}

	public void SetTargetProfile(DynamicFogProfile targetProfile, float duration)
	{
		if (_useFogVolumes)
		{
			preset = FOG_PRESET.Custom;
			initialProfile = ScriptableObject.CreateInstance<DynamicFogProfile>();
			initialProfile.Save(this);
			this.targetProfile = targetProfile;
			transitionDuration = duration;
			transitionStartTime = Time.time;
			transitionProfile = true;
		}
	}

	public void ClearTargetProfile(float duration)
	{
		SetTargetProfile(initialProfile, duration);
	}

	public void SetTargetAlpha(float newFogAlpha, float newSkyHazeAlpha, float duration)
	{
		if (useFogVolumes)
		{
			preset = FOG_PRESET.Custom;
			initialFogAlpha = currentFogAlpha;
			initialSkyHazeAlpha = currentSkyHazeAlpha;
			targetFogAlpha = newFogAlpha;
			targetSkyHazeAlpha = newSkyHazeAlpha;
			transitionDuration = duration;
			transitionStartTime = Time.time;
			transitionAlpha = true;
		}
	}

	public void ClearTargetAlpha(float duration)
	{
		SetTargetAlpha(-1f, -1f, duration);
	}

	public void SetTargetColors(Color color1, Color color2, float duration)
	{
		if (useFogVolumes)
		{
			preset = FOG_PRESET.Custom;
			initialFogColor1 = currentFogColor1;
			initialFogColor2 = currentFogColor2;
			targetFogColor1 = color1;
			targetFogColor2 = color2;
			transitionDuration = duration;
			transitionStartTime = Time.time;
			targetFogColors = true;
			transitionColor = true;
		}
	}

	public void ClearTargetColors(float duration)
	{
		targetFogColors = false;
		SetTargetColors(color, color2, duration);
	}

	private void UpdateFogOfWarTexture()
	{
		if (fogOfWarEnabled)
		{
			int scaledSize = GetScaledSize(fogOfWarTextureSize, 1f);
			fogOfWarTexture = new Texture2D(scaledSize, scaledSize, TextureFormat.ARGB32, mipChain: false);
			fogOfWarTexture.hideFlags = HideFlags.DontSave;
			fogOfWarTexture.filterMode = FilterMode.Bilinear;
			fogOfWarTexture.wrapMode = TextureWrapMode.Clamp;
			ResetFogOfWar();
		}
	}

	public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha)
	{
		if (fogOfWarTexture == null)
		{
			return;
		}
		float num = (worldPosition.x - fogOfWarCenter.x) / fogOfWarSize.x + 0.5f;
		if (num < 0f || num > 1f)
		{
			return;
		}
		float num2 = (worldPosition.z - fogOfWarCenter.z) / fogOfWarSize.z + 0.5f;
		if (num2 < 0f || num2 > 1f)
		{
			return;
		}
		int width = fogOfWarTexture.width;
		int num3 = fogOfWarTexture.height;
		int num4 = (int)(num * (float)width);
		int num5 = (int)(num2 * (float)num3);
		int num6 = num5 * width + num4;
		byte b = (byte)(fogNewAlpha * 255f);
		Color32 color = fogOfWarColorBuffer[num6];
		if (b == color.a)
		{
			return;
		}
		float num7 = radius / fogOfWarSize.z;
		int num8 = Mathf.FloorToInt((float)num3 * num7);
		for (int i = num5 - num8; i <= num5 + num8; i++)
		{
			if (i <= 0 || i >= num3 - 1)
			{
				continue;
			}
			for (int j = num4 - num8; j <= num4 + num8; j++)
			{
				if (j > 0 && j < width - 1)
				{
					int num9 = Mathf.FloorToInt(Mathf.Sqrt((num5 - i) * (num5 - i) + (num4 - j) * (num4 - j)));
					if (num9 <= num8)
					{
						num6 = i * width + j;
						Color32 color2 = fogOfWarColorBuffer[num6];
						color2.a = (byte)Mathf.Lerp((int)b, (int)color2.a, (float)num9 / (float)num8);
						fogOfWarColorBuffer[num6] = color2;
						fogOfWarTexture.SetPixel(j, i, color2);
					}
				}
			}
		}
		fogOfWarTexture.Apply();
	}

	public void ResetFogOfWarAlpha(Vector3 worldPosition, float radius)
	{
		if (fogOfWarTexture == null)
		{
			return;
		}
		float num = (worldPosition.x - fogOfWarCenter.x) / fogOfWarSize.x + 0.5f;
		if (num < 0f || num > 1f)
		{
			return;
		}
		float num2 = (worldPosition.z - fogOfWarCenter.z) / fogOfWarSize.z + 0.5f;
		if (num2 < 0f || num2 > 1f)
		{
			return;
		}
		int width = fogOfWarTexture.width;
		int num3 = fogOfWarTexture.height;
		int num4 = (int)(num * (float)width);
		int num5 = (int)(num2 * (float)num3);
		int num6 = num5 * width + num4;
		float num7 = radius / fogOfWarSize.z;
		int num8 = Mathf.FloorToInt((float)num3 * num7);
		for (int i = num5 - num8; i <= num5 + num8; i++)
		{
			if (i > 0 && i < num3 - 1)
			{
				for (int j = num4 - num8; j <= num4 + num8; j++)
				{
					if (j > 0 && j < width - 1 && Mathf.FloorToInt(Mathf.Sqrt((num5 - i) * (num5 - i) + (num4 - j) * (num4 - j))) <= num8)
					{
						num6 = i * width + j;
						Color32 color = fogOfWarColorBuffer[num6];
						color.a = byte.MaxValue;
						fogOfWarColorBuffer[num6] = color;
						fogOfWarTexture.SetPixel(j, i, color);
					}
				}
			}
			fogOfWarTexture.Apply();
		}
	}

	public void ResetFogOfWar()
	{
		if (!(fogOfWarTexture == null))
		{
			int num = fogOfWarTexture.height;
			int width = fogOfWarTexture.width;
			int num2 = num * width;
			if (fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length != num2)
			{
				fogOfWarColorBuffer = new Color32[num2];
			}
			Color32 color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			for (int i = 0; i < num2; i++)
			{
				fogOfWarColorBuffer[i] = color;
			}
			fogOfWarTexture.SetPixels32(fogOfWarColorBuffer);
			fogOfWarTexture.Apply();
		}
	}

	private int GetScaledSize(int size, float factor)
	{
		size = (int)((float)size / factor);
		size /= 4;
		if (size < 1)
		{
			size = 1;
		}
		return size * 4;
	}
}
