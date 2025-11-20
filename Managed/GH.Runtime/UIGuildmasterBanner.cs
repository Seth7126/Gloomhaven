using System.Text;
using Assets.Script.GUI.NewAdventureMode.Guildmaster;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildmasterBanner : MonoBehaviour
{
	[SerializeField]
	private Image shieldIcon;

	[SerializeField]
	private TextLocalizedListener nameText;

	[SerializeField]
	private RectTransform _headerContainer;

	[CanBeNull]
	private IGuildmasterBannerConfig _config;

	private float? _defaultContainerHeight;

	private TextAlignmentOptions? _defaultTextAlignmentOptions;

	private void OnDestroy()
	{
		if (nameText != null)
		{
			nameText.EventLanguageChanged.RemoveListener(OnTextChanged);
		}
	}

	public void ShowMode(EGuildmasterMode mode, IGuildmasterBannerConfig config = null)
	{
		_config = config;
		nameText.EventLanguageChanged.RemoveAllListeners();
		nameText.EventLanguageChanged.AddListener(OnTextChanged);
		if (!_defaultContainerHeight.HasValue && _headerContainer != null)
		{
			_defaultContainerHeight = _headerContainer.sizeDelta.y;
		}
		IGuildmasterBannerConfig config2;
		if (_headerContainer != null && _defaultContainerHeight.HasValue)
		{
			RectTransform headerContainer = _headerContainer;
			float x = _headerContainer.sizeDelta.x;
			config2 = _config;
			headerContainer.sizeDelta = new Vector2(x, (config2 != null && config2.ContainerHeight > 0f) ? _config.ContainerHeight : _defaultContainerHeight.Value);
		}
		TextAlignmentOptions valueOrDefault = _defaultTextAlignmentOptions.GetValueOrDefault();
		if (!_defaultTextAlignmentOptions.HasValue)
		{
			valueOrDefault = nameText.Text.alignment;
			_defaultTextAlignmentOptions = valueOrDefault;
		}
		TextMeshProUGUI text = nameText.Text;
		config2 = _config;
		text.alignment = ((config2 != null && config2.CustomTextAlignment) ? _config.TextAlignmentOptions : _defaultTextAlignmentOptions.Value);
		nameText.SetTextKey($"GUI_GUILD_{mode}");
		if (shieldIcon != null)
		{
			shieldIcon.sprite = UIInfoTools.Instance.GetGuildmasterModeSprite(mode);
		}
		Show();
	}

	private void OnTextChanged()
	{
		IGuildmasterBannerConfig config = _config;
		if (config != null && config.InsertNewLineCharacter)
		{
			string text = nameText.Text.text;
			StringBuilder stringBuilder = new StringBuilder(text);
			int index = text.IndexOf(' ', text.IndexOf(' ') + 1);
			stringBuilder[index] = '\n';
			nameText.Text.text = stringBuilder.ToString();
		}
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		nameText.EventLanguageChanged.RemoveListener(OnTextChanged);
		base.gameObject.SetActive(value: false);
	}
}
