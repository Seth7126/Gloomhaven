using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICreateModView : MonoBehaviour
{
	[Serializable]
	public class ModEvent : UnityEvent<ModDataView>
	{
	}

	private class ModOptionData : TMP_Dropdown.OptionData
	{
		public GHModMetaData.EModType ModType { get; private set; }

		public ModOptionData(GHModMetaData.EModType type)
			: base(LocalizationManager.GetTranslation("GUI_MOD_" + type.ToString().ToUpper() + "_TYPE"))
		{
			ModType = type;
		}
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private TMP_Dropdown typeDropdown;

	[SerializeField]
	private GameObject formContainer;

	[SerializeField]
	private TMP_InputField nameInput;

	[SerializeField]
	private TMP_InputField descriptionInput;

	[SerializeField]
	private RawImage thumbnailIcon;

	[SerializeField]
	private Button selectThumbnailButton;

	[SerializeField]
	private ExtendedButton confirmModButton;

	[SerializeField]
	private FileSelector fileSelector;

	public ModEvent OnConfirmedModEvent = new ModEvent();

	private List<ModOptionData> options;

	public UnityEvent OnHidden => window.onHidden;

	private void Awake()
	{
		nameInput.contentType = TMP_InputField.ContentType.Alphanumeric;
		nameInput.onValueChanged.AddListener(delegate
		{
			Validate();
		});
		confirmModButton.onClick.AddListener(Confirm);
		selectThumbnailButton.onClick.AddListener(SelectThumbnail);
		options = new List<ModOptionData>
		{
			new ModOptionData(GHModMetaData.EModType.Campaign),
			new ModOptionData(GHModMetaData.EModType.Guildmaster),
			new ModOptionData(GHModMetaData.EModType.Language),
			new ModOptionData(GHModMetaData.EModType.CustomLevels),
			new ModOptionData(GHModMetaData.EModType.Global)
		};
		typeDropdown.AddOptions(options.Cast<TMP_Dropdown.OptionData>().ToList());
		typeDropdown.onValueChanged.AddListener(OnSelectedModType);
	}

	private void OnDestroy()
	{
		nameInput.onValueChanged.RemoveAllListeners();
		confirmModButton.onClick.RemoveAllListeners();
		selectThumbnailButton.onClick.RemoveAllListeners();
		typeDropdown.onValueChanged.RemoveAllListeners();
	}

	private void SelectThumbnail()
	{
		StartCoroutine(SelectThumbnailCoroutine());
	}

	private IEnumerator SelectThumbnailCoroutine()
	{
		fileSelector.result = null;
		yield return fileSelector.Select(fileSelector.defaultPath);
		if (fileSelector.result != null)
		{
			try
			{
				Texture2D texture2D = new Texture2D(2, 2);
				texture2D.LoadImage(File.ReadAllBytes(fileSelector.result));
				thumbnailIcon.texture = texture2D;
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception while trying to load image file " + fileSelector.result + "\n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
	}

	private void Validate()
	{
		confirmModButton.interactable = IsValid();
		confirmModButton.buttonText.CrossFadeColor(confirmModButton.interactable ? UIInfoTools.Instance.White : UIInfoTools.Instance.greyedOutTextColor, 0f, ignoreTimeScale: true, useAlpha: true);
	}

	private bool IsValid()
	{
		if (nameInput.text.Length == 0 || typeDropdown.value < 0)
		{
			return false;
		}
		return true;
	}

	public void Show()
	{
		formContainer.SetActive(value: false);
		TMP_InputField tMP_InputField = nameInput;
		string text = (descriptionInput.text = string.Empty);
		tMP_InputField.text = text;
		thumbnailIcon.texture = UIInfoTools.Instance.defaultModThumbnail;
		typeDropdown.SetValueWithoutNotify(-1);
		Validate();
		window.Show();
	}

	public void Hide()
	{
		StopAllCoroutines();
		window.Hide();
	}

	private void Confirm()
	{
		GHModMetaData.EModType modType = options[typeDropdown.value].ModType;
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_CREATE_MOD_CONFIRMATION"), null, delegate
		{
			OnConfirmedModEvent.Invoke(new ModDataView(nameInput.text, descriptionInput.text, modType, thumbnailIcon.mainTexture as Texture2D));
		});
	}

	private void OnSelectedModType(int option)
	{
		formContainer.SetActive(option >= 0);
		Validate();
	}
}
