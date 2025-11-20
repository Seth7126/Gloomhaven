using System.Collections;
using Chronos;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniCardEffects : MonoBehaviour
{
	public enum FXTask
	{
		BurnCard,
		HighlightBurn,
		DiscardMode,
		LostMode,
		HighlightGhost
	}

	private static readonly int _posAndBounds = Shader.PropertyToID("_PosAndBounds");

	private static readonly int _greyOut = Shader.PropertyToID("_GreyOut");

	private static readonly int _flow = Shader.PropertyToID("_Flow");

	private static readonly int _dissolve = Shader.PropertyToID("_Dissolve");

	private static readonly int _burn = Shader.PropertyToID("_Burn");

	private static readonly int _burnColourTint = Shader.PropertyToID("_Burn_ColourTint");

	private static readonly int _flowOffset = Shader.PropertyToID("_Flow_Offset");

	private static readonly int _flowSpeed = Shader.PropertyToID("_Flow_Speed");

	private static readonly int _animNoiseMask = Shader.PropertyToID("_AnimNoise_Mask");

	private static readonly int _dissolveVerticalGradient = Shader.PropertyToID("_Dissolve_VerticalGradient");

	private static readonly int _tintColor = Shader.PropertyToID("_TintColor");

	private static readonly int _particleTexture = Shader.PropertyToID("_ParticleTexture");

	private static readonly int _speed = Shader.PropertyToID("_FlowSpeed");

	private static readonly int _offsetStrength = Shader.PropertyToID("_OffsetStrength");

	private static readonly int _thinHighlights = Shader.PropertyToID("_ThinHighlights");

	private static readonly int _thinHighlightMin = Shader.PropertyToID("_ThinHighlight_Min");

	private static readonly int _thinHighlightMax = Shader.PropertyToID("_ThinHighlight_Max");

	private static readonly int _flowNoiseTiling = Shader.PropertyToID("_Flow_NoiseTiling");

	private static readonly int _glow = Shader.PropertyToID("_Glow");

	private static readonly int _fxAnimMin = Shader.PropertyToID("_FXAnim_Min");

	private static readonly int _fxAnimMax = Shader.PropertyToID("_FXAnim_Max");

	public Image[] imgComp;

	public TextMeshProUGUI[] txtComp;

	private Color[] txtColourStore;

	private bool[] txtGradientStore;

	private Color[] imgColourStore;

	private int txtCount;

	private int imgCount;

	private bool[] txtAffected;

	private bool[] imgAffected;

	public Image fgFx;

	private string fxAnim = "_FXAnim";

	public FXTask task;

	private bool isBurning;

	private bool isGhosting;

	private float mc_Burn;

	private Color mc_Burn_ColourTint;

	private float mc_Flow;

	private float mc_Flow_Offset;

	private float mc_Flow_Speed;

	private float mc_AnimNoise_Mask_tile;

	private float mc_Dissolve;

	private float mc_Dissolve_VerticalGradient;

	private float mc_GreyOut;

	[SerializeField]
	private Vector3 _localPositionEffect;

	[SerializeField]
	private Vector3 _localEulerRotate;

	[SerializeField]
	private Vector3 _localScale;

	private Color fx_Smoke_color;

	private Color fx_Overlay_color;

	private float fx_Overlay_anim;

	private Texture2D fx_Overlay_texture;

	private float fx_Overlay_FlowSpeed;

	private float fx_Overlay_OffsetStrength;

	private float fx_Overlay_ThinHighlights;

	private float fx_Overlay_ThinHighlight_Min;

	private float fx_Overlay_ThinHighlight_Max;

	private float fx_Overlay_Flow_NoiseTiling;

	private float fx_Overlay_Flow_NoiseSpeed;

	private float fx_Overlay_Glow;

	private ParticleSystem _smokeEffect;

	public Texture2D overlayFrameGhost;

	public Texture2D overlayFrameBurn;

	private bool _useMaterials => !PlatformLayer.Setting.SimplifiedUISettings.MiniCardEffectWithoutMaterials;

	[UsedImplicitly]
	private void Awake()
	{
		Initialize();
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		HideParticle();
	}

	private void ShowSmoke()
	{
		SpawnParticle();
	}

	private void Initialize()
	{
		txtColourStore = new Color[txtComp.Length];
		txtGradientStore = new bool[txtComp.Length];
		txtCount = txtComp.Length;
		for (int i = 0; i < txtCount; i++)
		{
			if (txtComp[i] != null)
			{
				txtColourStore[i] = txtComp[i].color;
				txtGradientStore[i] = txtComp[i].enableVertexGradient;
			}
		}
		imgColourStore = new Color[imgComp.Length];
		imgCount = imgComp.Length;
		for (int j = 0; j < imgCount; j++)
		{
			if (imgComp[j] != null)
			{
				imgColourStore[j] = imgComp[j].color;
			}
		}
		RectTransform rectTransform = base.transform as RectTransform;
		if (_useMaterials)
		{
			Image[] array = imgComp;
			foreach (Image image in array)
			{
				if (image != null)
				{
					image.material = new Material(image.material);
				}
			}
		}
		else
		{
			for (int l = 0; l < imgCount; l++)
			{
				if (imgComp[l] != null)
				{
					imgComp[l].material = null;
				}
			}
		}
		fgFx.material = new Material(fgFx.material);
		if (_useMaterials)
		{
			for (int m = 0; m < imgCount; m++)
			{
				if (imgComp[m] != null)
				{
					imgComp[m].material.SetVector(_posAndBounds, new Vector4(rectTransform.position.x, rectTransform.position.y, rectTransform.rect.width, rectTransform.rect.height));
				}
			}
		}
		RestoreCard();
	}

	public void ToggleEffect(bool active, FXTask effect)
	{
		RestoreCard();
		txtAffected = new bool[txtCount];
		imgAffected = new bool[imgCount];
		for (int i = 0; i < txtCount; i++)
		{
			txtAffected[i] = false;
		}
		for (int j = 0; j < imgCount; j++)
		{
			imgAffected[j] = false;
		}
		ToggleAdditiveEffect(active, effect);
	}

	public void ToggleAdditiveEffect(bool active, FXTask effect)
	{
		if (active)
		{
			switch (effect)
			{
			case FXTask.DiscardMode:
				GhostOutOn(ghostAnim: true);
				break;
			case FXTask.LostMode:
				BurnCard(burnAnim: true);
				break;
			case FXTask.BurnCard:
				BurnCard(burnAnim: true);
				break;
			}
		}
		else
		{
			switch (effect)
			{
			case FXTask.DiscardMode:
				GhostOutOn(ghostAnim: false);
				break;
			case FXTask.LostMode:
				BurnCard(burnAnim: false);
				break;
			case FXTask.BurnCard:
				BurnCard(burnAnim: false);
				break;
			}
		}
	}

	private void BurnCard(bool burnAnim)
	{
		if (Choreographer.s_Choreographer == null)
		{
			BurnCard();
		}
		else
		{
			Choreographer.s_Choreographer.StartCoroutine(BurnCardTimeline(burnAnim));
		}
	}

	private void HighlightBurnOn()
	{
	}

	private void HighlightBurnOff()
	{
	}

	private void GhostOutOn(bool ghostAnim)
	{
		if (Choreographer.s_Choreographer == null)
		{
			GhostCard();
		}
		else
		{
			Choreographer.s_Choreographer.StartCoroutine(GhostOutOnTimeline(ghostAnim));
		}
	}

	[ContextMenu("RestoreCard")]
	public void RestoreCard()
	{
		if (_useMaterials)
		{
			for (int i = 0; i < imgCount; i++)
			{
				if (imgComp[i] != null)
				{
					imgComp[i].material.SetFloat(_greyOut, 0f);
					imgComp[i].material.SetFloat(_flow, 0f);
					imgComp[i].material.SetFloat(_dissolve, 0f);
					imgComp[i].material.SetFloat(_burn, 0f);
				}
			}
		}
		HideParticle();
		if (fgFx != null)
		{
			fgFx.material.SetFloat(fxAnim, 0f);
		}
		if (_useMaterials)
		{
			for (int j = 0; j < imgCount; j++)
			{
				if (imgComp[j] != null)
				{
					imgComp[j].GetComponent<Image>().color = imgColourStore[j];
				}
			}
		}
		for (int k = 0; k < txtCount; k++)
		{
			if (txtAffected != null && txtAffected[k] && txtComp[k] != null)
			{
				txtComp[k].color = txtColourStore[k];
			}
		}
	}

	public IEnumerator BurnCardTimeline(bool burnAnim)
	{
		fgFx.enabled = true;
		float burnTime = 2f;
		float dTime = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		mc_Burn = 0.691f;
		mc_Burn_ColourTint = new Color(0.36862746f, 0.14509805f, 0.07450981f, 0.601f);
		mc_Flow = 0.1f;
		mc_Flow_Offset = 0.01f;
		mc_Flow_Speed = 0.4f;
		mc_AnimNoise_Mask_tile = 40f;
		mc_Dissolve = 0.646f;
		mc_Dissolve_VerticalGradient = 0.2f;
		mc_GreyOut = 0.1f;
		fx_Smoke_color = new Color(10f / 51f, 10f / 51f, 10f / 51f, 0.5f);
		fx_Overlay_color = new Color(1f, 0.3f, 0f, 0.8f);
		fx_Overlay_anim = 0.5f;
		fx_Overlay_texture = overlayFrameBurn;
		fx_Overlay_FlowSpeed = 0.5f;
		fx_Overlay_OffsetStrength = 0.07f;
		fx_Overlay_ThinHighlights = 0.463f;
		fx_Overlay_ThinHighlight_Min = 0f;
		fx_Overlay_ThinHighlight_Max = 0.195f;
		fx_Overlay_Flow_NoiseTiling = 6f;
		fx_Overlay_Glow = 3f;
		float value = -0.522f;
		float value2 = 0.709f;
		if (_useMaterials)
		{
			for (int i = 0; i < imgCount; i++)
			{
				if (imgComp[i] != null)
				{
					imgComp[i].material.SetFloat(_burn, mc_Burn);
					imgComp[i].material.SetColor(_burnColourTint, mc_Burn_ColourTint);
					imgComp[i].material.SetFloat(_flowOffset, mc_Flow_Offset);
					imgComp[i].material.SetFloat(_flowSpeed, mc_Flow_Speed);
					imgComp[i].material.SetTextureScale(_animNoiseMask, new Vector2(mc_AnimNoise_Mask_tile, mc_AnimNoise_Mask_tile));
					imgComp[i].material.SetFloat(_dissolveVerticalGradient, mc_Dissolve_VerticalGradient);
				}
			}
		}
		fgFx.material.SetColor(_tintColor, fx_Overlay_color);
		fgFx.material.SetTexture(_particleTexture, fx_Overlay_texture);
		fgFx.material.SetFloat(_speed, fx_Overlay_FlowSpeed);
		fgFx.material.SetFloat(_offsetStrength, fx_Overlay_OffsetStrength);
		fgFx.material.SetFloat(_thinHighlights, fx_Overlay_ThinHighlights);
		fgFx.material.SetFloat(_thinHighlightMin, fx_Overlay_ThinHighlight_Min);
		fgFx.material.SetFloat(_thinHighlightMax, fx_Overlay_ThinHighlight_Max);
		fgFx.material.SetFloat(_flowNoiseTiling, fx_Overlay_Flow_NoiseTiling);
		fgFx.material.SetFloat(_glow, fx_Overlay_Glow);
		fgFx.material.SetFloat(_fxAnimMin, value);
		fgFx.material.SetFloat(_fxAnimMax, value2);
		if (burnAnim)
		{
			while (Timekeeper.instance.m_GlobalClock.time - startTime < burnTime)
			{
				dTime += Timekeeper.instance.m_GlobalClock.deltaTime / burnTime;
				if (_useMaterials)
				{
					for (int j = 0; j < imgCount; j++)
					{
						if (imgComp[j] != null)
						{
							imgComp[j].material.SetFloat(_greyOut, Mathf.Clamp01(dTime));
							imgComp[j].material.SetFloat(_flow, Mathf.Clamp01(dTime));
							imgComp[j].material.SetFloat(_dissolve, Mathf.Lerp(0f, mc_Dissolve, Mathf.Clamp01(dTime)));
						}
					}
				}
				if (fgFx != null)
				{
					fgFx.material.SetFloat(fxAnim, Mathf.Clamp(dTime * 4f, 0f, 1f) / 2f);
				}
				ShowSmoke();
				for (int k = 0; k < txtCount; k++)
				{
					if (txtAffected[k] && txtComp[k] != null)
					{
						txtComp[k].color = new Color(0.5f, 0.5f, 0.5f, 1f);
					}
				}
				yield return new WaitForEndOfFrame();
			}
		}
		else
		{
			BurnCard();
		}
	}

	private void BurnCard()
	{
		if (_useMaterials)
		{
			for (int i = 0; i < imgCount; i++)
			{
				if (imgComp[i] != null)
				{
					imgComp[i].material.SetFloat(_greyOut, mc_GreyOut);
					imgComp[i].material.SetFloat(_flow, mc_Flow);
					imgComp[i].material.SetFloat(_dissolve, mc_Dissolve);
				}
			}
		}
		fgFx.material.SetFloat(fxAnim, 0.5f);
		ShowSmoke();
		for (int j = 0; j < txtCount; j++)
		{
			if (txtAffected[j] && txtComp[j] != null)
			{
				txtComp[j].color = new Color(0.5f, 0.5f, 0.5f, 1f);
			}
		}
		fgFx.enabled = false;
	}

	public IEnumerator GhostOutOnTimeline(bool ghostAnim)
	{
		isGhosting = true;
		if (fgFx != null)
		{
			fgFx.gameObject.SetActive(value: true);
		}
		float animTime = 2f;
		float dTime = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		Color greyOutCol = new Color(1f, 1f, 1f, 1f);
		mc_Burn = 0.7f;
		mc_Burn_ColourTint = new Color(0.24313726f, 24f / 85f, 29f / 85f, 0.5f);
		mc_Flow = 0.1f;
		mc_Flow_Offset = 0.01f;
		mc_Flow_Speed = 0.2f;
		mc_AnimNoise_Mask_tile = 10f;
		mc_Dissolve = 0.646f;
		mc_Dissolve_VerticalGradient = 0.2f;
		mc_GreyOut = 0.1f;
		fx_Smoke_color = new Color(0.3019608f, 0.3019608f, 33f / 85f, 0.4f);
		fx_Overlay_color = new Color(0.8f, 0.8f, 1f, 0.25f);
		fx_Overlay_anim = 0.5f;
		fx_Overlay_texture = overlayFrameGhost;
		fx_Overlay_FlowSpeed = 0.2f;
		fx_Overlay_OffsetStrength = 0.07f;
		fx_Overlay_ThinHighlights = 0.125f;
		fx_Overlay_ThinHighlight_Min = 0.77f;
		fx_Overlay_ThinHighlight_Max = 0.9f;
		fx_Overlay_Flow_NoiseTiling = 2f;
		fx_Overlay_Glow = 0f;
		float value = -0.22f;
		float value2 = 1.31f;
		if (_useMaterials)
		{
			for (int i = 0; i < imgCount; i++)
			{
				if (imgComp[i] != null)
				{
					imgComp[i].material.SetFloat(_burn, mc_Burn);
					imgComp[i].material.SetColor(_burnColourTint, mc_Burn_ColourTint);
					imgComp[i].material.SetFloat(_flowOffset, mc_Flow_Offset);
					imgComp[i].material.SetFloat(_flowSpeed, mc_Flow_Speed);
					imgComp[i].material.SetTextureScale(_animNoiseMask, new Vector2(mc_AnimNoise_Mask_tile, mc_AnimNoise_Mask_tile));
					imgComp[i].material.SetFloat(_dissolveVerticalGradient, mc_Dissolve_VerticalGradient);
				}
			}
		}
		fgFx.material.SetColor(_tintColor, fx_Overlay_color);
		fgFx.material.SetTexture(_particleTexture, fx_Overlay_texture);
		fgFx.material.SetFloat(_speed, fx_Overlay_FlowSpeed);
		fgFx.material.SetFloat(_offsetStrength, fx_Overlay_OffsetStrength);
		fgFx.material.SetFloat(_thinHighlights, fx_Overlay_ThinHighlights);
		fgFx.material.SetFloat(_thinHighlightMin, fx_Overlay_ThinHighlight_Min);
		fgFx.material.SetFloat(_thinHighlightMax, fx_Overlay_ThinHighlight_Max);
		fgFx.material.SetFloat(_flowNoiseTiling, fx_Overlay_Flow_NoiseTiling);
		fgFx.material.SetFloat(_glow, fx_Overlay_Glow);
		fgFx.material.SetFloat(_fxAnimMin, value);
		fgFx.material.SetFloat(_fxAnimMax, value2);
		if (ghostAnim)
		{
			ShowSmoke();
			while (Timekeeper.instance.m_GlobalClock.time - startTime < animTime)
			{
				dTime += Timekeeper.instance.m_GlobalClock.deltaTime / animTime;
				if (_useMaterials)
				{
					for (int j = 0; j < imgCount; j++)
					{
						if (imgComp[j] != null)
						{
							imgComp[j].material.SetFloat(_greyOut, Mathf.Clamp01(dTime));
							imgComp[j].material.SetFloat(_flow, Mathf.Clamp01(dTime));
							imgComp[j].material.SetFloat(_dissolve, Mathf.Lerp(0f, mc_Dissolve, Mathf.Clamp01(dTime)));
						}
					}
				}
				if (fgFx != null)
				{
					fgFx.material.SetFloat(fxAnim, Mathf.Clamp(dTime * 4f, 0f, 1f) / 2f);
				}
				for (int k = 0; k < txtCount; k++)
				{
					if (txtAffected[k] && txtComp[k] != null)
					{
						txtComp[k].color = Color.Lerp(txtColourStore[k], greyOutCol, dTime);
						if (dTime > 0.5f)
						{
							txtComp[k].enableVertexGradient = false;
						}
					}
				}
				yield return new WaitForEndOfFrame();
			}
		}
		else
		{
			GhostCard();
		}
		yield return new WaitForEndOfFrame();
	}

	private void GhostCard()
	{
		Color b = new Color(1f, 1f, 1f, 1f);
		if (_useMaterials)
		{
			for (int i = 0; i < imgCount; i++)
			{
				if (imgComp[i] != null)
				{
					imgComp[i].material.SetFloat(_greyOut, mc_GreyOut);
					imgComp[i].material.SetFloat(_flow, mc_Flow);
					imgComp[i].material.SetFloat(_dissolve, mc_Dissolve);
				}
			}
		}
		ShowSmoke();
		fgFx.material.SetFloat(fxAnim, 0.5f);
		for (int j = 0; j < txtCount; j++)
		{
			if (txtAffected[j] && txtComp[j] != null)
			{
				txtComp[j].color = Color.Lerp(txtColourStore[j], b, 1f);
				txtComp[j].enableVertexGradient = false;
			}
		}
	}

	private void SpawnParticle()
	{
		if (base.gameObject.activeInHierarchy && !PlatformLayer.Setting.LowParticlesUse.NoCardsParticles)
		{
			if (_smokeEffect == null)
			{
				GameObject gameObject = ObjectPool.Spawn(GlobalSettings.Instance.VisualEffects.CardSmokeMini, base.transform);
				_smokeEffect = gameObject.GetComponent<ParticleSystem>();
			}
			if (_smokeEffect != null)
			{
				Color color = fx_Smoke_color;
				ParticleSystem.MainModule main = _smokeEffect.main;
				main.startColor = color;
				Transform obj = _smokeEffect.transform;
				obj.localPosition = _localPositionEffect;
				obj.localEulerAngles = _localEulerRotate;
				obj.localScale = _localScale;
			}
		}
	}

	private void HideParticle()
	{
		if (_smokeEffect != null)
		{
			ObjectPool.Recycle(_smokeEffect.gameObject, 0.3f, GlobalSettings.Instance.VisualEffects.CardSmokeMini);
			_smokeEffect = null;
		}
	}
}
