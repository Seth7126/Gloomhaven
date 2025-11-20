using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Manatee.Json.Internal;

internal class XmlNamespaceRegistry
{
	private class XmlNamespacePair
	{
		public string Namespace { get; }

		public string Label { get; }

		public XmlNamespacePair(string @namespace, string label)
		{
			Namespace = @namespace;
			Label = label;
		}
	}

	[ThreadStatic]
	private static XmlNamespaceRegistry? _instance;

	private readonly Dictionary<XElement, List<XmlNamespacePair>> _registry = new Dictionary<XElement, List<XmlNamespacePair>>();

	private readonly Dictionary<string, Stack<string>> _stack = new Dictionary<string, Stack<string>>();

	public static XmlNamespaceRegistry Instance => _instance ?? (_instance = new XmlNamespaceRegistry());

	private XmlNamespaceRegistry()
	{
	}

	public void RegisterElement(XElement element)
	{
		if (_registry.ContainsKey(element))
		{
			_registry.Remove(element);
		}
		IEnumerable<XAttribute> source = from a in element.Attributes()
			where a.IsNamespaceDeclaration
			select a;
		_registry.Add(element, new List<XmlNamespacePair>(source.Select((XAttribute a) => new XmlNamespacePair(a.Value, a.Name.LocalName))));
	}

	public void UnRegisterElement(XElement element)
	{
		_registry.Remove(element);
	}

	public bool ElementDefinesNamespace(XElement element, string space)
	{
		if (_registry.TryGetValue(element, out List<XmlNamespacePair> value))
		{
			return value.Any((XmlNamespacePair pair) => pair.Namespace == space);
		}
		return false;
	}

	public string GetLabel(XElement element, string space)
	{
		return _registry[element].First((XmlNamespacePair pair) => pair.Namespace == space).Label;
	}

	public void Register(string label, string space)
	{
		if (!_stack.TryGetValue(label, out Stack<string> value))
		{
			value = new Stack<string>();
			_stack.Add(label, value);
		}
		value.Push(space);
	}

	public void Unregister(string label)
	{
		_stack[label].Pop();
		if (_stack[label].Count == 0)
		{
			_stack.Remove(label);
		}
	}

	public string? GetNamespace(string label)
	{
		if (!_stack.TryGetValue(label, out Stack<string> value))
		{
			return null;
		}
		if (!_stack.TryGetValue(label, out value) || value.Count == 0)
		{
			return null;
		}
		return value.Peek();
	}
}
