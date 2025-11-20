namespace System.Data.OleDb;

/// <summary>Represents the method that will handle the <see cref="E:System.Data.OleDb.OleDbDataAdapter.RowUpdated" /> event of an <see cref="T:System.Data.OleDb.OleDbDataAdapter" />.</summary>
/// <param name="sender">The source of the event. </param>
/// <param name="e">The <see cref="T:System.Data.OleDb.OleDbRowUpdatedEventArgs" /> that contains the event data. </param>
/// <filterpriority>2</filterpriority>
public delegate void OleDbRowUpdatedEventHandler(object sender, OleDbRowUpdatedEventArgs e);
