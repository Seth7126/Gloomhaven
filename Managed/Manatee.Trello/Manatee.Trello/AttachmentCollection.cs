using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;
using Manatee.Trello.Rest;

namespace Manatee.Trello;

public class AttachmentCollection : ReadOnlyAttachmentCollection, IAttachmentCollection, IReadOnlyCollection<IAttachment>, IEnumerable<IAttachment>, IEnumerable, IRefreshable
{
	internal AttachmentCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	public async Task<IAttachment> Add(string url, string name = null, CancellationToken ct = default(CancellationToken))
	{
		List<string> list = new List<string>
		{
			NotNullOrWhiteSpaceRule.Instance.Validate(null, url),
			UriRule.Instance.Validate(null, url)
		};
		if (list.Any((string e) => e != null))
		{
			throw new ValidationException<string>(url, list);
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object> { { "url", url } };
		if (!name.IsNullOrWhiteSpace())
		{
			dictionary.Add("name", name);
		}
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Write_AddAttachment, new Dictionary<string, object> { { "_id", base.OwnerId } });
		return new Attachment(await JsonRepository.Execute<IJsonAttachment>(base.Auth, endpoint, ct, dictionary), base.OwnerId, base.Auth);
	}

	public async Task<IAttachment> Add(byte[] data, string name, CancellationToken ct = default(CancellationToken))
	{
		Dictionary<string, object> parameters = new Dictionary<string, object> { 
		{
			"file",
			new RestFile
			{
				ContentBytes = data,
				FileName = name
			}
		} };
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Write_AddAttachment, new Dictionary<string, object> { { "_id", base.OwnerId } });
		return new Attachment(await JsonRepository.Execute<IJsonAttachment>(base.Auth, endpoint, ct, parameters), base.OwnerId, base.Auth);
	}
}
