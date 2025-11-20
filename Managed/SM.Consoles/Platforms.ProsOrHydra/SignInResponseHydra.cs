using Hydra.Api.Auth;

namespace Platforms.ProsOrHydra;

public class SignInResponseHydra : ISignInResponse
{
	private readonly SignInResponse _signInResponse;

	public long Date => _signInResponse.Date;

	public string ExternalIdentityToken => _signInResponse.ExternalIdentityToken;

	public string KernelSessionId => _signInResponse.UserContext.Data.KernelSessionId;

	public SignInResponseHydra(SignInResponse signInResponse)
	{
		_signInResponse = signInResponse;
	}
}
