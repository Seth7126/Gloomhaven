using System.Runtime.InteropServices;

namespace Sony.PS4.Dialog;

public class Main
{
	public static event Messages.EventHandler OnLog;

	public static event Messages.EventHandler OnLogWarning;

	public static event Messages.EventHandler OnLogError;

	[DllImport("CommonDialog")]
	private static extern int PrxCommonDialogInitialise();

	[DllImport("CommonDialog")]
	private static extern int PrxCommonDialogUpdate();

	public static void Initialise()
	{
		PrxCommonDialogInitialise();
	}

	public static void Update()
	{
		PrxCommonDialogUpdate();
		PumpMessages();
	}

	private static void PumpMessages()
	{
		while (Messages.HasMessage())
		{
			Messages.PluginMessage msg = default(Messages.PluginMessage);
			Messages.GetFirstMessage(out msg);
			Common.ProcessMessage(msg);
			Ime.ProcessMessage(msg);
			Signin.ProcessMessage(msg);
			WebBrowser.ProcessMessage(msg);
			switch (msg.type)
			{
			case Messages.MessageType.kDialog_Log:
				if (Main.OnLog != null)
				{
					Main.OnLog(msg);
				}
				break;
			case Messages.MessageType.kDialog_LogWarning:
				if (Main.OnLogWarning != null)
				{
					Main.OnLogWarning(msg);
				}
				break;
			case Messages.MessageType.kDialog_LogError:
				if (Main.OnLogError != null)
				{
					Main.OnLogError(msg);
				}
				break;
			}
			Messages.RemoveFirstMessage();
		}
	}
}
