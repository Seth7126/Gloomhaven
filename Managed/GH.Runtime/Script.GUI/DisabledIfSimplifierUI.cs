using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI;

public class DisabledIfSimplifierUI : MonoBehaviour
{
	[SerializeField]
	[UsedImplicitly]
	private bool _disableGraphicsOnly;

	[UsedImplicitly]
	private void Awake()
	{
		if (global::PlatformLayer.Setting.SimplifiedUI)
		{
			if (_disableGraphicsOnly)
			{
				GetComponent<Graphic>().enabled = false;
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
