using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class Token : IToken, ICacheable, IRefreshable, IMergeJson<IJsonToken>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	[Flags]
	public enum Fields
	{
		[Display(Description = "identifier")]
		Id = 1,
		[Display(Description = "idMember")]
		Member = 2,
		[Display(Description = "dateCreated")]
		DateCreated = 4,
		[Display(Description = "dateExpires")]
		DateExpires = 8,
		[Display(Description = "permissions")]
		Permissions = 0x10
	}

	private readonly Field<string> _appName;

	private readonly Field<DateTime?> _dateCreated;

	private readonly Field<DateTime?> _dateExpires;

	private readonly Field<Member> _member;

	private readonly TokenContext _context;

	private string _id;

	private DateTime? _creation;

	private static Fields _downloadedFields;

	public static Fields DownloadedFields
	{
		get
		{
			return _downloadedFields;
		}
		set
		{
			_downloadedFields = value;
			TokenContext.UpdateParameters();
		}
	}

	public string AppName => _appName.Value;

	public ITokenPermission BoardPermissions { get; }

	public DateTime CreationDate
	{
		get
		{
			if (!_creation.HasValue)
			{
				_creation = Id.ExtractCreationDate();
			}
			return _creation.Value;
		}
	}

	public DateTime? DateCreated => _dateCreated.Value;

	public DateTime? DateExpires => _dateExpires.Value;

	public string Id
	{
		get
		{
			if (!_context.HasValidId)
			{
				_context.Synchronize(force: true, CancellationToken.None).Wait();
			}
			return _id;
		}
		private set
		{
			_id = value;
		}
	}

	public IMember Member => _member.Value;

	public ITokenPermission MemberPermissions { get; }

	public ITokenPermission OrganizationPermissions { get; }

	TrelloAuthorization IBatchRefresh.Auth => _context.Auth;

	static Token()
	{
		DownloadedFields = (Fields)Enum.GetValues(typeof(Fields)).Cast<int>().Sum();
	}

	public Token(string id, TrelloAuthorization auth = null)
	{
		Id = id;
		_context = new TokenContext(id, auth);
		_context.Synchronized.Add(this);
		_appName = new Field<string>(_context, "AppName");
		BoardPermissions = new TokenPermission(_context.BoardPermissions);
		_dateCreated = new Field<DateTime?>(_context, "DateCreated");
		_dateExpires = new Field<DateTime?>(_context, "DateExpires");
		_member = new Field<Member>(_context, "Member");
		MemberPermissions = new TokenPermission(_context.MemberPermissions);
		OrganizationPermissions = new TokenPermission(_context.OrganizationPermissions);
		if (auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
	}

	internal Token(IJsonToken json, TrelloAuthorization auth)
		: this(json.Id, auth)
	{
		_context.Merge(json);
	}

	public async Task Delete(CancellationToken ct = default(CancellationToken))
	{
		await _context.Delete(ct);
		if (TrelloConfiguration.RemoveDeletedItemsFromCache)
		{
			TrelloConfiguration.Cache.Remove(this);
		}
	}

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return _context.Synchronize(force, ct);
	}

	void IMergeJson<IJsonToken>.Merge(IJsonToken json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonToken json = TrelloConfiguration.Deserializer.Deserialize<IJsonToken>(content);
		_context.Merge(json);
	}

	public override string ToString()
	{
		return AppName;
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		Id = _context.Data.Id;
	}
}
