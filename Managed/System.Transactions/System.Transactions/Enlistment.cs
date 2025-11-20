namespace System.Transactions;

/// <summary>Facilitates communication between an enlisted transaction participant and the transaction manager during the final phase of the transaction.</summary>
public class Enlistment
{
	internal bool done;

	internal Enlistment()
	{
		done = false;
	}

	/// <summary>Indicates that the transaction participant has completed its work.</summary>
	public void Done()
	{
		done = true;
		InternalOnDone();
	}

	internal virtual void InternalOnDone()
	{
	}
}
