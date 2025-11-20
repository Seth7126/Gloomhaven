namespace System.Data.Odbc;

/// <summary>Represents the method that will handle the <see cref="E:System.Data.Odbc.OdbcDataAdapter.RowUpdating" /> event of an <see cref="T:System.Data.Odbc.OdbcDataAdapter" />.</summary>
/// <param name="sender">The source of the event. </param>
/// <param name="e">The <see cref="T:System.Data.Odbc.OdbcRowUpdatingEventArgs" /> that contains the event data. </param>
/// <filterpriority>2</filterpriority>
public delegate void OdbcRowUpdatingEventHandler(object sender, OdbcRowUpdatingEventArgs e);
