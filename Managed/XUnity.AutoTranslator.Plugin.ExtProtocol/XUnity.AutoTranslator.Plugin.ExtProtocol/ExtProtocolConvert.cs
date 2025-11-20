using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol;

public static class ExtProtocolConvert
{
	private static readonly Dictionary<string, Type> IdToType;

	private static readonly Dictionary<Type, string> TypeToId;

	static ExtProtocolConvert()
	{
		IdToType = new Dictionary<string, Type>();
		TypeToId = new Dictionary<Type, string>();
		Register(TranslationRequest.Type, typeof(TranslationRequest));
		Register(TranslationResponse.Type, typeof(TranslationResponse));
		Register(TranslationError.Type, typeof(TranslationError));
		Register(ConfigurationMessage.Type, typeof(ConfigurationMessage));
	}

	public static void Register(string id, Type type)
	{
		IdToType[id] = type;
		TypeToId[type] = id;
	}

	public static string Encode(ProtocolMessage message)
	{
		StringWriter stringWriter = new StringWriter();
		string value = TypeToId[message.GetType()];
		stringWriter.WriteLine(value);
		message.Encode(stringWriter);
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(stringWriter.ToString()), Base64FormattingOptions.None);
	}

	public static ProtocolMessage Decode(string message)
	{
		StringReader stringReader = new StringReader(Encoding.UTF8.GetString(Convert.FromBase64String(message)));
		string key = stringReader.ReadLine();
		ProtocolMessage obj = (ProtocolMessage)Activator.CreateInstance(IdToType[key]);
		obj.Decode(stringReader);
		return obj;
	}
}
