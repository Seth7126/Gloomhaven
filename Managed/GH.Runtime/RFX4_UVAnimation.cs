using UnityEngine;

public class RFX4_UVAnimation : MonoBehaviour
{
	public int TilesX = 4;

	public int TilesY = 4;

	public int FPS = 30;

	public int StartFrameOffset;

	public bool IsLoop = true;

	public float StartDelay;

	public bool IsReverse;

	public bool IsInterpolateFrames;

	public RFX4_TextureShaderProperties[] TextureNames = new RFX4_TextureShaderProperties[1];

	public AnimationCurve FrameOverTime = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	private int count;

	private Renderer currentRenderer;

	private Projector projector;

	private Material instanceMaterial;

	private float animationStartTime;

	private bool canUpdate;

	private int previousIndex;

	private int totalFrames;

	private float currentInterpolatedTime;

	private int currentIndex;

	private Vector2 size;

	private bool isInitialized;

	private void OnEnable()
	{
		if (isInitialized)
		{
			InitDefaultVariables();
		}
	}

	private void Start()
	{
		InitDefaultVariables();
		isInitialized = true;
	}

	private void Update()
	{
		if (canUpdate)
		{
			UpdateMaterial();
			SetSpriteAnimation();
			if (IsInterpolateFrames)
			{
				SetSpriteAnimationIterpolated();
			}
		}
	}

	private void InitDefaultVariables()
	{
		InitializeMaterial();
		totalFrames = TilesX * TilesY;
		previousIndex = 0;
		canUpdate = true;
		count = TilesY * TilesX;
		Vector3 zero = Vector3.zero;
		StartFrameOffset -= StartFrameOffset / count * count;
		size = new Vector2(1f / (float)TilesX, 1f / (float)TilesY);
		animationStartTime = Time.time;
		if (instanceMaterial != null)
		{
			RFX4_TextureShaderProperties[] textureNames = TextureNames;
			for (int i = 0; i < textureNames.Length; i++)
			{
				RFX4_TextureShaderProperties rFX4_TextureShaderProperties = textureNames[i];
				instanceMaterial.SetTextureScale(rFX4_TextureShaderProperties.ToString(), size);
				instanceMaterial.SetTextureOffset(rFX4_TextureShaderProperties.ToString(), zero);
			}
		}
	}

	private void InitializeMaterial()
	{
		currentRenderer = GetComponent<Renderer>();
		if (currentRenderer == null)
		{
			projector = GetComponent<Projector>();
			if (projector != null)
			{
				if (!projector.material.name.EndsWith("(Instance)"))
				{
					projector.material = new Material(projector.material)
					{
						name = projector.material.name + " (Instance)"
					};
				}
				instanceMaterial = projector.material;
			}
		}
		else
		{
			instanceMaterial = currentRenderer.material;
		}
	}

	private void UpdateMaterial()
	{
		if (currentRenderer == null)
		{
			if (projector != null)
			{
				if (!projector.material.name.EndsWith("(Instance)"))
				{
					projector.material = new Material(projector.material)
					{
						name = projector.material.name + " (Instance)"
					};
				}
				instanceMaterial = projector.material;
			}
		}
		else
		{
			instanceMaterial = currentRenderer.material;
		}
	}

	private void SetSpriteAnimation()
	{
		int num = (int)((Time.time - animationStartTime) * (float)FPS);
		num %= totalFrames;
		if (!IsLoop && num < previousIndex)
		{
			canUpdate = false;
			return;
		}
		if (IsInterpolateFrames && num != previousIndex)
		{
			currentInterpolatedTime = 0f;
		}
		previousIndex = num;
		if (IsReverse)
		{
			num = totalFrames - num - 1;
		}
		int num2 = num % TilesX;
		int num3 = num / TilesX;
		float x = (float)num2 * size.x;
		float y = 1f - size.y - (float)num3 * size.y;
		Vector2 value = new Vector2(x, y);
		if (instanceMaterial != null)
		{
			RFX4_TextureShaderProperties[] textureNames = TextureNames;
			for (int i = 0; i < textureNames.Length; i++)
			{
				RFX4_TextureShaderProperties rFX4_TextureShaderProperties = textureNames[i];
				instanceMaterial.SetTextureScale(rFX4_TextureShaderProperties.ToString(), size);
				instanceMaterial.SetTextureOffset(rFX4_TextureShaderProperties.ToString(), value);
			}
		}
	}

	private void SetSpriteAnimationIterpolated()
	{
		currentInterpolatedTime += Time.deltaTime;
		int num = previousIndex + 1;
		if (num == totalFrames)
		{
			num = previousIndex;
		}
		if (IsReverse)
		{
			num = totalFrames - num - 1;
		}
		int num2 = num % TilesX;
		int num3 = num / TilesX;
		float x = (float)num2 * size.x;
		float y = 1f - size.y - (float)num3 * size.y;
		Vector2 vector = new Vector2(x, y);
		if (instanceMaterial != null)
		{
			instanceMaterial.SetVector("_Tex_NextFrame", new Vector4(size.x, size.y, vector.x, vector.y));
			instanceMaterial.SetFloat("InterpolationValue", Mathf.Clamp01(currentInterpolatedTime * (float)FPS));
		}
	}
}
