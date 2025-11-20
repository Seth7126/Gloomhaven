using UnityEngine;

namespace RenderHeads.Media.AVProMovieCapture;

[AddComponentMenu("AVPro Movie Capture/Motion Blur", 301)]
public class MotionBlur : MonoBehaviour
{
	[SerializeField]
	public RenderTextureFormat _format = RenderTextureFormat.ARGBFloat;

	[SerializeField]
	private int _numSamples = 16;

	private RenderTexture _accum;

	private RenderTexture _lastComp;

	private Material _addMaterial;

	private Material _divMaterial;

	private int _frameCount;

	private int _targetWidth;

	private int _targetHeight;

	private bool _isDirty;

	private static int _propNumSamples;

	private static int _propWeight;

	[SerializeField]
	public float _bias = 1f;

	private float _total;

	public bool IsFrameAccumulated { get; private set; }

	public int NumSamples
	{
		get
		{
			return _numSamples;
		}
		set
		{
			_numSamples = value;
			OnNumSamplesChanged();
		}
	}

	public int FrameCount => _frameCount;

	public RenderTexture FinalTexture => _lastComp;

	private void Awake()
	{
		if (_propNumSamples == 0)
		{
			_propNumSamples = Shader.PropertyToID("_NumSamples");
			_propWeight = Shader.PropertyToID("_Weight");
		}
	}

	public void SetTargetSize(int width, int height)
	{
		if (_targetWidth != width || _targetHeight != height)
		{
			_targetWidth = width;
			_targetHeight = height;
			_isDirty = true;
		}
	}

	private void Start()
	{
		Setup();
	}

	private void OnEnable()
	{
		_frameCount = 0;
		IsFrameAccumulated = false;
		ClearAccumulation();
	}

	private void Setup()
	{
		if (_addMaterial == null)
		{
			Shader shader = Resources.Load<Shader>("AVProMovieCapture_MotionBlur_Add");
			Shader shader2 = Resources.Load<Shader>("AVProMovieCapture_MotionBlur_Div");
			_addMaterial = new Material(shader);
			_divMaterial = new Material(shader2);
		}
		if (_targetWidth == 0 && _targetHeight == 0)
		{
			_targetWidth = Screen.width;
			_targetHeight = Screen.height;
		}
		if (_accum != null)
		{
			_accum.Release();
			Object.Destroy(_accum);
			_accum = null;
		}
		if (_lastComp != null)
		{
			_lastComp.Release();
			Object.Destroy(_lastComp);
			_lastComp = null;
		}
		_accum = new RenderTexture(_targetWidth, _targetHeight, 0, _format, RenderTextureReadWrite.Default);
		_lastComp = new RenderTexture(_targetWidth, _targetHeight, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
		_accum.filterMode = FilterMode.Point;
		_lastComp.filterMode = FilterMode.Bilinear;
		_accum.Create();
		_lastComp.Create();
		OnNumSamplesChanged();
		_isDirty = false;
	}

	private void ClearAccumulation()
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = _accum;
		GL.Clear(clearDepth: false, clearColor: true, Color.black);
		RenderTexture.active = active;
	}

	private void OnDestroy()
	{
		if (_addMaterial != null)
		{
			Object.Destroy(_addMaterial);
			_addMaterial = null;
		}
		if (_divMaterial != null)
		{
			Object.Destroy(_divMaterial);
			_divMaterial = null;
		}
		if (_accum != null)
		{
			_accum.Release();
			Object.Destroy(_accum);
			_accum = null;
		}
		if (_lastComp != null)
		{
			_lastComp.Release();
			Object.Destroy(_lastComp);
			_lastComp = null;
		}
	}

	public void OnNumSamplesChanged()
	{
		if (_divMaterial != null)
		{
			_addMaterial.SetFloat(_propWeight, 1f);
			_divMaterial.SetFloat(_propNumSamples, _numSamples);
		}
	}

	private static float LerpUnclamped(float a, float b, float t)
	{
		return a + (b - a) * t;
	}

	private void ApplyWeighting()
	{
		float f = (float)_frameCount / (float)_numSamples;
		f = Mathf.Pow(f, 2f);
		_total += f;
		float num = (float)_numSamples / 2f + 0.5f;
		num = _total;
		f = LerpUnclamped(f, 1f, _bias);
		num = LerpUnclamped(num, _numSamples, _bias);
		_addMaterial.SetFloat(_propWeight, f);
		_divMaterial.SetFloat(_propNumSamples, num);
	}

	public void Accumulate(Texture src)
	{
		if (_isDirty)
		{
			Setup();
		}
		Graphics.Blit(src, _accum, _addMaterial);
		_frameCount++;
		if (_frameCount >= _numSamples)
		{
			Graphics.Blit(_accum, _lastComp, _divMaterial);
			ClearAccumulation();
			IsFrameAccumulated = true;
			_frameCount = 0;
			_total = 0f;
		}
		else
		{
			IsFrameAccumulated = false;
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		Accumulate(src);
		Graphics.Blit(_lastComp, dst);
	}
}
