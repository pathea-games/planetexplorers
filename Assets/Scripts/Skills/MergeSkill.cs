//using UnityEngine;
//using Mono.Data.SqliteClient;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using ItemAsset;

//namespace SkillAsset
//{
//    public class MergeSkillMaterialItem
//    {
//        internal int m_id;
//        internal int m_count;

//        public MergeSkillMaterialItem(int id, int count)
//        {
//            m_id = id;
//            m_count = count;
//        }
//    }

//    public class MergeSkill
//    {
//        internal int m_id;
//        internal int m_productItemId;
//        internal float m_timeNeeded;
//        internal short m_productItemNumber;	//number_each_product
//        internal short m_tabId;
//        internal List<MergeSkillMaterialItem> m_materialItems;
//        // string from item table coresponding to id
//        internal string m_tab;
//        internal string m_name;
//        internal int m_workSpace;

//        public bool CheckCond()
//        {
//            //TODO : check if toolset is available
//            return true;
//        }

//        // input	: product count( times of m_productItemNumber ), 
//        // return	: a list to be return(count is max count of products)
//        public List<MergeSkillMaterialItem> ProductCountEval(SkillRunner caster)
//        {
//            //			List<MergeSkillMaterialItem> maxProductNumList = new List<MergeSkillMaterialItem> ();
//            //			ItemPackage pack = caster.GetItemPackage ();
//            //			for (int i = 0; i < m_materialItems.Count; i++) 
//            //			{
//            //				int id = m_materialItems [i].m_id;
//            //				int count = pack.GetItemCount (id) / m_materialItems [i].m_count;
//            //				maxProductNumList.Add (new MergeSkillMaterialItem (id, count));
//            //			}
//            //			return maxProductNumList;
//            return m_materialItems;
//        }
//        // return	: available grid index for production
//        //		public int AvailableProductGrid (SkillRunner caster, int productCount)
//        //		{
//        //			ItemPackage pack = caster.GetItemPackage ();
//        //			int idx = pack.GetEmptyGrid ();
//        //			if (idx >= 0)
//        //				return idx;
//        //			
//        //			for (int i = 0; i < m_materialItems.Count; i++)
//        //			{
//        //				int count = m_materialItems [i].m_count * productCount;
//        //				for (int j = 0; j < pack.GetItemList(0).Count; j++)
//        //				{
//        //					if (pack.m_itemGrids [j].m_itemID == m_materialItems [i].m_id && 
//        //					   pack.m_itemGrids [j].GetCount () <= count)
//        //					{
//        //						return j;
//        //					}
//        //				}
//        //			}
//        //			return -1;
//        //		}
//        // input	: meterial list, and a empty grid
//        // output	: meterial list, and grid with product items in.
//        public IEnumerator Exec(SkillRunner caster, int productCount, MergeSkillInstance inst)
//        {
//            caster.m_mergeSkillInsts.Add(inst);

//            if (m_timeNeeded > 0)
//            {
//                yield return new WaitForSeconds(m_timeNeeded);
//            }


//            if (!GameConfig.IsMultiMode)
//            {
//                //print(""+m_materialItems.Count+","+m_materialItems[0].m_count);
//                List<int> countList = new List<int>();
//                for (int i = 0; i < m_materialItems.Count; i++)
//                {
//                    countList.Add(productCount * m_materialItems[i].m_count);
//                }

//                ItemPackage pack = caster.GetItemPackage();

//                ItemSample addItem = new ItemSample(m_productItemId,productCount * m_productItemNumber);

//                pack.AddItem(addItem);

//                //			int idx = AvailableProductGrid (caster, productCount);
//                //			if (pack.m_itemGrids [idx] != ItemGrid.EmptyGrid)
//                //			{
//                //				for (int i = 0; i < m_materialItems.Count; i++)
//                //				{
//                //					if (pack.m_itemGrids [idx].m_itemID == m_materialItems [i].m_id)
//                //					{
//                //						countList [i] -= pack.m_itemGrids [idx].GetCount ();
//                //					}
//                //				}
//                //			}
//                //			
//                //			int Index = pack.GetSameItem (addItem);
//                //			if (Index != 255 && pack.m_itemGrids [Index].mItem.m_StackNum > 1)
//                //			{
//                //				pack.m_itemGrids [Index].CountUp (productCount * m_productItemNumber);
//                //			}
//                //			else
//                //			{
//                //				pack.m_itemGrids [idx] = addItem;
//                //				pack.m_itemGrids [idx].CountUp (productCount * m_productItemNumber);
//                //			}
//                //			
//                //GameMainGUI.m_Pack.m_GridList[idx].SetItemGrid(pack.m_itemGrids[idx]);
//                for (int i = 0; i < m_materialItems.Count; i++)
//                {
//                    pack.DeleteItemWithItemID(m_materialItems[i].m_id, countList[i]);
//                }

//                //PlayerFactory.mMainPlayer.UseMergeSkill(inst.m_data.m_id);

                
//            }
//            caster.m_mergeSkillInsts.Remove(inst);

//            //             if (GameConfig.IsMultiMode() && uLink.Network.isServer)
//            //             {
//            //                 caster.SendRemoveMergeSkillInsts(inst.m_data.m_id);
//            //             }
//            GameUI.Instance.mUIItemPackageCtrl.ResetItem();
//            //			GameMainGUI.m_compound.updateRight();
//        }

//        public static List<MergeSkill> s_tblMergeSkills;

//        public static void LoadData()
//        {
//            s_tblMergeSkills = new List<MergeSkill>();
//            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("synthesis_skill");
//            while (reader.Read())
//            {
//                MergeSkill skill = new MergeSkill();
//                skill.m_id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
//                skill.m_productItemId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("object_ID")));
//                skill.m_timeNeeded = Convert.ToSingle(reader.GetString(reader.GetOrdinal("need_time")));
//                //skill.m_tabId = Convert.ToInt16(reader.GetString(reader.GetOrdinal("tab")));
//                skill.m_productItemNumber = Convert.ToInt16(reader.GetString(reader.GetOrdinal("num_each_product")));
//                int idxMat = reader.GetOrdinal("material1");
//                skill.m_materialItems = new List<MergeSkillMaterialItem>();
//                for (int i = 0; i < 6; i++)
//                {
//                    int count = Convert.ToInt32(reader.GetString(idxMat + 1 + i * 2));
//                    if (count <= 0)
//                        break;
//                    skill.m_materialItems.Add(new MergeSkillMaterialItem(Convert.ToInt32(reader.GetString(idxMat + i * 2)), count));
//                }
//                ItemData itemProduct = ItemData.s_tblItemData.Find(iter0 => ItemData.MatchId(iter0, skill.m_productItemId));
//                skill.m_name = itemProduct == null ? "NA" : itemProduct.GetName();
//                skill.m_workSpace = Convert.ToInt32(reader.GetString(reader.GetOrdinal("workspace")));

//                s_tblMergeSkills.Add(skill);
//            }
//        }

//        public static bool MatchId(MergeSkill iter, int id)
//        {
//            return iter.m_id == id;
//        }

//        public static string GetName(int id)
//        {
//            MergeSkill data = MergeSkill.s_tblMergeSkills.Find(iterSkill1 => MergeSkill.MatchId(iterSkill1, id));
//            if (data == null)
//                return "";

//            return ItemAsset.ItemData.GetName(data.m_productItemId);
//        }
//        public static string GetIconName(int id)
//        {
//            MergeSkill data = MergeSkill.s_tblMergeSkills.Find(iterSkill1 => MergeSkill.MatchId(iterSkill1, id));
//            if (data == null)
//                return null;

//            return ItemAsset.ItemData.GetIconName(data.m_productItemId);
//        }
//    }

//}
