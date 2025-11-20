using System;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ICustomFieldDefinition : ICacheable, IRefreshable
{
	IBoard Board { get; }

	ICustomFieldDisplayInfo DisplayInfo { get; }

	string FieldGroup { get; }

	string Name { get; set; }

	Position Position { get; set; }

	CustomFieldType? Type { get; }

	IDropDownOptionCollection Options { get; }

	Task Delete(CancellationToken ct = default(CancellationToken));

	Task<ICustomField<double?>> SetValueForCard(ICard card, double? value, CancellationToken ct = default(CancellationToken));

	Task<ICustomField<bool?>> SetValueForCard(ICard card, bool? value, CancellationToken ct = default(CancellationToken));

	Task<ICustomField<string>> SetValueForCard(ICard card, string value, CancellationToken ct = default(CancellationToken));

	Task<ICustomField<IDropDownOption>> SetValueForCard(ICard card, IDropDownOption value, CancellationToken ct = default(CancellationToken));

	Task<ICustomField<DateTime?>> SetValueForCard(ICard card, DateTime? value, CancellationToken ct = default(CancellationToken));
}
