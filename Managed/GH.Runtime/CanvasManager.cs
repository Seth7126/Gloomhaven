using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Canvas))]
public class CanvasManager : MonoBehaviour
{
	[SerializeField]
	private Canvas persistentUICanvas;

	[SerializeField]
	private Canvas tooltipCanvas;

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		Camera[] allCameras = Camera.allCameras;
		for (int i = 0; i < allCameras.Length; i++)
		{
			if (allCameras[i].tag.Equals("UICamera"))
			{
				Canvas canvas = persistentUICanvas;
				Camera worldCamera = (tooltipCanvas.worldCamera = allCameras[i]);
				canvas.worldCamera = worldCamera;
				break;
			}
		}
	}
}
