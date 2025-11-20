using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core;

internal sealed class TextureTranslationCache : IDisposable
{
	private Dictionary<string, TranslatedImage> _translatedImages = new Dictionary<string, TranslatedImage>(StringComparer.InvariantCultureIgnoreCase);

	private HashSet<string> _untranslatedImages = new HashSet<string>();

	private Dictionary<string, string> _keyToFileName = new Dictionary<string, string>();

	private SafeFileWatcher _fileWatcher;

	private bool _disposed;

	public event Action TextureTranslationFileChanged;

	public TextureTranslationCache()
	{
		if (Settings.ReloadTranslationsOnFileChange)
		{
			try
			{
				Directory.CreateDirectory(Settings.TexturesPath);
				_fileWatcher = new SafeFileWatcher(Settings.TexturesPath);
				_fileWatcher.DirectoryUpdated += FileWatcher_DirectoryUpdated;
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An error occurred while initializing translation file watching for textures.");
			}
		}
	}

	private void FileWatcher_DirectoryUpdated()
	{
		this.TextureTranslationFileChanged?.Invoke();
	}

	private IEnumerable<string> GetTextureFiles()
	{
		return from x in Directory.GetFiles(Settings.TexturesPath, "*.*", SearchOption.AllDirectories)
			where x.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
			select x;
	}

	public void LoadTranslationFiles()
	{
		try
		{
			if (!Settings.EnableTextureTranslation && !Settings.EnableTextureDumping)
			{
				return;
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			_translatedImages.Clear();
			_untranslatedImages.Clear();
			_keyToFileName.Clear();
			Directory.CreateDirectory(Settings.TexturesPath);
			foreach (string textureFile in GetTextureFiles())
			{
				RegisterImageFromFile(textureFile);
			}
			float realtimeSinceStartup2 = Time.realtimeSinceStartup;
			XuaLogger.AutoTranslator.Debug($"Loaded texture files (took {Math.Round(realtimeSinceStartup2 - realtimeSinceStartup, 2)} seconds)");
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while loading translations.");
		}
	}

	private void RegisterImageFromStream(string fullFileName, ITranslatedImageSource source)
	{
		try
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFileName);
			int num = fileNameWithoutExtension.LastIndexOf('[');
			int num2 = fileNameWithoutExtension.LastIndexOf(']');
			if (num2 > -1 && num > -1 && num2 > num)
			{
				int num3 = num + 1;
				string[] array = fileNameWithoutExtension.Substring(num3, num2 - num3).Split('-');
				string key;
				string x;
				if (array.Length == 1)
				{
					key = array[0];
					x = array[0];
				}
				else
				{
					if (array.Length != 2)
					{
						XuaLogger.AutoTranslator.Warn("Image not loaded (unknown hash): " + fullFileName + ".");
						return;
					}
					key = array[0];
					x = array[1];
				}
				byte[] data = source.GetData();
				string y = HashHelper.Compute(data);
				bool flag = StringComparer.InvariantCultureIgnoreCase.Compare(x, y) != 0;
				_keyToFileName[key] = fullFileName;
				if (Settings.LoadUnmodifiedTextures || flag)
				{
					RegisterTranslatedImage(fullFileName, key, data, source);
					if (!Settings.EnableSilentMode)
					{
						XuaLogger.AutoTranslator.Debug("Image loaded: " + fullFileName + ".");
					}
				}
				else
				{
					RegisterUntranslatedImage(key);
					XuaLogger.AutoTranslator.Warn("Image not loaded (unmodified): " + fullFileName + ".");
				}
			}
			else
			{
				XuaLogger.AutoTranslator.Warn("Image not loaded (no hash): " + fullFileName + ".");
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while loading texture file: " + fullFileName);
		}
	}

	private void RegisterImageFromFile(string fullFileName)
	{
		if (!File.Exists(fullFileName))
		{
			return;
		}
		if (fullFileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
		{
			ZipFile zipFile = new ZipFile(fullFileName);
			try
			{
				foreach (object item in zipFile)
				{
					if (item is ZipEntry { IsFile: not false } zipEntry && zipEntry.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
					{
						ZipFileTranslatedImageSource source = new ZipFileTranslatedImageSource(zipFile, zipEntry);
						RegisterImageFromStream(fullFileName + Path.DirectorySeparatorChar + zipEntry.Name, source);
					}
				}
				return;
			}
			finally
			{
				if (Settings.CacheTexturesInMemory)
				{
					zipFile.Close();
				}
			}
		}
		FileSystemTranslatedImageSource source2 = new FileSystemTranslatedImageSource(fullFileName);
		RegisterImageFromStream(fullFileName, source2);
	}

	public void RenameFileWithKey(string name, string key, string newKey)
	{
		_fileWatcher?.Disable();
		try
		{
			if (_keyToFileName.TryGetValue(key, out var value))
			{
				_keyToFileName.Remove(key);
				value.Replace(key, newKey);
				if (!IsImageRegistered(newKey))
				{
					byte[] data = File.ReadAllBytes(value);
					RegisterImageFromData(name, newKey, data);
					File.Delete(value);
					XuaLogger.AutoTranslator.Warn("Replaced old file with name '" + name + "' registered with key old '" + key + "'.");
				}
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while trying to rename file with key '" + key + "'.");
		}
		finally
		{
			_fileWatcher?.Enable();
		}
	}

	internal void RegisterImageFromData(string textureName, string key, byte[] data)
	{
		_fileWatcher?.Disable();
		try
		{
			string text = textureName.SanitizeForFileSystem();
			string text2 = HashHelper.Compute(data);
			string text3 = ((!(key == text2)) ? (text + " [" + key + "-" + text2 + "].png") : (text + " [" + key + "].png"));
			string text4 = Path.Combine(Settings.TexturesPath, text3);
			File.WriteAllBytes(text4, data);
			XuaLogger.AutoTranslator.Info("Dumped texture file: " + text3);
			_keyToFileName[key] = text4;
			if (Settings.LoadUnmodifiedTextures)
			{
				FileSystemTranslatedImageSource source = new FileSystemTranslatedImageSource(text4);
				RegisterTranslatedImage(text4, key, data, source);
			}
			else
			{
				RegisterUntranslatedImage(key);
			}
		}
		finally
		{
			_fileWatcher?.Enable();
		}
	}

	private void RegisterTranslatedImage(string fileName, string key, byte[] data, ITranslatedImageSource source)
	{
		_translatedImages[key] = new TranslatedImage(fileName, data, Settings.CacheTexturesInMemory ? null : source);
	}

	private void RegisterUntranslatedImage(string key)
	{
		_untranslatedImages.Add(key);
	}

	internal bool IsImageRegistered(string key)
	{
		if (!_translatedImages.ContainsKey(key))
		{
			return _untranslatedImages.Contains(key);
		}
		return true;
	}

	internal bool TryGetTranslatedImage(string key, out byte[] data, out TranslatedImage image)
	{
		if (_translatedImages.TryGetValue(key, out var value))
		{
			try
			{
				data = value.GetData();
				image = value;
				return data != null;
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An error occurrd while attempting to load image: " + value.FileName);
				_translatedImages.Remove(key);
			}
		}
		data = null;
		image = null;
		return false;
	}

	private void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_fileWatcher?.Dispose();
			}
			_disposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
