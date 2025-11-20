using System.Collections.Generic;

namespace Manatee.Trello.Internal.Synchronization;

internal interface IHandleSynchronization
{
	void HandleSynchronized(IEnumerable<string> properties);
}
