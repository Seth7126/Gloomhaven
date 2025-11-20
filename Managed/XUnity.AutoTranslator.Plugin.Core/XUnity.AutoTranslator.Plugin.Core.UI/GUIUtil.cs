using System;
using UnityEngine;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.UI;

internal static class GUIUtil
{
	public const float WindowTitleClearance = 10f;

	public const float ComponentSpacing = 10f;

	public const float HalfComponentSpacing = 5f;

	public const float LabelWidth = 60f;

	public const float LabelHeight = 21f;

	public const float RowHeight = 21f;

	public static GUIContent none = new GUIContent("");

	public static readonly RectOffset Empty = new RectOffset
	{
		left = 0,
		right = 0,
		top = 0,
		bottom = 0
	};

	public static readonly GUIStyle LabelTranslation = CopyStyle(GUI.skin.label, delegate(GUIStyle style)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		style.richText = false;
		style.margin = new RectOffset
		{
			left = GUI.skin.label.margin.left,
			right = GUI.skin.label.margin.right,
			top = 0,
			bottom = 0
		};
		style.padding = new RectOffset
		{
			left = GUI.skin.label.padding.left,
			right = GUI.skin.label.padding.right,
			top = 2,
			bottom = 3
		};
	});

	public static readonly GUIStyle LabelCenter = CopyStyle(GUI.skin.label, delegate(GUIStyle style)
	{
		style.alignment = (TextAnchor)1;
		style.richText = true;
	});

	public static readonly GUIStyle LabelRight = CopyStyle(GUI.skin.label, delegate(GUIStyle style)
	{
		style.alignment = (TextAnchor)2;
	});

	public static readonly GUIStyle LabelRich = CopyStyle(GUI.skin.label, delegate(GUIStyle style)
	{
		style.richText = true;
	});

	public static readonly GUIStyle NoMarginButtonStyle = CopyStyle(GUI.skin.button, delegate(GUIStyle style)
	{
		style.margin = Empty;
	});

	public static readonly GUIStyle NoMarginButtonPressedStyle = CopyStyle(GUI.skin.button, delegate(GUIStyle style)
	{
		style.margin = Empty;
		style.onNormal = GUI.skin.button.onActive;
		style.onFocused = GUI.skin.button.onActive;
		style.onHover = GUI.skin.button.onActive;
		style.normal = GUI.skin.button.onActive;
		style.focused = GUI.skin.button.onActive;
		style.hover = GUI.skin.button.onActive;
	});

	public static readonly GUIStyle NoSpacingBoxStyle = CopyStyle(GUI.skin.box, delegate(GUIStyle style)
	{
		style.margin = Empty;
		style.padding = Empty;
	});

	public static GUIStyle WindowBackgroundStyle = new GUIStyle
	{
		normal = new GUIStyleState
		{
			background = CreateBackgroundTexture()
		}
	};

	public static bool IsAnyMouseButtonOrScrollWheelDownSafe
	{
		get
		{
			if (!UnityFeatures.SupportsMouseScrollDelta)
			{
				return IsAnyMouseButtonOrScrollWheelDownLegacy;
			}
			return IsAnyMouseButtonOrScrollWheelDown;
		}
	}

	public static bool IsAnyMouseButtonOrScrollWheelDownLegacy
	{
		get
		{
			if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
			{
				return Input.GetMouseButtonDown(2);
			}
			return true;
		}
	}

	public static bool IsAnyMouseButtonOrScrollWheelDown
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			if (Input.mouseScrollDelta.y == 0f && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
			{
				return Input.GetMouseButtonDown(2);
			}
			return true;
		}
	}

	public static bool IsAnyMouseButtonOrScrollWheelSafe
	{
		get
		{
			if (!UnityFeatures.SupportsMouseScrollDelta)
			{
				return IsAnyMouseButtonOrScrollWheelLegacy;
			}
			return IsAnyMouseButtonOrScrollWheel;
		}
	}

	public static bool IsAnyMouseButtonOrScrollWheelLegacy
	{
		get
		{
			if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
			{
				return Input.GetMouseButton(2);
			}
			return true;
		}
	}

	public static bool IsAnyMouseButtonOrScrollWheel
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			if (Input.mouseScrollDelta.y == 0f && !Input.GetMouseButton(0) && !Input.GetMouseButton(1))
			{
				return Input.GetMouseButton(2);
			}
			return true;
		}
	}

	public static GUIStyle CopyStyle(GUIStyle other, Action<GUIStyle> setProperties)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		GUIStyle val = new GUIStyle(other);
		setProperties(val);
		return val;
	}

	public static GUIContent CreateContent(string text)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		return new GUIContent(text);
	}

	public static GUIContent CreateContent(string text, string tooltip)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		return new GUIContent(text, tooltip);
	}

	public static Rect R(float x, float y, float width, float height)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		return new Rect(x, y, width, height);
	}

	private static Texture2D CreateBackgroundTexture()
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0037: Expected O, but got Unknown
		Texture2D val = new Texture2D(1, 1, (TextureFormat)5, false);
		val.SetPixel(0, 0, new Color(0.6f, 0.6f, 0.6f, 1f));
		val.Apply();
		Object.DontDestroyOnLoad((Object)val);
		return val;
	}

	public static GUIStyle GetWindowBackgroundStyle()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0036: Expected O, but got Unknown
		if (!Object.op_Implicit((Object)(object)WindowBackgroundStyle.normal.background))
		{
			WindowBackgroundStyle = new GUIStyle
			{
				normal = new GUIStyleState
				{
					background = CreateBackgroundTexture()
				}
			};
		}
		return WindowBackgroundStyle;
	}
}
