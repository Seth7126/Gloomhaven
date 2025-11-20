using System.Collections.Generic;
using Code.State;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;
using UnityEngine.UI;

public class PromotionDLCManager : Singleton<PromotionDLCManager>
{
	private class PromotionDLCState
	{
		private Sprite[] images;

		private int currentImage;

		public DLCRegistry.EDLCKey DLC { get; }

		public bool HasSeveralImages
		{
			get
			{
				if (!images.IsNullOrEmpty())
				{
					return images.Length > 1;
				}
				return false;
			}
		}

		public PromotionDLCState(DLCRegistry.EDLCKey dlc)
		{
			DLC = dlc;
			images = UIInfoTools.Instance.GetDLCPromotionSprites(dlc);
			currentImage = -1;
		}

		public Sprite GetImage()
		{
			currentImage = (currentImage + 1) % images.Length;
			return images[currentImage];
		}
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private UIPromotionDLCSlot buyDLCPrefab;

	[SerializeField]
	private GUIAnimator imageTransitionAnimator;

	[SerializeField]
	private UIPromotionDLCSlot comingSoonDLCPrefab;

	private List<PromotionDLCState> dlcToShow = new List<PromotionDLCState>();

	private int currentDlc;

	private void Start()
	{
		if (PlatformLayer.Instance.IsDelayedInit)
		{
			RectTransform component = base.transform.GetComponent<RectTransform>();
			component.offsetMax = new Vector2(component.offsetMax.x, -135f);
		}
		window.onHidden.AddListener(imageTransitionAnimator.Stop);
		SetInstance(this);
	}

	public void InitPromotionDlcSlots()
	{
		dlcToShow.Clear();
		RemoveListeners();
		DLCRegistry.EDLCKey[] dLCKeys = DLCRegistry.DLCKeys;
		foreach (DLCRegistry.EDLCKey dlc in dLCKeys)
		{
			if (UIInfoTools.Instance.GetDLCState(dlc) != DLCConfig.EDLCState.Unavailable && !PlatformLayer.DLC.UserOwnsDLC(dlc) && (!PlatformLayer.Instance.IsConsole || !UIInfoTools.Instance.IsDlcHideForPromotion(dlc)))
			{
				dlcToShow.Add(new PromotionDLCState(dlc));
			}
		}
		if (dlcToShow.IsNullOrEmpty())
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			AddListeners();
		}
	}

	private void ShowNextPromotion()
	{
		ShowAvailableDLC(currentDlc + 1);
	}

	private void ShowAvailableDLC(int dlcIndex)
	{
		currentDlc = dlcIndex % dlcToShow.Count;
		PromotionDLCState promotionDLCState = dlcToShow[currentDlc];
		if (UIInfoTools.Instance.GetDLCState(promotionDLCState.DLC) == DLCConfig.EDLCState.Available)
		{
			comingSoonDLCPrefab.gameObject.SetActive(value: false);
			buyDLCPrefab.SetDLC(promotionDLCState.DLC, promotionDLCState.GetImage());
			buyDLCPrefab.gameObject.SetActive(value: true);
		}
		else
		{
			buyDLCPrefab.gameObject.SetActive(value: false);
			comingSoonDLCPrefab.SetDLC(promotionDLCState.DLC, promotionDLCState.GetImage());
			comingSoonDLCPrefab.gameObject.SetActive(value: true);
		}
		if (dlcToShow.Count > 1 || promotionDLCState.HasSeveralImages)
		{
			imageTransitionAnimator.Play();
		}
	}

	private void GoToStore(DLCRegistry.EDLCKey dlc)
	{
		if (PlatformLayer.Instance.IsConsole)
		{
			IState currentState = Singleton<UINavigation>.Instance.StateMachine.CurrentState;
			NavigationStateMachine stateMachine = Singleton<UINavigation>.Instance.StateMachine;
			if (currentState != stateMachine.GetState(MainStateTag.MainOptions) && currentState != stateMachine.GetState(MainStateTag.SubMenuOptions))
			{
				return;
			}
		}
		PlatformLayer.DLC.OpenPlatformStoreDLCOverlay(dlc);
	}

	public void Hide()
	{
		imageTransitionAnimator.Stop();
		window.Hide();
		comingSoonDLCPrefab.gameObject.SetActive(value: false);
		buyDLCPrefab.gameObject.SetActive(value: false);
	}

	public void Show()
	{
		if (dlcToShow.Count != 0)
		{
			window.Show();
			ShowAvailableDLC(0);
		}
	}

	public void UpdateWindow()
	{
		if (dlcToShow.Count != 0)
		{
			ShowAvailableDLC(0);
		}
	}

	private void AddListeners()
	{
		buyDLCPrefab.OnClick.AddListener(GoToStore);
		comingSoonDLCPrefab.OnClick.AddListener(GoToStore);
		imageTransitionAnimator.OnAnimationFinished.AddListener(ShowNextPromotion);
	}

	private void RemoveListeners()
	{
		buyDLCPrefab.OnClick.RemoveListener(GoToStore);
		comingSoonDLCPrefab.OnClick.RemoveListener(GoToStore);
		imageTransitionAnimator.OnAnimationFinished.RemoveListener(ShowNextPromotion);
	}

	private void OnDisable()
	{
		imageTransitionAnimator.Stop();
	}
}
