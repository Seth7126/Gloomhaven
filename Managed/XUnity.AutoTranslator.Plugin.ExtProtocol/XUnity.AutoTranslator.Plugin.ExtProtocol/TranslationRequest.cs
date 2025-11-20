using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol;

public class TranslationRequest : ProtocolMessage
{
	public static readonly string Type = "1";

	private string[] _untranslatedTexts;

	public string SourceLanguage { get; set; }

	public string DestinationLanguage { get; set; }

	public string[] UntranslatedTexts => _untranslatedTexts ?? (_untranslatedTexts = UntranslatedTextInfos.Select((TransmittableUntranslatedTextInfo x) => x.UntranslatedText).ToArray());

	public TransmittableUntranslatedTextInfo[] UntranslatedTextInfos { get; set; }

	internal override void Decode(TextReader reader)
	{
		base.Id = new Guid(reader.ReadLine());
		SourceLanguage = reader.ReadLine();
		DestinationLanguage = reader.ReadLine();
		int num = int.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
		TransmittableUntranslatedTextInfo[] array = new TransmittableUntranslatedTextInfo[num];
		for (int i = 0; i < num; i++)
		{
			TransmittableUntranslatedTextInfo transmittableUntranslatedTextInfo = new TransmittableUntranslatedTextInfo();
			transmittableUntranslatedTextInfo.Decode(reader);
			array[i] = transmittableUntranslatedTextInfo;
		}
		UntranslatedTextInfos = array;
	}

	internal override void Encode(TextWriter writer)
	{
		writer.WriteLine(base.Id.ToString());
		writer.WriteLine(SourceLanguage);
		writer.WriteLine(DestinationLanguage);
		writer.WriteLine(UntranslatedTextInfos.Length.ToString(CultureInfo.InvariantCulture));
		TransmittableUntranslatedTextInfo[] untranslatedTextInfos = UntranslatedTextInfos;
		for (int i = 0; i < untranslatedTextInfos.Length; i++)
		{
			untranslatedTextInfos[i].Encode(writer);
		}
	}
}
