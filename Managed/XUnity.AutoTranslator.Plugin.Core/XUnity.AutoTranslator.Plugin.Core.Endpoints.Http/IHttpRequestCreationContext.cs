using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;

public interface IHttpRequestCreationContext : IHttpTranslationContext, ITranslationContextBase
{
	void Complete(XUnityWebRequest request);
}
