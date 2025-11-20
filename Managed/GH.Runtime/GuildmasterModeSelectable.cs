using UnityEngine;
using UnityEngine.UI;

public class GuildmasterModeSelectable : MonoBehaviour
{
	[SerializeField]
	private EGuildmasterMode _guildmasterMode;

	private Selectable _selectable;

	public EGuildmasterMode GuildmasterMode => _guildmasterMode;

	private void Awake()
	{
		_selectable = GetComponent<Selectable>();
	}

	public Selectable GetSelectable()
	{
		return _selectable;
	}
}
