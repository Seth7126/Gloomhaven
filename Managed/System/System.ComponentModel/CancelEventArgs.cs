namespace System.ComponentModel;

/// <summary>Provides data for a cancelable event.</summary>
public class CancelEventArgs : EventArgs
{
	/// <summary>Gets or sets a value indicating whether the event should be canceled.</summary>
	/// <returns>true if the event should be canceled; otherwise, false.</returns>
	public bool Cancel { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.CancelEventArgs" /> class with the <see cref="P:System.ComponentModel.CancelEventArgs.Cancel" /> property set to false.</summary>
	public CancelEventArgs()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.CancelEventArgs" /> class with the <see cref="P:System.ComponentModel.CancelEventArgs.Cancel" /> property set to the given value.</summary>
	/// <param name="cancel">true to cancel the event; otherwise, false. </param>
	public CancelEventArgs(bool cancel)
	{
		Cancel = cancel;
	}
}
