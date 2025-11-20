namespace Platforms.ProsOrHydra;

public interface ISignInResponse
{
	long Date { get; }

	string ExternalIdentityToken { get; }

	string KernelSessionId { get; }
}
