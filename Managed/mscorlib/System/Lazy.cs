using System.Diagnostics;
using System.Threading;

namespace System;

/// <summary>Provides support for lazy initialization.</summary>
/// <typeparam name="T">The type of object that is being lazily initialized.</typeparam>
[Serializable]
[DebuggerTypeProxy(typeof(LazyDebugView<>))]
[DebuggerDisplay("ThreadSafetyMode={Mode}, IsValueCreated={IsValueCreated}, IsValueFaulted={IsValueFaulted}, Value={ValueForDebugDisplay}")]
public class Lazy<T>
{
	private volatile LazyHelper _state;

	private Func<T> _factory;

	private T _value;

	internal T ValueForDebugDisplay
	{
		get
		{
			if (!IsValueCreated)
			{
				return default(T);
			}
			return _value;
		}
	}

	internal LazyThreadSafetyMode? Mode => LazyHelper.GetMode(_state);

	internal bool IsValueFaulted => LazyHelper.GetIsValueFaulted(_state);

	/// <summary>Gets a value that indicates whether a value has been created for this <see cref="T:System.Lazy`1" /> instance.</summary>
	/// <returns>true if a value has been created for this <see cref="T:System.Lazy`1" /> instance; otherwise, false.</returns>
	public bool IsValueCreated => _state == null;

	/// <summary>Gets the lazily initialized value of the current <see cref="T:System.Lazy`1" /> instance.</summary>
	/// <returns>The lazily initialized value of the current <see cref="T:System.Lazy`1" /> instance.</returns>
	/// <exception cref="T:System.MemberAccessException">The <see cref="T:System.Lazy`1" /> instance is initialized to use the default constructor of the type that is being lazily initialized, and permissions to access the constructor are missing. </exception>
	/// <exception cref="T:System.MissingMemberException">The <see cref="T:System.Lazy`1" /> instance is initialized to use the default constructor of the type that is being lazily initialized, and that type does not have a public, parameterless constructor. </exception>
	/// <exception cref="T:System.InvalidOperationException">The initialization function tries to access <see cref="P:System.Lazy`1.Value" /> on this instance. </exception>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public T Value
	{
		get
		{
			if (_state != null)
			{
				return CreateValue();
			}
			return _value;
		}
	}

	private static T CreateViaDefaultConstructor()
	{
		return (T)LazyHelper.CreateViaDefaultConstructor(typeof(T));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`1" /> class. When lazy initialization occurs, the default constructor of the target type is used.</summary>
	public Lazy()
		: this((Func<T>)null, LazyThreadSafetyMode.ExecutionAndPublication, useDefaultConstructor: true)
	{
	}

	public Lazy(T value)
	{
		_value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`1" /> class. When lazy initialization occurs, the specified initialization function is used.</summary>
	/// <param name="valueFactory">The delegate that is invoked to produce the lazily initialized value when it is needed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="valueFactory" /> is null. </exception>
	public Lazy(Func<T> valueFactory)
		: this(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication, useDefaultConstructor: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`1" /> class. When lazy initialization occurs, the default constructor of the target type and the specified initialization mode are used.</summary>
	/// <param name="isThreadSafe">true to make this instance usable concurrently by multiple threads; false to make the instance usable by only one thread at a time. </param>
	public Lazy(bool isThreadSafe)
		: this((Func<T>)null, LazyHelper.GetModeFromIsThreadSafe(isThreadSafe), useDefaultConstructor: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`1" /> class that uses the default constructor of <paramref name="T" /> and the specified thread-safety mode.</summary>
	/// <param name="mode">One of the enumeration values that specifies the thread safety mode. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" /> contains an invalid value. </exception>
	public Lazy(LazyThreadSafetyMode mode)
		: this((Func<T>)null, mode, useDefaultConstructor: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`1" /> class. When lazy initialization occurs, the specified initialization function and initialization mode are used.</summary>
	/// <param name="valueFactory">The delegate that is invoked to produce the lazily initialized value when it is needed.</param>
	/// <param name="isThreadSafe">true to make this instance usable concurrently by multiple threads; false to make this instance usable by only one thread at a time.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="valueFactory" /> is null. </exception>
	public Lazy(Func<T> valueFactory, bool isThreadSafe)
		: this(valueFactory, LazyHelper.GetModeFromIsThreadSafe(isThreadSafe), useDefaultConstructor: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`1" /> class that uses the specified initialization function and thread-safety mode.</summary>
	/// <param name="valueFactory">The delegate that is invoked to produce the lazily initialized value when it is needed.</param>
	/// <param name="mode">One of the enumeration values that specifies the thread safety mode. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" /> contains an invalid value. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="valueFactory" /> is null. </exception>
	public Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode)
		: this(valueFactory, mode, useDefaultConstructor: false)
	{
	}

	private Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode, bool useDefaultConstructor)
	{
		if (valueFactory == null && !useDefaultConstructor)
		{
			throw new ArgumentNullException("valueFactory");
		}
		_factory = valueFactory;
		_state = LazyHelper.Create(mode, useDefaultConstructor);
	}

	private void ViaConstructor()
	{
		_value = CreateViaDefaultConstructor();
		_state = null;
	}

	private void ViaFactory(LazyThreadSafetyMode mode)
	{
		try
		{
			Func<T> factory = _factory;
			if (factory == null)
			{
				throw new InvalidOperationException("ValueFactory attempted to access the Value property of this instance.");
			}
			_factory = null;
			_value = factory();
			_state = null;
		}
		catch (Exception exception)
		{
			_state = new LazyHelper(mode, exception);
			throw;
		}
	}

	private void ExecutionAndPublication(LazyHelper executionAndPublication, bool useDefaultConstructor)
	{
		lock (executionAndPublication)
		{
			if (_state == executionAndPublication)
			{
				if (useDefaultConstructor)
				{
					ViaConstructor();
				}
				else
				{
					ViaFactory(LazyThreadSafetyMode.ExecutionAndPublication);
				}
			}
		}
	}

	private void PublicationOnly(LazyHelper publicationOnly, T possibleValue)
	{
		if (Interlocked.CompareExchange(ref _state, LazyHelper.PublicationOnlyWaitForOtherThreadToPublish, publicationOnly) == publicationOnly)
		{
			_factory = null;
			_value = possibleValue;
			_state = null;
		}
	}

	private void PublicationOnlyViaConstructor(LazyHelper initializer)
	{
		PublicationOnly(initializer, CreateViaDefaultConstructor());
	}

	private void PublicationOnlyViaFactory(LazyHelper initializer)
	{
		Func<T> factory = _factory;
		if (factory == null)
		{
			PublicationOnlyWaitForOtherThreadToPublish();
		}
		else
		{
			PublicationOnly(initializer, factory());
		}
	}

	private void PublicationOnlyWaitForOtherThreadToPublish()
	{
		SpinWait spinWait = default(SpinWait);
		while (_state != null)
		{
			spinWait.SpinOnce();
		}
	}

	private T CreateValue()
	{
		LazyHelper state = _state;
		if (state != null)
		{
			switch (state.State)
			{
			case LazyState.NoneViaConstructor:
				ViaConstructor();
				break;
			case LazyState.NoneViaFactory:
				ViaFactory(LazyThreadSafetyMode.None);
				break;
			case LazyState.PublicationOnlyViaConstructor:
				PublicationOnlyViaConstructor(state);
				break;
			case LazyState.PublicationOnlyViaFactory:
				PublicationOnlyViaFactory(state);
				break;
			case LazyState.PublicationOnlyWait:
				PublicationOnlyWaitForOtherThreadToPublish();
				break;
			case LazyState.ExecutionAndPublicationViaConstructor:
				ExecutionAndPublication(state, useDefaultConstructor: true);
				break;
			case LazyState.ExecutionAndPublicationViaFactory:
				ExecutionAndPublication(state, useDefaultConstructor: false);
				break;
			default:
				state.ThrowException();
				break;
			}
		}
		return Value;
	}

	/// <summary>Creates and returns a string representation of the <see cref="P:System.Lazy`1.Value" /> property for this instance.</summary>
	/// <returns>The result of calling the <see cref="M:System.Object.ToString" /> method on the <see cref="P:System.Lazy`1.Value" /> property for this instance, if the value has been created (that is, if the <see cref="P:System.Lazy`1.IsValueCreated" /> property returns true). Otherwise, a string indicating that the value has not been created. </returns>
	/// <exception cref="T:System.NullReferenceException">The <see cref="P:System.Lazy`1.Value" /> property is null.</exception>
	public override string ToString()
	{
		if (!IsValueCreated)
		{
			return "Value is not created.";
		}
		return Value.ToString();
	}
}
