using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

namespace PeCustom
{
    public class DialogMgr
    {
        #region Quest_Part
        public Action<int, int> onNpoQuestsChanged;

        public IList<string> GetQuests (int world_index, int npo_id)
        {
            int key = WorldIndexAndNpoIdToKey(world_index, npo_id);
            if (m_Quests.ContainsKey(key))
                return m_Quests[npo_id].Values;
            else
                return null;
        }

        public int GetQuestId (int world_index, int npo_id, int index)
        {
            if (index < 0)
                return -1;

            int key = WorldIndexAndNpoIdToKey(world_index, npo_id);

            if (m_Quests.ContainsKey(key))
            {
                IList<int> ids = m_Quests[key].Keys;
                if (index < ids.Count )
                    return  m_Quests[key].Keys[index];
                else
                    return -1;
            }

            return -1;
        }

        public void SetQuest (int world_index, int npo_id, int quest_id, string text)
        {
            int key = WorldIndexAndNpoIdToKey(world_index, npo_id);

            if (!m_Quests.ContainsKey(key))
            {
               SortedList<int, string> val = new SortedList<int, string>(5);
               val[quest_id] = text;
                m_Quests.Add(key, val);
            }
            else
            {
                m_Quests[key][quest_id] = text;
            }

            if (onNpoQuestsChanged != null)
                onNpoQuestsChanged(world_index, npo_id);
        }

        public void RemoveQuest (int world_index, int npo_id, int quest_id)
        {
            int key = WorldIndexAndNpoIdToKey(world_index, npo_id);
            if (m_Quests.ContainsKey(key))
            {
                m_Quests[key].Remove(quest_id);

                if (onNpoQuestsChanged != null)
                    onNpoQuestsChanged(world_index, npo_id);
            }
        }

        public void Clear (int world_index, int npo_id)
        {
            int key = WorldIndexAndNpoIdToKey(world_index, npo_id);
            if (m_Quests.ContainsKey(key))
            {
                m_Quests[key].Clear();
                m_Quests.Remove(key);

                if (onNpoQuestsChanged != null)
                    onNpoQuestsChanged(world_index, npo_id);
            }
        }

        public void ClearAll ()
        {
            foreach (var kvp in m_Quests)
            {
                kvp.Value.Clear();

                int world_index = 0;
                int npo_id =0;
                KeyToWorldIndexAndNpoId(kvp.Key, out world_index, out npo_id);
                if (onNpoQuestsChanged != null)
                    onNpoQuestsChanged(world_index, npo_id);
            }

            m_Quests.Clear();
        }
        #endregion


        #region Choice_Part

        public Action onChoiceChanged;

        public IList<string> GetChoices()
        {
            return m_Choices.Values;
        }

        public int GetChoiceId (int index)
        {
            if (index < 0 && index > m_Choices.Count)
                return -1;

            return m_Choices.Keys[index];
        }

        public bool BeginChooseGroup ()
        {
           if (!m_BeginChooseGroup)
            {
                m_Choices.Clear();

                if (onChoiceChanged != null)
                    onChoiceChanged();

                m_BeginChooseGroup = true;

                return true;
            }

           return false;

        }

        public bool AddChoose(int choose_id, string text)
        {
            if (m_BeginChooseGroup)
            {
                m_Choices[choose_id] = text;

                if (onChoiceChanged != null)
                    onChoiceChanged();
                return true;
            }

            return  false;

        }

        public bool EndChooseGroup()
        {
            if (m_BeginChooseGroup)
            {
                m_BeginChooseGroup = false;
                return true;
            }
            
            return false;
        }
        #endregion
        #region IO
        const int VERSION = 0x00000001;

        public void Import (BinaryReader r)
        {
            r.ReadInt32();

            switch (VERSION)
            {
                case 0x00000001:
                    {
                        // Quest
                        int count = r.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            int key = r.ReadInt32();
                            SortedList<int, string> list = new SortedList<int, string>(5);
                            int sub_cnt = r.ReadInt32();
                            for (int j = 0; j < sub_cnt; j++)
                            {
                                list.Add(r.ReadInt32(), r.ReadString());
                            }

                            m_Quests.Add(key, list);
                        }

                        // choice
                        m_BeginChooseGroup = r.ReadBoolean();
                        count = r.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            m_Choices.Add(r.ReadInt32(), r.ReadString());
                        }
                    }break;
            }
        }

        public byte[] Export ()
        {
            byte[] data = null;
            using (MemoryStream ms_iso = new MemoryStream())
            {
                BinaryWriter w = new BinaryWriter(ms_iso);
                Export(w);

                data = ms_iso.ToArray();
                ms_iso.Close();
            }

            return data;
        }

        public void Export (BinaryWriter w)
        {
            w.Write(VERSION);

            // Quest
            w.Write(m_Quests.Count);
            foreach (var kvp in m_Quests)
            {
                w.Write(kvp.Key);
                w.Write(kvp.Value.Count);
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    w.Write(kvp.Value.Keys[i]);
                    w.Write(kvp.Value.Values[i]);
                }
            }

            // Speech Choices
            w.Write(m_BeginChooseGroup);
            w.Write(m_Choices.Count);
            for (int i = 0; i< m_Choices.Count; i++)
            {
                w.Write(m_Choices.Keys[i]);
                w.Write(m_Choices.Values[i]);
            }
        }
        #endregion

        public DialogMgr()
        {
            m_Quests = new Dictionary<int, SortedList<int, string>>(32);

            m_Choices = new SortedList<int, string>();

        }

        // Key1 : NPOOBJECT ID in scenario. Key2 : Quest ID
        Dictionary<int, SortedList<int,string>> m_Quests;

        // Key2 : Choice ID
        SortedList<int, string> m_Choices;

        bool m_BeginChooseGroup = false;


        #region HELP_FUNC
        public static int WorldIndexAndNpoIdToKey(int world_index, int npo_id)
        {
            return (world_index << 16) + npo_id;
        }

        public static void KeyToWorldIndexAndNpoId (int key, out int world_index, out int npo_id)
        {
            world_index = key >> 16;
            npo_id = key - ( world_index << 16);

        }
        #endregion
    }
}
