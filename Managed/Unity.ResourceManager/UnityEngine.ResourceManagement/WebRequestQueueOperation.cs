using System;
using UnityEngine.Networking;

namespace UnityEngine.ResourceManagement;

internal class WebRequestQueueOperation
{
	private bool m_Completed;

	public UnityWebRequestAsyncOperation Result;

	public Action<UnityWebRequestAsyncOperation> OnComplete;

	internal UnityWebRequest m_WebRequest;

	public bool IsDone
	{
		get
		{
			if (!m_Completed)
			{
				return Result != null;
			}
			return true;
		}
	}

	public WebRequestQueueOperation(UnityWebRequest request)
	{
		m_WebRequest = request;
	}

	internal void Complete(UnityWebRequestAsyncOperation asyncOp)
	{
		m_Completed = true;
		Result = asyncOp;
		OnComplete?.Invoke(Result);
	}
}
