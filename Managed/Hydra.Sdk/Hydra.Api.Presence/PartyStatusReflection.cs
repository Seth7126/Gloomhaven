using System;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Presence;

public static class PartyStatusReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PartyStatusReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChpQcmVzZW5jZS9QYXJ0eVN0YXR1cy5wcm90bxISSHlkcmEuQXBpLlByZXNl" + "bmNlGh5QcmVzZW5jZS9NYXRjaG1ha2VTdGF0dXMucHJvdG8iTgoHUGFydHlJ" + "ZBIKCgJpZBgCIAEoCRI3CgZyZWFzb24YAyABKA4yJy5IeWRyYS5BcGkuUHJl" + "c2VuY2UuUGFydHlJZENoYW5nZVJlYXNvbiKbAgoNUGFydHlTZXR0aW5ncxIX" + "Cg9wYXJ0eV9tYXhfY291bnQYASABKAUSRAoRaW52aXRlX2RlbGVnYXRpb24Y" + "AiABKA4yKS5IeWRyYS5BcGkuUHJlc2VuY2UuUGFydHlJbnZpdGVEZWxlZ2F0" + "aW9uEkAKD2pvaW5fZGVsZWdhdGlvbhgDIAEoDjInLkh5ZHJhLkFwaS5QcmVz" + "ZW5jZS5QYXJ0eUpvaW5EZWxlZ2F0aW9uEhgKEGFsbG93ZWRfdXNlcl9pZHMY" + "BCADKAkSLwoIam9pbmFibGUYBSABKA4yHS5IeWRyYS5BcGkuUHJlc2VuY2Uu" + "UGFydHlKb2luEh4KFmRpc2JhbmRfb25fb3duZXJfbGVhdmUYBiABKAgimgEK" + "C1BhcnR5TWVtYmVyEg8KB3VzZXJfaWQYASABKAkSEAoIaXNfb3duZXIYAiAB" + "KAgSDAoEZGF0YRgDIAEoCRITCgtzdGF0aWNfZGF0YRgEIAEoCRISCgpzb3J0" + "X2luZGV4GAUgASgFEjEKBXN0YXRlGAYgASgOMiIuSHlkcmEuQXBpLlByZXNl" + "bmNlLk1hdGNobWFrZVN0YXRlImsKEVBhcnR5Q3JlYXRlUGFyYW1zEjMKCHNl" + "dHRpbmdzGAEgASgLMiEuSHlkcmEuQXBpLlByZXNlbmNlLlBhcnR5U2V0dGlu" + "Z3MSDAoEZGF0YRgCIAEoCRITCgttZW1iZXJfZGF0YRgDIAEoCSo7CgpQYXJ0" + "eVN0YXRlEhQKEFBBUlRZX1NUQVRFX05PTkUQABIXChNQQVJUWV9TVEFURV9D" + "UkVBVEVEEAEq4QEKE1BhcnR5SWRDaGFuZ2VSZWFzb24SHwobUEFSVFlfSURf" + "Q0hBTkdFX1JFQVNPTl9OT05FEAASIQodUEFSVFlfSURfQ0hBTkdFX1JFQVNP" + "Tl9DUkVBVEUQARIfChtQQVJUWV9JRF9DSEFOR0VfUkVBU09OX0pPSU4QAhIg" + "ChxQQVJUWV9JRF9DSEFOR0VfUkVBU09OX0xFQVZFEAQSHwobUEFSVFlfSURf" + "Q0hBTkdFX1JFQVNPTl9LSUNLEAUSIgoeUEFSVFlfSURfQ0hBTkdFX1JFQVNP" + "Tl9ESVNCQU5EEAYqYAoVUGFydHlJbnZpdGVEZWxlZ2F0aW9uEiEKHVBBUlRZ" + "X0lOVklURV9ERUxFR0FUSU9OX09XTkVSEAASJAogUEFSVFlfSU5WSVRFX0RF" + "TEVHQVRJT05fRVZFUllPTkUQASo8CglQYXJ0eUpvaW4SFwoTUEFSVFlfSk9J" + "Tl9ESVNBQkxFRBAAEhYKElBBUlRZX0pPSU5fRU5BQkxFRBABKoYBChNQYXJ0" + "eUpvaW5EZWxlZ2F0aW9uEiIKHlBBUlRZX0pPSU5fREVMRUdBVElPTl9ESVNB" + "QkxFRBAAEiIKHlBBUlRZX0pPSU5fREVMRUdBVElPTl9FVkVSWU9ORRABEicK" + "I1BBUlRZX0pPSU5fREVMRUdBVElPTl9BTExPV0VEX1VTRVJTEAJiBnByb3Rv" + "Mw==");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[1] { MatchmakeStatusReflection.Descriptor }, new GeneratedClrTypeInfo(new Type[5]
		{
			typeof(PartyState),
			typeof(PartyIdChangeReason),
			typeof(PartyInviteDelegation),
			typeof(PartyJoin),
			typeof(PartyJoinDelegation)
		}, null, new GeneratedClrTypeInfo[4]
		{
			new GeneratedClrTypeInfo(typeof(PartyId), PartyId.Parser, new string[2] { "Id", "Reason" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartySettings), PartySettings.Parser, new string[6] { "PartyMaxCount", "InviteDelegation", "JoinDelegation", "AllowedUserIds", "Joinable", "DisbandOnOwnerLeave" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyMember), PartyMember.Parser, new string[6] { "UserId", "IsOwner", "Data", "StaticData", "SortIndex", "State" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PartyCreateParams), PartyCreateParams.Parser, new string[3] { "Settings", "Data", "MemberData" }, null, null, null, null)
		}));
	}
}
