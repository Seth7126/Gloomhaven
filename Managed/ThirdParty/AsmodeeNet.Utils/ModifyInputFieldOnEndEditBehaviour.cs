using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace AsmodeeNet.Utils;

public static class ModifyInputFieldOnEndEditBehaviour
{
	public static void ModifyBehaviour(TMP_InputField inputField)
	{
		for (int i = 0; i < inputField.onEndEdit.GetPersistentEventCount(); i++)
		{
			int index = i;
			inputField.onEndEdit.SetPersistentListenerState(i, UnityEventCallState.Off);
			inputField.onEndEdit.AddListener(delegate(string value)
			{
				if (Input.GetButtonDown(EventSystem.current.GetComponent<StandaloneInputModule>().submitButton))
				{
					((Component)inputField.onEndEdit.GetPersistentTarget(index)).SendMessage(inputField.onEndEdit.GetPersistentMethodName(index), value);
				}
			});
		}
	}
}
