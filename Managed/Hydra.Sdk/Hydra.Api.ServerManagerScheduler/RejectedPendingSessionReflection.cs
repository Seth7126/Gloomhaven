using System;
using Google.Protobuf.Reflection;
using Hydra.Api.SessionControl;

namespace Hydra.Api.ServerManagerScheduler;

public static class RejectedPendingSessionReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static RejectedPendingSessionReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("CjNTZXJ2ZXJNYW5hZ2VyU2NoZWR1bGVyL1JlamVjdGVkUGVuZGluZ1Nlc3Np" + "b24ucHJvdG8SIEh5ZHJhLkFwaS5TZXJ2ZXJNYW5hZ2VyU2NoZWR1bGVyGitT" + "ZXJ2ZXJNYW5hZ2VyU2NoZWR1bGVyL1BlbmRpbmdTZXNzaW9uLnByb3RvIsIB" + "ChZSZWplY3RlZFBlbmRpbmdTZXNzaW9uEg4KBmRzbV9pZBgBIAEoCRIWCg5k" + "YXRhX2NlbnRlcl9pZBgCIAEoCRI5CgdzZXNzaW9uGAMgASgLMiguSHlkcmEu" + "QXBpLlNlc3Npb25Db250cm9sLlBlbmRpbmdTZXNzaW9uEkUKBnJlYXNvbhgE" + "IAEoDjI1Lkh5ZHJhLkFwaS5TZXJ2ZXJNYW5hZ2VyU2NoZWR1bGVyLlNlc3Np" + "b25SZWplY3RSZWFzb24qjgEKE1Nlc3Npb25SZWplY3RSZWFzb24SIQodU0VT" + "U0lPTl9SRUpFQ1RfUkVBU09OX1VOS05PV04QABIkCiBTRVNTSU9OX1JFSkVD" + "VF9SRUFTT05fTk9fVkVSU0lPThABEi4KKlNFU1NJT05fUkVKRUNUX1JFQVNP" + "Tl9OT1RfRU5PVUdIX1JFU09VUkNFUxACYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[1] { PendingSessionReflection.Descriptor }, new GeneratedClrTypeInfo(new Type[1] { typeof(SessionRejectReason) }, null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RejectedPendingSession), RejectedPendingSession.Parser, new string[4] { "DsmId", "DataCenterId", "Session", "Reason" }, null, null, null, null)
		}));
	}
}
