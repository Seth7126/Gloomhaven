#define ENABLE_LOGS
using System;
using System.Collections;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class PhaseBannerHandler : Singleton<PhaseBannerHandler>
{
	private enum PhaseBanner
	{
		NONE,
		ENEMY_TURN,
		PLAYER_TURN,
		START_ROUND,
		END_ROUND,
		EXHAUSTED,
		DEATH,
		PRE_DEATH
	}

	[SerializeField]
	private TextMeshProUGUI phaseText;

	[SerializeField]
	private Image interactionBlock;

	[SerializeField]
	private GUIAnimator showAnimation;

	[SerializeField]
	private string subtextFormat = "<size=24><font=\"Sarala-Regular SDF\"><color=#d0d0d0>{1}</color></font></size>";

	[Header("Phase Colors")]
	[SerializeField]
	private Color roundColor;

	[SerializeField]
	private Color playerTurnColor;

	[SerializeField]
	private Color enemyTurnColor;

	[SerializeField]
	private Color dieColor;

	private string lastShownEnemyTurn;

	private UIWindow window;

	private Action onClosed;

	private PhaseBanner currentPhaseShown;

	private bool isEnabled;

	protected override void Awake()
	{
		base.Awake();
		isEnabled = true;
		window = GetComponent<UIWindow>();
		window.onHidden.AddListener(OnHidden);
		window.onShown.AddListener(OnShown);
		showAnimation.OnAnimationFinished.AddListener(delegate
		{
			Debug.LogGUI($"Banner: Finished show banner animation phase {currentPhaseShown}");
			window.Hide();
		});
		showAnimation.OnAnimationStopped.AddListener(delegate
		{
			Debug.LogGUI($"Banner: Stopped show banner animation {currentPhaseShown}");
		});
		showAnimation.OnAnimationStarted.AddListener(delegate
		{
			Debug.LogGUI($"Banner: Started show banner animation {currentPhaseShown}");
		});
	}

	private void SetData(string text, Color color, bool lockInteraction = false, Action onClosed = null)
	{
		phaseText.text = text;
		phaseText.color = color;
		this.onClosed = onClosed;
		interactionBlock.enabled = lockInteraction;
	}

	private void ShowPhase(string text, Color color, bool lockInteraction = false, Action onClosed = null)
	{
		if (!isEnabled)
		{
			Debug.LogGUI($"Banner: Skipped show phase {text} since it's disabled (phase {currentPhaseShown})");
			return;
		}
		if (window.IsOpen)
		{
			Debug.LogGUI($"Banner: Trigger on closed {text} since window is open (phase {currentPhaseShown})");
			onClosed?.Invoke();
		}
		SetData(text, color, lockInteraction, onClosed);
		Show();
	}

	private void Show()
	{
		if (!isEnabled)
		{
			Debug.LogGUI($"Banner: Skipped show since it's disabled (phase {currentPhaseShown})");
			return;
		}
		StopAllCoroutines();
		if (HasToWaitoToShow())
		{
			StartCoroutine(WaitToShow());
			return;
		}
		Debug.LogGUI($"Banner: Show {currentPhaseShown} isOpen {window.IsOpen}");
		if (window.IsOpen)
		{
			showAnimation.Stop();
			showAnimation.Play();
		}
		window.Show();
	}

	public void ResetAndHide()
	{
		if (!isEnabled)
		{
			Debug.LogGUI($"Banner: Skipped reset and hide since it's disabled {currentPhaseShown}");
			return;
		}
		Debug.LogGUI($"Banner: ResetAndHide {currentPhaseShown} isOpen {window.IsOpen}");
		window.Hide();
	}

	private bool HasToWaitoToShow()
	{
		if (TransitionManager.s_Instance != null)
		{
			return !TransitionManager.s_Instance.TransitionDone;
		}
		return false;
	}

	private IEnumerator WaitToShow()
	{
		if (HasToWaitoToShow())
		{
			yield return new WaitUntil(() => TransitionManager.s_Instance.TransitionDone);
		}
		window.Show();
	}

	private void OnShown()
	{
		Debug.Log($"* banner - show ({currentPhaseShown})");
		Debug.LogGUI($"Banner: On window show {currentPhaseShown}. Is already playing " + showAnimation.IsPlaying);
		showAnimation.Stop();
		showAnimation.Play();
		CameraController.s_CameraController.RequestDisableCameraInput(this);
		WorldspaceStarHexDisplay.Instance.Hide(this);
	}

	private void OnHidden()
	{
		Debug.Log($"* banner - hide ({currentPhaseShown})");
		Debug.LogGUI($"Banner: Hidden banner phase {currentPhaseShown}");
		currentPhaseShown = PhaseBanner.NONE;
		StopAllCoroutines();
		showAnimation.Stop();
		CameraController.s_CameraController.FreeDisableCameraInput(this);
		WorldspaceStarHexDisplay.Instance.CancelHide(this);
		Action action = onClosed;
		onClosed = null;
		action?.Invoke();
	}

	private void SetPhase(PhaseBanner phase)
	{
		currentPhaseShown = phase;
		if (phase == PhaseBanner.END_ROUND || phase == PhaseBanner.START_ROUND || phase == PhaseBanner.PLAYER_TURN)
		{
			lastShownEnemyTurn = null;
		}
	}

	public void Disable()
	{
		Debug.LogGUI($"Banner: Disable {currentPhaseShown} isOpen {window.IsOpen}");
		isEnabled = false;
	}

	public void ShowStartRound(int round, Action onClosed = null)
	{
		SetPhase(PhaseBanner.START_ROUND);
		ShowPhase(string.Format(LocalizationManager.GetTranslation("GUI_START_ROUND_BANNER"), round), roundColor, lockInteraction: true, onClosed);
	}

	public void ShowEnemyTurn(string enemyName, Action onClosed = null)
	{
		if (lastShownEnemyTurn == enemyName)
		{
			Debug.LogGUI("Banner: End immediatly since it's the same enemy");
			onClosed?.Invoke();
		}
		else
		{
			lastShownEnemyTurn = enemyName;
			SetPhase(PhaseBanner.ENEMY_TURN);
			ShowPhase(string.Format(LocalizationManager.GetTranslation("GUI_COMBATLOG_START_TURN"), enemyName), enemyTurnColor, lockInteraction: true, onClosed);
		}
	}

	public void ShowPlayerTurn(string playerName, string subtext = null)
	{
		SetPhase(PhaseBanner.PLAYER_TURN);
		string text = string.Format(LocalizationManager.GetTranslation("GUI_COMBATLOG_START_TURN"), playerName);
		if (subtext.IsNOTNullOrEmpty())
		{
			text = text + "\n" + string.Format(subtextFormat, subtext);
		}
		ShowPhase(text, playerTurnColor);
	}

	public void ShowDie()
	{
		SetPhase(PhaseBanner.DEATH);
		Show();
	}

	public void ShowExhausted(string name, Action callback = null)
	{
		if (currentPhaseShown == PhaseBanner.PRE_DEATH)
		{
			SetData(string.Format(LocalizationManager.GetTranslation("GUI_PLAYER_EXHAUSTED_BANNER"), name), dieColor, lockInteraction: true, delegate
			{
				callback?.Invoke();
			});
		}
		else
		{
			ShowPhase(string.Format(LocalizationManager.GetTranslation("GUI_PLAYER_EXHAUSTED_BANNER"), name), dieColor, lockInteraction: true, callback);
		}
		SetPhase(PhaseBanner.EXHAUSTED);
	}

	public void ShowPreDie(string name, Action onClosed = null)
	{
		SetData(string.Format(LocalizationManager.GetTranslation("GUI_PLAYER_EXHAUSTED_BANNER"), name), dieColor, lockInteraction: true, onClosed);
		SetPhase(PhaseBanner.PRE_DEATH);
	}

	public bool IsShowingDeath()
	{
		if (currentPhaseShown != PhaseBanner.DEATH && currentPhaseShown != PhaseBanner.EXHAUSTED)
		{
			return currentPhaseShown == PhaseBanner.PRE_DEATH;
		}
		return true;
	}
}
