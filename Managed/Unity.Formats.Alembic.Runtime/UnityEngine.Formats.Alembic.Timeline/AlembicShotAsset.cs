using System.ComponentModel;
using UnityEngine.Formats.Alembic.Importer;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.Formats.Alembic.Timeline;

[DisplayName("Alembic Shot")]
public class AlembicShotAsset : PlayableAsset, ITimelineClipAsset, IPropertyPreview
{
	private AlembicStreamPlayer m_stream;

	[Tooltip("Alembic asset to play")]
	[SerializeField]
	private ExposedReference<AlembicStreamPlayer> streamPlayer;

	ClipCaps ITimelineClipAsset.clipCaps => ClipCaps.Looping | ClipCaps.Extrapolation | ClipCaps.ClipIn | ClipCaps.SpeedMultiplier;

	public ExposedReference<AlembicStreamPlayer> StreamPlayer
	{
		get
		{
			return streamPlayer;
		}
		set
		{
			streamPlayer = value;
		}
	}

	public override double duration => (m_stream == null) ? 0f : m_stream.Duration;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<AlembicShotPlayable> scriptPlayable = ScriptPlayable<AlembicShotPlayable>.Create(graph);
		AlembicShotPlayable behaviour = scriptPlayable.GetBehaviour();
		m_stream = StreamPlayer.Resolve(graph.GetResolver());
		behaviour.streamPlayer = m_stream;
		return scriptPlayable;
	}

	public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
	{
		AlembicStreamPlayer alembicStreamPlayer = streamPlayer.Resolve(director);
		if (alembicStreamPlayer != null)
		{
			driver.AddFromName<AlembicStreamPlayer>(alembicStreamPlayer.gameObject, "currentTime");
		}
	}
}
