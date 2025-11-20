namespace System.Drawing.Printing;

/// <summary>Represents the method that will handle the <see cref="E:System.Drawing.Printing.PrintDocument.BeginPrint" /> or <see cref="E:System.Drawing.Printing.PrintDocument.EndPrint" /> event of a <see cref="T:System.Drawing.Printing.PrintDocument" />.</summary>
/// <param name="sender">The source of the event. </param>
/// <param name="e">A <see cref="T:System.Drawing.Printing.PrintEventArgs" /> that contains the event data. </param>
public delegate void PrintEventHandler(object sender, PrintEventArgs e);
