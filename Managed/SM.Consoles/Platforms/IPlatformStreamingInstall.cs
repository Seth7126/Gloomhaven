using System;

namespace Platforms;

public interface IPlatformStreamingInstall
{
	int EstimatedRequiredTime { get; }

	float NormalizedProgress { get; }

	bool AllFilesAccessible { get; }

	bool AllFilesDownloaded { get; }

	event Action<float, int> EventProgressChanged;

	event Action EventFilesAccessible;

	event Action EventFilesDownloaded;
}
