using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Script.GUI;

public class KeyActionBindingCollection : MonoBehaviour
{
	[Serializable]
	private class Binding
	{
		[SerializeField]
		private KeyAction _keyAction;

		[SerializeField]
		private List<LocalizationBinding> _localizationBindings;

		public KeyAction KeyAction => _keyAction;

		public string GetInputDisplayNameByLocalizationKey(string localizationKey)
		{
			if (_localizationBindings.Count <= 0)
			{
				throw new KeyNotFoundException($"Not found input display name for {KeyAction}");
			}
			if (string.IsNullOrEmpty(localizationKey))
			{
				return _localizationBindings[0].InputDisplayName;
			}
			LocalizationBinding localizationBinding = _localizationBindings.FirstOrDefault((LocalizationBinding x) => x.LocalizationKey == localizationKey);
			if (localizationBinding == null)
			{
				return _localizationBindings[0].InputDisplayName;
			}
			return localizationBinding.InputDisplayName;
		}
	}

	[Serializable]
	private class LocalizationBinding
	{
		public string LocalizationKey;

		public string InputDisplayName;
	}

	[SerializeField]
	private List<Binding> _bindings;

	public string GetInputDisplayName(KeyAction keyAction, string localizationKey = null)
	{
		return (_bindings.FirstOrDefault((Binding x) => x.KeyAction == keyAction) ?? throw new KeyNotFoundException($"Not found Input Display Name for {keyAction}")).GetInputDisplayNameByLocalizationKey(localizationKey);
	}
}
