using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VolumetricFogAndMist;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Rendering/Volumetric Fog & Mist")]
[HelpURL("http://kronnect.com/taptapgo")]
public class VolumetricFog : MonoBehaviour
{
	[Serializable]
	public struct PointLightParams
	{
		public Light light;

		[HideInInspector]
		public VolumetricFogLightParams lightParams;

		public float range;

		public float rangeMultiplier;

		public float intensity;

		public float intensityMultiplier;

		public Vector3 position;

		public Color color;
	}

	private struct FogOfWarTransition
	{
		public bool enabled;

		public int x;

		public int y;

		public float startTime;

		public float startDelay;

		public float duration;

		public int initialAlpha;

		public int targetAlpha;
	}

	public const string SKW_FOG_DISTANCE_ON = "FOG_DISTANCE_ON";

	public const string SKW_LIGHT_SCATTERING = "FOG_SCATTERING_ON";

	public const string SKW_FOG_AREA_BOX = "FOG_AREA_BOX";

	public const string SKW_FOG_AREA_SPHERE = "FOG_AREA_SPHERE";

	public const string SKW_FOG_VOID_BOX = "FOG_VOID_BOX";

	public const string SKW_FOG_VOID_SPHERE = "FOG_VOID_SPHERE";

	public const string SKW_FOG_HAZE_ON = "FOG_HAZE_ON";

	public const string SKW_FOG_OF_WAR_ON = "FOG_OF_WAR_ON";

	public const string SKW_FOG_BLUR = "FOG_BLUR_ON";

	public const string SKW_SUN_SHADOWS = "FOG_SUN_SHADOWS_ON";

	public const string SKW_FOG_USE_XY_PLANE = "FOG_USE_XY_PLANE";

	public const string SKW_FOG_COMPUTE_DEPTH = "FOG_COMPUTE_DEPTH";

	public const string SKW_POINT_LIGHTS = "FOG_POINT_LIGHTS";

	private const string DEPTH_CAM_NAME = "VFMDepthCamera";

	private const string DEPTH_SUN_CAM_NAME = "VFMDepthSunCamera";

	private const string VFM_BUILD_FIRST_INSTALL = "VFMFirstInstall";

	private const string VFM_BUILD_HINT = "VFMBuildHint1065RC1";

	private static VolumetricFog _fog;

	[HideInInspector]
	public bool isDirty;

	[SerializeField]
	private FOG_PRESET _preset = FOG_PRESET.Mist;

	[SerializeField]
	private VolumetricFogProfile _profile;

	[SerializeField]
	private bool _profileSync;

	[SerializeField]
	private bool _useFogVolumes;

	[SerializeField]
	private bool _debugPass;

	[SerializeField]
	private bool _showInSceneView = true;

	[SerializeField]
	private TRANSPARENT_MODE _transparencyBlendMode;

	[SerializeField]
	[Range(0f, 1f)]
	private float _transparencyBlendPower = 1f;

	[SerializeField]
	private LayerMask _transparencyLayerMask = -1;

	[SerializeField]
	private FOG_VISIBILITY_SCOPE _visibilityScope;

	[SerializeField]
	private Bounds _visibilityVolume = new Bounds(Vector3.zero, new Vector3(1000f, 1000f, 1000f));

	[SerializeField]
	private LIGHTING_MODEL _lightingModel;

	[SerializeField]
	private bool _enableMultipleCameras;

	[SerializeField]
	private bool _computeDepth;

	[SerializeField]
	private COMPUTE_DEPTH_SCOPE _computeDepthScope;

	[SerializeField]
	private float _transparencyCutOff = 0.1f;

	[SerializeField]
	private bool _renderBeforeTransparent;

	[SerializeField]
	private GameObject _sun;

	[SerializeField]
	[Range(0f, 0.5f)]
	private float _timeBetweenTextureUpdates = 0.2f;

	[SerializeField]
	private bool _sunCopyColor = true;

	[SerializeField]
	private MeshRenderer[] _renderersForDepth;

	[SerializeField]
	private Material _materialDrawDepth;

	private RenderTexture _depthTexture;

	private CommandBuffer _commandBufferDepthTexture;

	[SerializeField]
	[Range(0f, 1.25f)]
	private float _density = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _noiseStrength = 0.8f;

	[SerializeField]
	[Range(1f, 2f)]
	private float _noiseFinalMultiplier = 1f;

	[SerializeField]
	[Range(-0.3f, 2f)]
	private float _noiseSparse;

	[SerializeField]
	[Range(0f, 1000f)]
	private float _distance;

	[SerializeField]
	private float _maxFogLength = 1000f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _maxFogLengthFallOff;

	[SerializeField]
	[Range(0f, 5f)]
	private float _distanceFallOff;

	[SerializeField]
	[Range(0.0001f, 500f)]
	private float _height = 4f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _heightFallOff = 0.6f;

	[SerializeField]
	private float _deepObscurance = 1f;

	[SerializeField]
	private float _baselineHeight;

	[SerializeField]
	private bool _baselineRelativeToCamera;

	[SerializeField]
	[Range(0f, 1f)]
	private float _baselineRelativeToCameraDelay;

	[SerializeField]
	private float _noiseScale = 1f;

	[SerializeField]
	[Range(0f, 1.05f)]
	private float _alpha = 1f;

	[SerializeField]
	private Color _color = new Color(0.89f, 0.89f, 0.89f, 1f);

	[SerializeField]
	private Color _specularColor = new Color(1f, 1f, 0.8f, 1f);

	[SerializeField]
	[Range(0f, 1f)]
	private float _specularThreshold = 0.6f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _specularIntensity = 0.2f;

	[SerializeField]
	private Vector3 _lightDirection = new Vector3(1f, 0f, -1f);

	[SerializeField]
	private float _lightIntensity = 0.2f;

	[SerializeField]
	private Color _lightColor = Color.white;

	[SerializeField]
	[Range(1f, 5f)]
	private int _updateTextureSpread = 1;

	[SerializeField]
	[Range(0f, 1f)]
	private float _speed = 0.01f;

	[SerializeField]
	private Vector3 _windDirection = new Vector3(-1f, 0f, 0f);

	[SerializeField]
	private bool _useRealTime;

	[SerializeField]
	private Color _skyColor = new Color(0.89f, 0.89f, 0.89f, 1f);

	[SerializeField]
	private float _skyHaze = 50f;

	[SerializeField]
	private float _skyNoiseScale = 1.5f;

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
	[Range(0f, 0.999f)]
	private float _skyDepth = 0.999f;

	[SerializeField]
	private GameObject _character;

	[SerializeField]
	private FOG_VOID_TOPOLOGY _fogVoidTopology;

	[SerializeField]
	[Range(0f, 10f)]
	private float _fogVoidFallOff = 1f;

	[SerializeField]
	private float _fogVoidRadius;

	[SerializeField]
	private Vector3 _fogVoidPosition = Vector3.zero;

	[SerializeField]
	private float _fogVoidDepth;

	[SerializeField]
	private float _fogVoidHeight;

	[SerializeField]
	private bool _fogVoidInverted;

	[SerializeField]
	private bool _fogAreaShowGizmos = true;

	[SerializeField]
	private GameObject _fogAreaCenter;

	[SerializeField]
	[Range(0.001f, 10f)]
	private float _fogAreaFallOff = 1f;

	[SerializeField]
	private FOG_AREA_FOLLOW_MODE _fogAreaFollowMode;

	[SerializeField]
	private FOG_AREA_TOPOLOGY _fogAreaTopology = FOG_AREA_TOPOLOGY.Sphere;

	[SerializeField]
	private float _fogAreaRadius;

	[SerializeField]
	private Vector3 _fogAreaPosition = Vector3.zero;

	[SerializeField]
	private float _fogAreaDepth;

	[SerializeField]
	private float _fogAreaHeight;

	[SerializeField]
	private FOG_AREA_SORTING_MODE _fogAreaSortingMode;

	[SerializeField]
	private int _fogAreaRenderOrder = 1;

	public PointLightParams[] pointLightParams;

	[SerializeField]
	private bool pointLightDataMigrated;

	private Vector4[] pointLightColorBuffer;

	private Vector4[] pointLightPositionBuffer;

	[SerializeField]
	private GameObject[] _pointLights = new GameObject[6];

	[SerializeField]
	private float[] _pointLightRanges = new float[6];

	[SerializeField]
	private float[] _pointLightIntensities = new float[6] { 1f, 1f, 1f, 1f, 1f, 1f };

	[SerializeField]
	private float[] _pointLightIntensitiesMultiplier = new float[6] { 1f, 1f, 1f, 1f, 1f, 1f };

	[SerializeField]
	private Vector3[] _pointLightPositions = new Vector3[6];

	[SerializeField]
	private Color[] _pointLightColors = new Color[6]
	{
		new Color(1f, 1f, 0f, 1f),
		new Color(1f, 1f, 0f, 1f),
		new Color(1f, 1f, 0f, 1f),
		new Color(1f, 1f, 0f, 1f),
		new Color(1f, 1f, 0f, 1f),
		new Color(1f, 1f, 0f, 1f)
	};

	[SerializeField]
	private bool _pointLightTrackingAuto;

	[SerializeField]
	private Transform _pointLightTrackingPivot;

	[SerializeField]
	private int _pointLightTrackingCount;

	[SerializeField]
	[Range(0f, 5f)]
	private float _pointLightTrackingCheckInterval = 1f;

	[SerializeField]
	private float _pointLightTrackingNewLightsCheckInterval = 3f;

	[SerializeField]
	private float _pointLightInscattering = 1f;

	[SerializeField]
	private float _pointLightIntensity = 1f;

	[SerializeField]
	private float _pointLightInsideAtten;

	[SerializeField]
	[Range(1f, 8f)]
	private int _downsampling = 1;

	[SerializeField]
	private bool _forceComposition;

	[SerializeField]
	private bool _edgeImprove;

	[SerializeField]
	[Range(1E-05f, 0.005f)]
	private float _edgeThreshold = 0.0005f;

	[SerializeField]
	[Range(1f, 20f)]
	private float _stepping = 12f;

	[SerializeField]
	[Range(0f, 50f)]
	private float _steppingNear = 1f;

	[SerializeField]
	private bool _dithering;

	[SerializeField]
	[Range(0.1f, 5f)]
	private float _ditherStrength = 0.75f;

	[SerializeField]
	[Range(0f, 2f)]
	private float _jitterStrength = 0.5f;

	[SerializeField]
	private bool _lightScatteringEnabled;

	[SerializeField]
	[Range(0f, 1f)]
	private float _lightScatteringDiffusion = 0.7f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _lightScatteringSpread = 0.686f;

	[SerializeField]
	[Range(4f, 64f)]
	private int _lightScatteringSamples = 16;

	[SerializeField]
	[Range(0f, 50f)]
	private float _lightScatteringWeight = 1.9f;

	[SerializeField]
	[Range(0f, 50f)]
	private float _lightScatteringIllumination = 18f;

	[SerializeField]
	[Range(0.9f, 1.1f)]
	private float _lightScatteringDecay = 0.986f;

	[SerializeField]
	[Range(0f, 0.2f)]
	private float _lightScatteringExposure;

	[SerializeField]
	[Range(0f, 1f)]
	private float _lightScatteringJittering = 0.5f;

	[SerializeField]
	[Range(1f, 4f)]
	private int _lightScatteringBlurDownscale = 1;

	[SerializeField]
	private bool _fogBlur;

	[SerializeField]
	[Range(0f, 1f)]
	private float _fogBlurDepth = 0.05f;

	[SerializeField]
	private bool _sunShadows;

	[SerializeField]
	private LayerMask _sunShadowsLayerMask = -1;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunShadowsStrength = 0.5f;

	[SerializeField]
	private float _sunShadowsBias = 0.1f;

	[SerializeField]
	[Range(0f, 0.5f)]
	private float _sunShadowsJitterStrength = 0.1f;

	[SerializeField]
	[Range(0f, 4f)]
	private int _sunShadowsResolution = 2;

	[SerializeField]
	[Range(50f, 2000f)]
	private float _sunShadowsMaxDistance = 200f;

	[SerializeField]
	private SUN_SHADOWS_BAKE_MODE _sunShadowsBakeMode = SUN_SHADOWS_BAKE_MODE.Discrete;

	[SerializeField]
	private float _sunShadowsRefreshInterval;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunShadowsCancellation;

	[SerializeField]
	[Range(0f, 10f)]
	private float _turbulenceStrength;

	[SerializeField]
	private bool _useXYPlane;

	[SerializeField]
	private bool _useSinglePassStereoRenderingMatrix;

	[SerializeField]
	private SPSR_BEHAVIOUR _spsrBehaviour;

	[SerializeField]
	private bool _reduceFlickerBigWorlds;

	[SerializeField]
	private bool _enableMask;

	[SerializeField]
	private LayerMask _maskLayer = 8388608;

	[SerializeField]
	[Range(1f, 4f)]
	private int _maskDownsampling = 1;

	public bool isRendering;

	[NonSerialized]
	public float distanceToCameraMin;

	[NonSerialized]
	public float distanceToCameraMax;

	[NonSerialized]
	public float distanceToCamera;

	[NonSerialized]
	public float distanceToCameraYAxis;

	[NonSerialized]
	public FOG_TEMPORARY_PROPERTIES temporaryProperties;

	public VolumetricFog fogRenderer;

	private VolumetricFog[] allFogRenderers;

	private bool isPartOfScene;

	private int noiseTextureSize;

	private float initialFogAlpha;

	private float targetFogAlpha;

	private float initialSkyHazeAlpha;

	private float targetSkyHazeAlpha;

	private bool transitionAlpha;

	private bool transitionColor;

	private bool transitionSpecularColor;

	private bool transitionLightColor;

	private bool transitionProfile;

	private bool targetColorActive;

	private bool targetSpecularColorActive;

	private bool targetLightColorActive;

	private Color initialFogColor;

	private Color targetFogColor;

	private Color initialFogSpecularColor;

	private Color targetFogSpecularColor;

	private Color initialLightColor;

	private Color targetLightColor;

	private float transitionDuration;

	private float transitionStartTime;

	private float currentFogAlpha;

	private float currentSkyHazeAlpha;

	private Color currentFogColor;

	private Color currentFogSpecularColor;

	private Color currentLightColor;

	private VolumetricFogProfile initialProfile;

	private VolumetricFogProfile targetProfile;

	private float oldBaselineRelativeCameraY;

	private float currentFogAltitude;

	private float skyHazeSpeedAcum;

	private Color skyHazeLightColor;

	private bool _hasCamera;

	private bool _hasCameraChecked;

	private Camera mainCamera;

	private List<string> shaderKeywords;

	private Material blurMat;

	private RenderBuffer[] mrt;

	private int _renderingInstancesCount;

	private bool shouldUpdateMaterialProperties;

	private int lastFrameCount;

	[NonSerialized]
	public Material fogMat;

	private RenderTexture depthTexture;

	private RenderTexture depthSunTexture;

	private RenderTexture reducedDestination;

	private Light[] lastFoundLights;

	private Light[] lightBuffer;

	private Light[] currentLights;

	private float trackPointAutoLastTime;

	private float trackPointCheckNewLightsLastTime;

	private Vector4 black = new Vector4(0f, 0f, 0f, 1f);

	private Shader depthShader;

	private Shader depthShaderAndTrans;

	private GameObject depthCamObj;

	private Camera depthCam;

	private float lastTextureUpdate;

	private Vector3 windSpeedAcum;

	private Texture2D adjustedTexture;

	private Color[] noiseColors;

	private Color[] adjustedColors;

	private float sunLightIntensity = 1f;

	private bool needUpdateTexture;

	private bool hasChangeAdjustedColorsAlpha;

	private int updatingTextureSlice;

	private Color updatingTextureLightColor;

	private Color lastRenderSettingsAmbientLight;

	private float lastRenderSettingsAmbientIntensity;

	private int lastFrameAppliedChaos;

	private int lastFrameAppliedWind;

	private Light sunLight;

	private Vector2 oldSunPos;

	private float sunFade = 1f;

	private Vector3 lastLightDirection;

	private GameObject depthSunCamObj;

	private Camera depthSunCam;

	private Shader depthSunShader;

	[NonSerialized]
	public bool needUpdateDepthSunTexture;

	private float lastShadowUpdateFrame;

	private bool sunShadowsActive;

	private int currentDepthSunTextureRes;

	private Matrix4x4 lightMatrix;

	private Texture2D adjustedChaosTexture;

	private Material chaosLerpMat;

	private float turbAcum;

	private float deltaTime;

	private float timeOfLastRender;

	private RenderTexture rtAdjusted;

	private List<VolumetricFog> fogInstances = new List<VolumetricFog>();

	private List<VolumetricFog> fogRenderInstances = new List<VolumetricFog>();

	private MeshRenderer mr;

	private float lastTimeSortInstances;

	private const float FOG_INSTANCES_SORT_INTERVAL = 2f;

	private Vector3 lastCamPos;

	private bool needResort;

	private CommandBuffer maskCommandBuffer;

	private RenderTextureDescriptor rtMaskDesc;

	private Material maskMaterial;

	private const int MAX_SIMULTANEOUS_TRANSITIONS = 10000;

	[SerializeField]
	private bool _fogOfWarEnabled;

	[SerializeField]
	private Vector3 _fogOfWarCenter;

	[SerializeField]
	private Vector3 _fogOfWarSize = new Vector3(1024f, 0f, 1024f);

	[SerializeField]
	[Range(32f, 2048f)]
	private int _fogOfWarTextureSize = 256;

	[SerializeField]
	[Range(0f, 100f)]
	private float _fogOfWarRestoreDelay;

	[SerializeField]
	[Range(0f, 25f)]
	private float _fogOfWarRestoreDuration = 2f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _fogOfWarSmoothness = 1f;

	[SerializeField]
	private bool _fogOfWarBlur;

	[SerializeField]
	private bool _maskEditorEnabled;

	[SerializeField]
	private MASK_TEXTURE_BRUSH_MODE _maskBrushMode = MASK_TEXTURE_BRUSH_MODE.RemoveFog;

	[SerializeField]
	[Range(1f, 128f)]
	private int _maskBrushWidth = 20;

	[SerializeField]
	[Range(0f, 1f)]
	private float _maskBrushFuzziness = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _maskBrushOpacity = 0.15f;

	private bool canDestroyFOWTexture;

	[SerializeField]
	private Texture2D _fogOfWarTexture;

	private Color32[] fogOfWarColorBuffer;

	private FogOfWarTransition[] fowTransitionList;

	private int lastTransitionPos;

	private Dictionary<int, int> fowTransitionIndices;

	private bool requiresTextureUpload;

	private Material fowBlur;

	private RenderTexture fowBlur1;

	private RenderTexture fowBlur2;

	public const int MAX_POINT_LIGHTS = 6;

	public const bool LIGHT_SCATTERING_BLUR_ENABLED = false;

	public const bool USE_UNITY_SHADOW_MAP = false;

	public const bool USE_DIRECTIONAL_LIGHT_COOKIE = false;

	public const bool LIGHT_DIFFUSION_ENABLED = false;

	public static VolumetricFog instance
	{
		get
		{
			if (_fog == null)
			{
				if (Camera.main != null)
				{
					_fog = Camera.main.GetComponent<VolumetricFog>();
				}
				if (_fog == null)
				{
					Camera[] allCameras = Camera.allCameras;
					for (int i = 0; i < allCameras.Length; i++)
					{
						_fog = allCameras[i].GetComponent<VolumetricFog>();
						if (_fog != null)
						{
							break;
						}
					}
				}
			}
			return _fog;
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
				UpdatePreset();
				isDirty = true;
			}
		}
	}

	public VolumetricFogProfile profile
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
				}
				isDirty = true;
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
				isDirty = true;
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
				isDirty = true;
			}
		}
	}

	public bool debugDepthPass
	{
		get
		{
			return _debugPass;
		}
		set
		{
			if (value != _debugPass)
			{
				_debugPass = value;
				isDirty = true;
			}
		}
	}

	public bool showInSceneView
	{
		get
		{
			return _showInSceneView;
		}
		set
		{
			if (value != _showInSceneView)
			{
				_showInSceneView = value;
				isDirty = true;
			}
		}
	}

	public TRANSPARENT_MODE transparencyBlendMode
	{
		get
		{
			return _transparencyBlendMode;
		}
		set
		{
			if (value != _transparencyBlendMode)
			{
				_transparencyBlendMode = value;
				UpdateRenderComponents();
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float transparencyBlendPower
	{
		get
		{
			return _transparencyBlendPower;
		}
		set
		{
			if (value != _transparencyBlendPower)
			{
				_transparencyBlendPower = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public LayerMask transparencyLayerMask
	{
		get
		{
			return _transparencyLayerMask;
		}
		set
		{
			if ((int)_transparencyLayerMask != (int)value)
			{
				_transparencyLayerMask = value;
				isDirty = true;
			}
		}
	}

	public FOG_VISIBILITY_SCOPE visibilityScope
	{
		get
		{
			return _visibilityScope;
		}
		set
		{
			if (_visibilityScope != value)
			{
				_visibilityScope = value;
				isDirty = true;
			}
		}
	}

	public Bounds visibilityVolume
	{
		get
		{
			return _visibilityVolume;
		}
		set
		{
			if (_visibilityVolume != value)
			{
				_visibilityVolume = value;
				isDirty = true;
			}
		}
	}

	public LIGHTING_MODEL lightingModel
	{
		get
		{
			return _lightingModel;
		}
		set
		{
			if (value != _lightingModel)
			{
				_lightingModel = value;
				UpdateMaterialProperties();
				UpdateTexture();
				isDirty = true;
			}
		}
	}

	public bool enableMultipleCameras
	{
		get
		{
			return _enableMultipleCameras;
		}
		set
		{
			if (value != _enableMultipleCameras)
			{
				_enableMultipleCameras = value;
				UpdateMultiCameraSetup();
				isDirty = true;
			}
		}
	}

	public bool computeDepth
	{
		get
		{
			return _computeDepth;
		}
		set
		{
			if (value != _computeDepth)
			{
				_computeDepth = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public COMPUTE_DEPTH_SCOPE computeDepthScope
	{
		get
		{
			return _computeDepthScope;
		}
		set
		{
			if (value != _computeDepthScope)
			{
				_computeDepthScope = value;
				if (_computeDepthScope == COMPUTE_DEPTH_SCOPE.TreeBillboardsAndTransparentObjects)
				{
					_transparencyBlendMode = TRANSPARENT_MODE.None;
				}
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float transparencyCutOff
	{
		get
		{
			return _transparencyCutOff;
		}
		set
		{
			if (value != _transparencyCutOff)
			{
				_transparencyCutOff = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public bool renderBeforeTransparent
	{
		get
		{
			return _renderBeforeTransparent;
		}
		set
		{
			if (value != _renderBeforeTransparent)
			{
				_renderBeforeTransparent = value;
				if (_renderBeforeTransparent)
				{
					_transparencyBlendMode = TRANSPARENT_MODE.None;
				}
				UpdateRenderComponents();
				UpdateMaterialProperties();
				isDirty = true;
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
				UpdateSun();
				isDirty = true;
			}
		}
	}

	public float timeBetweenTextureUpdates
	{
		get
		{
			return _timeBetweenTextureUpdates;
		}
		set
		{
			if (value != _timeBetweenTextureUpdates)
			{
				_timeBetweenTextureUpdates = value;
				isDirty = true;
			}
		}
	}

	public bool sunCopyColor
	{
		get
		{
			return _sunCopyColor;
		}
		set
		{
			if (value != _sunCopyColor)
			{
				_sunCopyColor = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float density
	{
		get
		{
			return _density;
		}
		set
		{
			if (value != _density)
			{
				_preset = FOG_PRESET.Custom;
				_density = value;
				UpdateMaterialProperties();
				UpdateTextureAlpha();
				UpdateTexture();
				isDirty = true;
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
				_preset = FOG_PRESET.Custom;
				_noiseStrength = value;
				UpdateMaterialProperties();
				UpdateTextureAlpha();
				UpdateTexture();
				isDirty = true;
			}
		}
	}

	public float noiseFinalMultiplier
	{
		get
		{
			return _noiseFinalMultiplier;
		}
		set
		{
			if (value != _noiseFinalMultiplier)
			{
				_preset = FOG_PRESET.Custom;
				_noiseFinalMultiplier = value;
				UpdateMaterialProperties();
				UpdateTextureAlpha();
				UpdateTexture();
				isDirty = true;
			}
		}
	}

	public float noiseSparse
	{
		get
		{
			return _noiseSparse;
		}
		set
		{
			if (value != _noiseSparse)
			{
				_preset = FOG_PRESET.Custom;
				_noiseSparse = value;
				UpdateMaterialProperties();
				UpdateTextureAlpha();
				UpdateTexture();
				isDirty = true;
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
				_preset = FOG_PRESET.Custom;
				_distance = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float maxFogLength
	{
		get
		{
			return _maxFogLength;
		}
		set
		{
			if (value != _maxFogLength)
			{
				_maxFogLength = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float maxFogLengthFallOff
	{
		get
		{
			return _maxFogLengthFallOff;
		}
		set
		{
			if (value != _maxFogLengthFallOff)
			{
				_maxFogLengthFallOff = value;
				UpdateMaterialProperties();
				isDirty = true;
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
				_preset = FOG_PRESET.Custom;
				_distanceFallOff = value;
				UpdateMaterialProperties();
				isDirty = true;
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
				_preset = FOG_PRESET.Custom;
				_height = Mathf.Max(value, 0.0001f);
				UpdateMaterialProperties();
				isDirty = true;
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
				_preset = FOG_PRESET.Custom;
				_heightFallOff = Mathf.Clamp01(value);
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float deepObscurance
	{
		get
		{
			return _deepObscurance;
		}
		set
		{
			if (value != _deepObscurance && value >= 0f)
			{
				_preset = FOG_PRESET.Custom;
				_deepObscurance = Mathf.Clamp01(value);
				UpdateMaterialProperties();
				isDirty = true;
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
				_preset = FOG_PRESET.Custom;
				_baselineHeight = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public bool baselineRelativeToCamera
	{
		get
		{
			return _baselineRelativeToCamera;
		}
		set
		{
			if (value != _baselineRelativeToCamera)
			{
				_preset = FOG_PRESET.Custom;
				_baselineRelativeToCamera = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float baselineRelativeToCameraDelay
	{
		get
		{
			return _baselineRelativeToCameraDelay;
		}
		set
		{
			if (value != _baselineRelativeToCameraDelay)
			{
				_baselineRelativeToCameraDelay = value;
				UpdateMaterialProperties();
				isDirty = true;
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
			if (value != _noiseScale && value >= 0.2f)
			{
				_preset = FOG_PRESET.Custom;
				_noiseScale = value;
				UpdateMaterialProperties();
				isDirty = true;
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
				_preset = FOG_PRESET.Custom;
				_alpha = value;
				currentFogAlpha = _alpha;
				UpdateMaterialProperties();
				isDirty = true;
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
				_preset = FOG_PRESET.Custom;
				_color = value;
				currentFogColor = _color;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public Color specularColor
	{
		get
		{
			return _specularColor;
		}
		set
		{
			if (value != _specularColor)
			{
				_preset = FOG_PRESET.Custom;
				_specularColor = value;
				currentFogSpecularColor = _specularColor;
				UpdateTexture();
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float specularThreshold
	{
		get
		{
			return _specularThreshold;
		}
		set
		{
			if (value != _specularThreshold)
			{
				_preset = FOG_PRESET.Custom;
				_specularThreshold = value;
				UpdateTexture();
				isDirty = true;
			}
		}
	}

	public float specularIntensity
	{
		get
		{
			return _specularIntensity;
		}
		set
		{
			if (value != _specularIntensity)
			{
				_preset = FOG_PRESET.Custom;
				_specularIntensity = value;
				UpdateMaterialProperties();
				UpdateTexture();
				isDirty = true;
			}
		}
	}

	public Vector3 lightDirection
	{
		get
		{
			return _lightDirection;
		}
		set
		{
			if (value != _lightDirection)
			{
				_preset = FOG_PRESET.Custom;
				_lightDirection = value;
				UpdateMaterialProperties();
				UpdateTexture();
				isDirty = true;
			}
		}
	}

	public float lightIntensity
	{
		get
		{
			return _lightIntensity;
		}
		set
		{
			if (value != _lightIntensity)
			{
				_preset = FOG_PRESET.Custom;
				_lightIntensity = value;
				UpdateMaterialProperties();
				UpdateTexture();
				isDirty = true;
			}
		}
	}

	public Color lightColor
	{
		get
		{
			return _lightColor;
		}
		set
		{
			if (value != _lightColor)
			{
				_preset = FOG_PRESET.Custom;
				_lightColor = value;
				currentLightColor = _lightColor;
				UpdateMaterialProperties();
				UpdateTexture();
				isDirty = true;
			}
		}
	}

	public int updateTextureSpread
	{
		get
		{
			return _updateTextureSpread;
		}
		set
		{
			if (value != _updateTextureSpread)
			{
				_updateTextureSpread = value;
				isDirty = true;
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
				_preset = FOG_PRESET.Custom;
				_speed = value;
				if (!Application.isPlaying)
				{
					UpdateWindSpeedQuick();
				}
				isDirty = true;
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
				_preset = FOG_PRESET.Custom;
				_windDirection = value.normalized;
				if (!Application.isPlaying)
				{
					UpdateWindSpeedQuick();
				}
				isDirty = true;
			}
		}
	}

	public bool useRealTime
	{
		get
		{
			return _useRealTime;
		}
		set
		{
			if (value != _useRealTime)
			{
				_useRealTime = value;
				isDirty = true;
			}
		}
	}

	public Color skyColor
	{
		get
		{
			return _skyColor;
		}
		set
		{
			if (value != _skyColor)
			{
				_preset = FOG_PRESET.Custom;
				_skyColor = value;
				ComputeLightColor();
				UpdateMaterialProperties();
				isDirty = true;
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
				_preset = FOG_PRESET.Custom;
				_skyHaze = value;
				if (!Application.isPlaying)
				{
					UpdateWindSpeedQuick();
				}
				isDirty = true;
			}
		}
	}

	public float skyNoiseScale
	{
		get
		{
			return _skyNoiseScale;
		}
		set
		{
			if (value != _skyNoiseScale)
			{
				_skyNoiseScale = value;
				UpdateMaterialProperties();
				isDirty = true;
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
				_preset = FOG_PRESET.Custom;
				_skySpeed = value;
				isDirty = true;
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
				_preset = FOG_PRESET.Custom;
				_skyNoiseStrength = value;
				if (!Application.isPlaying)
				{
					UpdateWindSpeedQuick();
				}
				isDirty = true;
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
				_preset = FOG_PRESET.Custom;
				_skyAlpha = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float skyDepth
	{
		get
		{
			return _skyDepth;
		}
		set
		{
			if (value != _skyDepth)
			{
				_skyDepth = value;
				if (!Application.isPlaying)
				{
					UpdateWindSpeedQuick();
				}
				isDirty = true;
			}
		}
	}

	public GameObject character
	{
		get
		{
			return _character;
		}
		set
		{
			if (value != _character)
			{
				_character = value;
				isDirty = true;
			}
		}
	}

	public FOG_VOID_TOPOLOGY fogVoidTopology
	{
		get
		{
			return _fogVoidTopology;
		}
		set
		{
			if (value != _fogVoidTopology)
			{
				_fogVoidTopology = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float fogVoidFallOff
	{
		get
		{
			return _fogVoidFallOff;
		}
		set
		{
			if (value != _fogVoidFallOff)
			{
				_preset = FOG_PRESET.Custom;
				_fogVoidFallOff = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float fogVoidRadius
	{
		get
		{
			return _fogVoidRadius;
		}
		set
		{
			if (value != _fogVoidRadius)
			{
				_preset = FOG_PRESET.Custom;
				_fogVoidRadius = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public Vector3 fogVoidPosition
	{
		get
		{
			return _fogVoidPosition;
		}
		set
		{
			if (value != _fogVoidPosition)
			{
				_preset = FOG_PRESET.Custom;
				_fogVoidPosition = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float fogVoidDepth
	{
		get
		{
			return _fogVoidDepth;
		}
		set
		{
			if (value != _fogVoidDepth)
			{
				_preset = FOG_PRESET.Custom;
				_fogVoidDepth = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float fogVoidHeight
	{
		get
		{
			return _fogVoidHeight;
		}
		set
		{
			if (value != _fogVoidHeight)
			{
				_preset = FOG_PRESET.Custom;
				_fogVoidHeight = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	[Obsolete("Fog Void inverted is now deprecated. Use Fog Area settings.")]
	public bool fogVoidInverted
	{
		get
		{
			return _fogVoidInverted;
		}
		set
		{
			_fogVoidInverted = value;
		}
	}

	public bool fogAreaShowGizmos
	{
		get
		{
			return _fogAreaShowGizmos;
		}
		set
		{
			if (value != _fogAreaShowGizmos)
			{
				_fogAreaShowGizmos = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public GameObject fogAreaCenter
	{
		get
		{
			return _fogAreaCenter;
		}
		set
		{
			if (value != _fogAreaCenter)
			{
				_fogAreaCenter = value;
				isDirty = true;
			}
		}
	}

	public float fogAreaFallOff
	{
		get
		{
			return _fogAreaFallOff;
		}
		set
		{
			if (value != _fogAreaFallOff)
			{
				_fogAreaFallOff = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public FOG_AREA_FOLLOW_MODE fogAreaFollowMode
	{
		get
		{
			return _fogAreaFollowMode;
		}
		set
		{
			if (value != _fogAreaFollowMode)
			{
				_fogAreaFollowMode = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public FOG_AREA_TOPOLOGY fogAreaTopology
	{
		get
		{
			return _fogAreaTopology;
		}
		set
		{
			if (value != _fogAreaTopology)
			{
				_fogAreaTopology = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float fogAreaRadius
	{
		get
		{
			return _fogAreaRadius;
		}
		set
		{
			if (value != _fogAreaRadius)
			{
				_fogAreaRadius = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public Vector3 fogAreaPosition
	{
		get
		{
			return _fogAreaPosition;
		}
		set
		{
			if (value != _fogAreaPosition)
			{
				_fogAreaPosition = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float fogAreaDepth
	{
		get
		{
			return _fogAreaDepth;
		}
		set
		{
			if (value != _fogAreaDepth)
			{
				_fogAreaDepth = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float fogAreaHeight
	{
		get
		{
			return _fogAreaHeight;
		}
		set
		{
			if (value != _fogAreaHeight)
			{
				_fogAreaHeight = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public FOG_AREA_SORTING_MODE fogAreaSortingMode
	{
		get
		{
			return _fogAreaSortingMode;
		}
		set
		{
			if (value != _fogAreaSortingMode)
			{
				_fogAreaSortingMode = value;
				lastTimeSortInstances = 0f;
				isDirty = true;
			}
		}
	}

	public int fogAreaRenderOrder
	{
		get
		{
			return _fogAreaRenderOrder;
		}
		set
		{
			if (value != _fogAreaRenderOrder)
			{
				_fogAreaRenderOrder = value;
				lastTimeSortInstances = 0f;
				isDirty = true;
			}
		}
	}

	public bool pointLightTrackAuto
	{
		get
		{
			return _pointLightTrackingAuto;
		}
		set
		{
			if (value != _pointLightTrackingAuto)
			{
				_pointLightTrackingAuto = value;
				TrackPointLights();
				isDirty = true;
			}
		}
	}

	public Transform pointLightTrackingPivot
	{
		get
		{
			return _pointLightTrackingPivot;
		}
		set
		{
			if (value != _pointLightTrackingPivot)
			{
				_pointLightTrackingPivot = value;
				TrackPointLights();
				isDirty = true;
			}
		}
	}

	public int pointLightTrackingCount
	{
		get
		{
			return _pointLightTrackingCount;
		}
		set
		{
			if (value != _pointLightTrackingCount)
			{
				_pointLightTrackingCount = Mathf.Clamp(value, 0, 6);
				CheckPointLightData();
				TrackPointLights();
				isDirty = true;
			}
		}
	}

	public float pointLightTrackingCheckInterval
	{
		get
		{
			return _pointLightTrackingCheckInterval;
		}
		set
		{
			if (value != _pointLightTrackingCheckInterval)
			{
				_pointLightTrackingCheckInterval = value;
				TrackPointLights();
				isDirty = true;
			}
		}
	}

	public float pointLightTrackingNewLightsCheckInterval
	{
		get
		{
			return _pointLightTrackingNewLightsCheckInterval;
		}
		set
		{
			if (value != _pointLightTrackingNewLightsCheckInterval)
			{
				_pointLightTrackingNewLightsCheckInterval = value;
				TrackPointLights();
				isDirty = true;
			}
		}
	}

	public float pointLightInscattering
	{
		get
		{
			return _pointLightInscattering;
		}
		set
		{
			if (value != _pointLightInscattering)
			{
				_pointLightInscattering = value;
				isDirty = true;
			}
		}
	}

	public float pointLightIntensity
	{
		get
		{
			return _pointLightIntensity;
		}
		set
		{
			if (value != _pointLightIntensity)
			{
				_pointLightIntensity = value;
				isDirty = true;
			}
		}
	}

	public float pointLightInsideAtten
	{
		get
		{
			return _pointLightInsideAtten;
		}
		set
		{
			if (value != _pointLightInsideAtten)
			{
				_pointLightInsideAtten = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public int downsampling
	{
		get
		{
			return _downsampling;
		}
		set
		{
			if (value != _downsampling)
			{
				_preset = FOG_PRESET.Custom;
				_downsampling = value;
				isDirty = true;
			}
		}
	}

	public bool forceComposition
	{
		get
		{
			return _forceComposition;
		}
		set
		{
			if (value != _forceComposition)
			{
				_preset = FOG_PRESET.Custom;
				_forceComposition = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public bool edgeImprove
	{
		get
		{
			return _edgeImprove;
		}
		set
		{
			if (value != _edgeImprove)
			{
				_preset = FOG_PRESET.Custom;
				_edgeImprove = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float edgeThreshold
	{
		get
		{
			return _edgeThreshold;
		}
		set
		{
			if (value != _edgeThreshold)
			{
				_preset = FOG_PRESET.Custom;
				_edgeThreshold = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float stepping
	{
		get
		{
			return _stepping;
		}
		set
		{
			if (value != _stepping)
			{
				_preset = FOG_PRESET.Custom;
				_stepping = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float steppingNear
	{
		get
		{
			return _steppingNear;
		}
		set
		{
			if (value != _steppingNear)
			{
				_preset = FOG_PRESET.Custom;
				_steppingNear = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public bool dithering
	{
		get
		{
			return _dithering;
		}
		set
		{
			if (value != _dithering)
			{
				_dithering = value;
				UpdateMaterialProperties();
				isDirty = true;
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
				isDirty = true;
			}
		}
	}

	public float jitterStrength
	{
		get
		{
			return _jitterStrength;
		}
		set
		{
			if (value != _jitterStrength)
			{
				_jitterStrength = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public bool lightScatteringEnabled
	{
		get
		{
			return _lightScatteringEnabled;
		}
		set
		{
			if (value != _lightScatteringEnabled)
			{
				_lightScatteringEnabled = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float lightScatteringDiffusion
	{
		get
		{
			return _lightScatteringDiffusion;
		}
		set
		{
			if (value != _lightScatteringDiffusion)
			{
				_lightScatteringDiffusion = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float lightScatteringSpread
	{
		get
		{
			return _lightScatteringSpread;
		}
		set
		{
			if (value != _lightScatteringSpread)
			{
				_lightScatteringSpread = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public int lightScatteringSamples
	{
		get
		{
			return _lightScatteringSamples;
		}
		set
		{
			if (value != _lightScatteringSamples)
			{
				_lightScatteringSamples = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float lightScatteringWeight
	{
		get
		{
			return _lightScatteringWeight;
		}
		set
		{
			if (value != _lightScatteringWeight)
			{
				_lightScatteringWeight = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float lightScatteringIllumination
	{
		get
		{
			return _lightScatteringIllumination;
		}
		set
		{
			if (value != _lightScatteringIllumination)
			{
				_lightScatteringIllumination = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float lightScatteringDecay
	{
		get
		{
			return _lightScatteringDecay;
		}
		set
		{
			if (value != _lightScatteringDecay)
			{
				_lightScatteringDecay = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float lightScatteringExposure
	{
		get
		{
			return _lightScatteringExposure;
		}
		set
		{
			if (value != _lightScatteringExposure)
			{
				_lightScatteringExposure = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float lightScatteringJittering
	{
		get
		{
			return _lightScatteringJittering;
		}
		set
		{
			if (value != _lightScatteringJittering)
			{
				_lightScatteringJittering = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public int lightScatteringBlurDownscale
	{
		get
		{
			return _lightScatteringBlurDownscale;
		}
		set
		{
			if (value != _lightScatteringBlurDownscale)
			{
				_lightScatteringBlurDownscale = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public bool fogBlur
	{
		get
		{
			return _fogBlur;
		}
		set
		{
			if (value != _fogBlur)
			{
				_fogBlur = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float fogBlurDepth
	{
		get
		{
			return _fogBlurDepth;
		}
		set
		{
			if (value != _fogBlurDepth)
			{
				_fogBlurDepth = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public bool sunShadows
	{
		get
		{
			return _sunShadows;
		}
		set
		{
			if (value != _sunShadows)
			{
				_sunShadows = value;
				CleanUpTextureDepthSun();
				if (_sunShadows)
				{
					needUpdateDepthSunTexture = true;
				}
				else
				{
					DestroySunShadowsDependencies();
				}
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public LayerMask sunShadowsLayerMask
	{
		get
		{
			return _sunShadowsLayerMask;
		}
		set
		{
			if ((int)_sunShadowsLayerMask != (int)value)
			{
				_sunShadowsLayerMask = value;
				isDirty = true;
			}
		}
	}

	public float sunShadowsStrength
	{
		get
		{
			return _sunShadowsStrength;
		}
		set
		{
			if (value != _sunShadowsStrength)
			{
				_sunShadowsStrength = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float sunShadowsBias
	{
		get
		{
			return _sunShadowsBias;
		}
		set
		{
			if (value != _sunShadowsBias)
			{
				_sunShadowsBias = value;
				needUpdateDepthSunTexture = true;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float sunShadowsJitterStrength
	{
		get
		{
			return _sunShadowsJitterStrength;
		}
		set
		{
			if (value != _sunShadowsJitterStrength)
			{
				_sunShadowsJitterStrength = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public int sunShadowsResolution
	{
		get
		{
			return _sunShadowsResolution;
		}
		set
		{
			if (value != _sunShadowsResolution)
			{
				_sunShadowsResolution = value;
				needUpdateDepthSunTexture = true;
				CleanUpTextureDepthSun();
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float sunShadowsMaxDistance
	{
		get
		{
			return _sunShadowsMaxDistance;
		}
		set
		{
			if (value != _sunShadowsMaxDistance)
			{
				_sunShadowsMaxDistance = value;
				needUpdateDepthSunTexture = true;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public SUN_SHADOWS_BAKE_MODE sunShadowsBakeMode
	{
		get
		{
			return _sunShadowsBakeMode;
		}
		set
		{
			if (value != _sunShadowsBakeMode)
			{
				_sunShadowsBakeMode = value;
				needUpdateDepthSunTexture = true;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float sunShadowsRefreshInterval
	{
		get
		{
			return _sunShadowsRefreshInterval;
		}
		set
		{
			if (value != _sunShadowsRefreshInterval)
			{
				_sunShadowsRefreshInterval = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float sunShadowsCancellation
	{
		get
		{
			return _sunShadowsCancellation;
		}
		set
		{
			if (value != _sunShadowsCancellation)
			{
				_sunShadowsCancellation = value;
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float turbulenceStrength
	{
		get
		{
			return _turbulenceStrength;
		}
		set
		{
			if (value != _turbulenceStrength)
			{
				_turbulenceStrength = value;
				if (_turbulenceStrength <= 0f)
				{
					UpdateTexture();
				}
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public bool useXYPlane
	{
		get
		{
			return _useXYPlane;
		}
		set
		{
			if (value != _useXYPlane)
			{
				_useXYPlane = value;
				if (_sunShadows)
				{
					needUpdateDepthSunTexture = true;
				}
				UpdateMaterialProperties();
				isDirty = true;
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
				isDirty = true;
			}
		}
	}

	public SPSR_BEHAVIOUR spsrBehaviour
	{
		get
		{
			return _spsrBehaviour;
		}
		set
		{
			if (value != _spsrBehaviour)
			{
				_spsrBehaviour = value;
				isDirty = true;
			}
		}
	}

	public bool reduceFlickerBigWorlds
	{
		get
		{
			return _reduceFlickerBigWorlds;
		}
		set
		{
			if (value != _reduceFlickerBigWorlds)
			{
				_reduceFlickerBigWorlds = value;
				isDirty = true;
			}
		}
	}

	public bool enableMask
	{
		get
		{
			return _enableMask;
		}
		set
		{
			if (value != _enableMask)
			{
				_enableMask = value;
				UpdateVolumeMask();
				isDirty = true;
			}
		}
	}

	public LayerMask maskLayer
	{
		get
		{
			return _maskLayer;
		}
		set
		{
			if ((int)value != (int)_maskLayer)
			{
				_maskLayer = value;
				UpdateVolumeMask();
				isDirty = true;
			}
		}
	}

	public int maskDownsampling
	{
		get
		{
			return _maskDownsampling;
		}
		set
		{
			if (value != _maskDownsampling)
			{
				_maskDownsampling = value;
				UpdateVolumeMask();
				isDirty = true;
			}
		}
	}

	public Camera fogCamera => mainCamera;

	public int renderingInstancesCount => _renderingInstancesCount;

	public List<VolumetricFog> renderingInstances => fogRenderInstances;

	public List<VolumetricFog> instances => fogInstances;

	public bool hasCamera
	{
		get
		{
			if (!_hasCameraChecked)
			{
				_hasCamera = GetComponent<Camera>() != null;
				_hasCameraChecked = true;
			}
			return _hasCamera;
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
				FogOfWarInit();
				UpdateMaterialProperties();
				isDirty = true;
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
				isDirty = true;
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
			if (value != _fogOfWarSize && value.x > 0f && value.z > 0f)
			{
				_fogOfWarSize = value;
				UpdateMaterialProperties();
				isDirty = true;
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
			if (value != _fogOfWarTextureSize && value > 16)
			{
				_fogOfWarTextureSize = value;
				FogOfWarUpdateTexture();
				UpdateMaterialProperties();
				isDirty = true;
			}
		}
	}

	public float fogOfWarRestoreDelay
	{
		get
		{
			return _fogOfWarRestoreDelay;
		}
		set
		{
			if (value != _fogOfWarRestoreDelay)
			{
				_fogOfWarRestoreDelay = value;
				isDirty = true;
			}
		}
	}

	public float fogOfWarRestoreDuration
	{
		get
		{
			return _fogOfWarRestoreDuration;
		}
		set
		{
			if (value != _fogOfWarRestoreDuration)
			{
				_fogOfWarRestoreDuration = value;
				isDirty = true;
			}
		}
	}

	public float fogOfWarSmoothness
	{
		get
		{
			return _fogOfWarSmoothness;
		}
		set
		{
			if (value != _fogOfWarSmoothness)
			{
				_fogOfWarSmoothness = value;
				isDirty = true;
			}
		}
	}

	public bool fogOfWarBlur
	{
		get
		{
			return _fogOfWarBlur;
		}
		set
		{
			if (value != _fogOfWarBlur)
			{
				_fogOfWarBlur = value;
				isDirty = true;
			}
		}
	}

	public bool maskEditorEnabled
	{
		get
		{
			return _maskEditorEnabled;
		}
		set
		{
			if (value != _maskEditorEnabled)
			{
				_maskEditorEnabled = value;
			}
		}
	}

	public MASK_TEXTURE_BRUSH_MODE maskBrushMode
	{
		get
		{
			return _maskBrushMode;
		}
		set
		{
			if (value != _maskBrushMode)
			{
				_maskBrushMode = value;
			}
		}
	}

	public int maskBrushWidth
	{
		get
		{
			return _maskBrushWidth;
		}
		set
		{
			if (value != _maskBrushWidth)
			{
				_maskBrushWidth = value;
			}
		}
	}

	public float maskBrushFuzziness
	{
		get
		{
			return _maskBrushFuzziness;
		}
		set
		{
			if (value != _maskBrushFuzziness)
			{
				_maskBrushFuzziness = value;
			}
		}
	}

	public float maskBrushOpacity
	{
		get
		{
			return _maskBrushOpacity;
		}
		set
		{
			if (value != _maskBrushOpacity)
			{
				_maskBrushOpacity = value;
			}
		}
	}

	public Texture2D fogOfWarTexture
	{
		get
		{
			return _fogOfWarTexture;
		}
		set
		{
			if (_fogOfWarTexture != value && value != null)
			{
				if (value.width != value.height)
				{
					Debug.LogError("Fog of war texture must be square.");
					return;
				}
				_fogOfWarTexture = value;
				canDestroyFOWTexture = false;
				ReloadFogOfWarTexture();
			}
		}
	}

	public Color32[] fogOfWarTextureData
	{
		get
		{
			return fogOfWarColorBuffer;
		}
		set
		{
			fogOfWarEnabled = true;
			fogOfWarColorBuffer = value;
			if (value != null && !(_fogOfWarTexture == null) && value.Length == _fogOfWarTexture.width * _fogOfWarTexture.height)
			{
				_fogOfWarTexture.SetPixels32(fogOfWarColorBuffer);
				_fogOfWarTexture.Apply();
			}
		}
	}

	private void InitDepthTextureForFog()
	{
		_depthTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
		_depthTexture.name = "DepthTextureForFog";
		_commandBufferDepthTexture = new CommandBuffer();
		_commandBufferDepthTexture.name = "CommandBuffer_DepthTextureForFog";
	}

	private void GenerateDepthTextureForFog()
	{
		if (_commandBufferDepthTexture != null && !(_depthTexture == null))
		{
			_commandBufferDepthTexture.Clear();
			_commandBufferDepthTexture.SetRenderTarget(_depthTexture);
			_commandBufferDepthTexture.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black);
			for (int i = 0; i < _renderersForDepth.Length; i++)
			{
				_commandBufferDepthTexture.DrawRenderer(_renderersForDepth[i], _materialDrawDepth);
			}
			Graphics.ExecuteCommandBuffer(_commandBufferDepthTexture);
		}
	}

	private void OnEnable()
	{
		isPartOfScene = isPartOfScene || IsPartOfScene();
		if (!isPartOfScene)
		{
			return;
		}
		temporaryProperties.color = Color.white;
		temporaryProperties.density = 1f;
		if (_fogVoidInverted)
		{
			_fogVoidInverted = false;
			_fogAreaCenter = _character;
			_fogAreaDepth = _fogVoidDepth;
			_fogAreaFallOff = _fogVoidFallOff;
			_fogAreaHeight = _fogVoidHeight;
			_fogAreaPosition = _fogVoidPosition;
			_fogAreaRadius = _fogVoidRadius;
			_fogVoidRadius = 0f;
			_character = null;
		}
		mainCamera = base.gameObject.GetComponent<Camera>();
		_hasCamera = mainCamera != null;
		_hasCameraChecked = true;
		if (_hasCamera)
		{
			fogRenderer = this;
			if (mainCamera.depthTextureMode == DepthTextureMode.None)
			{
				mainCamera.depthTextureMode = DepthTextureMode.Depth;
			}
			UpdateVolumeMask();
		}
		else if (fogRenderer == null)
		{
			FindMainCamera();
			if (mainCamera == null)
			{
				Debug.LogError("Volumetric Fog: no camera found!");
				return;
			}
			fogRenderer = mainCamera.GetComponent<VolumetricFog>();
			if (fogRenderer == null)
			{
				fogRenderer = mainCamera.gameObject.AddComponent<VolumetricFog>();
				fogRenderer.density = 0f;
			}
		}
		else
		{
			mainCamera = fogRenderer.mainCamera;
			if (mainCamera == null)
			{
				mainCamera = fogRenderer.GetComponent<Camera>();
			}
		}
		if (fogMat == null)
		{
			InitFogMaterial();
			if (_profile != null && _profileSync)
			{
				_profile.Load(this);
			}
		}
		else
		{
			UpdateMaterialPropertiesNow();
		}
		RegisterWithRenderers();
		needResort = true;
	}

	private void OnDisable()
	{
		RemoveMaskCommandBuffer();
		RemoveDirectionalLightCommandBuffer();
	}

	private void OnDestroy()
	{
		if (!_hasCamera)
		{
			UnregisterWithRenderers();
		}
		else
		{
			RemoveMaskCommandBuffer();
			UnregisterFogArea(this);
		}
		if (depthCamObj != null)
		{
			UnityEngine.Object.DestroyImmediate(depthCamObj);
			depthCamObj = null;
		}
		if (adjustedTexture != null)
		{
			UnityEngine.Object.DestroyImmediate(adjustedTexture);
			adjustedTexture = null;
		}
		if (chaosLerpMat != null)
		{
			UnityEngine.Object.DestroyImmediate(chaosLerpMat);
			chaosLerpMat = null;
		}
		if (adjustedChaosTexture != null)
		{
			UnityEngine.Object.DestroyImmediate(adjustedChaosTexture);
			adjustedChaosTexture = null;
		}
		if (blurMat != null)
		{
			UnityEngine.Object.DestroyImmediate(blurMat);
			blurMat = null;
		}
		if (fogMat != null)
		{
			UnityEngine.Object.DestroyImmediate(fogMat);
			fogMat = null;
		}
		FogOfWarDestroy();
		CleanUpDepthTexture();
		DestroySunShadowsDependencies();
		if (_fog == this)
		{
			_fog = null;
		}
	}

	public void DestroySelf()
	{
		DestroyRenderComponent<VolumetricFogPreT>();
		DestroyRenderComponent<VolumetricFogPosT>();
		UnityEngine.Object.DestroyImmediate(this);
	}

	private void Start()
	{
		currentFogAlpha = _alpha;
		currentSkyHazeAlpha = _skyAlpha;
		lastTextureUpdate = Time.time + _timeBetweenTextureUpdates;
		RegisterWithRenderers();
		InitDepthTextureForFog();
		Update();
	}

	private void Update()
	{
		if (!isPartOfScene || fogRenderer == null)
		{
			return;
		}
		if (fogRenderer.sun != null)
		{
			_lightDirection = fogRenderer.sun.transform.forward;
			if (!Application.isPlaying || (updatingTextureSlice < 0 && Time.time - lastTextureUpdate >= _timeBetweenTextureUpdates))
			{
				if (lastLightDirection != _lightDirection)
				{
					lastLightDirection = _lightDirection;
					needUpdateTexture = true;
					needUpdateDepthSunTexture = true;
				}
				if (sunLight != null)
				{
					if (_sunCopyColor && sunLight.color != _lightColor)
					{
						_lightColor = sunLight.color;
						currentLightColor = _lightColor;
						needUpdateTexture = true;
					}
					if (sunLightIntensity != sunLight.intensity)
					{
						sunLightIntensity = sunLight.intensity;
						needUpdateTexture = true;
					}
				}
			}
		}
		if (!needUpdateTexture)
		{
			if (_lightingModel == LIGHTING_MODEL.Classic)
			{
				if (lastRenderSettingsAmbientIntensity != RenderSettings.ambientIntensity)
				{
					needUpdateTexture = true;
				}
				else if (lastRenderSettingsAmbientLight != RenderSettings.ambientLight)
				{
					needUpdateTexture = true;
				}
			}
			else if (_lightingModel == LIGHTING_MODEL.Natural && lastRenderSettingsAmbientLight != RenderSettings.ambientLight)
			{
				needUpdateTexture = true;
			}
		}
		if (transitionProfile)
		{
			float num = (Time.time - transitionStartTime) / transitionDuration;
			if (num > 1f)
			{
				num = 1f;
			}
			VolumetricFogProfile.Lerp(initialProfile, targetProfile, num, this);
			if (num >= 1f)
			{
				transitionProfile = false;
			}
		}
		if (transitionAlpha)
		{
			if (targetFogAlpha >= 0f || targetSkyHazeAlpha >= 0f)
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
					UpdateSkyColor(currentSkyHazeAlpha);
				}
			}
			else if (currentFogAlpha != _alpha || currentSkyHazeAlpha != _skyAlpha)
			{
				if (transitionDuration > 0f)
				{
					currentFogAlpha = Mathf.Lerp(initialFogAlpha, _alpha, (Time.time - transitionStartTime) / transitionDuration);
					currentSkyHazeAlpha = Mathf.Lerp(initialSkyHazeAlpha, alpha, (Time.time - transitionStartTime) / transitionDuration);
				}
				else
				{
					currentFogAlpha = _alpha;
					currentSkyHazeAlpha = _skyAlpha;
					transitionAlpha = false;
				}
				fogMat.SetFloat("_FogAlpha", currentFogAlpha);
				UpdateSkyColor(currentSkyHazeAlpha);
			}
		}
		if (transitionColor)
		{
			if (targetColorActive)
			{
				if (targetFogColor != currentFogColor)
				{
					if (transitionDuration > 0f)
					{
						currentFogColor = Color.Lerp(initialFogColor, targetFogColor, (Time.time - transitionStartTime) / transitionDuration);
					}
					else
					{
						currentFogColor = targetFogColor;
						transitionColor = false;
					}
				}
			}
			else if (currentFogColor != _color)
			{
				if (transitionDuration > 0f)
				{
					currentFogColor = Color.Lerp(initialFogColor, _color, (Time.time - transitionStartTime) / transitionDuration);
				}
				else
				{
					currentFogColor = _color;
					transitionColor = false;
				}
			}
			UpdateMaterialFogColor();
		}
		if (transitionSpecularColor)
		{
			if (targetSpecularColorActive)
			{
				if (targetFogSpecularColor != currentFogSpecularColor)
				{
					if (transitionDuration > 0f)
					{
						currentFogSpecularColor = Color.Lerp(initialFogSpecularColor, targetFogSpecularColor, (Time.time - transitionStartTime) / transitionDuration);
					}
					else
					{
						currentFogSpecularColor = targetFogSpecularColor;
						transitionSpecularColor = false;
					}
					needUpdateTexture = true;
				}
			}
			else if (currentFogSpecularColor != _specularColor)
			{
				if (transitionDuration > 0f)
				{
					currentFogSpecularColor = Color.Lerp(initialFogSpecularColor, _specularColor, (Time.time - transitionStartTime) / transitionDuration);
				}
				else
				{
					currentFogSpecularColor = _specularColor;
					transitionSpecularColor = false;
				}
				needUpdateTexture = true;
			}
		}
		if (transitionLightColor)
		{
			if (targetLightColorActive)
			{
				if (targetLightColor != currentLightColor)
				{
					if (transitionDuration > 0f)
					{
						currentLightColor = Color.Lerp(initialLightColor, targetLightColor, (Time.time - transitionStartTime) / transitionDuration);
					}
					else
					{
						currentLightColor = targetLightColor;
						transitionLightColor = false;
					}
					needUpdateTexture = true;
				}
			}
			else if (currentLightColor != _lightColor)
			{
				if (transitionDuration > 0f)
				{
					currentLightColor = Color.Lerp(initialLightColor, _lightColor, (Time.time - transitionStartTime) / transitionDuration);
				}
				else
				{
					currentLightColor = _lightColor;
					transitionLightColor = false;
				}
				needUpdateTexture = true;
			}
		}
		if (_baselineRelativeToCamera)
		{
			UpdateMaterialHeights(mainCamera);
		}
		else if (_character != null)
		{
			_fogVoidPosition = _character.transform.position;
			UpdateMaterialHeights(mainCamera);
		}
		if (_fogAreaCenter != null)
		{
			if (_fogAreaFollowMode == FOG_AREA_FOLLOW_MODE.FullXYZ)
			{
				_fogAreaPosition = _fogAreaCenter.transform.position;
			}
			else
			{
				_fogAreaPosition.x = _fogAreaCenter.transform.position.x;
				_fogAreaPosition.z = _fogAreaCenter.transform.position.z;
			}
			UpdateMaterialHeights(mainCamera);
		}
		if (_pointLightTrackingAuto && (!Application.isPlaying || Time.time - trackPointAutoLastTime > _pointLightTrackingCheckInterval))
		{
			trackPointAutoLastTime = Time.time;
			TrackPointLights();
		}
		if (updatingTextureSlice >= 0)
		{
			UpdateTextureColors(adjustedColors, forceUpdateEntireTexture: false);
		}
		else if (needUpdateTexture)
		{
			UpdateTexture();
		}
		if (!_hasCamera)
		{
			return;
		}
		if (_fogOfWarEnabled)
		{
			UpdateFogOfWar();
		}
		if (sunShadowsActive)
		{
			CastSunShadows();
		}
		int count = fogInstances.Count;
		if (count <= 1)
		{
			return;
		}
		Vector3 position = mainCamera.transform.position;
		if (!Application.isPlaying || Time.time - lastTimeSortInstances >= 2f)
		{
			needResort = true;
		}
		if (!needResort && (position.x - lastCamPos.x) * (position.x - lastCamPos.x) + (position.y - lastCamPos.y) * (position.y - lastCamPos.y) + (position.z - lastCamPos.z) * (position.z - lastCamPos.z) > 625f)
		{
			lastCamPos = position;
			needResort = true;
		}
		if (!needResort)
		{
			return;
		}
		needResort = false;
		lastTimeSortInstances = Time.time;
		float x = position.x;
		float y = position.y;
		float z = position.z;
		for (int i = 0; i < count; i++)
		{
			VolumetricFog volumetricFog = fogInstances[i];
			if (volumetricFog != null)
			{
				Vector3 position2 = volumetricFog.transform.position;
				position2.y = volumetricFog.currentFogAltitude;
				float num2 = x - position2.x;
				float num3 = y - position2.y;
				float num4 = num3 * num3;
				float num5 = y - (position2.y + volumetricFog.height);
				float num6 = num5 * num5;
				volumetricFog.distanceToCameraYAxis = ((num4 < num6) ? num4 : num6);
				float num7 = z - position2.z;
				float num8 = num2 * num2 + num3 * num3 + num7 * num7;
				volumetricFog.distanceToCamera = num8;
				Vector3 position3 = position2 - volumetricFog.transform.localScale * 0.5f;
				Vector3 position4 = position2 + volumetricFog.transform.localScale * 0.5f;
				volumetricFog.distanceToCameraMin = mainCamera.WorldToScreenPoint(position3).z;
				volumetricFog.distanceToCameraMax = mainCamera.WorldToScreenPoint(position4).z;
			}
		}
		fogInstances.Sort(delegate(VolumetricFog volumetricFog2, VolumetricFog volumetricFog3)
		{
			if (!volumetricFog2 || !volumetricFog3)
			{
				return 0;
			}
			if (volumetricFog2._fogAreaSortingMode == FOG_AREA_SORTING_MODE.Fixed || volumetricFog3._fogAreaSortingMode == FOG_AREA_SORTING_MODE.Fixed)
			{
				if (volumetricFog2._fogAreaRenderOrder < volumetricFog3._fogAreaRenderOrder)
				{
					return -1;
				}
				if (volumetricFog2._fogAreaRenderOrder > volumetricFog3._fogAreaRenderOrder)
				{
					return 1;
				}
				return 0;
			}
			if ((volumetricFog2.distanceToCameraMin < volumetricFog3.distanceToCameraMin && volumetricFog2.distanceToCameraMax > volumetricFog3.distanceToCameraMax) || (volumetricFog3.distanceToCameraMin < volumetricFog2.distanceToCameraMin && volumetricFog3.distanceToCameraMax > volumetricFog2.distanceToCameraMax) || volumetricFog2._fogAreaSortingMode == FOG_AREA_SORTING_MODE.Altitude || volumetricFog3._fogAreaSortingMode == FOG_AREA_SORTING_MODE.Altitude)
			{
				if (volumetricFog2.distanceToCameraYAxis < volumetricFog3.distanceToCameraYAxis)
				{
					return 1;
				}
				if (volumetricFog2.distanceToCameraYAxis > volumetricFog3.distanceToCameraYAxis)
				{
					return -1;
				}
				return 0;
			}
			if (volumetricFog2.distanceToCamera < volumetricFog3.distanceToCamera)
			{
				return 1;
			}
			return (volumetricFog2.distanceToCamera > volumetricFog3.distanceToCamera) ? (-1) : 0;
		});
	}

	public void OnPreCull()
	{
		if (!base.enabled || !base.gameObject.activeSelf || fogMat == null || !_hasCamera || mainCamera == null)
		{
			return;
		}
		if (mainCamera.depthTextureMode == DepthTextureMode.None)
		{
			mainCamera.depthTextureMode = DepthTextureMode.Depth;
		}
		if (_computeDepth)
		{
			GetTransparentDepth();
		}
		if (!_hasCamera || !Application.isPlaying)
		{
			return;
		}
		int count = fogRenderInstances.Count;
		for (int i = 0; i < count; i++)
		{
			if (fogRenderInstances[i] != null && fogRenderInstances[i].turbulenceStrength > 0f)
			{
				fogRenderInstances[i].ApplyChaos();
			}
		}
	}

	private void OnPostRender()
	{
		if (!_hasCamera || !Application.isPlaying)
		{
			return;
		}
		int count = fogRenderInstances.Count;
		for (int i = 0; i < count; i++)
		{
			if (fogRenderInstances[i] != null)
			{
				fogRenderInstances[i].DoOnPostRender();
			}
		}
	}

	private void DoOnPostRender()
	{
		if (rtAdjusted != null)
		{
			RenderTexture.ReleaseTemporary(rtAdjusted);
		}
	}

	private void OnDidApplyAnimationProperties()
	{
		shouldUpdateMaterialProperties = true;
	}

	private void FindMainCamera()
	{
		mainCamera = Camera.main;
		if (!(mainCamera == null))
		{
			return;
		}
		Camera[] array = UnityEngine.Object.FindObjectsOfType<Camera>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].isActiveAndEnabled)
			{
				mainCamera = array[i];
				break;
			}
		}
	}

	private bool IsPartOfScene()
	{
		VolumetricFog[] array = UnityEngine.Object.FindObjectsOfType<VolumetricFog>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == this)
			{
				return true;
			}
		}
		return false;
	}

	private void InitFogMaterial()
	{
		targetFogAlpha = -1f;
		targetSkyHazeAlpha = -1f;
		_skyColor.a = _skyAlpha;
		updatingTextureSlice = -1;
		fogMat = new Material(Shader.Find("VolumetricFogAndMist/VolumetricFog"));
		fogMat.hideFlags = HideFlags.DontSave;
		fogMat.name = "FogMat " + base.name;
		Texture2D texture2D = Resources.Load<Texture2D>("Textures/Noise3");
		noiseTextureSize = texture2D.width;
		noiseColors = texture2D.GetPixels();
		adjustedColors = new Color[noiseColors.Length];
		adjustedTexture = new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGBA32, mipChain: false);
		adjustedTexture.hideFlags = HideFlags.DontSave;
		timeOfLastRender = Time.time;
		CheckPointLightData();
		if (_pointLightTrackingAuto)
		{
			TrackPointLights();
		}
		FogOfWarInit();
		CopyTransitionValues();
		UpdatePreset();
		oldBaselineRelativeCameraY = mainCamera.transform.position.y;
		if (_sunShadows)
		{
			needUpdateDepthSunTexture = true;
		}
	}

	private void UpdateRenderComponents()
	{
		if (_hasCamera)
		{
			if (_renderBeforeTransparent)
			{
				AssignRenderComponent<VolumetricFogPreT>();
				DestroyRenderComponent<VolumetricFogPosT>();
			}
			else if (_transparencyBlendMode == TRANSPARENT_MODE.Blend)
			{
				AssignRenderComponent<VolumetricFogPreT>();
				AssignRenderComponent<VolumetricFogPosT>();
			}
			else
			{
				AssignRenderComponent<VolumetricFogPosT>();
				DestroyRenderComponent<VolumetricFogPreT>();
			}
		}
	}

	private void DestroyRenderComponent<T>() where T : IVolumetricFogRenderComponent
	{
		T[] componentsInChildren = GetComponentsInChildren<T>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].fog == this || componentsInChildren[i].fog == null)
			{
				componentsInChildren[i].DestroySelf();
			}
		}
	}

	private void AssignRenderComponent<T>() where T : Component, IVolumetricFogRenderComponent
	{
		T[] componentsInChildren = GetComponentsInChildren<T>(includeInactive: true);
		int num = -1;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].fog == this)
			{
				return;
			}
			if (componentsInChildren[i].fog == null)
			{
				num = i;
			}
		}
		if (num < 0)
		{
			base.gameObject.AddComponent<T>().fog = this;
		}
		else
		{
			componentsInChildren[num].fog = this;
		}
	}

	private void RegisterFogArea(VolumetricFog fog)
	{
		if (!fogInstances.Contains(fog))
		{
			fogInstances.Add(fog);
		}
	}

	private void UnregisterFogArea(VolumetricFog fog)
	{
		if (fogInstances.Contains(fog))
		{
			fogInstances.Remove(fog);
		}
	}

	private void RegisterWithRenderers()
	{
		allFogRenderers = UnityEngine.Object.FindObjectsOfType<VolumetricFog>();
		if (!_hasCamera && fogRenderer != null)
		{
			if (fogRenderer.enableMultipleCameras)
			{
				for (int i = 0; i < allFogRenderers.Length; i++)
				{
					if (allFogRenderers[i].hasCamera)
					{
						allFogRenderers[i].RegisterFogArea(this);
					}
				}
			}
			else
			{
				fogRenderer.RegisterFogArea(this);
			}
		}
		else
		{
			fogInstances.Clear();
			RegisterFogArea(this);
			for (int j = 0; j < allFogRenderers.Length; j++)
			{
				if (!allFogRenderers[j].hasCamera && (_enableMultipleCameras || allFogRenderers[j].fogRenderer == this))
				{
					RegisterFogArea(allFogRenderers[j]);
				}
			}
		}
		lastTimeSortInstances = 0f;
	}

	private void UnregisterWithRenderers()
	{
		if (allFogRenderers == null)
		{
			return;
		}
		for (int i = 0; i < allFogRenderers.Length; i++)
		{
			if (allFogRenderers[i] != null && allFogRenderers[i].hasCamera)
			{
				allFogRenderers[i].UnregisterFogArea(this);
			}
		}
	}

	public void UpdateMultiCameraSetup()
	{
		allFogRenderers = UnityEngine.Object.FindObjectsOfType<VolumetricFog>();
		for (int i = 0; i < allFogRenderers.Length; i++)
		{
			if (allFogRenderers[i] != null && allFogRenderers[i].hasCamera)
			{
				allFogRenderers[i].SetEnableMultipleCameras(_enableMultipleCameras);
			}
		}
		RegisterWithRenderers();
	}

	private void SetEnableMultipleCameras(bool state)
	{
		_enableMultipleCameras = state;
		RegisterWithRenderers();
	}

	private void OnRenderObject()
	{
		GenerateDepthTextureForFog();
	}

	internal void DoOnRenderImage(RenderTexture source, RenderTexture destination)
	{
		int count = fogInstances.Count;
		fogRenderInstances.Clear();
		Vector3 position = Camera.current.transform.position;
		for (int i = 0; i < count; i++)
		{
			VolumetricFog volumetricFog = fogInstances[i];
			volumetricFog.isRendering = false;
			if (volumetricFog != null && volumetricFog.isActiveAndEnabled && volumetricFog.density > 0f && (volumetricFog._visibilityScope == FOG_VISIBILITY_SCOPE.Global || volumetricFog._visibilityVolume.Contains(position)))
			{
				fogRenderInstances.Add(fogInstances[i]);
			}
		}
		_renderingInstancesCount = fogRenderInstances.Count;
		if (_renderingInstancesCount == 0 || mainCamera == null)
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (_hasCamera && _density <= 0f && shouldUpdateMaterialProperties)
		{
			UpdateMaterialPropertiesNow(Camera.current);
		}
		if (_renderingInstancesCount == 1)
		{
			fogRenderInstances[0].DoOnRenderImageInstance(source, destination);
			return;
		}
		RenderTextureDescriptor descriptor = source.descriptor;
		descriptor.depthBufferBits = 0;
		descriptor.msaaSamples = 1;
		RenderTexture temporary = RenderTexture.GetTemporary(descriptor);
		fogRenderInstances[0].DoOnRenderImageInstance(source, temporary);
		if (_renderingInstancesCount == 2)
		{
			fogRenderInstances[1].DoOnRenderImageInstance(temporary, destination);
		}
		if (_renderingInstancesCount >= 3)
		{
			RenderTexture temporary2 = RenderTexture.GetTemporary(descriptor);
			RenderTexture source2 = temporary;
			RenderTexture renderTexture = temporary2;
			int num = _renderingInstancesCount - 1;
			for (int j = 1; j < num; j++)
			{
				if (j > 1)
				{
					renderTexture.DiscardContents();
				}
				fogRenderInstances[j].DoOnRenderImageInstance(source2, renderTexture);
				if (renderTexture == temporary2)
				{
					source2 = temporary2;
					renderTexture = temporary;
				}
				else
				{
					source2 = temporary;
					renderTexture = temporary2;
				}
			}
			fogRenderInstances[num].DoOnRenderImageInstance(source2, destination);
			RenderTexture.ReleaseTemporary(temporary2);
		}
		RenderTexture.ReleaseTemporary(temporary);
	}

	internal void DoOnRenderImageInstance(RenderTexture source, RenderTexture destination)
	{
		Camera current = Camera.current;
		if (current == null || fogMat == null)
		{
			Graphics.Blit(source, destination);
			return;
		}
		isRendering = true;
		if (!_hasCamera)
		{
			CheckFogAreaDimensions();
			if (_sunShadows && !fogRenderer.sunShadows)
			{
				fogRenderer.sunShadows = true;
			}
		}
		if (shouldUpdateMaterialProperties)
		{
			UpdateMaterialPropertiesNow(current);
		}
		if (lastFrameCount != Time.frameCount && Application.isPlaying)
		{
			if (_useRealTime)
			{
				deltaTime = Time.time - timeOfLastRender;
				timeOfLastRender = Time.time;
			}
			else
			{
				deltaTime = Time.deltaTime;
			}
			UpdateWindSpeedQuick();
		}
		if (_hasCamera)
		{
			if (_spsrBehaviour == SPSR_BEHAVIOUR.ForcedOn && !_useSinglePassStereoRenderingMatrix)
			{
				useSinglePassStereoRenderingMatrix = true;
			}
			else if (_spsrBehaviour == SPSR_BEHAVIOUR.ForcedOff && _useSinglePassStereoRenderingMatrix)
			{
				useSinglePassStereoRenderingMatrix = false;
			}
		}
		fogMat.SetTexture("_DepthTextureForFog", _depthTexture);
		bool flag = false;
		Vector3 position = current.transform.position;
		bool flag2 = fogRenderer.reduceFlickerBigWorlds;
		if (flag2)
		{
			fogMat.SetVector("_FlickerFreeCamPos", position);
			current.transform.position = Vector3.zero;
			if (flag)
			{
				current.ResetWorldToCameraMatrix();
			}
		}
		else
		{
			fogMat.SetVector("_FlickerFreeCamPos", Vector3.zero);
		}
		if (current.orthographic)
		{
			fogMat.SetVector("_ClipDir", current.transform.forward);
		}
		if (flag && fogRenderer.useSinglePassStereoRenderingMatrix)
		{
			fogMat.SetMatrix("_ClipToWorld", current.cameraToWorldMatrix);
		}
		else
		{
			fogMat.SetMatrix("_ClipToWorld", current.cameraToWorldMatrix * current.projectionMatrix.inverse);
		}
		if (flag2)
		{
			current.transform.position = position;
		}
		if (_lightScatteringEnabled && (bool)fogRenderer.sun)
		{
			UpdateScatteringData(current);
		}
		if (lastFrameCount != Time.frameCount || !Application.isPlaying)
		{
			if (pointLightParams.Length != 6)
			{
				CheckPointLightData();
			}
			for (int i = 0; i < pointLightParams.Length; i++)
			{
				Light light = pointLightParams[i].light;
				if (!(light != null))
				{
					continue;
				}
				if (pointLightParams[i].color != light.color)
				{
					pointLightParams[i].color = light.color;
					isDirty = true;
				}
				if (pointLightParams[i].range != light.range)
				{
					pointLightParams[i].range = light.range;
					isDirty = true;
				}
				if (pointLightParams[i].position != light.transform.position)
				{
					pointLightParams[i].position = light.transform.position;
					isDirty = true;
				}
				if (pointLightParams[i].intensity != light.intensity)
				{
					pointLightParams[i].intensity = light.intensity;
					isDirty = true;
				}
				if (pointLightParams[i].lightParams == null)
				{
					pointLightParams[i].lightParams = pointLightParams[i].light.GetComponent<VolumetricFogLightParams>();
					if (pointLightParams[i].lightParams == null)
					{
						pointLightParams[i].lightParams = pointLightParams[i].light.gameObject.AddComponent<VolumetricFogLightParams>();
					}
				}
				pointLightParams[i].rangeMultiplier = pointLightParams[i].lightParams.rangeMultiplier;
				pointLightParams[i].intensityMultiplier = pointLightParams[i].lightParams.intensityMultiplier;
			}
			SetPointLightMaterialProperties(current);
		}
		RenderTexture renderTexture = null;
		if ((float)_downsampling > 1f || _forceComposition)
		{
			int scaledSize = GetScaledSize(source.width, _downsampling);
			int scaledSize2 = GetScaledSize(source.width, _downsampling);
			RenderTextureDescriptor descriptor = source.descriptor;
			descriptor.width = scaledSize;
			descriptor.height = scaledSize2;
			descriptor.msaaSamples = 1;
			reducedDestination = RenderTexture.GetTemporary(descriptor);
			RenderTextureDescriptor descriptor2 = source.descriptor;
			descriptor2.width = scaledSize;
			descriptor2.height = scaledSize2;
			descriptor2.msaaSamples = 1;
			RenderTextureFormat colorFormat = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat) ? RenderTextureFormat.RFloat : RenderTextureFormat.ARGBFloat);
			descriptor.colorFormat = colorFormat;
			RenderTexture temporary = RenderTexture.GetTemporary(descriptor);
			if (_fogBlur)
			{
				SetBlurTexture(source, descriptor);
			}
			if (!_edgeImprove || flag || SystemInfo.supportedRenderTargetCount < 2)
			{
				Graphics.Blit(source, reducedDestination, fogMat, 3);
				if (_edgeImprove)
				{
					Graphics.Blit(source, temporary, fogMat, 4);
					fogMat.SetTexture("_DownsampledDepth", temporary);
				}
				else
				{
					fogMat.SetTexture("_DownsampledDepth", null);
				}
			}
			else
			{
				fogMat.SetTexture("_MainTex", source);
				if (mrt == null)
				{
					mrt = new RenderBuffer[2];
				}
				mrt[0] = reducedDestination.colorBuffer;
				mrt[1] = temporary.colorBuffer;
				Graphics.SetRenderTarget(mrt, reducedDestination.depthBuffer);
				Graphics.Blit(null, fogMat, 1);
				fogMat.SetTexture("_DownsampledDepth", temporary);
			}
			fogMat.SetTexture("_FogDownsampled", reducedDestination);
			Graphics.Blit(source, destination, fogMat, 2);
			RenderTexture.ReleaseTemporary(temporary);
			RenderTexture.ReleaseTemporary(reducedDestination);
		}
		else
		{
			if (_fogBlur)
			{
				RenderTextureDescriptor descriptor3 = source.descriptor;
				descriptor3.width = 256;
				descriptor3.height = 256;
				SetBlurTexture(source, descriptor3);
			}
			Graphics.Blit(source, destination, fogMat, 0);
		}
		if (flag2 && flag)
		{
			current.ResetWorldToCameraMatrix();
		}
		if (renderTexture != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		lastFrameCount = Time.frameCount;
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

	private void CleanUpDepthTexture()
	{
		if ((bool)depthTexture)
		{
			RenderTexture.ReleaseTemporary(depthTexture);
			depthTexture = null;
		}
	}

	private void GetTransparentDepth()
	{
		CleanUpDepthTexture();
		if (depthCam == null)
		{
			if (depthCamObj == null)
			{
				depthCamObj = GameObject.Find("VFMDepthCamera");
			}
			if (depthCamObj == null)
			{
				depthCamObj = new GameObject("VFMDepthCamera");
				depthCam = depthCamObj.AddComponent<Camera>();
				depthCam.enabled = false;
				depthCamObj.hideFlags = HideFlags.HideAndDontSave;
			}
			else
			{
				depthCam = depthCamObj.GetComponent<Camera>();
				if (depthCam == null)
				{
					UnityEngine.Object.DestroyImmediate(depthCamObj);
					depthCamObj = null;
					return;
				}
			}
		}
		Camera other = mainCamera;
		depthCam.CopyFrom(other);
		depthCam.depthTextureMode = DepthTextureMode.None;
		depthTexture = RenderTexture.GetTemporary(mainCamera.pixelWidth, mainCamera.pixelHeight, 24, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
		depthCam.backgroundColor = new Color(0f, 0f, 0f, 0f);
		depthCam.clearFlags = CameraClearFlags.Color;
		depthCam.cullingMask = _transparencyLayerMask;
		depthCam.targetTexture = depthTexture;
		depthCam.renderingPath = RenderingPath.Forward;
		if (depthShader == null)
		{
			depthShader = Shader.Find("VolumetricFogAndMist/CopyDepth");
		}
		if (depthShaderAndTrans == null)
		{
			depthShaderAndTrans = Shader.Find("VolumetricFogAndMist/CopyDepthAndTrans");
		}
		switch (_computeDepthScope)
		{
		case COMPUTE_DEPTH_SCOPE.OnlyTreeBillboards:
			depthCam.RenderWithShader(depthShader, "RenderType");
			break;
		case COMPUTE_DEPTH_SCOPE.TreeBillboardsAndTransparentObjects:
			depthCam.RenderWithShader(depthShaderAndTrans, "RenderType");
			break;
		default:
			depthCam.RenderWithShader(depthShaderAndTrans, null);
			break;
		}
		Shader.SetGlobalTexture("_VolumetricFogDepthTexture", depthTexture);
	}

	private void CastSunShadows()
	{
		if (base.enabled && base.gameObject.activeSelf && !(fogMat == null))
		{
			if (_sunShadowsBakeMode == SUN_SHADOWS_BAKE_MODE.Discrete && _sunShadowsRefreshInterval > 0f && Time.time > lastShadowUpdateFrame + _sunShadowsRefreshInterval)
			{
				needUpdateDepthSunTexture = true;
			}
			if (!Application.isPlaying || needUpdateDepthSunTexture || depthSunTexture == null || !depthSunTexture.IsCreated())
			{
				needUpdateDepthSunTexture = false;
				lastShadowUpdateFrame = Time.time;
				GetSunShadows();
			}
		}
	}

	private void GetSunShadows()
	{
		if (_sun == null || !_sunShadows)
		{
			return;
		}
		if (depthSunCam == null)
		{
			if (depthSunCamObj == null)
			{
				depthSunCamObj = GameObject.Find("VFMDepthSunCamera");
			}
			if (depthSunCamObj == null)
			{
				depthSunCamObj = new GameObject("VFMDepthSunCamera");
				depthSunCamObj.hideFlags = HideFlags.HideAndDontSave;
				depthSunCam = depthSunCamObj.AddComponent<Camera>();
			}
			else
			{
				depthSunCam = depthSunCamObj.GetComponent<Camera>();
				if (depthSunCam == null)
				{
					UnityEngine.Object.DestroyImmediate(depthSunCamObj);
					depthSunCamObj = null;
					return;
				}
			}
			if (depthSunShader == null)
			{
				depthSunShader = Shader.Find("VolumetricFogAndMist/CopySunDepth");
			}
			depthSunCam.SetReplacementShader(depthSunShader, "RenderType");
			depthSunCam.nearClipPlane = 1f;
			depthSunCam.renderingPath = RenderingPath.Forward;
			depthSunCam.orthographic = true;
			depthSunCam.aspect = 1f;
			depthSunCam.backgroundColor = new Color(0f, 0f, 0.5f, 0f);
			depthSunCam.clearFlags = CameraClearFlags.Color;
			depthSunCam.depthTextureMode = DepthTextureMode.None;
		}
		float orthographicSize = _sunShadowsMaxDistance / 0.95f;
		depthSunCam.transform.position = mainCamera.transform.position - _sun.transform.forward * 2000f;
		depthSunCam.transform.rotation = _sun.transform.rotation;
		depthSunCam.farClipPlane = 4000f;
		depthSunCam.orthographicSize = orthographicSize;
		if (sunLight != null)
		{
			depthSunCam.cullingMask = _sunShadowsLayerMask;
		}
		if (depthSunTexture == null || currentDepthSunTextureRes != _sunShadowsResolution)
		{
			currentDepthSunTextureRes = _sunShadowsResolution;
			int width = (int)Mathf.Pow(2f, _sunShadowsResolution + 9);
			depthSunTexture = new RenderTexture(width, width, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			depthSunTexture.hideFlags = HideFlags.DontSave;
			depthSunTexture.filterMode = FilterMode.Point;
			depthSunTexture.wrapMode = TextureWrapMode.Clamp;
			depthSunTexture.Create();
		}
		depthSunCam.targetTexture = depthSunTexture;
		Shader.SetGlobalFloat("_VF_ShadowBias", _sunShadowsBias);
		if (Application.isPlaying && _sunShadowsBakeMode == SUN_SHADOWS_BAKE_MODE.Realtime)
		{
			if (!depthSunCam.enabled)
			{
				depthSunCam.enabled = true;
			}
		}
		else
		{
			if (depthSunCam.enabled)
			{
				depthSunCam.enabled = false;
			}
			depthSunCam.Render();
		}
		Shader.SetGlobalMatrix("_VolumetricFogSunProj", depthSunCam.projectionMatrix * depthSunCam.worldToCameraMatrix);
		Shader.SetGlobalTexture("_VolumetricFogSunDepthTexture", depthSunTexture);
		Vector4 value = depthSunCam.transform.position;
		value.w = Mathf.Min(_sunShadowsMaxDistance, _maxFogLength);
		Shader.SetGlobalVector("_VolumetricFogSunWorldPos", value);
		UpdateSunShadowsData();
	}

	private void SetBlurTexture(RenderTexture source, RenderTextureDescriptor desc)
	{
		if (blurMat == null)
		{
			Shader shader = Shader.Find("VolumetricFogAndMist/Blur");
			blurMat = new Material(shader);
			blurMat.hideFlags = HideFlags.DontSave;
		}
		if (!(blurMat == null))
		{
			blurMat.SetFloat("_BlurDepth", _fogBlurDepth);
			RenderTexture temporary = RenderTexture.GetTemporary(desc);
			Graphics.Blit(source, temporary, blurMat, 0);
			RenderTexture temporary2 = RenderTexture.GetTemporary(desc);
			Graphics.Blit(temporary, temporary2, blurMat, 1);
			blurMat.SetFloat("_BlurDepth", _fogBlurDepth * 2f);
			temporary.DiscardContents();
			Graphics.Blit(temporary2, temporary, blurMat, 0);
			temporary2.DiscardContents();
			Graphics.Blit(temporary, temporary2, blurMat, 1);
			fogMat.SetTexture("_BlurTex", temporary2);
			RenderTexture.ReleaseTemporary(temporary2);
			RenderTexture.ReleaseTemporary(temporary);
		}
	}

	private void DestroySunShadowsDependencies()
	{
		if (depthSunCamObj != null)
		{
			UnityEngine.Object.DestroyImmediate(depthSunCamObj);
			depthSunCamObj = null;
		}
		CleanUpTextureDepthSun();
	}

	private void CleanUpTextureDepthSun()
	{
		if (depthSunTexture != null)
		{
			depthSunTexture.Release();
			depthSunTexture = null;
		}
	}

	public string GetCurrentPresetName()
	{
		return Enum.GetName(typeof(FOG_PRESET), _preset);
	}

	public void UpdatePreset()
	{
		switch (_preset)
		{
		case FOG_PRESET.Clear:
			_density = 0f;
			_fogOfWarEnabled = false;
			_fogVoidRadius = 0f;
			break;
		case FOG_PRESET.Mist:
			_skySpeed = 0.3f;
			_skyHaze = 15f;
			_skyNoiseStrength = 0.1f;
			_skyAlpha = 0.8f;
			_density = 0.3f;
			_noiseStrength = 0.6f;
			_noiseScale = 1f;
			_skyNoiseScale = 1f;
			_noiseSparse = 0f;
			_distance = 0f;
			_distanceFallOff = 0f;
			_maxFogLengthFallOff = 0.5f;
			_height = 6f;
			_heightFallOff = 0.6f;
			_deepObscurance = 1f;
			_stepping = 8f;
			_steppingNear = 0f;
			_alpha = 1f;
			_color = new Color(0.89f, 0.89f, 0.89f);
			_skyColor = new Color(0.81f, 0.81f, 0.81f);
			_specularColor = new Color(1f, 1f, 0.8f, 1f);
			_specularIntensity = 0.1f;
			_specularThreshold = 0.6f;
			_lightColor = Color.white;
			_lightIntensity = 0.12f;
			_speed = 0.01f;
			_downsampling = 1;
			_baselineRelativeToCamera = false;
			CheckWaterLevel(baseZero: false);
			_fogVoidRadius = 0f;
			CopyTransitionValues();
			break;
		case FOG_PRESET.WindyMist:
			_skySpeed = 0.3f;
			_skyHaze = 25f;
			_skyNoiseStrength = 0.1f;
			_skyAlpha = 0.85f;
			_density = 0.3f;
			_noiseStrength = 0.5f;
			_noiseScale = 1.15f;
			_skyNoiseScale = 1.15f;
			_noiseSparse = 0f;
			_distance = 0f;
			_distanceFallOff = 0f;
			_maxFogLengthFallOff = 0.5f;
			_height = 6.5f;
			_heightFallOff = 0.6f;
			_deepObscurance = 1f;
			_stepping = 10f;
			_steppingNear = 0f;
			_alpha = 1f;
			_color = new Color(0.89f, 0.89f, 0.89f, 1f);
			_skyColor = new Color(0.81f, 0.81f, 0.81f);
			_specularColor = new Color(1f, 1f, 0.8f, 1f);
			_specularIntensity = 0.1f;
			_specularThreshold = 0.6f;
			_lightColor = Color.white;
			_lightIntensity = 0f;
			_speed = 0.15f;
			_downsampling = 1;
			_baselineRelativeToCamera = false;
			CheckWaterLevel(baseZero: false);
			_fogVoidRadius = 0f;
			CopyTransitionValues();
			break;
		case FOG_PRESET.GroundFog:
			_skySpeed = 0.3f;
			_skyHaze = 0f;
			_skyNoiseStrength = 0.1f;
			_skyAlpha = 0.85f;
			_density = 0.6f;
			_noiseStrength = 0.479f;
			_noiseScale = 1.15f;
			_skyNoiseScale = 1.15f;
			_noiseSparse = 0f;
			_distance = 5f;
			_distanceFallOff = 1f;
			_maxFogLengthFallOff = 0.6f;
			_height = 1.5f;
			_heightFallOff = 0.6f;
			_deepObscurance = 1f;
			_stepping = 8f;
			_steppingNear = 0f;
			_alpha = 0.95f;
			_color = new Color(0.89f, 0.89f, 0.89f, 1f);
			_skyColor = new Color(0.79f, 0.79f, 0.79f, 1f);
			_specularColor = new Color(1f, 1f, 0.8f, 1f);
			_specularIntensity = 0.2f;
			_specularThreshold = 0.6f;
			_lightColor = Color.white;
			_lightIntensity = 0.2f;
			_speed = 0.01f;
			_downsampling = 1;
			_baselineRelativeToCamera = false;
			CheckWaterLevel(baseZero: false);
			_fogVoidRadius = 0f;
			CopyTransitionValues();
			break;
		case FOG_PRESET.FrostedGround:
			_skySpeed = 0f;
			_skyHaze = 0f;
			_skyNoiseStrength = 0.729f;
			_skyAlpha = 0.55f;
			_density = 1f;
			_noiseStrength = 0.164f;
			_noiseScale = 1.81f;
			_skyNoiseScale = 1.81f;
			_noiseSparse = 0f;
			_distance = 0f;
			_distanceFallOff = 0f;
			_height = 0.5f;
			_heightFallOff = 0.6f;
			_deepObscurance = 1f;
			_stepping = 20f;
			_steppingNear = 50f;
			_alpha = 0.97f;
			_color = new Color(0.546f, 0.648f, 0.71f, 1f);
			_skyColor = _color;
			_specularColor = new Color(0.792f, 0.792f, 0.792f, 1f);
			_specularIntensity = 1f;
			_specularThreshold = 0.866f;
			_lightColor = new Color(0.972f, 0.972f, 0.972f, 1f);
			_lightIntensity = 0.743f;
			_speed = 0f;
			_downsampling = 1;
			_baselineRelativeToCamera = false;
			CheckWaterLevel(baseZero: false);
			_fogVoidRadius = 0f;
			CopyTransitionValues();
			break;
		case FOG_PRESET.FoggyLake:
			_skySpeed = 0.3f;
			_skyHaze = 40f;
			_skyNoiseStrength = 0.574f;
			_skyAlpha = 0.827f;
			_density = 1f;
			_noiseStrength = 0.03f;
			_noiseScale = 5.77f;
			_skyNoiseScale = 5.77f;
			_noiseSparse = 0f;
			_distance = 0f;
			_distanceFallOff = 0f;
			_maxFogLengthFallOff = 0.6f;
			_height = 4f;
			_heightFallOff = 0.6f;
			_deepObscurance = 1f;
			_stepping = 6f;
			_steppingNear = 14.4f;
			_alpha = 1f;
			_color = new Color(0f, 0.96f, 1f, 1f);
			_skyColor = _color;
			_specularColor = Color.white;
			_lightColor = Color.white;
			_specularIntensity = 0.861f;
			_specularThreshold = 0.907f;
			_lightIntensity = 0.126f;
			_speed = 0f;
			_downsampling = 1;
			_baselineRelativeToCamera = false;
			CheckWaterLevel(baseZero: false);
			_fogVoidRadius = 0f;
			CopyTransitionValues();
			break;
		case FOG_PRESET.LowClouds:
			_skySpeed = 0.3f;
			_skyHaze = 60f;
			_skyNoiseStrength = 0.97f;
			_skyAlpha = 0.96f;
			_density = 1f;
			_noiseStrength = 0.7f;
			_noiseScale = 1f;
			_skyNoiseScale = 1f;
			_noiseSparse = 0f;
			_distance = 0f;
			_distanceFallOff = 0f;
			_maxFogLengthFallOff = 0.6f;
			_height = 4f;
			_heightFallOff = 0.6f;
			_deepObscurance = 1f;
			_stepping = 12f;
			_steppingNear = 0f;
			_alpha = 0.98f;
			_color = new Color(0.89f, 0.89f, 0.89f, 1f);
			_skyColor = new Color(0.79f, 0.79f, 0.79f, 1f);
			_specularColor = new Color(1f, 1f, 0.8f, 1f);
			_specularIntensity = 0.15f;
			_specularThreshold = 0.6f;
			_lightColor = Color.white;
			_lightIntensity = 0.15f;
			_speed = 0.008f;
			_downsampling = 1;
			_baselineRelativeToCamera = false;
			CheckWaterLevel(baseZero: false);
			_fogVoidRadius = 0f;
			CopyTransitionValues();
			break;
		case FOG_PRESET.SeaClouds:
			_skySpeed = 0.3f;
			_skyHaze = 60f;
			_skyNoiseStrength = 0.97f;
			_skyAlpha = 0.96f;
			_density = 1f;
			_noiseStrength = 1f;
			_noiseScale = 1.5f;
			_skyNoiseScale = 1.5f;
			_noiseSparse = 0f;
			_distance = 0f;
			_distanceFallOff = 0f;
			_maxFogLengthFallOff = 0.7f;
			_deepObscurance = 1f;
			_height = 12.4f;
			_heightFallOff = 0.6f;
			_stepping = 6f;
			_alpha = 0.98f;
			_color = new Color(0.89f, 0.89f, 0.89f, 1f);
			_skyColor = new Color(0.83f, 0.83f, 0.83f, 1f);
			_specularColor = new Color(1f, 1f, 0.8f, 1f);
			_specularIntensity = 0.259f;
			_specularThreshold = 0.6f;
			_lightColor = Color.white;
			_lightIntensity = 0.15f;
			_speed = 0.008f;
			_downsampling = 1;
			_baselineRelativeToCamera = false;
			CheckWaterLevel(baseZero: false);
			_fogVoidRadius = 0f;
			CopyTransitionValues();
			break;
		case FOG_PRESET.Fog:
			_skySpeed = 0.3f;
			_skyHaze = 144f;
			_skyNoiseStrength = 0.7f;
			_skyAlpha = 0.9f;
			_density = 0.35f;
			_noiseStrength = 0.3f;
			_noiseScale = 1f;
			_skyNoiseScale = 1f;
			_noiseSparse = 0f;
			_distance = 20f;
			_distanceFallOff = 0.7f;
			_maxFogLengthFallOff = 0.5f;
			_height = 8f;
			_heightFallOff = 0.6f;
			_deepObscurance = 1f;
			_stepping = 8f;
			_steppingNear = 0f;
			_alpha = 0.97f;
			_color = new Color(0.89f, 0.89f, 0.89f, 1f);
			_skyColor = new Color(0.79f, 0.79f, 0.79f, 1f);
			_specularColor = new Color(1f, 1f, 0.8f, 1f);
			_specularIntensity = 0f;
			_specularThreshold = 0.6f;
			_lightColor = Color.white;
			_lightIntensity = 0f;
			_speed = 0.05f;
			_downsampling = 1;
			_baselineRelativeToCamera = false;
			CheckWaterLevel(baseZero: false);
			_fogVoidRadius = 0f;
			CopyTransitionValues();
			break;
		case FOG_PRESET.HeavyFog:
			_skySpeed = 0.05f;
			_skyHaze = 500f;
			_skyNoiseStrength = 0.826f;
			_skyAlpha = 1f;
			_density = 0.35f;
			_noiseStrength = 0.1f;
			_noiseScale = 1f;
			_skyNoiseScale = 1f;
			_noiseSparse = 0f;
			_distance = 20f;
			_distanceFallOff = 0.8f;
			_deepObscurance = 1f;
			_height = 18f;
			_heightFallOff = 0.6f;
			_stepping = 6f;
			_steppingNear = 0f;
			_alpha = 1f;
			_color = new Color(0.91f, 0.91f, 0.91f, 1f);
			_skyColor = new Color(0.79f, 0.79f, 0.79f, 1f);
			_specularColor = new Color(1f, 1f, 0.8f, 1f);
			_specularIntensity = 0f;
			_specularThreshold = 0.6f;
			_lightColor = Color.white;
			_lightIntensity = 0f;
			_speed = 0.015f;
			_downsampling = 1;
			_baselineRelativeToCamera = false;
			CheckWaterLevel(baseZero: false);
			_fogVoidRadius = 0f;
			CopyTransitionValues();
			break;
		case FOG_PRESET.SandStorm1:
			_skySpeed = 0.35f;
			_skyHaze = 388f;
			_skyNoiseStrength = 0.847f;
			_skyAlpha = 1f;
			_density = 0.487f;
			_noiseStrength = 0.758f;
			_noiseScale = 1.71f;
			_skyNoiseScale = 1.71f;
			_noiseSparse = 0f;
			_distance = 0f;
			_distanceFallOff = 0f;
			_height = 16f;
			_heightFallOff = 0.6f;
			_deepObscurance = 1f;
			_stepping = 6f;
			_steppingNear = 0f;
			_alpha = 1f;
			_color = new Color(0.505f, 0.505f, 0.505f, 1f);
			_skyColor = _color;
			_specularColor = new Color(1f, 1f, 0.8f, 1f);
			_specularIntensity = 0f;
			_specularThreshold = 0.6f;
			_lightColor = Color.white;
			_lightIntensity = 0f;
			_speed = 0.3f;
			_windDirection = Vector3.right;
			_downsampling = 1;
			_baselineRelativeToCamera = false;
			CheckWaterLevel(baseZero: false);
			_fogVoidRadius = 0f;
			CopyTransitionValues();
			break;
		case FOG_PRESET.Smoke:
			_skySpeed = 0.109f;
			_skyHaze = 10f;
			_skyNoiseStrength = 0.119f;
			_skyAlpha = 1f;
			_density = 1f;
			_noiseStrength = 0.767f;
			_noiseScale = 1.6f;
			_skyNoiseScale = 1.6f;
			_noiseSparse = 0f;
			_distance = 0f;
			_distanceFallOff = 0f;
			_maxFogLengthFallOff = 0.7f;
			_height = 8f;
			_heightFallOff = 0.6f;
			_deepObscurance = 1f;
			_stepping = 12f;
			_steppingNear = 25f;
			_alpha = 1f;
			_color = new Color(0.125f, 0.125f, 0.125f, 1f);
			_skyColor = _color;
			_specularColor = new Color(1f, 1f, 1f, 1f);
			_specularIntensity = 0.575f;
			_specularThreshold = 0.6f;
			_lightColor = Color.white;
			_lightIntensity = 1f;
			_speed = 0.075f;
			_windDirection = Vector3.right;
			_downsampling = 1;
			_baselineRelativeToCamera = false;
			CheckWaterLevel(baseZero: false);
			_baselineHeight += 8f;
			_fogVoidRadius = 0f;
			CopyTransitionValues();
			break;
		case FOG_PRESET.ToxicSwamp:
			_skySpeed = 0.062f;
			_skyHaze = 22f;
			_skyNoiseStrength = 0.694f;
			_skyAlpha = 1f;
			_density = 1f;
			_noiseStrength = 1f;
			_noiseScale = 1f;
			_skyNoiseScale = 1f;
			_noiseSparse = 0f;
			_distance = 0f;
			_distanceFallOff = 0f;
			_height = 2.5f;
			_heightFallOff = 0.6f;
			_deepObscurance = 1f;
			_stepping = 20f;
			_steppingNear = 50f;
			_alpha = 0.95f;
			_color = new Color(0.0238f, 0.175f, 0.109f, 1f);
			_skyColor = _color;
			_specularColor = new Color(0.593f, 0.625f, 0.207f, 1f);
			_specularIntensity = 0.735f;
			_specularThreshold = 0.6f;
			_lightColor = new Color(0.73f, 0.746f, 0.511f, 1f);
			_lightIntensity = 0.492f;
			_speed = 0.0003f;
			_windDirection = Vector3.right;
			_downsampling = 1;
			_baselineRelativeToCamera = false;
			CheckWaterLevel(baseZero: false);
			_fogVoidRadius = 0f;
			CopyTransitionValues();
			break;
		case FOG_PRESET.SandStorm2:
			_skySpeed = 0f;
			_skyHaze = 0f;
			_skyNoiseStrength = 0.729f;
			_skyAlpha = 0.55f;
			_density = 0.545f;
			_noiseStrength = 1f;
			_noiseScale = 3f;
			_skyNoiseScale = 3f;
			_noiseSparse = 0f;
			_distance = 0f;
			_distanceFallOff = 0f;
			_height = 12f;
			_heightFallOff = 0.6f;
			_deepObscurance = 1f;
			_stepping = 5f;
			_steppingNear = 19.6f;
			_alpha = 0.96f;
			_color = new Color(0.609f, 0.609f, 0.609f, 1f);
			_skyColor = _color;
			_specularColor = new Color(0.589f, 0.621f, 0.207f, 1f);
			_specularIntensity = 0.505f;
			_specularThreshold = 0.6f;
			_lightColor = new Color(0.726f, 0.742f, 0.507f, 1f);
			_lightIntensity = 0.581f;
			_speed = 0.168f;
			_windDirection = Vector3.right;
			_downsampling = 1;
			_baselineRelativeToCamera = false;
			CheckWaterLevel(baseZero: false);
			_fogVoidRadius = 0f;
			CopyTransitionValues();
			break;
		case FOG_PRESET.WorldEdge:
		{
			_skySpeed = 0.3f;
			_skyHaze = 60f;
			_skyNoiseStrength = 0.97f;
			_skyAlpha = 0.96f;
			_density = 1f;
			_noiseStrength = 1f;
			_noiseScale = 3f;
			_skyNoiseScale = 3f;
			_noiseSparse = 0f;
			_distance = 0f;
			_distanceFallOff = 0f;
			_maxFogLengthFallOff = 1f;
			_height = 20f;
			_heightFallOff = 0.6f;
			_deepObscurance = 1f;
			_stepping = 6f;
			_alpha = 0.98f;
			_color = new Color(0.89f, 0.89f, 0.89f, 1f);
			_skyColor = _color;
			_specularColor = new Color(1f, 1f, 0.8f, 1f);
			_specularIntensity = 0.259f;
			_specularThreshold = 0.6f;
			_lightColor = Color.white;
			_lightIntensity = 0.15f;
			_speed = 0.03f;
			_downsampling = 2;
			_baselineRelativeToCamera = false;
			CheckWaterLevel(baseZero: false);
			Terrain activeTerrain = GetActiveTerrain();
			if (activeTerrain != null)
			{
				_fogVoidPosition = activeTerrain.transform.position + activeTerrain.terrainData.size * 0.5f;
				_fogVoidRadius = activeTerrain.terrainData.size.x * 0.45f;
				_fogVoidHeight = activeTerrain.terrainData.size.y;
				_fogVoidDepth = activeTerrain.terrainData.size.z * 0.45f;
				_fogVoidFallOff = 6f;
				_fogAreaRadius = 0f;
				_character = null;
				_fogAreaCenter = null;
				float x = activeTerrain.terrainData.size.x;
				if (mainCamera.farClipPlane < x)
				{
					mainCamera.farClipPlane = x;
				}
				if (_maxFogLength < x * 0.6f)
				{
					_maxFogLength = x * 0.6f;
				}
			}
			CopyTransitionValues();
			break;
		}
		}
		currentFogAlpha = _alpha;
		currentFogColor = _color;
		currentFogSpecularColor = _specularColor;
		currentLightColor = _lightColor;
		currentSkyHazeAlpha = _skyAlpha;
		UpdateSun();
		FogOfWarUpdateTexture();
		UpdateMaterialProperties(forceNow: true);
		UpdateRenderComponents();
		UpdateTextureAlpha();
		UpdateTexture();
		if (_sunShadows)
		{
			needUpdateDepthSunTexture = true;
		}
		else
		{
			DestroySunShadowsDependencies();
		}
		if (!Application.isPlaying)
		{
			UpdateWindSpeedQuick();
		}
		TrackPointLights();
		lastTimeSortInstances = 0f;
	}

	public void CheckWaterLevel(bool baseZero)
	{
		if (mainCamera == null)
		{
			return;
		}
		if (_baselineHeight > mainCamera.transform.position.y || baseZero)
		{
			_baselineHeight = 0f;
		}
		GameObject gameObject = GameObject.Find("Water");
		if (gameObject == null)
		{
			GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null && array[i].layer == 4)
				{
					gameObject = array[i];
					break;
				}
			}
		}
		if (gameObject != null)
		{
			_renderBeforeTransparent = false;
			if (_baselineHeight < gameObject.transform.position.y)
			{
				_baselineHeight = gameObject.transform.position.y;
			}
		}
		UpdateMaterialHeights(mainCamera);
	}

	public static Terrain GetActiveTerrain()
	{
		Terrain activeTerrain = Terrain.activeTerrain;
		if (activeTerrain != null && activeTerrain.isActiveAndEnabled)
		{
			return activeTerrain;
		}
		for (int i = 0; i < Terrain.activeTerrains.Length; i++)
		{
			activeTerrain = Terrain.activeTerrains[i];
			if (activeTerrain != null && activeTerrain.isActiveAndEnabled)
			{
				return activeTerrain;
			}
		}
		return null;
	}

	private void UpdateMaterialFogColor()
	{
		Color value = currentFogColor;
		value.r *= 2f * temporaryProperties.color.r;
		value.g *= 2f * temporaryProperties.color.g;
		value.b *= 2f * temporaryProperties.color.b;
		value.a = 1f - _heightFallOff;
		fogMat.SetColor("_FogColor", value);
	}

	private void UpdateMaterialHeights(Camera mainCamera)
	{
		currentFogAltitude = _baselineHeight;
		Vector3 vector = _fogAreaPosition;
		if (_fogAreaRadius > 0f)
		{
			currentFogAltitude += _fogAreaPosition.y;
			if (_useXYPlane)
			{
				vector.z = 0f;
			}
			else
			{
				vector.y = 0f;
			}
		}
		if (_baselineRelativeToCamera && !_useXYPlane)
		{
			oldBaselineRelativeCameraY += (mainCamera.transform.position.y - oldBaselineRelativeCameraY) * Mathf.Clamp01(1.001f - _baselineRelativeToCameraDelay);
			currentFogAltitude += oldBaselineRelativeCameraY - 1f;
		}
		float w = 0.01f / _noiseScale;
		fogMat.SetVector("_FogData", new Vector4(currentFogAltitude, _height, 1f / (_density * temporaryProperties.density), w));
		fogMat.SetFloat("_FogSkyHaze", _skyHaze + currentFogAltitude);
		Vector3 vector2 = _fogVoidPosition - currentFogAltitude * Vector3.up;
		fogMat.SetVector("_FogVoidPosition", vector2);
		fogMat.SetVector("_FogAreaPosition", vector);
	}

	public void UpdateMaterialProperties(bool forceNow = false)
	{
		if (forceNow || !Application.isPlaying)
		{
			UpdateMaterialPropertiesNow();
		}
		else
		{
			shouldUpdateMaterialProperties = true;
		}
	}

	public void UpdateMaterialPropertiesNow()
	{
		UpdateMaterialPropertiesNow(mainCamera);
	}

	private void UpdateMaterialPropertiesNow(Camera mainCamera)
	{
		if (fogMat == null || fogRenderer == null)
		{
			return;
		}
		shouldUpdateMaterialProperties = false;
		UpdateSkyColor(_skyAlpha);
		fogMat.SetFloat("_DeepObscurance", _deepObscurance);
		Vector4 value = new Vector4(1f / (_stepping + 1f), 1f / (1f + _steppingNear), _edgeThreshold, _dithering ? (_ditherStrength * 0.01f) : 0f);
		fogMat.SetFloat("_Jitter", _jitterStrength);
		if (!_edgeImprove)
		{
			value.z = 0f;
		}
		fogMat.SetVector("_FogStepping", value);
		fogMat.SetFloat("_FogAlpha", currentFogAlpha);
		UpdateMaterialHeights(mainCamera);
		float num = 0.01f / _noiseScale;
		if (_maxFogLength < 0f)
		{
			_maxFogLength = 0f;
		}
		float w = _maxFogLength - _maxFogLength * (1f - _maxFogLengthFallOff) + 1f;
		fogMat.SetVector("_FogDistance", new Vector4(num * num * _distance * _distance, _distanceFallOff * _distanceFallOff + 0.1f, _maxFogLength, w));
		UpdateMaterialFogColor();
		if (shaderKeywords == null)
		{
			shaderKeywords = new List<string>();
		}
		else
		{
			shaderKeywords.Clear();
		}
		if (_distance > 0f)
		{
			shaderKeywords.Add("FOG_DISTANCE_ON");
		}
		if (_fogVoidRadius > 0f && _fogVoidFallOff > 0f)
		{
			Vector4 value2 = new Vector4(1f / (1f + _fogVoidRadius), 1f / (1f + _fogVoidHeight), 1f / (1f + _fogVoidDepth), _fogVoidFallOff);
			if (_fogVoidTopology == FOG_VOID_TOPOLOGY.Box)
			{
				shaderKeywords.Add("FOG_VOID_BOX");
			}
			else
			{
				shaderKeywords.Add("FOG_VOID_SPHERE");
			}
			fogMat.SetVector("_FogVoidData", value2);
		}
		if (_fogAreaRadius > 0f && _fogAreaFallOff > 0f)
		{
			Vector4 value3 = new Vector4(1f / (0.0001f + _fogAreaRadius), 1f / (0.0001f + _fogAreaHeight), 1f / (0.0001f + _fogAreaDepth), _fogAreaFallOff);
			if (_fogAreaTopology == FOG_AREA_TOPOLOGY.Box)
			{
				shaderKeywords.Add("FOG_AREA_BOX");
			}
			else
			{
				shaderKeywords.Add("FOG_AREA_SPHERE");
				value3.y = _fogAreaRadius * _fogAreaRadius;
				value3.x /= num;
				value3.z /= num;
			}
			fogMat.SetVector("_FogAreaData", value3);
		}
		if (_skyHaze < 0f)
		{
			_skyHaze = 0f;
		}
		if (_skyHaze > 0f && _skyAlpha > 0f && !_useXYPlane && hasCamera)
		{
			shaderKeywords.Add("FOG_HAZE_ON");
		}
		if (_fogOfWarEnabled)
		{
			shaderKeywords.Add("FOG_OF_WAR_ON");
			fogMat.SetTexture("_FogOfWar", fogOfWarTexture);
			fogMat.SetVector("_FogOfWarCenter", _fogOfWarCenter);
			fogMat.SetVector("_FogOfWarSize", _fogOfWarSize);
			Vector3 vector = _fogOfWarCenter - 0.5f * _fogOfWarSize;
			if (_useXYPlane)
			{
				fogMat.SetVector("_FogOfWarCenterAdjusted", new Vector3(vector.x / _fogOfWarSize.x, vector.y / (_fogOfWarSize.y + 0.0001f), 1f));
			}
			else
			{
				fogMat.SetVector("_FogOfWarCenterAdjusted", new Vector3(vector.x / _fogOfWarSize.x, 1f, vector.z / (_fogOfWarSize.z + 0.0001f)));
			}
		}
		CheckPointLightData();
		bool flag = false;
		for (int i = 0; i < pointLightParams.Length; i++)
		{
			if (pointLightParams[i].light != null || pointLightParams[i].range * pointLightParams[i].intensity > 0f)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			fogMat.SetFloat("_PointLightInsideAtten", _pointLightInsideAtten);
			shaderKeywords.Add("FOG_POINT_LIGHTS");
		}
		sunShadowsActive = false;
		if ((bool)fogRenderer.sun)
		{
			UpdateScatteringData(mainCamera);
			if (_lightScatteringEnabled && _lightScatteringExposure > 0f)
			{
				shaderKeywords.Add("FOG_SCATTERING_ON");
			}
			if (_sunShadows)
			{
				sunShadowsActive = true;
				shaderKeywords.Add("FOG_SUN_SHADOWS_ON");
				UpdateSunShadowsData();
				SetupDirectionalLightCommandBuffer();
			}
		}
		if (_fogBlur)
		{
			shaderKeywords.Add("FOG_BLUR_ON");
			fogMat.SetFloat("_FogBlurDepth", _fogBlurDepth);
		}
		if (_useXYPlane)
		{
			shaderKeywords.Add("FOG_USE_XY_PLANE");
		}
		if (fogRenderer.computeDepth)
		{
			shaderKeywords.Add("FOG_COMPUTE_DEPTH");
		}
		fogMat.shaderKeywords = shaderKeywords.ToArray();
		if (_computeDepth && _computeDepthScope == COMPUTE_DEPTH_SCOPE.TreeBillboardsAndTransparentObjects)
		{
			Shader.SetGlobalFloat("_VFM_CutOff", _transparencyCutOff);
		}
	}

	public void NotifyChangesToFogInstances()
	{
		if (!hasCamera)
		{
			return;
		}
		int num = ((fogInstances != null) ? fogInstances.Count : 0);
		for (int i = 0; i < num; i++)
		{
			VolumetricFog volumetricFog = fogInstances[i];
			if (volumetricFog != null && volumetricFog != this)
			{
				volumetricFog.UpdateMaterialProperties();
			}
		}
	}

	private void UpdateSunShadowsData()
	{
		if (!(_sun == null) && _sunShadows && !(fogMat == null))
		{
			float num = _sunShadowsStrength * Mathf.Clamp01((0f - _sun.transform.forward.y) * 10f);
			if (num < 0f)
			{
				num = 0f;
			}
			if (num > 0f && !fogMat.IsKeywordEnabled("FOG_SUN_SHADOWS_ON"))
			{
				fogMat.EnableKeyword("FOG_SUN_SHADOWS_ON");
			}
			else if (num <= 0f && fogMat.IsKeywordEnabled("FOG_SUN_SHADOWS_ON"))
			{
				fogMat.DisableKeyword("FOG_SUN_SHADOWS_ON");
			}
			if (_hasCamera)
			{
				Shader.SetGlobalVector("_VolumetricFogSunShadowsData", new Vector4(num, _sunShadowsJitterStrength, _sunShadowsCancellation, 0f));
			}
		}
	}

	private void SetupDirectionalLightCommandBuffer()
	{
	}

	private void RemoveDirectionalLightCommandBuffer()
	{
		if (!(_sun == null))
		{
			ShadowMapCopy component = _sun.GetComponent<ShadowMapCopy>();
			if (component != null)
			{
				UnityEngine.Object.DestroyImmediate(component);
			}
		}
	}

	private void UpdateWindSpeedQuick()
	{
		if (!(fogMat == null) && (!Application.isPlaying || lastFrameAppliedWind != Time.frameCount))
		{
			lastFrameAppliedWind = Time.frameCount;
			windSpeedAcum += deltaTime * _windDirection * _speed;
			fogMat.SetVector("_FogWindDir", new Vector3(windSpeedAcum.x % (float)noiseTextureSize, 0f, windSpeedAcum.z % (float)noiseTextureSize));
			skyHazeSpeedAcum += deltaTime * _skySpeed / 20f;
			fogMat.SetVector("_FogSkyData", new Vector4(_skyHaze, _skyNoiseStrength / (0.0001f + _density * temporaryProperties.density), skyHazeSpeedAcum, _skyDepth));
		}
	}

	private void UpdateScatteringData(Camera mainCamera)
	{
		bool orthographic = mainCamera.orthographic;
		Vector3 position = mainCamera.transform.position + _lightDirection * (orthographic ? 100f : 1000f);
		Vector3 vector = mainCamera.WorldToViewportPoint(position, Camera.MonoOrStereoscopicEye.Mono);
		if (vector.z < 0f)
		{
			if (orthographic)
			{
				vector.y = 1f - vector.y;
				vector.x = 1f - vector.x;
			}
			Vector2 vector2 = new Vector2(vector.x, vector.y);
			float num = Mathf.Clamp01(1f - _lightDirection.y);
			if (vector2 != oldSunPos)
			{
				oldSunPos = vector2;
				sunFade = Mathf.SmoothStep(1f, 0f, (vector2 - Vector2.one * 0.5f).magnitude * 0.5f) * num;
			}
			fogMat.SetVector("_SunPosition", vector2);
			if (_lightScatteringEnabled && !fogMat.IsKeywordEnabled("FOG_SCATTERING_ON"))
			{
				fogMat.EnableKeyword("FOG_SCATTERING_ON");
			}
			float num2 = _lightScatteringExposure * sunFade;
			fogMat.SetVector("_FogScatteringData", new Vector4(_lightScatteringSpread / (float)_lightScatteringSamples, (num2 > 0f) ? _lightScatteringSamples : 0, num2, _lightScatteringWeight / (float)_lightScatteringSamples));
			_ = _lightScatteringEnabled;
			fogMat.SetVector("_FogScatteringData2", new Vector4(_lightScatteringIllumination, _lightScatteringDecay, _lightScatteringJittering, 0f));
			fogMat.SetVector("_SunDir", -_lightDirection);
			fogMat.SetColor("_SunColor", _lightColor);
		}
		else if (fogMat.IsKeywordEnabled("FOG_SCATTERING_ON"))
		{
			fogMat.DisableKeyword("FOG_SCATTERING_ON");
		}
	}

	private void UpdateSun()
	{
		if (fogRenderer != null && fogRenderer.sun != null)
		{
			sunLight = fogRenderer.sun.GetComponent<Light>();
		}
		else
		{
			sunLight = null;
		}
	}

	private void UpdateSkyColor(float alpha)
	{
		if (!(fogMat == null))
		{
			Color value = skyHazeLightColor;
			value.a = alpha;
			fogMat.SetColor("_FogSkyColor", value);
			fogMat.SetFloat("_FogSkyNoiseScale", 0.01f / _skyNoiseScale);
		}
	}

	private void UpdateTextureAlpha()
	{
		if (adjustedColors == null)
		{
			return;
		}
		float num = Mathf.Clamp(_noiseStrength, 0f, 0.95f);
		for (int i = 0; i < adjustedColors.Length; i++)
		{
			float num2 = 1f - (_noiseSparse + noiseColors[i].b) * num;
			num2 *= _density * temporaryProperties.density * _noiseFinalMultiplier;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			else if (num2 > 1f)
			{
				num2 = 1f;
			}
			adjustedColors[i].a = num2;
		}
		hasChangeAdjustedColorsAlpha = true;
	}

	private void UpdateTexture()
	{
		if (!(fogMat == null))
		{
			ComputeLightColor();
			if (Application.isPlaying)
			{
				updatingTextureSlice = 0;
			}
			else
			{
				updatingTextureSlice = -1;
			}
			UpdateTextureColors(adjustedColors, hasChangeAdjustedColorsAlpha);
			needUpdateTexture = false;
			UpdateSkyColor(_skyAlpha);
		}
	}

	private void ComputeLightColor()
	{
		float num = _lightIntensity + sunLightIntensity;
		if (!_useXYPlane)
		{
			num *= Mathf.Clamp01(1f - _lightDirection.y * 2f);
		}
		switch (_lightingModel)
		{
		default:
		{
			lastRenderSettingsAmbientLight = RenderSettings.ambientLight;
			lastRenderSettingsAmbientIntensity = RenderSettings.ambientIntensity;
			Color a = lastRenderSettingsAmbientLight * lastRenderSettingsAmbientIntensity;
			updatingTextureLightColor = Color.Lerp(a, currentLightColor * num, num);
			skyHazeLightColor = Color.Lerp(a, _skyColor * num, num);
			break;
		}
		case LIGHTING_MODEL.Natural:
			lastRenderSettingsAmbientLight = RenderSettings.ambientLight;
			lastRenderSettingsAmbientIntensity = RenderSettings.ambientIntensity;
			updatingTextureLightColor = Color.Lerp(lastRenderSettingsAmbientLight, currentLightColor * num + lastRenderSettingsAmbientLight, _lightIntensity);
			skyHazeLightColor = Color.Lerp(lastRenderSettingsAmbientLight, _skyColor * num + lastRenderSettingsAmbientLight, _lightIntensity);
			break;
		case LIGHTING_MODEL.SingleLight:
			lastRenderSettingsAmbientLight = Color.black;
			lastRenderSettingsAmbientIntensity = RenderSettings.ambientIntensity;
			updatingTextureLightColor = Color.Lerp(lastRenderSettingsAmbientLight, currentLightColor * num, _lightIntensity);
			skyHazeLightColor = Color.Lerp(lastRenderSettingsAmbientLight, _skyColor * num, _lightIntensity);
			break;
		}
	}

	private void UpdateTextureColors(Color[] colors, bool forceUpdateEntireTexture)
	{
		float num = 1.0001f - _specularThreshold;
		int width = adjustedTexture.width;
		Vector3 vector = new Vector3(0f - _lightDirection.x, 0f, 0f - _lightDirection.z).normalized * 0.3f;
		vector.y = ((_lightDirection.y > 0f) ? Mathf.Clamp01(1f - _lightDirection.y) : (1f - Mathf.Clamp01(0f - _lightDirection.y)));
		int num2 = (int)((float)(Mathf.FloorToInt(vector.z * (float)width) * width) + vector.x * (float)width) + colors.Length;
		float num3 = vector.y / num;
		Color obj = currentFogSpecularColor * (1f + _specularIntensity) * _specularIntensity;
		bool flag = false;
		if (updatingTextureSlice >= 1 || forceUpdateEntireTexture)
		{
			flag = true;
		}
		float num4 = updatingTextureLightColor.r * 0.5f;
		float num5 = updatingTextureLightColor.g * 0.5f;
		float num6 = updatingTextureLightColor.b * 0.5f;
		float num7 = obj.r * 0.5f;
		float num8 = obj.g * 0.5f;
		float num9 = obj.b * 0.5f;
		int num10 = colors.Length;
		int num11 = 0;
		int num12 = num10;
		if (updatingTextureSlice >= 0)
		{
			if (updatingTextureSlice > _updateTextureSpread)
			{
				updatingTextureSlice = -1;
				needUpdateTexture = true;
				return;
			}
			num11 = num10 * updatingTextureSlice / _updateTextureSpread;
			num12 = num10 * (updatingTextureSlice + 1) / _updateTextureSpread;
		}
		int num13 = 0;
		for (int i = num11; i < num12; i++)
		{
			int num14 = (i + num2) % num10;
			float num15 = (colors[i].a - colors[num14].a) * num3;
			if (num15 < 0f)
			{
				num15 = 0f;
			}
			else if (num15 > 1f)
			{
				num15 = 1f;
			}
			float num16 = num4 + num7 * num15;
			float num17 = num5 + num8 * num15;
			float num18 = num6 + num9 * num15;
			if (!flag)
			{
				if (num13++ < 100)
				{
					if (num16 != colors[i].r || num17 != colors[i].g || num18 != colors[i].b)
					{
						flag = true;
					}
				}
				else if (!flag)
				{
					break;
				}
			}
			colors[i].r = num16;
			colors[i].g = num17;
			colors[i].b = num18;
		}
		bool flag2 = forceUpdateEntireTexture;
		if (flag)
		{
			if (updatingTextureSlice >= 0)
			{
				updatingTextureSlice++;
				if (updatingTextureSlice >= _updateTextureSpread)
				{
					updatingTextureSlice = -1;
					flag2 = true;
				}
			}
			else
			{
				flag2 = true;
			}
		}
		else
		{
			updatingTextureSlice = -1;
		}
		if (flag2)
		{
			if (Application.isPlaying && _turbulenceStrength > 0f && (bool)adjustedChaosTexture)
			{
				adjustedChaosTexture.SetPixels(adjustedColors);
				adjustedChaosTexture.Apply();
			}
			else
			{
				adjustedTexture.SetPixels(adjustedColors);
				adjustedTexture.Apply();
				fogMat.SetTexture("_NoiseTex", adjustedTexture);
			}
			lastTextureUpdate = Time.time;
		}
	}

	internal void ApplyChaos()
	{
		if ((bool)adjustedTexture && (!Application.isPlaying || lastFrameAppliedChaos != Time.frameCount))
		{
			lastFrameAppliedChaos = Time.frameCount;
			if (chaosLerpMat == null)
			{
				Shader shader = Shader.Find("VolumetricFogAndMist/Chaos Lerp");
				chaosLerpMat = new Material(shader);
				chaosLerpMat.hideFlags = HideFlags.DontSave;
			}
			turbAcum += deltaTime * _turbulenceStrength;
			chaosLerpMat.SetFloat("_Amount", turbAcum);
			if (!adjustedChaosTexture)
			{
				adjustedChaosTexture = UnityEngine.Object.Instantiate(adjustedTexture);
				adjustedChaosTexture.hideFlags = HideFlags.DontSave;
			}
			rtAdjusted = RenderTexture.GetTemporary(adjustedTexture.width, adjustedTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			rtAdjusted.wrapMode = TextureWrapMode.Repeat;
			Graphics.Blit(adjustedChaosTexture, rtAdjusted, chaosLerpMat);
			fogMat.SetTexture("_NoiseTex", rtAdjusted);
		}
	}

	private void CopyTransitionValues()
	{
		currentFogAlpha = _alpha;
		currentSkyHazeAlpha = _skyAlpha;
		currentFogColor = _color;
		currentFogSpecularColor = _specularColor;
		currentLightColor = _lightColor;
	}

	public void SetTargetProfile(VolumetricFogProfile targetProfile, float duration)
	{
		if (_useFogVolumes)
		{
			initialProfile = ScriptableObject.CreateInstance<VolumetricFogProfile>();
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
		if (_useFogVolumes)
		{
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

	public void SetTargetColor(Color newColor, float duration)
	{
		if (useFogVolumes)
		{
			initialFogColor = currentFogColor;
			targetFogColor = newColor;
			transitionDuration = duration;
			transitionStartTime = Time.time;
			transitionColor = true;
			targetColorActive = true;
		}
	}

	public void ClearTargetColor(float duration)
	{
		SetTargetColor(_color, duration);
		targetColorActive = false;
	}

	public void SetTargetSpecularColor(Color newSpecularColor, float duration)
	{
		if (useFogVolumes)
		{
			initialFogSpecularColor = currentFogSpecularColor;
			targetFogSpecularColor = newSpecularColor;
			transitionDuration = duration;
			transitionStartTime = Time.time;
			transitionSpecularColor = true;
			targetSpecularColorActive = true;
		}
	}

	public void ClearTargetSpecularColor(float duration)
	{
		SetTargetSpecularColor(_specularColor, duration);
		targetSpecularColorActive = false;
	}

	public void SetTargetLightColor(Color newLightColor, float duration)
	{
		if (useFogVolumes)
		{
			_sunCopyColor = false;
			initialLightColor = currentLightColor;
			targetLightColor = newLightColor;
			transitionDuration = duration;
			transitionStartTime = Time.time;
			transitionLightColor = true;
			targetLightColorActive = true;
		}
	}

	public void ClearTargetLightColor(float duration)
	{
		SetTargetLightColor(_lightColor, duration);
		targetLightColorActive = false;
	}

	public void CheckPointLightData()
	{
		if (_pointLightTrackingPivot == null)
		{
			_pointLightTrackingPivot = base.transform;
		}
		if (!pointLightDataMigrated)
		{
			pointLightParams = new PointLightParams[6];
			for (int i = 0; i < _pointLightColors.Length; i++)
			{
				pointLightParams[i].color = _pointLightColors[i];
				Light light = null;
				if (_pointLights[i] != null)
				{
					light = _pointLights[i].GetComponent<Light>();
				}
				pointLightParams[i].light = light;
				pointLightParams[i].intensity = _pointLightIntensities[i];
				pointLightParams[i].intensityMultiplier = _pointLightIntensitiesMultiplier[i];
				pointLightParams[i].position = _pointLightPositions[i];
				pointLightParams[i].range = _pointLightRanges[i];
				pointLightParams[i].rangeMultiplier = 1f;
			}
			for (int j = _pointLightColors.Length; j < 6; j++)
			{
				PointLightDataSetDefaults(j);
			}
			pointLightDataMigrated = true;
			isDirty = true;
		}
		if (_pointLightTrackingCount > 6)
		{
			_pointLightTrackingCount = 6;
			isDirty = true;
		}
		if (pointLightParams != null)
		{
			if (pointLightParams.Length != 6)
			{
				PointLightParams[] array = new PointLightParams[6];
				int num = Mathf.Min(array.Length, pointLightParams.Length);
				Array.Copy(pointLightParams, array, num);
				pointLightParams = array;
				for (int k = num; k < array.Length; k++)
				{
					PointLightDataSetDefaults(k);
				}
				isDirty = true;
			}
			for (int l = 0; l < pointLightParams.Length; l++)
			{
				if (pointLightParams[l].rangeMultiplier <= 0f)
				{
					pointLightParams[l].rangeMultiplier = 1f;
				}
			}
		}
		else
		{
			pointLightParams = new PointLightParams[6];
			for (int m = 0; m < pointLightParams.Length; m++)
			{
				PointLightDataSetDefaults(m);
			}
			isDirty = true;
		}
		if (currentLights == null || currentLights.Length != 6)
		{
			currentLights = new Light[6];
		}
	}

	private void PointLightDataSetDefaults(int k)
	{
		if (k < pointLightParams.Length)
		{
			pointLightParams[k].color = new Color(1f, 1f, 0f, 1f);
			pointLightParams[k].intensity = 1f;
			pointLightParams[k].intensityMultiplier = 1f;
			pointLightParams[k].range = 0f;
			pointLightParams[k].rangeMultiplier = 1f;
		}
	}

	private void SetPointLightMaterialProperties(Camera mainCamera)
	{
		int num = pointLightParams.Length;
		if (pointLightColorBuffer == null || pointLightColorBuffer.Length != num)
		{
			pointLightColorBuffer = new Vector4[num];
		}
		if (pointLightPositionBuffer == null || pointLightPositionBuffer.Length != num)
		{
			pointLightPositionBuffer = new Vector4[num];
		}
		Vector3 vector = ((mainCamera != null) ? mainCamera.transform.position : Vector3.zero);
		for (int i = 0; i < num; i++)
		{
			Vector3 position = pointLightParams[i].position;
			if (!sunShadowsActive)
			{
				position.y -= _baselineHeight;
			}
			float num2 = pointLightParams[i].range * pointLightParams[i].rangeMultiplier * _pointLightInscattering / 25f;
			float num3 = pointLightParams[i].intensity * pointLightParams[i].intensityMultiplier * _pointLightIntensity;
			if (num2 > 0f && num3 > 0f)
			{
				if (_distance > 0f)
				{
					float num4 = 0.01f / _noiseScale;
					float num5 = _distance * num4;
					float num6 = Mathf.Max(num5 * num5 - new Vector2((vector.x - position.x) * num4, (vector.z - position.z) * num4).sqrMagnitude, 0f);
					num6 *= _distanceFallOff * _distanceFallOff + 0.1f;
					num3 = ((num3 > num6) ? (num3 - num6) : 0f);
				}
				pointLightPositionBuffer[i].x = position.x;
				pointLightPositionBuffer[i].y = position.y;
				pointLightPositionBuffer[i].z = position.z;
				pointLightPositionBuffer[i].w = 0f;
				pointLightColorBuffer[i] = new Vector4(pointLightParams[i].color.r * num3, pointLightParams[i].color.g * num3, pointLightParams[i].color.b * num3, num2);
			}
			else
			{
				pointLightColorBuffer[i] = black;
			}
		}
		fogMat.SetVectorArray("_FogPointLightColor", pointLightColorBuffer);
		fogMat.SetVectorArray("_FogPointLightPosition", pointLightPositionBuffer);
	}

	public Light GetPointLight(int index)
	{
		if (index < 0 || index >= pointLightParams.Length)
		{
			return null;
		}
		return pointLightParams[index].light;
	}

	private void TrackNewLights()
	{
		lastFoundLights = UnityEngine.Object.FindObjectsOfType<Light>();
	}

	public void TrackPointLights(bool forceImmediateUpdate = false)
	{
		if (!_pointLightTrackingAuto)
		{
			return;
		}
		if (_pointLightTrackingPivot == null)
		{
			_pointLightTrackingPivot = base.transform;
		}
		if (forceImmediateUpdate || lastFoundLights == null || !Application.isPlaying || (_pointLightTrackingNewLightsCheckInterval > 0f && Time.time - trackPointCheckNewLightsLastTime > _pointLightTrackingNewLightsCheckInterval))
		{
			trackPointCheckNewLightsLastTime = Time.time;
			TrackNewLights();
		}
		int num = lastFoundLights.Length;
		if (lightBuffer == null || lightBuffer.Length != num)
		{
			lightBuffer = new Light[num];
		}
		for (int i = 0; i < num; i++)
		{
			lightBuffer[i] = lastFoundLights[i];
		}
		bool flag = false;
		for (int j = 0; j < pointLightParams.Length && j < currentLights.Length; j++)
		{
			Light light = null;
			if (j < _pointLightTrackingCount)
			{
				light = GetNearestLight(lightBuffer);
			}
			if (pointLightParams[j].light != light)
			{
				pointLightParams[j].light = light;
				pointLightParams[j].lightParams = null;
			}
			if (pointLightParams[j].range != 0f && light == null)
			{
				pointLightParams[j].range = 0f;
			}
			if (currentLights[j] != light)
			{
				currentLights[j] = light;
				flag = true;
			}
		}
		if (flag)
		{
			UpdateMaterialProperties();
		}
	}

	private Light GetNearestLight(Light[] lights)
	{
		float num = float.MaxValue;
		Vector3 position = _pointLightTrackingPivot.position;
		Light result = null;
		int num2 = -1;
		for (int i = 0; i < lights.Length; i++)
		{
			Light light = lights[i];
			if (!(light == null) && light.isActiveAndEnabled && light.type == LightType.Point)
			{
				float sqrMagnitude = (light.transform.position - position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = light;
					num = sqrMagnitude;
					num2 = i;
				}
			}
		}
		if (num2 >= 0)
		{
			lights[num2] = null;
		}
		return result;
	}

	public static VolumetricFog CreateFogArea(Vector3 position, float radius, float height = 16f, float fallOff = 1f)
	{
		VolumetricFog volumetricFog = CreateFogAreaPlaceholder(spherical: true, position, radius, height, radius);
		volumetricFog.preset = FOG_PRESET.SeaClouds;
		volumetricFog.transform.position = position;
		volumetricFog.skyHaze = 0f;
		volumetricFog.dithering = true;
		return volumetricFog;
	}

	public static VolumetricFog CreateFogArea(Vector3 position, Vector3 boxSize)
	{
		VolumetricFog volumetricFog = CreateFogAreaPlaceholder(spherical: false, position, boxSize.x * 0.5f, boxSize.y * 0.5f, boxSize.z * 0.5f);
		volumetricFog.preset = FOG_PRESET.SeaClouds;
		volumetricFog.transform.position = position;
		volumetricFog.height = boxSize.y * 0.98f;
		volumetricFog.skyHaze = 0f;
		return volumetricFog;
	}

	private static VolumetricFog CreateFogAreaPlaceholder(bool spherical, Vector3 position, float radius, float height, float depth)
	{
		GameObject obj = UnityEngine.Object.Instantiate(spherical ? Resources.Load<GameObject>("Prefabs/FogSphereArea") : Resources.Load<GameObject>("Prefabs/FogBoxArea"));
		obj.transform.position = position;
		obj.transform.localScale = new Vector3(radius, height, depth);
		return obj.GetComponent<VolumetricFog>();
	}

	public static void RemoveAllFogAreas()
	{
		VolumetricFog[] array = UnityEngine.Object.FindObjectsOfType<VolumetricFog>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null && !array[i].hasCamera)
			{
				UnityEngine.Object.DestroyImmediate(array[i].gameObject);
			}
		}
	}

	private void CheckFogAreaDimensions()
	{
		if (!_hasCamera && mr == null)
		{
			mr = GetComponent<MeshRenderer>();
		}
		if (mr == null)
		{
			return;
		}
		Vector3 extents = mr.bounds.extents;
		switch (_fogAreaTopology)
		{
		case FOG_AREA_TOPOLOGY.Box:
			fogAreaRadius = extents.x;
			fogAreaHeight = extents.y;
			fogAreaDepth = extents.z;
			break;
		case FOG_AREA_TOPOLOGY.Sphere:
			fogAreaRadius = extents.x;
			if (base.transform.localScale.z != base.transform.localScale.x)
			{
				base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y, base.transform.localScale.x);
			}
			break;
		}
		if (_fogAreaCenter != null)
		{
			if (_fogAreaFollowMode == FOG_AREA_FOLLOW_MODE.FullXYZ)
			{
				base.transform.position = _fogAreaCenter.transform.position;
			}
			else
			{
				base.transform.position = new Vector3(_fogAreaCenter.transform.position.x, base.transform.position.y, _fogAreaCenter.transform.position.z);
			}
		}
		fogAreaPosition = base.transform.position;
	}

	public void UpdateVolumeMask()
	{
		if (!_hasCamera || mainCamera == null)
		{
			return;
		}
		RemoveMaskCommandBuffer();
		if (!_enableMask)
		{
			return;
		}
		if (maskCommandBuffer != null)
		{
			maskCommandBuffer.Clear();
		}
		else
		{
			maskCommandBuffer = new CommandBuffer();
			maskCommandBuffer.name = "Volumetric Fog Mask Write";
		}
		if (maskMaterial == null)
		{
			maskMaterial = new Material(Shader.Find("VolumetricFogAndMist/MaskWrite"));
		}
		rtMaskDesc = new RenderTextureDescriptor(mainCamera.pixelWidth, mainCamera.pixelHeight);
		rtMaskDesc.colorFormat = RenderTextureFormat.Depth;
		rtMaskDesc.depthBufferBits = 24;
		rtMaskDesc.sRGB = false;
		rtMaskDesc.msaaSamples = 1;
		rtMaskDesc.useMipMap = false;
		rtMaskDesc.volumeDepth = 1;
		int num = Mathf.Max(1, _maskDownsampling);
		rtMaskDesc.width /= num;
		rtMaskDesc.height /= num;
		int num2 = Shader.PropertyToID("_VolumetricFogScreenMaskTexture");
		maskCommandBuffer.GetTemporaryRT(num2, rtMaskDesc);
		maskCommandBuffer.SetRenderTarget(num2);
		maskCommandBuffer.ClearRenderTarget(clearDepth: true, clearColor: false, Color.white);
		Renderer[] array = UnityEngine.Object.FindObjectsOfType<Renderer>();
		for (int i = 0; i < array.Length; i++)
		{
			if (((1 << array[i].gameObject.layer) & _maskLayer.value) != 0 && array[i].gameObject.activeSelf)
			{
				if (array[i].enabled && Application.isPlaying)
				{
					array[i].enabled = false;
				}
				maskCommandBuffer.DrawRenderer(array[i], maskMaterial);
			}
		}
		maskCommandBuffer.ReleaseTemporaryRT(num2);
		mainCamera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, maskCommandBuffer);
	}

	public void TogglePreviewMask()
	{
		Renderer[] array = UnityEngine.Object.FindObjectsOfType<Renderer>();
		for (int i = 0; i < array.Length; i++)
		{
			if (((1 << array[i].gameObject.layer) & _maskLayer.value) != 0 && array[i].gameObject.activeSelf)
			{
				array[i].enabled = !array[i].enabled;
			}
		}
	}

	private void RemoveMaskCommandBuffer()
	{
		if (maskCommandBuffer != null && mainCamera != null)
		{
			mainCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, maskCommandBuffer);
		}
	}

	private void FogOfWarInit()
	{
		if (fowTransitionList == null || fowTransitionList.Length != 10000)
		{
			fowTransitionList = new FogOfWarTransition[10000];
		}
		if (fowTransitionIndices == null)
		{
			fowTransitionIndices = new Dictionary<int, int>(10000);
		}
		else
		{
			fowTransitionIndices.Clear();
		}
		lastTransitionPos = -1;
		if (_fogOfWarTexture == null)
		{
			FogOfWarUpdateTexture();
		}
		else if (_fogOfWarEnabled && (fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length == 0))
		{
			ReloadFogOfWarTexture();
		}
	}

	private void FogOfWarDestroy()
	{
		if (canDestroyFOWTexture && _fogOfWarTexture != null)
		{
			UnityEngine.Object.DestroyImmediate(_fogOfWarTexture);
		}
		if (fowBlur1 != null)
		{
			fowBlur1.Release();
		}
		if (fowBlur2 != null)
		{
			fowBlur2.Release();
		}
	}

	public void ReloadFogOfWarTexture()
	{
		if (!(_fogOfWarTexture == null))
		{
			_fogOfWarTextureSize = _fogOfWarTexture.width;
			fogOfWarColorBuffer = _fogOfWarTexture.GetPixels32();
			lastTransitionPos = -1;
			fowTransitionIndices.Clear();
			isDirty = true;
			fogOfWarEnabled = true;
		}
	}

	private void FogOfWarUpdateTexture()
	{
		if (_fogOfWarEnabled && Application.isPlaying)
		{
			int scaledSize = GetScaledSize(_fogOfWarTextureSize, 1f);
			if (_fogOfWarTexture == null || _fogOfWarTexture.width != scaledSize || _fogOfWarTexture.height != scaledSize)
			{
				_fogOfWarTexture = new Texture2D(scaledSize, scaledSize, TextureFormat.Alpha8, mipChain: false);
				_fogOfWarTexture.hideFlags = HideFlags.DontSave;
				_fogOfWarTexture.filterMode = FilterMode.Bilinear;
				_fogOfWarTexture.wrapMode = TextureWrapMode.Clamp;
				canDestroyFOWTexture = true;
				ResetFogOfWar();
			}
		}
	}

	public void UpdateFogOfWar(bool forceUpload = false)
	{
		if (!_fogOfWarEnabled || _fogOfWarTexture == null)
		{
			return;
		}
		if (forceUpload)
		{
			requiresTextureUpload = true;
		}
		int width = _fogOfWarTexture.width;
		for (int i = 0; i <= lastTransitionPos; i++)
		{
			FogOfWarTransition fogOfWarTransition = fowTransitionList[i];
			if (!fogOfWarTransition.enabled)
			{
				continue;
			}
			float num = Time.time - fogOfWarTransition.startTime - fogOfWarTransition.startDelay;
			if (!(num > 0f))
			{
				continue;
			}
			float num2 = ((fogOfWarTransition.duration <= 0f) ? 1f : (num / fogOfWarTransition.duration));
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			else if (num2 > 1f)
			{
				num2 = 1f;
			}
			int num3 = (int)((float)fogOfWarTransition.initialAlpha + (float)(fogOfWarTransition.targetAlpha - fogOfWarTransition.initialAlpha) * num2);
			int num4 = fogOfWarTransition.y * width + fogOfWarTransition.x;
			fogOfWarColorBuffer[num4].a = (byte)num3;
			requiresTextureUpload = true;
			if (num2 >= 1f)
			{
				fowTransitionList[i].enabled = false;
				if (fogOfWarTransition.targetAlpha < 255 && _fogOfWarRestoreDelay > 0f)
				{
					AddFogOfWarTransitionSlot(fogOfWarTransition.x, fogOfWarTransition.y, (byte)fogOfWarTransition.targetAlpha, byte.MaxValue, _fogOfWarRestoreDelay, _fogOfWarRestoreDuration);
				}
			}
		}
		if (requiresTextureUpload)
		{
			requiresTextureUpload = false;
			_fogOfWarTexture.SetPixels32(fogOfWarColorBuffer);
			_fogOfWarTexture.Apply();
			if (_fogOfWarBlur)
			{
				SetFowBlurTexture();
			}
		}
	}

	private void SetFowBlurTexture()
	{
		if (fowBlur == null)
		{
			fowBlur = new Material(Shader.Find("VolumetricFogAndMist/FoWBlur"));
			fowBlur.hideFlags = HideFlags.DontSave;
		}
		if (!(fowBlur == null))
		{
			if (fowBlur1 == null || fowBlur1.width != _fogOfWarTexture.width || fowBlur2 == null || fowBlur2.width != _fogOfWarTexture.width)
			{
				CreateFoWBlurRTs();
			}
			fowBlur1.DiscardContents();
			Graphics.Blit(_fogOfWarTexture, fowBlur1, fowBlur, 0);
			fowBlur2.DiscardContents();
			Graphics.Blit(fowBlur1, fowBlur2, fowBlur, 1);
			fogMat.SetTexture("_FogOfWar", fowBlur2);
		}
	}

	private void CreateFoWBlurRTs()
	{
		if (fowBlur1 != null)
		{
			fowBlur1.Release();
		}
		if (fowBlur2 != null)
		{
			fowBlur2.Release();
		}
		RenderTextureDescriptor desc = new RenderTextureDescriptor(_fogOfWarTexture.width, _fogOfWarTexture.height, RenderTextureFormat.ARGB32, 0);
		fowBlur1 = new RenderTexture(desc);
		fowBlur2 = new RenderTexture(desc);
	}

	public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha)
	{
		SetFogOfWarAlpha(worldPosition, radius, fogNewAlpha, 1f);
	}

	public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha, float duration)
	{
		SetFogOfWarAlpha(worldPosition, radius, fogNewAlpha, blendAlpha: true, duration, _fogOfWarSmoothness, _fogOfWarRestoreDelay, _fogOfWarRestoreDuration);
	}

	public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha, float duration, float smoothness)
	{
		SetFogOfWarAlpha(worldPosition, radius, fogNewAlpha, blendAlpha: true, duration, smoothness, _fogOfWarRestoreDelay, _fogOfWarRestoreDuration);
	}

	public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha, bool blendAlpha, float duration, float smoothness, float restoreDelay, float restoreDuration)
	{
		if (_fogOfWarTexture == null || fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length == 0)
		{
			return;
		}
		float num = (worldPosition.x - _fogOfWarCenter.x) / _fogOfWarSize.x + 0.5f;
		if (num < 0f || num > 1f)
		{
			return;
		}
		float num2 = (worldPosition.z - _fogOfWarCenter.z) / _fogOfWarSize.z + 0.5f;
		if (num2 < 0f || num2 > 1f)
		{
			return;
		}
		int width = _fogOfWarTexture.width;
		int num3 = _fogOfWarTexture.height;
		int num4 = (int)(num * (float)width);
		int num5 = (int)(num2 * (float)num3);
		float num6 = 0.0001f + smoothness;
		int num7 = num5 * width + num4;
		byte b = (byte)(fogNewAlpha * 255f);
		float num8 = radius / _fogOfWarSize.z;
		int num9 = (int)((float)num3 * num8);
		int num10 = num9 * num9;
		for (int i = num5 - num9; i <= num5 + num9; i++)
		{
			if (i <= 0 || i >= num3 - 1)
			{
				continue;
			}
			for (int j = num4 - num9; j <= num4 + num9; j++)
			{
				if (j <= 0 || j >= width - 1)
				{
					continue;
				}
				int num11 = (num5 - i) * (num5 - i) + (num4 - j) * (num4 - j);
				if (num11 > num10)
				{
					continue;
				}
				num7 = i * width + j;
				Color32 color = fogOfWarColorBuffer[num7];
				if (!blendAlpha)
				{
					color.a = byte.MaxValue;
				}
				num11 = num10 - num11;
				float num12 = (float)num11 / ((float)num10 * num6);
				num12 = 1f - num12;
				if (num12 < 0f)
				{
					num12 = 0f;
				}
				else if (num12 > 1f)
				{
					num12 = 1f;
				}
				byte b2 = (byte)((float)(int)b + (float)(color.a - b) * num12);
				if (b2 >= byte.MaxValue)
				{
					continue;
				}
				if (duration > 0f)
				{
					AddFogOfWarTransitionSlot(j, i, color.a, b2, 0f, duration);
					continue;
				}
				color.a = b2;
				fogOfWarColorBuffer[num7] = color;
				requiresTextureUpload = true;
				if (restoreDelay > 0f)
				{
					AddFogOfWarTransitionSlot(j, i, b2, byte.MaxValue, restoreDelay, restoreDuration);
				}
			}
		}
	}

	public void SetFogOfWarAlpha(Bounds bounds, float fogNewAlpha, float duration)
	{
		SetFogOfWarAlpha(bounds, fogNewAlpha, blendAlpha: true, duration, _fogOfWarSmoothness, _fogOfWarRestoreDelay, _fogOfWarRestoreDuration);
	}

	public void SetFogOfWarAlpha(Bounds bounds, float fogNewAlpha, float duration, float smoothness)
	{
		SetFogOfWarAlpha(bounds, fogNewAlpha, blendAlpha: true, duration, smoothness, _fogOfWarRestoreDelay, _fogOfWarRestoreDuration);
	}

	public void SetFogOfWarAlpha(Bounds bounds, float fogNewAlpha, bool blendAlpha, float duration, float smoothness, float restoreDelay, float restoreDuration)
	{
		if (_fogOfWarTexture == null || fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length == 0)
		{
			return;
		}
		Vector3 center = bounds.center;
		float num = (center.x - _fogOfWarCenter.x) / _fogOfWarSize.x + 0.5f;
		if (num < 0f || num > 1f)
		{
			return;
		}
		float num2 = (center.z - _fogOfWarCenter.z) / _fogOfWarSize.z + 0.5f;
		if (num2 < 0f || num2 > 1f)
		{
			return;
		}
		int width = _fogOfWarTexture.width;
		int num3 = _fogOfWarTexture.height;
		int num4 = (int)(num * (float)width);
		int num5 = (int)(num2 * (float)num3);
		int num6 = num5 * width + num4;
		byte b = (byte)(fogNewAlpha * 255f);
		float num7 = bounds.extents.z / _fogOfWarSize.z;
		float num8 = bounds.extents.x / _fogOfWarSize.x;
		float num9 = ((num8 > num7) ? 1f : (num7 / num8));
		float num10 = ((num8 > num7) ? (num8 / num7) : 1f);
		int num11 = (int)((float)num3 * num7);
		int num12 = num11 * num11;
		int num13 = (int)((float)width * num8);
		int num14 = num13 * num13;
		float num15 = 0.0001f + smoothness;
		for (int i = num5 - num11; i <= num5 + num11; i++)
		{
			if (i <= 0 || i >= num3 - 1)
			{
				continue;
			}
			int num16 = (num5 - i) * (num5 - i);
			num16 = num12 - num16;
			float num17 = (float)num16 * num9 / ((float)num12 * num15);
			for (int j = num4 - num13; j <= num4 + num13; j++)
			{
				if (j <= 0 || j >= width - 1)
				{
					continue;
				}
				int num18 = (num4 - j) * (num4 - j);
				num6 = i * width + j;
				Color32 color = fogOfWarColorBuffer[num6];
				if (!blendAlpha)
				{
					color.a = byte.MaxValue;
				}
				num18 = num14 - num18;
				float num19 = (float)num18 * num10 / ((float)num14 * num15);
				float num20 = ((num17 < num19) ? num17 : num19);
				num20 = 1f - num20;
				if (num20 < 0f)
				{
					num20 = 0f;
				}
				else if (num20 > 1f)
				{
					num20 = 1f;
				}
				byte b2 = (byte)((float)(int)b + (float)(color.a - b) * num20);
				if (b2 >= byte.MaxValue)
				{
					continue;
				}
				if (duration > 0f)
				{
					AddFogOfWarTransitionSlot(j, i, color.a, b2, 0f, duration);
					continue;
				}
				color.a = b2;
				fogOfWarColorBuffer[num6] = color;
				requiresTextureUpload = true;
				if (restoreDelay > 0f)
				{
					AddFogOfWarTransitionSlot(j, i, b2, byte.MaxValue, restoreDelay, restoreDuration);
				}
			}
		}
	}

	public void ResetFogOfWarAlpha(Vector3 worldPosition, float radius)
	{
		if (_fogOfWarTexture == null || fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length == 0)
		{
			return;
		}
		float num = (worldPosition.x - _fogOfWarCenter.x) / _fogOfWarSize.x + 0.5f;
		if (num < 0f || num > 1f)
		{
			return;
		}
		float num2 = (worldPosition.z - _fogOfWarCenter.z) / _fogOfWarSize.z + 0.5f;
		if (num2 < 0f || num2 > 1f)
		{
			return;
		}
		int width = _fogOfWarTexture.width;
		int num3 = _fogOfWarTexture.height;
		int num4 = (int)(num * (float)width);
		int num5 = (int)(num2 * (float)num3);
		float num6 = radius / _fogOfWarSize.z;
		int num7 = (int)((float)num3 * num6);
		int num8 = num7 * num7;
		for (int i = num5 - num7; i <= num5 + num7; i++)
		{
			if (i <= 0 || i >= num3 - 1)
			{
				continue;
			}
			for (int j = num4 - num7; j <= num4 + num7; j++)
			{
				if (j > 0 && j < width - 1 && (num5 - i) * (num5 - i) + (num4 - j) * (num4 - j) <= num8)
				{
					int num9 = i * width + j;
					Color32 color = fogOfWarColorBuffer[num9];
					color.a = byte.MaxValue;
					fogOfWarColorBuffer[num9] = color;
					requiresTextureUpload = true;
				}
			}
		}
	}

	public void ResetFogOfWarAlpha(Bounds bounds)
	{
		ResetFogOfWarAlpha(bounds.center, bounds.extents.x, bounds.extents.z);
	}

	public void ResetFogOfWarAlpha(Vector3 position, Vector3 size)
	{
		ResetFogOfWarAlpha(position, size.x * 0.5f, size.z * 0.5f);
	}

	public void ResetFogOfWarAlpha(Vector3 position, float extentsX, float extentsZ)
	{
		if (_fogOfWarTexture == null || fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length == 0)
		{
			return;
		}
		float num = (position.x - _fogOfWarCenter.x) / _fogOfWarSize.x + 0.5f;
		if (num < 0f || num > 1f)
		{
			return;
		}
		float num2 = (position.z - _fogOfWarCenter.z) / _fogOfWarSize.z + 0.5f;
		if (num2 < 0f || num2 > 1f)
		{
			return;
		}
		int width = _fogOfWarTexture.width;
		int num3 = _fogOfWarTexture.height;
		int num4 = (int)(num * (float)width);
		int num5 = (int)(num2 * (float)num3);
		float num6 = extentsZ / _fogOfWarSize.z;
		float num7 = extentsX / _fogOfWarSize.x;
		int num8 = (int)((float)num3 * num6);
		int num9 = (int)((float)width * num7);
		for (int i = num5 - num8; i <= num5 + num8; i++)
		{
			if (i <= 0 || i >= num3 - 1)
			{
				continue;
			}
			for (int j = num4 - num9; j <= num4 + num9; j++)
			{
				if (j > 0 && j < width - 1)
				{
					int num10 = i * width + j;
					Color32 color = fogOfWarColorBuffer[num10];
					color.a = byte.MaxValue;
					fogOfWarColorBuffer[num10] = color;
					requiresTextureUpload = true;
				}
			}
		}
	}

	public void ResetFogOfWar(byte alpha = byte.MaxValue)
	{
		if (!(_fogOfWarTexture == null) && isPartOfScene)
		{
			int num = _fogOfWarTexture.height;
			int width = _fogOfWarTexture.width;
			int num2 = num * width;
			if (fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length != num2)
			{
				fogOfWarColorBuffer = new Color32[num2];
			}
			Color32 color = new Color32(alpha, alpha, alpha, alpha);
			for (int i = 0; i < num2; i++)
			{
				fogOfWarColorBuffer[i] = color;
			}
			_fogOfWarTexture.SetPixels32(fogOfWarColorBuffer);
			_fogOfWarTexture.Apply();
			lastTransitionPos = -1;
			fowTransitionIndices.Clear();
			isDirty = true;
		}
	}

	private void AddFogOfWarTransitionSlot(int x, int y, byte initialAlpha, byte targetAlpha, float delay, float duration)
	{
		int key = y * 64000 + x;
		if (!fowTransitionIndices.TryGetValue(key, out var value))
		{
			value = -1;
			for (int i = 0; i <= lastTransitionPos; i++)
			{
				if (!fowTransitionList[i].enabled)
				{
					value = i;
					fowTransitionIndices[key] = value;
					break;
				}
			}
		}
		if (value >= 0 && fowTransitionList[value].enabled && (fowTransitionList[value].x != x || fowTransitionList[value].y != y))
		{
			value = -1;
		}
		if (value < 0)
		{
			if (lastTransitionPos >= 9999)
			{
				return;
			}
			value = ++lastTransitionPos;
			fowTransitionIndices[key] = value;
		}
		fowTransitionList[value].x = x;
		fowTransitionList[value].y = y;
		fowTransitionList[value].duration = duration;
		fowTransitionList[value].startTime = Time.time;
		fowTransitionList[value].startDelay = delay;
		fowTransitionList[value].initialAlpha = initialAlpha;
		fowTransitionList[value].targetAlpha = targetAlpha;
		fowTransitionList[value].enabled = true;
	}

	public float GetFogOfWarAlpha(Vector3 worldPosition)
	{
		if (fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length == 0 || _fogOfWarTexture == null)
		{
			return 1f;
		}
		float num = (worldPosition.x - _fogOfWarCenter.x) / _fogOfWarSize.x + 0.5f;
		if (num < 0f || num > 1f)
		{
			return 1f;
		}
		float num2 = (worldPosition.z - _fogOfWarCenter.z) / _fogOfWarSize.z + 0.5f;
		if (num2 < 0f || num2 > 1f)
		{
			return 1f;
		}
		int width = _fogOfWarTexture.width;
		int num3 = _fogOfWarTexture.height;
		int num4 = (int)(num * (float)width);
		int num5 = (int)(num2 * (float)num3) * width + num4;
		if (num5 < 0 || num5 >= fogOfWarColorBuffer.Length)
		{
			return 1f;
		}
		return (float)(int)fogOfWarColorBuffer[num5].a / 255f;
	}

	private void ShowFoWGizmo()
	{
		if (_maskEditorEnabled && _fogOfWarEnabled && !Application.isPlaying)
		{
			Vector3 center = _fogOfWarCenter;
			center.y = -10f;
			center.y += _baselineHeight + _height * 0.5f;
			Vector3 size = new Vector3(_fogOfWarSize.x, 0.1f, _fogOfWarSize.z);
			for (int i = 0; i < 5; i++)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireCube(center, size);
				center.y += 0.5f;
				Gizmos.color = Color.white;
				Gizmos.DrawWireCube(center, size);
				center.y += 0.5f;
			}
		}
	}
}
