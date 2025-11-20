using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace RedLynx.Api;

public static class BuildVersionExtensions
{
	public static readonly Extension<MessageOptions, Build> Build = new Extension<MessageOptions, Build>(50001, FieldCodec.ForMessage(400010u, RedLynx.Api.Build.Parser));
}
