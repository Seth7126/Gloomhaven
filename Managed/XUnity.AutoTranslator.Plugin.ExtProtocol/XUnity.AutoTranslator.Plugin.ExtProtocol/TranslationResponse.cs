using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol;

public class TranslationResponse : ProtocolMessage
{
	public static readonly string Type = "2";

	public string[] TranslatedTexts { get; set; }

	internal override void Decode(TextReader reader)
	{
		base.Id = new Guid(reader.ReadLine());
		int num = int.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			string s = reader.ReadLine();
			string text = Encoding.UTF8.GetString(Convert.FromBase64String(s));
			array[i] = text;
		}
		TranslatedTexts = array;
	}

	internal override void Encode(TextWriter writer)
	{
		writer.WriteLine(base.Id.ToString());
		writer.WriteLine(TranslatedTexts.Length.ToString(CultureInfo.InvariantCulture));
		string[] translatedTexts = TranslatedTexts;
		foreach (string s in translatedTexts)
		{
			string value = Convert.ToBase64String(Encoding.UTF8.GetBytes(s), Base64FormattingOptions.None);
			writer.WriteLine(value);
		}
	}
}
