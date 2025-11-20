using System.Text;

namespace Photon.Bolt;

internal struct PropertyStringSettings
{
	public StringEncodings Encoding;

	public Encoding EncodingClass => System.Text.Encoding.UTF8;
}
