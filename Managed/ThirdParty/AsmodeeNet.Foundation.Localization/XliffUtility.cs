using System.Xml;

namespace AsmodeeNet.Foundation.Localization;

public static class XliffUtility
{
	public static LocalizationManager.Language GetXliffTargetLang(string xliffFilePath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(xliffFilePath);
		return LanguageHelper.LanguageFromXsdLanguage(xmlDocument.GetElementsByTagName("file")[0].Attributes.GetNamedItem("target-language").Value);
	}

	public static LocalizationManager.Language GetXliffTargetLangFromXml(string xml)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xml);
		return LanguageHelper.LanguageFromXsdLanguage(xmlDocument.GetElementsByTagName("file")[0].Attributes.GetNamedItem("target-language").Value);
	}
}
