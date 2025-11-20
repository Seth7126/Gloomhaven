using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;

namespace UnityEngine.UIElements;

[AddComponentMenu("UI Toolkit/Panel Raycaster (UI Toolkit)")]
public class PanelRaycaster : BaseRaycaster, IRuntimePanelComponent
{
	[StructLayout(LayoutKind.Explicit, Size = 4)]
	private struct FloatIntBits
	{
		[FieldOffset(0)]
		public float f;

		[FieldOffset(0)]
		public int i;
	}

	private BaseRuntimePanel m_Panel;

	public IPanel panel
	{
		get
		{
			return m_Panel;
		}
		set
		{
			BaseRuntimePanel baseRuntimePanel = (BaseRuntimePanel)value;
			if (m_Panel != baseRuntimePanel)
			{
				UnregisterCallbacks();
				m_Panel = baseRuntimePanel;
				RegisterCallbacks();
			}
		}
	}

	private GameObject selectableGameObject => m_Panel?.selectableGameObject;

	public override int sortOrderPriority => (int)(m_Panel?.sortingPriority ?? 0f);

	public override int renderOrderPriority => ConvertFloatBitsToInt(m_Panel?.sortingPriority ?? 0f);

	public override Camera eventCamera => null;

	private void RegisterCallbacks()
	{
		if (m_Panel != null)
		{
			m_Panel.destroyed += OnPanelDestroyed;
		}
	}

	private void UnregisterCallbacks()
	{
		if (m_Panel != null)
		{
			m_Panel.destroyed -= OnPanelDestroyed;
		}
	}

	private void OnPanelDestroyed()
	{
		panel = null;
	}

	public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		if (m_Panel == null)
		{
			return;
		}
		Vector3 vector = Display.RelativeMouseAt(eventData.position);
		int targetDisplay = m_Panel.targetDisplay;
		if (vector != Vector3.zero)
		{
			if ((int)vector.z != targetDisplay)
			{
				return;
			}
		}
		else
		{
			vector = eventData.position;
		}
		Vector3 vector2 = vector;
		Vector2 delta = eventData.delta;
		float num = Screen.height;
		if (targetDisplay > 0 && targetDisplay < Display.displays.Length)
		{
			num = Display.displays[targetDisplay].systemHeight;
		}
		vector2.y = num - vector2.y;
		delta.y = 0f - delta.y;
		int pointerId = (UIElementsRuntimeUtility.activeEventSystem as EventSystem).currentInputModule.ConvertUIToolkitPointerId(eventData);
		IEventHandler capturingElement = m_Panel.GetCapturingElement(pointerId);
		if (!(capturingElement is VisualElement visualElement) || visualElement.panel == m_Panel)
		{
			IPanel panel = ((PointerDeviceState.GetPressedButtons(pointerId) != 0) ? PointerDeviceState.GetPlayerPanelWithSoftPointerCapture(pointerId) : null);
			if ((panel == null || panel == m_Panel) && (capturingElement != null || panel != null || (m_Panel.ScreenToPanel(vector2, delta, out var panelPosition, out var _) && m_Panel.Pick(panelPosition) != null)))
			{
				resultAppendList.Add(new RaycastResult
				{
					gameObject = selectableGameObject,
					module = this,
					screenPosition = vector,
					displayIndex = m_Panel.targetDisplay
				});
			}
		}
	}

	private static int ConvertFloatBitsToInt(float f)
	{
		FloatIntBits floatIntBits = new FloatIntBits
		{
			f = f
		};
		return floatIntBits.i;
	}
}
