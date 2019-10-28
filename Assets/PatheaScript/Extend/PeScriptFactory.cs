using PatheaScript;

namespace PatheaScriptExt
{
    public class PeFactory : Factory
    {
        const int DefaultEntryScriptId = 0;
        const string GameCustomDir = "GameCustom";
        const string QuestDir = "Script";

        static readonly string DefaultScriptDirPath = GameConfig.PEDataPath + "/" + GameCustomDir + "/" + QuestDir + "/";

        public override int EntryScriptId
        {
            get
            {
                return DefaultEntryScriptId;
            }
        }

        public override string ScriptDirPath
        {
            get
            {
                return DefaultScriptDirPath;
            }
        }

        public override bool Init()
        {
            if (false == base.Init())
            {
                return false;
            }

            RegisterEvents();

            RegisterConditions();

            RegisterActions();

            //ScriptDirPath = DefaultScriptDirPath;
            //EntryScriptId = DefaultEntryScriptId;

            return true;
        }

        private void RegisterActions()
        {
            RegisterAction("ORDER MOVE", "PatheaScriptExt.ActionMove");
            RegisterAction("WAIT", "PatheaScriptExt.ActionWait");
            RegisterAction("PLAYER POSITION", "PatheaScriptExt.ActionPlayerPosition");
            RegisterAction("MODIFY PLAYER ITEM", "PatheaScriptExt.ActionModifyPlayerItem");
            RegisterAction("SET STOPWATCH", "PatheaScriptExt.ActionStopWatch");
            RegisterAction("UNSET STOPWATCH", "PatheaScriptExt.ActionKillStopWatch");
            RegisterAction("KILL PLAYER", "PatheaScriptExt.ActionKillPlayer");
            RegisterAction("KILL NPC", "PatheaScriptExt.ActionKillNpc");
            RegisterAction("KILL MONSTER", "PatheaScriptExt.ActionKillMonster");
            RegisterAction("SET PLAYER STATS MAX", "PatheaScriptExt.ActionModifyPlayerStatusMax");
            RegisterAction("SET PLAYER STATS", "PatheaScriptExt.ActionModifyPlayerStatus");
            RegisterAction("SET NPC STATS MAX", "PatheaScriptExt.ActionModifyNpcStatusMax");
            RegisterAction("SET NPC STATS", "PatheaScriptExt.ActionModifyNpcStatus");
            RegisterAction("SET MONSTER STATS MAX", "PatheaScriptExt.ActionModifyMonsterStatusMax");
            RegisterAction("SET MONSTER STATS", "PatheaScriptExt.ActionModifyMonsterStatus");
            RegisterAction("CONVERSATION", "PatheaScriptExt.PeConversation");
        }

        private void RegisterConditions()
        {
            RegisterCondition("PLAYER OWN", "PatheaScriptExt.CPlayerOwn");
            RegisterCondition("STOPWATCH", "PatheaScriptExt.CStopWatch");
            RegisterCondition("KILL", "PatheaScriptExt.CPlayerKillMonster");
        }

        private void RegisterEvents()
        {
            RegisterEvent("FRAME", "PatheaScriptExt.EFrameProxy", "PatheaScriptExt.EFrame");
            RegisterEvent("TALK TO NPC", "PatheaScriptExt.EPlayerTalkToNpcProxy", "PatheaScriptExt.EPlayerTalkToNpc");
            RegisterEvent("CLICK UI", "PatheaScriptExt.EUiClickedProxy", "PatheaScriptExt.EUiClicked");
            RegisterEvent("PLAYER DEAD", "PatheaScriptExt.EPlayerDeadProxy", "PatheaScriptExt.EPlayerDead");
            RegisterEvent("NPC DEAD", "PatheaScriptExt.ENpcDeadProxy", "PatheaScriptExt.ENpcDead");
            RegisterEvent("MONSTER DEAD", "PatheaScriptExt.EMonsterDeadProxy", "PatheaScriptExt.EMonsterDead");
        }
    } 
}
