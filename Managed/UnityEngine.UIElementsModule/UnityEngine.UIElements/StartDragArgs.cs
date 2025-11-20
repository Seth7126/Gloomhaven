using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class StartDragArgs
{
	private readonly Hashtable m_GenericData = new Hashtable();

	public string title { get; }

	public object userData { get; }

	internal Hashtable genericData => m_GenericData;

	internal IEnumerable<Object> unityObjectReferences { get; private set; } = null;

	internal StartDragArgs()
	{
		title = string.Empty;
	}

	public StartDragArgs(string title, object userData)
	{
		this.title = title;
		this.userData = userData;
	}

	public void SetGenericData(string key, object data)
	{
		m_GenericData[key] = data;
	}

	public void SetUnityObjectReferences(IEnumerable<Object> references)
	{
		unityObjectReferences = references;
	}
}
