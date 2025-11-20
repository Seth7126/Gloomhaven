using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Manatee.Json.Path.ArrayParameters;
using Manatee.Json.Path.Expressions.Translation;
using Manatee.Json.Path.Operators;
using Manatee.Json.Path.SearchParameters;

namespace Manatee.Json.Path;

public static class JsonPathWith
{
	public static JsonPath Length()
	{
		return new JsonPath
		{
			Operators = { (IJsonPathOperator)LengthOperator.Instance }
		};
	}

	public static JsonPath Name(string? name = null)
	{
		JsonPath jsonPath = new JsonPath();
		if (name != null)
		{
			if (name == "length")
			{
				jsonPath.Operators.Add(LengthOperator.Instance);
			}
			else
			{
				jsonPath.Operators.Add(new NameOperator(name));
			}
		}
		else
		{
			jsonPath.Operators.Add(WildCardOperator.Instance);
		}
		return jsonPath;
	}

	public static JsonPath Search(string? name = null)
	{
		JsonPath jsonPath = new JsonPath();
		List<IJsonPathOperator> operators = jsonPath.Operators;
		IJsonPathSearchParameter parameter;
		if (name != null)
		{
			IJsonPathSearchParameter jsonPathSearchParameter = new NameSearchParameter(name);
			parameter = jsonPathSearchParameter;
		}
		else
		{
			IJsonPathSearchParameter jsonPathSearchParameter = WildCardSearchParameter.Instance;
			parameter = jsonPathSearchParameter;
		}
		operators.Add(new SearchOperator(parameter));
		return jsonPath;
	}

	public static JsonPath SearchLength()
	{
		return new JsonPath
		{
			Operators = { (IJsonPathOperator)new SearchOperator(LengthSearchParameter.Instance) }
		};
	}

	public static JsonPath SearchArray(params Slice[] slices)
	{
		return new JsonPath
		{
			Operators = { (IJsonPathOperator)new SearchOperator(slices.Any() ? new ArraySearchParameter(new SliceQuery(slices)) : new ArraySearchParameter(WildCardQuery.Instance)) }
		};
	}

	public static JsonPath SearchArray(Expression<Func<JsonPathArray, int>> expression)
	{
		return new JsonPath
		{
			Operators = { (IJsonPathOperator)new SearchOperator(new ArraySearchParameter(new IndexExpressionQuery(ExpressionTranslator.Translate(expression)))) }
		};
	}

	public static JsonPath SearchArray(Expression<Func<JsonPathValue, bool>> expression)
	{
		return new JsonPath
		{
			Operators = { (IJsonPathOperator)new SearchOperator(new ArraySearchParameter(new FilterExpressionQuery(ExpressionTranslator.Translate(expression)))) }
		};
	}

	public static JsonPath Array(params Slice[] slices)
	{
		return new JsonPath
		{
			Operators = { (IJsonPathOperator)((!slices.Any()) ? new ArrayOperator(WildCardQuery.Instance) : new ArrayOperator(new SliceQuery(slices))) }
		};
	}

	public static JsonPath Array(Expression<Func<JsonPathArray, int>> expression)
	{
		return new JsonPath
		{
			Operators = { (IJsonPathOperator)new ArrayOperator(new IndexExpressionQuery(ExpressionTranslator.Translate(expression))) }
		};
	}

	public static JsonPath Array(Expression<Func<JsonPathValue, bool>> expression)
	{
		return new JsonPath
		{
			Operators = { (IJsonPathOperator)new ArrayOperator(new FilterExpressionQuery(ExpressionTranslator.Translate(expression))) }
		};
	}

	public static JsonPath Length(this JsonPath path)
	{
		JsonPath jsonPath = new JsonPath();
		jsonPath.Operators.AddRange(path.Operators);
		jsonPath.Operators.Add(LengthOperator.Instance);
		return jsonPath;
	}

	public static JsonPath Name(this JsonPath path, string? name = null)
	{
		JsonPath jsonPath = new JsonPath();
		jsonPath.Operators.AddRange(path.Operators);
		if (name != null)
		{
			if (name == "length")
			{
				jsonPath.Operators.Add(LengthOperator.Instance);
			}
			else
			{
				jsonPath.Operators.Add(new NameOperator(name));
			}
		}
		else
		{
			jsonPath.Operators.Add(WildCardOperator.Instance);
		}
		return jsonPath;
	}

	public static JsonPath Search(this JsonPath path, string? name = null)
	{
		JsonPath jsonPath = new JsonPath();
		jsonPath.Operators.AddRange(path.Operators);
		List<IJsonPathOperator> operators = jsonPath.Operators;
		IJsonPathSearchParameter parameter;
		if (name != null)
		{
			IJsonPathSearchParameter jsonPathSearchParameter = new NameSearchParameter(name);
			parameter = jsonPathSearchParameter;
		}
		else
		{
			IJsonPathSearchParameter jsonPathSearchParameter = WildCardSearchParameter.Instance;
			parameter = jsonPathSearchParameter;
		}
		operators.Add(new SearchOperator(parameter));
		return jsonPath;
	}

	public static JsonPath SearchLength(this JsonPath path)
	{
		JsonPath jsonPath = new JsonPath();
		jsonPath.Operators.AddRange(path.Operators);
		jsonPath.Operators.Add(new SearchOperator(LengthSearchParameter.Instance));
		return jsonPath;
	}

	public static JsonPath SearchArray(this JsonPath path, params Slice[] slices)
	{
		JsonPath jsonPath = new JsonPath();
		jsonPath.Operators.AddRange(path.Operators);
		jsonPath.Operators.Add(new SearchOperator(slices.Any() ? new ArraySearchParameter(new SliceQuery(slices)) : new ArraySearchParameter(WildCardQuery.Instance)));
		return jsonPath;
	}

	public static JsonPath SearchArray(this JsonPath path, Expression<Func<JsonPathArray, int>> expression)
	{
		JsonPath jsonPath = new JsonPath();
		jsonPath.Operators.AddRange(path.Operators);
		jsonPath.Operators.Add(new SearchOperator(new ArraySearchParameter(new IndexExpressionQuery(ExpressionTranslator.Translate(expression)))));
		return jsonPath;
	}

	public static JsonPath SearchArray(this JsonPath path, Expression<Func<JsonPathValue, bool>> expression)
	{
		JsonPath jsonPath = new JsonPath();
		jsonPath.Operators.AddRange(path.Operators);
		jsonPath.Operators.Add(new SearchOperator(new ArraySearchParameter(new FilterExpressionQuery(ExpressionTranslator.Translate(expression)))));
		return jsonPath;
	}

	public static JsonPath Array(this JsonPath path)
	{
		JsonPath jsonPath = new JsonPath();
		jsonPath.Operators.AddRange(path.Operators);
		jsonPath.Operators.Add(new ArrayOperator(WildCardQuery.Instance));
		return jsonPath;
	}

	public static JsonPath Array(this JsonPath path, params Slice[] slices)
	{
		JsonPath jsonPath = new JsonPath();
		jsonPath.Operators.AddRange(path.Operators);
		jsonPath.Operators.Add(new ArrayOperator(new SliceQuery(slices)));
		return jsonPath;
	}

	public static JsonPath Array(this JsonPath path, Expression<Func<JsonPathArray, int>> expression)
	{
		JsonPath jsonPath = new JsonPath();
		jsonPath.Operators.AddRange(path.Operators);
		jsonPath.Operators.Add(new ArrayOperator(new IndexExpressionQuery(ExpressionTranslator.Translate(expression))));
		return jsonPath;
	}

	public static JsonPath Array(this JsonPath path, Expression<Func<JsonPathValue, bool>> expression)
	{
		JsonPath jsonPath = new JsonPath();
		jsonPath.Operators.AddRange(path.Operators);
		jsonPath.Operators.Add(new ArrayOperator(new FilterExpressionQuery(ExpressionTranslator.Translate(expression))));
		return jsonPath;
	}
}
