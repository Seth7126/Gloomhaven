using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ICustomFieldDefinitionCollection : IReadOnlyCollection<ICustomFieldDefinition>, IEnumerable<ICustomFieldDefinition>, IEnumerable, IRefreshable
{
	Task<ICustomFieldDefinition> Add(string name, CustomFieldType type, CancellationToken ct = default(CancellationToken), params IDropDownOption[] options);
}
