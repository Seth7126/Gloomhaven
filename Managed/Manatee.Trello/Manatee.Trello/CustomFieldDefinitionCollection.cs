using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class CustomFieldDefinitionCollection : ReadOnlyCustomFieldDefinitionCollection, ICustomFieldDefinitionCollection, IReadOnlyCollection<ICustomFieldDefinition>, IEnumerable<ICustomFieldDefinition>, IEnumerable, IRefreshable
{
	internal CustomFieldDefinitionCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	public async Task<ICustomFieldDefinition> Add(string name, CustomFieldType type, CancellationToken ct = default(CancellationToken), params IDropDownOption[] options)
	{
		string text = NotNullRule<string>.Instance.Validate(null, name);
		if (text != null)
		{
			throw new ValidationException<string>(name, new string[1] { text });
		}
		if (type == CustomFieldType.Unknown)
		{
			throw new ValidationException<CustomFieldType>(type, new string[0]);
		}
		IJsonCustomFieldDefinition jsonCustomFieldDefinition = TrelloConfiguration.JsonFactory.Create<IJsonCustomFieldDefinition>();
		jsonCustomFieldDefinition.Name = name;
		jsonCustomFieldDefinition.Board = TrelloConfiguration.JsonFactory.Create<IJsonBoard>();
		jsonCustomFieldDefinition.Board.Id = base.OwnerId;
		jsonCustomFieldDefinition.Type = type;
		jsonCustomFieldDefinition.Display = TrelloConfiguration.JsonFactory.Create<IJsonCustomFieldDisplayInfo>();
		jsonCustomFieldDefinition.Display.CardFront = true;
		if (type == CustomFieldType.DropDown)
		{
			jsonCustomFieldDefinition.Options = options.Select(delegate(IDropDownOption o, int i)
			{
				IJsonCustomDropDownOption jsonCustomDropDownOption = TrelloConfiguration.JsonFactory.Create<IJsonCustomDropDownOption>();
				jsonCustomDropDownOption.Color = o.Color;
				jsonCustomDropDownOption.Pos = TrelloConfiguration.JsonFactory.Create<IJsonPosition>();
				jsonCustomDropDownOption.Pos.Explicit = i * 1024;
				jsonCustomDropDownOption.Text = o.Text;
				return jsonCustomDropDownOption;
			}).ToList();
		}
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.CustomFieldDefinition_Write_Create);
		return new CustomFieldDefinition(await JsonRepository.Execute(base.Auth, endpoint, jsonCustomFieldDefinition, ct), base.Auth);
	}
}
