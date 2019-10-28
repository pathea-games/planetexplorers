using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class PromptData
{

 //"ItemName%"  -  道具名称
 //"PlayerName%"  -  角色名称
 //"HeroName%"   -   仆从名称
 //"MissionName%"  -  任务名称
 //"SkillName%"   -   技能名称
 //"ItemNum%"    -    道具名称


    public enum PromptType
    {
        PromptType_Unkown,
        PromptType_1,//接领任务
        PromptType_2,//任务失败
        PromptType_3,//任务交还  可能多个
        PromptType_4,//拾取物品  可能多个
        PromptType_5,//合成物品
        PromptType_6,//删除物品
        PromptType_7,//招募
        PromptType_8,//遣散仆从
        PromptType_9,//仆从死亡
        PromptType_10,//学习合成技能
        PromptType_11,//使用回复生命药剂
        PromptType_12,//使用舒适度回复食品
        PromptType_13,//购买物品
        PromptType_14,//出售物品
        PromptType_15,//修理物品
        PromptType_16,//死亡时丢失物品   可能多个
        PromptType_17,//主角生命值过低，生命值<20%
        PromptType_18,//角舒适度过低，舒适度<20%
        PromptType_19,//仆从生命值过低，生命值<20%
        PromptType_20,//仆从舒适度过低，舒适度<20%
        PromptType_21,//塔防任务的单独提示
        PromptType_22,//攻击状态上升
        PromptType_23,//防御状态上升
        PromptType_24,//已经学过合成技能了，不能再学。
        PromptType_25,//钱不够
    }

    public int m_Type;
    public string m_Content;

}

public class PromptRepository
{
    public static Dictionary<int, PromptData> m_PromptMap = new Dictionary<int, PromptData>();

    public static PromptData GetPromptData(int type)
    {
        return m_PromptMap.ContainsKey(type) ? m_PromptMap[type] : null;
    }

    public static string GetPromptContent(int type)
    {
        PromptData data = GetPromptData(type);
        if (data == null)
            return "";

        return data.m_Content;
    }

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("System_prompt");
        while (reader.Read())
        {
            PromptData data = new PromptData();
            data.m_Type = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
            data.m_Content = reader.GetString(reader.GetOrdinal("hint"));

            m_PromptMap.Add(data.m_Type, data);
        }
    }
}
