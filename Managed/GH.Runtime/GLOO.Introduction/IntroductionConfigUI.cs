using System.Collections.Generic;
using UnityEngine;

namespace GLOO.Introduction;

[CreateAssetMenu(menuName = "UI Config/Introduction/Basic")]
public class IntroductionConfigUI : ScriptableObject
{
	[SerializeField]
	protected List<IntroductionStepUI> Steps;

	public List<IntroductionStepUI> GetSteps()
	{
		return Steps.FindAll((IntroductionStepUI it) => it.IsVisible);
	}
}
