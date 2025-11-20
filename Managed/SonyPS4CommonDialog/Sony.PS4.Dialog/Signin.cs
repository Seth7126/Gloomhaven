using System.Runtime.InteropServices;

namespace Sony.PS4.Dialog;

public class Signin
{
	public enum EnumSigninDialogResult
	{
		RESULT_OK,
		RESULT_USER_CANCELED
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SigninDialogResult
	{
		public EnumSigninDialogResult result;
	}

	public static bool IsDialogOpen => PrxSigninDialogIsDialogOpen();

	public static event Messages.EventHandler OnGotSigninDialogResult;

	[DllImport("CommonDialog")]
	private static extern int PrxSigninDialogInitialise();

	[DllImport("CommonDialog")]
	private static extern void PrxSigninDialogUpdate();

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxSigninDialogIsDialogOpen();

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxSigninDialogOpen(int userId);

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxSigninDialogGetResult(out SigninDialogResult result);

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxCommonDialogHasMessage();

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxCommonDialogGetFirstMessage(out Messages.PluginMessage msg);

	[DllImport("CommonDialog")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool PrxCommonDialogRemoveFirstMessage();

	public static bool Open(int userId)
	{
		return PrxSigninDialogOpen(userId);
	}

	public static SigninDialogResult GetResult()
	{
		SigninDialogResult result = default(SigninDialogResult);
		PrxSigninDialogGetResult(out result);
		return result;
	}

	public static void ProcessMessage(Messages.PluginMessage msg)
	{
		Messages.MessageType type = msg.type;
		if (type == Messages.MessageType.kDialog_GotSigninDialogResult && Signin.OnGotSigninDialogResult != null)
		{
			Signin.OnGotSigninDialogResult(msg);
		}
	}

	public static void Initialise()
	{
		PrxSigninDialogInitialise();
	}

	public static void Update()
	{
		PrxSigninDialogUpdate();
	}
}
