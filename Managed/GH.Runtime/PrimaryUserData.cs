using Platforms;
using Platforms.Social;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrimaryUserData : MonoBehaviour
{
	[SerializeField]
	private Image _avatarImage;

	[SerializeField]
	private TextMeshProUGUI _usernameText;

	[SerializeField]
	private Hotkey _hotkey;

	private Sprite _defaultAvatar;

	private bool _isInitialised;

	private SimpleKeyActionHandlerBlocker _simpleKeyActionHandlerBlocker;

	private void Awake()
	{
		_defaultAvatar = _avatarImage.sprite;
		_hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		_simpleKeyActionHandlerBlocker = new SimpleKeyActionHandlerBlocker();
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.SWITCH_PROFILE, SwitchProfile).AddBlocker(_simpleKeyActionHandlerBlocker));
	}

	public void Init()
	{
		Platforms.IPlatformUserData currentUser = PlatformLayer.Platform.UserManagement.GetCurrentUser();
		_usernameText.text = currentUser.GetPlatformDisplayName();
		_avatarImage.sprite = _defaultAvatar;
		currentUser.GetAvatar(SetupAvatar);
		_isInitialised = true;
	}

	private void SwitchProfile()
	{
		Singleton<SignOutConfirmationBox>.Instance.Activate("Consoles/GUI_SIGNOUT_CONFIRMATION_TITLE", "Consoles/GUI_SIGNOUT_CONFIRMATION_MESSAGE", delegate
		{
			PlatformLayer.GameProvider.ReturnToInitialInteractiveScreen(isSignInUIRequired: true);
			_simpleKeyActionHandlerBlocker.SetBlock(value: true);
		}, delegate
		{
		});
	}

	private void SetupAvatar(OperationResult result, byte[] buffer)
	{
		if (result != OperationResult.UnspecifiedError)
		{
			Texture2D texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(buffer);
			_avatarImage.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		}
	}

	public void Show()
	{
		if (_isInitialised)
		{
			base.gameObject.SetActive(value: true);
			_simpleKeyActionHandlerBlocker.SetBlock(value: false);
		}
	}

	public void Hide()
	{
		if (_isInitialised)
		{
			_simpleKeyActionHandlerBlocker.SetBlock(value: true);
			base.gameObject.SetActive(value: false);
		}
	}
}
