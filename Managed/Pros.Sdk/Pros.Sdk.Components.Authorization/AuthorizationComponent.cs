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
using Pros.Sdk.Extensions;
using Pros.Sdk.Helpers;
using RedLynx.Api.Auth;
using RedLynx.Api.EndpointDispatcher;
using UnityEngine;

namespace Pros.Sdk.Components.Authorization;

public sealed class AuthorizationComponent : IHydraSdkComponent
{
	private IConnectionManager _connectionManager;

	private IHydraSdkLogger _logger;

	private StateObserver<SdkState> _sdkState;

	private RedLynx.Api.EndpointDispatcher.EndpointDispatcherApi.EndpointDispatcherApiClient _apiEds;

	private RedLynx.Api.Auth.AuthorizationApi.AuthorizationApiClient _apiAuth;

	private StateObserver<RequestContextWrapper> _requestContext;

	private StateObserver<UserContextWrapper> _userContext;

	private StateObserver<ToolContextWrapper> _toolContext;

	private StateObserver<ClientInfo> _clientInfo;

	private StateObserver<SdkSessionInfo> _sessionInfo;

	private SdkSessionInfo _tempSession;

	private DateTime _refreshTime;

	private bool _isRefreshing;

	public Platform Platform = Platform.Net;

	public string UserIdentity => _userContext.State?.Context?.Data?.UserIdentity;

	public string KernelSessionId { get; private set; }

	public bool IsRefreshRequired
	{
		get
		{
			if (_sdkState?.State != null && _sdkState.State.State == OnlineState.Online && !_sdkState.State.Suspended)
			{
				return DateTime.UtcNow > _refreshTime;
			}
			return false;
		}
	}

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
		Uri uri = new Uri(_clientInfo.State.HydraEndpoint);
		ServiceInfo serviceInfo = Pros.Sdk.Helpers.ServiceHelper.KnownServices.FirstOrDefault((ServiceInfo f) => f.Name == RedLynx.Api.EndpointDispatcher.EndpointDispatcherApi.Descriptor.FullName);
		_connectionManager.AddConnections(uri.ToEndpointInfo(RedLynx.Api.EndpointDispatcher.EndpointDispatcherApi.Descriptor.FullName, serviceInfo.Version));
		IHydraSdkChannel channel = _connectionManager.GetChannel(RedLynx.Api.EndpointDispatcher.EndpointDispatcherApi.Descriptor.FullName);
		_apiEds = new RedLynx.Api.EndpointDispatcher.EndpointDispatcherApi.EndpointDispatcherApiClient(channel.GetInvoker());
		return Task.CompletedTask;
	}

	public async Task<GetEnvironmentInfoResponse> GetEnvironmentInfo()
	{
		ServiceInfo serviceInfo = Pros.Sdk.Helpers.ServiceHelper.Get(RedLynx.Api.Auth.AuthorizationApi.Descriptor.FullName);
		GetEnvironmentInfoResponse getEnvironmentInfoResponse = await _apiEds.GetEnvironmentInfoAsync(new GetEnvironmentInfoRequest
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
		_tempSession.EnvironmentId = getEnvironmentInfoResponse.Environment.Id;
		_tempSession.TitleId = _clientInfo.State.TitleId;
		_connectionManager.AddConnections(getEnvironmentInfoResponse.Environment.Endpoint.ToHydraEndpointInfo(RedLynx.Api.Auth.AuthorizationApi.Descriptor.FullName));
		return getEnvironmentInfoResponse;
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
			BuildPlatform = Platform,
			RuntimePlatform = new PlatformDetails
			{
				PlatformInternalId = 0,
				PlatformDescription = (_clientInfo.State.Platform ?? string.Empty)
			},
			BuildVersion = _clientInfo.State.ClientVersion,
			Versions = { Pros.Sdk.Helpers.ServiceHelper.KnownServices.Select((ServiceInfo s) => s.AsServiceIdentity()) }
		};
		int platform = (int)Application.platform;
		signInData.BuildPlatform = ResolveUnityPlatform(platform);
		signInData.RuntimePlatform.PlatformInternalId = platform;
		if (string.IsNullOrEmpty(signInData.RuntimePlatform.PlatformDescription))
		{
			signInData.RuntimePlatform.PlatformDescription = Application.platform.ToString();
		}
		return signInData;
		static Platform ResolveUnityPlatform(int platformId)
		{
			return new Dictionary<Platform, int[]>
			{
				{
					Platform.Win64,
					new int[4] { 2, 5, 7, 44 }
				},
				{
					Platform.Android,
					new int[1] { 11 }
				},
				{
					Platform.Durango,
					new int[2] { 27, 37 }
				},
				{
					Platform.Ios,
					new int[1] { 8 }
				},
				{
					Platform.Linux64,
					new int[3] { 13, 16, 43 }
				},
				{
					Platform.Orbis,
					new int[1] { 25 }
				},
				{
					Platform.Prospero,
					new int[1] { 38 }
				},
				{
					Platform.Scarlett,
					new int[1] { 36 }
				},
				{
					Platform.Stadia,
					new int[1] { 34 }
				},
				{
					Platform.Switch,
					new int[1] { 32 }
				}
			}.FirstOrDefault((KeyValuePair<Platform, int[]> f) => f.Value.Any((int a) => a == platformId)).Key;
		}
	}

	private void CheckAuth()
	{
		if (_sdkState.State.State == OnlineState.Online)
		{
			throw new HydraSdkException(ErrorCode.SdkInternalError, "Client is already signed in");
		}
		if (_apiAuth == null)
		{
			IHydraSdkChannel channel = _connectionManager.GetChannel(RedLynx.Api.Auth.AuthorizationApi.Descriptor.FullName);
			_apiAuth = new RedLynx.Api.Auth.AuthorizationApi.AuthorizationApiClient(channel.GetInvoker());
		}
		if (_tempSession == null)
		{
			_tempSession = new SdkSessionInfo(_sessionInfo.State);
		}
	}

	private Task SignInProcess(HydraRequestContext requestContext, IEnumerable<EndpointInfo> endpoints, UserContext userContext = null, ToolContext toolContext = null, string kernelSessionId = null, int tokenRefreshAfterSeconds = -1)
	{
		_connectionManager.UpdateToken(requestContext.Token);
		foreach (EndpointInfo endpoint in endpoints)
		{
			_connectionManager.AddConnections(endpoint.ToHydraEndpointInfo(endpoint.Name));
		}
		if (tokenRefreshAfterSeconds != -1)
		{
			_refreshTime = DateTime.UtcNow.AddSeconds(tokenRefreshAfterSeconds);
		}
		KernelSessionId = kernelSessionId;
		_tempSession.SessionId = KernelSessionId;
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
					RefreshUserResponse refreshUserResponse = await _apiAuth.RefreshUserAsync(new RefreshUserRequest
					{
						RequestContext = _requestContext.State.Context,
						UserContext = _userContext.State.Context
					});
					_connectionManager.UpdateToken(refreshUserResponse.RequestContext.Token);
					_requestContext.Update(new RequestContextWrapper(refreshUserResponse.RequestContext));
					_refreshTime = DateTime.UtcNow.AddSeconds(refreshUserResponse.RefreshAfterSeconds);
					return true;
				}
				if (_toolContext.State?.Context != null)
				{
					return true;
				}
			}
			catch (Exception err)
			{
				_logger.Log(HydraLogType.Error, this.GetLogCatErr(), "Failed to refresh session, reconnect is required. Error: {0}", err.GetErrorMessage());
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
		if (_sdkState.State.State == OnlineState.Online && _userContext.State != null)
		{
			return SignOutUser();
		}
		return Task.CompletedTask;
	}

	public async Task<SignInHydraResponse> SignInHydra(string login)
	{
		CheckAuth();
		SignInHydraRequest request = new SignInHydraRequest
		{
			Data = GetSignInData(),
			Login = login
		};
		SignInHydraResponse response = await _apiAuth.SignInHydraAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds);
		return response;
	}

	public async Task<SignInEpicOnlineServicesResponse> SignInEpicOnlineServices(string authAccessToken)
	{
		CheckAuth();
		SignInEpicOnlineServicesRequest request = new SignInEpicOnlineServicesRequest
		{
			Data = GetSignInData(),
			AuthAccessToken = authAccessToken
		};
		SignInEpicOnlineServicesResponse response = await _apiAuth.SignInEpicOnlineServicesAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds);
		return response;
	}

	public async Task<SignInSteamResponse> SignInSteam(byte[] ticket, int ticketLength)
	{
		if (ticket == null || ticket.Length == 0 || ticketLength == 0 || ticket.Length < ticketLength)
		{
			throw new HydraSdkException(ErrorCode.SdkInvalidParameter, "Invalid ticket array length");
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < ticketLength; i++)
		{
			stringBuilder.Append($"{ticket[i]},");
		}
		CheckAuth();
		SignInSteamRequest request = new SignInSteamRequest
		{
			Data = GetSignInData(),
			AuthSessionTicket = "[" + stringBuilder.ToString() + "]"
		};
		SignInSteamResponse response = await _apiAuth.SignInSteamAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds);
		return response;
	}

	public async Task<SignInPsnResponse> SignInPsn(string authCode, int issuerId)
	{
		CheckAuth();
		SignInPsnRequest request = new SignInPsnRequest
		{
			Data = GetSignInData(),
			AuthCode = authCode,
			IssuerId = issuerId
		};
		SignInPsnResponse response = await _apiAuth.SignInPsnAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds);
		return response;
	}

	public async Task<SignInPsnTokenResponse> SignInPsnToken(string idToken)
	{
		CheckAuth();
		SignInPsnTokenRequest request = new SignInPsnTokenRequest
		{
			Data = GetSignInData(),
			IdToken = idToken
		};
		SignInPsnTokenResponse response = await _apiAuth.SignInPsnTokenAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds);
		return response;
	}

	public async Task<SignInStadiaResponse> SignInStadia(string authAccessToken)
	{
		CheckAuth();
		SignInStadiaRequest request = new SignInStadiaRequest
		{
			Data = GetSignInData(),
			AuthAccessToken = authAccessToken
		};
		SignInStadiaResponse response = await _apiAuth.SignInStadiaAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds);
		return response;
	}

	public async Task<SignInXboxResponse> SignInXbox(string xstsClientToken)
	{
		CheckAuth();
		SignInXboxRequest request = new SignInXboxRequest
		{
			Data = GetSignInData(),
			XstsClientToken = xstsClientToken
		};
		SignInXboxResponse response = await _apiAuth.SignInXboxAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds);
		return response;
	}

	public async Task<SignInNintendoResponse> SignInNintendo(string identityToken)
	{
		CheckAuth();
		SignInNintendoRequest request = new SignInNintendoRequest
		{
			Data = GetSignInData(),
			IdentityToken = identityToken
		};
		SignInNintendoResponse response = await _apiAuth.SignInNintendoAsync(request);
		await SignInProcess(response.Data.RequestContext, response.Data.Endpoints, response.Data.UserContext, null, response.Data.UserContext.Data.KernelSessionId, response.Data.RefreshAfterSeconds);
		return response;
	}
}
