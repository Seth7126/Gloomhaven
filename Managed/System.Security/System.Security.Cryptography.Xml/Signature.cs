using System.Collections;
using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the &lt;Signature&gt; element of an XML signature.</summary>
public class Signature
{
	private string _id;

	private SignedInfo _signedInfo;

	private byte[] _signatureValue;

	private string _signatureValueId;

	private KeyInfo _keyInfo;

	private IList _embeddedObjects;

	private CanonicalXmlNodeList _referencedItems;

	private SignedXml _signedXml;

	internal SignedXml SignedXml
	{
		get
		{
			return _signedXml;
		}
		set
		{
			_signedXml = value;
		}
	}

	/// <summary>Gets or sets the ID of the current <see cref="T:System.Security.Cryptography.Xml.Signature" />.</summary>
	/// <returns>The ID of the current <see cref="T:System.Security.Cryptography.Xml.Signature" />. The default is null.</returns>
	public string Id
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> of the current <see cref="T:System.Security.Cryptography.Xml.Signature" />.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> of the current <see cref="T:System.Security.Cryptography.Xml.Signature" />.</returns>
	public SignedInfo SignedInfo
	{
		get
		{
			return _signedInfo;
		}
		set
		{
			_signedInfo = value;
			if (SignedXml != null && _signedInfo != null)
			{
				_signedInfo.SignedXml = SignedXml;
			}
		}
	}

	/// <summary>Gets or sets the value of the digital signature.</summary>
	/// <returns>A byte array that contains the value of the digital signature.</returns>
	public byte[] SignatureValue
	{
		get
		{
			return _signatureValue;
		}
		set
		{
			_signatureValue = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> of the current <see cref="T:System.Security.Cryptography.Xml.Signature" />.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> of the current <see cref="T:System.Security.Cryptography.Xml.Signature" />.</returns>
	public KeyInfo KeyInfo
	{
		get
		{
			if (_keyInfo == null)
			{
				_keyInfo = new KeyInfo();
			}
			return _keyInfo;
		}
		set
		{
			_keyInfo = value;
		}
	}

	/// <summary>Gets or sets a list of objects to be signed.</summary>
	/// <returns>A list of objects to be signed.</returns>
	public IList ObjectList
	{
		get
		{
			return _embeddedObjects;
		}
		set
		{
			_embeddedObjects = value;
		}
	}

	internal CanonicalXmlNodeList ReferencedItems => _referencedItems;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.Signature" /> class.</summary>
	public Signature()
	{
		_embeddedObjects = new ArrayList();
		_referencedItems = new CanonicalXmlNodeList();
	}

	/// <summary>Returns the XML representation of the <see cref="T:System.Security.Cryptography.Xml.Signature" />.</summary>
	/// <returns>The XML representation of the <see cref="T:System.Security.Cryptography.Xml.Signature" />.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.Xml.Signature.SignedInfo" /> property is null.-or- The <see cref="P:System.Security.Cryptography.Xml.Signature.SignatureValue" /> property is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public XmlElement GetXml()
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.PreserveWhitespace = true;
		return GetXml(xmlDocument);
	}

	internal XmlElement GetXml(XmlDocument document)
	{
		XmlElement xmlElement = document.CreateElement("Signature", "http://www.w3.org/2000/09/xmldsig#");
		if (!string.IsNullOrEmpty(_id))
		{
			xmlElement.SetAttribute("Id", _id);
		}
		if (_signedInfo == null)
		{
			throw new CryptographicException("Signature requires a SignedInfo.");
		}
		xmlElement.AppendChild(_signedInfo.GetXml(document));
		if (_signatureValue == null)
		{
			throw new CryptographicException("Signature requires a SignatureValue.");
		}
		XmlElement xmlElement2 = document.CreateElement("SignatureValue", "http://www.w3.org/2000/09/xmldsig#");
		xmlElement2.AppendChild(document.CreateTextNode(Convert.ToBase64String(_signatureValue)));
		if (!string.IsNullOrEmpty(_signatureValueId))
		{
			xmlElement2.SetAttribute("Id", _signatureValueId);
		}
		xmlElement.AppendChild(xmlElement2);
		if (KeyInfo.Count > 0)
		{
			xmlElement.AppendChild(KeyInfo.GetXml(document));
		}
		foreach (object embeddedObject in _embeddedObjects)
		{
			if (embeddedObject is DataObject dataObject)
			{
				xmlElement.AppendChild(dataObject.GetXml(document));
			}
		}
		return xmlElement;
	}

	/// <summary>Loads a <see cref="T:System.Security.Cryptography.Xml.Signature" /> state from an XML element.</summary>
	/// <param name="value">The XML element from which to load the <see cref="T:System.Security.Cryptography.Xml.Signature" /> state. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="value" /> parameter does not contain a valid <see cref="P:System.Security.Cryptography.Xml.Signature.SignatureValue" />.-or- The <paramref name="value" /> parameter does not contain a valid <see cref="P:System.Security.Cryptography.Xml.Signature.SignedInfo" />. </exception>
	public void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!value.LocalName.Equals("Signature"))
		{
			throw new CryptographicException("Malformed element {0}.", "Signature");
		}
		_id = Utils.GetAttribute(value, "Id", "http://www.w3.org/2000/09/xmldsig#");
		if (!Utils.VerifyAttributes(value, "Id"))
		{
			throw new CryptographicException("Malformed element {0}.", "Signature");
		}
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(value.OwnerDocument.NameTable);
		xmlNamespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
		int num = 0;
		XmlNodeList xmlNodeList = value.SelectNodes("ds:SignedInfo", xmlNamespaceManager);
		if (xmlNodeList == null || xmlNodeList.Count == 0 || xmlNodeList.Count > 1)
		{
			throw new CryptographicException("Malformed element {0}.", "SignedInfo");
		}
		XmlElement value2 = xmlNodeList[0] as XmlElement;
		num += xmlNodeList.Count;
		SignedInfo = new SignedInfo();
		SignedInfo.LoadXml(value2);
		XmlNodeList xmlNodeList2 = value.SelectNodes("ds:SignatureValue", xmlNamespaceManager);
		if (xmlNodeList2 == null || xmlNodeList2.Count == 0 || xmlNodeList2.Count > 1)
		{
			throw new CryptographicException("Malformed element {0}.", "SignatureValue");
		}
		XmlElement xmlElement = xmlNodeList2[0] as XmlElement;
		num += xmlNodeList2.Count;
		_signatureValue = Convert.FromBase64String(Utils.DiscardWhiteSpaces(xmlElement.InnerText));
		_signatureValueId = Utils.GetAttribute(xmlElement, "Id", "http://www.w3.org/2000/09/xmldsig#");
		if (!Utils.VerifyAttributes(xmlElement, "Id"))
		{
			throw new CryptographicException("Malformed element {0}.", "SignatureValue");
		}
		XmlNodeList xmlNodeList3 = value.SelectNodes("ds:KeyInfo", xmlNamespaceManager);
		_keyInfo = new KeyInfo();
		if (xmlNodeList3 != null)
		{
			if (xmlNodeList3.Count > 1)
			{
				throw new CryptographicException("Malformed element {0}.", "KeyInfo");
			}
			foreach (XmlNode item in xmlNodeList3)
			{
				if (item is XmlElement value3)
				{
					_keyInfo.LoadXml(value3);
				}
			}
			num += xmlNodeList3.Count;
		}
		XmlNodeList xmlNodeList4 = value.SelectNodes("ds:Object", xmlNamespaceManager);
		_embeddedObjects.Clear();
		if (xmlNodeList4 != null)
		{
			foreach (XmlNode item2 in xmlNodeList4)
			{
				if (item2 is XmlElement value4)
				{
					DataObject dataObject = new DataObject();
					dataObject.LoadXml(value4);
					_embeddedObjects.Add(dataObject);
				}
			}
			num += xmlNodeList4.Count;
		}
		XmlNodeList xmlNodeList5 = value.SelectNodes("//*[@Id]", xmlNamespaceManager);
		if (xmlNodeList5 != null)
		{
			foreach (XmlNode item3 in xmlNodeList5)
			{
				_referencedItems.Add(item3);
			}
		}
		if (value.SelectNodes("*").Count != num)
		{
			throw new CryptographicException("Malformed element {0}.", "Signature");
		}
	}

	/// <summary>Adds a <see cref="T:System.Security.Cryptography.Xml.DataObject" /> to the list of objects to be signed.</summary>
	/// <param name="dataObject">The <see cref="T:System.Security.Cryptography.Xml.DataObject" /> to be added to the list of objects to be signed. </param>
	public void AddObject(DataObject dataObject)
	{
		_embeddedObjects.Add(dataObject);
	}
}
