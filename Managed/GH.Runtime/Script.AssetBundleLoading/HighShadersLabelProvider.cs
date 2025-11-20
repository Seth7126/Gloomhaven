using System.Collections.Generic;

namespace Script.AssetBundleLoading;

public class HighShadersLabelProvider : MonoLabelProvider
{
	private const string AlwaysLoadedBaseHigh = "always_loaded_base_high";

	public override IEnumerable<string> GetLabels()
	{
		List<string> list = new List<string>();
		if (!global::PlatformLayer.Setting.DoNotLoadHighShaders)
		{
			list.Add("always_loaded_base_high");
		}
		return list;
	}
}
