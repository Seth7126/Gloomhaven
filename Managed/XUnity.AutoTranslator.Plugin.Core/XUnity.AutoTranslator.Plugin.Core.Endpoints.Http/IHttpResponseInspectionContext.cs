using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;

public interface IHttpResponseInspectionContext : IHttpTranslationContext, ITranslationContextBase
{
	XUnityWebRequest Request { get; }

	XUnityWebResponse Response { get; }
}
