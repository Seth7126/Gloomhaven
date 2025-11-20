using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using AsmodeeNet.Foundation.Localization;
using AsmodeeNet.UserInterface;
using AsmodeeNet.Utils;
using UnityEngine;

namespace AsmodeeNet.Foundation;

[RequireComponent(typeof(CoreApplicationDelegate))]
[RequireComponent(typeof(SceneTransitionManager))]
[RequireComponent(typeof(EventManager))]
[RequireComponent(typeof(UINavigationManager))]
[RequireComponent(typeof(FocusableLayer))]
public class CoreApplication : MonoBehaviour
{
	private const string _documentation = "<b>CoreApplication</b> is the root singleton of your game (<i>DontDestroyOnLoad</i>). You should add it to all your scenes.\nYou may overide <b>CoreApplicationDelegate</b> to add your main services.";

	private const string _kModuleName = "CoreApplication";

	private static CoreApplication _instance;

	private Thread _mainThread = Thread.CurrentThread;

	private CoreApplicationDelegate _coreApplicationDelegate;

	private SceneTransitionManager _sceneTransitionManager;

	private EventManager _eventManager;

	private UINavigationManager _uiNavigationManager;

	private Preferences _preferences;

	private LocalizationManager _localizationManager;

	[Tooltip("You can manage localization in Asmodee.net/Localization")]
	public List<LocalizationManager.Language> supportedLanguages = new List<LocalizationManager.Language>
	{
		LocalizationManager.Language.en_US,
		LocalizationManager.Language.fr_FR,
		LocalizationManager.Language.de_DE,
		LocalizationManager.Language.es_ES,
		LocalizationManager.Language.pl_PL
	};

	private NotificationCenter _notificationCenter;

	public static CoreApplication Instance
	{
		get
		{
			if (_instance == null)
			{
				AsmoLogger.Log(Application.isPlaying ? AsmoLogger.Severity.Error : AsmoLogger.Severity.Warning, "CoreApplication", "Missing CoreApplication Instance");
			}
			return _instance;
		}
	}

	public static bool HasInstance => _instance != null;

	public bool IsMainThread => _mainThread.Equals(Thread.CurrentThread);

	public CoreApplicationDelegate CoreApplicationDelegate => _coreApplicationDelegate;

	public SceneTransitionManager SceneTransitionManager => _sceneTransitionManager;

	public EventManager EventManager => _eventManager;

	public UINavigationManager UINavigationManager => _uiNavigationManager;

	public Preferences Preferences => _preferences;

	public LocalizationManager LocalizationManager => _localizationManager;

	public NotificationCenter NotificationCenter
	{
		get
		{
			if (_notificationCenter == null)
			{
				_notificationCenter = new NotificationCenter();
			}
			return _notificationCenter;
		}
	}

	public static bool IsQuitting { get; private set; }

	public static void ExecuteCoroutine(IEnumerator action)
	{
		if (HasInstance && Instance.IsMainThread)
		{
			Instance.StartCoroutine(action);
		}
		else
		{
			AsyncCoroutine.RecursivelyMoveNext(action);
		}
	}

	protected virtual void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (_instance == null)
		{
			_Initialize();
		}
		else if (_instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void _Initialize()
	{
		Hashtable extraInfo = new Hashtable { 
		{
			"SDK Version",
			SDKVersionManager.Version()
		} };
		AsmoLogger.Info("CoreApplication", "Initialization", extraInfo);
		_instance = this;
		_coreApplicationDelegate = GetComponent<CoreApplicationDelegate>();
		if (_coreApplicationDelegate == null)
		{
			AsmoLogger.Error("CoreApplication", "Missing CoreApplicationDelegate component");
		}
		_sceneTransitionManager = GetComponent<SceneTransitionManager>();
		if (_sceneTransitionManager == null)
		{
			AsmoLogger.Error("CoreApplication", "Missing SceneTransitionManager component");
		}
		_eventManager = GetComponent<EventManager>();
		if (_eventManager == null)
		{
			AsmoLogger.Error("CoreApplication", "Missing EventManager component");
		}
		_uiNavigationManager = GetComponent<UINavigationManager>();
		if (_uiNavigationManager == null)
		{
			_uiNavigationManager = base.gameObject.AddComponent<UINavigationManager>();
		}
		if (GetComponent<FocusableLayer>() == null)
		{
			base.gameObject.AddComponent<FocusableLayer>();
		}
		_localizationManager = new LocalizationManager(supportedLanguages);
		_preferences = new Preferences();
		AsmoLogger.Debug("CoreApplication", "Initialized", extraInfo);
	}

	private void Start()
	{
		_mainThread = Thread.CurrentThread;
		_localizationManager.Init();
	}

	private void Update()
	{
		_preferences.Update();
	}

	private void OnApplicationQuit()
	{
		IsQuitting = true;
		KeyValueStore.ResetInstance();
	}

	public static string GetUserAgent()
	{
		string text = "";
		string text2 = Application.productName.Replace(" ", string.Empty);
		text = text + text2 + "/" + Application.version;
		string text3 = "Windows";
		text = text + " " + text3 + "/" + Environment.OSVersion.Platform.ToString() + "-" + Environment.OSVersion.Version.ToString();
		return text + " Unity/" + Application.unityVersion;
	}
}
