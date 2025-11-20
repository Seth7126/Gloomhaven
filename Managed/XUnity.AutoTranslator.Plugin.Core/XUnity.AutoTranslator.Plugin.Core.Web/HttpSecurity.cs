using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace XUnity.AutoTranslator.Plugin.Core.Web;

internal class HttpSecurity
{
	public readonly HashSet<string> _hosts = new HashSet<string>();

	public void EnableSslFor(params string[] hosts)
	{
		foreach (string item in hosts)
		{
			_hosts.Add(item);
		}
	}

	internal RemoteCertificateValidationCallback GetCertificateValidationCheck()
	{
		if (_hosts.Count == 0)
		{
			return null;
		}
		return (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => sender is HttpWebRequest httpWebRequest && _hosts.Contains(httpWebRequest.Address.Host);
	}
}
