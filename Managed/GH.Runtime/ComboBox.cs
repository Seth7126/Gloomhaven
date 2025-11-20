using UnityEngine;

public class ComboBox
{
	private static bool forceToUnShow = false;

	private static int useControlID = -1;

	private bool isClickedComboButton;

	private int selectedItemIndex;

	private Rect rect;

	private GUIContent buttonContent;

	private GUIContent[] listContent;

	private string buttonStyle;

	private string boxStyle;

	private GUIStyle listStyle;

	public int SelectedItemIndex
	{
		get
		{
			return selectedItemIndex;
		}
		set
		{
			selectedItemIndex = value;
		}
	}

	public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle)
	{
		this.rect = rect;
		this.buttonContent = buttonContent;
		this.listContent = listContent;
		buttonStyle = "button";
		boxStyle = "box";
		this.listStyle = listStyle;
	}

	public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, string buttonStyle, string boxStyle, GUIStyle listStyle)
	{
		this.rect = rect;
		this.buttonContent = buttonContent;
		this.listContent = listContent;
		this.buttonStyle = buttonStyle;
		this.boxStyle = boxStyle;
		this.listStyle = listStyle;
	}

	public int Show()
	{
		if (forceToUnShow)
		{
			forceToUnShow = false;
			isClickedComboButton = false;
		}
		bool flag = false;
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		if (Event.current.GetTypeForControl(controlID) == EventType.MouseUp && isClickedComboButton)
		{
			flag = true;
		}
		if (GUI.Button(rect, buttonContent, buttonStyle))
		{
			if (useControlID == -1)
			{
				useControlID = controlID;
				isClickedComboButton = false;
			}
			if (useControlID != controlID)
			{
				forceToUnShow = true;
				useControlID = controlID;
			}
			isClickedComboButton = true;
		}
		if (isClickedComboButton)
		{
			Rect position = new Rect(rect.x, rect.y + listStyle.CalcHeight(listContent[0], 1f), rect.width, listStyle.CalcHeight(listContent[0], 1f) * (float)listContent.Length);
			GUI.Box(position, "", boxStyle);
			int num = GUI.SelectionGrid(position, selectedItemIndex, listContent, 1, listStyle);
			if (num != selectedItemIndex)
			{
				selectedItemIndex = num;
				buttonContent = listContent[selectedItemIndex];
			}
		}
		if (flag)
		{
			isClickedComboButton = false;
		}
		return selectedItemIndex;
	}
}
