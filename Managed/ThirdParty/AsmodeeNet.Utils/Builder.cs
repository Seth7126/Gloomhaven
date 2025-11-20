using System;
using System.Reflection;

namespace AsmodeeNet.Utils;

public abstract class Builder<T> where T : class
{
	public class BuilderErrors
	{
		public string badField;

		public string reason;

		public BuilderErrors(string badField, string reason)
		{
			this.badField = badField;
			this.reason = reason;
		}

		public override string ToString()
		{
			return "'" + badField + "' field is badly formatted. The reason is '" + reason + "'";
		}
	}

	public abstract BuilderErrors[] Validate();

	public Either<T, BuilderErrors[]> Build(bool mustValidate = true)
	{
		if (mustValidate)
		{
			BuilderErrors[] array = Validate();
			if (array != null)
			{
				return Either<T, BuilderErrors[]>.newWithError(array);
			}
		}
		return Either<T, BuilderErrors[]>.newWithValue(Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[1] { this }, null) as T);
	}
}
