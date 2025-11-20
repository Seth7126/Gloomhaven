using System;

namespace Manatee.Json.Schema;

public class SchemaLoadException : Exception
{
	public MetaSchemaValidationResults MetaValidation { get; }

	internal SchemaLoadException(string message, MetaSchemaValidationResults metaValidation)
		: base(message)
	{
		MetaValidation = metaValidation;
	}
}
