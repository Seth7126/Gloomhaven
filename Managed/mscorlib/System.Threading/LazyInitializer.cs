namespace System.Threading;

/// <summary>Provides lazy initialization routines.</summary>
public static class LazyInitializer
{
	/// <summary>Initializes a target reference type with the type's default constructor if it hasn't already been initialized.</summary>
	/// <returns>The initialized reference of type <paramref name="T" />.</returns>
	/// <param name="target">A reference of type <paramref name="T" /> to initialize if it has not already been initialized.</param>
	/// <typeparam name="T">The type of the reference to be initialized.</typeparam>
	/// <exception cref="T:System.MemberAccessException">Permissions to access the constructor of type <paramref name="T" /> were missing.</exception>
	/// <exception cref="T:System.MissingMemberException">Type <paramref name="T" /> does not have a default constructor.</exception>
	public static T EnsureInitialized<T>(ref T target) where T : class
	{
		return Volatile.Read(ref target) ?? EnsureInitializedCore(ref target);
	}

	private static T EnsureInitializedCore<T>(ref T target) where T : class
	{
		try
		{
			Interlocked.CompareExchange(ref target, Activator.CreateInstance<T>(), null);
		}
		catch (MissingMethodException)
		{
			throw new MissingMemberException("The lazily-initialized type does not have a public, parameterless constructor.");
		}
		return target;
	}

	/// <summary>Initializes a target reference type by using a specified function if it hasn't already been initialized.</summary>
	/// <returns>The initialized value of type <paramref name="T" />.</returns>
	/// <param name="target">The reference of type <paramref name="T" /> to initialize if it hasn't already been initialized.</param>
	/// <param name="valueFactory">The function that is called to initialize the reference.</param>
	/// <typeparam name="T">The reference type of the reference to be initialized.</typeparam>
	/// <exception cref="T:System.MissingMemberException">Type <paramref name="T" /> does not have a default constructor.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="valueFactory" /> returned null (Nothing in Visual Basic).</exception>
	public static T EnsureInitialized<T>(ref T target, Func<T> valueFactory) where T : class
	{
		return Volatile.Read(ref target) ?? EnsureInitializedCore(ref target, valueFactory);
	}

	private static T EnsureInitializedCore<T>(ref T target, Func<T> valueFactory) where T : class
	{
		T val = valueFactory();
		if (val == null)
		{
			throw new InvalidOperationException("ValueFactory returned null.");
		}
		Interlocked.CompareExchange(ref target, val, null);
		return target;
	}

	/// <summary>Initializes a target reference or value type with its default constructor if it hasn't already been initialized.</summary>
	/// <returns>The initialized value of type <paramref name="T" />.</returns>
	/// <param name="target">A reference or value of type <paramref name="T" /> to initialize if it hasn't already been initialized.</param>
	/// <param name="initialized">A reference to a Boolean value that determines whether the target has already been initialized.</param>
	/// <param name="syncLock">A reference to an object used as the mutually exclusive lock for initializing <paramref name="target" />. If <paramref name="syncLock" /> is null, a new object will be instantiated.</param>
	/// <typeparam name="T">The type of the reference to be initialized.</typeparam>
	/// <exception cref="T:System.MemberAccessException">Permissions to access the constructor of type <paramref name="T" /> were missing.</exception>
	/// <exception cref="T:System.MissingMemberException">Type <paramref name="T" /> does not have a default constructor.</exception>
	public static T EnsureInitialized<T>(ref T target, ref bool initialized, ref object syncLock)
	{
		if (Volatile.Read(ref initialized))
		{
			return target;
		}
		return EnsureInitializedCore(ref target, ref initialized, ref syncLock);
	}

	private static T EnsureInitializedCore<T>(ref T target, ref bool initialized, ref object syncLock)
	{
		lock (EnsureLockInitialized(ref syncLock))
		{
			if (!Volatile.Read(ref initialized))
			{
				try
				{
					target = Activator.CreateInstance<T>();
				}
				catch (MissingMethodException)
				{
					throw new MissingMemberException("The lazily-initialized type does not have a public, parameterless constructor.");
				}
				Volatile.Write(ref initialized, value: true);
			}
		}
		return target;
	}

	/// <summary>Initializes a target reference or value type by using a specified function if it hasn't already been initialized.</summary>
	/// <returns>The initialized value of type <paramref name="T" />.</returns>
	/// <param name="target">A reference or value of type <paramref name="T" /> to initialize if it hasn't already been initialized.</param>
	/// <param name="initialized">A reference to a Boolean value that determines whether the target has already been initialized.</param>
	/// <param name="syncLock">A reference to an object used as the mutually exclusive lock for initializing <paramref name="target" />. If <paramref name="syncLock" /> is null, a new object will be instantiated.</param>
	/// <param name="valueFactory">The function that is called to initialize the reference or value.</param>
	/// <typeparam name="T">The type of the reference to be initialized.</typeparam>
	/// <exception cref="T:System.MemberAccessException">Permissions to access the constructor of type <paramref name="T" /> were missing.</exception>
	/// <exception cref="T:System.MissingMemberException">Type <paramref name="T" /> does not have a default constructor.</exception>
	public static T EnsureInitialized<T>(ref T target, ref bool initialized, ref object syncLock, Func<T> valueFactory)
	{
		if (Volatile.Read(ref initialized))
		{
			return target;
		}
		return EnsureInitializedCore(ref target, ref initialized, ref syncLock, valueFactory);
	}

	private static T EnsureInitializedCore<T>(ref T target, ref bool initialized, ref object syncLock, Func<T> valueFactory)
	{
		lock (EnsureLockInitialized(ref syncLock))
		{
			if (!Volatile.Read(ref initialized))
			{
				target = valueFactory();
				Volatile.Write(ref initialized, value: true);
			}
		}
		return target;
	}

	public static T EnsureInitialized<T>(ref T target, ref object syncLock, Func<T> valueFactory) where T : class
	{
		return Volatile.Read(ref target) ?? EnsureInitializedCore(ref target, ref syncLock, valueFactory);
	}

	private static T EnsureInitializedCore<T>(ref T target, ref object syncLock, Func<T> valueFactory) where T : class
	{
		lock (EnsureLockInitialized(ref syncLock))
		{
			if (Volatile.Read(ref target) == null)
			{
				Volatile.Write(ref target, valueFactory());
				if (target == null)
				{
					throw new InvalidOperationException("ValueFactory returned null.");
				}
			}
		}
		return target;
	}

	private static object EnsureLockInitialized(ref object syncLock)
	{
		return syncLock ?? Interlocked.CompareExchange(ref syncLock, new object(), null) ?? syncLock;
	}
}
