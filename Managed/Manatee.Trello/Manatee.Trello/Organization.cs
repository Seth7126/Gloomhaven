using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class Organization : IOrganization, ICanWebhook, ICacheable, IQueryable, IRefreshable, IMergeJson<IJsonOrganization>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	[Flags]
	public enum Fields
	{
		[Display(Description = "desc")]
		Description = 1,
		[Display(Description = "displayName")]
		DisplayName = 2,
		[Display(Description = "logoHash")]
		LogoHash = 4,
		[Display(Description = "name")]
		Name = 8,
		[Display(Description = "prefs")]
		Preferences = 0x40,
		[Display(Description = "url")]
		Url = 0x80,
		[Display(Description = "website")]
		Website = 0x100,
		Actions = 0x200,
		Boards = 0x400,
		Members = 0x800,
		Memberships = 0x1000,
		PowerUpData = 0x2000
	}

	private readonly Field<string> _description;

	private readonly Field<string> _displayName;

	private readonly Field<bool> _isBusinessClass;

	private readonly Field<string> _name;

	private readonly Field<string> _url;

	private readonly Field<string> _website;

	private readonly OrganizationContext _context;

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
			OrganizationContext.UpdateParameters();
		}
	}

	public IReadOnlyActionCollection Actions => _context.Actions;

	public IBoardCollection Boards => _context.Boards;

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

	public string Description
	{
		get
		{
			return _description.Value;
		}
		set
		{
			_description.Value = value;
		}
	}

	public string DisplayName
	{
		get
		{
			return _displayName.Value;
		}
		set
		{
			_displayName.Value = value;
		}
	}

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

	public bool IsBusinessClass => _isBusinessClass.Value;

	public IReadOnlyMemberCollection Members => _context.Members;

	public IOrganizationMembershipCollection Memberships => _context.Memberships;

	public string Name
	{
		get
		{
			return _name.Value;
		}
		set
		{
			_name.Value = value;
		}
	}

	public IReadOnlyCollection<IPowerUpData> PowerUpData => _context.PowerUpData;

	public IOrganizationPreferences Preferences { get; }

	public string Url => _url.Value;

	public string Website
	{
		get
		{
			return _website.Value;
		}
		set
		{
			_website.Value = value;
		}
	}

	internal IJsonOrganization Json
	{
		get
		{
			return _context.Data;
		}
		set
		{
			_context.Merge(value);
		}
	}

	TrelloAuthorization IBatchRefresh.Auth => _context.Auth;

	public event Action<IOrganization, IEnumerable<string>> Updated;

	static Organization()
	{
		DownloadedFields = (Fields)(Enum.GetValues(typeof(Fields)).Cast<int>().Sum() & -2049);
	}

	public Organization(string id, TrelloAuthorization auth = null)
	{
		Id = id;
		_context = new OrganizationContext(id, auth);
		_context.Synchronized.Add(this);
		_description = new Field<string>(_context, "Description");
		_displayName = new Field<string>(_context, "DisplayName");
		_isBusinessClass = new Field<bool>(_context, "IsBusinessClass");
		_name = new Field<string>(_context, "Name");
		_name.AddRule(OrganizationNameRule.Instance);
		Preferences = new OrganizationPreferences(_context.OrganizationPreferencesContext);
		_url = new Field<string>(_context, "Url");
		_website = new Field<string>(_context, "Website");
		_website.AddRule(UriRule.Instance);
		if (auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
	}

	internal Organization(IJsonOrganization json, TrelloAuthorization auth)
		: this(json.Id, auth)
	{
		_context.Merge(json);
	}

	public void ApplyAction(IAction action)
	{
		Action action2 = action as Action;
		ActionType? type = action.Type;
		ActionType updateOrganization = ActionType.UpdateOrganization;
		if (type.HasValue && (!type.HasValue || !(type.GetValueOrDefault() != updateOrganization)) && action2?.Json?.Data?.Org != null && !(action2.Json.Data.Org.Id != Id))
		{
			_context.Merge(action2.Json.Data.Org);
		}
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

	void IMergeJson<IJsonOrganization>.Merge(IJsonOrganization json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonOrganization json = TrelloConfiguration.Deserializer.Deserialize<IJsonOrganization>(content);
		_context.Merge(json);
	}

	public override string ToString()
	{
		return DisplayName;
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		Id = _context.Data.Id;
		this.Updated?.Invoke(this, properties);
	}
}
