using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Photon.Bolt.Internal;
using UnityEngine;

namespace Photon.Bolt.Utils;

[Documentation(Ignore = true)]
public class DebugInfo : MonoBehaviour
{
	private const string TextFrozenEntities = "Frozen Entities ";

	private const string TextActiveEntities = "Active Entities ";

	private const string TextPoll = "Poll ";

	private const string TextSend = "Send ";

	private const string TextMs = " ms";

	private Vector2 debugInfoScroll;

	private static Entity _locked;

	private static GUIStyle _labelStyle;

	private static GUIStyle _labelStyleBold;

	private static Texture2D _boltIconTexture;

	private static Texture2D _backgroundTexture;

	internal static bool _showEntityDebugInfo = true;

	internal static HashSet<NetworkId> _ignoreList = new HashSet<NetworkId>();

	private static Rect _debugArea = new Rect(10f, Screen.height - 30, Screen.width - 20, 20f);

	private static GUIStyle _debugAreaStyle = new GUIStyle
	{
		padding = 
		{
			left = 5,
			top = 5
		}
	};

	private static GUIStyle[] styles = new GUIStyle[11];

	public static float PollTime { get; internal set; }

	public static float SendTime { get; internal set; }

	public static float SendCommandPackTime { get; internal set; }

	public static float SendStatePackTime { get; internal set; }

	public static float PhysicsSnapshotTime { get; internal set; }

	public static bool Enabled { get; private set; }

	public static Texture2D BoltIconTexture
	{
		get
		{
			if (_boltIconTexture == null)
			{
				_boltIconTexture = (Texture2D)Resources.Load("BoltIcon", typeof(Texture2D));
			}
			return _boltIconTexture;
		}
	}

	public static Texture2D BackgroundTexture
	{
		get
		{
			if (_backgroundTexture == null)
			{
				_backgroundTexture = new Texture2D(2, 2);
				_backgroundTexture.SetPixels(new Color[4]
				{
					Color.white,
					Color.white,
					Color.white,
					Color.white
				});
			}
			return _backgroundTexture;
		}
	}

	public static GUIStyle LabelStyle
	{
		get
		{
			if (_labelStyle == null)
			{
				_labelStyle = new GUIStyle
				{
					fontStyle = FontStyle.Normal,
					fontSize = 10,
					alignment = TextAnchor.UpperLeft,
					clipping = TextClipping.Clip,
					normal = 
					{
						textColor = Color.white
					}
				};
			}
			return _labelStyle;
		}
	}

	public static GUIStyle LabelStyleBold
	{
		get
		{
			if (_labelStyleBold == null)
			{
				_labelStyleBold = new GUIStyle(LabelStyle)
				{
					fontStyle = FontStyle.Bold
				};
			}
			return _labelStyleBold;
		}
	}

	public static float GetStopWatchElapsedMilliseconds(Stopwatch stopWatch)
	{
		return (float)stopWatch.ElapsedTicks / (float)Stopwatch.Frequency * 1000f;
	}

	public static void Ignore(BoltEntity entity)
	{
		_ignoreList.Add(entity.Entity.NetworkId);
	}

	public static void DrawBackground(Rect r)
	{
		GUI.color = new Color(0f, 0f, 0f, 0.85f);
		GUI.DrawTexture(r, BackgroundTexture);
		GUI.color = Color.white;
	}

	public static GUIStyle LabelStyleColor(Color color)
	{
		GUIStyle gUIStyle = new GUIStyle(LabelStyle);
		gUIStyle.normal.textColor = color;
		return gUIStyle;
	}

	public static GUIStyle LabelStyleColorRamp(int current, int bad)
	{
		float num = Mathf.Clamp01((float)current / (float)bad);
		int num2 = (int)num * 10;
		if (styles[num2] == null)
		{
			GUIStyle gUIStyle = new GUIStyle(LabelStyle);
			gUIStyle.normal.textColor = GetColor(num);
			styles[num2] = gUIStyle;
		}
		return styles[num2];
	}

	public static void Label(object value)
	{
		GUILayout.Label(value.ToString(), LabelStyle, GUILayout.Height(11f));
	}

	public static void LabelBold(object value)
	{
		GUILayout.Label(value.ToString(), LabelStyleBold, GUILayout.Height(11f));
	}

	public static void LabelField(object label, object value)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label.ToString(), LabelStyle, GUILayout.Height(11f), GUILayout.Width(175f));
		GUILayout.Label(value.ToString(), LabelStyle, GUILayout.Height(11f));
		GUILayout.EndHorizontal();
	}

	public static void Show()
	{
		if (!Object.FindObjectOfType(typeof(DebugInfo)))
		{
			GameObject gameObject = new GameObject("BoltDebugInfo");
			gameObject.AddComponent<DebugInfo>();
			Object.DontDestroyOnLoad(gameObject);
			Enabled = true;
		}
	}

	public static void Hide()
	{
		DebugInfo debugInfo = Object.FindObjectOfType(typeof(DebugInfo)) as DebugInfo;
		if ((bool)debugInfo)
		{
			Object.Destroy(debugInfo.gameObject);
		}
		Enabled = false;
	}

	private static Color GetColor(int current, int bad)
	{
		return GetColor(Mathf.Clamp01((float)current / (float)bad));
	}

	private static Color GetColor(float t)
	{
		return Color.Lerp(BoltGUI.Debug, BoltGUI.Error, t);
	}

	private void DrawEntity(BoltEntity entity)
	{
		if (!entity || !entity.IsAttached)
		{
			return;
		}
		Camera main = Camera.main;
		if ((bool)main)
		{
			Vector3 vector = main.WorldToViewportPoint(entity.transform.position);
			if (vector.z >= 0f && vector.x >= 0f && vector.x <= 1f && vector.y >= 0f && vector.y <= 1f)
			{
				Vector3 vector2 = main.WorldToScreenPoint(entity.transform.position);
				Rect rect = new Rect(vector2.x - 8f, (float)Screen.height - vector2.y - 8f, 16f, 16f);
				DrawBackground(rect);
				GUI.DrawTexture(rect, BoltIconTexture);
			}
		}
	}

	private void OnGUI()
	{
		BoltNetworkInternal.DebugDrawer.IsEditor(isEditor: false);
		DrawSummary();
		Camera c = Camera.main;
		if (!c)
		{
			return;
		}
		foreach (Entity item in BoltCore._entitiesOK)
		{
			DrawEntity(item.UnityObject);
		}
		if (Input.GetKeyDown(KeyCode.Home))
		{
			_locked = (from x in BoltCore._entities
				where !_ignoreList.Contains(x.NetworkId)
				where c.WorldToViewportPoint(x.UnityObject.transform.position).ViewPointIsOnScreen()
				select x).OrderBy(delegate(Entity x)
			{
				Vector3 vector = new Vector3(0.5f, 0.5f, 0f);
				Vector3 vector2 = c.WorldToViewportPoint(x.UnityObject.transform.position);
				vector2.z = 0f;
				return (vector - vector2).sqrMagnitude;
			}).FirstOrDefault();
		}
		if ((bool)_locked)
		{
			Rect rect = new Rect(Screen.width - 410, 10f, 400f, Screen.height - 20);
			DrawBackground(rect);
			rect.xMin += 10f;
			rect.xMax -= 10f;
			rect.yMin += 10f;
			rect.yMax -= 10f;
			GUILayout.BeginArea(rect);
			debugInfoScroll = GUILayout.BeginScrollView(debugInfoScroll, false, false, GUIStyle.none, GUIStyle.none);
			GUILayout.BeginVertical();
			NetworkState networkState = (NetworkState)_locked.Serializer;
			if (Input.GetKeyDown(KeyCode.L))
			{
				BoltNetworkInternal.DebugDrawer.SelectGameObject(_locked.UnityObject.gameObject);
			}
			LabelBold("Entity Info");
			LabelField("Name", _locked.UnityObject.gameObject.name);
			LabelField("Network Id", _locked.NetworkId);
			LabelField("Is Frozen", _locked.IsFrozen);
			LabelField("Animator", (networkState.Animator == null) ? "NULL" : networkState.Animator.gameObject.name);
			LabelField("Entity Parent", _locked.HasParent ? _locked.Parent.UnityObject.ToString() : "NULL");
			LabelField("Has Control", _locked.HasControl);
			if (networkState.Animator != null)
			{
				for (int num = 0; num < networkState.Animator.layerCount; num++)
				{
					LabelField("  Layer", networkState.Animator.GetLayerName(num));
					AnimatorClipInfo[] currentAnimatorClipInfo = networkState.Animator.GetCurrentAnimatorClipInfo(num);
					AnimatorClipInfo[] array = currentAnimatorClipInfo;
					for (int num2 = 0; num2 < array.Length; num2++)
					{
						AnimatorClipInfo animatorClipInfo = array[num2];
						LabelField("    Clip", $"{animatorClipInfo.clip.name} (weight: {animatorClipInfo.weight})");
					}
				}
			}
			if (_locked.IsOwner)
			{
				LabelBold("");
				LabelBold("Connection Priorities");
				foreach (BoltConnection connection in BoltNetwork.Connections)
				{
					LabelField("Connection#" + connection.udpConnection.ConnectionId, connection._entityChannel.GetPriority(_locked).ToString());
				}
			}
			if (!_locked.IsOwner)
			{
				LabelBold("");
				LabelBold("Frame Info");
				LabelField("Buffer Count", networkState.Frames.count);
				LabelField("Latest Received Number", networkState.Frames.last.Frame);
				LabelField("Diff (Should be < 0)", BoltNetwork.ServerFrame - networkState.Frames.last.Frame);
			}
			LabelBold("");
			LabelBold("World Info");
			LabelField("Position", _locked.UnityObject.transform.position);
			LabelField("Distance From Camera", (c.transform.position - _locked.UnityObject.transform.position).magnitude);
			_locked.Serializer.DebugInfo();
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}
		if (Input.GetKey(KeyCode.PageUp))
		{
			debugInfoScroll.y = Mathf.Max(debugInfoScroll.y - 10f, 0f);
		}
		if (Input.GetKey(KeyCode.PageDown))
		{
			debugInfoScroll.y = Mathf.Min(debugInfoScroll.y + 10f, 2000f);
		}
	}

	private void DrawSummary()
	{
		DrawBackground(_debugArea);
		GUILayout.BeginArea(_debugArea, _debugAreaStyle);
		GUILayout.BeginHorizontal();
		GUILayout.Label("Bolt Performance: ", LabelStyleBold);
		string text = PollTime.ToString().PadLeft(3, '0');
		string text2 = SendTime.ToString().PadLeft(3, '0');
		GUILayout.BeginHorizontal();
		GUILayout.Label("Poll ", LabelStyleColorRamp((int)PollTime, 16), GUILayout.ExpandWidth(expand: false));
		GUILayout.Label(text, LabelStyleColorRamp((int)PollTime, 16), GUILayout.Width(40f));
		GUILayout.Label(" ms", LabelStyleColorRamp((int)PollTime, 16), GUILayout.ExpandWidth(expand: false));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Send ", LabelStyleColorRamp((int)PollTime, 16), GUILayout.ExpandWidth(expand: false));
		GUILayout.Label(text2, LabelStyleColorRamp((int)PollTime, 16), GUILayout.Width(40f));
		GUILayout.Label(" ms", LabelStyleColorRamp((int)PollTime, 16), GUILayout.ExpandWidth(expand: false));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Active Entities ", LabelStyle, GUILayout.ExpandWidth(expand: false));
		GUILayout.Label(BoltCore._entities.Count((Entity x) => !x.IsFrozen).ToString(), LabelStyle, GUILayout.ExpandWidth(expand: false));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Frozen Entities ", LabelStyle, GUILayout.ExpandWidth(expand: false));
		GUILayout.Label(BoltCore._entities.Count((Entity x) => x.IsFrozen).ToString(), LabelStyle, GUILayout.ExpandWidth(expand: false));
		GUILayout.EndHorizontal();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	internal static void SetupAndShow()
	{
		if (BoltRuntimeSettings.instance.showDebugInfo)
		{
			_ignoreList = new HashSet<NetworkId>();
			Show();
		}
	}
}
