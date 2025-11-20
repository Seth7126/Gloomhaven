using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hydra.Api.Auth;
using Hydra.Api.EndpointDispatcher;
using Hydra.Api.Errors;
using Hydra.Api.Infrastructure.Context;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Enums;
using Hydra.Sdk.Errors;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Helpers;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;
using UnityEngine;

namespace Hydra.Sdk.Components.Authorization;

public sealed class AuthorizationComponent : IHydraSdkComponent
{
	private IConnectionManager _connectionManager;

	private IHydraSdkLogger _logger;

	private StateObserver<SdkState> _sdkState;

	private EndpointDispatcherApi.EndpointDispatcherApiClient _apiEds;

	private AuthorizationApi.AuthorizationApiClient _apiAuth;

	private StateObserver<RequestContextWrapper> _requestContext;

	private StateObserver<UserContextWrapper> _userContext;

	private StateObserver<ToolContextWrapper> _toolContext;

	private StateObserver<ClientInfo> _clientInfo;

	private StateObserver<SdkSessionInfo> _sessionInfo;

	private StateObserver<StandaloneServerWrapper> _standaloneInfo;

	private SdkSessionInfo _tempSession;

	private DateTime _refreshTime;

	private bool _isRefreshing;

	public string UserId => _userContext.State?.Context?.Data?.UserIdentity;

	public string KernelSessionId { get; private set; }

	public bool IsRefreshRequired => _sdkState?.State != null && _sdkState.State.State == OnlineState.Online && !_sdkState.State.Suspended && DateTime.UtcNow > _refreshTime;

	public int GetDisposePriority()
	{
		return 255;
	}

	public Task Register(IConnectionManager connectionManager, ComponentMessager messageHandler, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		_connectionManager = connectionManager;
		_logger = logger;
		_sdkState = stateResolver.CreateLinkedObserver<SdkState>();
		_requestContext = stateResolver.CreateLinkedObserver<RequestContextWrapper>();
		_userContext = stateResolver.CreateLinkedObserver<UserContextWrapper>();
		_toolContext = stateResolver.CreateLinkedObserver<ToolContextWrapper>();
		_clientInfo = stateResolver.CreateLinkedObserver<ClientInfo>();
		_sessionInfo = stateResolver.CreateLinkedObserver<SdkSessionInfo>();
		_standaloneInfo = stateResolver.CreateLinkedObserver<StandaloneServerWrapper>();
		Uri uri = new Uri(_clientInfo.State.HydraEndpoint);
		ServiceInfo serviceInfo = ServiceHelper.KnownServices.FirstOrDefault((ServiceInfo f) => f.Name == EndpointDispatcherApi.Descriptor.FullName);
		_connectionManager.AddConnections(uri.ToEndpointInfo(EndpointDispatcherApi.Descriptor.FullName, serviceInfo.Version));
		_apiEds = _connectionManager.GetConnection<EndpointDispatcherApi.EndpointDispatcherApiClient>();
		return Task.CompletedTask;
	}

	private void ValidateAndInitializeSession(bool skipStateCheck = false)
	{
		if (!skipStateCheck)
		{
			SdkState state = _sdkState.State;
			if (state == null || state.State != OnlineState.Offline)
			{
				throw new HydraSdkException(ErrorCode.SdkInvalidState, "Already online.");
			}
		}
		if (_apiAuth == null)
		{
			_apiAuth = _connectionManager.GetConnection<AuthorizationApi.AuthorizationApiClient>();
		}
		_tempSession = new SdkSessionInfo(_sessionInfo.State)
		{
			UserId = null,
			SessionId = null,
			StartTime = DateTime.UtcNow
		};
	}

	public async Task<GetEnvironmentInfoResponse> GetEnvironmentInfo()
	{
		ServiceInfo serviceInfo = ServiceHelper.Get(AuthorizationApi.Descriptor.FullName);
		GetEnvironmentInfoResponse response = await _apiEds.GetEnvironmentInfoAsync(new GetEnvironmentInfoRequest
		{
			Authorization = serviceInfo.AsServiceIdentity(),
			Title = new TitleIdentity
			{
				TitleId = _clientInfo.State.TitleId,
				TitleSecret = _clientInfo.State.SecretKey
			},
			BuildVersion = serviceInfo.BuildInfo?.Version
		});
		_tempSession = new SdkSessionInfo(_sessionInfo.State);
		_tempSession.EnvironmentId = response.Environment.Id;
		_tempSession.TitleId = _clientInfo.State.TitleId;
		_connectionManager.AddConnections(response.Environment.Endpoint);
		return response;
	}

	private SignInData GetSignInData()
	{
		SignInData signInData = new SignInData
		{
			Title = new TitleIdentity
			{
				TitleId = _clientInfo.State.TitleId,
				TitleSecret = _clientInfo.State.SecretKey
			},
			BuildPlatform = Platform.Net,
			RuntimePlatform = new PlatformDetails
			{
				PlatformInternalId = 0,
				PlatformDescription = (_clientInfo.State.Platform ?? string.Empty)
			},
			BuildVersion = _clientInfo.State.ClientVersion,
			Versions = { ServiceHelper.KnownServices.Select((ServiceInfo s) => s.AsServiceIdentity()) }
		};
		signInData.BuildPlatform = Platform.Unity;
		signInData.RuntimePlatform.PlatformInternalId = (int)Application.platform;
		if (string.IsNullOrEmpty(signInData.RuntimePlatform.PlatformDescription))
		{
			signInData.RuntimePlatform.PlatformDescription = Application.platform.ToString();
		}
		return signInData;
	}

	private Task SignInProcess(HydraRequestContext requestContext, IEnumerable<EndpointInfo> endpoints, UserContext userContext = null, ToolContext toolContext = null, string kernelSessionId = null, int tokenRefreshAfterSeconds = -1, long timestamp = -1L, long expiresAt = -1L)
	{
		if (timestamp > 0)
		{
			TimeHelper.Initialize(timestamp);
		}
		_connectionManager.UpdateToken(requestContext.Token);
		_connectionManager.AddConnections(endpoints.ToArray());
		if (tokenRefreshAfterSeconds != -1)
		{
			_refreshTime = DateTime.UtcNow.AddSeconds(tokenRefreshAfterSeconds);
		}
		else if (expiresAt != -1)
		{
			_refreshTime = expiresAt.FromUnixMilliseconds().AddSeconds(-10.0);
		}
		KernelSessionId = kernelSessionId;
		_tempSession.SessionId = KernelSessionId;
		_tempSession.StartTime = timestamp.FromUnixMilliseconds();
		_sessionInfo.Update(_tempSession);
		_requestContext.Update(new RequestContextWrapper(requestContext));
		if (userContext != null)
		{
			_tempSession.UserId = userContext.Data.UserIdentity;
			_userContext.Update(new UserContextWrapper(userContext));
		}
		else if (toolContext != null)
		{
			_toolContext.Update(new ToolContextWrapper(toolContext));
		}
		_sdkState.Update(new SdkState(OnlineState.Online, _sdkState.State.Suspended));
		return Task.CompletedTask;
	}

	public async Task<bool> TryRefreshAuthSession()
	{
		if (IsRefreshRequired && !_isRefreshing)
		{
			_isRefreshing = true;
			try
			{
				if (_userContext.State?.Context != null)
				{
					RefreshUserResponse result = await _apiAuth.RefreshUserAsync(new RefreshUserRequest
					{
						RequestContext = _requestContext.State.Context,
						UserContext = _userContext.State.Context
					});
					_connectionManager.UpdateToken(result.RequestContext.Token);
					_requestContext.Update(new RequestContextWrapper(result.RequestContext));
					_refreshTime = DateTime.UtcNow.AddSeconds(result.RefreshAfterSeconds);
					return true;
				}
				if (_toolContext.State?.Context != null)
				{
					await SignInTool();
					return true;
				}
			}
			catch (Exception ex)
			{
				Exception ex2 = ex;
				_logger.Log(HydraLogType.Error, this.GetLogCatErr(), "Failed to refresh session, reconnect is required. Error: {0}", ex2.GetErrorMessage());
				_sdkState.Update(new SdkState(OnlineState.Offline, _sdkState.State.Suspended));
			}
			finally
			{
				_isRefreshing = false;
			}
		}
		return false;
	}

	public async Task SignOutUser()
	{
		if (_sdkState.State.State == OnlineState.Online && _userContext.State != null)
		{
			await _apiAuth.SignOutUserAsync(new SignOutUserRequest
			{
				RequestContext = _requestContext.State.Context,
				UserContext = _userContext.State.Context
			});
		}
		_sdkState.Update(new SdkState(OnlineState.Offline, _sdkState.State.Suspended));
	}

	public Task Unregister()
	{
		return SignOutUser();
	}

	public async Task<SignInHydraResponse> SignInHydra(string login)
	{
		ValidateAndInitializeSession();
		SignInHydraRequest request = new SignInHydraRequest
		{
			Data = GetSignInData(),
			Login = login
		};
		SignInHydraResponse response = await _apiAuth.SignInHydraAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds, response.Data.Date, -1L);
		return response;
	}

	public async Task<SignInHydraAuthTicketResponse> SignInHydraAuthTicket(string hydraAuthTicket)
	{
		ValidateAndInitializeSession(skipStateCheck: true);
		SignInHydraAuthTicketRequest request = new SignInHydraAuthTicketRequest
		{
			Data = GetSignInData(),
			HydraAuthTicket = hydraAuthTicket
		};
		SignInHydraAuthTicketResponse response = await _apiAuth.SignInHydraAuthTicketAsync(request);
		await SignInProcess(response.RequestContext, response.Endpoints, null, null, Guid.NewGuid().ToString(), -1, response.Date, -1L);
		return response;
	}

	public async Task<SignInToolResponse> SignInTool()
	{
		ValidateAndInitializeSession(skipStateCheck: true);
		SignInToolRequest request = new SignInToolRequest
		{
			Title = new TitleIdentity
			{
				TitleId = _clientInfo.State.TitleId,
				TitleSecret = _clientInfo.State.SecretKey
			},
			Versions = { ServiceHelper.KnownServices.Select((ServiceInfo s) => s.AsServiceIdentity()) }
		};
		SignInToolResponse response = await _apiAuth.SignInToolAsync(request);
		await SignInProcess(response.RequestContext, response.Endpoints, null, null, expiresAt: response.ExpiresAt, kernelSessionId: Guid.NewGuid().ToString(), tokenRefreshAfterSeconds: -1, timestamp: response.Date);
		_refreshTime = response.ExpiresAt.FromUnixMilliseconds().AddSeconds(-5.0);
		_toolContext.Update(new ToolContextWrapper(response.ToolContext));
		return response;
	}

	public async Task<SignInEpicOnlineServicesResponse> SignInEpicOnlineServices(string authAccessToken)
	{
		ValidateAndInitializeSession();
		SignInEpicOnlineServicesRequest request = new SignInEpicOnlineServicesRequest
		{
			Data = GetSignInData(),
			AuthAccessToken = authAccessToken
		};
		SignInEpicOnlineServicesResponse response = await _apiAuth.SignInEpicOnlineServicesAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds, response.Data.Date, -1L);
		return response;
	}

	public async Task<SignInSteamResponse> SignInSteam(byte[] ticket, int ticketLength)
	{
		if (ticket == null || ticket.Length == 0 || ticketLength == 0 || ticket.Length < ticketLength)
		{
			throw new HydraSdkException(ErrorCode.SdkInvalidParameter, "Invalid ticket array length");
		}
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < ticketLength; i++)
		{
			sb.Append($"{ticket[i]},");
		}
		ValidateAndInitializeSession();
		SignInSteamRequest request = new SignInSteamRequest
		{
			Data = GetSignInData(),
			AuthSessionTicket = "[" + sb.ToString() + "]"
		};
		SignInSteamResponse response = await _apiAuth.SignInSteamAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds, response.Data.Date, -1L);
		return response;
	}

	public async Task<SignInPsnResponse> SignInPsn(string authCode, int issuerId)
	{
		ValidateAndInitializeSession();
		SignInPsnRequest request = new SignInPsnRequest
		{
			Data = GetSignInData(),
			AuthCode = authCode,
			IssuerId = issuerId
		};
		SignInPsnResponse response = await _apiAuth.SignInPsnAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds, response.Data.Date, -1L);
		return response;
	}

	public async Task<SignInPsnTokenResponse> SignInPsnToken(string idToken)
	{
		ValidateAndInitializeSession();
		SignInPsnTokenRequest request = new SignInPsnTokenRequest
		{
			Data = GetSignInData(),
			IdToken = idToken
		};
		SignInPsnTokenResponse response = await _apiAuth.SignInPsnTokenAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds, response.Data.Date, -1L);
		return response;
	}

	public async Task<SignInStadiaResponse> SignInStadia(string authAccessToken)
	{
		ValidateAndInitializeSession();
		SignInStadiaRequest request = new SignInStadiaRequest
		{
			Data = GetSignInData(),
			AuthAccessToken = authAccessToken
		};
		SignInStadiaResponse response = await _apiAuth.SignInStadiaAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds, response.Data.Date, -1L);
		return response;
	}

	public async Task<SignInXboxResponse> SignInXbox(string xstsClientToken)
	{
		ValidateAndInitializeSession();
		SignInXboxRequest request = new SignInXboxRequest
		{
			Data = GetSignInData(),
			XstsClientToken = xstsClientToken
		};
		SignInXboxResponse response = await _apiAuth.SignInXboxAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds, response.Data.Date, -1L);
		return response;
	}

	public async Task<SignInMsStoreResponse> SignInMsStore(string xstsClientToken, string xstoreClientToken)
	{
		ValidateAndInitializeSession();
		SignInMsStoreRequest request = new SignInMsStoreRequest
		{
			Data = GetSignInData(),
			XstsClientToken = xstsClientToken,
			XstoreClientToken = xstoreClientToken
		};
		SignInMsStoreResponse response = await _apiAuth.SignInMsStoreAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds, response.Data.Date, -1L);
		return response;
	}

	public async Task<SignInNintendoResponse> SignInNintendo(string identityToken)
	{
		ValidateAndInitializeSession();
		SignInNintendoRequest request = new SignInNintendoRequest
		{
			Data = GetSignInData(),
			IdentityToken = identityToken
		};
		SignInNintendoResponse response = await _apiAuth.SignInNintendoAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds, response.Data.Date, -1L);
		return response;
	}

	public async Task<SignInOculusResponse> SignInOculus(long oculusUserId, string nonce)
	{
		ValidateAndInitializeSession();
		SignInOculusRequest request = new SignInOculusRequest
		{
			Data = GetSignInData(),
			UserId = oculusUserId,
			Nonce = nonce
		};
		SignInOculusResponse response = await _apiAuth.SignInOculusAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds, response.Data.Date, -1L);
		return response;
	}

	public async Task<SignInStandaloneCodeResponse> SignInStandaloneCode(string standaloneCode)
	{
		ValidateAndInitializeSession(skipStateCheck: true);
		SignInStandaloneCodeResponse response = await _apiAuth.SignInStandaloneCodeAsync(new SignInStandaloneCodeRequest
		{
			Data = GetSignInData(),
			StandaloneCode = standaloneCode
		});
		await SignInProcess(response.RequestContext, response.Endpoints, null, null, Guid.NewGuid().ToString(), -1, response.Date, -1L);
		_standaloneInfo.Update(new StandaloneServerWrapper(response.ServerToken));
		return response;
	}

	public async Task<GetStandaloneSignInCodeResponse> GetStandaloneSignInCode()
	{
		if (_userContext.State?.Context == null)
		{
			throw new HydraSdkException(ErrorCode.SdkInternalError, "UserContext is required, perform authorization first.");
		}
		return await _apiAuth.GetStandaloneSignInCodeAsync(new GetStandaloneSignInCodeRequest
		{
			UserContext = _userContext.State.Context
		});
	}
}
