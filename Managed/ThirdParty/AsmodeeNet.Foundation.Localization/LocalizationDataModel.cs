using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using AsmodeeNet.Utils;
using UnityEngine;

namespace AsmodeeNet.Foundation.Localization;

public class LocalizationDataModel
{
	private const string _kModuleName = "LocalizationDataModel";

	private XmlDocument _xmlDocument;

	private List<KeyValuePair<string, string>> _keyToTranslation = new List<KeyValuePair<string, string>>();

	public LocalizationManager.Language TargetLanguage { get; private set; }

	public Dictionary<string, string> KeyToTranslation => _keyToTranslation.ToDictionary((KeyValuePair<string, string> x) => x.Key, (KeyValuePair<string, string> x) => x.Value);

	public List<string> KeyNeedingTranslation { get; private set; } = new List<string>();

	public string GetFileName => Path.GetFileName(_xmlDocument.BaseURI);

	public string GetUri => _xmlDocument.BaseURI;

	private LocalizationDataModel()
	{
	}

	public static LocalizationDataModel CreateModelFromTextAsset(TextAsset source)
	{
		LocalizationDataModel localizationDataModel = new LocalizationDataModel();
		localizationDataModel._xmlDocument = new XmlDocument();
		localizationDataModel._xmlDocument.LoadXml(source.text);
		localizationDataModel.TargetLanguage = _GetFileTargetLanguage(localizationDataModel);
		if (localizationDataModel.TargetLanguage == LocalizationManager.Language.unknown)
		{
			return null;
		}
		localizationDataModel.Parse();
		return localizationDataModel;
	}

	public static LocalizationDataModel CreateModelFromTextFile(string path)
	{
		LocalizationDataModel localizationDataModel = new LocalizationDataModel();
		localizationDataModel._xmlDocument = new XmlDocument();
		localizationDataModel._xmlDocument.Load(path);
		localizationDataModel.TargetLanguage = _GetFileTargetLanguage(localizationDataModel);
		if (localizationDataModel.TargetLanguage == LocalizationManager.Language.unknown)
		{
			return null;
		}
		localizationDataModel.Parse();
		return localizationDataModel;
	}

	public static LocalizationDataModel CreateModelFromString(string content)
	{
		LocalizationDataModel localizationDataModel = new LocalizationDataModel();
		localizationDataModel._xmlDocument = new XmlDocument();
		localizationDataModel._xmlDocument.LoadXml(content);
		localizationDataModel.TargetLanguage = _GetFileTargetLanguage(localizationDataModel);
		if (localizationDataModel.TargetLanguage == LocalizationManager.Language.unknown)
		{
			return null;
		}
		localizationDataModel.Parse();
		return localizationDataModel;
	}

	public static LocalizationDataModel CreateModelByMerging2Files(string importedFile, string previousFile)
	{
		LocalizationDataModel localizationDataModel = CreateModelFromTextFile(importedFile);
		LocalizationDataModel localizationDataModel2 = CreateModelFromTextFile(previousFile);
		if (localizationDataModel == null || localizationDataModel2 == null)
		{
			return null;
		}
		LocalizationDataModel localizationDataModel3 = new LocalizationDataModel();
		localizationDataModel3._xmlDocument = localizationDataModel._xmlDocument.Clone() as XmlDocument;
		localizationDataModel3.TargetLanguage = localizationDataModel.TargetLanguage;
		if (localizationDataModel.TargetLanguage != localizationDataModel2.TargetLanguage)
		{
			return null;
		}
		bool flag = false;
		foreach (XmlNode item in localizationDataModel._xmlDocument.GetElementsByTagName("trans-unit"))
		{
			flag = false;
			XmlNode xmlNode = _GetSourceNodeFromTransUnit(item);
			foreach (XmlNode item2 in localizationDataModel._xmlDocument.GetElementsByTagName("trans-unit"))
			{
				_ = item2;
				if (_GetSourceNodeFromTransUnit(item).InnerText == xmlNode.InnerText)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				localizationDataModel3.AddTranslation(localizationDataModel.TargetLanguage, xmlNode.InnerText, _GetTargetNodeFromTransUnit(item).InnerText);
			}
		}
		return localizationDataModel3;
	}

	public Dictionary<string, string> Parse()
	{
		if (TargetLanguage == LocalizationManager.Language.unknown)
		{
			return null;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		KeyNeedingTranslation.Clear();
		XmlNodeList elementsByTagName = _xmlDocument.GetElementsByTagName("trans-unit");
		for (int i = 0; i < elementsByTagName.Count; i++)
		{
			XmlNode xmlNode = _GetSourceNodeFromTransUnit(elementsByTagName[i]);
			XmlNode xmlNode2 = _GetTargetNodeFromTransUnit(elementsByTagName[i]);
			if (xmlNode == null)
			{
				continue;
			}
			if (xmlNode2 == null)
			{
				AsmoLogger.Warning("LocalizationDataModel", $"No traduction have been found for the key: '{xmlNode.InnerText}' for the language: '{TargetLanguage}'. Key will be ignored at runtime.\nFile: {_xmlDocument.BaseURI}");
				continue;
			}
			XmlNode namedItem = xmlNode2.Attributes.GetNamedItem("xml:lang");
			if (namedItem == null)
			{
				AsmoLogger.Warning("LocalizationDataModel", "The key: '" + xmlNode.InnerText + "' has a translation but not a target language associated. Key will be ignored at runtime\nFile: " + _xmlDocument.BaseURI);
				continue;
			}
			if (namedItem.InnerText != TargetLanguage.ToXsdLanguage())
			{
				XmlNode xmlNode3 = xmlNode2.Attributes["state"];
				if (xmlNode3 == null || (!(xmlNode3.InnerText == "needs-translation") && !(xmlNode3.InnerText == "needs-review-translation")))
				{
					AsmoLogger.Warning("LocalizationDataModel", $"The key: '{xmlNode.InnerText}' is defined for the following language: '{namedItem.InnerText}' which does not match the target language of the file : '{TargetLanguage}'. Key will be ignored at runtime\nFile: {_xmlDocument.BaseURI}");
					continue;
				}
				AsmoLogger.Warning("LocalizationDataModel", "The key: '" + xmlNode.InnerText + "' is still waiting for being translated. Key will be ignored at runtime\nFile: " + _xmlDocument.BaseURI);
				if (Application.isPlaying)
				{
					continue;
				}
				KeyNeedingTranslation.Add(xmlNode.InnerText);
			}
			if (dictionary.ContainsKey(xmlNode.InnerText))
			{
				AsmoLogger.Warning("LocalizationDataModel", "The key: " + xmlNode.InnerText + " is present more than one time in the file. The previous translation will be overridden.");
			}
			else
			{
				dictionary[xmlNode.InnerText] = xmlNode2.InnerText;
			}
		}
		_keyToTranslation = dictionary.Select((KeyValuePair<string, string> x) => new KeyValuePair<string, string>(x.Key, x.Value)).ToList();
		return dictionary;
	}

	public bool AddTranslation(LocalizationManager.Language targetLang, string key, string val)
	{
		if (TargetLanguage != targetLang)
		{
			AsmoLogger.Warning("LocalizationDataModel", "Unable to create the translation for the keyword: \"" + key + "\": the chosen _targetLang does not match with the file target lang");
			return false;
		}
		XmlNode xmlNode = _xmlDocument.GetElementsByTagName("body")[0];
		XmlNode xmlNode2;
		if ((xmlNode2 = _GetTransUnitFromKey(key)) != null)
		{
			XmlNode xmlNode3;
			if ((xmlNode3 = _GetTargetNodeFromTransUnit(xmlNode2)) != null)
			{
				AsmoLogger.Warning("LocalizationDataModel", "Unable to create the translation for the keyword : \"" + key + "\" : this keyword already exists");
				return false;
			}
			xmlNode3 = _CreateTargetNode(val, targetLang);
			xmlNode2.AppendChild(xmlNode3);
		}
		else
		{
			xmlNode2 = _xmlDocument.CreateElement("trans-unit");
			XmlAttribute xmlAttribute = _xmlDocument.CreateAttribute("xmlns");
			xmlAttribute.InnerText = _xmlDocument.DocumentElement.NamespaceURI;
			xmlNode2.Attributes.Append(xmlAttribute);
			XmlAttribute xmlAttribute2 = _xmlDocument.CreateAttribute("id");
			xmlAttribute2.InnerText = key.GetHashCode().ToString();
			xmlNode2.Attributes.Append(xmlAttribute2);
			XmlNode xmlNode4 = _xmlDocument.CreateElement("source");
			xmlNode4.InnerText = key;
			XmlNode xmlNode3 = _CreateTargetNode(val, targetLang);
			xmlNode2.AppendChild(xmlNode4);
			xmlNode2.AppendChild(xmlNode3);
			xmlNode.AppendChild(xmlNode2);
		}
		_keyToTranslation.Add(new KeyValuePair<string, string>(key, val));
		if (!Application.isPlaying)
		{
			KeyNeedingTranslation.Remove(key);
		}
		return true;
	}

	public bool UpdateTargetLangTranslation(LocalizationManager.Language targetLang, string key, string val)
	{
		if (TargetLanguage != targetLang)
		{
			AsmoLogger.Warning("LocalizationDataModel", "Unable to update the translation for the keyword : \"" + key + "\" : the chosen _targetLang does not match with the file target lang");
			return false;
		}
		XmlNode xmlNode = _GetTransUnitFromKey(key);
		if (xmlNode == null)
		{
			AsmoLogger.Warning("LocalizationDataModel", "Unable to update the translation for the keyword : \"" + key + "\" : this keyword does not exist yet");
			return false;
		}
		XmlNode xmlNode2 = _GetTargetNodeFromTransUnit(xmlNode);
		if (xmlNode2 == null)
		{
			AsmoLogger.Warning("LocalizationDataModel", "Unable to update the translation for the keyword : \"" + key + "\" : this keyword has no target field and you try to update it");
			return false;
		}
		xmlNode.RemoveChild(xmlNode2);
		xmlNode2 = _CreateTargetNode(val, targetLang);
		xmlNode.AppendChild(xmlNode2);
		KeyValuePair<string, string> item = _keyToTranslation.SingleOrDefault((KeyValuePair<string, string> x) => x.Key == key);
		_keyToTranslation[_keyToTranslation.IndexOf(item)] = new KeyValuePair<string, string>(key, val);
		if (!Application.isPlaying)
		{
			KeyNeedingTranslation.Remove(key);
		}
		return true;
	}

	public bool RemoveTranslation(string key)
	{
		XmlNode xmlNode = _GetTransUnitFromKey(key);
		if (xmlNode == null)
		{
			AsmoLogger.Warning("LocalizationDataModel", "Unable to remove the translation associated with the keyword : \"" + key + "\" : this keyword does not exist");
			return false;
		}
		xmlNode.ParentNode.RemoveChild(xmlNode);
		_keyToTranslation.Remove(_keyToTranslation.Single((KeyValuePair<string, string> x) => x.Key == key));
		if (!Application.isPlaying)
		{
			KeyNeedingTranslation.Remove(key);
		}
		return true;
	}

	public bool UpdateKey(string oldKey, string newKey)
	{
		XmlNode xmlNode = _GetTransUnitFromKey(oldKey);
		if (xmlNode == null)
		{
			AsmoLogger.Warning("LocalizationDataModel", "Unable to update the translation associated with the keyword : \"" + oldKey + "\" : this keyword does not exist");
			return false;
		}
		XmlNode xmlNode2 = _GetSourceNodeFromTransUnit(xmlNode);
		if (xmlNode2 != null)
		{
			xmlNode2.InnerText = newKey;
		}
		XmlAttribute xmlAttribute = xmlNode.Attributes["id"];
		if (xmlAttribute != null)
		{
			xmlAttribute.InnerText = newKey.GetHashCode().ToString();
		}
		KeyValuePair<string, string> item = _keyToTranslation.SingleOrDefault((KeyValuePair<string, string> x) => x.Key == oldKey);
		KeyValuePair<string, string> value = new KeyValuePair<string, string>(newKey, item.Value);
		_keyToTranslation[_keyToTranslation.IndexOf(item)] = value;
		if (!Application.isPlaying)
		{
			KeyNeedingTranslation.Remove(newKey);
		}
		return true;
	}

	public void MoveKeyUp(string key)
	{
		KeyValuePair<string, string> item = _keyToTranslation.SingleOrDefault((KeyValuePair<string, string> x) => x.Key == key);
		int num = _keyToTranslation.IndexOf(item);
		_MoveKey(num, num - 1);
	}

	public void MoveKeyDown(string key)
	{
		KeyValuePair<string, string> item = _keyToTranslation.SingleOrDefault((KeyValuePair<string, string> x) => x.Key == key);
		int num = _keyToTranslation.IndexOf(item);
		_MoveKey(num, num + 1);
	}

	public void SortKeys()
	{
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>(_keyToTranslation);
		list.Sort((KeyValuePair<string, string> kTT1, KeyValuePair<string, string> kTT2) => string.Compare(kTT1.Key, kTT2.Key, ignoreCase: true));
		for (int num = 0; num < list.Count; num++)
		{
			string key = list[num].Key;
			KeyValuePair<string, string> item = _keyToTranslation.SingleOrDefault((KeyValuePair<string, string> x) => x.Key == key);
			int sourceIdx = _keyToTranslation.IndexOf(item);
			_MoveKey(sourceIdx, num);
		}
	}

	private void _MoveKey(int sourceIdx, int destinationIdx)
	{
		if (sourceIdx != destinationIdx)
		{
			if (sourceIdx < 0 || destinationIdx < 0 || sourceIdx >= _keyToTranslation.Count || destinationIdx >= _keyToTranslation.Count)
			{
				AsmoLogger.Warning("LocalizationDataModel", $"Index out of bounds sourceIdx: {sourceIdx} destinationIdx: {destinationIdx} [0..{_keyToTranslation.Count - 1}]");
			}
			string key = _keyToTranslation[sourceIdx].Key;
			XmlNode xmlNode = _GetTransUnitFromKey(key);
			string key2 = _keyToTranslation[destinationIdx].Key;
			XmlNode xmlNode2 = _GetTransUnitFromKey(key2);
			AsmoLogger.Warning("LocalizationDataModel", $"{sourceIdx} {key} - {destinationIdx} {key2}");
			xmlNode.ParentNode.RemoveChild(xmlNode);
			xmlNode2.ParentNode.InsertBefore(xmlNode, xmlNode2);
			KeyValuePair<string, string> item = _keyToTranslation[sourceIdx];
			_keyToTranslation.RemoveAt(sourceIdx);
			_keyToTranslation.Insert(destinationIdx, item);
		}
	}

	public void SaveModification(string path = null)
	{
		if (TargetLanguage != LocalizationManager.Language.unknown)
		{
			_xmlDocument.Save(path ?? new Uri(_xmlDocument.BaseURI).LocalPath);
		}
	}

	public void ExportFile(string path, LocalizationManager.Language exportLanguage, Dictionary<string, string> alreadyTranslatedKeys)
	{
		XmlDocument xmlDocument = _xmlDocument.CloneNode(deep: true) as XmlDocument;
		xmlDocument.GetElementsByTagName("file")[0].Attributes["target-language"].Value = exportLanguage.ToXsdLanguage();
		foreach (XmlNode item in xmlDocument.GetElementsByTagName("trans-unit"))
		{
			XmlNode xmlNode2 = _GetSourceNodeFromTransUnit(item);
			if (xmlNode2 == null)
			{
				continue;
			}
			if (alreadyTranslatedKeys.ContainsKey(xmlNode2.InnerText))
			{
				XmlNode xmlNode3 = _GetTargetNodeFromTransUnit(item);
				if (xmlNode3 != null)
				{
					xmlNode3.InnerText = alreadyTranslatedKeys[xmlNode2.InnerText];
					xmlNode3.Attributes["xml:lang"].Value = exportLanguage.ToXsdLanguage();
				}
				else
				{
					item.AppendChild(_CreateTargetNode(alreadyTranslatedKeys[xmlNode2.InnerText], exportLanguage));
				}
				continue;
			}
			XmlNode xmlNode4 = _GetTargetNodeFromTransUnit(item);
			XmlAttribute xmlAttribute = xmlNode4.Attributes["state"];
			if (xmlAttribute == null)
			{
				xmlAttribute = xmlDocument.CreateAttribute("state");
				xmlNode4.Attributes.Append(xmlAttribute);
			}
			xmlNode4.Attributes["xml:lang"].Value = exportLanguage.ToXsdLanguage();
			xmlAttribute.Value = "needs-translation";
		}
		xmlDocument.Save(path);
	}

	private static LocalizationManager.Language _GetFileTargetLanguage(LocalizationDataModel dm)
	{
		string value = dm._xmlDocument.GetElementsByTagName("file")[0].Attributes.GetNamedItem("target-language").Value;
		LocalizationManager.Language num = LanguageHelper.LanguageFromXsdLanguage(value);
		if (num == LocalizationManager.Language.unknown)
		{
			AsmoLogger.Error("LocalizationDataModel", "target-language " + value + " is not supported. Unable to use this file: " + dm._xmlDocument.BaseURI);
		}
		return num;
	}

	private XmlNode _GetTransUnitFromKey(string key)
	{
		XmlNodeList elementsByTagName = _xmlDocument.GetElementsByTagName("trans-unit");
		for (int i = 0; i < elementsByTagName.Count; i++)
		{
			for (int j = 0; j < elementsByTagName[i].ChildNodes.Count; j++)
			{
				if (elementsByTagName[i].ChildNodes[j].Name == "source" && elementsByTagName[i].ChildNodes[j].InnerText == key)
				{
					return elementsByTagName[i];
				}
			}
		}
		return null;
	}

	private static XmlNode _GetSourceNodeFromTransUnit(XmlNode transUnit)
	{
		foreach (XmlNode childNode in transUnit.ChildNodes)
		{
			if (childNode.Name == "source")
			{
				return childNode;
			}
		}
		return null;
	}

	private static XmlNode _GetTargetNodeFromTransUnit(XmlNode transUnit)
	{
		foreach (XmlNode childNode in transUnit.ChildNodes)
		{
			if (childNode.Name == "target")
			{
				return childNode;
			}
		}
		return null;
	}

	private XmlNode _CreateTargetNode(string val, LocalizationManager.Language _targetLang)
	{
		XmlElement xmlElement = _xmlDocument.CreateElement("target", _xmlDocument.DocumentElement.NamespaceURI);
		xmlElement.InnerText = val;
		XmlAttribute xmlAttribute = _xmlDocument.CreateAttribute("xml:lang");
		xmlAttribute.InnerText = _targetLang.ToXsdLanguage();
		xmlElement.Attributes.Append(xmlAttribute);
		return xmlElement;
	}
}
