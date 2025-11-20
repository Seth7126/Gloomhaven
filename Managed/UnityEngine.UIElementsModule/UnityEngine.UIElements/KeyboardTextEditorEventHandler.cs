using System;

namespace UnityEngine.UIElements;

internal class KeyboardTextEditorEventHandler : TextEditorEventHandler
{
	internal const int kDragThreshold = 4;

	internal bool m_Changed;

	private bool m_Dragged;

	private bool m_DragToPosition;

	private bool m_SelectAllOnMouseUp;

	private string m_PreDrawCursorText;

	private bool m_IsClicking;

	private Vector2 m_ClickStartPosition;

	private readonly Event m_ImguiEvent = new Event();

	private bool isClicking
	{
		get
		{
			return m_IsClicking;
		}
		set
		{
			if (m_IsClicking != value)
			{
				m_IsClicking = value;
				if (m_IsClicking)
				{
					base.textInputField.CaptureMouse();
				}
				else
				{
					base.textInputField.ReleaseMouse();
				}
			}
		}
	}

	public KeyboardTextEditorEventHandler(TextEditorEngine editorEngine, ITextInputField textInputField)
		: base(editorEngine, textInputField)
	{
	}

	public override void ExecuteDefaultActionAtTarget(EventBase evt)
	{
		base.ExecuteDefaultActionAtTarget(evt);
		if (evt.eventTypeId == EventBase<FocusEvent>.TypeId())
		{
			OnFocus(evt as FocusEvent);
		}
		else if (evt.eventTypeId == EventBase<BlurEvent>.TypeId())
		{
			OnBlur(evt as BlurEvent);
		}
		else if (evt.eventTypeId == EventBase<MouseDownEvent>.TypeId())
		{
			OnMouseDown(evt as MouseDownEvent);
		}
		else if (evt.eventTypeId == EventBase<MouseUpEvent>.TypeId())
		{
			OnMouseUp(evt as MouseUpEvent);
		}
		else if (evt.eventTypeId == EventBase<MouseMoveEvent>.TypeId())
		{
			OnMouseMove(evt as MouseMoveEvent);
		}
		else if (evt.eventTypeId == EventBase<KeyDownEvent>.TypeId())
		{
			OnKeyDown(evt as KeyDownEvent);
		}
		else if (evt.eventTypeId == EventBase<ValidateCommandEvent>.TypeId())
		{
			OnValidateCommandEvent(evt as ValidateCommandEvent);
		}
		else if (evt.eventTypeId == EventBase<ExecuteCommandEvent>.TypeId())
		{
			OnExecuteCommandEvent(evt as ExecuteCommandEvent);
		}
	}

	private void OnFocus(FocusEvent _)
	{
		GUIUtility.imeCompositionMode = IMECompositionMode.On;
		m_DragToPosition = false;
		if (PointerDeviceState.GetPressedButtons(PointerId.mousePointerId) == 0)
		{
			VisualElement obj = base.textInputField as VisualElement;
			if (obj == null || obj.panel.contextType != ContextType.Editor || Event.current != null)
			{
				return;
			}
		}
		m_SelectAllOnMouseUp = true;
	}

	private void OnBlur(BlurEvent _)
	{
		GUIUtility.imeCompositionMode = IMECompositionMode.Auto;
	}

	private void OnMouseDown(MouseDownEvent evt)
	{
		base.textInputField.SyncTextEngine();
		m_Changed = false;
		if (!base.textInputField.hasFocus)
		{
			base.editorEngine.m_HasFocus = true;
			base.editorEngine.MoveCursorToPosition_Internal(evt.localMousePosition, evt.button == 0 && evt.shiftKey);
			if (evt.button == 0)
			{
				isClicking = true;
				m_ClickStartPosition = evt.localMousePosition;
			}
			evt.StopPropagation();
		}
		else if (evt.button == 0)
		{
			if (evt.clickCount == 2 && base.textInputField.doubleClickSelectsWord)
			{
				base.editorEngine.SelectCurrentWord();
				base.editorEngine.DblClickSnap(TextEditor.DblClickSnapping.WORDS);
				base.editorEngine.MouseDragSelectsWholeWords(on: true);
				m_DragToPosition = false;
			}
			else if (evt.clickCount == 3 && base.textInputField.tripleClickSelectsLine)
			{
				base.editorEngine.SelectCurrentParagraph();
				base.editorEngine.MouseDragSelectsWholeWords(on: true);
				base.editorEngine.DblClickSnap(TextEditor.DblClickSnapping.PARAGRAPHS);
				m_DragToPosition = false;
			}
			else
			{
				base.editorEngine.MoveCursorToPosition_Internal(evt.localMousePosition, evt.shiftKey);
			}
			isClicking = true;
			m_ClickStartPosition = evt.localMousePosition;
			evt.StopPropagation();
		}
		else if (evt.button == 1)
		{
			if (base.editorEngine.cursorIndex == base.editorEngine.selectIndex)
			{
				base.editorEngine.MoveCursorToPosition_Internal(evt.localMousePosition, shift: false);
			}
			m_SelectAllOnMouseUp = false;
			m_DragToPosition = false;
		}
		base.editorEngine.UpdateScrollOffset();
	}

	private void OnMouseUp(MouseUpEvent evt)
	{
		if (evt.button == 0 && isClicking)
		{
			base.textInputField.SyncTextEngine();
			m_Changed = false;
			if (m_Dragged && m_DragToPosition)
			{
				base.editorEngine.MoveSelectionToAltCursor();
			}
			else if (m_SelectAllOnMouseUp)
			{
				base.editorEngine.SelectAll();
			}
			base.editorEngine.MouseDragSelectsWholeWords(on: false);
			isClicking = false;
			m_DragToPosition = true;
			m_Dragged = false;
			m_SelectAllOnMouseUp = false;
			evt.StopPropagation();
			base.editorEngine.UpdateScrollOffset();
		}
	}

	private void OnMouseMove(MouseMoveEvent evt)
	{
		if (evt.button == 0 && isClicking)
		{
			base.textInputField.SyncTextEngine();
			m_Changed = false;
			m_Dragged = m_Dragged || MoveDistanceQualifiesForDrag(m_ClickStartPosition, evt.localMousePosition);
			if (m_Dragged)
			{
				ProcessDragMove(evt);
			}
			evt.StopPropagation();
			base.editorEngine.UpdateScrollOffset();
		}
	}

	private void ProcessDragMove(MouseMoveEvent evt)
	{
		if (!evt.shiftKey && base.editorEngine.hasSelection && m_DragToPosition)
		{
			base.editorEngine.MoveAltCursorToPosition(evt.localMousePosition);
			return;
		}
		if (evt.shiftKey)
		{
			base.editorEngine.MoveCursorToPosition_Internal(evt.localMousePosition, evt.shiftKey);
		}
		else
		{
			base.editorEngine.SelectToPosition(evt.localMousePosition);
		}
		m_DragToPosition = false;
		m_SelectAllOnMouseUp = !base.editorEngine.hasSelection;
	}

	private bool MoveDistanceQualifiesForDrag(Vector2 start, Vector2 current)
	{
		return (start - current).sqrMagnitude >= 16f;
	}

	private void OnKeyDown(KeyDownEvent evt)
	{
		if (!base.textInputField.hasFocus)
		{
			return;
		}
		base.textInputField.SyncTextEngine();
		m_Changed = false;
		evt.GetEquivalentImguiEvent(m_ImguiEvent);
		if (base.editorEngine.HandleKeyEvent(m_ImguiEvent, base.textInputField.isReadOnly))
		{
			if (base.textInputField.text != base.editorEngine.text)
			{
				m_Changed = true;
			}
			evt.StopPropagation();
		}
		else
		{
			char character = evt.character;
			if ((!base.editorEngine.multiline && (evt.keyCode == KeyCode.Tab || character == '\t')) || (base.editorEngine.multiline && (evt.keyCode == KeyCode.Tab || character == '\t') && evt.modifiers != EventModifiers.None) || (evt.actionKey && (!evt.altKey || character == '\0')))
			{
				return;
			}
			evt.StopPropagation();
			if ((character == '\n' && !base.editorEngine.multiline && !evt.altKey) || (character == '\n' && base.editorEngine.multiline && evt.shiftKey) || !base.textInputField.AcceptCharacter(character))
			{
				return;
			}
			Font font = base.editorEngine.style.font;
			if ((font != null && font.HasCharacter(character)) || character == '\n' || character == '\t')
			{
				base.editorEngine.Insert(character);
				m_Changed = true;
			}
			else if (character == '\0' && !string.IsNullOrEmpty(GUIUtility.compositionString))
			{
				base.editorEngine.ReplaceSelection("");
				m_Changed = true;
			}
		}
		if (m_Changed)
		{
			base.editorEngine.text = base.textInputField.CullString(base.editorEngine.text);
			base.textInputField.UpdateText(base.editorEngine.text);
		}
		base.editorEngine.UpdateScrollOffset();
	}

	private void OnValidateCommandEvent(ValidateCommandEvent evt)
	{
		if (!base.textInputField.hasFocus)
		{
			return;
		}
		base.textInputField.SyncTextEngine();
		m_Changed = false;
		switch (evt.commandName)
		{
		case "Cut":
			if (!base.editorEngine.hasSelection || base.textInputField.isReadOnly)
			{
				return;
			}
			break;
		case "Copy":
			if (!base.editorEngine.hasSelection)
			{
				return;
			}
			break;
		case "Paste":
			if (!base.editorEngine.CanPaste() || base.textInputField.isReadOnly)
			{
				return;
			}
			break;
		case "Delete":
			if (base.textInputField.isReadOnly)
			{
				return;
			}
			break;
		}
		evt.StopPropagation();
	}

	private void OnExecuteCommandEvent(ExecuteCommandEvent evt)
	{
		if (!base.textInputField.hasFocus)
		{
			return;
		}
		base.textInputField.SyncTextEngine();
		m_Changed = false;
		bool flag = false;
		string text = base.editorEngine.text;
		if (!base.textInputField.hasFocus)
		{
			return;
		}
		switch (evt.commandName)
		{
		case "OnLostFocus":
			evt.StopPropagation();
			return;
		case "Cut":
			if (!base.textInputField.isReadOnly)
			{
				base.editorEngine.Cut();
				flag = true;
			}
			break;
		case "Copy":
			base.editorEngine.Copy();
			evt.StopPropagation();
			return;
		case "Paste":
			if (!base.textInputField.isReadOnly)
			{
				base.editorEngine.Paste();
				flag = true;
			}
			break;
		case "SelectAll":
			base.editorEngine.SelectAll();
			evt.StopPropagation();
			return;
		case "Delete":
			if (!base.textInputField.isReadOnly)
			{
				if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
				{
					base.editorEngine.Delete();
				}
				else
				{
					base.editorEngine.Cut();
				}
				flag = true;
			}
			break;
		}
		if (flag)
		{
			if (text != base.editorEngine.text)
			{
				m_Changed = true;
			}
			evt.StopPropagation();
		}
		if (m_Changed)
		{
			base.editorEngine.text = base.textInputField.CullString(base.editorEngine.text);
			base.textInputField.UpdateText(base.editorEngine.text);
			evt.StopPropagation();
		}
		base.editorEngine.UpdateScrollOffset();
	}

	public void PreDrawCursor(string newText)
	{
		base.textInputField.SyncTextEngine();
		m_PreDrawCursorText = base.editorEngine.text;
		int num = base.editorEngine.cursorIndex;
		if (!string.IsNullOrEmpty(GUIUtility.compositionString))
		{
			base.editorEngine.text = newText.Substring(0, base.editorEngine.cursorIndex) + GUIUtility.compositionString + newText.Substring(base.editorEngine.selectIndex);
			num += GUIUtility.compositionString.Length;
		}
		else
		{
			base.editorEngine.text = newText;
		}
		base.editorEngine.text = base.textInputField.CullString(base.editorEngine.text);
		num = Math.Min(num, base.editorEngine.text.Length);
		base.editorEngine.graphicalCursorPos = base.editorEngine.style.GetCursorPixelPosition(base.editorEngine.localPosition, new GUIContent(base.editorEngine.text), num);
	}

	public void PostDrawCursor()
	{
		base.editorEngine.text = m_PreDrawCursorText;
	}
}
