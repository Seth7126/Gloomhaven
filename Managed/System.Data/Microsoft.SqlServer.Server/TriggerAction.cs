namespace Microsoft.SqlServer.Server;

/// <summary>The <see cref="T:Microsoft.SqlServer.Server.TriggerAction" /> enumeration is used by the <see cref="T:Microsoft.SqlServer.Server.SqlTriggerContext" /> class to indicate what action fired the trigger. </summary>
public enum TriggerAction
{
	/// <summary>An invalid trigger action, one that is not exposed to the user, occurred.</summary>
	Invalid = 0,
	/// <summary>An INSERT Transact-SQL statement was executed.</summary>
	Insert = 1,
	/// <summary>An UPDATE Transact-SQL statement was executed.</summary>
	Update = 2,
	/// <summary>A DELETE Transact-SQL statement was executed.</summary>
	Delete = 3,
	/// <summary>A CREATE TABLE Transact-SQL statement was executed.</summary>
	CreateTable = 21,
	/// <summary>An ALTER TABLE Transact-SQL statement was executed.</summary>
	AlterTable = 22,
	/// <summary>A DROP TABLE Transact-SQL statement was executed.</summary>
	DropTable = 23,
	/// <summary>A CREATE INDEX Transact-SQL statement was executed.</summary>
	CreateIndex = 24,
	/// <summary>An ALTER INDEX Transact-SQL statement was executed.</summary>
	AlterIndex = 25,
	/// <summary>A DROP INDEX Transact-SQL statement was executed.</summary>
	DropIndex = 26,
	/// <summary>A CREATE SYNONYM Transact-SQL statement was executed.</summary>
	CreateSynonym = 34,
	/// <summary>A DROP SYNONYM Transact-SQL statement was executed.</summary>
	DropSynonym = 36,
	/// <summary>Not available.</summary>
	CreateSecurityExpression = 31,
	/// <summary>Not available.</summary>
	DropSecurityExpression = 33,
	/// <summary>A CREATE VIEW Transact-SQL statement was executed.</summary>
	CreateView = 41,
	/// <summary>An ALTER VIEW Transact-SQL statement was executed.</summary>
	AlterView = 42,
	/// <summary>A DROP VIEW Transact-SQL statement was executed.</summary>
	DropView = 43,
	/// <summary>A CREATE PROCEDURE Transact-SQL statement was executed.</summary>
	CreateProcedure = 51,
	/// <summary>An ALTER PROCEDURE Transact-SQL statement was executed.</summary>
	AlterProcedure = 52,
	/// <summary>A DROP PROCEDURE Transact-SQL statement was executed.</summary>
	DropProcedure = 53,
	/// <summary>A CREATE FUNCTION Transact-SQL statement was executed.</summary>
	CreateFunction = 61,
	/// <summary>An ALTER FUNCTION Transact-SQL statement was executed.</summary>
	AlterFunction = 62,
	/// <summary>A DROP FUNCTION Transact-SQL statement was executed.</summary>
	DropFunction = 63,
	/// <summary>A CREATE TRIGGER Transact-SQL statement was executed.</summary>
	CreateTrigger = 71,
	/// <summary>An ALTER TRIGGER Transact-SQL statement was executed.</summary>
	AlterTrigger = 72,
	/// <summary>A DROP TRIGGER Transact-SQL statement was executed.</summary>
	DropTrigger = 73,
	/// <summary>A CREATE EVENT NOTIFICATION Transact-SQL statement was executed.</summary>
	CreateEventNotification = 74,
	/// <summary>A DROP EVENT NOTIFICATION Transact-SQL statement was executed.</summary>
	DropEventNotification = 76,
	/// <summary>A CREATE TYPE Transact-SQL statement was executed.</summary>
	CreateType = 91,
	/// <summary>A DROP TYPE Transact-SQL statement was executed.</summary>
	DropType = 93,
	/// <summary>A CREATE ASSEMBLY Transact-SQL statement was executed.</summary>
	CreateAssembly = 101,
	/// <summary>An ALTER ASSEMBLY Transact-SQL statement was executed.</summary>
	AlterAssembly = 102,
	/// <summary>A DROP ASSEMBLY Transact-SQL statement was executed.</summary>
	DropAssembly = 103,
	/// <summary>A CREATE USER Transact-SQL statement was executed.</summary>
	CreateUser = 131,
	/// <summary>An ALTER USER Transact-SQL statement was executed.</summary>
	AlterUser = 132,
	/// <summary>A DROP USER Transact-SQL statement was executed.</summary>
	DropUser = 133,
	/// <summary>A CREATE ROLE Transact-SQL statement was executed.</summary>
	CreateRole = 134,
	/// <summary>An ALTER ROLE Transact-SQL statement was executed.</summary>
	AlterRole = 135,
	/// <summary>A DROP ROLE Transact-SQL statement was executed.</summary>
	DropRole = 136,
	/// <summary>A CREATE APPLICATION ROLE Transact-SQL statement was executed.</summary>
	CreateAppRole = 137,
	/// <summary>An ALTER APPLICATION ROLE Transact-SQL statement was executed.</summary>
	AlterAppRole = 138,
	/// <summary>A DROP APPLICATION ROLE Transact-SQL statement was executed.</summary>
	DropAppRole = 139,
	/// <summary>A CREATE SCHEMA Transact-SQL statement was executed.</summary>
	CreateSchema = 141,
	/// <summary>An ALTER SCHEMA Transact-SQL statement was executed.</summary>
	AlterSchema = 142,
	/// <summary>A DROP SCHEMA Transact-SQL statement was executed.</summary>
	DropSchema = 143,
	/// <summary>A CREATE LOGIN Transact-SQL statement was executed.</summary>
	CreateLogin = 144,
	/// <summary>An ALTER LOGIN Transact-SQL statement was executed.</summary>
	AlterLogin = 145,
	/// <summary>A DROP LOGIN Transact-SQL statement was executed.</summary>
	DropLogin = 146,
	/// <summary>A CREATE MESSAGE TYPE Transact-SQL statement was executed.</summary>
	CreateMsgType = 151,
	/// <summary>A DROP MESSAGE TYPE Transact-SQL statement was executed.</summary>
	DropMsgType = 153,
	/// <summary>A CREATE CONTRACT Transact-SQL statement was executed.</summary>
	CreateContract = 154,
	/// <summary>A DROP CONTRACT Transact-SQL statement was executed.</summary>
	DropContract = 156,
	/// <summary>A CREATE QUEUE Transact-SQL statement was executed.</summary>
	CreateQueue = 157,
	/// <summary>An ALTER QUEUE Transact-SQL statement was executed.</summary>
	AlterQueue = 158,
	/// <summary>A DROP QUEUE Transact-SQL statement was executed.</summary>
	DropQueue = 159,
	/// <summary>A CREATE SERVICE Transact-SQL statement was executed.</summary>
	CreateService = 161,
	/// <summary>An ALTER SERVICE Transact-SQL statement was executed.</summary>
	AlterService = 162,
	/// <summary>A DROP SERVICE Transact-SQL statement was executed.</summary>
	DropService = 163,
	/// <summary>A CREATE ROUTE Transact-SQL statement was executed.</summary>
	CreateRoute = 164,
	/// <summary>An ALTER ROUTE Transact-SQL statement was executed.</summary>
	AlterRoute = 165,
	/// <summary>A DROP ROUTE Transact-SQL statement was executed.</summary>
	DropRoute = 166,
	/// <summary>A GRANT Transact-SQL statement was executed.</summary>
	GrantStatement = 167,
	/// <summary>A DENY Transact-SQL statement was executed.</summary>
	DenyStatement = 168,
	/// <summary>A REVOKE Transact-SQL statement was executed.</summary>
	RevokeStatement = 169,
	/// <summary>A GRANT OBJECT Transact-SQL statement was executed.</summary>
	GrantObject = 170,
	/// <summary>A DENY Object Permissions Transact-SQL statement was executed.</summary>
	DenyObject = 171,
	/// <summary>A REVOKE OBJECT Transact-SQL statement was executed.</summary>
	RevokeObject = 172,
	/// <summary>A CREATE_REMOTE_SERVICE_BINDING event type was specified when an event notification was created on the database or server instance.</summary>
	CreateBinding = 174,
	/// <summary>An ALTER_REMOTE_SERVICE_BINDING event type was specified when an event notification was created on the database or server instance.</summary>
	AlterBinding = 175,
	/// <summary>A DROP_REMOTE_SERVICE_BINDING event type was specified when an event notification was created on the database or server instance.</summary>
	DropBinding = 176,
	/// <summary>A CREATE PARTITION FUNCTION Transact-SQL statement was executed.</summary>
	CreatePartitionFunction = 191,
	/// <summary>An ALTER PARTITION FUNCTION Transact-SQL statement was executed.</summary>
	AlterPartitionFunction = 192,
	/// <summary>A DROP PARTITION FUNCTION Transact-SQL statement was executed.</summary>
	DropPartitionFunction = 193,
	/// <summary>A CREATE PARTITION SCHEME Transact-SQL statement was executed.</summary>
	CreatePartitionScheme = 194,
	/// <summary>An ALTER PARTITION SCHEME Transact-SQL statement was executed.</summary>
	AlterPartitionScheme = 195,
	/// <summary>A DROP PARTITION SCHEME Transact-SQL statement was executed.</summary>
	DropPartitionScheme = 196
}
