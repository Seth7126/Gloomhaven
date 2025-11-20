using System.Collections.Generic;

public sealed class StandaloneLabelProvider : MonoLabelProvider
{
	private readonly string _standaloneLabel = "always_loaded_standalone";

	public override IEnumerable<string> GetLabels()
	{
		List<string> list = new List<string>();
		if (PlatformLayer.Instance != null && PlatformLayer.Setting.Platform == DeviceType.Standalone)
		{
			list.Add(_standaloneLabel);
		}
		return list;
	}
}
