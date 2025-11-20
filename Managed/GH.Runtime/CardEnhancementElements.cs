using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardEnhancementElements : ISerializationCallbackReceiver
{
	[SerializeField]
	private List<EnhancementButtonBase> _all;

	public List<EnhancementButtonBase> All => _all;

	public List<EnhancementButton> Buttons { get; } = new List<EnhancementButton>();

	public List<EnhancedAreaHex> AreaHexes { get; } = new List<EnhancedAreaHex>();

	public void Add(CardEnhancementElements enhancementElements)
	{
		Add(enhancementElements.All);
	}

	public void Add(IEnumerable<EnhancementButtonBase> baseButtons)
	{
		foreach (EnhancementButtonBase baseButton in baseButtons)
		{
			Add(baseButton);
		}
	}

	public void Add(EnhancementButtonBase baseButton)
	{
		_all.Add(baseButton);
		ProcessAdd(baseButton);
	}

	private void ProcessAdd(EnhancementButtonBase baseButton)
	{
		if (!(baseButton is EnhancementButton item))
		{
			if (baseButton is EnhancedAreaHex item2)
			{
				AreaHexes.Add(item2);
			}
		}
		else
		{
			Buttons.Add(item);
		}
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		foreach (EnhancementButtonBase item in _all)
		{
			ProcessAdd(item);
		}
	}
}
