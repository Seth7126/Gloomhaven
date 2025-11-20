using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using XUnity.Common.Extensions;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection;

internal class UnzippedDirectory : IDisposable
{
	private class FileEntry
	{
		private string _fullPath;

		public string FileName { get; }

		public string ContainerFile { get; }

		public ZipFile ZipFile { get; }

		public ZipEntry ZipEntry { get; }

		public bool IsZipped => ContainerFile != null;

		public string FullPath
		{
			get
			{
				if (_fullPath == null)
				{
					if (ContainerFile != null)
					{
						_fullPath = Path.Combine(ContainerFile, FileName);
					}
					else
					{
						_fullPath = FileName;
					}
				}
				return _fullPath;
			}
		}

		public FileEntry(string fileName, string containerFile, ZipFile zipFile, ZipEntry zipEntry)
		{
			FileName = fileName;
			ContainerFile = containerFile;
			ZipFile = zipFile;
			ZipEntry = zipEntry;
		}

		public FileEntry(string fileName)
		{
			FileName = fileName;
		}
	}

	private class DirectoryEntry
	{
		private Dictionary<string, DirectoryEntry> _directories = new Dictionary<string, DirectoryEntry>(StringComparer.OrdinalIgnoreCase);

		private Dictionary<string, List<FileEntry>> _files = new Dictionary<string, List<FileEntry>>(StringComparer.OrdinalIgnoreCase);

		public DirectoryEntry GetOrCreateDirectory(string name)
		{
			if (!_directories.TryGetValue(name, out var value))
			{
				value = new DirectoryEntry();
				_directories.Add(name, value);
			}
			return value;
		}

		public DirectoryEntry GetDirectory(string name)
		{
			_directories.TryGetValue(name, out var value);
			return value;
		}

		public void AddFile(string name, FileEntry entry)
		{
			if (!_files.TryGetValue(name, out var value))
			{
				value = new List<FileEntry>();
				_files.Add(name, value);
			}
			value.Add(entry);
		}

		public List<FileEntry> GetFiles(string fullPath, string[] extensions, bool findAllByExtensionInLastDirectory)
		{
			List<FileEntry> list = new List<FileEntry>();
			string[] parts = fullPath.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
			FillEntries(parts, 0, extensions, findAllByExtensionInLastDirectory, list);
			return list;
		}

		public bool DirectoryExists(string fullPath)
		{
			DirectoryEntry directoryEntry = this;
			string[] array = fullPath.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
			foreach (string name in array)
			{
				directoryEntry = directoryEntry.GetDirectory(name);
				if (directoryEntry == null)
				{
					return false;
				}
			}
			return true;
		}

		public bool FileExists(string fullPath)
		{
			DirectoryEntry directoryEntry = this;
			string[] array = fullPath.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				if (i == array.Length - 1)
				{
					return directoryEntry._files.ContainsKey(text);
				}
				directoryEntry = directoryEntry.GetDirectory(text);
				if (directoryEntry == null)
				{
					return false;
				}
			}
			return true;
		}

		private void FillEntries(string[] parts, int index, string[] extensions, bool findAllByExtensionInLastDirectory, List<FileEntry> entries)
		{
			if (index < parts.Length)
			{
				string key = parts[index];
				DirectoryEntry value2;
				if (!findAllByExtensionInLastDirectory && index == parts.Length - 1)
				{
					if (_files.TryGetValue(key, out var value))
					{
						entries.AddRange(value);
					}
				}
				else if (_directories.TryGetValue(key, out value2))
				{
					value2.FillEntries(parts, index + 1, extensions, findAllByExtensionInLastDirectory, entries);
				}
			}
			else
			{
				if (!findAllByExtensionInLastDirectory)
				{
					return;
				}
				if (extensions == null || extensions.Length == 0)
				{
					foreach (KeyValuePair<string, List<FileEntry>> file in _files)
					{
						entries.AddRange(file.Value);
					}
					return;
				}
				foreach (KeyValuePair<string, List<FileEntry>> file2 in _files)
				{
					string fileName = file2.Key;
					if (extensions.Any((string x) => fileName.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
					{
						entries.AddRange(file2.Value);
					}
				}
			}
		}
	}

	private static readonly char[] PathSeparators = new char[2]
	{
		Path.DirectorySeparatorChar,
		Path.AltDirectorySeparatorChar
	};

	private static readonly string _loweredCurrentDirectory = Paths.GameRoot.ToLowerInvariant();

	private readonly string _root;

	private readonly bool _cacheNormalFiles;

	private DirectoryEntry _rootDirectory;

	private List<ZipFile> _allZipFiles;

	private bool disposedValue;

	public UnzippedDirectory(string root, bool cacheNormalFiles)
	{
		_cacheNormalFiles = cacheNormalFiles;
		_allZipFiles = new List<ZipFile>();
		Directory.CreateDirectory(root);
		if (Path.IsPathRooted(root))
		{
			_root = root.ToLowerInvariant();
		}
		else
		{
			_root = Path.Combine(_loweredCurrentDirectory, root.ToLowerInvariant());
		}
		Initialize();
	}

	public IEnumerable<RedirectedResource> GetFiles(string path, params string[] extensions)
	{
		if (!_cacheNormalFiles && Directory.Exists(path))
		{
			bool noExtensions = extensions == null || extensions.Length == 0;
			string[] files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
			string[] array = files;
			foreach (string file in array)
			{
				if (noExtensions || extensions.Any((string x) => file.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
				{
					yield return new RedirectedResource(file);
				}
			}
		}
		if (_rootDirectory == null)
		{
			yield break;
		}
		path = path.ToLowerInvariant();
		if (!Path.IsPathRooted(path))
		{
			path = Path.Combine(_loweredCurrentDirectory, path);
		}
		path = path.MakeRelativePath(_root);
		List<FileEntry> list = (from x in _rootDirectory.GetFiles(path, null, findAllByExtensionInLastDirectory: true)
			orderby x.IsZipped, x.ContainerFile, x.FullPath
			select x).ToList();
		foreach (FileEntry entry in list)
		{
			if (entry.IsZipped)
			{
				yield return new RedirectedResource(() => entry.ZipFile.GetInputStream(entry.ZipEntry), entry.ContainerFile, entry.FullPath);
			}
			else
			{
				yield return new RedirectedResource(entry.FileName);
			}
		}
	}

	public IEnumerable<RedirectedResource> GetFile(string path)
	{
		if (!_cacheNormalFiles && File.Exists(path))
		{
			yield return new RedirectedResource(path);
		}
		if (_rootDirectory == null)
		{
			yield break;
		}
		path = path.ToLowerInvariant();
		if (!Path.IsPathRooted(path))
		{
			path = Path.Combine(_loweredCurrentDirectory, path);
		}
		path = path.MakeRelativePath(_root);
		IOrderedEnumerable<FileEntry> orderedEnumerable = from x in _rootDirectory.GetFiles(path, null, findAllByExtensionInLastDirectory: false)
			orderby x.IsZipped, x.ContainerFile, x.FullPath
			select x;
		foreach (FileEntry entry in orderedEnumerable)
		{
			if (entry.IsZipped)
			{
				yield return new RedirectedResource(() => entry.ZipFile.GetInputStream(entry.ZipEntry), entry.ContainerFile, entry.FullPath);
			}
			else
			{
				yield return new RedirectedResource(entry.FileName);
			}
		}
	}

	public bool DirectoryExists(string path)
	{
		string path2 = path;
		bool flag = false;
		if (_rootDirectory != null)
		{
			path = path.ToLowerInvariant();
			if (!Path.IsPathRooted(path))
			{
				path = Path.Combine(_loweredCurrentDirectory, path);
			}
			path = path.MakeRelativePath(_root);
			flag = _rootDirectory.DirectoryExists(path);
		}
		if (!flag)
		{
			if (!_cacheNormalFiles)
			{
				return Directory.Exists(path2);
			}
			return false;
		}
		return true;
	}

	public bool FileExists(string path)
	{
		string path2 = path;
		bool flag = false;
		if (_rootDirectory != null)
		{
			path = path.ToLowerInvariant();
			if (!Path.IsPathRooted(path))
			{
				path = Path.Combine(_loweredCurrentDirectory, path);
			}
			path = path.MakeRelativePath(_root);
			flag = _rootDirectory.FileExists(path);
		}
		if (!flag)
		{
			if (!_cacheNormalFiles)
			{
				return File.Exists(path2);
			}
			return false;
		}
		return true;
	}

	private void Initialize()
	{
		if (!Directory.Exists(_root))
		{
			return;
		}
		string[] files = Directory.GetFiles(_root, "*", SearchOption.AllDirectories);
		if (files.Length != 0)
		{
			_rootDirectory = new DirectoryEntry();
		}
		string[] array = files;
		foreach (string text in array)
		{
			bool flag = text.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
			if (!flag && (!_cacheNormalFiles || flag))
			{
				continue;
			}
			DirectoryEntry directoryEntry = _rootDirectory;
			string[] array2 = text.ToLowerInvariant().MakeRelativePath(_root).Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
			for (int j = 0; j < array2.Length; j++)
			{
				string text2 = array2[j];
				if (j == array2.Length - 1)
				{
					if (text2.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
					{
						DirectoryEntry directoryEntry2 = directoryEntry;
						ZipFile zipFile = new ZipFile(text);
						_allZipFiles.Add(zipFile);
						foreach (ZipEntry item in zipFile)
						{
							directoryEntry = directoryEntry2;
							string text3 = item.Name.UseCorrectDirectorySeparators().ToLowerInvariant();
							string[] array3 = text3.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
							for (int k = 0; k < array3.Length; k++)
							{
								string name = array3[k];
								if (k == array3.Length - 1)
								{
									if (item.IsFile)
									{
										directoryEntry.AddFile(name, new FileEntry(text3, text, zipFile, item));
									}
									else
									{
										directoryEntry = directoryEntry.GetOrCreateDirectory(name);
									}
								}
								else
								{
									directoryEntry = directoryEntry.GetOrCreateDirectory(name);
								}
							}
						}
					}
					else
					{
						directoryEntry.AddFile(text2, new FileEntry(text));
					}
				}
				else
				{
					directoryEntry = directoryEntry.GetOrCreateDirectory(text2);
				}
			}
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposedValue)
		{
			return;
		}
		foreach (ZipFile allZipFile in _allZipFiles)
		{
			allZipFile.Close();
		}
		disposedValue = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
