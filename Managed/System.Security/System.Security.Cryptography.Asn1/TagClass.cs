namespace System.Security.Cryptography.Asn1;

internal enum TagClass : byte
{
	Universal = 0,
	Application = 64,
	ContextSpecific = 128,
	Private = 192
}
