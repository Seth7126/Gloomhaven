using System.IO;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection;

internal class TextAssetExtensionData
{
	private string _text;

	private byte[] _data;

	public Encoding Encoding { get; set; }

	public byte[] Data
	{
		get
		{
			if (_data == null && _text != null)
			{
				MemoryStream memoryStream = new MemoryStream();
				using StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding ?? Encoding.UTF8);
				streamWriter.Write(_text);
				streamWriter.Flush();
				_data = memoryStream.ToArray();
			}
			return _data;
		}
		set
		{
			_data = value;
		}
	}

	public string Text
	{
		get
		{
			if (_text == null && _data != null)
			{
				using StreamReader streamReader = new StreamReader(new MemoryStream(_data), Encoding ?? Encoding.UTF8);
				_text = streamReader.ReadToEnd();
			}
			return _text;
		}
		set
		{
			_text = value;
		}
	}
}
