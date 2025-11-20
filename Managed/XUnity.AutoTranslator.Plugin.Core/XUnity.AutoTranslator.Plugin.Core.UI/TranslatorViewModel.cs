using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace XUnity.AutoTranslator.Plugin.Core.UI;

internal class TranslatorViewModel
{
	private bool _isEnabled;

	public TranslationEndpointManager Endpoint { get; set; }

	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (_isEnabled != value)
			{
				_isEnabled = value;
				if (_isEnabled)
				{
					Settings.AddTranslator(Endpoint.Endpoint.Id);
				}
				else
				{
					Settings.RemoveTranslator(Endpoint.Endpoint.Id);
				}
			}
		}
	}

	public TranslatorViewModel(TranslationEndpointManager endpoint)
	{
		Endpoint = endpoint;
		IsEnabled = Settings.EnabledTranslators.Contains(endpoint.Endpoint.Id);
	}
}
