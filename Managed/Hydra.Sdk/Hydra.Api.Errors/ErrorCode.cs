using Google.Protobuf.Reflection;

namespace Hydra.Api.Errors;

public enum ErrorCode
{
	[OriginalName("SUCCESS")]
	Success = 0,
	[OriginalName("CANCELLED")]
	Cancelled = 1,
	[OriginalName("UNKNOWN_ERROR")]
	UnknownError = 2,
	[OriginalName("INVALID_ARGUMENT")]
	InvalidArgument = 3,
	[OriginalName("DEADLINE_EXCEEDED")]
	DeadlineExceeded = 4,
	[OriginalName("NOT_FOUND")]
	NotFound = 5,
	[OriginalName("ALREADY_EXISTS")]
	AlreadyExists = 6,
	[OriginalName("PERMISSION_DENIED")]
	PermissionDenied = 7,
	[OriginalName("RESOURCE_EXHAUSTED")]
	ResourceExhausted = 8,
	[OriginalName("FAILED_PRECONDITION")]
	FailedPrecondition = 9,
	[OriginalName("ABORTED")]
	Aborted = 10,
	[OriginalName("OUT_OF_RANGE")]
	OutOfRange = 11,
	[OriginalName("UNIMPLEMENTED")]
	Unimplemented = 12,
	[OriginalName("INTERNAL_ERROR")]
	InternalError = 13,
	[OriginalName("UNAVAILABLE")]
	Unavailable = 14,
	[OriginalName("DATA_LOSS")]
	DataLoss = 15,
	[OriginalName("UNAUTHENTICATED")]
	Unauthenticated = 16,
	[OriginalName("INVALID_CONTEXT")]
	InvalidContext = 50,
	[OriginalName("INVALID_HYDRA_USER_ID")]
	InvalidHydraUserId = 60,
	[OriginalName("SDK_NOT_INITIALIZED")]
	SdkNotInitialized = 100,
	[OriginalName("SDK_BUSY")]
	SdkBusy = 101,
	[OriginalName("SDK_INTERNAL_ERROR")]
	SdkInternalError = 103,
	[OriginalName("SDK_TIMEOUT")]
	SdkTimeout = 104,
	[OriginalName("SDK_PARSE_ERROR")]
	SdkParseError = 105,
	[OriginalName("SDK_CANCELLED")]
	SdkCancelled = 109,
	[OriginalName("SDK_NOT_ALLOWED")]
	SdkNotAllowed = 113,
	[OriginalName("SDK_TIMEOUT_SPIKE")]
	SdkTimeoutSpike = 114,
	[OriginalName("SDK_INVALID_PARAMETER")]
	SdkInvalidParameter = 120,
	[OriginalName("SDK_NO_SERVICE_ENDPOINT")]
	SdkNoServiceEndpoint = 121,
	[OriginalName("SDK_ENDPOINTS_DISPATCHER_NO_DEFAULT_ENVIRONMENT")]
	SdkEndpointsDispatcherNoDefaultEnvironment = 122,
	[OriginalName("SDK_INVALID_STATE")]
	SdkInvalidState = 123,
	[OriginalName("SDK_NOT_A_MEMBER")]
	SdkNotAMember = 130,
	[OriginalName("SDK_NOT_AN_OWNER")]
	SdkNotAnOwner = 131,
	[OriginalName("SDK_USER_IS_NOT_ALLOWED")]
	SdkUserIsNotAllowed = 132,
	[OriginalName("SDK_MAX_COUNT_LIMIT")]
	SdkMaxCountLimit = 134,
	[OriginalName("SDK_NOT_FOUND")]
	SdkNotFound = 135,
	[OriginalName("SDK_ASYNC_WAIT_CALLBACK")]
	SdkAsyncWaitCallback = 136,
	[OriginalName("USER_LOGIN_ALREADY_EXISTS")]
	UserLoginAlreadyExists = 302,
	[OriginalName("USER_SESSION_WAS_NOT_FOUND")]
	UserSessionWasNotFound = 308,
	[OriginalName("USER_VERSION_MISMATCH")]
	UserVersionMismatch = 342,
	[OriginalName("USER_DATA_TOO_LARGE")]
	UserDataTooLarge = 344,
	[OriginalName("USER_SESSION_REQUEST_CONTEXT_IS_MALFORMED")]
	UserSessionRequestContextIsMalformed = 345,
	[OriginalName("USER_SIGNED_OUT")]
	UserSignedOut = 347,
	[OriginalName("USER_DATA_ACCESS_DENIED")]
	UserDataAccessDenied = 352,
	[OriginalName("USER_DATA_INVALID_PARAMETERS")]
	UserDataInvalidParameters = 353,
	[OriginalName("USER_DATA_RULE_FOR_CONTAINER_DOES_NOT_EXIST")]
	UserDataRuleForContainerDoesNotExist = 354,
	[OriginalName("USER_SESSION_EXPIRED")]
	UserSessionExpired = 361,
	[OriginalName("AUTHORIZATION_INVALID_CREDENTIALS")]
	AuthorizationInvalidCredentials = 401,
	[OriginalName("AUTHORIZATION_INVALID_LOGIN")]
	AuthorizationInvalidLogin = 403,
	[OriginalName("AUTHORIZATION_INVALID_TOKEN")]
	AuthorizationInvalidToken = 411,
	[OriginalName("AUTHORIZATION_TOKEN_EXPIRED")]
	AuthorizationTokenExpired = 412,
	[OriginalName("AUTHORIZATION_INVALID_AUTH_TICKET")]
	AuthorizationInvalidAuthTicket = 415,
	[OriginalName("AUTHORIZATION_DOUBLE_SIGN_IN")]
	AuthorizationDoubleSignIn = 427,
	[OriginalName("AUTHORIZATION_SESSION_STORAGE_ISSUE")]
	AuthorizationSessionStorageIssue = 441,
	[OriginalName("AUTHORIZATION_PROVIDER_IS_DISABLED")]
	AuthorizationProviderIsDisabled = 442,
	[OriginalName("AUTHORIZATION_TITLE_NOT_FOUND")]
	AuthorizationTitleNotFound = 451,
	[OriginalName("AUTHORIZATION_TITLE_IS_DISABLED")]
	AuthorizationTitleIsDisabled = 452,
	[OriginalName("AUTHORIZATION_INVALID_TITLE_ID")]
	AuthorizationInvalidTitleId = 453,
	[OriginalName("AUTHORIZATION_ROLE_NOT_FOUND")]
	AuthorizationRoleNotFound = 455,
	[OriginalName("AUTHORIZATION_SECRET_NOT_FOUND")]
	AuthorizationSecretNotFound = 461,
	[OriginalName("AUTHORIZATION_SECRET_EXPIRED")]
	AuthorizationSecretExpired = 462,
	[OriginalName("AUTHORIZATION_SECRET_NOT_ACTIVE")]
	AuthorizationSecretNotActive = 463,
	[OriginalName("AUTHORIZATION_INVALID_STANDALONE_CODE")]
	AuthorizationInvalidStandaloneCode = 465,
	[OriginalName("AUTHORIZATION_NINTENDO_ENTITLEMENT_CHECK_FAILED")]
	AuthorizationNintendoEntitlementCheckFailed = 466,
	[OriginalName("AUTHORIZATION_PSN_EMPTY_ID_TOKEN")]
	AuthorizationPsnEmptyIdToken = 467,
	[OriginalName("EOS_OWNERSHIP_VERIFICATION_FAILED")]
	EosOwnershipVerificationFailed = 510,
	[OriginalName("STEAM_TOKEN_VALIDATION_FAILED")]
	SteamTokenValidationFailed = 520,
	[OriginalName("STEAM_GAME_ACCESS_VALIDATION_FAILED")]
	SteamGameAccessValidationFailed = 521,
	[OriginalName("GAME_CONFIGURATION_INVALID_VERSION")]
	GameConfigurationInvalidVersion = 607,
	[OriginalName("DATACENTER_TAG_NOT_FOUND")]
	DatacenterTagNotFound = 700,
	[OriginalName("DATACENTER_PROVIDER_TAG_NOT_FOUND")]
	DatacenterProviderTagNotFound = 701,
	[OriginalName("DATACENTER_CONFIGURATION_TAG_IS_NOT_UNIQUE")]
	DatacenterConfigurationTagIsNotUnique = 702,
	[OriginalName("DATA_CENTER_NOT_FOUND")]
	DataCenterNotFound = 703,
	[OriginalName("DATA_CENTER_IN_USE")]
	DataCenterInUse = 704,
	[OriginalName("DATA_CENTER_ALREADY_EXISTS")]
	DataCenterAlreadyExists = 705,
	[OriginalName("DATA_CENTER_BAD_REQUEST")]
	DataCenterBadRequest = 706,
	[OriginalName("FACTS_UNABLE_TO_WRITE_INDEX")]
	FactsUnableToWriteIndex = 800,
	[OriginalName("FACTS_CATEGORIES_CONTEXTS_EMPTY")]
	FactsCategoriesContextsEmpty = 801,
	[OriginalName("FACTS_PARAMETERS_NOT_SPECIFIED")]
	FactsParametersNotSpecified = 802,
	[OriginalName("FACTS_UNABLE_TO_WRITE_CHUNK")]
	FactsUnableToWriteChunk = 803,
	[OriginalName("SERVER_MANAGER_AGENT_NOT_FOUND")]
	ServerManagerAgentNotFound = 900,
	[OriginalName("SERVER_MANAGER_PACKS_COUNT_LIMIT_EXCEEDED")]
	ServerManagerPacksCountLimitExceeded = 902,
	[OriginalName("SERVER_MANAGER_INVALID_DATACENTER_CONFIGURATION_TAG")]
	ServerManagerInvalidDatacenterConfigurationTag = 903,
	[OriginalName("SESSION_CONTROL_SESSION_NOT_FOUND")]
	SessionControlSessionNotFound = 1053,
	[OriginalName("SESSION_CONTROL_SESSION_FINISHED_NORMAL")]
	SessionControlSessionFinishedNormal = 1054,
	[OriginalName("SESSION_CONTROL_SESSION_FINISHED_REJECTED")]
	SessionControlSessionFinishedRejected = 1055,
	[OriginalName("SESSION_CONTROL_SESSION_FINISH_NO_MATCHING_DSM")]
	SessionControlSessionFinishNoMatchingDsm = 1056,
	[OriginalName("SESSION_CONTROL_SESSION_FINISH_TIMEOUT_ACTIVATE")]
	SessionControlSessionFinishTimeoutActivate = 1057,
	[OriginalName("SESSION_CONTROL_SESSION_FINISH_TIMEOUT_DS")]
	SessionControlSessionFinishTimeoutDs = 1058,
	[OriginalName("SESSION_CONTROL_DSM_TIMEOUT")]
	SessionControlDsmTimeout = 1059,
	[OriginalName("SESSION_CONTROL_INVALID_CLIENT_VERSION")]
	SessionControlInvalidClientVersion = 1060,
	[OriginalName("SESSION_CONTROL_INVALID_MAX_PLAYERS_COUNT")]
	SessionControlInvalidMaxPlayersCount = 1061,
	[OriginalName("SESSION_CONTROL_SESSION_ALREADY_ACTIVATED")]
	SessionControlSessionAlreadyActivated = 1067,
	[OriginalName("SESSION_CONTROL_SESSION_ACTIVATION_FAILED")]
	SessionControlSessionActivationFailed = 1068,
	[OriginalName("MESSAGING_UNEXPECTED")]
	MessagingUnexpected = 1100,
	[OriginalName("MESSAGING_INVALID_ARGUMENT")]
	MessagingInvalidArgument = 1101,
	[OriginalName("MESSAGING_CHANNEL_NAME_VALIDATION")]
	MessagingChannelNameValidation = 1102,
	[OriginalName("MESSAGING_MESSAGE_HAS_INVALID_SIZE")]
	MessagingMessageHasInvalidSize = 1103,
	[OriginalName("MESSAGING_CHANNEL_HAS_INVALID_MEMBER_COUNT")]
	MessagingChannelHasInvalidMemberCount = 1104,
	[OriginalName("MESSAGING_CHANNEL_ALREADY_EXISTS")]
	MessagingChannelAlreadyExists = 1105,
	[OriginalName("MESSAGING_CHANNEL_REACHED_MAX_MEMBER_COUNT")]
	MessagingChannelReachedMaxMemberCount = 1106,
	[OriginalName("MESSAGING_CHANNEL_MEMBERSHIP_LOCKED")]
	MessagingChannelMembershipLocked = 1107,
	[OriginalName("MESSAGING_USER_NOT_CHANNEL_OWNER")]
	MessagingUserNotChannelOwner = 1108,
	[OriginalName("MESSAGING_CHANNEL_INVALID_CREDENTIALS")]
	MessagingChannelInvalidCredentials = 1109,
	[OriginalName("MESSAGING_CHANNEL_NO_ACCESS")]
	MessagingChannelNoAccess = 1110,
	[OriginalName("MESSAGING_CHANNEL_NO_WRITING")]
	MessagingChannelNoWriting = 1111,
	[OriginalName("MESSAGING_USER_NOT_JOINED_CHANNEL")]
	MessagingUserNotJoinedChannel = 1112,
	[OriginalName("ENDPOINT_DISPATCHER_AUTH_ENDPOINTS_NOT_FOUND")]
	EndpointDispatcherAuthEndpointsNotFound = 1201,
	[OriginalName("ENDPOINT_DISPATCHER_INCOMPATIBLE_CLIENT_VERSIONS")]
	EndpointDispatcherIncompatibleClientVersions = 1203,
	[OriginalName("PRESENCE_UNEXPECTED")]
	PresenceUnexpected = 1300,
	[OriginalName("PRESENCE_PARTY_ALREADY_MEMBER")]
	PresencePartyAlreadyMember = 1308,
	[OriginalName("PRESENCE_PARTY_MAX_COUNT_LIMIT")]
	PresencePartyMaxCountLimit = 1309,
	[OriginalName("PRESENCE_PLAYLIST_NOT_FOUND")]
	PresencePlaylistNotFound = 1350,
	[OriginalName("PRESENCE_PARTY_NOT_A_MEMBER")]
	PresencePartyNotAMember = 1351,
	[OriginalName("PRESENCE_PARTY_NOT_AN_OWNER")]
	PresencePartyNotAnOwner = 1352,
	[OriginalName("PRESENCE_SESSION_NOT_A_MEMBER")]
	PresenceSessionNotAMember = 1353,
	[OriginalName("PRESENCE_SESSION_NOT_AN_OWNER")]
	PresenceSessionNotAnOwner = 1354,
	[OriginalName("PRESENCE_INVALID_STATE")]
	PresenceInvalidState = 1363,
	[OriginalName("PRESENCE_SESSION_JOIN_NO_SESSION")]
	PresenceSessionJoinNoSession = 1364,
	[OriginalName("PRESENCE_SESSION_JOIN_DOMAIN_MISMATCH")]
	PresenceSessionJoinDomainMismatch = 1365,
	[OriginalName("PRESENCE_SESSION_JOIN_SESSION_FULL")]
	PresenceSessionJoinSessionFull = 1366,
	[OriginalName("PRESENCE_SESSION_JOIN_SESSION_TEAM_MODE")]
	PresenceSessionJoinSessionTeamMode = 1367,
	[OriginalName("PRESENCE_SESSION_JOIN_SESSION_SETTINGS")]
	PresenceSessionJoinSessionSettings = 1368,
	[OriginalName("PRESENCE_SESSION_JOIN_IN_SESSION")]
	PresenceSessionJoinInSession = 1369,
	[OriginalName("PRESENCE_VARIANTS_DUPLICATE_KEYS")]
	PresenceVariantsDuplicateKeys = 1370,
	[OriginalName("PRESENCE_VARIANTS_DUPLICATE_VALUES")]
	PresenceVariantsDuplicateValues = 1371,
	[OriginalName("PRESENCE_DCPINGS_EMPTY")]
	PresenceDcpingsEmpty = 1372,
	[OriginalName("PRESENCE_SESSSION_INVALID_MAX_COUNT")]
	PresenceSesssionInvalidMaxCount = 1373,
	[OriginalName("PRESENCE_MEMBERS_EXCEED")]
	PresenceMembersExceed = 1381,
	[OriginalName("PRESENCE_PARTY_JOIN_NO_PARTY")]
	PresencePartyJoinNoParty = 1385,
	[OriginalName("PRESENCE_PARTY_NOT_INVITABLE")]
	PresencePartyNotInvitable = 1387,
	[OriginalName("PRESENCE_USER_TIMEOUTED")]
	PresenceUserTimeouted = 1389,
	[OriginalName("PRESENCE_INVITE_ALREADY_SENT")]
	PresenceInviteAlreadySent = 1390,
	[OriginalName("PRESENCE_INVITE_NOT_FOUND")]
	PresenceInviteNotFound = 1391,
	[OriginalName("PRESENCE_INVITE_DELEGATION_OWNER")]
	PresenceInviteDelegationOwner = 1392,
	[OriginalName("PRESENCE_PARTY_MAX_COUNT_EXCEEDS_PLAYLIST_COUNT")]
	PresencePartyMaxCountExceedsPlaylistCount = 1393,
	[OriginalName("PRESENCE_PARTY_MAX_COUNT_EXCEEDS_CURRENT_COUNT")]
	PresencePartyMaxCountExceedsCurrentCount = 1394,
	[OriginalName("PRESENCE_PARTY_IS_NOT_JOINABLE")]
	PresencePartyIsNotJoinable = 1395,
	[OriginalName("PRESENCE_INVITE_PARTY_IS_NOT_JOINABLE")]
	PresenceInvitePartyIsNotJoinable = 1396,
	[OriginalName("PRESENCE_JOIN_CODE_PARTY_IS_NOT_JOINABLE")]
	PresenceJoinCodePartyIsNotJoinable = 1397,
	[OriginalName("PRESENCE_DATA_SIZE_LIMIT")]
	PresenceDataSizeLimit = 1399,
	[OriginalName("PRESENCE_PARTY_MEMBERS_INCORRECT_STATE")]
	PresencePartyMembersIncorrectState = 1400,
	[OriginalName("PRESENCE_EXTERNAL_CALL_TIMEOUT")]
	PresenceExternalCallTimeout = 1401,
	[OriginalName("ZEN_TOURNAMENT_NOT_FOUND")]
	ZenTournamentNotFound = 1450,
	[OriginalName("ZEN_TOURNAMENT_FINISHED")]
	ZenTournamentFinished = 1451,
	[OriginalName("ZEN_TOURNAMENT_INVALID_PROPERTIES")]
	ZenTournamentInvalidProperties = 1452,
	[OriginalName("ZEN_TOURNAMENT_INVALID_ATTEMPT_KEY")]
	ZenTournamentInvalidAttemptKey = 1453,
	[OriginalName("ZEN_TOURNAMENT_ONLY_ONE_ACTIVE_USER_TOURNAMENT_ALLOWED")]
	ZenTournamentOnlyOneActiveUserTournamentAllowed = 1454,
	[OriginalName("ZEN_TOURNAMENT_INCORRECT_PASSWORD")]
	ZenTournamentIncorrectPassword = 1455,
	[OriginalName("PUSH_TOKEN_ALREADY_CONNECTED")]
	PushTokenAlreadyConnected = 1500,
	[OriginalName("PUSH_SYSTEM_FAILED_TO_INITIALIZE")]
	PushSystemFailedToInitialize = 1501,
	[OriginalName("LEADERBOARD_INVALID_CONFIGURATION")]
	LeaderboardInvalidConfiguration = 1600,
	[OriginalName("LEADERBOARD_UPDATE_EXCEEDS_MAX_UPDATE_COUNT")]
	LeaderboardUpdateExceedsMaxUpdateCount = 1602,
	[OriginalName("LEADERBOARD_UPDATE_MUST_CONTAIN_USER_RESULTS")]
	LeaderboardUpdateMustContainUserResults = 1603,
	[OriginalName("LEADERBOARD_RESULTS_COUNT_EXCEEDS_256")]
	LeaderboardResultsCountExceeds256 = 1604,
	[OriginalName("LEADERBOARD_CUSTOM_DATA_EXCEEDS_1024")]
	LeaderboardCustomDataExceeds1024 = 1605,
	[OriginalName("LEADERBOARD_WRONG_LEADERBOARD_ID")]
	LeaderboardWrongLeaderboardId = 1606,
	[OriginalName("LEADERBOARD_TEMPORARY_UNAVAILABLE")]
	LeaderboardTemporaryUnavailable = 1607,
	[OriginalName("RATINGS_CONFIGURATION_INVALID_RATING_ID")]
	RatingsConfigurationInvalidRatingId = 1650,
	[OriginalName("RATINGS_RESULTS_COUNT_EXCEEDS_256")]
	RatingsResultsCountExceeds256 = 1651,
	[OriginalName("RATINGS_CUSTOM_DATA_EXCEEDS_1024")]
	RatingsCustomDataExceeds1024 = 1652,
	[OriginalName("RATINGS_UPDATE_EXCEEDS_MAX_UPDATE_COUNT")]
	RatingsUpdateExceedsMaxUpdateCount = 1653,
	[OriginalName("RATINGS_WRONG_RATING_ID")]
	RatingsWrongRatingId = 1654,
	[OriginalName("CHALLENGES_CHALLENGE_UPDATE_FAILED")]
	ChallengesChallengeUpdateFailed = 1802,
	[OriginalName("CHALLENGES_CHALLENGE_UPDATE_AFTER_REWARD_FAILED")]
	ChallengesChallengeUpdateAfterRewardFailed = 1804,
	[OriginalName("ECONOMY_OFFER_NOT_FOUND")]
	EconomyOfferNotFound = 1900,
	[OriginalName("ECONOMY_UNEXPECTED_OFFER_ERROR")]
	EconomyUnexpectedOfferError = 1901,
	[OriginalName("ECONOMY_DOES_NOT_HAVE_ITEM")]
	EconomyDoesNotHaveItem = 1902,
	[OriginalName("ECONOMY_ITEM_ALREADY_BOUGHT")]
	EconomyItemAlreadyBought = 1903,
	[OriginalName("ECONOMY_COMMAND_NOT_SUPPORTED")]
	EconomyCommandNotSupported = 1904,
	[OriginalName("ECONOMY_ERROR")]
	EconomyError = 1910,
	[OriginalName("ECONOMY_DBC_ERROR")]
	EconomyDbcError = 1912,
	[OriginalName("ECONOMY_VALIDATION_FAILED")]
	EconomyValidationFailed = 1914,
	[OriginalName("OFFER_UNSUCCESSFUL_STEAM_RESULT")]
	OfferUnsuccessfulSteamResult = 1916,
	[OriginalName("OFFER_GOOGLE_PLAY_RMT_FAILED")]
	OfferGooglePlayRmtFailed = 1932,
	[OriginalName("BI_KAFKA_SENDING_FAILED")]
	BiKafkaSendingFailed = 3001,
	[OriginalName("BI_KAFKA_CONNECTION_ERROR")]
	BiKafkaConnectionError = 3002,
	[OriginalName("BOT_SERVICE_RETRIABLE")]
	BotServiceRetriable = 4000,
	[OriginalName("BOT_SERVICE_FATAL")]
	BotServiceFatal = 4001,
	[OriginalName("BOT_SERVICE_TERMINAL")]
	BotServiceTerminal = 4002,
	[OriginalName("BOT_LOGIC_ERROR")]
	BotLogicError = 4003,
	[OriginalName("ROLES_INVALID_PARAMETERS")]
	RolesInvalidParameters = 6000,
	[OriginalName("TITLES_WORLD_ID_IS_EMPTY")]
	TitlesWorldIdIsEmpty = 6001,
	[OriginalName("TITLES_STORAGE_IS_FULL")]
	TitlesStorageIsFull = 6002,
	[OriginalName("TITLE_NAME_IS_INVALID")]
	TitleNameIsInvalid = 6003,
	[OriginalName("BUILDS_GROUP_DATA_FAILED")]
	BuildsGroupDataFailed = 6500,
	[OriginalName("BUILDS_GROUP_REQUEST_FAILED")]
	BuildsGroupRequestFailed = 6501,
	[OriginalName("BUILDS_GROUP_LOGIC_FAILED")]
	BuildsGroupLogicFailed = 6502,
	[OriginalName("BUILD_VERSION_ALREADY_REGISTERED")]
	BuildVersionAlreadyRegistered = 6503,
	[OriginalName("BUILD_VERSION_IS_NOT_SPECIFIED")]
	BuildVersionIsNotSpecified = 6504,
	[OriginalName("DIAGNOSTICS_VALIDATION_FAILED")]
	DiagnosticsValidationFailed = 7201,
	[OriginalName("DIAGNOSTICS_DATA_IS_EMPTY")]
	DiagnosticsDataIsEmpty = 7202,
	[OriginalName("DIAGNOSTICS_UNABLE_TO_WRITE_DATA")]
	DiagnosticsUnableToWriteData = 7206,
	[OriginalName("TITLES_STORAGE_WRONG_IMAGE_TYPE")]
	TitlesStorageWrongImageType = 7301,
	[OriginalName("WEB_PORTAL_BACKEND_PARAMETER_INVALID_VALUE")]
	WebPortalBackendParameterInvalidValue = 8000,
	[OriginalName("WEB_PORTAL_BACKEND_TOKEN_DECRYPTION_FAILED")]
	WebPortalBackendTokenDecryptionFailed = 8001,
	[OriginalName("PLUGIN_BAD_REQUEST")]
	PluginBadRequest = 13000,
	[OriginalName("PLUGIN_INTERNAL_ERROR")]
	PluginInternalError = 13001,
	[OriginalName("PLUGIN_NOT_FOUND")]
	PluginNotFound = 13002,
	[OriginalName("PLUGIN_BUILD_NOT_FOUND")]
	PluginBuildNotFound = 13003,
	[OriginalName("PLUGIN_NOT_BELONGS_TO_TITLE")]
	PluginNotBelongsToTitle = 13004,
	[OriginalName("PLUGIN_BUILD_NOT_BELONGS_TO_TITLE")]
	PluginBuildNotBelongsToTitle = 13005,
	[OriginalName("PLUGIN_ALREADY_EXISTS")]
	PluginAlreadyExists = 13006,
	[OriginalName("PLUGIN_SECRET_NOT_FOUND")]
	PluginSecretNotFound = 13007,
	[OriginalName("PLUGIN_BUILD_ALREADY_UPLOADED")]
	PluginBuildAlreadyUploaded = 13008,
	[OriginalName("PLUGIN_BUILD_NOT_UPLOADED")]
	PluginBuildNotUploaded = 13009,
	[OriginalName("PLUGIN_BUILD_ALREADY_REGISTERED")]
	PluginBuildAlreadyRegistered = 13010,
	[OriginalName("BINARY_PACK_INVALID_COMPRESSION")]
	BinaryPackInvalidCompression = 14000,
	[OriginalName("BINARY_PACK_INVALID_FORMAT")]
	BinaryPackInvalidFormat = 14001,
	[OriginalName("NOTIFICATIONS_SLACK_TOKEN_IS_NOT_VALID")]
	NotificationsSlackTokenIsNotValid = 14100,
	[OriginalName("NOTIFICATIONS_TELEGRAM_TOKEN_IS_NOT_VALID")]
	NotificationsTelegramTokenIsNotValid = 14101,
	[OriginalName("NOTIFICATIONS_SLACK_TOKEN_IS_ALREADY_LINKED")]
	NotificationsSlackTokenIsAlreadyLinked = 14102,
	[OriginalName("NOTIFICATIONS_TELEGRAM_TOKEN_IS_ALREADY_LINKED")]
	NotificationsTelegramTokenIsAlreadyLinked = 14103,
	[OriginalName("NOTIFICATIONS_SLACK_NOT_ALLOWED_TOKEN_TYPE")]
	NotificationsSlackNotAllowedTokenType = 14104,
	[OriginalName("NOC_PORTAL_SEARCH_TOKEN_DECRYPTION_FAILED")]
	NocPortalSearchTokenDecryptionFailed = 14200
}
