using System;
using System.Collections.Generic;
using System.IO;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core;

internal class TranslatedImage
{
	private static readonly Dictionary<string, ImageFormat> Formats = new Dictionary<string, ImageFormat>(StringComparer.OrdinalIgnoreCase)
	{
		{
			".png",
			ImageFormat.PNG
		},
		{
			".tga",
			ImageFormat.TGA
		}
	};

	private readonly ITranslatedImageSource _source;

	private WeakReference<byte[]> _weakData;

	private byte[] _data;

	public string FileName { get; }

	internal ImageFormat ImageFormat { get; }

	private byte[] Data
	{
		get
		{
			if (_source == null)
			{
				return _data;
			}
			byte[] target = _weakData.Target;
			if (target != null)
			{
				return target;
			}
			return null;
		}
		set
		{
			if (_source == null)
			{
				_data = value;
			}
			else
			{
				_weakData = WeakReference<byte[]>.Create(value);
			}
		}
	}

	public TranslatedImage(string fileName, byte[] data, ITranslatedImageSource source)
	{
		_source = source;
		FileName = fileName;
		Data = data;
		ImageFormat = Formats[Path.GetExtension(fileName)];
	}

	public byte[] GetData()
	{
		byte[] array = Data;
		if (array != null)
		{
			return array;
		}
		if (_source != null)
		{
			array = (Data = _source.GetData());
			if (!Settings.EnableSilentMode)
			{
				XuaLogger.AutoTranslator.Debug("Image loaded: " + FileName + ".");
			}
		}
		return array;
	}
}
