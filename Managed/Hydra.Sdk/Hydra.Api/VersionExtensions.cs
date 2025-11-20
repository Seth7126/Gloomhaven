using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api;

public static class VersionExtensions
{
	public static readonly Extension<MessageOptions, Build> Build = new Extension<MessageOptions, Build>(50001, FieldCodec.ForMessage(400010u, Hydra.Api.Build.Parser));
}
