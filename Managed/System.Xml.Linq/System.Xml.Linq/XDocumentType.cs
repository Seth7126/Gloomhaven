using System.Threading;
using System.Threading.Tasks;

namespace System.Xml.Linq;

/// <summary>Represents an XML Document Type Definition (DTD). </summary>
/// <filterpriority>2</filterpriority>
public class XDocumentType : XNode
{
	private string _name;

	private string _publicId;

	private string _systemId;

	private string _internalSubset;

	/// <summary>Gets or sets the internal subset for this Document Type Definition (DTD).</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the internal subset for this Document Type Definition (DTD).</returns>
	public string InternalSubset
	{
		get
		{
			return _internalSubset;
		}
		set
		{
			bool num = NotifyChanging(this, XObjectChangeEventArgs.Value);
			_internalSubset = value;
			if (num)
			{
				NotifyChanged(this, XObjectChangeEventArgs.Value);
			}
		}
	}

	/// <summary>Gets or sets the name for this Document Type Definition (DTD).</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the name for this Document Type Definition (DTD).</returns>
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			value = XmlConvert.VerifyName(value);
			bool num = NotifyChanging(this, XObjectChangeEventArgs.Name);
			_name = value;
			if (num)
			{
				NotifyChanged(this, XObjectChangeEventArgs.Name);
			}
		}
	}

	/// <summary>Gets the node type for this node.</summary>
	/// <returns>The node type. For <see cref="T:System.Xml.Linq.XDocumentType" /> objects, this value is <see cref="F:System.Xml.XmlNodeType.DocumentType" />.</returns>
	public override XmlNodeType NodeType => XmlNodeType.DocumentType;

	/// <summary>Gets or sets the public identifier for this Document Type Definition (DTD).</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the public identifier for this Document Type Definition (DTD).</returns>
	public string PublicId
	{
		get
		{
			return _publicId;
		}
		set
		{
			bool num = NotifyChanging(this, XObjectChangeEventArgs.Value);
			_publicId = value;
			if (num)
			{
				NotifyChanged(this, XObjectChangeEventArgs.Value);
			}
		}
	}

	/// <summary>Gets or sets the system identifier for this Document Type Definition (DTD).</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the system identifier for this Document Type Definition (DTD).</returns>
	public string SystemId
	{
		get
		{
			return _systemId;
		}
		set
		{
			bool num = NotifyChanging(this, XObjectChangeEventArgs.Value);
			_systemId = value;
			if (num)
			{
				NotifyChanged(this, XObjectChangeEventArgs.Value);
			}
		}
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Xml.Linq.XDocumentType" /> class. </summary>
	/// <param name="name">A <see cref="T:System.String" /> that contains the qualified name of the DTD, which is the same as the qualified name of the root element of the XML document.</param>
	/// <param name="publicId">A <see cref="T:System.String" /> that contains the public identifier of an external public DTD.</param>
	/// <param name="systemId">A <see cref="T:System.String" /> that contains the system identifier of an external private DTD.</param>
	/// <param name="internalSubset">A <see cref="T:System.String" /> that contains the internal subset for an internal DTD.</param>
	public XDocumentType(string name, string publicId, string systemId, string internalSubset)
	{
		_name = XmlConvert.VerifyName(name);
		_publicId = publicId;
		_systemId = systemId;
		_internalSubset = internalSubset;
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Xml.Linq.XDocumentType" /> class from another <see cref="T:System.Xml.Linq.XDocumentType" /> object.</summary>
	/// <param name="other">An <see cref="T:System.Xml.Linq.XDocumentType" /> object to copy from.</param>
	public XDocumentType(XDocumentType other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		_name = other._name;
		_publicId = other._publicId;
		_systemId = other._systemId;
		_internalSubset = other._internalSubset;
	}

	internal XDocumentType(XmlReader r)
	{
		_name = r.Name;
		_publicId = r.GetAttribute("PUBLIC");
		_systemId = r.GetAttribute("SYSTEM");
		_internalSubset = r.Value;
		r.Read();
	}

	/// <summary>Write this <see cref="T:System.Xml.Linq.XDocumentType" /> to an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">An <see cref="T:System.Xml.XmlWriter" /> into which this method will write.</param>
	/// <filterpriority>2</filterpriority>
	public override void WriteTo(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteDocType(_name, _publicId, _systemId, _internalSubset);
	}

	public override Task WriteToAsync(XmlWriter writer, CancellationToken cancellationToken)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled(cancellationToken);
		}
		return writer.WriteDocTypeAsync(_name, _publicId, _systemId, _internalSubset);
	}

	internal override XNode CloneNode()
	{
		return new XDocumentType(this);
	}

	internal override bool DeepEquals(XNode node)
	{
		if (node is XDocumentType xDocumentType && _name == xDocumentType._name && _publicId == xDocumentType._publicId && _systemId == xDocumentType.SystemId)
		{
			return _internalSubset == xDocumentType._internalSubset;
		}
		return false;
	}

	internal override int GetDeepHashCode()
	{
		return _name.GetHashCode() ^ ((_publicId != null) ? _publicId.GetHashCode() : 0) ^ ((_systemId != null) ? _systemId.GetHashCode() : 0) ^ ((_internalSubset != null) ? _internalSubset.GetHashCode() : 0);
	}
}
