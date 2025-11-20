using SM.Gamepad;
using Script.GUI.SMNavigation;
using TMPro;
using UnityEngine;

namespace GLOOM.MainMenu;

public class ConfirmNameBlock : MonoBehaviour
{
	private const string LocalizationPrefix = "Consoles/";

	[SerializeField]
	private LongConfirmHandler _longConfirmButton;

	[SerializeField]
	private GameObject _selectNameInfoBlock;

	[SerializeField]
	private Hotkey _longConfirmHotKey;

	[SerializeField]
	private string _confirmNameTranslationKey;

	[SerializeField]
	private TextMeshProUGUI _confirmNameText;

	private CanvasGroup _longConfirmButtonCanvasGroup;

	public LongConfirmHandler LongConfirmButton => _longConfirmButton;

	private void Awake()
	{
		_longConfirmButtonCanvasGroup = _longConfirmButton.GetComponent<CanvasGroup>();
	}

	protected void OnDestroy()
	{
		DeinitHotKeys();
	}

	public void Initialize()
	{
		InitHotKeys();
		DeactivateConfirmButton();
		InitializeConfirmNameText();
	}

	public void Deinitialize()
	{
		DeinitHotKeys();
	}

	public void ActivateConfirmButton()
	{
		_longConfirmButtonCanvasGroup.alpha = 1f;
		_longConfirmButton.enabled = true;
		_selectNameInfoBlock.gameObject.SetActive(value: false);
	}

	public void DeactivateConfirmButton()
	{
		_longConfirmButtonCanvasGroup.alpha = 0f;
		_longConfirmButton.enabled = false;
		_selectNameInfoBlock.gameObject.SetActive(value: true);
	}

	private void InitializeConfirmNameText()
	{
		string translation = LocalizationManager.GetTranslation("Consoles/" + _confirmNameTranslationKey);
		_confirmNameText.text = translation;
	}

	private void InitHotKeys()
	{
		_longConfirmHotKey.Initialize(Singleton<UINavigation>.Instance.Input);
	}

	private void DeinitHotKeys()
	{
		_longConfirmHotKey.Deinitialize();
	}
}
