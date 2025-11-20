using UnityEngine;

public class ExcludeFromReleaseBuild : MonoBehaviour
{
	[SerializeField]
	private bool _excludeInStart;

	private void Awake()
	{
		if (!_excludeInStart)
		{
			Exclude();
		}
	}

	private void Start()
	{
		if (_excludeInStart)
		{
			Exclude();
		}
	}

	private void Exclude()
	{
		base.gameObject.SetActive(value: false);
	}
}
