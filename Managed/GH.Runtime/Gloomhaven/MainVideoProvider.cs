using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Video;

namespace Gloomhaven;

public class MainVideoProvider : Singleton<MainVideoProvider>
{
	[SerializeField]
	[UsedImplicitly]
	private VideoPlayer _videoPlayer;

	public VideoPlayer VideoPlayer => _videoPlayer;
}
