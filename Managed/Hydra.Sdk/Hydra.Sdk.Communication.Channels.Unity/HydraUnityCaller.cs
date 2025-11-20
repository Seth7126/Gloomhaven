using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Errors;
using Hydra.Sdk.Components.Facts.Core;
using Hydra.Sdk.Errors;
using Hydra.Sdk.Generated;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;
using UnityEngine.Networking;

namespace Hydra.Sdk.Communication.Channels.Unity;

internal class HydraUnityCaller : ICaller, IDisposable
{
	private string _token;

	private Uri _uri;

	private IHydraSdkLogger _logger;

	public HydraUnityCaller(Uri uri, IHydraSdkLogger logger, string token)
	{
		_uri = uri;
		_logger = logger;
		_token = token;
		SetToken(_token);
	}

	public void SetToken(string token)
	{
		_token = token;
	}

	public async Task<TResponse> Execute<TResponse, TRequest>(IDescriptor descriptor, string method, TRequest request) where TResponse : IMessage<TResponse> where TRequest : IMessage<TRequest>
	{
		TResponse result = default(TResponse);
		bool writeLogs = descriptor.FullName != FactConstants.ServiceName;
		string category = "SDK/Hydra/" + descriptor.FullName + "/" + method + "/";
		if (writeLogs)
		{
			_logger.Log(HydraLogType.Message, category + "Request", "{0}", request);
		}
		byte[] bytes = request.ToByteArray();
		byte[] contentBytes = new byte[bytes.Length + 5];
		byte[] header = BitConverter.GetBytes(SwapEndianness(bytes.Length));
		Buffer.BlockCopy(header, 0, contentBytes, 1, header.Length);
		Buffer.BlockCopy(bytes, 0, contentBytes, header.Length + 1, bytes.Length);
		using (UnityWebRequest web = new UnityWebRequest())
		{
			web.uri = new Uri($"{_uri}{descriptor.FullName}/{method}");
			web.uploadHandler = new UploadHandlerRaw(contentBytes)
			{
				contentType = "application/grpc"
			};
			web.downloadHandler = new DownloadHandlerBuffer();
			web.method = "POST";
			if (!string.IsNullOrWhiteSpace(_token))
			{
				web.SetRequestHeader("Authorization", "Bearer " + _token);
			}
			UnityWebRequestAsyncOperation operation = web.SendWebRequest();
			while (!operation.isDone)
			{
				await Task.Yield();
			}
			ErrorCode status = ErrorCode.Success;
			string message = null;
			string correlationId = null;
			Dictionary<string, string> headers = web.GetResponseHeaders();
			if (headers != null)
			{
				if (headers.TryGetValue("grpc-status", out var statusHeader))
				{
					status = (ErrorCode)int.Parse(statusHeader);
				}
				headers.TryGetValue("x-error-correlation-id", out correlationId);
				headers.TryGetValue("grpc-message", out message);
			}
			try
			{
				if (web.isNetworkError)
				{
					_logger.Log(HydraLogType.Error, category + "Error", "HttpError: {0}, Message: {1}", web.responseCode, web.error);
					throw new HydraSdkException(ErrorCode.SdkInternalError, null, web.error);
				}
				if (status != ErrorCode.Success)
				{
					_logger.Log(HydraLogType.Error, category + "Error", "ErrorCode: {0}, CorrelationId: {1}", status, correlationId);
					throw new HydraSdkException(status, correlationId, message);
				}
				byte[] data = web.downloadHandler.data;
				if (data.Length == 0)
				{
					throw new HydraSdkException(ErrorCode.SdkParseError, correlationId, "Response data is empty.");
				}
				byte[] responseBytes = new byte[data.Length - 5];
				Buffer.BlockCopy(data, 5, responseBytes, 0, responseBytes.Length);
				object parser = typeof(TResponse).GetProperty("Parser").GetValue(null, null);
				MethodInfo parseMethod = parser.GetType().GetMethod("ParseFrom", new Type[1] { typeof(byte[]) });
				result = (TResponse)parseMethod.Invoke(parser, new object[1] { responseBytes });
			}
			finally
			{
				if (writeLogs)
				{
					_logger.Log((status != ErrorCode.Success) ? HydraLogType.Error : HydraLogType.Message, category + "Response", "{0}", result);
				}
			}
		}
		return result;
	}

	private int SwapEndianness(int value)
	{
		int num = value & 0xFF;
		int num2 = (value >> 8) & 0xFF;
		int num3 = (value >> 16) & 0xFF;
		int num4 = (value >> 24) & 0xFF;
		return (num << 24) | (num2 << 16) | (num3 << 8) | num4;
	}

	public void Dispose()
	{
	}
}
