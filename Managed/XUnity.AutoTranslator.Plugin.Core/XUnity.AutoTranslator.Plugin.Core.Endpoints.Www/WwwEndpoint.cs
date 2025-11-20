using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Www;

public abstract class WwwEndpoint : ITranslateEndpoint
{
	public abstract string Id { get; }

	public abstract string FriendlyName { get; }

	public virtual int MaxConcurrency => 1;

	public virtual int MaxTranslationsPerRequest => 1;

	public virtual IEnumerator OnBeforeTranslate(IWwwTranslationContext context)
	{
		return null;
	}

	public abstract void Initialize(IInitializationContext context);

	public abstract void OnCreateRequest(IWwwRequestCreationContext context);

	public abstract void OnExtractTranslation(IWwwTranslationExtractionContext context);

	protected WWW CreateWww(string address, byte[] data, Dictionary<string, string> headers)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Expected O, but got Unknown
		return new WWW(address, data, headers);
	}

	public IEnumerator Translate(ITranslationContext context)
	{
		WwwTranslationContext wwwContext = new WwwTranslationContext(context);
		IEnumerator setup = OnBeforeTranslate(wwwContext);
		if (setup != null)
		{
			while (setup.MoveNext())
			{
				yield return setup.Current;
			}
		}
		OnCreateRequest(wwwContext);
		if (wwwContext.RequestInfo == null)
		{
			wwwContext.Fail("No request object was provided by the translator.");
		}
		WwwRequestInfo requestInfo = wwwContext.RequestInfo;
		_ = requestInfo.Address;
		string data = requestInfo.Data;
		Dictionary<string, string> headers = requestInfo.Headers;
		WWW www = CreateWww(requestInfo.Address, (data != null) ? Encoding.UTF8.GetBytes(data) : null, headers);
		yield return www;
		string error = www.error;
		if (error != null)
		{
			wwwContext.Fail("Error occurred while retrieving translation. " + error);
		}
		string text = www.text;
		if (text == null)
		{
			wwwContext.Fail("Error occurred while extracting text from response.");
		}
		wwwContext.ResponseData = text;
		OnExtractTranslation(wwwContext);
	}
}
