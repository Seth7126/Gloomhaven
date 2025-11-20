namespace System.Security.Cryptography.Pkcs.Asn1;

[Flags]
internal enum PkiFailureInfo
{
	None = 0,
	BadAlg = 1,
	BadMessageCheck = 2,
	BadRequest = 4,
	BadTime = 8,
	BadCertId = 0x10,
	BadDataFormat = 0x20,
	WrongAuthority = 0x40,
	IncorrectData = 0x80,
	MissingTimeStamp = 0x100,
	BadPop = 0x200,
	CertRevoked = 0x400,
	CertConfirmed = 0x800,
	WrongIntegrity = 0x1000,
	BadRecipientNonce = 0x2000,
	TimeNotAvailable = 0x4000,
	UnacceptedPolicy = 0x8000,
	UnacceptedExtension = 0x10000,
	AddInfoNotAvailable = 0x20000,
	BadSenderNonce = 0x40000,
	BadCertTemplate = 0x80000,
	SignerNotTrusted = 0x100000,
	TransactionIdInUse = 0x200000,
	UnsupportedVersion = 0x400000,
	NotAuthorized = 0x800000,
	SystemUnavail = 0x1000000,
	SystemFailure = 0x2000000,
	DuplicateCertReq = 0x4000000
}
