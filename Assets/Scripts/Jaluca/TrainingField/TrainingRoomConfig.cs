using UnityEngine;
using System.Collections;

namespace TrainingScene
{
    public enum TrainingTaskType
    {
        SYN,
        DIG,
        BUILDIn,
        BuildPoint,
        CreationOpen,
        SaveIso,
        CreateIso,
        Replicator,
        Move,
        GetMedicine,
        BackToAndy,
        OpenPack,
        PutMed,
        EquipKnife,
        Fighting,
        GATHER,
        CUT,
        BuildSaveIso,
        BuildExpotIso,
        ChangeControlMode,
        BaseControl,
        BuildMenu,
        MissionPlane
    }
    public class TrainingRoomConfig
    {
        public int Lauguage = 1;
        public const string NPCNAME = "Jaluca";
        public const float Felling1time = 0.6f;
        public const float Felling2time = 1.1f;
        public const float FellingHittime = 0.6f;
        public const float Felling3time = 0.533f;
        public const float HerbGatherTime = 1.966f;
        public const float HerbHitTime = 1.1f;
        public const string GatherName = "train_gather";
        public const string CutName = "train_cut";
        public const string SynName = "train_syn";
        public const string DigName = "train_dig";
        public const string BuildName = "train_build";
        public const int StartID = 718;
        public const int GatherID = 719;
        public const int CutID = 748;
        public const int SynID = 721;
        public const int DigID = 751;
        public const int BuildoneID = 723;
        public const int BuildInID = 752;
        public const int BuildPoint = 753;
        public const int CreateIso = 756;
        public const int Replicator = 757;
        public const int ChangeControlModeID = 739;
        public const int BaseControlID = 747;
        public const int MoveID = 740;
        public const int GetMed = 741;
        public const int BackToAndy = 742;
        public const int OpenPackage = 743;
        public const int PutMed = 744;
        public const int EquipKnife = 745;
        public const int Fighting = 746;
        public const int BuildSaveIso = 754;
        public const int BuildExportIso = 755;
        public const int BuildMenu = 750;
        public const int UseJuice = 749;
        public const int MissionPlane = 718;

        public static int[] IDs = { 719, 720, 721, 722, 723, 724 };

        public static bool IsTrainingMission(int missionID)
        {
            foreach (int i in IDs)
            {
                if (i == missionID)
                    return true;
            }
            return false;
        }

        public static TrainingMission[] missions = new TrainingMission[5]
		{
			new TrainingMission("采集训练", "dig train", "装备工具挖掘，直到能打开埋藏的宝箱", 
			                    "equip the tool and dig until you can get the treasures buried in the soil", "SubMission"),
			new TrainingMission("砍树训练", "felling train", "装备工具砍树，获得材料", 
			                    "equip the tool and cut the tree down to gather materials", "SubMission"),
			new TrainingMission("训练", "felling train", "装备工具砍树，获得材料", 
			                    "equip the tool and cut the tree down to gather materials", "SubMission"),
			new TrainingMission("挖掘训练", "dig train", "装备工具挖掘，直到能打开埋藏的宝箱", 
			                    "equip the tool and dig until you can get the treasures buried in the soil", "SubMission"),
			new TrainingMission("搭建训练", "felling train", "装备工具砍树，获得材料", 
			                    "equip the tool and cut the tree down to gather materials", "SubMission")

		};

        public class TrainingMission
        {
            string titleChs = "";
            string titleEng = "";
            string discribeChs = "";
            string discribeEng = "";
            string iconname = "SubMission";

            public TrainingMission(string titleC, string titleE, string discC, string discE, string iconN)
            {
                titleChs = titleC;
                titleEng = titleE;
                discribeChs = discC;
                discribeEng = discE;
                iconname = iconN;
            }
            public string getTitle(int isChs)
            {
                if (isChs == 1)
                    return titleChs;
                else
                    return titleEng;
            }
            public string getDiscribe(int isChs)
            {
                if (isChs == 1)
                    return discribeChs;
                else
                    return discribeEng;
            }
            public string getIcon()
            {
                return iconname;
            }
        }
    }

}
