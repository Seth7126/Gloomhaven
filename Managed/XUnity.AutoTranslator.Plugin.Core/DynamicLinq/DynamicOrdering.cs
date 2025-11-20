using System.Linq.Expressions;

namespace DynamicLinq;

internal class DynamicOrdering
{
	public Expression Selector;

	public bool Ascending;
}
