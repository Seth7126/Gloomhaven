using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents an &lt;X509Data&gt; subelement of an XMLDSIG or XML Encryption &lt;KeyInfo&gt; element.</summary>
public class KeyInfoX509Data : KeyInfoClause
{
	private ArrayList _certificates;

	private ArrayList _issuerSerials;

	private ArrayList _subjectKeyIds;

	private ArrayList _subjectNames;

	private byte[] _CRL;

	/// <summary>Gets a list of the X.509v3 certificates contained in the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <returns>A list of the X.509 certificates contained in the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</returns>
	public ArrayList Certificates => _certificates;

	/// <summary>Gets a list of the subject key identifiers (SKIs) contained in the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <returns>A list of the subject key identifiers (SKIs) contained in the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</returns>
	public ArrayList SubjectKeyIds => _subjectKeyIds;

	/// <summary>Gets a list of the subject names of the entities contained in the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <returns>A list of the subject names of the entities contained in the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</returns>
	public ArrayList SubjectNames => _subjectNames;

	/// <summary>Gets a list of <see cref="T:System.Security.Cryptography.Xml.X509IssuerSerial" /> structures that represent an issuer name and serial number pair.</summary>
	/// <returns>A list of <see cref="T:System.Security.Cryptography.Xml.X509IssuerSerial" /> structures that represent an issuer name and serial number pair.</returns>
	public ArrayList IssuerSerials => _issuerSerials;

	/// <summary>Gets or sets the Certificate Revocation List (CRL) contained within the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <returns>The Certificate Revocation List (CRL) contained within the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</returns>
	public byte[] CRL
	{
		get
		{
			return _CRL;
		}
		set
		{
			_CRL = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> class.</summary>
	public KeyInfoX509Data()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> class from the specified ASN.1 DER encoding of an X.509v3 certificate.</summary>
	/// <param name="rgbCert">The ASN.1 DER encoding of an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to initialize the new instance of <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> from.</param>
	public KeyInfoX509Data(byte[] rgbCert)
	{
		X509Certificate2 certificate = new X509Certificate2(rgbCert);
		AddCertificate(certificate);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> class from the specified X.509v3 certificate.</summary>
	/// <param name="cert">The <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to initialize the new instance of <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> from.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="cert" /> parameter is null.</exception>
	public KeyInfoX509Data(X509Certificate cert)
	{
		AddCertificate(cert);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> class from the specified X.509v3 certificate.</summary>
	/// <param name="cert">The <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to initialize the new instance of <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> from.</param>
	/// <param name="includeOption">One of the <see cref="T:System.Security.Cryptography.X509Certificates.X509IncludeOption" /> values that specifies how much of the certificate chain to include.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="cert" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate has only a partial certificate chain.</exception>
	public KeyInfoX509Data(X509Certificate cert, X509IncludeOption includeOption)
	{
		if (cert == null)
		{
			throw new ArgumentNullException("cert");
		}
		X509Certificate2 certificate = new X509Certificate2(cert);
		X509ChainElementCollection x509ChainElementCollection = null;
		X509Chain x509Chain = null;
		switch (includeOption)
		{
		case X509IncludeOption.ExcludeRoot:
		{
			x509Chain = new X509Chain();
			x509Chain.Build(certificate);
			if (x509Chain.ChainStatus.Length != 0 && (x509Chain.ChainStatus[0].Status & X509ChainStatusFlags.PartialChain) == X509ChainStatusFlags.PartialChain)
			{
				throw new CryptographicException("A certificate chain could not be built to a trusted root authority.");
			}
			x509ChainElementCollection = x509Chain.ChainElements;
			for (int i = 0; i < (Utils.IsSelfSigned(x509Chain) ? 1 : (x509ChainElementCollection.Count - 1)); i++)
			{
				AddCertificate(x509ChainElementCollection[i].Certificate);
			}
			break;
		}
		case X509IncludeOption.EndCertOnly:
			AddCertificate(certificate);
			break;
		case X509IncludeOption.WholeChain:
		{
			x509Chain = new X509Chain();
			x509Chain.Build(certificate);
			if (x509Chain.ChainStatus.Length != 0 && (x509Chain.ChainStatus[0].Status & X509ChainStatusFlags.PartialChain) == X509ChainStatusFlags.PartialChain)
			{
				throw new CryptographicException("A certificate chain could not be built to a trusted root authority.");
			}
			x509ChainElementCollection = x509Chain.ChainElements;
			X509ChainElementEnumerator enumerator = x509ChainElementCollection.GetEnumerator();
			while (enumerator.MoveNext())
			{
				X509ChainElement current = enumerator.Current;
				AddCertificate(current.Certificate);
			}
			break;
		}
		}
	}

	/// <summary>Adds the specified X.509v3 certificate to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" />.</summary>
	/// <param name="certificate">The <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to add to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="certificate" /> parameter is null.</exception>
	public void AddCertificate(X509Certificate certificate)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		if (_certificates == null)
		{
			_certificates = new ArrayList();
		}
		X509Certificate2 value = new X509Certificate2(certificate);
		_certificates.Add(value);
	}

	/// <summary>Adds the specified subject key identifier (SKI) byte array to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <param name="subjectKeyId">A byte array that represents the subject key identifier (SKI) to add to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object. </param>
	public void AddSubjectKeyId(byte[] subjectKeyId)
	{
		if (_subjectKeyIds == null)
		{
			_subjectKeyIds = new ArrayList();
		}
		_subjectKeyIds.Add(subjectKeyId);
	}

	/// <summary>Adds the specified subject key identifier (SKI) string to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <param name="subjectKeyId">A string that represents the subject key identifier (SKI) to add to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</param>
	public void AddSubjectKeyId(string subjectKeyId)
	{
		if (_subjectKeyIds == null)
		{
			_subjectKeyIds = new ArrayList();
		}
		_subjectKeyIds.Add(Utils.DecodeHexString(subjectKeyId));
	}

	/// <summary>Adds the subject name of the entity that was issued an X.509v3 certificate to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <param name="subjectName">The name of the entity that was issued an X.509 certificate to add to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object. </param>
	public void AddSubjectName(string subjectName)
	{
		if (_subjectNames == null)
		{
			_subjectNames = new ArrayList();
		}
		_subjectNames.Add(subjectName);
	}

	/// <summary>Adds the specified issuer name and serial number pair to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <param name="issuerName">The issuer name portion of the pair to add to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object. </param>
	/// <param name="serialNumber">The serial number portion of the pair to add to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object. </param>
	public void AddIssuerSerial(string issuerName, string serialNumber)
	{
		if (string.IsNullOrEmpty(issuerName))
		{
			throw new ArgumentException("String cannot be empty or null.", "issuerName");
		}
		if (string.IsNullOrEmpty(serialNumber))
		{
			throw new ArgumentException("String cannot be empty or null.", "serialNumber");
		}
		if (!BigInteger.TryParse(serialNumber, NumberStyles.AllowHexSpecifier, NumberFormatInfo.CurrentInfo, out var result))
		{
			throw new ArgumentException("X509 issuer serial number is invalid.", "serialNumber");
		}
		if (_issuerSerials == null)
		{
			_issuerSerials = new ArrayList();
		}
		_issuerSerials.Add(Utils.CreateX509IssuerSerial(issuerName, result.ToString()));
	}

	internal void InternalAddIssuerSerial(string issuerName, string serialNumber)
	{
		if (_issuerSerials == null)
		{
			_issuerSerials = new ArrayList();
		}
		_issuerSerials.Add(Utils.CreateX509IssuerSerial(issuerName, serialNumber));
	}

	private void Clear()
	{
		_CRL = null;
		if (_subjectKeyIds != null)
		{
			_subjectKeyIds.Clear();
		}
		if (_subjectNames != null)
		{
			_subjectNames.Clear();
		}
		if (_issuerSerials != null)
		{
			_issuerSerials.Clear();
		}
		if (_certificates != null)
		{
			_certificates.Clear();
		}
	}

	/// <summary>Returns an XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <returns>An XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override XmlElement GetXml()
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.PreserveWhitespace = true;
		return GetXml(xmlDocument);
	}

	internal override XmlElement GetXml(XmlDocument xmlDocument)
	{
		XmlElement xmlElement = xmlDocument.CreateElement("X509Data", "http://www.w3.org/2000/09/xmldsig#");
		if (_issuerSerials != null)
		{
			foreach (X509IssuerSerial issuerSerial in _issuerSerials)
			{
				XmlElement xmlElement2 = xmlDocument.CreateElement("X509IssuerSerial", "http://www.w3.org/2000/09/xmldsig#");
				XmlElement xmlElement3 = xmlDocument.CreateElement("X509IssuerName", "http://www.w3.org/2000/09/xmldsig#");
				xmlElement3.AppendChild(xmlDocument.CreateTextNode(issuerSerial.IssuerName));
				xmlElement2.AppendChild(xmlElement3);
				XmlElement xmlElement4 = xmlDocument.CreateElement("X509SerialNumber", "http://www.w3.org/2000/09/xmldsig#");
				xmlElement4.AppendChild(xmlDocument.CreateTextNode(issuerSerial.SerialNumber));
				xmlElement2.AppendChild(xmlElement4);
				xmlElement.AppendChild(xmlElement2);
			}
		}
		if (_subjectKeyIds != null)
		{
			foreach (byte[] subjectKeyId in _subjectKeyIds)
			{
				XmlElement xmlElement5 = xmlDocument.CreateElement("X509SKI", "http://www.w3.org/2000/09/xmldsig#");
				xmlElement5.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(subjectKeyId)));
				xmlElement.AppendChild(xmlElement5);
			}
		}
		if (_subjectNames != null)
		{
			foreach (string subjectName in _subjectNames)
			{
				XmlElement xmlElement6 = xmlDocument.CreateElement("X509SubjectName", "http://www.w3.org/2000/09/xmldsig#");
				xmlElement6.AppendChild(xmlDocument.CreateTextNode(subjectName));
				xmlElement.AppendChild(xmlElement6);
			}
		}
		if (_certificates != null)
		{
			foreach (X509Certificate certificate in _certificates)
			{
				XmlElement xmlElement7 = xmlDocument.CreateElement("X509Certificate", "http://www.w3.org/2000/09/xmldsig#");
				xmlElement7.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(certificate.GetRawCertData())));
				xmlElement.AppendChild(xmlElement7);
			}
		}
		if (_CRL != null)
		{
			XmlElement xmlElement8 = xmlDocument.CreateElement("X509CRL", "http://www.w3.org/2000/09/xmldsig#");
			xmlElement8.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(_CRL)));
			xmlElement.AppendChild(xmlElement8);
		}
		return xmlElement;
	}

	/// <summary>Parses the input <see cref="T:System.Xml.XmlElement" /> object and configures the internal state of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object to match.</summary>
	/// <param name="element">The <see cref="T:System.Xml.XmlElement" /> object that specifies the state of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="element" /> parameter does not contain an &lt;X509IssuerName&gt; node.-or-The <paramref name="element" /> parameter does not contain an &lt;X509SerialNumber&gt; node.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public override void LoadXml(XmlElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(element.OwnerDocument.NameTable);
		xmlNamespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
		XmlNodeList xmlNodeList = element.SelectNodes("ds:X509IssuerSerial", xmlNamespaceManager);
		XmlNodeList xmlNodeList2 = element.SelectNodes("ds:X509SKI", xmlNamespaceManager);
		XmlNodeList xmlNodeList3 = element.SelectNodes("ds:X509SubjectName", xmlNamespaceManager);
		XmlNodeList xmlNodeList4 = element.SelectNodes("ds:X509Certificate", xmlNamespaceManager);
		XmlNodeList xmlNodeList5 = element.SelectNodes("ds:X509CRL", xmlNamespaceManager);
		if (xmlNodeList5.Count == 0 && xmlNodeList.Count == 0 && xmlNodeList2.Count == 0 && xmlNodeList3.Count == 0 && xmlNodeList4.Count == 0)
		{
			throw new CryptographicException("Malformed element {0}.", "X509Data");
		}
		Clear();
		if (xmlNodeList5.Count != 0)
		{
			_CRL = Convert.FromBase64String(Utils.DiscardWhiteSpaces(xmlNodeList5.Item(0).InnerText));
		}
		foreach (XmlNode item in xmlNodeList)
		{
			XmlNode xmlNode = item.SelectSingleNode("ds:X509IssuerName", xmlNamespaceManager);
			XmlNode xmlNode2 = item.SelectSingleNode("ds:X509SerialNumber", xmlNamespaceManager);
			if (xmlNode == null || xmlNode2 == null)
			{
				throw new CryptographicException("Malformed element {0}.", "IssuerSerial");
			}
			InternalAddIssuerSerial(xmlNode.InnerText.Trim(), xmlNode2.InnerText.Trim());
		}
		foreach (XmlNode item2 in xmlNodeList2)
		{
			AddSubjectKeyId(Convert.FromBase64String(Utils.DiscardWhiteSpaces(item2.InnerText)));
		}
		foreach (XmlNode item3 in xmlNodeList3)
		{
			AddSubjectName(item3.InnerText.Trim());
		}
		foreach (XmlNode item4 in xmlNodeList4)
		{
			AddCertificate(new X509Certificate2(Convert.FromBase64String(Utils.DiscardWhiteSpaces(item4.InnerText))));
		}
	}
}
