using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Manatee.Json.Internal;

namespace Manatee.Json;

public static class XmlExtensions
{
	private const string EncodingWithoutKeyError = "Key cannot be empty or contain whitespace characters.";

	private const string DecodingNestedArrayWithMismatchedKeysError = "The element name for items in nested arrays must match the element name of its parent.";

	private const string NestAttribute = "nest";

	private const string TypeAttribute = "type";

	private const string XmlNamespaceAttribute = "xmlns";

	public static XElement ToXElement(this JsonValue json, string key)
	{
		if (string.IsNullOrWhiteSpace(key) && json.Type != JsonValueType.Object)
		{
			throw new ArgumentException("Key cannot be empty or contain whitespace characters.");
		}
		XName xName = _GetXName(key);
		switch (json.Type)
		{
		case JsonValueType.Number:
			return new XElement(xName, json.Number);
		case JsonValueType.String:
		{
			XElement xElement = new XElement(xName, json.String);
			if (_RequiresTypeAttribute(json.String))
			{
				xElement.SetAttributeValue("type", "String");
			}
			return xElement;
		}
		case JsonValueType.Boolean:
			return new XElement(xName, json.Boolean);
		case JsonValueType.Object:
		{
			if (xName == null)
			{
				if (json.Object.Count != 1)
				{
					throw new ArgumentException("Key cannot be empty or contain whitespace characters.");
				}
				KeyValuePair<string, JsonValue> keyValuePair = json.Object.ElementAt(0);
				return keyValuePair.Value.ToXElement(keyValuePair.Key);
			}
			XElement xElement = new XElement(xName);
			{
				foreach (KeyValuePair<string, JsonValue> item in json.Object)
				{
					XElement xElement3 = item.Value.ToXElement(item.Key);
					if (item.Value.Type == JsonValueType.Array && !_ContainsAttributeList(item.Value.Array))
					{
						xElement.Add(xElement3.Elements());
					}
					else
					{
						xElement.Add(xElement3);
					}
				}
				return xElement;
			}
		}
		case JsonValueType.Array:
		{
			if (_ContainsAttributeList(json.Array))
			{
				JsonObject jsonObject = json.Array[0].Object;
				List<XAttribute> list = new List<XAttribute>();
				foreach (KeyValuePair<string, JsonValue> item2 in jsonObject)
				{
					XAttribute xAttribute = new XAttribute(_GetXName(item2.Key.Substring(1)), item2.Value.ToXElement(key).Value);
					if (xAttribute.IsNamespaceDeclaration)
					{
						XmlNamespaceRegistry.Instance.Register(xAttribute.Name.LocalName, xAttribute.Value);
					}
					list.Add(xAttribute);
				}
				XElement xElement = json.Array[1].ToXElement(key);
				{
					foreach (XAttribute item3 in list)
					{
						if (item3.IsNamespaceDeclaration)
						{
							if (item3.Name.LocalName == "xmlns")
							{
								XNamespace xNamespace = XmlNamespaceRegistry.Instance.GetNamespace("xmlns");
								xElement.Name = xNamespace + xElement.Name?.LocalName;
							}
							else
							{
								xElement.Add(item3);
							}
							XmlNamespaceRegistry.Instance.Unregister(item3.Name.LocalName);
						}
						else
						{
							xElement.Add(item3);
						}
					}
					return xElement;
				}
			}
			List<XElement> list2 = new List<XElement>();
			foreach (JsonValue item4 in json.Array)
			{
				XElement xElement = item4.ToXElement(key);
				if (item4.Type == JsonValueType.Array)
				{
					XElement xElement2 = new XElement(xName, xElement.Elements());
					xElement2.SetAttributeValue("nest", true);
					list2.Add(xElement2);
				}
				else
				{
					list2.Add(xElement);
				}
			}
			return new XElement(xName, list2);
		}
		case JsonValueType.Null:
			return new XElement(xName);
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public static JsonValue ToJson(this XElement xElement)
	{
		return new JsonObject { 
		{
			_GetNamespaceForElement(xElement) + xElement.Name.LocalName,
			_GetValue(xElement)
		} };
	}

	public static JsonValue ToJson(this IEnumerable<XElement> xElements)
	{
		JsonObject jsonObject = new JsonObject();
		foreach (XElement xElement in xElements)
		{
			XmlNamespaceRegistry.Instance.RegisterElement(xElement);
			string text = _GetNamespaceForElement(xElement) + xElement.Name.LocalName;
			JsonValue jsonValue = _GetValue(xElement);
			if (jsonObject.ContainsKey(text))
			{
				JsonValue jsonValue2 = jsonObject[text];
				XAttribute xAttribute = xElement.Attribute("nest");
				if (xAttribute != null && xAttribute.Value.ToLower() == "true")
				{
					if (jsonValue.Object.Count > 1 || (jsonValue.Object.Count != 0 && jsonValue.Object.Keys.ElementAt(0) != text))
					{
						throw new XmlException("The element name for items in nested arrays must match the element name of its parent.");
					}
					jsonValue = ((jsonValue.Object[text].Type == JsonValueType.Array) ? jsonValue.Object[text] : ((JsonValue)new JsonArray { jsonValue.Object[text] }));
				}
				if (jsonValue2.Type == JsonValueType.Array)
				{
					jsonValue2.Array.Add(jsonValue);
				}
				else
				{
					jsonObject[text] = new JsonArray { jsonValue2, jsonValue };
				}
			}
			else
			{
				jsonObject[text] = jsonValue;
			}
			XmlNamespaceRegistry.Instance.UnRegisterElement(xElement);
		}
		return jsonObject;
	}

	private static bool _RequiresTypeAttribute(string value)
	{
		string text = value.ToLower();
		switch (text)
		{
		default:
		{
			double result;
			return double.TryParse(text, out result);
		}
		case "true":
		case "false":
		case "null":
			return true;
		}
	}

	private static JsonValue _GetValue(XElement xElement)
	{
		XAttribute xAttribute = xElement.Attribute("type");
		if (xElement.HasElements)
		{
			return _AttachAttributes(xElement.Elements().ToJson(), xElement);
		}
		if (string.IsNullOrEmpty(xElement.Value) && xAttribute == null)
		{
			return _AttachAttributes(JsonValue.Null, xElement);
		}
		string value = xElement.Value;
		if (xAttribute != null && xAttribute.Value.ToLower() == "string")
		{
			return _AttachAttributes(value, xElement);
		}
		return _AttachAttributes(_ParseValue(value), xElement);
	}

	private static JsonValue _AttachAttributes(JsonValue json, XElement xElement)
	{
		List<XAttribute> list = (from a in xElement.Attributes()
			where a.Name != "nest" && a.Name != "type"
			select a).ToList();
		if (list.Count == 0)
		{
			return json;
		}
		JsonObject jsonObject = new JsonObject();
		foreach (XAttribute item in list)
		{
			string text = ((item.IsNamespaceDeclaration && item.Name.LocalName != "xmlns") ? ("xmlns:" + item.Name.LocalName) : (_GetNamespaceForElement(xElement, item.Name.NamespaceName) + item.Name.LocalName));
			jsonObject.Add("-" + text, _ParseValue(item.Value));
		}
		return new JsonArray { jsonObject, json };
	}

	private static JsonValue _ParseValue(string value)
	{
		if (bool.TryParse(value, out var result))
		{
			return result;
		}
		if (double.TryParse(value, out var result2))
		{
			return result2;
		}
		return value;
	}

	private static bool _ContainsAttributeList(JsonArray json)
	{
		if (json.Count != 2)
		{
			return false;
		}
		if (json[0].Type != JsonValueType.Object)
		{
			return false;
		}
		return json[0].Object.Keys.All((string key) => key[0] == '-');
	}

	private static string _GetNamespaceForElement(XElement xElement, string? space = null)
	{
		string text = space ?? xElement.Name.NamespaceName;
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		for (XElement xElement2 = xElement; xElement2 != null; xElement2 = xElement2.Parent)
		{
			if (XmlNamespaceRegistry.Instance.ElementDefinesNamespace(xElement2, text))
			{
				if (!(XmlNamespaceRegistry.Instance.GetLabel(xElement2, text) == "xmlns"))
				{
					return XmlNamespaceRegistry.Instance.GetLabel(xElement2, text) + ":";
				}
				return string.Empty;
			}
		}
		return string.Empty;
	}

	private static XName? _GetXName(string key)
	{
		if (key == null)
		{
			return null;
		}
		if (!key.Contains(":") && key != "xmlns")
		{
			return key;
		}
		if (key.Contains(":"))
		{
			string text = key.Substring(0, key.IndexOf(':'));
			string text2 = key.Substring(text.Length + 1);
			string text3 = XmlNamespaceRegistry.Instance.GetNamespace((text == "xmlns") ? text2 : text);
			return ((text3 != null) ? ((XNamespace)text3) : XNamespace.Xmlns) + text2;
		}
		return XName.Get("xmlns", string.Empty);
	}
}
