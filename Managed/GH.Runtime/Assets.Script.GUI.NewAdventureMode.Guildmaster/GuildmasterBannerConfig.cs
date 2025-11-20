using TMPro;
using UnityEngine;

namespace Assets.Script.GUI.NewAdventureMode.Guildmaster;

public class GuildmasterBannerConfig : MonoBehaviour, IGuildmasterBannerConfig
{
	[Header("Container")]
	[SerializeField]
	private float _containerHeight;

	[Header("Text Settings")]
	[SerializeField]
	private bool _customTextAlignment;

	[SerializeField]
	private TextAlignmentOptions _textAlignmentOptions;

	[Header("New Line Option")]
	[SerializeField]
	private bool _insertNewLineCharacter;

	[SerializeField]
	private int _newLineIndex;

	public float ContainerHeight => _containerHeight;

	public bool InsertNewLineCharacter => _insertNewLineCharacter;

	public int NewLineIndex => _newLineIndex;

	public TextAlignmentOptions TextAlignmentOptions => _textAlignmentOptions;

	public bool CustomTextAlignment => _customTextAlignment;
}
