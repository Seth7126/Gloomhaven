using System.Diagnostics;

namespace System.Xml.Xsl.Qil;

internal class QilValidationVisitor : QilScopedVisitor
{
	private SubstitutionList _subs = new SubstitutionList();

	private QilTypeChecker _typeCheck = new QilTypeChecker();

	[Conditional("DEBUG")]
	public static void Validate(QilNode node)
	{
		new QilValidationVisitor().VisitAssumeReference(node);
	}

	protected QilValidationVisitor()
	{
	}

	[Conditional("DEBUG")]
	internal static void SetError(QilNode n, string message)
	{
		message = global::SR.Format("QIL Validation Error! '{0}'.", message);
		if (n.Annotation is string text)
		{
			message = text + "\n" + message;
		}
		n.Annotation = message;
	}
}
