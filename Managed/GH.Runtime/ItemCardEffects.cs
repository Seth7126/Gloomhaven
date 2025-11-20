#define ENABLE_LOGS
using System.Collections;
using Chronos;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCardEffects : MonoBehaviour
{
	public enum FXTask
	{
		Consumed,
		Spent
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

	public Image[] imgComp;

	public TextMeshProUGUI[] txtComp;

	private Color[] txtColourStore;

	private bool[] txtGradientStore;

	private Color[] imgColourStore;

	private int txtCount;

	private int imgCount;

	[SerializeField]
	private Image fgFx;

	private string fxAnim = "_FXAnim";

	public FXTask task;

	private bool isBurning;

	private bool isGhosting;

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

	[SerializeField]
	private ParticleSystem fx_Smoke;

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

	private void Awake()
	{
		Initialize();
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
		cardBounds = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
		Image[] array = imgComp;
		foreach (Image image in array)
		{
			if (image != null)
			{
				image.material = new Material(image.material);
			}
		}
		fgFx.material = new Material(fgFx.material);
		for (int l = 0; l < imgCount; l++)
		{
			if (imgComp[l] != null)
			{
				imgComp[l].material.SetVector(_posAndBounds, new Vector4(rectTransform.position.x, rectTransform.position.y, cardBounds.x, cardBounds.y));
			}
		}
		RestoreCard();
	}

	public void ToggleEffect(bool active, FXTask effect)
	{
		RestoreCard();
		ToggleAdditiveEffect(active, effect);
	}

	public void ToggleAdditiveEffect(bool active, FXTask effect)
	{
		if (active)
		{
			switch (effect)
			{
			case FXTask.Spent:
				GhostOutOn(ghostAnim: true);
				break;
			case FXTask.Consumed:
				BurnCard(burnAnim: true);
				break;
			}
		}
		else
		{
			switch (effect)
			{
			case FXTask.Spent:
				GhostOutOn(ghostAnim: false);
				break;
			case FXTask.Consumed:
				BurnCard(burnAnim: false);
				break;
			}
		}
	}

	private void BurnCard(bool burnAnim)
	{
		if (Choreographer.s_Choreographer != null)
		{
			Choreographer.s_Choreographer.StartCoroutine(BurnCardTimeline(burnAnim));
		}
		else
		{
			StartCoroutine(BurnCardTimeline(burnAnim));
		}
	}

	private void GhostOutOn(bool ghostAnim)
	{
		if (Choreographer.s_Choreographer != null)
		{
			Choreographer.s_Choreographer.StartCoroutine(GhostOutOnTimeline(ghostAnim));
		}
		else
		{
			StartCoroutine(GhostOutOnTimeline(ghostAnim));
		}
	}

	public void RestoreCard()
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
		if (fx_Smoke != null)
		{
			fx_Smoke.gameObject.SetActive(value: false);
		}
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
			if (txtComp[k] != null)
			{
				txtComp[k].color = txtColourStore[k];
			}
		}
	}

	public IEnumerator BurnCardTimeline(bool burnAnim)
	{
		if (fgFx == null)
		{
			yield break;
		}
		fgFx.gameObject.SetActive(value: true);
		float dTime = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		float burnTime = 0.001f;
		mc_Burn = 0.691f;
		mc_Burn_ColourTint = new Color(0.36862746f, 0.14509805f, 0.07450981f, 0.601f);
		mc_Flow = 1f;
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
						imgComp[j].material.SetFloat(_flow, Mathf.Clamp01(dTime));
						imgComp[j].material.SetFloat(_dissolve, Mathf.Lerp(0f, mc_Dissolve, Mathf.Clamp01(dTime)));
					}
				}
				if (fgFx != null)
				{
					fgFx.material.SetFloat(fxAnim, Mathf.Clamp(dTime * 4f, 0f, 1f) / 2f);
				}
				if (fx_Smoke != null)
				{
					fx_Smoke.gameObject.SetActive(value: true);
				}
				for (int k = 0; k < txtCount; k++)
				{
					if (txtComp[k] != null)
					{
						txtComp[k].color = new Color(0.5f, 0.5f, 0.5f, 1f);
					}
				}
				yield return new WaitForEndOfFrame();
			}
			yield break;
		}
		for (int l = 0; l < imgCount; l++)
		{
			if (imgComp[l] != null)
			{
				imgComp[l].material.SetFloat(_greyOut, 1f);
				imgComp[l].material.SetFloat(_flow, 1f);
				imgComp[l].material.SetFloat(_dissolve, mc_Dissolve);
			}
		}
		fgFx.material.SetFloat(fxAnim, 0.5f);
		for (int m = 0; m < txtCount; m++)
		{
			if (txtComp[m] != null)
			{
				txtComp[m].color = new Color(0.5f, 0.5f, 0.5f, 1f);
			}
		}
	}

	public IEnumerator GhostOutOnTimeline(bool ghostAnim)
	{
		float animTime = 0.001f;
		isGhosting = true;
		if (fgFx != null)
		{
			fgFx.gameObject.SetActive(value: true);
		}
		float dTime = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		Color greyOutCol = new Color(1f, 1f, 1f, 1f);
		mc_Burn = 0.7f;
		mc_Burn_ColourTint = new Color(0.24313726f, 24f / 85f, 29f / 85f, 0.5f);
		mc_Flow = 1f;
		mc_Flow_Offset = 0.03f;
		mc_Flow_Speed = 0.2f;
		mc_AnimNoise_Mask_tile = 10f;
		mc_Dissolve = 0.646f;
		mc_Dissolve_VerticalGradient = 0.2f;
		mc_GreyOut = 1f;
		fx_Smoke_color = new Color(0.3019608f, 0.3019608f, 0.3019608f, 0.4f);
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
		fgFx.material.SetColor(_tintColor, fx_Overlay_color);
		fgFx.material.SetTexture(_particleTexture, fx_Overlay_texture);
		fgFx.material.SetFloat(_speed, fx_Overlay_FlowSpeed);
		fgFx.material.SetFloat(_offsetStrength, fx_Overlay_OffsetStrength);
		fgFx.material.SetFloat(_thinHighlights, fx_Overlay_ThinHighlights);
		fgFx.material.SetFloat(_thinHighlightMin, fx_Overlay_ThinHighlight_Min);
		fgFx.material.SetFloat(_thinHighlightMax, fx_Overlay_ThinHighlight_Max);
		fgFx.material.SetFloat(_flowNoiseTiling, fx_Overlay_Flow_NoiseTiling);
		if (ghostAnim)
		{
			if (fgFx != null && fx_Smoke != null)
			{
				fx_Smoke.gameObject.SetActive(value: true);
			}
			while (Timekeeper.instance.m_GlobalClock.time - startTime < animTime)
			{
				dTime += Timekeeper.instance.m_GlobalClock.deltaTime / animTime;
				for (int j = 0; j < imgCount; j++)
				{
					if (imgComp[j] != null)
					{
						imgComp[j].material.SetFloat(_greyOut, Mathf.Clamp01(dTime));
						imgComp[j].material.SetFloat(_flow, Mathf.Clamp01(dTime));
						imgComp[j].material.SetFloat(_dissolve, Mathf.Lerp(0f, mc_Dissolve, Mathf.Clamp01(dTime)));
					}
				}
				if (fgFx != null)
				{
					fgFx.material.SetFloat(fxAnim, Mathf.Clamp(dTime * 4f, 0f, 1f) / 2f);
				}
				for (int k = 0; k < txtCount; k++)
				{
					if (txtComp[k] != null)
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
					imgComp[l].material.SetFloat(_flow, 1f);
					imgComp[l].material.SetFloat(_dissolve, mc_Dissolve);
				}
			}
			Debug.Log("right");
			if (fx_Smoke != null)
			{
				fx_Smoke.gameObject.SetActive(value: true);
			}
			fgFx.material.SetFloat(fxAnim, 0.5f);
			for (int m = 0; m < txtCount; m++)
			{
				if (txtComp[m] != null)
				{
					txtComp[m].color = Color.Lerp(txtColourStore[m], greyOutCol, 1f);
					txtComp[m].enableVertexGradient = false;
				}
			}
		}
		yield return new WaitForEndOfFrame();
	}

	public void PreviewRefresh()
	{
		mc_GreyOut = 0f;
		for (int i = 0; i < imgCount; i++)
		{
			if (imgComp[i] != null)
			{
				imgComp[i].material.SetFloat(_greyOut, mc_GreyOut);
			}
		}
	}
}
