using System;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;

namespace MapRuleLibrary.YML.Message;

public class MapDialogueLine
{
	public enum EExpression
	{
		Default,
		Angry
	}

	public static readonly EExpression[] Expressions = (EExpression[])Enum.GetValues(typeof(EExpression));

	public string Text { get; private set; }

	public string Character { get; private set; }

	public EExpression Expression { get; private set; }

	public CVector3 CameraLocation { get; private set; }

	public string NarrativeImageID { get; private set; }

	public string NarrativeAudioID { get; private set; }

	public MapDialogueLine(string text, string character, EExpression expression, TileIndex cameraLocation, string narrativeImageID = null, string narrativeAudioId = null)
	{
		Text = text;
		Character = character;
		Expression = expression;
		CameraLocation = ((cameraLocation != null) ? MapYMLShared.GetScreenPointFromMap(cameraLocation.X, cameraLocation.Y) : null);
		NarrativeImageID = narrativeImageID;
		NarrativeAudioID = narrativeAudioId;
	}
}
