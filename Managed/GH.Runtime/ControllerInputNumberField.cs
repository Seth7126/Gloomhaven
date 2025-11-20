using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ControllerInputNumberField : ControllerInputTMPInputField, IMoveHandler, IEventSystemHandler
{
	[SerializeField]
	protected TMP_InputField m_InputField;

	public int MaxNumber = int.MaxValue;

	public int MinNumber;

	public void OnMove(AxisEventData eventData)
	{
		switch (eventData.moveDir)
		{
		case MoveDirection.Left:
		{
			int num2 = int.Parse(m_InputField.text);
			if (num2 > MinNumber)
			{
				m_InputField.text = (num2 - 1).ToString();
			}
			break;
		}
		case MoveDirection.Right:
		{
			int num = int.Parse(m_InputField.text);
			if (num < MaxNumber)
			{
				m_InputField.text = (num + 1).ToString();
			}
			break;
		}
		}
	}
}
