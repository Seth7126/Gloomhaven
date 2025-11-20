namespace System.Data;

/// <summary>Represents the method that will handle the <see cref="E:System.Data.DataTable.ColumnChanging" /> event.</summary>
/// <param name="sender">The source of the event. </param>
/// <param name="e">A <see cref="T:System.Data.DataColumnChangeEventArgs" /> that contains the event data. </param>
/// <filterpriority>2</filterpriority>
public delegate void DataColumnChangeEventHandler(object sender, DataColumnChangeEventArgs e);
