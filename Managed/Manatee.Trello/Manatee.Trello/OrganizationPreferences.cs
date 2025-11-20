using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;

namespace Manatee.Trello;

public class OrganizationPreferences : IOrganizationPreferences
{
	private readonly Field<OrganizationPermissionLevel?> _permissionLevel;

	private readonly Field<bool?> _externalMembersDisabled;

	private readonly Field<string> _assocatedDomain;

	private readonly Field<OrganizationBoardVisibility?> _publicBoardVisibility;

	private readonly Field<OrganizationBoardVisibility?> _organizationBoardVisibility;

	private readonly Field<OrganizationBoardVisibility?> _privateBoardVisibility;

	private OrganizationPreferencesContext _context;

	public OrganizationPermissionLevel? PermissionLevel
	{
		get
		{
			return _permissionLevel.Value;
		}
		set
		{
			_permissionLevel.Value = value;
		}
	}

	public bool? ExternalMembersDisabled
	{
		get
		{
			return _externalMembersDisabled.Value;
		}
		set
		{
			_externalMembersDisabled.Value = value;
		}
	}

	public string AssociatedDomain
	{
		get
		{
			return _assocatedDomain.Value;
		}
		set
		{
			_assocatedDomain.Value = value;
		}
	}

	public OrganizationBoardVisibility? PublicBoardVisibility
	{
		get
		{
			return _publicBoardVisibility.Value;
		}
		set
		{
			_publicBoardVisibility.Value = value;
		}
	}

	public OrganizationBoardVisibility? OrganizationBoardVisibility
	{
		get
		{
			return _organizationBoardVisibility.Value;
		}
		set
		{
			_organizationBoardVisibility.Value = value;
		}
	}

	public OrganizationBoardVisibility? PrivateBoardVisibility
	{
		get
		{
			return _privateBoardVisibility.Value;
		}
		set
		{
			_privateBoardVisibility.Value = value;
		}
	}

	internal OrganizationPreferences(OrganizationPreferencesContext context)
	{
		_context = context;
		_permissionLevel = new Field<OrganizationPermissionLevel?>(_context, "PermissionLevel");
		_permissionLevel.AddRule(NullableHasValueRule<OrganizationPermissionLevel>.Instance);
		_permissionLevel.AddRule(EnumerationRule<OrganizationPermissionLevel?>.Instance);
		_externalMembersDisabled = new Field<bool?>(_context, "ExternalMembersDisabled");
		_externalMembersDisabled.AddRule(NullableHasValueRule<bool>.Instance);
		_assocatedDomain = new Field<string>(_context, "AssociatedDomain");
		_publicBoardVisibility = new Field<OrganizationBoardVisibility?>(_context, "PublicBoardVisibility");
		_publicBoardVisibility.AddRule(NullableHasValueRule<Manatee.Trello.OrganizationBoardVisibility>.Instance);
		_publicBoardVisibility.AddRule(EnumerationRule<Manatee.Trello.OrganizationBoardVisibility?>.Instance);
		_organizationBoardVisibility = new Field<OrganizationBoardVisibility?>(_context, "OrganizationBoardVisibility");
		_organizationBoardVisibility.AddRule(NullableHasValueRule<Manatee.Trello.OrganizationBoardVisibility>.Instance);
		_organizationBoardVisibility.AddRule(EnumerationRule<Manatee.Trello.OrganizationBoardVisibility?>.Instance);
		_privateBoardVisibility = new Field<OrganizationBoardVisibility?>(_context, "PrivateBoardVisibility");
		_privateBoardVisibility.AddRule(NullableHasValueRule<Manatee.Trello.OrganizationBoardVisibility>.Instance);
		_privateBoardVisibility.AddRule(EnumerationRule<Manatee.Trello.OrganizationBoardVisibility?>.Instance);
	}
}
