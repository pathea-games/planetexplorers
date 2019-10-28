using UnityEngine;
using System.Collections.Generic;
using ScenarioRTL.IO;
using ScenarioRTL;

namespace PeCustom
{

    public class MissionProperty
    {
        public string name;
        public enum EType
        {
            Hidden,
            MainStory,
            SideQuest
        }

        public EType type;
        public bool canAbort;
        public string objective;

        public int beginNpcId;
        public int beginNpcWorldIndex;

        public int endNpcId;
        public int endNpcWorldIndex;

        public string rewardDesc;
        public List<int> rewardItemIds;
        public List<int> rewardItemCount;

        public MissionProperty()
        {
            rewardItemCount = new List<int>(5);
            rewardItemIds = new List<int>(5);
        }

        public void Parse(ParamRaw param, string mission_name)
        {
            name = mission_name;
            type = (EType)Utility.ToInt(null, param["type"]);
            canAbort = Utility.ToBool(null, param["can_abort"]);
            objective = param["objective"];

            string str = null;
            string[] split_str = null;
            try
            {
                str = param["begin_npc"];
                split_str = str.Split('|');
                beginNpcWorldIndex = int.Parse(split_str[0]);
                beginNpcId = int.Parse(split_str[1]);
            }
            catch
            {
                beginNpcWorldIndex = -1;
                beginNpcId = -1;
            }

            try
            {
                str = param["end_npc"];
                split_str = str.Split('|');
                endNpcWorldIndex = int.Parse(split_str[0]);
                endNpcId = int.Parse(split_str[1]);
            }
            catch
            {
                endNpcId = -1;
                endNpcWorldIndex = -1;
            }

            try
            {
                str = param["award"];
                split_str = str.Split('|');
                if (split_str != null && split_str.Length != 0)
                {
                    for (int i = 0; i < split_str.Length; i++)
                    {
                        string[] sub_split_str = split_str[i].Split(':');
                        rewardItemIds.Add(int.Parse(sub_split_str[0]));
                        rewardItemCount.Add(int.Parse(sub_split_str[1]));
                    }
                }
                else
                    rewardDesc = str;
            }
            catch
            {
                rewardDesc = str;
            }
        }
    }
}
