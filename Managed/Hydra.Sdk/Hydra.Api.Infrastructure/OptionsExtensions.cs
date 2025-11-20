using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure;

public static class OptionsExtensions
{
	public static readonly Extension<ServiceOptions, ServiceAttribute> Service = new Extension<ServiceOptions, ServiceAttribute>(50000, FieldCodec.ForMessage(400002u, ServiceAttribute.Parser));

	public static readonly Extension<MethodOptions, MethodAttribute> Method = new Extension<MethodOptions, MethodAttribute>(50002, FieldCodec.ForMessage(400018u, MethodAttribute.Parser));

	public static readonly Extension<EnumValueOptions, EnumValueDescription> Desc = new Extension<EnumValueOptions, EnumValueDescription>(1235, FieldCodec.ForMessage(9882u, EnumValueDescription.Parser));
}
