using System;

namespace UnityEngine.UIElements;

internal static class DragAndDropUtility
{
	private static Func<IDragAndDrop> s_MakeClientFunc;

	private static IDragAndDrop s_DragAndDrop;

	public static IDragAndDrop dragAndDrop
	{
		get
		{
			if (s_DragAndDrop == null)
			{
				if (s_MakeClientFunc != null)
				{
					s_DragAndDrop = s_MakeClientFunc();
				}
				else
				{
					s_DragAndDrop = new DefaultDragAndDropClient();
				}
			}
			return s_DragAndDrop;
		}
	}

	internal static void RegisterMakeClientFunc(Func<IDragAndDrop> makeClient)
	{
		if (s_MakeClientFunc != null)
		{
			throw new UnityException("The MakeClientFunc has already been registered. Registration denied.");
		}
		s_MakeClientFunc = makeClient;
	}
}
