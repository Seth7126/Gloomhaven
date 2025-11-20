using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure.Errors;

public static class ErrorCodeDescExtensions
{
	public static readonly Extension<EnumValueOptions, ErrorCodeDesc> Desc = new Extension<EnumValueOptions, ErrorCodeDesc>(1234, FieldCodec.ForMessage(9874u, ErrorCodeDesc.Parser));
}
