using System;
using UnityEngine;

namespace GLOO.Introduction;

public abstract class UIIntroduce : MonoBehaviour
{
	[ContextMenu("Show")]
	public abstract void Show(Action onFinished = null);

	public abstract void Hide();
}
