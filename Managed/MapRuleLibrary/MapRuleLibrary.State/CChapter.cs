using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace MapRuleLibrary.State;

[Serializable]
public class CChapter : ISerializable
{
	public int Chapter;

	public int SubChapter;

	public CChapter()
	{
	}

	public CChapter(CChapter state, ReferenceDictionary references)
	{
		Chapter = state.Chapter;
		SubChapter = state.SubChapter;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Chapter", Chapter);
		info.AddValue("SubChapter", SubChapter);
	}

	public CChapter(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "Chapter"))
				{
					if (name == "SubChapter")
					{
						SubChapter = info.GetInt32("SubChapter");
					}
				}
				else
				{
					Chapter = info.GetInt32("Chapter");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CChapter entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CChapter(int chapter, int subChapter)
	{
		Chapter = chapter;
		SubChapter = subChapter;
	}
}
