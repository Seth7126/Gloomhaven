using UnityEngine;
using UnityEngine.UI;

public class ExtendedScrollRect : ScrollRect
{
	public bool useGlobalSettings = true;

	[ConditionalField("useGlobalSettings", "false", true)]
	[SerializeField]
	private ScrollRectSettings customSettings;

	protected override void OnEnable()
	{
		LoadOverrideValues();
		base.OnEnable();
	}

	protected void LoadOverrideValues()
	{
		ScrollRectSettings scrollRectSettings = null;
		if (useGlobalSettings && UIInfoTools.Instance != null)
		{
			scrollRectSettings = UIInfoTools.Instance.scrollRectConfig;
		}
		else if (customSettings != null)
		{
			scrollRectSettings = customSettings;
		}
		if (scrollRectSettings != null)
		{
			base.scrollSensitivity = scrollRectSettings.ScrollSensitivity;
			base.decelerationRate = scrollRectSettings.DecelerationRate;
			base.movementType = scrollRectSettings.MovementType;
			base.inertia = scrollRectSettings.Inertia;
			base.elasticity = scrollRectSettings.Elasticity;
		}
	}

	public void ScrollToTop()
	{
		base.verticalNormalizedPosition = 1f;
	}
}
