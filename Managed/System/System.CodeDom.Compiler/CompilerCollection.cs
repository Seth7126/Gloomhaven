using System.Collections.Generic;
using System.Configuration;

namespace System.CodeDom.Compiler;

[ConfigurationCollection(typeof(Compiler), AddItemName = "compiler", CollectionType = ConfigurationElementCollectionType.BasicMap)]
internal sealed class CompilerCollection : ConfigurationElementCollection
{
	private static readonly string defaultCompilerVersion;

	private static ConfigurationPropertyCollection properties;

	private static List<CompilerInfo> compiler_infos;

	private static Dictionary<string, CompilerInfo> compiler_languages;

	private static Dictionary<string, CompilerInfo> compiler_extensions;

	protected override bool ThrowOnDuplicate => false;

	public string[] AllKeys
	{
		get
		{
			string[] array = new string[compiler_infos.Count];
			for (int i = 0; i < base.Count; i++)
			{
				array[i] = string.Join(";", compiler_infos[i].GetLanguages());
			}
			return array;
		}
	}

	public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;

	protected override string ElementName => "compiler";

	protected override ConfigurationPropertyCollection Properties => properties;

	public Compiler this[int index] => (Compiler)BaseGet(index);

	public new CompilerInfo this[string language] => GetCompilerInfoForLanguage(language);

	public CompilerInfo[] CompilerInfos => compiler_infos.ToArray();

	static CompilerCollection()
	{
		defaultCompilerVersion = "4.0";
		properties = new ConfigurationPropertyCollection();
		compiler_infos = new List<CompilerInfo>();
		compiler_languages = new Dictionary<string, CompilerInfo>(16, StringComparer.OrdinalIgnoreCase);
		compiler_extensions = new Dictionary<string, CompilerInfo>(4, StringComparer.OrdinalIgnoreCase);
		CompilerInfo compilerInfo = new CompilerInfo(null, "Microsoft.CSharp.CSharpCodeProvider, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", new string[3] { "c#", "cs", "csharp" }, new string[1] { ".cs" });
		compilerInfo.ProviderOptions["CompilerVersion"] = defaultCompilerVersion;
		AddCompilerInfo(compilerInfo);
		CompilerInfo compilerInfo2 = new CompilerInfo(null, "Microsoft.VisualBasic.VBCodeProvider, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", new string[4] { "vb", "vbs", "visualbasic", "vbscript" }, new string[1] { ".vb" });
		compilerInfo2.ProviderOptions["CompilerVersion"] = defaultCompilerVersion;
		AddCompilerInfo(compilerInfo2);
	}

	private static void AddCompilerInfo(CompilerInfo ci)
	{
		ci.CreateProvider();
		compiler_infos.Add(ci);
		string[] languages = ci.GetLanguages();
		if (languages != null)
		{
			string[] array = languages;
			foreach (string key in array)
			{
				compiler_languages[key] = ci;
			}
		}
		string[] extensions = ci.GetExtensions();
		if (extensions != null)
		{
			string[] array = extensions;
			foreach (string key2 in array)
			{
				compiler_extensions[key2] = ci;
			}
		}
	}

	private static void AddCompilerInfo(Compiler compiler)
	{
		CompilerInfo compilerInfo = new CompilerInfo(null, compiler.Type, new string[1] { compiler.Extension }, new string[1] { compiler.Language });
		compilerInfo.CompilerParams.CompilerOptions = compiler.CompilerOptions;
		compilerInfo.CompilerParams.WarningLevel = compiler.WarningLevel;
		AddCompilerInfo(compilerInfo);
	}

	protected override void BaseAdd(ConfigurationElement element)
	{
		if (element is Compiler compiler)
		{
			AddCompilerInfo(compiler);
		}
		base.BaseAdd(element);
	}

	protected override ConfigurationElement CreateNewElement()
	{
		return new Compiler();
	}

	public CompilerInfo GetCompilerInfoForLanguage(string language)
	{
		if (compiler_languages.Count == 0)
		{
			return null;
		}
		if (compiler_languages.TryGetValue(language, out var value))
		{
			return value;
		}
		return null;
	}

	public CompilerInfo GetCompilerInfoForExtension(string extension)
	{
		if (compiler_extensions.Count == 0)
		{
			return null;
		}
		if (compiler_extensions.TryGetValue(extension, out var value))
		{
			return value;
		}
		return null;
	}

	public string GetLanguageFromExtension(string extension)
	{
		CompilerInfo compilerInfoForExtension = GetCompilerInfoForExtension(extension);
		if (compilerInfoForExtension == null)
		{
			return null;
		}
		string[] languages = compilerInfoForExtension.GetLanguages();
		if (languages != null && languages.Length != 0)
		{
			return languages[0];
		}
		return null;
	}

	public Compiler Get(int index)
	{
		return (Compiler)BaseGet(index);
	}

	public Compiler Get(string language)
	{
		return (Compiler)BaseGet(language);
	}

	protected override object GetElementKey(ConfigurationElement element)
	{
		return ((Compiler)element).Language;
	}

	public string GetKey(int index)
	{
		return (string)BaseGetKey(index);
	}
}
