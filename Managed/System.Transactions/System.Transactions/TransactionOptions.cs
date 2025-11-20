namespace System.Transactions;

/// <summary>Contains additional information that specifies transaction behaviors.</summary>
public struct TransactionOptions
{
	private IsolationLevel level;

	private TimeSpan timeout;

	/// <summary>Gets or sets the isolation level of the transaction.</summary>
	/// <returns>A <see cref="T:System.Transactions.IsolationLevel" /> enumeration that specifies the isolation level of the transaction.</returns>
	public IsolationLevel IsolationLevel
	{
		get
		{
			return level;
		}
		set
		{
			level = value;
		}
	}

	/// <summary>Gets or sets the timeout period for the transaction.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> value that specifies the timeout period for the transaction.</returns>
	public TimeSpan Timeout
	{
		get
		{
			return timeout;
		}
		set
		{
			timeout = value;
		}
	}

	internal TransactionOptions(IsolationLevel level, TimeSpan timeout)
	{
		this.level = level;
		this.timeout = timeout;
	}

	/// <summary>Tests whether two specified <see cref="T:System.Transactions.TransactionOptions" /> instances are equivalent.</summary>
	/// <returns>true if <paramref name="x" /> and <paramref name="y" /> are equal; otherwise, false.</returns>
	/// <param name="x">The <see cref="T:System.Transactions.TransactionOptions" /> instance that is to the left of the equality operator.</param>
	/// <param name="y">The <see cref="T:System.Transactions.TransactionOptions" /> instance that is to the right of the equality operator.</param>
	public static bool operator ==(TransactionOptions x, TransactionOptions y)
	{
		if (x.level == y.level)
		{
			return x.timeout == y.timeout;
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether two <see cref="T:System.Transactions.TransactionOptions" /> instances are not equal.</summary>
	/// <returns>true if <paramref name="x" /> and <paramref name="y" /> are not equal; otherwise, false.</returns>
	/// <param name="x">The <see cref="T:System.Transactions.TransactionOptions" /> instance that is to the left of the equality operator.</param>
	/// <param name="y">The <see cref="T:System.Transactions.TransactionOptions" /> instance that is to the right of the equality operator.</param>
	public static bool operator !=(TransactionOptions x, TransactionOptions y)
	{
		if (x.level == y.level)
		{
			return x.timeout != y.timeout;
		}
		return true;
	}

	/// <summary>Determines whether this <see cref="T:System.Transactions.TransactionOptions" /> instance and the specified object are equal.</summary>
	/// <returns>true if <paramref name="obj" /> and this <see cref="T:System.Transactions.TransactionOptions" /> instance are identical; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this instance.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is TransactionOptions))
		{
			return false;
		}
		return this == (TransactionOptions)obj;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return (int)level ^ timeout.GetHashCode();
	}
}
