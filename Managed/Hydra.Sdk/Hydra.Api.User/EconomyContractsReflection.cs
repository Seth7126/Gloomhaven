using System;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Hydra.Api.Infrastructure.Context;
using Hydra.Api.Push;

namespace Hydra.Api.User;

public static class EconomyContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EconomyContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChtVc2VyL0Vjb25vbXlDb250cmFjdHMucHJvdG8SDkh5ZHJhLkFwaS5Vc2Vy" + "GhlDb250ZXh0L1VzZXJDb250ZXh0LnByb3RvGiJDb250ZXh0L0NvbmZpZ3Vy" + "YXRpb25Db250ZXh0LnByb3RvGhRQdXNoL1B1c2hUb2tlbi5wcm90bxoQVXNl" + "ci9FbnVtcy5wcm90bxofZ29vZ2xlL3Byb3RvYnVmL3RpbWVzdGFtcC5wcm90" + "byLWAQoOQ29ubmVjdFJlcXVlc3QSPgoHY29udGV4dBgBIAEoCzItLkh5ZHJh" + "LkFwaS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlVzZXJDb250ZXh0Ei0KCnB1" + "c2hfdG9rZW4YAiABKAsyGS5IeWRyYS5BcGkuUHVzaC5QdXNoVG9rZW4SVQoV" + "Y29uZmlndXJhdGlvbl9jb250ZXh0GAMgASgLMjYuSHlkcmEuQXBpLkluZnJh" + "c3RydWN0dXJlLkNvbnRleHQuQ29uZmlndXJhdGlvbkNvbnRleHQiOwoPQ29u" + "bmVjdFJlc3BvbnNlEigKBGRhdGEYASABKAsyGi5IeWRyYS5BcGkuVXNlci5V" + "c2VyU3RhdGVzIlMKEURpc2Nvbm5lY3RSZXF1ZXN0Ej4KB2NvbnRleHQYASAB" + "KAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2VyQ29u" + "dGV4dCIUChJEaXNjb25uZWN0UmVzcG9uc2UiVgoUR2V0VXNlclN0YXRlc1Jl" + "cXVlc3QSPgoHY29udGV4dBgBIAEoCzItLkh5ZHJhLkFwaS5JbmZyYXN0cnVj" + "dHVyZS5Db250ZXh0LlVzZXJDb250ZXh0IkEKFUdldFVzZXJTdGF0ZXNSZXNw" + "b25zZRIoCgRkYXRhGAEgASgLMhouSHlkcmEuQXBpLlVzZXIuVXNlclN0YXRl" + "cyKEAQoWR2V0VHJhbnNhY3Rpb25zUmVxdWVzdBI+Cgdjb250ZXh0GAEgASgL" + "Mi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNlckNvbnRl" + "eHQSGwoTZnJvbV90cmFuc2FjdGlvbl9pZBgCIAEoAxINCgVjb3VudBgDIAEo" + "AyJXChdHZXRUcmFuc2FjdGlvbnNSZXNwb25zZRI8Cgx0cmFuc2FjdGlvbnMY" + "ASABKAsyJi5IeWRyYS5BcGkuVXNlci5Vc2VyVHJhbnNhY3Rpb25zVXBkYXRl" + "Io0BCh1HZXRUcmFuc2FjdGlvbnNSZXZlcnNlUmVxdWVzdBI+Cgdjb250ZXh0" + "GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNl" + "ckNvbnRleHQSHQoVYmVmb3JlX3RyYW5zYWN0aW9uX2lkGAIgASgDEg0KBWNv" + "dW50GAMgASgDIl4KHkdldFRyYW5zYWN0aW9uc1JldmVyc2VSZXNwb25zZRI8" + "Cgx0cmFuc2FjdGlvbnMYASABKAsyJi5IeWRyYS5BcGkuVXNlci5Vc2VyVHJh" + "bnNhY3Rpb25zVXBkYXRlIt8BChJBcHBseU9mZmVyc1JlcXVlc3QSQwoMdXNl" + "cl9jb250ZXh0GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNv" + "bnRleHQuVXNlckNvbnRleHQSVQoVY29uZmlndXJhdGlvbl9jb250ZXh0GAIg" + "ASgLMjYuSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuQ29uZmln" + "dXJhdGlvbkNvbnRleHQSLQoGb2ZmZXJzGAMgAygLMh0uSHlkcmEuQXBpLlVz" + "ZXIuT2ZmZXJMaXN0SXRlbSIVChNBcHBseU9mZmVyc1Jlc3BvbnNlIjcKDU9m" + "ZmVyTGlzdEl0ZW0SFAoMcmVmZXJlbmNlX2lkGAEgASgJEhAKCG9mZmVyX2lk" + "GAIgASgJIlMKClVzZXJTdGF0ZXMSGwoTbGFzdF90cmFuc2FjdGlvbl9pZBgB" + "IAEoAxIoCgVpdGVtcxgCIAMoCzIZLkh5ZHJhLkFwaS5Vc2VyLlVzZXJTdGF0" + "ZSJPCglVc2VyU3RhdGUSEQoJc3RhdGVfdWlkGAEgASgJEi8KC3N0YXRlX3Zh" + "bHVlGAIgASgLMhouSHlkcmEuQXBpLlVzZXIuU3RhdGVWYWx1ZSJPChZVc2Vy" + "VHJhbnNhY3Rpb25zVXBkYXRlEjUKDHRyYW5zYWN0aW9ucxgBIAMoCzIfLkh5" + "ZHJhLkFwaS5Vc2VyLlVzZXJUcmFuc2FjdGlvbiK0AgoPVXNlclRyYW5zYWN0" + "aW9uEgoKAmlkGAEgASgDEhAKCG9mZmVyX2lkGAIgASgJEhQKDHJlZmVyZW5j" + "ZV9pZBgDIAEoCRI+ChF0cmFuc2FjdGlvbl9pdGVtcxgEIAMoCzIjLkh5ZHJh" + "LkFwaS5Vc2VyLlVzZXJUcmFuc2FjdGlvbkl0ZW0SSAoNZXh0ZW5kZWRfaW5m" + "bxgFIAMoCzIxLkh5ZHJhLkFwaS5Vc2VyLlVzZXJUcmFuc2FjdGlvbi5FeHRl" + "bmRlZEluZm9FbnRyeRIuCgpjcmVhdGVkX2F0GAYgASgLMhouZ29vZ2xlLnBy" + "b3RvYnVmLlRpbWVzdGFtcBozChFFeHRlbmRlZEluZm9FbnRyeRILCgNrZXkY" + "ASABKAkSDQoFdmFsdWUYAiABKAk6AjgBIsYCChNVc2VyVHJhbnNhY3Rpb25J" + "dGVtEhEKCXN0YXRlX3VpZBgBIAEoCRI2Cg1zdGF0ZV9vcF90eXBlGAIgASgO" + "Mh8uSHlkcmEuQXBpLlVzZXIuVXNlclN0YXRlT3BUeXBlEi4KCnByZXZfdmFs" + "dWUYAyABKAsyGi5IeWRyYS5BcGkuVXNlci5TdGF0ZVZhbHVlEjEKDWN1cnJl" + "bnRfdmFsdWUYBCABKAsyGi5IeWRyYS5BcGkuVXNlci5TdGF0ZVZhbHVlEkwK" + "DWV4dGVuZGVkX2luZm8YBSADKAsyNS5IeWRyYS5BcGkuVXNlci5Vc2VyVHJh" + "bnNhY3Rpb25JdGVtLkV4dGVuZGVkSW5mb0VudHJ5GjMKEUV4dGVuZGVkSW5m" + "b0VudHJ5EgsKA2tleRgBIAEoCRINCgV2YWx1ZRgCIAEoCToCOAEitQIKClN0" + "YXRlVmFsdWUSOAoOc3RhdGVfb3duX3R5cGUYASABKA4yIC5IeWRyYS5BcGku" + "VXNlci5Vc2VyU3RhdGVPd25UeXBlEjoKD3N0YXRlX2RhdGFfdHlwZRgCIAEo" + "DjIhLkh5ZHJhLkFwaS5Vc2VyLlVzZXJTdGF0ZURhdGFUeXBlEjEKC2ludDY0" + "X3ZhbHVlGAMgASgLMhouSHlkcmEuQXBpLlVzZXIuSW50NjRWYWx1ZUgAEjMK" + "DHN0cmluZ192YWx1ZRgEIAEoCzIbLkh5ZHJhLkFwaS5Vc2VyLlN0cmluZ1Zh" + "bHVlSAASQAoTdmVjdG9yX3N0cmluZ192YWx1ZRgFIAEoCzIhLkh5ZHJhLkFw" + "aS5Vc2VyLlZlY3RvclN0cmluZ1ZhbHVlSABCBwoFdmFsdWUiwAEKIlByb2Nl" + "c3NQbGF0Zm9ybUVudGl0bGVtZW50c1JlcXVlc3QSQwoMdXNlcl9jb250ZXh0" + "GAEgASgLMi0uSHlkcmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNl" + "ckNvbnRleHQSVQoVY29uZmlndXJhdGlvbl9jb250ZXh0GAIgASgLMjYuSHlk" + "cmEuQXBpLkluZnJhc3RydWN0dXJlLkNvbnRleHQuQ29uZmlndXJhdGlvbkNv" + "bnRleHQiJQojUHJvY2Vzc1BsYXRmb3JtRW50aXRsZW1lbnRzUmVzcG9uc2Ui" + "GwoKSW50NjRWYWx1ZRINCgV2YWx1ZRgBIAEoAyIcCgtTdHJpbmdWYWx1ZRIN" + "CgV2YWx1ZRgBIAEoCSIiChFWZWN0b3JTdHJpbmdWYWx1ZRINCgV2YWx1ZRgB" + "IAMoCSJoCh1Vc2VyVHJhbnNhY3Rpb25zVXBkYXRlVmVyc2lvbhIPCgd2ZXJz" + "aW9uGAEgASgFEjYKBnVwZGF0ZRgCIAEoCzImLkh5ZHJhLkFwaS5Vc2VyLlVz" + "ZXJUcmFuc2FjdGlvbnNVcGRhdGViBnByb3RvMw==");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[5]
		{
			UserContextReflection.Descriptor,
			ConfigurationContextReflection.Descriptor,
			PushTokenReflection.Descriptor,
			EnumsReflection.Descriptor,
			TimestampReflection.Descriptor
		}, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[25]
		{
			new GeneratedClrTypeInfo(typeof(ConnectRequest), ConnectRequest.Parser, new string[3] { "Context", "PushToken", "ConfigurationContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ConnectResponse), ConnectResponse.Parser, new string[1] { "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DisconnectRequest), DisconnectRequest.Parser, new string[1] { "Context" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(DisconnectResponse), DisconnectResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetUserStatesRequest), GetUserStatesRequest.Parser, new string[1] { "Context" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetUserStatesResponse), GetUserStatesResponse.Parser, new string[1] { "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetTransactionsRequest), GetTransactionsRequest.Parser, new string[3] { "Context", "FromTransactionId", "Count" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetTransactionsResponse), GetTransactionsResponse.Parser, new string[1] { "Transactions" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetTransactionsReverseRequest), GetTransactionsReverseRequest.Parser, new string[3] { "Context", "BeforeTransactionId", "Count" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetTransactionsReverseResponse), GetTransactionsReverseResponse.Parser, new string[1] { "Transactions" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ApplyOffersRequest), ApplyOffersRequest.Parser, new string[3] { "UserContext", "ConfigurationContext", "Offers" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ApplyOffersResponse), ApplyOffersResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(OfferListItem), OfferListItem.Parser, new string[2] { "ReferenceId", "OfferId" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UserStates), UserStates.Parser, new string[2] { "LastTransactionId", "Items" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UserState), UserState.Parser, new string[2] { "StateUid", "StateValue" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UserTransactionsUpdate), UserTransactionsUpdate.Parser, new string[1] { "Transactions" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UserTransaction), UserTransaction.Parser, new string[6] { "Id", "OfferId", "ReferenceId", "TransactionItems", "ExtendedInfo", "CreatedAt" }, null, null, null, new GeneratedClrTypeInfo[1]),
			new GeneratedClrTypeInfo(typeof(UserTransactionItem), UserTransactionItem.Parser, new string[5] { "StateUid", "StateOpType", "PrevValue", "CurrentValue", "ExtendedInfo" }, null, null, null, new GeneratedClrTypeInfo[1]),
			new GeneratedClrTypeInfo(typeof(StateValue), StateValue.Parser, new string[5] { "StateOwnType", "StateDataType", "Int64Value", "StringValue", "VectorStringValue" }, new string[1] { "Value" }, null, null, null),
			new GeneratedClrTypeInfo(typeof(ProcessPlatformEntitlementsRequest), ProcessPlatformEntitlementsRequest.Parser, new string[2] { "UserContext", "ConfigurationContext" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(ProcessPlatformEntitlementsResponse), ProcessPlatformEntitlementsResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(Int64Value), Int64Value.Parser, new string[1] { "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(StringValue), StringValue.Parser, new string[1] { "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(VectorStringValue), VectorStringValue.Parser, new string[1] { "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UserTransactionsUpdateVersion), UserTransactionsUpdateVersion.Parser, new string[2] { "Version", "Update" }, null, null, null, null)
		}));
	}
}
