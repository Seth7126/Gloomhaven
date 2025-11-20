namespace System.Data;

/// <summary>Represents the method that will handle the <see cref="E:System.Data.DataTable.RowChanging" />, <see cref="E:System.Data.DataTable.RowChanged" />, <see cref="E:System.Data.DataTable.RowDeleting" />, and <see cref="E:System.Data.DataTable.RowDeleted" /> events of a <see cref="T:System.Data.DataTable" />.</summary>
/// <param name="sender">The source of the event. </param>
/// <param name="e">A <see cref="T:System.Data.DataRowChangeEventArgs" /> that contains the event data. </param>
/// <filterpriority>2</filterpriority>
public delegate void DataRowChangeEventHandler(object sender, DataRowChangeEventArgs e);
