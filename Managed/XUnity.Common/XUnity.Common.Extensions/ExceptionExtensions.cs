using System;

namespace XUnity.Common.Extensions;

public static class ExceptionExtensions
{
	public static TException FirstInnerExceptionOfType<TException>(this Exception e) where TException : Exception
	{
		for (Exception ex = e; ex != null; ex = ex.InnerException)
		{
			if (ex is TException)
			{
				return (TException)ex;
			}
		}
		return null;
	}
}
