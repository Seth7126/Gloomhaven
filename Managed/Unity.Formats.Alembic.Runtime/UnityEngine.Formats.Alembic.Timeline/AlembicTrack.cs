using System;
using UnityEngine.Timeline;

namespace UnityEngine.Formats.Alembic.Timeline;

[Serializable]
[TrackClipType(typeof(AlembicShotAsset))]
[TrackColor(0.53f, 0f, 0.08f)]
public class AlembicTrack : TrackAsset
{
}
