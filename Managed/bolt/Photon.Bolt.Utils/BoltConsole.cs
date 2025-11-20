using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Photon.Bolt.Collections;
using UnityEngine;

namespace Photon.Bolt.Utils;

public class BoltConsole : MonoBehaviour
{
	internal struct Line
	{
		public Color color;

		public string text;
	}

	private const string TextDebug = "Debug";

	private const string TextInfo = "Info";

	private const string TextWarn = "Warn";

	private static volatile int _changed = 0;

	private static readonly object _mutex = new object();

	private static readonly BoltRingBuffer<Line> _lines = new BoltRingBuffer<Line>(1024);

	private static readonly BoltRingBuffer<Line> _linesRender = new BoltRingBuffer<Line>(1024);

	public float scrollIndex = 0f;

	private static bool _debugToggle = true;

	private static bool _infoToggle = true;

	private static bool _warnToggle = true;

	[SerializeField]
	private float consoleHeight = 0.5f;

	[SerializeField]
	private int lineHeight = 14;

	[SerializeField]
	public bool visible = true;

	[SerializeField]
	internal KeyCode toggleKey = KeyCode.Tab;

	[SerializeField]
	private int padding = 10;

	[SerializeField]
	private int fontSize = 12;

	[SerializeField]
	private int inset = 10;

	private int sizeAdjustment = 1;

	private bool init = false;

	private GUIStyle checkboxStyle = null;

	private int rectWidth;

	private Rect debugToggleRect;

	private Rect infoRect;

	private Rect warnRect;

	private GUIStyle labelStyleDebug;

	private GUIStyle labelStyleInfo;

	private GUIStyle labelStyleWarn;

	internal static int LinesCount => _linesRender.count;

	internal static IEnumerable<Line> Lines => _linesRender;

	public static void Write(string line, Color color)
	{
		lock (_mutex)
		{
			if (line.Contains("\r") || line.Contains("\n"))
			{
				string[] array = Regex.Split(line, "[\r\n]+");
				foreach (string line2 in array)
				{
					WriteReal(line2, color);
				}
			}
			else
			{
				WriteReal(line, color);
			}
		}
		Interlocked.Increment(ref _changed);
	}

	public static void Write(string line)
	{
		Write(line, Color.white);
	}

	internal static void WriteReal(string line, Color color)
	{
		if (_lines.full)
		{
			_lines.Dequeue();
		}
		_lines.Enqueue(new Line
		{
			text = line,
			color = color
		});
	}

	private void Awake()
	{
		RuntimePlatform platform = Application.platform;
		RuntimePlatform runtimePlatform = platform;
		if (runtimePlatform == RuntimePlatform.IPhonePlayer || runtimePlatform == RuntimePlatform.Android)
		{
			fontSize *= 2;
			lineHeight *= 2;
			sizeAdjustment *= 2;
			if (Screen.width > 1024)
			{
				inset *= 2;
				padding *= 2;
			}
		}
	}

	internal static void Clear()
	{
		lock (_mutex)
		{
			_lines.Clear();
			_linesRender.Clear();
		}
	}

	private void OnGUI()
	{
		if (toggleKey != KeyCode.None && UnityEngine.Event.current.type == EventType.KeyDown && UnityEngine.Event.current.keyCode == toggleKey)
		{
			visible = !visible;
		}
		if (!init)
		{
			RuntimePlatform platform = Application.platform;
			RuntimePlatform runtimePlatform = platform;
			if ((runtimePlatform == RuntimePlatform.IPhonePlayer || runtimePlatform == RuntimePlatform.Android) && Screen.width > 1024)
			{
				GUI.skin.verticalScrollbar.fixedWidth *= 2f;
				GUI.skin.verticalScrollbar.fixedHeight *= 2f;
				GUI.skin.verticalScrollbarThumb.fixedWidth *= 2f;
				GUI.skin.verticalScrollbarThumb.fixedHeight *= 2f;
			}
			checkboxStyle = GUI.skin.toggle;
			checkboxStyle.fontSize = fontSize;
			rectWidth = 100 * sizeAdjustment;
			debugToggleRect = new Rect(Screen.width - rectWidth, padding, rectWidth, 40f);
			infoRect = new Rect(Screen.width - rectWidth, padding + 30 * sizeAdjustment, rectWidth, 40f);
			warnRect = new Rect(Screen.width - rectWidth, padding + 60 * sizeAdjustment, rectWidth, 40f);
			labelStyleDebug = DebugInfo.LabelStyleColor(BoltGUI.Debug);
			labelStyleDebug.fontSize = fontSize;
			labelStyleInfo = DebugInfo.LabelStyleColor(BoltGUI.Info);
			labelStyleInfo.fontSize = fontSize;
			labelStyleWarn = DebugInfo.LabelStyleColor(BoltGUI.Warn);
			labelStyleWarn.fontSize = fontSize;
			init = true;
		}
		if (!visible)
		{
			return;
		}
		LinesRefresh();
		int num = Mathf.Max(1, (int)((float)Screen.height * consoleHeight) / lineHeight);
		DebugInfo.DrawBackground(new Rect(inset, inset, Screen.width - inset * 2, (num - 1) * lineHeight + padding * 2));
		scrollIndex = GUI.VerticalScrollbar(new Rect(inset, inset, 25f, (num - 1) * lineHeight + padding * 2), scrollIndex, 1f, _linesRender.count, 0f);
		_debugToggle = GUI.Toggle(debugToggleRect, _debugToggle, "Debug", checkboxStyle);
		_infoToggle = GUI.Toggle(infoRect, _infoToggle, "Info", checkboxStyle);
		_warnToggle = GUI.Toggle(warnRect, _warnToggle, "Warn", checkboxStyle);
		int num2 = 0;
		int num3 = 0;
		while (num3 <= num)
		{
			int num4 = Mathf.Min(_linesRender.count, num - 1 + (int)scrollIndex);
			if (num2 < _linesRender.count)
			{
				Line line = _linesRender[_linesRender.count - num4 + num2];
				if ((line.color == BoltGUI.Debug && !_debugToggle) || (line.color == BoltGUI.Info && !_infoToggle) || (line.color == BoltGUI.Warn && !_warnToggle))
				{
					num2++;
					continue;
				}
				GUIStyle gUIStyle = null;
				if (line.color == BoltGUI.Info)
				{
					gUIStyle = labelStyleInfo;
				}
				if (line.color == BoltGUI.Warn)
				{
					gUIStyle = labelStyleWarn;
				}
				if (line.color == BoltGUI.Debug || gUIStyle == null)
				{
					gUIStyle = labelStyleDebug;
				}
				GUI.Label(GetRect(num3), line.text, gUIStyle);
				num2++;
			}
			num3++;
		}
	}

	internal static void LinesRefresh()
	{
		if (_changed <= 0)
		{
			return;
		}
		int changed = _changed;
		do
		{
			changed = _changed;
			lock (_mutex)
			{
				_lines.CopyTo(_linesRender);
			}
		}
		while (Interlocked.Add(ref _changed, -changed) > 0);
	}

	private Rect GetRect(int line)
	{
		return new Rect(inset + padding, inset + padding + line * lineHeight, Screen.width - inset * 2 - padding * 2, lineHeight);
	}
}
