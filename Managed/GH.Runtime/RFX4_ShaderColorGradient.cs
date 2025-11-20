using UnityEngine;

public class RFX4_ShaderColorGradient : MonoBehaviour
{
	public RFX4_ShaderProperties ShaderColorProperty;

	public Gradient Color = new Gradient();

	public float TimeMultiplier = 1f;

	public bool IsLoop;

	public bool UseSharedMaterial;

	[HideInInspector]
	public float HUE = -1f;

	[HideInInspector]
	public bool canUpdate;

	private Material mat;

	private int propertyID;

	private float startTime;

	private Color startColor;

	private bool isInitialized;

	private string shaderProperty;

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		shaderProperty = ShaderColorProperty.ToString();
		startTime = Time.time;
		canUpdate = true;
		Renderer component = GetComponent<Renderer>();
		if (component == null)
		{
			Projector component2 = GetComponent<Projector>();
			if (component2 != null)
			{
				if (!component2.material.name.EndsWith("(Instance)"))
				{
					component2.material = new Material(component2.material)
					{
						name = component2.material.name + " (Instance)"
					};
				}
				mat = component2.material;
			}
		}
		else if (!UseSharedMaterial)
		{
			mat = component.material;
		}
		else
		{
			mat = component.sharedMaterial;
		}
		if (mat == null)
		{
			canUpdate = false;
			return;
		}
		if (!mat.HasProperty(shaderProperty))
		{
			canUpdate = false;
			return;
		}
		if (mat.HasProperty(shaderProperty))
		{
			propertyID = Shader.PropertyToID(shaderProperty);
		}
		startColor = mat.GetColor(propertyID);
		Color color = Color.Evaluate(0f);
		mat.SetColor(propertyID, color * startColor);
		isInitialized = true;
	}

	private void OnEnable()
	{
		if (isInitialized)
		{
			startTime = Time.time;
			canUpdate = true;
		}
	}

	private void Update()
	{
		if (mat == null)
		{
			return;
		}
		float num = Time.time - startTime;
		if (canUpdate)
		{
			Color color = Color.Evaluate(num / TimeMultiplier);
			if (HUE > -0.9f)
			{
				color = RFX4_ColorHelper.ConvertRGBColorByHUE(color, HUE);
				startColor = RFX4_ColorHelper.ConvertRGBColorByHUE(startColor, HUE);
			}
			mat.SetColor(propertyID, color * startColor);
		}
		if (num >= TimeMultiplier)
		{
			if (IsLoop)
			{
				startTime = Time.time;
			}
			else
			{
				canUpdate = false;
			}
		}
	}

	private void OnDisable()
	{
		if (!(mat == null))
		{
			if (UseSharedMaterial)
			{
				mat.SetColor(propertyID, startColor);
			}
			mat.SetColor(propertyID, startColor);
		}
	}
}
