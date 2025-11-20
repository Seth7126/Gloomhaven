using System.Collections.Specialized;
using System.Configuration;
using System.Xml;

namespace System.Data.Common;

/// <summary>This class can be used by any provider to support a provider-specific configuration section.</summary>
/// <filterpriority>2</filterpriority>
public class DbProviderConfigurationHandler : IConfigurationSectionHandler
{
	private sealed class DbProviderDictionarySectionHandler
	{
		internal static NameValueCollection CreateStatic(NameValueCollection config, object context, XmlNode section)
		{
			if (section != null)
			{
				HandlerBase.CheckForUnrecognizedAttributes(section);
			}
			foreach (XmlNode childNode in section.ChildNodes)
			{
				if (!HandlerBase.IsIgnorableAlsoCheckForNonElement(childNode))
				{
					switch (childNode.Name)
					{
					case "add":
						HandleAdd(childNode, config);
						break;
					case "remove":
						HandleRemove(childNode, config);
						break;
					case "clear":
						HandleClear(childNode, config);
						break;
					default:
						throw ADP.ConfigUnrecognizedElement(childNode);
					}
				}
			}
			return config;
		}

		private static void HandleAdd(XmlNode child, NameValueCollection config)
		{
			HandlerBase.CheckForChildNodes(child);
			string name = RemoveAttribute(child, "name");
			string value = RemoveAttribute(child, "value");
			HandlerBase.CheckForUnrecognizedAttributes(child);
			config.Add(name, value);
		}

		private static void HandleRemove(XmlNode child, NameValueCollection config)
		{
			HandlerBase.CheckForChildNodes(child);
			string name = RemoveAttribute(child, "name");
			HandlerBase.CheckForUnrecognizedAttributes(child);
			config.Remove(name);
		}

		private static void HandleClear(XmlNode child, NameValueCollection config)
		{
			HandlerBase.CheckForChildNodes(child);
			HandlerBase.CheckForUnrecognizedAttributes(child);
			config.Clear();
		}
	}

	internal const string settings = "settings";

	/// <summary>This class can be used by any provider to support a provider-specific configuration section.</summary>
	public DbProviderConfigurationHandler()
	{
	}

	internal static NameValueCollection CloneParent(NameValueCollection parentConfig)
	{
		parentConfig = ((parentConfig != null) ? new NameValueCollection(parentConfig) : new NameValueCollection());
		return parentConfig;
	}

	/// <summary>Creates a new <see cref="System.Collections.Specialized.NameValueCollection" /> expression.</summary>
	/// <returns>The new expression.</returns>
	/// <param name="parent">This type supports the .NET Framework infrastructure and is not intended to be used directly from your code.</param>
	/// <param name="configContext">This type supports the .NET Framework infrastructure and is not intended to be used directly from your code.</param>
	/// <param name="section">This type supports the .NET Framework infrastructure and is not intended to be used directly from your code.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual object Create(object parent, object configContext, XmlNode section)
	{
		return CreateStatic(parent, configContext, section);
	}

	internal static object CreateStatic(object parent, object configContext, XmlNode section)
	{
		object obj = parent;
		if (section != null)
		{
			obj = CloneParent(parent as NameValueCollection);
			bool flag = false;
			HandlerBase.CheckForUnrecognizedAttributes(section);
			foreach (XmlNode childNode in section.ChildNodes)
			{
				if (!HandlerBase.IsIgnorableAlsoCheckForNonElement(childNode))
				{
					if (!(childNode.Name == "settings"))
					{
						throw ADP.ConfigUnrecognizedElement(childNode);
					}
					if (flag)
					{
						throw ADP.ConfigSectionsUnique("settings");
					}
					flag = true;
					DbProviderDictionarySectionHandler.CreateStatic(obj as NameValueCollection, configContext, childNode);
				}
			}
		}
		return obj;
	}

	internal static string RemoveAttribute(XmlNode node, string name)
	{
		XmlNode xmlNode = node.Attributes.RemoveNamedItem(name);
		if (xmlNode == null)
		{
			throw ADP.ConfigRequiredAttributeMissing(name, node);
		}
		string value = xmlNode.Value;
		if (value.Length == 0)
		{
			throw ADP.ConfigRequiredAttributeEmpty(name, node);
		}
		return value;
	}
}
