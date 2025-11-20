using System;
using System.IO;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol;

public class ConfigurationMessage : ProtocolMessage
{
	public static readonly string Type = "4";

	public string Config { get; set; }

	internal override void Decode(TextReader reader)
	{
		base.Id = new Guid(reader.ReadLine());
		Config = reader.ReadToEnd();
	}

	internal override void Encode(TextWriter writer)
	{
		writer.WriteLine(base.Id.ToString());
		writer.Write(Config);
	}
}
