namespace Pros.Sdk;

public class ProsSdkSettings
{
	public SdkEnvironment Environment { get; set; }

	public string TitleId { get; set; }

	public string SecretKey { get; set; }

	public string Version { get; set; }

	public string Platform { get; set; }

	public bool ManualComponentsHandling { get; set; }
}
