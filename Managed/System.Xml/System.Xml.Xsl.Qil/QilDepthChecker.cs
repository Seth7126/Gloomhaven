using System.Collections.Generic;

namespace System.Xml.Xsl.Qil;

internal class QilDepthChecker
{
	private const int MAX_QIL_DEPTH = 800;

	private Dictionary<QilNode, bool> _visitedRef = new Dictionary<QilNode, bool>();

	public static void Check(QilNode input)
	{
		if (System.LocalAppContextSwitches.LimitXPathComplexity)
		{
			new QilDepthChecker().Check(input, 0);
		}
	}

	private void Check(QilNode input, int depth)
	{
		if (depth > 800)
		{
			throw XsltException.Create("The stylesheet is too complex.");
		}
		if (input is QilReference)
		{
			if (_visitedRef.ContainsKey(input))
			{
				return;
			}
			_visitedRef[input] = true;
		}
		int depth2 = depth + 1;
		for (int i = 0; i < input.Count; i++)
		{
			QilNode qilNode = input[i];
			if (qilNode != null)
			{
				Check(qilNode, depth2);
			}
		}
	}
}
