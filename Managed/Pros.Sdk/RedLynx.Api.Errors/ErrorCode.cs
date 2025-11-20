using Google.Protobuf.Reflection;

namespace RedLynx.Api.Errors;

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
	[OriginalName("CONCURRENCY_FAILURE")]
	ConcurrencyFailure = 51,
	[OriginalName("SDK_NOT_INITIALIZED")]
	SdkNotInitialized = 100,
	[OriginalName("SDK_INTERNAL_ERROR")]
	SdkInternalError = 103,
	[OriginalName("SDK_TIMEOUT")]
	SdkTimeout = 104,
	[OriginalName("SDK_PARSE_ERROR")]
	SdkParseError = 105,
	[OriginalName("SDK_CANCELLED")]
	SdkCancelled = 109,
	[OriginalName("SDK_TIMEOUT_SPIKE")]
	SdkTimeoutSpike = 114,
	[OriginalName("SDK_INVALID_PARAMETER")]
	SdkInvalidParameter = 120,
	[OriginalName("SDK_NO_SERVICE_ENDPOINT")]
	SdkNoServiceEndpoint = 121,
	[OriginalName("SDK_ENDPOINTS_DISPATCHER_NO_DEFAULT_ENVIRONMENT")]
	SdkEndpointsDispatcherNoDefaultEnvironment = 122,
	[OriginalName("USER_LOGIN_ALREADY_EXISTS")]
	UserLoginAlreadyExists = 302,
	[OriginalName("USER_VERSION_MISMATCH")]
	UserVersionMismatch = 342,
	[OriginalName("USER_DATA_TOO_LARGE")]
	UserDataTooLarge = 344,
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
	[OriginalName("AUTHORIZATION_DOUBLE_SIGN_IN")]
	AuthorizationDoubleSignIn = 427,
	[OriginalName("AUTHORIZATION_SESSION_STORAGE_ISSUE")]
	AuthorizationSessionStorageIssue = 441,
	[OriginalName("AUTHORIZATION_PROVIDER_IS_DISABLED")]
	AuthorizationProviderIsDisabled = 442,
	[OriginalName("AUTHORIZATION_SECRET_NOT_FOUND")]
	AuthorizationSecretNotFound = 461,
	[OriginalName("AUTHORIZATION_SECRET_EXPIRED")]
	AuthorizationSecretExpired = 462,
	[OriginalName("AUTHORIZATION_SECRET_NOT_ACTIVE")]
	AuthorizationSecretNotActive = 463,
	[OriginalName("AUTHORIZATION_NINTENDO_ENTITLEMENT_CHECK_FAILED")]
	AuthorizationNintendoEntitlementCheckFailed = 466,
	[OriginalName("EOS_OWNERSHIP_VERIFICATION_FAILED")]
	EosOwnershipVerificationFailed = 510,
	[OriginalName("STEAM_TOKEN_VALIDATION_FAILED")]
	SteamTokenValidationFailed = 520,
	[OriginalName("STEAM_GAME_ACCESS_VALIDATION_FAILED")]
	SteamGameAccessValidationFailed = 521,
	[OriginalName("ENDPOINT_DISPATCHER_AUTH_ENDPOINTS_NOT_FOUND")]
	EndpointDispatcherAuthEndpointsNotFound = 1201,
	[OriginalName("ENDPOINT_DISPATCHER_INCOMPATIBLE_CLIENT_VERSIONS")]
	EndpointDispatcherIncompatibleClientVersions = 1203,
	[OriginalName("OFFER_UNSUCCESSFUL_STEAM_RESULT")]
	OfferUnsuccessfulSteamResult = 1916,
	[OriginalName("BI_KAFKA_SENDING_FAILED")]
	BiKafkaSendingFailed = 3001,
	[OriginalName("BI_KAFKA_CONNECTION_ERROR")]
	BiKafkaConnectionError = 3002,
	[OriginalName("ROLES_INVALID_PARAMETERS")]
	RolesInvalidParameters = 6000,
	[OriginalName("TITLES_WORLD_ID_IS_EMPTY")]
	TitlesWorldIdIsEmpty = 6001,
	[OriginalName("TITLES_STORAGE_IS_FULL")]
	TitlesStorageIsFull = 6002,
	[OriginalName("WEB_PORTAL_BACKEND_PARAMETER_INVALID_VALUE")]
	WebPortalBackendParameterInvalidValue = 8000,
	[OriginalName("WEB_PORTAL_RECAPTCHA_FAILED")]
	WebPortalRecaptchaFailed = 8001,
	[OriginalName("ACCOUNT_CODE_EXPIRED")]
	AccountCodeExpired = 20000,
	[OriginalName("ACCOUNT_CODE_ALREADY_EXIST")]
	AccountCodeAlreadyExist = 20001,
	[OriginalName("ACCOUNT_CODE_NOT_FOUND")]
	AccountCodeNotFound = 20002,
	[OriginalName("ACCOUNT_CODE_NOT_VALID")]
	AccountCodeNotValid = 20003,
	[OriginalName("WP_INVALID_PROVIDER")]
	WpInvalidProvider = 20200,
	[OriginalName("WP_INVALID_PASSWORD")]
	WpInvalidPassword = 20201,
	[OriginalName("WP_INVALID_LOGIN")]
	WpInvalidLogin = 20202,
	[OriginalName("WP_ACCOUNT_ALREADY_EXIST")]
	WpAccountAlreadyExist = 20203,
	[OriginalName("WP_CODE_ALREADY_LINKED")]
	WpCodeAlreadyLinked = 20204,
	[OriginalName("WP_ACCOUNT_NOT_FOUND")]
	WpAccountNotFound = 20205,
	[OriginalName("WP_ACCOUNT_NOT_CONFIRMED")]
	WpAccountNotConfirmed = 20206,
	[OriginalName("WP_ACCOUNT_CONFIRMED")]
	WpAccountConfirmed = 20207,
	[OriginalName("WP_THROTTLE_CONFIRMATION_CODE_GENERATION")]
	WpThrottleConfirmationCodeGeneration = 20208,
	[OriginalName("WP_INVALID_CONFIRMATION_CODE")]
	WpInvalidConfirmationCode = 20209,
	[OriginalName("WP_CONFIRMATION_CODE_EXPIRED")]
	WpConfirmationCodeExpired = 20210,
	[OriginalName("WP_PASSWORD_RESET_CODE_CLAIMED")]
	WpPasswordResetCodeClaimed = 20211,
	[OriginalName("WP_INVALID_PASSWORD_RESET_CODE")]
	WpInvalidPasswordResetCode = 20212,
	[OriginalName("WP_PASSWORD_RESET_CODE_EXPIRED")]
	WpPasswordResetCodeExpired = 20213,
	[OriginalName("WP_CODE_CLAIM_ATTEMPTS_EXCEEDED")]
	WpCodeClaimAttemptsExceeded = 20214,
	[OriginalName("WP_ACCOUNT_INVALID_EMAIL")]
	WpAccountInvalidEmail = 20215,
	[OriginalName("WP_ACCOUNT_ANOTHER_USER_WITH_SAME_PROVIDER_ALREADY_LINKED")]
	WpAccountAnotherUserWithSameProviderAlreadyLinked = 20216,
	[OriginalName("WP_ACCOUNT_ANOTHER_USER_WITH_SAME_PROVIDER_AND_DIFFERENT_TITLE_ALREADY_LINKED")]
	WpAccountAnotherUserWithSameProviderAndDifferentTitleAlreadyLinked = 20217,
	[OriginalName("WP_ACCOUNT_USER_ALREADY_HAVE_ACCOUNT_WITH_LINKED_TITLES_FOR_CURRENT_PROVIDER")]
	WpAccountUserAlreadyHaveAccountWithLinkedTitlesForCurrentProvider = 20218,
	[OriginalName("WP_ACCOUNT_EMAIL_ALREADY_EXIST")]
	WpAccountEmailAlreadyExist = 20219,
	[OriginalName("WP_ACCOUNT_INVALID_USERNAME")]
	WpAccountInvalidUsername = 20220,
	[OriginalName("WP_ACCOUNT_INVALID_PROVIDER_FOR_EXISTING_ACCOUNT")]
	WpAccountInvalidProviderForExistingAccount = 20221,
	[OriginalName("WP_ACCOUNT_PASSWORD_ALREADY_SET")]
	WpAccountPasswordAlreadySet = 20222,
	[OriginalName("WP_ACCOUNT_IS_UNDER_DELETION")]
	WpAccountIsUnderDeletion = 20223,
	[OriginalName("WP_ACCOUNT_IS_NOT_ACTIVE")]
	WpAccountIsNotActive = 20224,
	[OriginalName("WP_ACCOUNT_IS_SUSPENDED")]
	WpAccountIsSuspended = 20225,
	[OriginalName("WP_THROTTLE_CONFIRMATION_CODE_CLAIM")]
	WpThrottleConfirmationCodeClaim = 20226,
	[OriginalName("WP_ACCOUNT_NOT_CONFIRMED_AND_CANT_RESEND_CONFIRMATION_CODE")]
	WpAccountNotConfirmedAndCantResendConfirmationCode = 20227,
	[OriginalName("WP_ACCOUNT_LANGUAGE_IN_WRONG_FORMAT")]
	WpAccountLanguageInWrongFormat = 20228,
	[OriginalName("WP_SUPPORT_FILE_MALICIOUS_DETECTED")]
	WpSupportFileMaliciousDetected = 20229,
	[OriginalName("WP_USERNAME_REQUIRES_MODERATION")]
	WpUsernameRequiresModeration = 20230,
	[OriginalName("ENTITLEMENT_NOT_FOUND")]
	EntitlementNotFound = 20300,
	[OriginalName("ENTITLEMENT_NOT_VALID")]
	EntitlementNotValid = 20301,
	[OriginalName("ENTITLEMENT_UPDATE_FAILED")]
	EntitlementUpdateFailed = 20302,
	[OriginalName("ENTITLEMENT_REWARD_CODE_EXPIRED")]
	EntitlementRewardCodeExpired = 20303,
	[OriginalName("ENTITLEMENT_ACCOUNT_IS_NOT_LINKED_WITH_ANY_USER")]
	EntitlementAccountIsNotLinkedWithAnyUser = 20304,
	[OriginalName("ENTITLEMENT_REWARD_CODE_ALREADY_EXIST")]
	EntitlementRewardCodeAlreadyExist = 20305,
	[OriginalName("ENTITLEMENT_REWARD_CODE_WRONG_ACCOUNT")]
	EntitlementRewardCodeWrongAccount = 20306,
	[OriginalName("ENTITLEMENT_REWARD_CODE_ALREADY_USED")]
	EntitlementRewardCodeAlreadyUsed = 20307,
	[OriginalName("ENTITLEMENT_REWARD_CODE_FILTER_FAILED")]
	EntitlementRewardCodeFilterFailed = 20308,
	[OriginalName("ENTITLEMENT_REWARD_CODE_NOT_FOUND")]
	EntitlementRewardCodeNotFound = 20309,
	[OriginalName("ENTITLEMENT_CONSUME_COUNT_EXCEEDS_CONSUME_LIMIT")]
	EntitlementConsumeCountExceedsConsumeLimit = 20310,
	[OriginalName("ENTITLEMENT_USER_NOT_LINKED")]
	EntitlementUserNotLinked = 20311,
	[OriginalName("ENTITLEMENT_CATALOG_ITEM_ID_ALREADY_EXISTS")]
	EntitlementCatalogItemIdAlreadyExists = 20400,
	[OriginalName("ENTITLEMENT_CATALOG_ITEM_NOT_FOUND")]
	EntitlementCatalogItemNotFound = 20401,
	[OriginalName("ENTITLEMENT_CATALOG_ITEM_INTERNAL_ERROR")]
	EntitlementCatalogItemInternalError = 20402,
	[OriginalName("PII_STORAGE_USER_NOT_FOUND")]
	PiiStorageUserNotFound = 21000,
	[OriginalName("PII_STORAGE_ACCOUNT_NOT_FOUND")]
	PiiStorageAccountNotFound = 21001,
	[OriginalName("PII_SHARD_NOT_FOUND")]
	PiiShardNotFound = 480,
	[OriginalName("BANNER_CONFIGURATION_NOT_FOUND")]
	BannerConfigurationNotFound = 22000,
	[OriginalName("WP_BANNER_CAMPAIGN_ALREADY_EXIST")]
	WpBannerCampaignAlreadyExist = 22001,
	[OriginalName("WP_BANNER_CAMPAIGN_CONTAINS_NONUNIQUE_BANNERS")]
	WpBannerCampaignContainsNonuniqueBanners = 22002,
	[OriginalName("WP_BANNER_CAMPAIGN_CONTAINS_MISMATCHED_BANNERS")]
	WpBannerCampaignContainsMismatchedBanners = 22003,
	[OriginalName("WP_BANNER_CAMPAIGN_BANNERS_REQUIRED")]
	WpBannerCampaignBannersRequired = 22004,
	[OriginalName("WP_BANNER_BAD_CONTENT_LOGIC_CONFIGURATION")]
	WpBannerBadContentLogicConfiguration = 22005,
	[OriginalName("BANNERS_BAD_CONFIGURATION")]
	BannersBadConfiguration = 22006,
	[OriginalName("TITLE_STORAGE_TITLE_DETAILS_NOT_ALLOWED_FOR_REQUESTED_ACCOUNT")]
	TitleStorageTitleDetailsNotAllowedForRequestedAccount = 23000,
	[OriginalName("UNIQUE_CODE_GENERATION_CODE_NOT_CREATED")]
	UniqueCodeGenerationCodeNotCreated = 23100,
	[OriginalName("CAN_NOT_INIT_SEQUENCE_FOR_UNIQUE_CODE_GENERATION")]
	CanNotInitSequenceForUniqueCodeGeneration = 23101,
	[OriginalName("CAN_NOT_UPDATE_SEQUENCE_FOR_UNIQUE_CODE_GENERATION")]
	CanNotUpdateSequenceForUniqueCodeGeneration = 23102,
	[OriginalName("CONTENT_MODERATION_LIST_UPDATE_OPERATION_FAILED")]
	ContentModerationListUpdateOperationFailed = 23110,
	[OriginalName("CONTENT_MODERATION_IMPORT_FAILED")]
	ContentModerationImportFailed = 23111,
	[OriginalName("NOT_SUPPORTED_AGREEMENT_DOCUMENT_TYPE")]
	NotSupportedAgreementDocumentType = 23121,
	[OriginalName("AGREEMENTS_CACHE_CAN_NOT_BE_UPDATED")]
	AgreementsCacheCanNotBeUpdated = 23122,
	[OriginalName("AGREEMENT_STATUS_DRAFT_CAN_NOT_BE_SET")]
	AgreementStatusDraftCanNotBeSet = 23123,
	[OriginalName("AGREEMENT_OR_AGREEMENT_VERSION_NOT_IN_STATUS_DRAFT_CAN_NOT_BE_DELETED")]
	AgreementOrAgreementVersionNotInStatusDraftCanNotBeDeleted = 23124,
	[OriginalName("CAN_NOT_CREATE_AGREEMENT_VERSION_FOR_NON_EXISTING_AGREEMENT")]
	CanNotCreateAgreementVersionForNonExistingAgreement = 23125,
	[OriginalName("NEW_AGREEMENT_VERSION_MUST_BE_GREATER_THAN_LAST_EXISTING_VERSION")]
	NewAgreementVersionMustBeGreaterThanLastExistingVersion = 23126,
	[OriginalName("AGREEMENT_VERSION_TAG_IS_INVALID")]
	AgreementVersionTagIsInvalid = 23127,
	[OriginalName("AGREEMENT_DOES_NOT_EXIST")]
	AgreementDoesNotExist = 23158,
	[OriginalName("AGREEMENT_VERSION_DOES_NOT_EXIST")]
	AgreementVersionDoesNotExist = 23129,
	[OriginalName("CAN_NOT_ACTIVATE_AGREEMENT_VERSION_WITH_LOWER_VERSION_THAN_PREVIOUS_ACTIVE")]
	CanNotActivateAgreementVersionWithLowerVersionThanPreviousActive = 23130,
	[OriginalName("AGREEMENT_OR_AGREEMENT_VERSION_NOT_IN_STATUS_DRAFT_CAN_NOT_BE_SET_TO_STATUS_ACTIVE")]
	AgreementOrAgreementVersionNotInStatusDraftCanNotBeSetToStatusActive = 23131,
	[OriginalName("AGREEMENT_OR_AGREEMENT_VERSION_NOT_IN_STATUS_ACTIVE_CAN_NOT_BE_SET_TO_STATUS_INACTIVE")]
	AgreementOrAgreementVersionNotInStatusActiveCanNotBeSetToStatusInactive = 23132,
	[OriginalName("CAN_NOT_DELETE_AGREEMENT_VERSION_FOR_NON_EXISTING_AGREEMENT")]
	CanNotDeleteAgreementVersionForNonExistingAgreement = 23133,
	[OriginalName("AGREEMENT_WITHOUT_ACTIVE_VERSION_CAN_NOT_BE_ACTIVATED")]
	AgreementWithoutActiveVersionCanNotBeActivated = 23134,
	[OriginalName("SIGNED_AGREEMENT_CAN_NOT_BE_CHANGED")]
	SignedAgreementCanNotBeChanged = 23135,
	[OriginalName("AGREEMENT_VERSION_ALREADY_EXISTS")]
	AgreementVersionAlreadyExists = 23136,
	[OriginalName("AGREEMENT_VERSION_NOT_IN_STATUS_DRAFT_CAN_NOT_BE_UPDATED")]
	AgreementVersionNotInStatusDraftCanNotBeUpdated = 23137,
	[OriginalName("LOCALIZATION_FOR_TITLE_NOT_FOUND")]
	LocalizationForTitleNotFound = 23200,
	[OriginalName("LOCALIZATION_STRING_ID_NOT_FOUND")]
	LocalizationStringIdNotFound = 23201,
	[OriginalName("LOCALIZATION_UPDATE_FAILED")]
	LocalizationUpdateFailed = 23202,
	[OriginalName("LOCALIZATION_STRING_ID_ALREADY_EXIST")]
	LocalizationStringIdAlreadyExist = 23204,
	[OriginalName("LOCALIZATION_NOT_SUPPORTED_FILE_TYPE")]
	LocalizationNotSupportedFileType = 23205,
	[OriginalName("LOCALIZATION_TRANSLATION_NOT_FOUND")]
	LocalizationTranslationNotFound = 23206,
	[OriginalName("LOCALIZATION_INCORRECT_TITLE_ID_IN_THE_FILE")]
	LocalizationIncorrectTitleIdInTheFile = 23207,
	[OriginalName("LOCALIZATION_INVALID_FILE_FORMAT")]
	LocalizationInvalidFileFormat = 23208,
	[OriginalName("LOCALIZATION_INVALID_STRING_ID_IN_THE_FILE")]
	LocalizationInvalidStringIdInTheFile = 23209,
	[OriginalName("LOCALIZATION_INVALID_LOCALE_CODE_IN_THE_FILE")]
	LocalizationInvalidLocaleCodeInTheFile = 23210,
	[OriginalName("LOCALIZATION_INVALID_STRING_ID")]
	LocalizationInvalidStringId = 23211,
	[OriginalName("MAILING_TOPIC_DOESNT_EXIST")]
	MailingTopicDoesntExist = 23250,
	[OriginalName("MAILING_TOPIC_ALREADY_EXISTS")]
	MailingTopicAlreadyExists = 23251,
	[OriginalName("MAILING_INVALID_NAME")]
	MailingInvalidName = 23252,
	[OriginalName("MAILING_INVALID_EVENT")]
	MailingInvalidEvent = 23253,
	[OriginalName("MAILING_TEMPLATE_PARSE_ERROR")]
	MailingTemplateParseError = 23254,
	[OriginalName("MAILING_IMAGE_BAD_DATA")]
	MailingImageBadData = 23255,
	[OriginalName("MAILING_IMAGE_NOT_FOUND")]
	MailingImageNotFound = 23256,
	[OriginalName("CROSS_SAVE_TRANSFER_NOT_VALID")]
	CrossSaveTransferNotValid = 23301,
	[OriginalName("CROSS_SAVE_SNAPSHOT_MAX_SIZE_EXCEEDED")]
	CrossSaveSnapshotMaxSizeExceeded = 23302,
	[OriginalName("CROSS_SAVE_NEXT_TRANSFER_NOT_AVAILABLE")]
	CrossSaveNextTransferNotAvailable = 23303,
	[OriginalName("CROSS_SAVE_PLATFORM_NOT_LINKED")]
	CrossSavePlatformNotLinked = 23304,
	[OriginalName("CROSS_SAVE_SNAPSHOT_NOT_FOUND")]
	CrossSaveSnapshotNotFound = 23305,
	[OriginalName("CROSS_SAVE_TARGET_PLATFORM_NOT_AVAILABLE")]
	CrossSaveTargetPlatformNotAvailable = 23306,
	[OriginalName("CROSS_SAVE_FEATURE_IS_DISABLED")]
	CrossSaveFeatureIsDisabled = 23307,
	[OriginalName("CROSS_SAVE_ACCOUNT_UPDATE_FAIL")]
	CrossSaveAccountUpdateFail = 23308,
	[OriginalName("CROSS_SAVE_TRANSFER_TARGET_PLATFORM_UNAVAILABLE")]
	CrossSaveTransferTargetPlatformUnavailable = 23309,
	[OriginalName("CROSS_SAVE_BACKUP_SNAPSHOT_CANT_BE_NULL")]
	CrossSaveBackupSnapshotCantBeNull = 23310,
	[OriginalName("CROSS_SAVE_SNAPSHOT_DESCRIPTION_SYMBOLS_LIMIT_EXCEEDED")]
	CrossSaveSnapshotDescriptionSymbolsLimitExceeded = 23311,
	[OriginalName("CROSS_SAVE_SNAPSHOT_CONTENT_TYPE_SYMBOLS_LIMIT_EXCEEDED")]
	CrossSaveSnapshotContentTypeSymbolsLimitExceeded = 23312,
	[OriginalName("THIRD_PARTY_ACCOUNTS_COUNT_REQUEST_LIMIT")]
	ThirdPartyAccountsCountRequestLimit = 23500,
	[OriginalName("THIRD_PARTY_ACCOUNT_PAIRWISE_NOT_FOUND")]
	ThirdPartyAccountPairwiseNotFound = 23501,
	[OriginalName("THIRD_PARTY_TITLE_NOT_FOUND")]
	ThirdPartyTitleNotFound = 23502,
	[OriginalName("STATISTICS_WIDGET_DISABLED")]
	StatisticsWidgetDisabled = 23600,
	[OriginalName("MOD_UNKNOWN_TAG")]
	ModUnknownTag = 23700,
	[OriginalName("MOD_UNKNOWN_TYPE")]
	ModUnknownType = 23701,
	[OriginalName("MOD_DOES_NOT_EXIST")]
	ModDoesNotExist = 23702,
	[OriginalName("MOD_REQUESTED_USER_IS_NOT_MOD_AUTHOR")]
	ModRequestedUserIsNotModAuthor = 23703,
	[OriginalName("MOD_VERSION_DOES_NOT_EXIST")]
	ModVersionDoesNotExist = 23704,
	[OriginalName("MOD_VERSION_MUST_BE_GREATER_THAN_LATEST")]
	ModVersionMustBeGreaterThanLatest = 23705,
	[OriginalName("MOD_REQUESTED_USER_IS_NOT_MOD_AUTHOR_OR_MODERATOR")]
	ModRequestedUserIsNotModAuthorOrModerator = 23706,
	[OriginalName("MOD_STATUS_IS_NOT_PUBLIC")]
	ModStatusIsNotPublic = 23707,
	[OriginalName("MOD_HAVE_NO_PUBLIC_VERSION")]
	ModHaveNoPublicVersion = 23708,
	[OriginalName("MOD_IS_BLOCKED")]
	ModIsBlocked = 23709,
	[OriginalName("MOD_VERSION_IS_NOT_READY_FOR_TEST")]
	ModVersionIsNotReadyForTest = 23710,
	[OriginalName("MOD_VERSION_TRANSITION_IS_NOT_READY_FOR_TEST")]
	ModVersionTransitionIsNotReadyForTest = 23711,
	[OriginalName("MOD_VERSION_TRANSITION_IS_NOT_FOR_MODERATION_RESULT")]
	ModVersionTransitionIsNotForModerationResult = 23712,
	[OriginalName("MOD_VERSION_IS_NOT_APPROVED")]
	ModVersionIsNotApproved = 23713,
	[OriginalName("MOD_VERSION_IS_NOT_PUBLIC")]
	ModVersionIsNotPublic = 23714,
	[OriginalName("MOD_TRANSITION_IS_NOT_PUBLIC")]
	ModTransitionIsNotPublic = 23715,
	[OriginalName("MOD_REQUESTED_USER_IS_NOT_MODERATOR")]
	ModRequestedUserIsNotModerator = 23716,
	[OriginalName("MOD_VERSION_ALREADY_EXISTS")]
	ModVersionAlreadyExists = 23717,
	[OriginalName("MOD_DOCUMENT_STORAGE_OPERATION_FAILED")]
	ModDocumentStorageOperationFailed = 23718,
	[OriginalName("MOD_INVALID_CONTINUATION_TOKEN")]
	ModInvalidContinuationToken = 23719,
	[OriginalName("MOD_NOT_SUPPORTED_PLATFORM")]
	ModNotSupportedPlatform = 23720,
	[OriginalName("MOD_VERSION_INFO_CAN_NOT_BE_CHANGED")]
	ModVersionInfoCanNotBeChanged = 23721,
	[OriginalName("MOD_INFO_CAN_NOT_BE_CHANGED")]
	ModInfoCanNotBeChanged = 23722,
	[OriginalName("MOD_IS_DELETED")]
	ModIsDeleted = 23723,
	[OriginalName("MOD_VERSION_IS_DELETED")]
	ModVersionIsDeleted = 23724,
	[OriginalName("MOD_ACCOUNT_ID_REQUIRED")]
	ModAccountIdRequired = 23725,
	[OriginalName("MOD_WRONG_TITLE_ID")]
	ModWrongTitleId = 23726,
	[OriginalName("MOD_FILE_DOES_NOT_EXIST")]
	ModFileDoesNotExist = 23727,
	[OriginalName("MOD_NAME_IS_NOT_UNIQUE")]
	ModNameIsNotUnique = 23728
}
