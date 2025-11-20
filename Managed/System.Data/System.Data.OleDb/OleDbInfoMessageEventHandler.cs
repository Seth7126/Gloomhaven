namespace System.Data.OleDb;

/// <summary>Represents the method that will handle the <see cref="E:System.Data.OleDb.OleDbConnection.InfoMessage" /> event of an <see cref="T:System.Data.OleDb.OleDbConnection" />.</summary>
/// <param name="sender">The source of the event. </param>
/// <param name="e">An <see cref="T:System.Data.OleDb.OleDbInfoMessageEventArgs" /> object that contains the event data. </param>
/// <filterpriority>2</filterpriority>
public delegate void OleDbInfoMessageEventHandler(object sender, OleDbInfoMessageEventArgs e);
