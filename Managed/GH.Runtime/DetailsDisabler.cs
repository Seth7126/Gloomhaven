using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class DetailsDisabler : MonoBehaviour
{
	private List<IDetailDisablerProvider> _disablerProviders;

	private void Awake()
	{
		_disablerProviders = new List<IDetailDisablerProvider>();
		GetComponents(_disablerProviders);
	}

	private void Start()
	{
		StartDisable();
	}

	private void StartDisable()
	{
		_disablerProviders.ForEach(delegate(IDetailDisablerProvider x)
		{
			x.StartDisable();
		});
	}
}
