using UnityEngine;

namespace VolumetricFogAndMist;

[ExecuteInEditMode]
public class VolumetricFogDayCycleManager : MonoBehaviour
{
	[Range(0f, 24f)]
	public float currentTime;

	public Gradient colorOverTime;

	public AnimationCurve densityOverTime;

	private int prevTime;

	private VolumetricFog[] fogs;

	private void OnEnable()
	{
		fogs = Object.FindObjectsOfType<VolumetricFog>();
		if (colorOverTime == null)
		{
			colorOverTime = new Gradient();
			GradientColorKey[] array = new GradientColorKey[2];
			array[0].color = Color.white;
			array[0].time = 0f;
			array[1].color = Color.white;
			array[1].time = 1f;
			colorOverTime.colorKeys = array;
			GradientAlphaKey[] array2 = new GradientAlphaKey[2];
			array2[0].alpha = 1f;
			array2[0].time = 0f;
			array2[1].alpha = 1f;
			array2[1].time = 1f;
			colorOverTime.alphaKeys = array2;
		}
		if (densityOverTime == null)
		{
			densityOverTime = new AnimationCurve();
			densityOverTime.AddKey(0f, 1f);
			densityOverTime.AddKey(24f, 1f);
		}
	}

	private void Update()
	{
		currentTime = GetCurrentTime();
		int num = (int)(currentTime * 60f);
		if (num == prevTime && Application.isPlaying)
		{
			return;
		}
		prevTime = num;
		Color color = colorOverTime.Evaluate(currentTime / 24f);
		float num2 = densityOverTime.Evaluate(currentTime);
		for (int i = 0; i < fogs.Length; i++)
		{
			bool flag = false;
			if (!(fogs[i] == null))
			{
				if (fogs[i].temporaryProperties.color != color)
				{
					fogs[i].color = color;
					flag = true;
				}
				if (fogs[i].temporaryProperties.density != num2)
				{
					fogs[i].density = num2;
					flag = true;
				}
				if (flag)
				{
					fogs[i].UpdateMaterialProperties();
				}
			}
		}
	}

	private float GetCurrentTime()
	{
		return currentTime;
	}
}
