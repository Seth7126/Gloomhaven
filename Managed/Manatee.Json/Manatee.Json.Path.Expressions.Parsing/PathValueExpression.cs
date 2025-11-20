namespace Manatee.Json.Path.Expressions.Parsing;

internal class PathValueExpression<T> : ValueExpression
{
	public PathExpression<T> Path => (PathExpression<T>)base.Value;

	public PathValueExpression(object value)
		: base(value)
	{
	}
}
