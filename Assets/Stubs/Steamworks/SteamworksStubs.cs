// Stub implementations for Steamworks.NET library.
// These stubs exist solely so the Roslyn analyzer project compiles.
// THESE TYPES MUST NOT BE USED IN RUNTIME CODE.

using System;

namespace Steamworks
{
    public struct CSteamID : IEquatable<CSteamID>
    {
        public static readonly CSteamID Nil = default;
        public ulong m_SteamID;
        public CSteamID(ulong id) { m_SteamID = id; }
        public bool IsValid() => false;
        public uint GetAccountID() => 0;
        public bool Equals(CSteamID other) => false;
        public override bool Equals(object obj) => false;
        public override int GetHashCode() => 0;
        public static bool operator ==(CSteamID a, CSteamID b) => false;
        public static bool operator !=(CSteamID a, CSteamID b) => true;
    }

    public struct CGameID : IEquatable<CGameID>
    {
        public CGameID(ulong id) { }
        public uint AppID() => 0;
        public bool Equals(CGameID other) => false;
        public override bool Equals(object obj) => false;
        public override int GetHashCode() => 0;
        public static explicit operator ulong(CGameID id) => 0;
        public static explicit operator CGameID(ulong id) => default;
    }

    public struct HAuthTicket
    {
        public static readonly HAuthTicket Invalid = default;
        public static bool operator ==(HAuthTicket a, HAuthTicket b) => false;
        public static bool operator !=(HAuthTicket a, HAuthTicket b) => true;
        public override bool Equals(object obj) => false;
        public override int GetHashCode() => 0;
    }
    public struct SteamAPICall_t { public static readonly SteamAPICall_t Invalid = default; }
    public struct SteamLeaderboard_t
    {
        public ulong m_SteamLeaderboard;
        public static bool operator ==(SteamLeaderboard_t a, SteamLeaderboard_t b) => a.m_SteamLeaderboard == b.m_SteamLeaderboard;
        public static bool operator !=(SteamLeaderboard_t a, SteamLeaderboard_t b) => a.m_SteamLeaderboard != b.m_SteamLeaderboard;
        public override bool Equals(object obj) => false;
        public override int GetHashCode() => 0;
    }
    public struct SteamLeaderboardEntries_t { }
    public struct UGCHandle_t
    {
        public static readonly UGCHandle_t Invalid = default;
        public ulong m_UGCHandle;
        public UGCHandle_t(ulong handle) { m_UGCHandle = handle; }
        public static bool operator ==(UGCHandle_t a, UGCHandle_t b) => a.m_UGCHandle == b.m_UGCHandle;
        public static bool operator !=(UGCHandle_t a, UGCHandle_t b) => a.m_UGCHandle != b.m_UGCHandle;
        public override bool Equals(object obj) => false;
        public override int GetHashCode() => 0;
    }
    public struct UGCQueryHandle_t { public static readonly UGCQueryHandle_t Invalid = default; }
    public struct PublishedFileId_t
    {
        public static readonly PublishedFileId_t Invalid = default;
        public ulong m_PublishedFileId;
        public PublishedFileId_t(ulong id) { m_PublishedFileId = id; }
        public static bool operator ==(PublishedFileId_t a, PublishedFileId_t b) => a.m_PublishedFileId == b.m_PublishedFileId;
        public static bool operator !=(PublishedFileId_t a, PublishedFileId_t b) => a.m_PublishedFileId != b.m_PublishedFileId;
        public override bool Equals(object obj) => false;
        public override int GetHashCode() => 0;
    }
    public struct AppId_t
    {
        public static readonly AppId_t Invalid = default;
        public uint m_AppId;
        public AppId_t(uint appId) { m_AppId = appId; }
    }

    public enum EPersonaState { Offline, Online, Busy, Away, Snooze, LookingToTrade, LookingToPlay, Max }
    public enum EWorkshopEnumerationType
    {
        RankedByVote = 0,
        Recent = 1,
        Trending = 2,
        FavoritesOfFriends = 3,
        VotedByFriends = 4,
        ContentByFriends = 5,
        RecentFromFollowedUsers = 6,
        // k_ prefixed aliases used by legacy Steamworks.NET v1.x code
        k_EWorkshopEnumerationTypeRankedByVote = RankedByVote,
        k_EWorkshopEnumerationTypeRecent = Recent,
        k_EWorkshopEnumerationTypeTrending = Trending,
        k_EWorkshopEnumerationTypeFavoritesOfFriends = FavoritesOfFriends,
        k_EWorkshopEnumerationTypeVotedByFriends = VotedByFriends,
        k_EWorkshopEnumerationTypeContentByFriends = ContentByFriends,
        k_EWorkshopEnumerationTypeRecentFromFollowedUsers = RecentFromFollowedUsers,
    }

    // Callback result structs
    public struct PersonaStateChange_t { public ulong m_ulSteamID; public int m_nChangeFlags; }
    public struct GameOverlayActivated_t { public byte m_bActive; }
    public struct GameConnectedFriendChatMsg_t { public CSteamID m_steamIDUser; public int m_iMessageID; }
    public struct UserStatsReceived_t { public ulong m_nGameID; public EResult m_eResult; public CSteamID m_steamIDUser; }
    public struct UserStatsStored_t { public ulong m_nGameID; public EResult m_eResult; }
    public struct UserAchievementStored_t { public ulong m_nGameID; public bool m_bGroupAchievement; public string m_rgchAchievementName; public uint m_nCurProgress; public uint m_nMaxProgress; }
    public struct LeaderboardFindResult_t { public SteamLeaderboard_t m_hSteamLeaderboard; public byte m_bLeaderboardFound; }
    public struct LeaderboardScoresDownloaded_t
    {
        public SteamLeaderboard_t m_hSteamLeaderboard;
        public SteamLeaderboardEntries_t m_hSteamLeaderboardEntries;
        public int m_cEntryCount;
    }
    public struct LeaderboardEntry_t
    {
        public CSteamID m_steamIDUser;
        public int m_nGlobalRank;
        public int m_nScore;
        public int m_cDetails;
        public UGCHandle_t m_hUGC;
    }
    public struct LeaderboardScoreUploaded_t { public byte m_bSuccess; public SteamLeaderboard_t m_hSteamLeaderboard; public int m_nScore; public byte m_bScoreChanged; public int m_nGlobalRankNew; public int m_nGlobalRankPrevious; }
    public struct SteamUGCQueryCompleted_t { public UGCQueryHandle_t m_handle; public EResult m_eResult; public uint m_unNumResultsReturned; public uint m_unTotalMatchingResults; }
    public struct SteamUGCDetails_t
    {
        public PublishedFileId_t m_nPublishedFileId;
        public EResult m_eResult;
        public int m_eFileType;
        public AppId_t m_nCreatorAppID;
        public AppId_t m_nConsumerAppID;
        public string m_rgchTitle;
        public string m_rgchDescription;
        public UGCHandle_t m_hFile;
        public UGCHandle_t m_hPreviewFile;
        public ulong m_ulSteamIDOwner;
        public uint m_rtimeCreated;
        public uint m_rtimeUpdated;
        public int m_eVisibility;
        public bool m_bBanned;
        public string m_rgchTags;
        public bool m_bTagsTruncated;
        public string m_pchFileName;
        public int m_nFileSize;
        public int m_nPreviewFileSize;
        public string m_rgchURL;
        public uint m_unVotesUp;
        public uint m_unVotesDown;
        public float m_flScore;
        public uint m_unNumChildren;
    }
    public struct RemoteStoragePublishFileResult_t { public EResult m_eResult; public PublishedFileId_t m_nPublishedFileId; }
    public struct RemoteStorageUpdateUserPublishedItemVoteResult_t { public EResult m_eResult; public PublishedFileId_t m_nPublishedFileId; }
    public struct RemoteStorageGetPublishedItemVoteDetailsResult_t
    {
        public EResult m_eResult;
        public PublishedFileId_t m_unPublishedFileId;
        public int m_nVotesFor;
        public int m_nVotesAgainst;
        public int m_nReports;
        public float m_fScore;
    }
    public struct RemoteStorageEnumerateUserSharedWorkshopFilesResult_t
    {
        public EResult m_eResult;
        public int m_nResultsReturned;
        public int m_nTotalResultCount;
        public PublishedFileId_t[] m_rgPublishedFileId;
    }
    public struct RemoteStorageEnumerateWorkshopFilesResult_t
    {
        public EResult m_eResult;
        public int m_nResultsReturned;
        public int m_nTotalResultCount;
        // m_rgPublishedFileId: array of file IDs in the result set.
        public PublishedFileId_t[] m_rgPublishedFileId;
    }
    public struct RemoteStorageFileShareResult_t { public EResult m_eResult; public UGCHandle_t m_hFile; }
    public struct RemoteStorageGetPublishedFileDetailsResult_t
    {
        public EResult m_eResult;
        public PublishedFileId_t m_nPublishedFileId;
        public string m_rgchTitle;
        public string m_rgchDescription;
        public string m_rgchTags;
        public string m_pchFileName;
        public UGCHandle_t m_hFile;
        public UGCHandle_t m_hPreviewFile;
    }
    public struct RemoteStorageDownloadUGCResult_t { public EResult m_eResult; public UGCHandle_t m_hFile; public ulong m_nAppID; public int m_nSizeInBytes; public string m_pchFileName; public ulong m_ulSteamIDOwner; }

    public delegate void SteamAPIWarningMessageHook_t(int nSeverity, System.Text.StringBuilder pchDebugText);

    public class Callback<T>
    {
        public delegate void DispatchDelegate(T param);
        public Callback(DispatchDelegate func) { }
        public static Callback<T> Create(DispatchDelegate func) => new Callback<T>(func);
        public void Unregister() { }
    }

    public class CallResult<T>
    {
        public delegate void APIDispatchDelegate(T param, bool bIOFailure);
        public CallResult() { }
        public CallResult(APIDispatchDelegate func) { }
        public static CallResult<T> Create(APIDispatchDelegate func) => new CallResult<T>(func);
        public void Set(SteamAPICall_t hAPICall, APIDispatchDelegate func = null) { }
        public bool IsActive() => false;
        public void Cancel() { }
        public void Unregister() { }
    }

    // Packsize and DllCheck are utility classes in Steamworks.NET for self-validation.
    public static class Packsize
    {
        public static bool Test() => true;
    }

    public static class DllCheck
    {
        public static bool Test() => true;
    }

    public static class SteamAPI
    {
        public static bool Init() => false;
        public static void Shutdown() { }
        public static bool IsSteamRunning() => false;
        public static void RunCallbacks() { }
        public static bool RestartAppIfNecessary(uint unOwnAppID) => false;
        public static bool RestartAppIfNecessary(AppId_t unOwnAppID) => false;
        public static void SetWarningMessageHook(SteamAPIWarningMessageHook_t func) { }
    }

    public static class SteamUser
    {
        public static CSteamID GetSteamID() => default;
        public static HAuthTicket GetAuthSessionTicket(byte[] pTicket, int cbMaxTicket, out uint pcbTicket) { pcbTicket = 0; return default; }
        public static void CancelAuthTicket(HAuthTicket hAuthTicket) { }
        public static EPersonaState GetUserSteamLevel() => EPersonaState.Offline;
        public static bool BLoggedOn() => false;
    }

    public enum EFriendFlags
    {
        None = 0,
        Blocked = 1,
        FriendshipRequested = 2,
        Immediate = 4,
        ClanMember = 8,
        OnGameServer = 16,
        RequestingFriendship = 128,
        RequestingInfo = 256,
        Ignored = 512,
        IgnoredFriend = 1024,
        Suggested = 2048,
        All = 65535,
        k_EFriendFlagImmediate = Immediate,
        k_EFriendFlagIgnoredFriend = IgnoredFriend,
    }

    public static class SteamFriends
    {
        public static string GetPersonaName() => string.Empty;
        public static int GetFriendCount(int iFriendFlags) => 0;
        public static int GetFriendCount(EFriendFlags iFriendFlags) => 0;
        public static CSteamID GetFriendByIndex(int iFriend, int iFriendFlags) => default;
        public static CSteamID GetFriendByIndex(int iFriend, EFriendFlags iFriendFlags) => default;
        public static string GetFriendPersonaName(CSteamID steamIDFriend) => string.Empty;
        public static EPersonaState GetFriendPersonaState(CSteamID steamIDFriend) => EPersonaState.Offline;
        public static bool GetFriendGamePlayed(CSteamID steamIDFriend, out FriendGameInfo_t pFriendGameInfo) { pFriendGameInfo = default; return false; }
        public static int GetFriendMessage(CSteamID steamIDFriend, int iMessageID, out string pvData, int cubData, out EChatEntryType peChatEntryType) { pvData = string.Empty; peChatEntryType = EChatEntryType.k_EChatEntryTypeChatMsg; return 0; }
        public static bool ReplyToFriendMessage(CSteamID steamIDFriend, string pchMsgToSend) => false;
        public static void ActivateGameOverlay(string pchDialog) { }
        public static void ActivateGameOverlayToUser(string pchDialog, CSteamID steamID) { }
        public static int GetSmallFriendAvatar(CSteamID steamIDFriend) => 0;
        public static bool SetListenForFriendsMessages(bool bInterceptEnabled) => false;
        public static bool InviteUserToGame(CSteamID steamIDFriend, string pchConnectString) => false;
    }

    public enum EChatEntryType { k_EChatEntryTypeInvalid = 0, k_EChatEntryTypeChatMsg = 1 }

    public struct FriendGameInfo_t
    {
        public CGameID m_gameID;
        public uint m_unGameIP;
        public ushort m_usGamePort;
        public ushort m_usQueryPort;
        public CSteamID m_steamIDLobby;
    }

    public static class SteamUserStats
    {
        public static bool RequestCurrentStats() => false;
        public static bool GetAchievement(string pchName, out bool pbAchieved) { pbAchieved = false; return false; }
        public static bool SetAchievement(string pchName) => false;
        public static bool ClearAchievement(string pchName) => false;
        public static bool StoreStats() => false;
        public static bool GetStat(string pchName, out int pData) { pData = 0; return false; }
        public static bool GetStat(string pchName, out float pData) { pData = 0f; return false; }
        public static bool SetStat(string pchName, int nData) => false;
        public static bool SetStat(string pchName, float fData) => false;
        public static SteamAPICall_t FindLeaderboard(string pchLeaderboardName) => default;
        public static SteamAPICall_t DownloadLeaderboardEntries(SteamLeaderboard_t hSteamLeaderboard, ELeaderboardDataRequest eLeaderboardDataRequest, int nRangeStart, int nRangeEnd) => default;
        public static SteamAPICall_t UploadLeaderboardScore(SteamLeaderboard_t hSteamLeaderboard, ELeaderboardUploadScoreMethod eLeaderboardUploadScoreMethod, int nScore, int[] pScoreDetails, int cScoreDetailsCount) => default;
        public static bool GetDownloadedLeaderboardEntry(SteamLeaderboardEntries_t hSteamLeaderboardEntries, int index, out LeaderboardEntry_t pLeaderboardEntry, int[] pDetails, int cDetailsMax) { pLeaderboardEntry = default; return false; }
        public static string GetAchievementDisplayAttribute(string pchName, string pchKey) => string.Empty;
    }

    public enum ELeaderboardDataRequest
    {
        Global,
        GlobalAroundUser,
        Friends,
        Users,
        k_ELeaderboardDataRequestGlobal = Global,
        k_ELeaderboardDataRequestGlobalAroundUser = GlobalAroundUser,
        k_ELeaderboardDataRequestFriends = Friends,
        k_ELeaderboardDataRequestUsers = Users
    }
    public enum ELeaderboardUploadScoreMethod
    {
        None,
        KeepBest,
        ForceUpdate,
        k_ELeaderboardUploadScoreMethodNone = None,
        k_ELeaderboardUploadScoreMethodKeepBest = KeepBest,
        k_ELeaderboardUploadScoreMethodForceUpdate = ForceUpdate
    }

    public static class SteamRemoteStorage
    {
        public static bool FileWrite(string pchFile, byte[] pvData, int cubData) => false;
        public static int FileRead(string pchFile, byte[] pvData, int cubDataToRead) => 0;
        public static bool FileExists(string pchFile) => false;
        public static bool FileDelete(string pchFile) => false;
        public static bool DeletePublishedFile(PublishedFileId_t unPublishedFileId) => false;
        public static SteamAPICall_t FileShare(string pchFile) => default;
        public static SteamAPICall_t PublishWorkshopFile(string pchFile, string pchPreviewFile, uint nConsumerAppId, string pchTitle, string pchDescription, ERemoteStoragePublishedFileVisibility eVisibility, System.Collections.Generic.SortedDictionary<string, string> pTags, EWorkshopFileType eWorkshopFileType) => default;
        // SteamFileItem.cs passes string[] for tags.
        public static SteamAPICall_t PublishWorkshopFile(string pchFile, string pchPreviewFile, uint nConsumerAppId, string pchTitle, string pchDescription, ERemoteStoragePublishedFileVisibility eVisibility, string[] pTags, EWorkshopFileType eWorkshopFileType) => default;
        public static SteamAPICall_t EnumerateUserSharedWorkshopFiles(CSteamID steamId, uint unStartIndex, System.Collections.Generic.SortedDictionary<string, string> pRequiredTags, System.Collections.Generic.SortedDictionary<string, string> pExcludedTags) => default;
        // Overload accepting string[] as used by legacy game code.
        public static SteamAPICall_t EnumerateUserSharedWorkshopFiles(CSteamID steamId, uint unStartIndex, string[] pRequiredTags, string[] pExcludedTags) => default;
        public static SteamAPICall_t EnumerateWorkshopFiles(EWorkshopEnumerationType eEnumerationType, uint unStartIndex, uint unCount, uint unDays, System.Collections.Generic.SortedDictionary<string, string> pTags, System.Collections.Generic.SortedDictionary<string, string> pUserTags) => default;
        // Overload accepting string[] as used by legacy game code.
        public static SteamAPICall_t EnumerateWorkshopFiles(EWorkshopEnumerationType eEnumerationType, uint unStartIndex, uint unCount, uint unDays, string[] pTags, string[] pUserTags) => default;
        public static SteamAPICall_t GetPublishedFileDetails(PublishedFileId_t unPublishedFileId, uint unMaxSecondsOld) => default;
        public static SteamAPICall_t UGCDownload(UGCHandle_t hContent, uint unPriority) => default;
        public static int UGCRead(UGCHandle_t hContent, byte[] pvData, int cubDataToRead, uint cOffset, EUGCReadAction eAction) => 0;
        public static bool GetUGCDownloadProgress(UGCHandle_t hContent, out int pnBytesDownloaded, out int pnBytesExpected) { pnBytesDownloaded = 0; pnBytesExpected = 0; return false; }
        public static SteamAPICall_t GetPublishedItemVoteDetails(PublishedFileId_t unPublishedFileId) => default;
        public static SteamAPICall_t UpdateUserPublishedItemVote(PublishedFileId_t unPublishedFileId, bool bVoteUp) => default;
        public static int GetFileCount() => 0;
        public static string GetFileNameAndSize(int iFile, out int pnFileSizeInBytes) { pnFileSizeInBytes = 0; return string.Empty; }
        public static bool IsCloudEnabledForAccount() => false;
        public static bool IsCloudEnabledForApp() => false;
        public static SteamAPICall_t EnumeratePublishedWorkshopFiles(EWorkshopEnumerationType eEnumerationType, uint unStartIndex, uint unCount, uint unDays, System.Collections.Generic.SortedDictionary<string, string> pTags, System.Collections.Generic.SortedDictionary<string, string> pUserTags) => default;
        // SteamGetPreFileListProcess.cs and SteamRandomGetIsoProcess.cs pass string[] for tags.
        public static SteamAPICall_t EnumeratePublishedWorkshopFiles(EWorkshopEnumerationType eEnumerationType, uint unStartIndex, uint unCount, uint unDays, string[] pTags, System.Collections.Generic.SortedDictionary<string, string> pUserTags) => default;
    }

    public enum ERemoteStoragePublishedFileVisibility
    {
        Public,
        FriendsOnly,
        Private,
        k_ERemoteStoragePublishedFileVisibilityPublic = Public,
        k_ERemoteStoragePublishedFileVisibilityFriendsOnly = FriendsOnly,
        k_ERemoteStoragePublishedFileVisibilityPrivate = Private,
    }

    public static class SteamUGC
    {
        public static UGCQueryHandle_t CreateQueryAllUGCRequest(EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, uint nCreatorAppID, uint nConsumerAppID, uint unPage) => default;
        public static UGCQueryHandle_t CreateQueryUserUGCRequest(uint unAccountID, EUserUGCList eListType, EUGCMatchingUGCType eMatchingUGCType, EUserUGCListSortOrder eSortOrder, AppId_t nCreatorAppID, AppId_t nConsumerAppID, uint unPage) => default;
        // SteamSearchMyProcess.cs passes uint directly for creator/consumer app IDs.
        public static UGCQueryHandle_t CreateQueryUserUGCRequest(uint unAccountID, EUserUGCList eListType, EUGCMatchingUGCType eMatchingUGCType, EUserUGCListSortOrder eSortOrder, uint nCreatorAppID, uint nConsumerAppID, uint unPage) => default;
        public static SteamAPICall_t SendQueryUGCRequest(UGCQueryHandle_t handle) => default;
        public static bool ReleaseQueryUGCRequest(UGCQueryHandle_t handle) => false;
        public static bool GetQueryUGCResult(UGCQueryHandle_t handle, uint index, out SteamUGCDetails_t pDetails) { pDetails = default; return false; }
        public static bool AddRequiredTag(UGCQueryHandle_t handle, string pTagName) => false;
        public static bool SetSearchText(UGCQueryHandle_t handle, string pSearchText) => false;
        public static bool SetCloudFileNameFilter(UGCQueryHandle_t handle, string pMatchCloudFileName) => false;
    }

    public enum EUGCQuery
    {
        RankedByVote,
        RankedByPublicationDate,
        AcceptedForGameRankedByAcceptanceDate,
        RankedByTrend,
        FavoritedByFriendsRankedByPublicationDate,
        CreatedByFriendsRankedByPublicationDate,
        RankedByNumTimesReported,
        CreatedByFollowedUsersRankedByPublicationDate,
        NotYetRated,
        RankedByTotalVotesAsc,
        RankedByVotesUp,
        RankedByTextSearch,
        RankedByTotalUniqueSubscriptions,
        RankedByPlaytimeTrend,
        RankedByTotalPlaytime,
        RankedByAveragePlaytimeTrend,
        RankedByLifetimeAveragePlaytime,
        RankedByPlaytimeSessionsTrend,
        RankedByLifetimePlaytimeSessions,
        k_EUGCQuery_RankedByVote = RankedByVote,
    }
    public enum EUGCMatchingUGCType
    {
        Items,
        Items_Mtx,
        Items_ReadyToUse,
        Collections,
        Artwork,
        Videos,
        Screenshots,
        AllGuides,
        WebGuides,
        IntegratedGuides,
        UsableInGame,
        ControllerBindings,
        GameManagedItems,
        All,
        k_EUGCMatchingUGCType_Items = Items,
    }
    public enum EUserUGCList
    {
        Published, VotedOn, VotedUp, VotedDown, WillVoteLater, Favorited, Subscribed, UsedOrPlayed, Followed,
        k_EUserUGCList_Published = Published,
    }
    public enum EUserUGCListSortOrder
    {
        CreationOrderDesc, CreationOrderAsc, TitleAsc, LastUpdatedDesc, SubscriptionDateDesc, VoteScoreDesc, ForModeration,
        k_EUserUGCListSortOrder_VoteScoreDesc = VoteScoreDesc,
    }
    public enum EWorkshopFileType
    {
        Community,
        Microtransaction,
        Collection,
        Art,
        Video,
        Screenshot,
        Game,
        Software,
        Concept,
        WebGuide,
        IntegratedGuide,
        Merch,
        ControllerBinding,
        SteamworksAccessInvite,
        SteamVideo,
        GameManagedItem,
        Max,
        k_EWorkshopFileTypeCommunity = Community,
    }

    // EResult is used both as enum and compared to int by legacy game code.
    // We use int-based enum so implicit int comparisons work.
    public enum EResult
    {
        None = 0,
        OK = 1,
        k_EResultOK = OK,
        k_EResultFail = Fail,
        k_EResultInvalidParam = InvalidParam,
        k_EResultFileNotFound = FileNotFound,
        k_EResultTimeout = Timeout,
        k_EResultInsufficientPrivilege = InsufficientPrivilege,
        Fail = 2,
        NoConnection = 3,
        InvalidPassword = 5,
        LoggedInElsewhere = 6,
        InvalidProtocolVer = 7,
        InvalidParam = 8,
        FileNotFound = 9,
        Busy = 10,
        InvalidState = 11,
        InvalidName = 12,
        InvalidEmail = 13,
        DuplicateName = 14,
        AccessDenied = 15,
        Timeout = 16,
        Banned = 17,
        AccountNotFound = 18,
        InvalidSteamID = 19,
        ServiceUnavailable = 20,
        NotLoggedOn = 21,
        Pending = 22,
        EncryptionFailure = 23,
        InsufficientPrivilege = 24,
        LimitExceeded = 25,
        Revoked = 26,
        Expired = 27,
        AlreadyRedeemed = 28,
        DuplicateRequest = 29,
        AlreadyOwned = 30,
        IPNotFound = 31,
        PersistFailed = 32,
        LockingFailed = 33,
        LogonSessionReplaced = 34,
        ConnectFailed = 35,
        HandshakeFailed = 36,
        IOFailure = 37,
        RemoteDisconnect = 38,
        ShoppingCartNotFound = 39,
        Blocked = 40,
        Ignored = 41,
        NoMatch = 42,
        AccountDisabled = 43,
        ServiceReadOnly = 44,
        AccountNotFeatured = 45,
        AdministratorOK = 46,
        ContentVersion = 47,
        TryAnotherCM = 48,
        PasswordRequiredToKickSession = 49,
        AlreadyLoggedInElsewhere = 50,
        Suspended = 51,
        Cancelled = 52,
        DataCorruption = 53,
        DiskFull = 54,
        RemoteCallFailed = 55,
        PasswordUnset = 56,
        ExternalAccountUnlinked = 57,
        PSNTicketInvalid = 58,
        ExternalAccountAlreadyLinked = 59,
        RemoteFileConflict = 60,
        IllegalPassword = 61,
        SameAsPreviousValue = 62,
        AccountLogonDenied = 63,
        CannotUseOldPassword = 64,
        InvalidLoginAuthCode = 65,
        AccountLogonDeniedNoMail = 66,
        HardwareNotCapableOfIPT = 67,
        IPTInitError = 68,
        ParentalControlRestricted = 69,
        FacebookQueryError = 70,
        ExpiredLoginAuthCode = 71,
        IPLoginRestrictionFailed = 72,
        AccountLockedDown = 73,
        AccountLogonDeniedVerifiedEmailRequired = 74,
        NoMatchingURL = 75,
    }

    public static class SteamUtils
    {
        public static uint GetAppID() => 0;
        public static bool IsOverlayEnabled() => false;
        public static bool IsSteamRunningInVR() => false;
        public static uint GetCurrentBatteryPower() => 0;
        public static bool GetImageSize(int iImage, out uint pnWidth, out uint pnHeight) { pnWidth = 0; pnHeight = 0; return false; }
        public static bool GetImageRGBA(int iImage, byte[] pubDest, int nDestBufferSize) => false;
    }

    public static class SteamClient
    {
        public static bool Init(uint uAppID) => false;
        public static void Shutdown() { }
        public static void SetWarningMessageHook(SteamAPIWarningMessageHook_t func) { }
    }

    public static class SteamAppList
    {
        public static uint GetNumInstalledApps() => 0;
        public static uint GetInstalledApps(AppId_t[] pvecAppID, uint unMaxAppIDs) => 0;
        public static int GetAppName(AppId_t nAppID, System.Text.StringBuilder pchName, int cchNameMax) => 0;
        public static int GetAppInstallDir(AppId_t nAppID, System.Text.StringBuilder pchDirectory, int cchNameMax) => 0;
    }

    public enum EUGCReadAction
    {
        ContinueReading,
        ContinueReadingUntilFinished,
        Close,
        k_EUGCRead_ContinueReading = ContinueReading,
        k_EUGCRead_ContinueReadingUntilFinished = ContinueReadingUntilFinished,
        k_EUGCRead_Close = Close,
    }
}
