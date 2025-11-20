using SRDebugger.UI.Controls;
using SRF;
using SRF.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace SRDebugger.UI.Other;

public class TriggerRoot : SRMonoBehaviourEx
{
	[RequiredField]
	public Canvas Canvas;

	[RequiredField]
	public LongPressButton TapHoldButton;

	[RequiredField]
	public RectTransform TriggerTransform;

	[RequiredField]
	public ErrorNotifier ErrorNotifier;

	[RequiredField]
	[FormerlySerializedAs("TriggerButton")]
	public MultiTapButton TripleTapButton;

	[RequiredField]
	public InputsComboController InputsComboController;
}
