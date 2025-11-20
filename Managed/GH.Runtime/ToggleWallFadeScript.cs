using UnityEngine;

public class ToggleWallFadeScript : MonoBehaviour
{
	private static readonly int _toggleWallFadeLocal = Shader.PropertyToID("_ToggleWallFadeLocal");

	private Renderer[] rend;

	public bool ToggleWallFadeLocal;

	private void Start()
	{
		rend = base.gameObject.GetComponentsInChildren<Renderer>();
		if (ToggleWallFadeLocal)
		{
			Renderer[] array = rend;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].material.SetInt(_toggleWallFadeLocal, 0);
			}
		}
		if (!ToggleWallFadeLocal)
		{
			Renderer[] array = rend;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].material.SetInt(_toggleWallFadeLocal, 1);
			}
		}
	}

	private void Update()
	{
	}
}
