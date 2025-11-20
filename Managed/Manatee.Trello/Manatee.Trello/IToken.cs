using System;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IToken : ICacheable, IRefreshable
{
	string AppName { get; }

	ITokenPermission BoardPermissions { get; }

	DateTime CreationDate { get; }

	DateTime? DateCreated { get; }

	DateTime? DateExpires { get; }

	IMember Member { get; }

	ITokenPermission MemberPermissions { get; }

	ITokenPermission OrganizationPermissions { get; }

	Task Delete(CancellationToken ct = default(CancellationToken));
}
