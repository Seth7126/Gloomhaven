using System;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Rating;

public static class RatingContractsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static RatingContractsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChxSYXRpbmcvUmF0aW5nQ29udHJhY3RzLnByb3RvEhBIeWRyYS5BcGkuUmF0" + "aW5nGh9nb29nbGUvcHJvdG9idWYvdGltZXN0YW1wLnByb3RvGhlDb250ZXh0" + "L1VzZXJDb250ZXh0LnByb3RvGhtDb250ZXh0L1NlcnZlckNvbnRleHQucHJv" + "dG8iugEKEVVzZXJSYXRpbmdIaXN0b3J5EhMKC2dhbWVzX2NvdW50GAEgASgF" + "Eg4KBnJhdGluZxgCIAEoARISCgpzZXNzaW9uX2lkGAMgASgJEigKBHRpbWUY" + "BCABKAsyGi5nb29nbGUucHJvdG9idWYuVGltZXN0YW1wEhMKC2N1c3RvbV9k" + "YXRhGAUgASgJEi0KBnJlc3VsdBgGIAEoDjIdLkh5ZHJhLkFwaS5SYXRpbmcu" + "TWF0Y2hSZXN1bHQiZwoKUmF0aW5nRGF0YRIOCgZyYXRpbmcYASABKAESEwoL" + "Z2FtZXNfY291bnQYAiABKAUSNAoHaGlzdG9yeRgEIAMoCzIjLkh5ZHJhLkFw" + "aS5SYXRpbmcuVXNlclJhdGluZ0hpc3RvcnkiSQoKVXNlclJhdGluZxIPCgd1" + "c2VyX2lkGAEgASgJEioKBGRhdGEYAiABKAsyHC5IeWRyYS5BcGkuUmF0aW5n" + "LlJhdGluZ0RhdGEiWAoUVXNlckluZGl2aWR1YWxSZXN1bHQSDwoHdXNlcl9p" + "ZBgBIAEoCRIaChJ1c2VyX3BsYWNlX2luX2dhbWUYAiABKAUSEwoLY3VzdG9t" + "X2RhdGEYAyABKAkibgoOVXNlclRlYW1SZXN1bHQSDwoHdXNlcl9pZBgBIAEo" + "CRIaChJ1c2VyX3BsYWNlX2luX3RlYW0YAiABKAUSGgoSdGVhbV9wbGFjZV9p" + "bl9nYW1lGAMgASgFEhMKC2N1c3RvbV9kYXRhGAQgASgJIlIKF1Nlc3Npb25J" + "bmRpdmlkdWFsUmVzdWx0EjcKB3Jlc3VsdHMYASADKAsyJi5IeWRyYS5BcGku" + "UmF0aW5nLlVzZXJJbmRpdmlkdWFsUmVzdWx0IkYKEVNlc3Npb25UZWFtUmVz" + "dWx0EjEKB3Jlc3VsdHMYASADKAsyIC5IeWRyYS5BcGkuUmF0aW5nLlVzZXJU" + "ZWFtUmVzdWx0Ip4BCgxSYXRpbmdVcGRhdGUSRwoSaW5kaXZpZHVhbF9yZXN1" + "bHRzGAIgASgLMikuSHlkcmEuQXBpLlJhdGluZy5TZXNzaW9uSW5kaXZpZHVh" + "bFJlc3VsdEgAEjsKDHRlYW1fcmVzdWx0cxgDIAEoCzIjLkh5ZHJhLkFwaS5S" + "YXRpbmcuU2Vzc2lvblRlYW1SZXN1bHRIAEIICgZyZXN1bHQilQEKFUdldFJh" + "dGluZ3NVc2VyUmVxdWVzdBI+Cgdjb250ZXh0GAEgASgLMi0uSHlkcmEuQXBp" + "LkluZnJhc3RydWN0dXJlLkNvbnRleHQuVXNlckNvbnRleHQSEQoJcmF0aW5n" + "X2lkGAIgASgJEhcKD2luY2x1ZGVfaGlzdG9yeRgDIAEoCBIQCgh1c2VyX2lk" + "cxgEIAMoCSJHChZHZXRSYXRpbmdzVXNlclJlc3BvbnNlEi0KB3JhdGluZ3MY" + "ASADKAsyHC5IeWRyYS5BcGkuUmF0aW5nLlVzZXJSYXRpbmcimQEKF0dldFJh" + "dGluZ3NTZXJ2ZXJSZXF1ZXN0EkAKB2NvbnRleHQYASABKAsyLy5IeWRyYS5B" + "cGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5TZXJ2ZXJDb250ZXh0EhEKCXJh" + "dGluZ19pZBgCIAEoCRIXCg9pbmNsdWRlX2hpc3RvcnkYAyABKAgSEAoIdXNl" + "cl9pZHMYBCADKAkiSQoYR2V0UmF0aW5nc1NlcnZlclJlc3BvbnNlEi0KB3Jh" + "dGluZ3MYASADKAsyHC5IeWRyYS5BcGkuUmF0aW5nLlVzZXJSYXRpbmciUQoY" + "R2V0U2VydmljZVJhdGluZ3NSZXF1ZXN0EhAKCHRpdGxlX2lkGAEgASgJEhEK" + "CXJhdGluZ19pZBgCIAEoCRIQCgh1c2VyX2lkcxgDIAMoCSKhAQoaVXBkYXRl" + "UmF0aW5nc1NlcnZlclJlcXVlc3QSQAoHY29udGV4dBgBIAEoCzIvLkh5ZHJh" + "LkFwaS5JbmZyYXN0cnVjdHVyZS5Db250ZXh0LlNlcnZlckNvbnRleHQSEQoJ" + "cmF0aW5nX2lkGAIgASgJEi4KBnVwZGF0ZRgDIAEoCzIeLkh5ZHJhLkFwaS5S" + "YXRpbmcuUmF0aW5nVXBkYXRlIh0KG1VwZGF0ZVJhdGluZ3NTZXJ2ZXJSZXNw" + "b25zZSKxAQoYVXBkYXRlUmF0aW5nc1VzZXJSZXF1ZXN0Ej4KB2NvbnRleHQY" + "ASABKAsyLS5IeWRyYS5BcGkuSW5mcmFzdHJ1Y3R1cmUuQ29udGV4dC5Vc2Vy" + "Q29udGV4dBISCgpzZXNzaW9uX2lkGAIgASgJEhEKCXJhdGluZ19pZBgDIAEo" + "CRIuCgZ1cGRhdGUYBCABKAsyHi5IeWRyYS5BcGkuUmF0aW5nLlJhdGluZ1Vw" + "ZGF0ZSIbChlVcGRhdGVSYXRpbmdzVXNlclJlc3BvbnNlInkKEkN1c3RvbVJh" + "dGluZ1VwZGF0ZRIPCgd1c2VyX2lkGAEgASgJEg4KBnJhdGluZxgCIAEoARIT" + "CgtjdXN0b21fZGF0YRgDIAEoCRItCgZyZXN1bHQYBCABKA4yHS5IeWRyYS5B" + "cGkuUmF0aW5nLk1hdGNoUmVzdWx0Iq0BCiBVcGRhdGVDdXN0b21SYXRpbmdz" + "U2VydmVyUmVxdWVzdBJACgdjb250ZXh0GAEgASgLMi8uSHlkcmEuQXBpLklu" + "ZnJhc3RydWN0dXJlLkNvbnRleHQuU2VydmVyQ29udGV4dBIRCglyYXRpbmdf" + "aWQYAiABKAkSNAoGdXBkYXRlGAMgAygLMiQuSHlkcmEuQXBpLlJhdGluZy5D" + "dXN0b21SYXRpbmdVcGRhdGUiIwohVXBkYXRlQ3VzdG9tUmF0aW5nc1NlcnZl" + "clJlc3BvbnNlIr0BCh5VcGRhdGVDdXN0b21SYXRpbmdzVXNlclJlcXVlc3QS" + "PgoHY29udGV4dBgBIAEoCzItLkh5ZHJhLkFwaS5JbmZyYXN0cnVjdHVyZS5D" + "b250ZXh0LlVzZXJDb250ZXh0EhIKCnNlc3Npb25faWQYAiABKAkSEQoJcmF0" + "aW5nX2lkGAMgASgJEjQKBnVwZGF0ZRgEIAMoCzIkLkh5ZHJhLkFwaS5SYXRp" + "bmcuQ3VzdG9tUmF0aW5nVXBkYXRlIiEKH1VwZGF0ZUN1c3RvbVJhdGluZ3NV" + "c2VyUmVzcG9uc2UqUQoLTWF0Y2hSZXN1bHQSFQoRTUFUQ0hfUkVTVUxUX05P" + "TkUQABIUChBNQVRDSF9SRVNVTFRfV0lOEAESFQoRTUFUQ0hfUkVTVUxUX0xP" + "U0UQAmIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[3]
		{
			TimestampReflection.Descriptor,
			UserContextReflection.Descriptor,
			ServerContextReflection.Descriptor
		}, new GeneratedClrTypeInfo(new System.Type[1] { typeof(MatchResult) }, null, new GeneratedClrTypeInfo[22]
		{
			new GeneratedClrTypeInfo(typeof(UserRatingHistory), UserRatingHistory.Parser, new string[6] { "GamesCount", "Rating", "SessionId", "Time", "CustomData", "Result" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(RatingData), RatingData.Parser, new string[3] { "Rating", "GamesCount", "History" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UserRating), UserRating.Parser, new string[2] { "UserId", "Data" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UserIndividualResult), UserIndividualResult.Parser, new string[3] { "UserId", "UserPlaceInGame", "CustomData" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UserTeamResult), UserTeamResult.Parser, new string[4] { "UserId", "UserPlaceInTeam", "TeamPlaceInGame", "CustomData" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SessionIndividualResult), SessionIndividualResult.Parser, new string[1] { "Results" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(SessionTeamResult), SessionTeamResult.Parser, new string[1] { "Results" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(RatingUpdate), RatingUpdate.Parser, new string[2] { "IndividualResults", "TeamResults" }, new string[1] { "Result" }, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetRatingsUserRequest), GetRatingsUserRequest.Parser, new string[4] { "Context", "RatingId", "IncludeHistory", "UserIds" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetRatingsUserResponse), GetRatingsUserResponse.Parser, new string[1] { "Ratings" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetRatingsServerRequest), GetRatingsServerRequest.Parser, new string[4] { "Context", "RatingId", "IncludeHistory", "UserIds" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetRatingsServerResponse), GetRatingsServerResponse.Parser, new string[1] { "Ratings" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(GetServiceRatingsRequest), GetServiceRatingsRequest.Parser, new string[3] { "TitleId", "RatingId", "UserIds" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UpdateRatingsServerRequest), UpdateRatingsServerRequest.Parser, new string[3] { "Context", "RatingId", "Update" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UpdateRatingsServerResponse), UpdateRatingsServerResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UpdateRatingsUserRequest), UpdateRatingsUserRequest.Parser, new string[4] { "Context", "SessionId", "RatingId", "Update" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UpdateRatingsUserResponse), UpdateRatingsUserResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(CustomRatingUpdate), CustomRatingUpdate.Parser, new string[4] { "UserId", "Rating", "CustomData", "Result" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UpdateCustomRatingsServerRequest), UpdateCustomRatingsServerRequest.Parser, new string[3] { "Context", "RatingId", "Update" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UpdateCustomRatingsServerResponse), UpdateCustomRatingsServerResponse.Parser, null, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UpdateCustomRatingsUserRequest), UpdateCustomRatingsUserRequest.Parser, new string[4] { "Context", "SessionId", "RatingId", "Update" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(UpdateCustomRatingsUserResponse), UpdateCustomRatingsUserResponse.Parser, null, null, null, null, null)
		}));
	}
}
