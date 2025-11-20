using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SM.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SpriteMemoryManagement;

[Obsolete]
public class ButtonSpritesLoader : MonoBehaviour
{
	private class ButtonSpritesLoaderStruct
	{
		public ReferenceToSprite ReferenceForImage { get; set; }

		public ReferenceToSpriteState ReferencesState { get; set; }

		public Button PlaceForSprites { get; set; }

		public bool IsDiscard { get; set; }

		public bool IsFinishedImage { get; set; }

		public bool IsFinishedHighlighted { get; set; }

		public bool IsFinishedPressed { get; set; }

		public bool IsFinishedSelected { get; set; }

		public bool IsFinishedDisabled { get; set; }

		public bool CheckAllLoaded()
		{
			if (!IsFinishedImage)
			{
				return false;
			}
			if (!IsFinishedHighlighted)
			{
				return false;
			}
			if (!IsFinishedPressed)
			{
				return false;
			}
			if (!IsFinishedSelected)
			{
				return false;
			}
			if (!IsFinishedDisabled)
			{
				return false;
			}
			return true;
		}

		public void AllRelease()
		{
			if (IsFinishedImage)
			{
				ReferenceForImage.Release();
			}
			if (IsFinishedHighlighted)
			{
				ReferencesState.HighlightedSprite.Release();
			}
			if (IsFinishedPressed)
			{
				ReferencesState.DisabledSprite.Release();
			}
			if (IsFinishedSelected)
			{
				ReferencesState.PressedSprite.Release();
			}
			if (IsFinishedDisabled)
			{
				ReferencesState.SelectedSprite.Release();
			}
		}

		public void ReleaseButtonReferences()
		{
			PlaceForSprites.image.sprite = null;
			SpriteState spriteState = PlaceForSprites.spriteState;
			spriteState.highlightedSprite = null;
			spriteState.pressedSprite = null;
			spriteState.selectedSprite = null;
			spriteState.disabledSprite = null;
			PlaceForSprites.spriteState = spriteState;
			PlaceForSprites.spriteState = default(SpriteState);
		}
	}

	[FormerlySerializedAs("circularLoader")]
	[SerializeField]
	private GameObject _circularLoader;

	private Dictionary<Button, ButtonSpritesLoaderStruct> _buttonInQueue = new Dictionary<Button, ButtonSpritesLoaderStruct>();

	[UsedImplicitly]
	private void Awake()
	{
		if (_circularLoader != null)
		{
			_circularLoader.SetActive(value: false);
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Release();
	}

	public void AddReferenceToSpritesForButton(Button button, ReferenceToSprite referenceForImage, ReferenceToSpriteState referencesForState)
	{
		DiscardReferenceForImage(button);
		if (_circularLoader != null)
		{
			_circularLoader.SetActive(value: true);
		}
		ButtonSpritesLoaderStruct item = new ButtonSpritesLoaderStruct
		{
			PlaceForSprites = button,
			ReferenceForImage = referenceForImage,
			ReferencesState = referencesForState
		};
		if (referenceForImage == null || referencesForState.HighlightedSprite == null || referencesForState.DisabledSprite == null || referencesForState.SelectedSprite == null || referencesForState.PressedSprite == null)
		{
			LogUtils.LogError("Lacks sprites for button: " + button.gameObject.name);
			return;
		}
		_buttonInQueue[button] = item;
		button.image.enabled = false;
		referenceForImage.GetAsyncSprite(delegate
		{
			item.IsFinishedImage = true;
			if (OnFinish(item))
			{
				FinishReferenceForImage(item);
			}
		});
		referencesForState.DisabledSprite.GetAsyncSprite(delegate
		{
			item.IsFinishedDisabled = true;
			if (OnFinish(item))
			{
				FinishReferenceForDisabledSprite(item);
			}
		});
		referencesForState.HighlightedSprite.GetAsyncSprite(delegate
		{
			item.IsFinishedHighlighted = true;
			if (OnFinish(item))
			{
				FinishReferenceForHighlightedSprite(item);
			}
		});
		referencesForState.SelectedSprite.GetAsyncSprite(delegate
		{
			item.IsFinishedSelected = true;
			if (OnFinish(item))
			{
				FinishReferenceForSelectedSprite(item);
			}
		});
		referencesForState.PressedSprite.GetAsyncSprite(delegate
		{
			item.IsFinishedPressed = true;
			if (OnFinish(item))
			{
				FinishReferenceForPressedSprite(item);
			}
		});
	}

	public void Release()
	{
		List<Button> list = new List<Button>(_buttonInQueue.Count);
		foreach (KeyValuePair<Button, ButtonSpritesLoaderStruct> item in _buttonInQueue)
		{
			list.Add(item.Key);
		}
		foreach (Button item2 in list)
		{
			DiscardReferenceForImage(item2);
		}
	}

	public void Release(Button button)
	{
		DiscardReferenceForImage(button);
	}

	private bool OnFinish(ButtonSpritesLoaderStruct item)
	{
		if (item.IsDiscard)
		{
			if (item.CheckAllLoaded())
			{
				item.AllRelease();
				item.ReleaseButtonReferences();
			}
			return false;
		}
		if (CheckAllLoaded() && _circularLoader != null)
		{
			_circularLoader.SetActive(value: false);
		}
		if (item.CheckAllLoaded())
		{
			item.PlaceForSprites.image.enabled = true;
		}
		return true;
	}

	private void FinishReferenceForImage(ButtonSpritesLoaderStruct item)
	{
		item.PlaceForSprites.image.sprite = item.ReferenceForImage.GetSprite();
	}

	private void FinishReferenceForHighlightedSprite(ButtonSpritesLoaderStruct item)
	{
		SpriteState spriteState = item.PlaceForSprites.spriteState;
		spriteState.highlightedSprite = item.ReferencesState.HighlightedSprite.GetSprite();
		item.PlaceForSprites.spriteState = spriteState;
	}

	private void FinishReferenceForPressedSprite(ButtonSpritesLoaderStruct item)
	{
		SpriteState spriteState = item.PlaceForSprites.spriteState;
		spriteState.pressedSprite = item.ReferencesState.PressedSprite.GetSprite();
		item.PlaceForSprites.spriteState = spriteState;
	}

	private void FinishReferenceForSelectedSprite(ButtonSpritesLoaderStruct item)
	{
		SpriteState spriteState = item.PlaceForSprites.spriteState;
		spriteState.selectedSprite = item.ReferencesState.SelectedSprite.GetSprite();
		item.PlaceForSprites.spriteState = spriteState;
	}

	private void FinishReferenceForDisabledSprite(ButtonSpritesLoaderStruct item)
	{
		SpriteState spriteState = item.PlaceForSprites.spriteState;
		spriteState.disabledSprite = item.ReferencesState.DisabledSprite.GetSprite();
		item.PlaceForSprites.spriteState = spriteState;
	}

	private void DiscardReferenceForImage(Button button)
	{
		if (_buttonInQueue.TryGetValue(button, out var value))
		{
			button.image.enabled = true;
			if (value.CheckAllLoaded())
			{
				value.AllRelease();
				value.ReleaseButtonReferences();
			}
			else
			{
				value.IsDiscard = true;
			}
			_buttonInQueue.Remove(button);
		}
	}

	private bool CheckAllLoaded()
	{
		foreach (KeyValuePair<Button, ButtonSpritesLoaderStruct> item in _buttonInQueue)
		{
			if (!item.Value.CheckAllLoaded())
			{
				return false;
			}
		}
		return true;
	}
}
