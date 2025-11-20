using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;
using UnityEngine;

namespace GLOO.Introduction;

public class UIIntroduceProcessHighlight : UIIntroduceProcess
{
	[Serializable]
	private class ElementHighlight
	{
		public string stepTag;

		public List<UIIntroduceElementHighlight> elements;
	}

	[SerializeField]
	private List<ElementHighlight> elementsHighlight;

	private HashSet<UIIntroduceElementHighlight> elementsHighlighted = new HashSet<UIIntroduceElementHighlight>();

	private string highlightTag;

	private Action cancelAction;

	public override ICallbackPromise Process(IntroductionConfigUI introductionConfig)
	{
		if (introductionConfig == null)
		{
			Debug.LogErrorGUI("Introduction config is null at " + base.name);
			return CallbackPromise.Resolved();
		}
		return Process(introductionConfig.GetSteps(), introductionConfig.name);
	}

	public CallbackPromise Process(List<IntroductionStepUI> steps, string id)
	{
		if (steps.Count == 0 || Singleton<UIIntroductionManager>.Instance == null)
		{
			return CallbackPromise.Resolved();
		}
		CallbackPromise promise = new CallbackPromise();
		promise.Done(delegate
		{
			cancelAction = null;
		});
		cancelAction = delegate
		{
			cancelAction = null;
			Singleton<UIIntroductionManager>.Instance.HideById(id);
			promise.Cancel();
		};
		ICallbackPromise callbackPromise = CallbackPromise.Resolved();
		foreach (IntroductionStepUI step in steps)
		{
			callbackPromise = callbackPromise.Then(() => Process(step, id));
		}
		callbackPromise.Done(delegate
		{
			ResetElements();
			promise.Resolve();
		});
		return promise;
	}

	public CallbackPromise Process(IntroductionStepUI step, string id)
	{
		CallbackPromise callbackPromise = new CallbackPromise();
		if (step.Tag != highlightTag)
		{
			ResetElements();
			if (step.Tag.IsNOTNullOrEmpty())
			{
				foreach (UIIntroduceElementHighlight item in elementsHighlight.Where((ElementHighlight it) => it.stepTag == step.Tag).SelectMany((ElementHighlight it) => it.elements))
				{
					item.Highlight();
					elementsHighlighted.Add(item);
				}
			}
		}
		highlightTag = step.Tag;
		Singleton<UIIntroductionManager>.Instance.ShowStep(id, step, callbackPromise.Resolve, (step.AutoCloseCondition == null) ? null : ((Func<bool>)(() => step.AutoCloseCondition.IsValid())));
		return callbackPromise;
	}

	public override void Cancel()
	{
		ResetElements();
		cancelAction?.Invoke();
	}

	private void ResetElements()
	{
		highlightTag = null;
		foreach (UIIntroduceElementHighlight item in elementsHighlighted)
		{
			item.Unhighlight();
		}
		elementsHighlighted.Clear();
	}
}
