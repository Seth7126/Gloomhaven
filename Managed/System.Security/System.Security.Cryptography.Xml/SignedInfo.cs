using System.Collections;
using System.Globalization;
using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Contains information about the canonicalization algorithm and signature algorithm used for the XML signature.</summary>
public class SignedInfo : ICollection, IEnumerable
{
	private string _id;

	private string _canonicalizationMethod;

	private string _signatureMethod;

	private string _signatureLength;

	private ArrayList _references;

	private XmlElement _cachedXml;

	private SignedXml _signedXml;

	private Transform _canonicalizationMethodTransform;

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

	/// <summary>Gets the number of references in the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</summary>
	/// <returns>The number of references in the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</returns>
	/// <exception cref="T:System.NotSupportedException">This property is not supported. </exception>
	public int Count
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets a value that indicates whether the collection is read-only.</summary>
	/// <returns>true if the collection is read-only; otherwise, false.</returns>
	/// <exception cref="T:System.NotSupportedException">This property is not supported. </exception>
	public bool IsReadOnly
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets a value that indicates whether the collection is synchronized.</summary>
	/// <returns>true if the collection is synchronized; otherwise, false.</returns>
	/// <exception cref="T:System.NotSupportedException">This property is not supported. </exception>
	public bool IsSynchronized
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets an object to use for synchronization.</summary>
	/// <returns>An object to use for synchronization.</returns>
	/// <exception cref="T:System.NotSupportedException">This property is not supported. </exception>
	public object SyncRoot
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets or sets the ID of the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</summary>
	/// <returns>The ID of the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</returns>
	public string Id
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
			_cachedXml = null;
		}
	}

	/// <summary>Gets or sets the canonicalization algorithm that is used before signing for the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</summary>
	/// <returns>The canonicalization algorithm used before signing for the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</returns>
	public string CanonicalizationMethod
	{
		get
		{
			if (_canonicalizationMethod == null)
			{
				return "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
			}
			return _canonicalizationMethod;
		}
		set
		{
			_canonicalizationMethod = value;
			_cachedXml = null;
		}
	}

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.Xml.Transform" /> object used for canonicalization.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Xml.Transform" /> object used for canonicalization.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">
	///   <see cref="T:System.Security.Cryptography.Xml.Transform" /> is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Transform CanonicalizationMethodObject
	{
		get
		{
			if (_canonicalizationMethodTransform == null)
			{
				_canonicalizationMethodTransform = CryptoHelpers.CreateFromName<Transform>(CanonicalizationMethod);
				if (_canonicalizationMethodTransform == null)
				{
					throw new CryptographicException(string.Format(CultureInfo.CurrentCulture, "Could not create the XML transformation identified by the URI {0}.", CanonicalizationMethod));
				}
				_canonicalizationMethodTransform.SignedXml = SignedXml;
				_canonicalizationMethodTransform.Reference = null;
			}
			return _canonicalizationMethodTransform;
		}
	}

	/// <summary>Gets or sets the name of the algorithm used for signature generation and validation for the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</summary>
	/// <returns>The name of the algorithm used for signature generation and validation for the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</returns>
	public string SignatureMethod
	{
		get
		{
			return _signatureMethod;
		}
		set
		{
			_signatureMethod = value;
			_cachedXml = null;
		}
	}

	/// <summary>Gets or sets the length of the signature for the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</summary>
	/// <returns>The length of the signature for the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</returns>
	public string SignatureLength
	{
		get
		{
			return _signatureLength;
		}
		set
		{
			_signatureLength = value;
			_cachedXml = null;
		}
	}

	/// <summary>Gets a list of the <see cref="T:System.Security.Cryptography.Xml.Reference" /> objects of the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</summary>
	/// <returns>A list of the <see cref="T:System.Security.Cryptography.Xml.Reference" /> elements of the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</returns>
	public ArrayList References => _references;

	internal bool CacheValid
	{
		get
		{
			if (_cachedXml == null)
			{
				return false;
			}
			foreach (Reference reference in References)
			{
				if (!reference.CacheValid)
				{
					return false;
				}
			}
			return true;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> class.</summary>
	public SignedInfo()
	{
		_references = new ArrayList();
	}

	/// <summary>Returns an enumerator that iterates through the collection of references.</summary>
	/// <returns>An enumerator that iterates through the collection of references.</returns>
	/// <exception cref="T:System.NotSupportedException">This method is not supported. </exception>
	public IEnumerator GetEnumerator()
	{
		throw new NotSupportedException();
	}

	/// <summary>Copies the elements of this instance into an <see cref="T:System.Array" /> object, starting at a specified index in the array.</summary>
	/// <param name="array">An <see cref="T:System.Array" /> object that holds the collection's elements. </param>
	/// <param name="index">The beginning index in the array where the elements are copied. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not supported. </exception>
	public void CopyTo(Array array, int index)
	{
		throw new NotSupportedException();
	}

	/// <summary>Returns the XML representation of the <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</summary>
	/// <returns>The XML representation of the <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> instance.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.Xml.SignedInfo.SignatureMethod" /> property is null.-or- The <see cref="P:System.Security.Cryptography.Xml.SignedInfo.References" /> property is empty. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public XmlElement GetXml()
	{
		if (CacheValid)
		{
			return _cachedXml;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.PreserveWhitespace = true;
		return GetXml(xmlDocument);
	}

	internal XmlElement GetXml(XmlDocument document)
	{
		XmlElement xmlElement = document.CreateElement("SignedInfo", "http://www.w3.org/2000/09/xmldsig#");
		if (!string.IsNullOrEmpty(_id))
		{
			xmlElement.SetAttribute("Id", _id);
		}
		XmlElement xml = CanonicalizationMethodObject.GetXml(document, "CanonicalizationMethod");
		xmlElement.AppendChild(xml);
		if (string.IsNullOrEmpty(_signatureMethod))
		{
			throw new CryptographicException("A signature method is required.");
		}
		XmlElement xmlElement2 = document.CreateElement("SignatureMethod", "http://www.w3.org/2000/09/xmldsig#");
		xmlElement2.SetAttribute("Algorithm", _signatureMethod);
		if (_signatureLength != null)
		{
			XmlElement xmlElement3 = document.CreateElement(null, "HMACOutputLength", "http://www.w3.org/2000/09/xmldsig#");
			XmlText newChild = document.CreateTextNode(_signatureLength);
			xmlElement3.AppendChild(newChild);
			xmlElement2.AppendChild(xmlElement3);
		}
		xmlElement.AppendChild(xmlElement2);
		if (_references.Count == 0)
		{
			throw new CryptographicException("At least one Reference element is required.");
		}
		for (int i = 0; i < _references.Count; i++)
		{
			Reference reference = (Reference)_references[i];
			xmlElement.AppendChild(reference.GetXml(document));
		}
		return xmlElement;
	}

	/// <summary>Loads a <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> state from an XML element.</summary>
	/// <param name="value">The XML element from which to load the <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> state. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="value" /> parameter is not a valid <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> element.-or- The <paramref name="value" /> parameter does not contain a valid <see cref="P:System.Security.Cryptography.Xml.SignedInfo.CanonicalizationMethod" /> property.-or- The <paramref name="value" /> parameter does not contain a valid <see cref="P:System.Security.Cryptography.Xml.SignedInfo.SignatureMethod" /> property.</exception>
	public void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!value.LocalName.Equals("SignedInfo"))
		{
			throw new CryptographicException("Malformed element {0}.", "SignedInfo");
		}
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(value.OwnerDocument.NameTable);
		xmlNamespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
		int num = 0;
		_id = Utils.GetAttribute(value, "Id", "http://www.w3.org/2000/09/xmldsig#");
		if (!Utils.VerifyAttributes(value, "Id"))
		{
			throw new CryptographicException("Malformed element {0}.", "SignedInfo");
		}
		XmlNodeList xmlNodeList = value.SelectNodes("ds:CanonicalizationMethod", xmlNamespaceManager);
		if (xmlNodeList == null || xmlNodeList.Count == 0 || xmlNodeList.Count > 1)
		{
			throw new CryptographicException("Malformed element {0}.", "SignedInfo/CanonicalizationMethod");
		}
		XmlElement xmlElement = xmlNodeList.Item(0) as XmlElement;
		num += xmlNodeList.Count;
		_canonicalizationMethod = Utils.GetAttribute(xmlElement, "Algorithm", "http://www.w3.org/2000/09/xmldsig#");
		if (_canonicalizationMethod == null || !Utils.VerifyAttributes(xmlElement, "Algorithm"))
		{
			throw new CryptographicException("Malformed element {0}.", "SignedInfo/CanonicalizationMethod");
		}
		_canonicalizationMethodTransform = null;
		if (xmlElement.ChildNodes.Count > 0)
		{
			CanonicalizationMethodObject.LoadInnerXml(xmlElement.ChildNodes);
		}
		XmlNodeList xmlNodeList2 = value.SelectNodes("ds:SignatureMethod", xmlNamespaceManager);
		if (xmlNodeList2 == null || xmlNodeList2.Count == 0 || xmlNodeList2.Count > 1)
		{
			throw new CryptographicException("Malformed element {0}.", "SignedInfo/SignatureMethod");
		}
		XmlElement xmlElement2 = xmlNodeList2.Item(0) as XmlElement;
		num += xmlNodeList2.Count;
		_signatureMethod = Utils.GetAttribute(xmlElement2, "Algorithm", "http://www.w3.org/2000/09/xmldsig#");
		if (_signatureMethod == null || !Utils.VerifyAttributes(xmlElement2, "Algorithm"))
		{
			throw new CryptographicException("Malformed element {0}.", "SignedInfo/SignatureMethod");
		}
		if (xmlElement2.SelectSingleNode("ds:HMACOutputLength", xmlNamespaceManager) is XmlElement xmlElement3)
		{
			_signatureLength = xmlElement3.InnerXml;
		}
		_references.Clear();
		XmlNodeList xmlNodeList3 = value.SelectNodes("ds:Reference", xmlNamespaceManager);
		if (xmlNodeList3 != null)
		{
			if (xmlNodeList3.Count > 100)
			{
				throw new CryptographicException("Malformed element {0}.", "SignedInfo/Reference");
			}
			foreach (XmlNode item in xmlNodeList3)
			{
				XmlElement value2 = item as XmlElement;
				Reference reference = new Reference();
				AddReference(reference);
				reference.LoadXml(value2);
			}
			num += xmlNodeList3.Count;
			if (value.SelectNodes("*").Count != num)
			{
				throw new CryptographicException("Malformed element {0}.", "SignedInfo");
			}
		}
		_cachedXml = value;
	}

	/// <summary>Adds a <see cref="T:System.Security.Cryptography.Xml.Reference" /> object to the list of references to digest and sign.</summary>
	/// <param name="reference">The reference to add to the list of references. </param>
	/// <exception cref="T:System.ArgumentNullException">The reference parameter is null.</exception>
	public void AddReference(Reference reference)
	{
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
		reference.SignedXml = SignedXml;
		_references.Add(reference);
	}
}
