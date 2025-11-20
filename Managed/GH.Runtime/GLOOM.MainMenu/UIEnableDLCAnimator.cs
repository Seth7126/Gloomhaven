using System.Collections.Generic;
using Assets.Script.Misc;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

public class UIEnableDLCAnimator : MonoBehaviour
{
	[SerializeField]
	private GUIAnimator enableDLCAnimator;

	[SerializeField]
	private TextLocalizedListener titleBanner;

	[SerializeField]
	private List<Image> icons;

	[SerializeField]
	private List<Image> glows;

	private CallbackPromise promise;

	private void Awake()
	{
		enableDLCAnimator.OnAnimationFinished.AddListener(Finish);
	}

	public ICallbackPromise Play(DLCRegistry.EDLCKey dlc)
	{
		string enumCategory = GloomUtility.GetEnumCategory(dlc);
		Sprite dLCGUIAssetFromBundle = PlatformLayer.DLC.GetDLCGUIAssetFromBundle<Sprite>(dlc, "GoldIcon_" + enumCategory, "Content/GUI/DLC Icons/", "png");
		Sprite dLCGUIAssetFromBundle2 = PlatformLayer.DLC.GetDLCGUIAssetFromBundle<Sprite>(dlc, "GoldIcon_" + enumCategory + "_Glow", "Content/GUI/DLC Icons/", "png");
		for (int i = 0; i < glows.Count; i++)
		{
			glows[i].sprite = dLCGUIAssetFromBundle2;
		}
		for (int j = 0; j < icons.Count; j++)
		{
			icons[j].sprite = dLCGUIAssetFromBundle;
		}
		titleBanner.SetTextKey(dlc.ToString());
		enableDLCAnimator.Play();
		base.gameObject.SetActive(value: true);
		promise = new CallbackPromise();
		InputManager.RequestDisableInput(this, EKeyActionTag.All);
		return promise;
	}

	[ContextMenu("Play")]
	private void Play()
	{
		enableDLCAnimator.Play();
		base.gameObject.SetActive(value: true);
	}

	private void Finish()
	{
		promise?.Resolve();
		Stop();
	}

	public void Stop()
	{
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
			enableDLCAnimator.Stop();
			if (promise != null && promise.IsPending)
			{
				promise.Cancel();
			}
		}
	}

	private void OnDisable()
	{
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
		enableDLCAnimator.Stop();
	}
}
