using System.Runtime.InteropServices;

namespace Sony.NP;

public class WordFilter
{
	[StructLayout(LayoutKind.Sequential)]
	public class FilterCommentRequest : RequestBase
	{
		public const int MAX_SIZE_COMMENT = 1024;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1025)]
		internal string comment;

		public string Comment
		{
			get
			{
				return comment;
			}
			set
			{
				if (value.Length > 1024)
				{
					throw new NpToolkitException("The size of the string is more than " + 1024 + " characters.");
				}
				comment = value;
			}
		}

		public FilterCommentRequest()
			: base(ServiceTypes.WordFilter, FunctionTypes.WordfilterFilterComment)
		{
		}
	}

	public class SanitizedCommentResponse : ResponseBase
	{
		internal string resultComment;

		internal bool isCommentChanged;

		public string ResultComment => resultComment;

		public bool IsCommentChanged => isCommentChanged;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.WordFilterBegin);
			memoryBuffer.ReadString(ref resultComment);
			isCommentChanged = memoryBuffer.ReadBool();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.WordFilterEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxFilterComment(FilterCommentRequest request, out APIResult result);

	public static int FilterComment(FilterCommentRequest request, SanitizedCommentResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxFilterComment(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}
}
