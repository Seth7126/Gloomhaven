using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hydra.Sdk.Extensions;

public static class ExceptionExtensions
{
	public static string GetErrorMessage(this Exception err, params object[] args)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (args.Length != 0)
		{
			foreach (object arg in args)
			{
				stringBuilder.AppendLine($"{arg}");
			}
		}
		stringBuilder.AppendLine(err.GetInnerExceptions());
		return stringBuilder.ToString();
	}

	public static string GetInnerExceptions(this Exception ex)
	{
		Exception ex2 = ex;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("[Source]: " + ex.Source);
		int num = 0;
		do
		{
			stringBuilder.AppendLine($"[{num}]: {ex2.Message}");
			stringBuilder.AppendLine($"[StackTrace-{num}]: {ex2.StackTrace}");
			num++;
		}
		while ((ex2 = ex2.InnerException) != null);
		return stringBuilder.ToString();
	}

	public static IEnumerable<Exception> GetAllExceptions(this Exception exception)
	{
		yield return exception;
		if (exception is AggregateException aggrEx)
		{
			foreach (Exception item in aggrEx.InnerExceptions.SelectMany((Exception e) => e.GetAllExceptions()))
			{
				yield return item;
			}
		}
		else
		{
			if (exception.InnerException == null)
			{
				yield break;
			}
			foreach (Exception allException in exception.InnerException.GetAllExceptions())
			{
				yield return allException;
			}
		}
	}
}
