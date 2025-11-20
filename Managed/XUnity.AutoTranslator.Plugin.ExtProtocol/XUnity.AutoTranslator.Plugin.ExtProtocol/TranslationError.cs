using System;
using System.IO;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol;

public class TranslationError : ProtocolMessage
{
	public static readonly string Type = "3";

	public string Reason { get; set; }

	public StatusCode FailureCode { get; set; }

	internal override void Decode(TextReader reader)
	{
		base.Id = new Guid(reader.ReadLine());
		FailureCode = (StatusCode)int.Parse(reader.ReadLine());
		Reason = reader.ReadToEnd();
	}

	internal override void Encode(TextWriter writer)
	{
		writer.WriteLine(base.Id.ToString());
		writer.WriteLine((int)FailureCode);
		writer.Write(Reason);
	}
}
