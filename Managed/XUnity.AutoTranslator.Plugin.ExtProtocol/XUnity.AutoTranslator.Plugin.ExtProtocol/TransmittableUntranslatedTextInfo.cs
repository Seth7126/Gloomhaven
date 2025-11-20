using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol;

public class TransmittableUntranslatedTextInfo
{
	public string[] ContextBefore { get; set; }

	public string UntranslatedText { get; set; }

	public string[] ContextAfter { get; set; }

	public TransmittableUntranslatedTextInfo(string[] contextBefore, string untranslatedText, string[] contextAfter)
	{
		ContextBefore = contextBefore;
		UntranslatedText = untranslatedText;
		ContextAfter = contextAfter;
	}

	public TransmittableUntranslatedTextInfo()
	{
	}

	internal void Encode(TextWriter writer)
	{
		string[] contextBefore = ContextBefore;
		writer.WriteLine(((contextBefore != null) ? contextBefore.Length : 0).ToString(CultureInfo.InvariantCulture));
		if (ContextBefore != null)
		{
			string[] contextBefore2 = ContextBefore;
			foreach (string s in contextBefore2)
			{
				string value = Convert.ToBase64String(Encoding.UTF8.GetBytes(s), Base64FormattingOptions.None);
				writer.WriteLine(value);
			}
		}
		string[] contextAfter = ContextAfter;
		writer.WriteLine(((contextAfter != null) ? contextAfter.Length : 0).ToString(CultureInfo.InvariantCulture));
		if (ContextAfter != null)
		{
			string[] contextBefore2 = ContextAfter;
			foreach (string s2 in contextBefore2)
			{
				string value2 = Convert.ToBase64String(Encoding.UTF8.GetBytes(s2), Base64FormattingOptions.None);
				writer.WriteLine(value2);
			}
		}
		string value3 = Convert.ToBase64String(Encoding.UTF8.GetBytes(UntranslatedText), Base64FormattingOptions.None);
		writer.WriteLine(value3);
	}

	internal void Decode(TextReader reader)
	{
		int num = int.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			string s = reader.ReadLine();
			string text = Encoding.UTF8.GetString(Convert.FromBase64String(s));
			array[i] = text;
		}
		ContextBefore = array;
		int num2 = int.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
		string[] array2 = new string[num2];
		for (int j = 0; j < num2; j++)
		{
			string s2 = reader.ReadLine();
			string text2 = Encoding.UTF8.GetString(Convert.FromBase64String(s2));
			array2[j] = text2;
		}
		ContextAfter = array2;
		string s3 = reader.ReadLine();
		string untranslatedText = Encoding.UTF8.GetString(Convert.FromBase64String(s3));
		UntranslatedText = untranslatedText;
	}
}
