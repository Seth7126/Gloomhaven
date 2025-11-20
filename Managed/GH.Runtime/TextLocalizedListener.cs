using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextLocalizedListener : LocalizedListener
{
	private TextMeshProUGUI text;

	[SerializeField]
	private string key;

	[SerializeField]
	private string[] arguments;

	[SerializeField]
	private string format;

	public UnityEvent EventLanguageChanged;

	public TextMeshProUGUI Text
	{
		get
		{
			Initialize();
			return text;
		}
	}

	private void Initialize()
	{
		if (text == null)
		{
			text = GetComponent<TextMeshProUGUI>();
		}
	}

	protected void Awake()
	{
		Initialize();
		OnLanguageChanged();
	}

	protected override void OnLanguageChanged()
	{
		if (!key.IsNullOrEmpty() && !(text == null))
		{
			string obj2;
			if (arguments != null && arguments.Length != 0)
			{
				string obj = LocalizationManager.GetTranslation(key).Replace("\\n", "\n");
				object[] args = arguments;
				obj2 = string.Format(obj, args);
			}
			else
			{
				obj2 = LocalizationManager.GetTranslation(key).Replace("\\n", "\n");
			}
			string arg = obj2;
			if (format.IsNOTNullOrEmpty())
			{
				arg = string.Format(format, arg);
			}
			text.text = arg;
			EventLanguageChanged?.Invoke();
		}
	}

	public void SetTextKey(string key)
	{
		if (!(this.key == key))
		{
			this.key = key;
			OnLanguageChanged();
		}
	}

	public void SetArguments(params string[] arguments)
	{
		this.arguments = arguments;
		OnLanguageChanged();
	}

	public void SetFormat(string format)
	{
		this.format = format;
		OnLanguageChanged();
	}
}
