using System.Collections;
using System.Collections.Generic;
using Chronos;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardEffects : MonoBehaviour
{
	public enum FXTask
	{
		BurnCard,
		DiscardMode,
		LostMode
	}

	private static readonly int _greyOut = Shader.PropertyToID("_GreyOut");

	private static readonly int _flow = Shader.PropertyToID("_Flow");

	private static readonly int _dissolve = Shader.PropertyToID("_Dissolve");

	private static readonly int _burn = Shader.PropertyToID("_Burn");

	private static readonly int _posAndBounds = Shader.PropertyToID("_PosAndBounds");

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

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI _header;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI _titleText;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI _topDefActionTxt;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI _bottomDefActionTxt;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI _initiativeText;

	[SerializeField]
	[UsedImplicitly]
	private Image _uiFxOverlay;

	[SerializeField]
	private Material _lowMaterial;

	[SerializeField]
	[UsedImplicitly]
	private Image _headerImage;

	[SerializeField]
	[UsedImplicitly]
	private Image _topButton;

	[SerializeField]
	[UsedImplicitly]
	private Image _bottomAction;

	[SerializeField]
	[UsedImplicitly]
	private Image _topDefAction;

	[SerializeField]
	[UsedImplicitly]
	private Image _botDefAction;

	[SerializeField]
	[UsedImplicitly]
	private Image _topDefActionIcon;

	[SerializeField]
	[UsedImplicitly]
	private Image _botDefActionIcon;

	private Image[] imgComp;

	private TextMeshProUGUI[] txtComp;

	private Color[] txtColourStore;

	private bool[] txtGradientStore;

	private Color[] imgColourStore;

	private int txtCount;

	private int imgCount;

	private bool[] txtAffected;

	private bool[] imgAffected;

	private Image fgFx;

	private Image header;

	private Image topAction;

	private Image bottomAction;

	private Image topDefAction;

	private Image bottomDefAction;

	private Image topDefActionIcon;

	private Image bottomDefActionIcon;

	private TextMeshProUGUI headerTxt;

	private TextMeshProUGUI topActionTxt;

	private TextMeshProUGUI bottomActionTxt;

	private TextMeshProUGUI topDefActionTxt;

	private TextMeshProUGUI bottomDefActionTxt;

	private TextMeshProUGUI initiativeTxt;

	private string fxAnim = "_FXAnim";

	public bool affectHeader = true;

	public bool affectTop = true;

	public bool affectBottom = true;

	public bool affectTopDefault = true;

	public bool affectBottomDefault = true;

	public FXTask task;

	private bool isBurning;

	private bool isGhosting;

	public Vector2 canvasPosition;

	public Vector2 cardBounds;

	private float mc_Burn;

	private Color mc_Burn_ColourTint;

	private float mc_Flow;

	private float mc_Flow_Offset;

	private float mc_Flow_Speed;

	private float mc_AnimNoise_Mask_tile;

	private float mc_Dissolve;

	private float mc_Dissolve_VerticalGradient;

	private float mc_GreyOut;

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

	public Texture2D overlayFrameGhost;

	public Texture2D overlayFrameBurn;

	private Color burntTextColor = new Color32(143, 58, 44, byte.MaxValue);

	private Coroutine coroutine;

	private HashSet<FXTask> toggledEffects = new HashSet<FXTask>();

	private bool initialized;

	private ParticleSystem _smokeEffect;

	private bool _useLowEffect
	{
		get
		{
			if (PlatformLayer.Setting.SimplifiedUI)
			{
				return PlatformLayer.Setting.SimplifiedUISettings.AbilityCardEffectWithoutMaterials;
			}
			return false;
		}
	}

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

	private void Initialize()
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		headerTxt = _header;
		topDefActionTxt = _topDefActionTxt;
		bottomDefActionTxt = _bottomDefActionTxt;
		initiativeTxt = _initiativeText;
		txtComp = new TextMeshProUGUI[6];
		txtComp[0] = headerTxt;
		txtComp[1] = topActionTxt;
		txtComp[2] = bottomActionTxt;
		txtComp[3] = topDefActionTxt;
		txtComp[4] = bottomDefActionTxt;
		txtComp[5] = initiativeTxt;
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
		fgFx = _uiFxOverlay;
		header = _headerImage;
		topAction = _topButton;
		bottomAction = _bottomAction;
		topDefAction = _topDefAction;
		bottomDefAction = _botDefAction;
		topDefActionIcon = _topDefActionIcon;
		bottomDefActionIcon = _botDefActionIcon;
		imgComp = new Image[10];
		imgComp[2] = header;
		imgComp[3] = topAction;
		imgComp[4] = bottomAction;
		imgComp[5] = topDefAction;
		imgComp[6] = bottomDefAction;
		imgComp[7] = topDefActionIcon;
		imgComp[8] = bottomDefActionIcon;
		imgColourStore = new Color[imgComp.Length];
		imgCount = imgComp.Length;
		for (int j = 0; j < imgCount; j++)
		{
			if (imgComp[j] != null)
			{
				imgColourStore[j] = imgComp[j].color;
			}
		}
		canvasPosition = ((RectTransform)base.transform.parent).anchoredPosition;
		Rect rect = header.rectTransform.rect;
		cardBounds = new Vector2(rect.width, rect.height);
		if (_useLowEffect)
		{
			Material material = new Material(_lowMaterial);
			material.enableInstancing = true;
			Image[] array = imgComp;
			foreach (Image image in array)
			{
				if (image != null)
				{
					image.material = material;
				}
			}
		}
		else
		{
			Image[] array = imgComp;
			foreach (Image image2 in array)
			{
				if (image2 != null)
				{
					image2.material = new Material(image2.material);
				}
			}
		}
		fgFx.material = new Material(fgFx.material);
		if (!_useLowEffect)
		{
			for (int l = 0; l < imgCount; l++)
			{
				if (imgComp[l] != null)
				{
					imgComp[l].material.SetVector(_posAndBounds, new Vector4(canvasPosition.x, canvasPosition.y, cardBounds.x, cardBounds.y));
				}
			}
		}
		RestoreCard();
	}

	public bool HasEffect(FXTask effect)
	{
		return toggledEffects.Contains(effect);
	}

	public void ToggleEffect(bool active, FXTask effect, bool playOnDisabled = false)
	{
		Initialize();
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
		if (affectHeader)
		{
			txtAffected[0] = true;
			txtAffected[5] = true;
			imgAffected[2] = true;
		}
		if (affectTop)
		{
			txtAffected[1] = true;
			imgAffected[3] = true;
		}
		if (affectBottom)
		{
			txtAffected[2] = true;
			imgAffected[4] = true;
		}
		if (affectTopDefault)
		{
			txtAffected[3] = true;
			imgAffected[5] = true;
			imgAffected[7] = true;
		}
		if (affectBottomDefault)
		{
			txtAffected[4] = true;
			imgAffected[6] = true;
			imgAffected[8] = true;
		}
		ToggleAdditiveEffect(active, effect, playOnDisabled);
	}

	public void ToggleAdditiveEffect(bool active, FXTask effect, bool playOnDisabled)
	{
		if (coroutine != null && Choreographer.s_Choreographer != null)
		{
			Choreographer.s_Choreographer.StopCoroutine(coroutine);
		}
		coroutine = null;
		if (active)
		{
			if (!toggledEffects.Contains(effect))
			{
				toggledEffects.Add(effect);
			}
			switch (effect)
			{
			case FXTask.DiscardMode:
				GhostOutOn(ghostAnim: true);
				break;
			case FXTask.LostMode:
				BurnCard(burnAnim: true, playOnDisabled);
				break;
			case FXTask.BurnCard:
				BurnCard(burnAnim: true, playOnDisabled);
				break;
			}
		}
		else
		{
			toggledEffects.Remove(effect);
			switch (effect)
			{
			case FXTask.DiscardMode:
				GhostOutOn(ghostAnim: false);
				break;
			case FXTask.LostMode:
				BurnCard(burnAnim: false, playOnDisabled);
				break;
			case FXTask.BurnCard:
				BurnCard(burnAnim: false, playOnDisabled);
				break;
			}
		}
	}

	private void BurnCard(bool burnAnim, bool playOnDisabled = false)
	{
		coroutine = Choreographer.s_Choreographer.StartCoroutine(BurnCardTimeline(burnAnim, playOnDisabled));
	}

	private void HighlightBurnOn()
	{
	}

	private void HighlightBurnOff()
	{
	}

	private void GhostOutOn(bool ghostAnim)
	{
		coroutine = Choreographer.s_Choreographer.StartCoroutine(GhostOutOnTimeline(ghostAnim));
	}

	public void RestoreCard()
	{
		toggledEffects.Clear();
		if (coroutine != null && Choreographer.s_Choreographer != null)
		{
			Choreographer.s_Choreographer.StopCoroutine(coroutine);
		}
		coroutine = null;
		for (int i = 0; i < imgCount; i++)
		{
			if (imgComp[i] != null)
			{
				imgComp[i].material.SetFloat(_greyOut, 0f);
				if (!_useLowEffect)
				{
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
		for (int j = 0; j < imgCount; j++)
		{
			if (imgComp[j] != null)
			{
				imgComp[j].GetComponent<Image>().color = imgColourStore[j];
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

	public IEnumerator BurnCardTimeline(bool burnAnim, bool playOnDisabled)
	{
		if (!base.gameObject.activeInHierarchy && !playOnDisabled)
		{
			coroutine = null;
			yield break;
		}
		float burnTime = 2f;
		float dTime = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		mc_Burn = 0.691f;
		mc_Burn_ColourTint = new Color(0.36862746f, 0.14509805f, 0.07450981f, 0.601f);
		mc_Flow = 0.3f;
		mc_Flow_Offset = 0.03f;
		mc_Flow_Speed = 0.4f;
		mc_AnimNoise_Mask_tile = 40f;
		mc_Dissolve = 0.646f;
		mc_Dissolve_VerticalGradient = 0.2f;
		mc_GreyOut = 1f;
		fx_Smoke_color = new Color(0.14901961f, 0.14901961f, 0.14901961f, 0.5f);
		fx_Overlay_color = new Color(1f, 0.3f, 0f, 0.8f);
		fx_Overlay_anim = 0.5f;
		fx_Overlay_texture = overlayFrameBurn;
		fx_Overlay_FlowSpeed = 0.5f;
		fx_Overlay_OffsetStrength = 0.2f;
		fx_Overlay_ThinHighlights = 0.463f;
		fx_Overlay_ThinHighlight_Min = 0f;
		fx_Overlay_ThinHighlight_Max = 0.195f;
		fx_Overlay_Flow_NoiseTiling = 6f;
		fx_Overlay_Glow = 3f;
		if (!_useLowEffect)
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
		if (burnAnim)
		{
			while (Timekeeper.instance.m_GlobalClock.time - startTime < burnTime)
			{
				dTime += Timekeeper.instance.m_GlobalClock.deltaTime / burnTime;
				for (int j = 0; j < imgCount; j++)
				{
					if (imgComp[j] != null)
					{
						imgComp[j].material.SetFloat(_greyOut, Mathf.Clamp01(dTime));
						if (!_useLowEffect)
						{
							imgComp[j].material.SetFloat(_flow, Mathf.Clamp01(dTime));
							imgComp[j].material.SetFloat(_dissolve, Mathf.Lerp(0f, mc_Dissolve, Mathf.Clamp01(dTime)));
						}
					}
				}
				if (fgFx != null)
				{
					fgFx.material.SetFloat(fxAnim, Mathf.Clamp(dTime * 4f, 0f, 1f) / 2f);
				}
				SpawnParticle();
				for (int k = 0; k < txtCount; k++)
				{
					if (txtAffected[k] && txtComp[k] != null)
					{
						txtComp[k].color = ((txtComp[k] == initiativeTxt || txtComp[k] == headerTxt) ? burntTextColor : new Color(0.5f, 0.5f, 0.5f, 1f));
					}
				}
				yield return new WaitForEndOfFrame();
			}
		}
		else
		{
			for (int l = 0; l < imgCount; l++)
			{
				if (imgComp[l] != null)
				{
					imgComp[l].material.SetFloat(_greyOut, 1f);
					if (!_useLowEffect)
					{
						imgComp[l].material.SetFloat(_flow, 1f);
						imgComp[l].material.SetFloat(_dissolve, mc_Dissolve);
					}
				}
			}
			fgFx.material.SetFloat(fxAnim, 0.5f);
			SpawnParticle();
			for (int m = 0; m < txtCount; m++)
			{
				if (txtAffected[m] && txtComp[m] != null)
				{
					txtComp[m].color = new Color(0.5f, 0.5f, 0.5f, 1f);
				}
			}
		}
		coroutine = null;
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
		mc_Flow = 0.3f;
		mc_Flow_Offset = 0.03f;
		mc_Flow_Speed = 0.2f;
		mc_AnimNoise_Mask_tile = 10f;
		mc_Dissolve = 0.646f;
		mc_Dissolve_VerticalGradient = 0.2f;
		mc_GreyOut = 1f;
		fx_Smoke_color = new Color(0.3019608f, 0.3019608f, 33f / 85f, 0.4f);
		fx_Overlay_color = new Color(0.8f, 0.8f, 1f, 0.6f);
		fx_Overlay_anim = 0.5f;
		fx_Overlay_texture = overlayFrameGhost;
		fx_Overlay_FlowSpeed = 0.2f;
		fx_Overlay_OffsetStrength = 0.8f;
		fx_Overlay_ThinHighlights = 0.125f;
		fx_Overlay_ThinHighlight_Min = 0.77f;
		fx_Overlay_ThinHighlight_Max = 0.9f;
		fx_Overlay_Flow_NoiseTiling = 2f;
		fx_Overlay_Glow = 0f;
		if (!_useLowEffect)
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
		if (ghostAnim)
		{
			SpawnParticle();
			while (Timekeeper.instance.m_GlobalClock.time - startTime < animTime)
			{
				dTime += Timekeeper.instance.m_GlobalClock.deltaTime / animTime;
				for (int j = 0; j < imgCount; j++)
				{
					if (imgComp[j] != null)
					{
						imgComp[j].material.SetFloat(_greyOut, Mathf.Clamp01(dTime));
						if (!_useLowEffect)
						{
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
			for (int l = 0; l < imgCount; l++)
			{
				if (imgComp[l] != null)
				{
					imgComp[l].material.SetFloat(_greyOut, 1f);
					if (!_useLowEffect)
					{
						imgComp[l].material.SetFloat(_flow, 1f);
						imgComp[l].material.SetFloat(_dissolve, mc_Dissolve);
					}
				}
			}
			SpawnParticle();
			fgFx.material.SetFloat(fxAnim, 0.5f);
			for (int m = 0; m < txtCount; m++)
			{
				if (txtAffected[m] && txtComp[m] != null)
				{
					txtComp[m].color = Color.Lerp(txtColourStore[m], greyOutCol, 1f);
					txtComp[m].enableVertexGradient = false;
				}
			}
		}
		yield return new WaitForEndOfFrame();
		coroutine = null;
	}

	private void SpawnParticle()
	{
		if (!PlatformLayer.Setting.LowParticlesUse.NoCardsParticles && base.gameObject.activeInHierarchy)
		{
			if (_smokeEffect == null)
			{
				GameObject gameObject = ObjectPool.Spawn(GlobalSettings.Instance.VisualEffects.CardSmoke, base.transform);
				_smokeEffect = gameObject.GetComponent<ParticleSystem>();
			}
			if (_smokeEffect != null)
			{
				Color color = fx_Smoke_color;
				ParticleSystem.MainModule main = _smokeEffect.main;
				main.startColor = color;
			}
		}
	}

	private void HideParticle()
	{
		if (_smokeEffect != null)
		{
			ObjectPool.Recycle(_smokeEffect.gameObject, 0.3f, GlobalSettings.Instance.VisualEffects.CardSmoke);
			_smokeEffect = null;
		}
	}
}
