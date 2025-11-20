#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.UI;

public class InteractabilityManager : MonoBehaviour
{
	public static InteractabilityManager s_Instance;

	private List<InteractabilityIsolatedUIControl> m_ControlsCurrentlyIsolatable = new List<InteractabilityIsolatedUIControl>();

	private List<InteractabilityIsolatedUIControl> m_CurrentlyIsolatedControls = new List<InteractabilityIsolatedUIControl>();

	private CLevelUIInteractionProfile m_CurrentlyLoadedProfile;

	public bool CurrentlyLoadedProfileIsNullOrDefault;

	private CLevelUIInteractionProfile DefaultAdditionalControlsToAllow = new CLevelUIInteractionProfile
	{
		ControlsToAllow = new List<CLevelUIInteractionProfile.CLevelUIInteractionSpecific>
		{
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.EscapeMenu, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.PersistentUI, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.LevelMessageWindow, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.ResultsScreen, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.StoryBox, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly)
		}
	};

	private CLevelUIInteractionProfile DefaultMessagelessControlsToAllow = new CLevelUIInteractionProfile
	{
		ControlsToAllow = new List<CLevelUIInteractionProfile.CLevelUIInteractionSpecific>
		{
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.EscapeMenu, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.PersistentUI, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.ResultsScreen, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.BottomBarConfirmButton, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly)
		}
	};

	[UsedImplicitly]
	private void Awake()
	{
		s_Instance = this;
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		s_Instance = null;
	}

	public static void RegisterControlForInteractionLimiting(InteractabilityIsolatedUIControl controlToRegister)
	{
		if (s_Instance != null && !s_Instance.m_ControlsCurrentlyIsolatable.Contains(controlToRegister))
		{
			s_Instance.m_ControlsCurrentlyIsolatable.Add(controlToRegister);
		}
	}

	public static void DeregisterControlForInteractionLimiting(InteractabilityIsolatedUIControl controlToDeregister)
	{
		if (s_Instance != null)
		{
			s_Instance.m_ControlsCurrentlyIsolatable.RemoveAll((InteractabilityIsolatedUIControl c) => c == null || c == controlToDeregister);
		}
	}

	public static void AddIsolatedControl(InteractabilityIsolatedUIControl controlToRegister)
	{
		if (s_Instance != null && !s_Instance.m_CurrentlyIsolatedControls.Contains(controlToRegister))
		{
			s_Instance.m_CurrentlyIsolatedControls.Add(controlToRegister);
		}
	}

	public static bool ShouldTryPreventControl()
	{
		if (s_Instance != null && s_Instance.m_CurrentlyLoadedProfile != null)
		{
			if (LevelEventsController.s_EventsControllerActive)
			{
				return true;
			}
			if (Singleton<InteractabilityChecker>.Instance != null && Singleton<InteractabilityChecker>.Instance.IsActive)
			{
				return true;
			}
		}
		return false;
	}

	public static bool ShouldAllowClickForExtendedButton(ExtendedButton buttonToCheck)
	{
		if (ShouldTryPreventControl())
		{
			if (s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndAllowAnyInteraction))
			{
				return true;
			}
			foreach (InteractabilityIsolatedUIControl currentlyIsolatedControl in s_Instance.m_CurrentlyIsolatedControls)
			{
				foreach (ExtendedButton item in currentlyIsolatedControl.ExtendedButtonsToAllow)
				{
					if (item.gameObject == buttonToCheck.gameObject)
					{
						return true;
					}
				}
			}
			return false;
		}
		return true;
	}

	public static bool ShouldAllowClickForExtendedToggle(ExtendedToggle toggleToCheck)
	{
		if (ShouldTryPreventControl())
		{
			if (s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndAllowAnyInteraction))
			{
				return true;
			}
			foreach (InteractabilityIsolatedUIControl currentlyIsolatedControl in s_Instance.m_CurrentlyIsolatedControls)
			{
				if (currentlyIsolatedControl.ExtendedTogglesToAllow.Any((ExtendedToggle toggle) => toggle.gameObject == toggleToCheck.gameObject))
				{
					return true;
				}
			}
			if (toggleToCheck.group == null || toggleToCheck.group.name == "UI Menu Panel")
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool ShouldAllowClickForTrackedButton(TrackedButton buttonToCheck)
	{
		if (ShouldTryPreventControl())
		{
			if (s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndAllowAnyInteraction))
			{
				return true;
			}
			foreach (InteractabilityIsolatedUIControl currentlyIsolatedControl in s_Instance.m_CurrentlyIsolatedControls)
			{
				if (currentlyIsolatedControl.TrackedButtonsToAllow.Any((TrackedButton button) => button.gameObject == buttonToCheck.gameObject))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static bool ShouldAllowClickForTrackedToggle(TrackedToggle toggleToCheck)
	{
		if (ShouldTryPreventControl())
		{
			if (s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndAllowAnyInteraction))
			{
				return true;
			}
			foreach (InteractabilityIsolatedUIControl currentlyIsolatedControl in s_Instance.m_CurrentlyIsolatedControls)
			{
				if (currentlyIsolatedControl.TrackedTogglesToAllow.Any((TrackedToggle toggle) => toggle.gameObject == toggleToCheck.gameObject))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static bool ShouldAllowClickForTab(UITab toggleToCheck)
	{
		if (ShouldTryPreventControl())
		{
			if (s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndAllowAnyInteraction))
			{
				return true;
			}
			foreach (InteractabilityIsolatedUIControl currentlyIsolatedControl in s_Instance.m_CurrentlyIsolatedControls)
			{
				if (currentlyIsolatedControl.TabsToAllow.Any((UITab toggle) => toggle.gameObject == toggleToCheck.gameObject))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static bool ShouldAllowSelectionForTileIndex(Point pointToCheck)
	{
		if (ShouldTryPreventControl())
		{
			if (s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndAllowAnyInteraction))
			{
				return true;
			}
			if (IsAllowSelectionForTile(pointToCheck))
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool IsAllowSelectionForTile(Point pointToCheck)
	{
		return s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.TileSelection && c.ControlBehaviour != CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightOnly && c.ControlBehaviour != CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightOnlyObsolete && c.ControlTileIndex.X == pointToCheck.X && c.ControlTileIndex.Y == pointToCheck.Y);
	}

	public void LoadProfile(CLevelUIInteractionProfile profileToLoad)
	{
		try
		{
			InteractabilityHighlightCanvas.s_Instance?.ClearHighlights();
			m_CurrentlyIsolatedControls.Clear();
			m_CurrentlyLoadedProfile = profileToLoad;
			if (m_CurrentlyLoadedProfile != null)
			{
				AddControlsFromProfileToCurrentState(DefaultAdditionalControlsToAllow);
				AddControlsFromProfileToCurrentState(profileToLoad);
				CurrentlyLoadedProfileIsNullOrDefault = false;
			}
			else
			{
				CurrentlyLoadedProfileIsNullOrDefault = true;
			}
		}
		catch (Exception ex)
		{
			m_CurrentlyLoadedProfile = null;
			CurrentlyLoadedProfileIsNullOrDefault = true;
			m_CurrentlyIsolatedControls.Clear();
			Debug.Log("Exception thrown trying to load interactibility profile:" + ex.Message + " | StackTrace:" + ex.StackTrace);
		}
	}

	public void LoadDefaultMessagelessProfile()
	{
		try
		{
			InteractabilityHighlightCanvas.s_Instance?.ClearHighlights();
			m_CurrentlyIsolatedControls.Clear();
			m_CurrentlyLoadedProfile = DefaultMessagelessControlsToAllow;
			AddControlsFromProfileToCurrentState(DefaultMessagelessControlsToAllow);
			CurrentlyLoadedProfileIsNullOrDefault = true;
		}
		catch (Exception ex)
		{
			m_CurrentlyLoadedProfile = null;
			CurrentlyLoadedProfileIsNullOrDefault = true;
			m_CurrentlyIsolatedControls.Clear();
			Debug.Log("Exception thrown trying to load default messageless interactibility profile:" + ex.Message + " | StackTrace:" + ex.StackTrace);
		}
	}

	public void LoadDefaultDismissMessageProfile()
	{
		try
		{
			InteractabilityHighlightCanvas.s_Instance?.ClearHighlights();
			m_CurrentlyIsolatedControls.Clear();
			m_CurrentlyLoadedProfile = DefaultAdditionalControlsToAllow;
			AddControlsFromProfileToCurrentState(DefaultAdditionalControlsToAllow);
		}
		catch (Exception ex)
		{
			m_CurrentlyLoadedProfile = null;
			CurrentlyLoadedProfileIsNullOrDefault = true;
			m_CurrentlyIsolatedControls.Clear();
			Debug.Log("Exception thrown trying to load default Dismiss button interactibility profile:" + ex.Message + " | StackTrace:" + ex.StackTrace);
		}
	}

	public void AddControlsFromProfileToCurrentState(CLevelUIInteractionProfile profileToAdd)
	{
		foreach (CLevelUIInteractionProfile.CLevelUIInteractionSpecific item in profileToAdd.ControlsToAllow)
		{
			if (item.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.TileSelection)
			{
				if (item.ControlTileIndex != null && item.ControlBehaviour != CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly)
				{
					InteractabilityHighlightCanvas.s_Instance?.AddHighlightToTile(item.ControlTileIndex);
				}
				continue;
			}
			foreach (InteractabilityIsolatedUIControl item2 in m_ControlsCurrentlyIsolatable)
			{
				if (MatchControlInProfileToControlInScene(item, item2))
				{
					if (item.ControlBehaviour != CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightOnly)
					{
						m_CurrentlyIsolatedControls.Add(item2);
					}
					if (item.ControlBehaviour != CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly)
					{
						InteractabilityHighlightCanvas.s_Instance?.AddHighlightForRectTransform(item2.GetHighlightRectTransform());
					}
				}
			}
		}
	}

	public bool MatchControlInProfileToControlInScene(CLevelUIInteractionProfile.CLevelUIInteractionSpecific controlInProfile, InteractabilityIsolatedUIControl controlInScene)
	{
		if (controlInProfile.ControlType == controlInScene.ControlType)
		{
			switch (controlInProfile.ControlType)
			{
			case CLevelUIInteractionProfile.EIsolatedControlType.CardSelection:
			case CLevelUIInteractionProfile.EIsolatedControlType.CardTopHalfSelection:
			case CLevelUIInteractionProfile.EIsolatedControlType.CardBottomHalfSelection:
			case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestDialogOption:
			case CLevelUIInteractionProfile.EIsolatedControlType.DefaultMoveAbility:
			case CLevelUIInteractionProfile.EIsolatedControlType.DefaultAttackAbility:
			case CLevelUIInteractionProfile.EIsolatedControlType.CharacterPortrait:
				if (!controlInProfile.ControlIdentifier.Equals(controlInScene.ControlIdentifier))
				{
					return controlInScene.ControlIdentifier.Contains(controlInProfile.ControlIdentifier.Replace(" ", string.Empty).Replace("'", string.Empty));
				}
				return true;
			case CLevelUIInteractionProfile.EIsolatedControlType.ConsumeElementOnCardTop:
			case CLevelUIInteractionProfile.EIsolatedControlType.ConsumeElementOnCardBottom:
				if (controlInProfile.ControlIdentifier.Equals(controlInScene.ControlIdentifier))
				{
					return controlInProfile.ControlIndex == controlInScene.ControlIndex;
				}
				return false;
			case CLevelUIInteractionProfile.EIsolatedControlType.ItemSlotButton:
				if (((CItem.EItemSlot)controlInProfile.ControlContextTypeInt/*cast due to .constrained prefix*/).ToString().Equals(controlInScene.ControlIdentifier) && (controlInProfile.ControlIndex < 0 || controlInProfile.ControlIndex == controlInScene.ControlIndex))
				{
					if (!string.IsNullOrEmpty(controlInProfile.ControlIdentifier2))
					{
						return controlInProfile.ControlIdentifier2 == controlInScene.ControlSecondIdentifier;
					}
					return true;
				}
				return false;
			case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestButton:
			case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestConfirmButton:
			case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestCancelButton:
			case CLevelUIInteractionProfile.EIsolatedControlType.CharacterTabIcon:
				if (!string.IsNullOrEmpty(controlInProfile.ControlIdentifier2))
				{
					return controlInProfile.ControlIdentifier2 == controlInScene.ControlSecondIdentifier;
				}
				return true;
			default:
				return true;
			}
		}
		return false;
	}

	public void ConfigureDefaultControlsForMessage(CLevelMessage messageDisplayed)
	{
		if (messageDisplayed.LayoutType != CLevelMessage.ELevelMessageLayoutType.HelpText && messageDisplayed.DismissTrigger.IsTriggeredByDismiss)
		{
			DefaultAdditionalControlsToAllow.ControlsToAllow.Single((CLevelUIInteractionProfile.CLevelUIInteractionSpecific ic) => ic.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.LevelMessageWindow).ControlBehaviour = CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndLimitInteraction;
		}
		else
		{
			DefaultAdditionalControlsToAllow.ControlsToAllow.Single((CLevelUIInteractionProfile.CLevelUIInteractionSpecific ic) => ic.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.LevelMessageWindow).ControlBehaviour = CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly;
		}
	}

	[ContextMenu("Clear out loaded profile")]
	public void Debug_ClearLoadedProfile()
	{
		LoadProfile(null);
	}
}
