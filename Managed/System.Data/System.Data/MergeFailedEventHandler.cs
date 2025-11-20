namespace System.Data;

/// <summary>Represents the method that will handle the <see cref="E:System.Data.DataSet.MergeFailed" /> event.</summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">The data for the event.</param>
/// <filterpriority>2</filterpriority>
public delegate void MergeFailedEventHandler(object sender, MergeFailedEventArgs e);
