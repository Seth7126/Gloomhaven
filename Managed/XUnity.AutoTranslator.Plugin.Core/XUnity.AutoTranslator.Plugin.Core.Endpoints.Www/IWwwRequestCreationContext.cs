namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Www;

public interface IWwwRequestCreationContext : IWwwTranslationContext, ITranslationContextBase
{
	void Complete(WwwRequestInfo requestInfo);
}
