using Gloomhaven;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class InitialiseAntialiasTechnique : MonoBehaviour
{
	public PostProcessLayer m_Antialiasing;

	private void Update()
	{
		if (m_Antialiasing.antialiasingMode != (PostProcessLayer.Antialiasing)GraphicSettings.s_Antialiasing)
		{
			m_Antialiasing.antialiasingMode = (PostProcessLayer.Antialiasing)GraphicSettings.s_Antialiasing;
		}
	}
}
