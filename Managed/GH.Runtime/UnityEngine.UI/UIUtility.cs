namespace UnityEngine.UI;

public static class UIUtility
{
	public static void BringToFront(GameObject go, Canvas optionalCanvas = null, bool worldPositionStays = true)
	{
		if (optionalCanvas != null)
		{
			go.transform.SetParent(optionalCanvas.transform, worldPositionStays);
		}
		else
		{
			CanvasGroup canvasGroup = FindInParents<CanvasGroup>(go);
			if (canvasGroup != null)
			{
				go.transform.SetParent(canvasGroup.transform, worldPositionStays);
			}
		}
		go.transform.SetAsLastSibling();
	}

	public static void BringToFront(GameObject go, Transform optionalParent, bool worldPositionStays = true)
	{
		if (optionalParent != null)
		{
			go.transform.SetParent(optionalParent, worldPositionStays);
		}
		else
		{
			CanvasGroup canvasGroup = FindInParents<CanvasGroup>(go);
			if (canvasGroup != null)
			{
				go.transform.SetParent(canvasGroup.transform, worldPositionStays);
			}
		}
		go.transform.SetAsLastSibling();
	}

	public static T FindInParents<T>(GameObject go) where T : Component
	{
		if (go == null)
		{
			return null;
		}
		T component = go.GetComponent<T>();
		if (component != null)
		{
			return component;
		}
		Transform parent = go.transform.parent;
		while (parent != null && component == null)
		{
			component = parent.gameObject.GetComponent<T>();
			parent = parent.parent;
		}
		return component;
	}
}
