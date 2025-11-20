using System.Collections;

namespace PlayEveryWare.EpicOnlineServices;

public interface IEOSCoroutineOwner
{
	void StartCoroutine(IEnumerator routine);
}
