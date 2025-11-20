using System;
using UnityEngine;

namespace GLOO.Introduction;

public class UIIntroduceConcept : UIIntroduceBase
{
	[SerializeField]
	[ConditionalField("conceptConfig", "null", true)]
	protected EIntroductionConcept concept;

	[SerializeField]
	protected IntroductionConceptConfigUI conceptConfig;

	public override void Show(Action onFinished = null)
	{
		Show((conceptConfig == null) ? UIInfoTools.Instance.GetIntroductionConfig(concept) : conceptConfig, onFinished);
	}
}
