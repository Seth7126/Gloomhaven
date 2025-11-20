using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class DropDownOptionCollection : ReadOnlyDropDownOptionCollection, IDropDownOptionCollection, IReadOnlyCollection<IDropDownOption>, IEnumerable<IDropDownOption>, IEnumerable, IRefreshable
{
	internal DropDownOptionCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	public async Task<IDropDownOption> Add(string text, Position position, LabelColor? color = null, CancellationToken ct = default(CancellationToken))
	{
		string text2 = NotNullRule<string>.Instance.Validate(null, text);
		if (text2 != null)
		{
			throw new ValidationException<string>(text, new string[1] { text2 });
		}
		IJsonCustomDropDownOption jsonCustomDropDownOption = TrelloConfiguration.JsonFactory.Create<IJsonCustomDropDownOption>();
		jsonCustomDropDownOption.Color = color;
		jsonCustomDropDownOption.Pos = Position.GetJson(position);
		jsonCustomDropDownOption.Text = text;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.CustomFieldDefinition_Write_AddOption, new Dictionary<string, object> { { "_id", base.OwnerId } });
		return new DropDownOption(await JsonRepository.Execute(base.Auth, endpoint, jsonCustomDropDownOption, ct), base.Auth);
	}
}
