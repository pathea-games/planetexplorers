using UnityEngine;
using System.Xml;
using System.IO;
using Steamworks;
using System.Collections.Generic;

public enum Eachievement :int
{
    mission1,
    mission2,
    mission3,
    mission4,
    mission5,
    mission6,
    mission7,
    mission8,
    mission9,
    Richer, //富可敌国
    Mounts_Rider,//怪物骑上
    Eleven,//同生共死
    Shit,//屎壳郎
    ALL,  
    Test,
    mission10,
    mission11,
    Max
}

public enum Estat
{
    None,
    stat1
}

/*在游戏会话开始时，调用 SteamUserStats()->RequestCurrentStats()，从 Steam 后端获取用户的数据。数据准备就绪后，您将收到 UserStatsReceived_t 回调。
使用 SteamUserStats()->GetStat() 和 SteamUserStats()->GetAchievement() 迭代数据并初始化游戏状态。
使用 SteamUserStats()->GetAchievementDisplayAttribute() 获取人类可读的成就属性，包括成就名称 ("name") 和说明 ("desc")。这些属性可在 Steamworks 合作伙伴网站实施本地化，返回的数据取决于用户运行游戏时使用的语言。您还可以使用 SteamUserStats()->GetAchievementIcon() 获得一项成就的图标，或使用 SteamUserStats()->GetAchievementAndUnlockTime() 获得各项成就的解锁时间
统计数据变化时，尤其是向用户显示任何变化前，调用SteamUserStats()->SetStat()或SteamUserStats()->UpdateAvgRateStat()。此调用仅修改 Steam 内存中的状态，非常便捷。这可以让 Steam 在游戏崩溃的情况下依然保留更改。
在游戏内恰当的时间点（如经过检查点、关卡过渡时）调用 SteamUserStats()->StoreStats() 可上传这些更改。完成后，您可以收到 UserStatsStored_t 回调。
对于拥有进度条的成就，在重要时间点使用 SteamUserStats()->IndicateAchievementProgress() 可显示进度弹窗。例如，如果您需要获胜 20 次，那么在获胜 10 次时，您可以调用此函数，向用户表明进度已完成一半。
解锁了一项或多项成就时，针对各项解锁的成就调用 SteamUserStats->SetAchievement()，然后调用 SteamUserStats()->StoreStats() 立即上传它们。针对各项解锁的成就，您的游戏将得到一个 UserStatsStored_t 回调和一个 UserAchievementStored_t 回调。Steam 游戏功能面板可向用户显示一个通知面板。
 * */
// This is a port of StatsAndAchievements.cpp from SpaceWar, the official Steamworks Example.  
public class SteamAchievementsSystem : MonoBehaviour
{
    const int VERSIONS_0 = 0;
    //int CUR_VERSIONS = VERSIONS_0;
#if UNITY_EDITOR
    public Eachievement AchivementType = Eachievement.Max;
    public bool ResetAchivements = false;
    public bool Test = false;
#endif
    private class AchievementInfo
    {
        public Eachievement m_eAchievementID;
        public Estat m_eStatID;
        public string m_apiName;
        public string m_strDescription;
        public bool m_bAchieved;
        public int m_ProtoId;

        /// <summary>  
        /// Creates an Achievement. You must also mirror the data provided here in https://partner.steamgames.com/apps/achievements/yourappid  
        /// </summary>  
        /// <param name="achievement">The "API Name Progress Stat" used to uniquely identify the achievement.</param>  
        /// <param name="name">The "Display Name" that will be shown to players in game and on the Steam Community.</param>  
        /// <param name="desc">The "Description" that will be shown to players in game and on the Steam Community.</param>  
        public AchievementInfo(Eachievement achievementID, Estat statID,string name, string desc,int protoId)
        {
            m_eAchievementID = achievementID;
            m_eStatID = statID;
            m_apiName = name;
            m_strDescription = desc;
            m_bAchieved = false;
            m_ProtoId = protoId;
        }
    }
    public  class MissionAchievement
    {
        public enum Emisschedule :int
        {
            Accomplish,
            Get,
            Max
        }
        public int _missionID;
        public Emisschedule _accomplishOrGet;
        public MissionAchievement(int missionid, Emisschedule accomOrget)
        {
            _missionID = missionid;
            _accomplishOrGet = accomOrget;
        }

        public bool EquitMission(int id, Emisschedule accomOrget)
        {
            return _missionID != 0 ? _missionID == id && _accomplishOrGet == accomOrget : false;
        }
    }
    #region 单例  
    private static SteamAchievementsSystem instance;

    public static SteamAchievementsSystem Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion
    private  AchievementInfo[]  m_Achievements;
    private MissionAchievement[] m_MissionAchs;

    string[] m_StatNames; //EstatID
    private Istat[] m_Stats;//EachievementID

    public List<GameObject> gos;
    // Our GameID  
    private Steamworks.CGameID m_GameID;

    // Did we get the stats from Steam?  
    private bool m_bRequestedStats;
    private bool m_bStatsValid;

    // Should we store stats this frame?  
    private bool m_bStoreStats;
    private Pathea.PeEntity m_Maiplayer = null;
    private Pathea.PlayerPackageCmpt m_playerPkg = null;

    private SteamLeaderboard_t m_steamLeaderboard;
    //成就回调
    protected Callback<UserStatsReceived_t> m_UserStatsReceived;
    protected Callback<UserStatsStored_t> m_UserStatsStored;
    protected Callback<UserAchievementStored_t> m_UserAchievementStored;

    //排名回调
    protected CallResult<LeaderboardFindResult_t> m_LeaderboardFindResult;
    protected CallResult<LeaderboardScoreUploaded_t> m_LeaderboardScoreUploaded;
    protected CallResult<LeaderboardScoresDownloaded_t> m_LeaderboardScoresDownloaded;

    #region mono
    void Awake()
    {
        instance = this;
        InitBaseData();
    }

    void OnEnable()
    {
        if (!SteamManager.Initialized)
            return;

        // Cache the GameID for use in the Callbacks  
        m_GameID = new CGameID(SteamUtils.GetAppID());

        //游戏会话开始后,从Steam后端获取用户的数据：SteamUserStats()->RequestCurrentStats()
        //得到回调：UserStatsReceived_t
        m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        //上传数据后：SteamUserStats()->StoreStats()
        //得到回调：UserStatsStored_t
        m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        //解锁了一项或多项成就时，针对各项解锁的成就调用 SteamUserStats->SetAchievement();
        //回调UserAchievementStored_t
        m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

       // m_LeaderboardFindResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFindResult);
       // m_LeaderboardScoreUploaded = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaerboardScoreUploaded);
       //m_LeaderboardScoresDownloaded = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);
        // These need to be reset to get the stats upon an Assembly reload in the Editor.  
        m_bRequestedStats = false;
        m_bStatsValid = false;
       // SteamAPICall_t handle = SteamUserStats.FindOrCreateLeaderboard("PlayerValue", ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric);
       // m_LeaderboardFindResult.Set(handle);

    }

    private void Update()
    {
        if (!SteamManager.Initialized)
            return;

        if (!m_bRequestedStats)
        {
            // Is Steam Loaded? if no, can't get stats, done  
            if (!SteamManager.Initialized)
            {
                m_bRequestedStats = true;
                return;
            }

            // If yes, request our stats  
            bool bSuccess = SteamUserStats.RequestCurrentStats();

            // This function should only return false if we weren't logged in, and we already checked that.  
            // But handle it being false again anyway, just ask again later.  
            m_bRequestedStats = bSuccess;
        }

        if (!m_bStatsValid)
            return;

#if UNITY_EDITOR
        if (ResetAchivements)
        {
            ResetAllAchievements();
            ResetAllStats();
            ResetAchivements = false;
        }

        if (Test)
        {
            OnGameStateChange(AchivementType);
            Test = false;
        }
#endif

        //获取当前玩家entity
        if (Pathea.PeCreature.Instance != null && Pathea.PeCreature.Instance.mainPlayer != null)
        {
            if(!m_Maiplayer || !m_Maiplayer.Equals(Pathea.PeCreature.Instance.mainPlayer))
            {
                m_Maiplayer = Pathea.PeCreature.Instance.mainPlayer;
                m_playerPkg = (m_Maiplayer.packageCmpt as Pathea.PlayerPackageCmpt);

                if (m_playerPkg != null)
                    m_playerPkg.package._playerPak.changeEventor.Subscribe(OnItemChange);
            }
        }
       

        if(m_Maiplayer && Pathea.Money.Digital)        
        {
            if(m_Maiplayer.packageCmpt != null && m_Maiplayer.packageCmpt.money != null && m_Maiplayer.packageCmpt.money.current >= 10000)
            {
               //完成成就：富可敌国
                OnGameStateChange(Eachievement.Richer);
            }
        }

        // Get info from sources  

        // Evaluate achievements 
        int achedNum = 0; 
        for (int i=0;i<m_Achievements.Length;i++)
        {
            if (m_Achievements[i].m_bAchieved)
            {
                achedNum++;
                continue;
            }

            if (m_Stats[(int)m_Achievements[i].m_eAchievementID].IsAccomplish())
                UnlockAchievement(m_Achievements[i]);

        }

        //全部成就完成
        if(achedNum == ((int)Eachievement.Max -2))
        {
            if(!m_Achievements[(int)Eachievement.ALL].m_bAchieved)
                UnlockAchievement(m_Achievements[(int)Eachievement.ALL]);
        }

        //Store stats by self  
        if (m_bStoreStats)
        {
            //保存更改
            bool bSuccess = false;//SteamUserStats.StoreStats();
            for (int i = 0; i < m_Achievements.Length; i++)
            {
                if (string.IsNullOrEmpty(m_StatNames[(int)m_Achievements[i].m_eStatID])) continue;

                bSuccess = StatsUserPrefs.SaveIntValue(m_StatNames[(int)m_Achievements[i].m_eStatID], m_Stats[(int)m_Achievements[i].m_eAchievementID].m_StatValue);               
            }
            SteamUserStats.StoreStats();
            // If this failed, we never sent anything to the server, try  
            // again later.  
            m_bStoreStats = !bSuccess;
        }
    }

    #endregion

    #region XmlData
    private void InitBaseData()
    {
        LoadAchievement((Resources.Load("Achievement/AchievementData") as TextAsset).text);
    }

    private void LoadAchievement(string str)
    {
        XmlDocument doc = new XmlDocument();
        StringReader reader = new StringReader(str);
        if (doc == null || reader == null) return;

        doc.Load(reader);
        XmlNode root = doc.SelectSingleNode("AchievementData");
        XmlNode achNode =  root.SelectSingleNode("Achievements");
        XmlNode statsNode = root.SelectSingleNode("Stats");

        //Achievements 
        XmlNodeList _AchDataList = ((XmlElement)achNode).GetElementsByTagName("TriggerData");
        XmlNodeList _statDataList = ((XmlElement)statsNode).GetElementsByTagName("Data");

        int _achCount = _AchDataList.Count;
        int _statCount = _statDataList.Count;

        m_Achievements = new AchievementInfo[_achCount];
        m_MissionAchs = new MissionAchievement[_achCount];

        m_StatNames = new string[1 + _statCount];
        m_Stats = new Istat[_achCount];
       
        foreach (XmlNode node in _AchDataList)
        {
            XmlElement xe = node as XmlElement;
            int _achid = PETools.XmlUtil.GetAttributeInt32(xe, "achID");
            int _staid = PETools.XmlUtil.GetAttributeInt32(xe, "staID");                       
            Eachievement _achievementID = (Eachievement)_achid;
            Estat _statID = (Estat)_staid;
            string _name = PETools.XmlUtil.GetAttributeString(xe, "APIname");
            string _desc = PETools.XmlUtil.GetAttributeString(xe, "AchieveDesc");
            int _missionid = PETools.XmlUtil.GetAttributeInt32(xe, "MissionID");
            MissionAchievement.Emisschedule _accomplishOrGet =(MissionAchievement.Emisschedule) PETools.XmlUtil.GetAttributeInt32(xe, "AccomplishOrGet");
            int _protoId = PETools.XmlUtil.GetAttributeInt32(xe, "ItemProtoId");

            m_Achievements[_achid] = (new AchievementInfo(_achievementID, _statID,_name, _desc, _protoId));

            m_StatNames[_staid] = "";
            TriggerStat mstat = new TriggerStat();
            mstat.SetGoal(1);
            m_Stats[_achid] = mstat;

            m_MissionAchs[_achid] = new MissionAchievement(_missionid, _accomplishOrGet);
        }

        //stats  
        foreach (XmlNode node in _statDataList)
        {
            XmlElement xe = node as XmlElement;
            int _id = PETools.XmlUtil.GetAttributeInt32(xe, "ID");
            int _achid = PETools.XmlUtil.GetAttributeInt32(xe, "achID");
            string _name = PETools.XmlUtil.GetAttributeString(xe, "StatName");
            int _goal = PETools.XmlUtil.GetAttributeInt32(xe, "Goal");

            if(m_Stats.Length >_achid && m_Stats[_achid] != null)
                m_Stats[_achid].SetGoal(_goal);

            m_StatNames[_id] = _name;
        }

    }

    #endregion
    //-----------------------------------------------------------------------------  
    // Purpose: Game state has changed  
    //-----------------------------------------------------------------------------  

    #region public fun
    public void OnGameStateChange(Eachievement eNewState,bool isIncrease =true )
    {
        if (!m_bStatsValid)
            return;

        if (m_Stats == null || ((int)eNewState) > m_Stats.Length|| (int)eNewState > m_Achievements.Length || m_Stats[(int)eNewState].IsAccomplish())
            return;

        if (isIncrease)
            m_Stats[(int)eNewState].IncreaseStat();
        else
            m_Stats[(int)eNewState].DropStat();

        // We want to update stats the next frame. 

        m_bStoreStats = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="missionID"></param>
    /// <param name="_Emisschedule" 1:接任务 ；0:交任务></param>
    public void OnMissionChange(int missionID,int _Emisschedule = 0)
    {
        for(int i=0;i<m_MissionAchs.Length;i++)
        {
            if(m_MissionAchs[i].EquitMission(missionID, (MissionAchievement.Emisschedule)_Emisschedule))
            {
                OnGameStateChange((Eachievement)i);
                break;
            }
        }        
    }
    #endregion

    #region private fun
    private void OnItemChange(object sender, ItemAsset.ItemPackage.EventArg arg)
    {
        for(int i=0;i<m_Achievements.Length;i++)
        {
            if (m_Achievements[i].m_bAchieved) continue;
            if (m_Achievements[i].m_ProtoId == 0) continue;

            if (Pathea.PeCreature.Instance != null && Pathea.PeCreature.Instance.mainPlayer != null)
            {
                Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.packageCmpt as Pathea.PlayerPackageCmpt;
                if (pkg != null)
                {
                    int count = pkg.package.GetCount(m_Achievements[i].m_ProtoId);
                    OnItemValueChange(m_Achievements[i].m_eAchievementID, count);
                }
            }

        }
        
    }
    private void OnItemValueChange(Eachievement eNewState,int value)
    {
        if (!m_bStatsValid)
            return;

        if (m_Stats == null 
            || ((int)eNewState) > m_Stats.Length 
            || (int)eNewState > m_Achievements.Length
            || m_Stats[(int)eNewState].IsAccomplish())
            return;

        m_Stats[(int)eNewState].m_StatValue = value;
        m_bStoreStats = true;
    }
    public void ResetAllAchievements()
    {
        // Reset achievements  
        for (int i = 0; i < m_Achievements.Length; i++)
        {
            ResetAchievement(m_Achievements[i]);
        }
    }
    private  void ResetAchievement(AchievementInfo info)
    {
        bool ret = SteamUserStats.GetAchievement(info.m_apiName, out info.m_bAchieved);
        if (ret && info.m_bAchieved)
        {
            //StatsUserPrefs.SaveIntValue();
            SteamUserStats.ClearAchievement(info.m_apiName);
            SteamUserStats.RequestCurrentStats();
        }
        else
        {
            Debug.LogWarning("SteamUserStats.GetAchievement failed Rest for Achievement " + info.m_apiName + "\nIs it registered in the Steam Partner site?");
        }
       

    }
    private void ResetAllStats()
    {
        for(int i=0;i<m_Stats.Length;i++)
        {
            m_Stats[i].ResetValue();
        }       
        m_bStoreStats = true;
    }

    #endregion

    #region 排名系统
    private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
    {
        if (pCallback.m_hSteamLeaderboard == m_steamLeaderboard)
        {
            if (pCallback.m_cEntryCount > 0)
            {
                Debug.LogError("排行榜数据量：" + pCallback.m_cEntryCount);
                for (int i = 0; i < pCallback.m_cEntryCount; i++)
                {
                    LeaderboardEntry_t leaderboardEntry;
                    int[] details = new int[pCallback.m_cEntryCount];
                    SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out leaderboardEntry, details, pCallback.m_cEntryCount);
                    Debug.LogError("用户ID：" + leaderboardEntry.m_steamIDUser + "用户分数" + leaderboardEntry.m_nScore + "用户排名" + leaderboardEntry.m_nGlobalRank + "Details" + leaderboardEntry.m_cDetails + "  " + SteamFriends.GetFriendPersonaName(leaderboardEntry.m_steamIDUser));
                    gos[i].transform.FindChild("name").GetComponent<UILabel>().text = leaderboardEntry.m_nGlobalRank + "  " + SteamFriends.GetFriendPersonaName(leaderboardEntry.m_steamIDUser);
                    gos[i].transform.FindChild("score").GetComponent<UILabel>().text = leaderboardEntry.m_nScore + "";
                }

            }
            else
            {
                Debug.LogError("排行榜数据为空！");
            }
        }
    }

    private void OnLeaerboardScoreUploaded(LeaderboardScoreUploaded_t pCallback, bool bIOFailure)
    {
        if (pCallback.m_bSuccess != 1)
        {
            //UploadScore(PlayerControl.Ins.Award);

        }
        //else if(pCallback.m_hSteamLeaderboard!= m_steamLeaderboard)  
        //{  
        //    Debug.LogError("获取排行榜数据信息有误！！");  
        //}  
        else
        {
            Debug.LogError("成功上传价值数据：" + pCallback.m_nScore + "榜内数据是否需要变更：" + pCallback.m_bScoreChanged
                + "新的排名：" + pCallback.m_nGlobalRankNew + "上次排名：" + pCallback.m_nGlobalRankPrevious);
            gos[gos.Count - 1].transform.FindChild("name").GetComponent<UILabel>().text = pCallback.m_nGlobalRankNew + "  " + SteamFriends.GetPersonaName();
            gos[gos.Count - 1].transform.FindChild("score").GetComponent<UILabel>().text = pCallback.m_nScore + "";
            if (pCallback.m_nGlobalRankPrevious == 0 || pCallback.m_bScoreChanged != 0)
            {

            }
            DownloadLeaderboardEntries();
        }
    }

    private void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool bIOFailure)
    {
        if (!SteamManager.Initialized)
            return;
        if (pCallback.m_hSteamLeaderboard.m_SteamLeaderboard == 0 || pCallback.m_bLeaderboardFound == 0)
        {
            Debug.LogError("There is no Leaderboard found");
        }
        else
        {
            m_steamLeaderboard = pCallback.m_hSteamLeaderboard;
            //UploadScore(PlayerControl.Ins.Award);
        }

    }

    public void UploadScore(int score)
    {
        if (m_steamLeaderboard.m_SteamLeaderboard != 0)
        {
            Debug.LogError("上传分数");
            var handle = SteamUserStats.UploadLeaderboardScore(m_steamLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, score, null, 0);
            m_LeaderboardScoreUploaded.Set(handle);
        }
    }

   
    public void DownloadLeaderboardEntries()
    {
        if (m_steamLeaderboard.m_SteamLeaderboard != 0)
        {
            Debug.LogError("请求排行榜数据");
            var handle = SteamUserStats.DownloadLeaderboardEntries(m_steamLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 1, 8);
            m_LeaderboardScoresDownloaded.Set(handle);
        }
    }
    #endregion
  
    #region 成就系统
    //-----------------------------------------------------------------------------  
    // Purpose: Unlock this achievement  
    //-----------------------------------------------------------------------------  

        /// <summary>
        /// 解锁一个成就
        /// </summary>
        /// <param name="achievement"></param>
    private void UnlockAchievement(AchievementInfo achievement)
    {
        achievement.m_bAchieved = true;

        // the icon may change once it's unlocked  
        //achievement.m_iIconImage = 0;  

        // mark it down  
        SteamUserStats.SetAchievement(achievement.m_apiName);

        // Store stats end of frame  
        m_bStoreStats = true;
    }

    //-----------------------------------------------------------------------------  
    // Purpose: We have stats data from Steam. It is authoritative, so update  
    //          our data with those results now.  
    //-----------------------------------------------------------------------------  
    /// <summary>
    /// 上传数据完成后，收到回调 UserStatsReceived_t
    /// </summary>
    /// <param name="pCallback"></param>
    private void OnUserStatsReceived(UserStatsReceived_t pCallback)
    {
        if (!SteamManager.Initialized)
            return;

        // we may get callbacks for other games' stats arriving, ignore them  
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                Debug.Log("Received stats and achievements from Steam\n");

                m_bStatsValid = true;

                // load achievements  
                for(int i=0;i<m_Achievements.Length;i++)               
                {
                    bool ret = SteamUserStats.GetAchievement(m_Achievements[i].m_apiName, out m_Achievements[i].m_bAchieved);
                    if (ret)
                    {
                        //m_Achievements[i].m_apiName = SteamUserStats.GetAchievementDisplayAttribute(m_Achievements[i].m_apiName, "name");
                        m_Achievements[i].m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(m_Achievements[i].m_apiName, "desc");
                        Debug.Log(m_Achievements[i].m_eAchievementID.ToString() + "  " + m_Achievements[i].m_bAchieved + "  " + m_Achievements[i].m_apiName + "  " + m_Achievements[i].m_strDescription);
                    }
                    else
                    {
                        Debug.Log("SteamUserStats.GetAchievement failed for Achievement " + m_Achievements[i].m_eAchievementID + "\nIs it registered in the Steam Partner site?");
                    }
                    // load stats 
                    if(!string.IsNullOrEmpty(m_StatNames[(int)m_Achievements[i].m_eStatID]))
                    {
                        bool bget = StatsUserPrefs.GetIntValue(m_StatNames[(int)m_Achievements[i].m_eStatID], out m_Stats[(int)m_Achievements[i].m_eAchievementID].m_StatValue);
                        if (!bget)
                        {
                            Debug.LogWarning("Do not has " + m_StatNames[(int)m_Achievements[i].m_eStatID]);
                        }
                    }
                    
                }
                
            }
            else
            {
                Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    //-----------------------------------------------------------------------------  
    // Purpose: Our stats data was stored!  
    //-----------------------------------------------------------------------------  
    private void OnUserStatsStored(UserStatsStored_t pCallback)
    {
        // we may get callbacks for other games' stats arriving, ignore them  
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                Debug.Log("StoreStats - success");
            }
            else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
            {
                // One or more stats we set broke a constraint. They've been reverted,  
                // and we should re-iterate the values now to keep in sync.  
                Debug.Log("StoreStats - some failed to validate");
                // Fake up a callback here so that we re-load the values.  
                UserStatsReceived_t callback = new UserStatsReceived_t();
                callback.m_eResult = EResult.k_EResultOK;
                callback.m_nGameID = (ulong)m_GameID;
                OnUserStatsReceived(callback);
            }
            else
            {
                Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    //-----------------------------------------------------------------------------  
    // Purpose: An achievement was stored  
    //-----------------------------------------------------------------------------  
    private void OnAchievementStored(UserAchievementStored_t pCallback)
    {
        // We may get callbacks for other games' stats arriving, ignore them  
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (0 == pCallback.m_nMaxProgress)
            {
                Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
            }
            else
            {
                Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
            }
        }
    }

    #endregion

}

public class StatsUserPrefs
{
    public static void InitIntKey(string statName,int value =0)
    {
        PlayerPrefs.SetInt(statName, value);
        PlayerPrefs.Save();
    }

    public static bool SaveIntValue(string statName, int value = 0)
    {
        PlayerPrefs.SetInt(statName, value);
        PlayerPrefs.Save();
        return true;
    }

    public static bool GetIntValue(string statName,out int value)
    {
        value = 0;
        if (!PlayerPrefs.HasKey(statName)) return false;

        value = PlayerPrefs.GetInt(statName);
        return true;
    }

}
public class Istat
{
    public int m_StatValue;
    public int m_Goal = 0;
    public virtual bool IsAccomplish() { return false; }
    public virtual void IncreaseStat() { }

    public virtual void DropStat() { }
    public virtual void SetGoal(object obj) { }

    public virtual void ResetValue() { }
  
}

public  class TriggerStat : Istat
{  
    public override bool IsAccomplish()
    {
        return m_StatValue >= m_Goal;
    }

    public override void IncreaseStat()
    {
        m_StatValue++;
    }

    public override void DropStat()
    {
        if (m_StatValue <= 0) return;

        m_StatValue--;
    }
    public override void ResetValue()
    {
        m_StatValue = 0;
    }
    public override void SetGoal(object obj)
    {
        m_Goal = (int)obj;
    }
}
