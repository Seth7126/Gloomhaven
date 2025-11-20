using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core;

internal class TextureTranslationInfo
{
	private static Dictionary<string, string> NameToHash = new Dictionary<string, string>();

	private static readonly Encoding UTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

	private string _key;

	private byte[] _originalData;

	private bool _initialized;

	private TextureFormat _textureFormat;

	public WeakReference<Texture2D> Original { get; private set; }

	public Texture2D Translated { get; private set; }

	public Sprite TranslatedSprite { get; set; }

	public bool IsTranslated { get; set; }

	public bool IsDumped { get; set; }

	public bool UsingReplacedTexture { get; set; }

	public void Initialize(Texture2D texture)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!_initialized)
		{
			_initialized = true;
			_textureFormat = texture.format;
			SetOriginal(texture);
		}
	}

	public void SetOriginal(Texture2D texture)
	{
		Original = WeakReference<Texture2D>.Create(texture);
	}

	private void SetTranslated(Texture2D texture)
	{
		Translated = texture;
	}

	public void CreateTranslatedTexture(byte[] newData, ImageFormat format)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Translated == (Object)null)
		{
			Texture2D target = Original.Target;
			Texture2D val = ComponentHelper.CreateEmptyTexture2D(_textureFormat);
			val.LoadImageEx(newData, format, target);
			SetTranslated(val);
			val.SetExtensionData(this);
			UsingReplacedTexture = true;
		}
	}

	public void CreateOriginalTexture()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!Original.IsAlive && _originalData != null)
		{
			Texture2D val = ComponentHelper.CreateEmptyTexture2D(_textureFormat);
			val.LoadImageEx(_originalData, ImageFormat.PNG, null);
			SetOriginal(val);
		}
	}

	public string GetKey()
	{
		SetupHashAndData(Original.Target);
		return _key;
	}

	public byte[] GetOriginalData()
	{
		SetupHashAndData(Original.Target);
		return _originalData;
	}

	public byte[] GetOrCreateOriginalData()
	{
		SetupHashAndData(Original.Target);
		if (_originalData != null)
		{
			return _originalData;
		}
		return Original.Target.GetTextureData().Data;
	}

	private TextureDataResult SetupKeyForNameWithFallback(string name, Texture2D texture)
	{
		bool flag = false;
		string value = null;
		string text = null;
		TextureDataResult textureDataResult = null;
		if (Settings.DetectDuplicateTextureNames)
		{
			textureDataResult = texture.GetTextureData();
			text = HashHelper.Compute(textureDataResult.Data);
			if (NameToHash.TryGetValue(name, out value))
			{
				if (value != text)
				{
					XuaLogger.AutoTranslator.Warn("Detected duplicate image name: " + name);
					flag = true;
					Settings.AddDuplicateName(name);
				}
			}
			else
			{
				NameToHash[name] = text;
			}
		}
		if (Settings.DuplicateTextureNames.Contains(name))
		{
			if (text == null)
			{
				if (textureDataResult == null)
				{
					textureDataResult = texture.GetTextureData();
				}
				text = HashHelper.Compute(textureDataResult.Data);
			}
			_key = text;
		}
		else
		{
			_key = HashHelper.Compute(UTF8.GetBytes(name));
		}
		if (flag && Settings.EnableTextureDumping)
		{
			string key = HashHelper.Compute(UTF8.GetBytes(name));
			AutoTranslationPlugin.Current.TextureCache.RenameFileWithKey(name, key, value);
		}
		return textureDataResult;
	}

	private void SetupHashAndData(Texture2D texture)
	{
		if (_key != null)
		{
			return;
		}
		if (Settings.TextureHashGenerationStrategy == TextureHashGenerationStrategy.FromImageData)
		{
			TextureDataResult textureData = texture.GetTextureData();
			_originalData = textureData.Data;
			_key = HashHelper.Compute(_originalData);
		}
		else if (Settings.TextureHashGenerationStrategy == TextureHashGenerationStrategy.FromImageName)
		{
			string textureName = texture.GetTextureName(null);
			if (textureName == null)
			{
				return;
			}
			TextureDataResult textureDataResult = SetupKeyForNameWithFallback(textureName, texture);
			if (Settings.EnableTextureToggling || Settings.DetectDuplicateTextureNames)
			{
				if (textureDataResult == null)
				{
					textureDataResult = texture.GetTextureData();
				}
				_originalData = textureDataResult.Data;
			}
		}
		else
		{
			if (Settings.TextureHashGenerationStrategy != TextureHashGenerationStrategy.FromImageNameAndScene)
			{
				return;
			}
			string textureName2 = texture.GetTextureName(null);
			if (textureName2 == null)
			{
				return;
			}
			textureName2 = textureName2 + "|" + TranslationScopeHelper.GetActiveSceneId();
			TextureDataResult textureDataResult2 = SetupKeyForNameWithFallback(textureName2, texture);
			if (Settings.EnableTextureToggling || Settings.DetectDuplicateTextureNames)
			{
				if (textureDataResult2 == null)
				{
					textureDataResult2 = texture.GetTextureData();
				}
				_originalData = textureDataResult2.Data;
			}
		}
	}
}
