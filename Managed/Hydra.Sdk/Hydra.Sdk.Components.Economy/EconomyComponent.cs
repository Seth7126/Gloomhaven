using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hydra.Api.Push;
using Hydra.Api.User;
using Hydra.Sdk.Communication;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Communication.WebSockets;
using Hydra.Sdk.Components.Push;
using Hydra.Sdk.Enums;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Components.Economy;

public sealed class EconomyComponent : APushListener, IHydraSdkComponent
{
	public delegate void OnTransactionReceivedDelegate(List<UserTransaction> transactions);

	public OnTransactionReceivedDelegate OnTransactionsReceived;

	private EconomyApi.EconomyApiClient _api;

	private StateObserver<GcContextWrapper> _gcContext;

	private StateObserver<SdkState> _sdkState;

	public ConcurrentDictionary<string, UserState> States { get; private set; }

	public async Task Register(IConnectionManager connectionManager, ComponentMessager componentMessager, StateResolver stateResolver, IHydraSdkLogger logger)
	{
		States = new ConcurrentDictionary<string, UserState>();
		await InitializePush(componentMessager, stateResolver, logger);
		_gcContext = stateResolver.CreateLinkedObserver<GcContextWrapper>();
		_sdkState = stateResolver.CreateLinkedObserver<SdkState>();
		_api = connectionManager.GetConnection<EconomyApi.EconomyApiClient>();
		OnTokenUpdate = (OnTokenUpdateDelegate)Delegate.Combine(OnTokenUpdate, new OnTokenUpdateDelegate(PushTokenUpdated));
		OnPushDisconnect = (OnPushDisconnectDelegate)Delegate.Combine(OnPushDisconnect, new OnPushDisconnectDelegate(PushDisconnected));
	}

	public Task Unregister()
	{
		if (base.IsConnected && _sdkState.State.State == OnlineState.Online)
		{
			return DisconnectInternal(() => HandleDisconnect(), MessageReason.Normal);
		}
		return Task.CompletedTask;
	}

	public Task<ConnectResponse> Connect()
	{
		return ConnectInternal((PushToken token) => HandleConnect(token));
	}

	public Task<DisconnectResponse> Disconnect()
	{
		return DisconnectInternal(() => HandleDisconnect(), MessageReason.User);
	}

	public async Task<GetUserStatesResponse> GetUserStates()
	{
		return await _api.GetUserStatesAsync(new GetUserStatesRequest
		{
			Context = _userContext.State.Context
		});
	}

	public async Task<GetTransactionsResponse> GetTransactions(long fromTransactionId, long count)
	{
		return await _api.GetTransactionsAsync(new GetTransactionsRequest
		{
			Context = _userContext.State.Context,
			FromTransactionId = fromTransactionId,
			Count = count
		});
	}

	public async Task<GetTransactionsReverseResponse> GetTransactionsReverse(long beforeTransactionId, long count)
	{
		return await _api.GetTransactionsReverseAsync(new GetTransactionsReverseRequest
		{
			Context = _userContext.State.Context,
			BeforeTransactionId = beforeTransactionId,
			Count = count
		});
	}

	public async Task<ApplyOffersResponse> ApplyOffers(IEnumerable<OfferListItem> offers)
	{
		return await _api.ApplyOffersAsync(new ApplyOffersRequest
		{
			ConfigurationContext = _gcContext.State.Context,
			UserContext = _userContext.State.Context,
			Offers = { offers }
		});
	}

	protected override void PushMessageReceived(WebSocketMessage msg)
	{
		object obj = null;
		PushMessageType type = msg.Type;
		PushMessageType pushMessageType = type;
		if (pushMessageType == PushMessageType.EconomyUserTransactionsUpdate)
		{
			UserTransactionsUpdateVersion userTransactionsUpdateVersion = UserTransactionsUpdateVersion.Parser.ParseFrom(msg.Data, msg.Offset, msg.Lenght);
			obj = userTransactionsUpdateVersion;
			_logger?.Log(HydraLogType.Message, string.Format("{0}/{1}/{2}", "SDK/Hydra", PushServiceStub.Descriptor.FullName, msg.Type), "{0}", userTransactionsUpdateVersion);
			HandleUserTransactions(userTransactionsUpdateVersion);
		}
	}

	private async Task<ConnectResponse> HandleConnect(PushToken token)
	{
		States.Clear();
		ConnectResponse response = await _api.ConnectAsync(new ConnectRequest
		{
			Context = _userContext.State?.Context,
			PushToken = token,
			ConfigurationContext = _gcContext.State?.Context
		});
		foreach (UserState item in response.Data.Items)
		{
			States[item.StateUid] = item;
		}
		return response;
	}

	private async Task<DisconnectResponse> HandleDisconnect()
	{
		return await _api.DisconnectAsync(new DisconnectRequest
		{
			Context = _userContext.State.Context
		});
	}

	private void HandleUserTransactions(UserTransactionsUpdateVersion update)
	{
		try
		{
			if (update.Update?.Transactions == null)
			{
				return;
			}
			foreach (UserTransaction transaction in update.Update.Transactions)
			{
				foreach (UserTransactionItem transactionItem in transaction.TransactionItems)
				{
					if (States.ContainsKey(transactionItem.StateUid))
					{
						States[transactionItem.StateUid].StateValue = transactionItem.CurrentValue;
						continue;
					}
					States.TryAdd(transactionItem.StateUid, new UserState
					{
						StateUid = transactionItem.StateUid,
						StateValue = transactionItem.CurrentValue
					});
				}
			}
			if (OnTransactionsReceived != null)
			{
				OnTransactionsReceived(update.Update.Transactions.ToList());
			}
		}
		catch (Exception err)
		{
			_logger.Log(HydraLogType.Error, this.GetLogCatErr(), "Transactions handle exception: {0}", err.GetErrorMessage());
		}
	}

	private void PushTokenUpdated(PushToken pushToken)
	{
		_logger.Log(HydraLogType.Information, this.GetLogCatInf(), "'Push' token changed, reconnecting...");
		HandleConnect(pushToken).Wait();
	}

	private Task PushDisconnected(MessageReason reason)
	{
		return DisconnectInternal(() => HandleDisconnect(), reason);
	}
}
