using UnityEngine;
using System.Collections;

/// <summary>
///lz-2016.8.22 这个类是UI枚举，用来声明UI的枚举和ID，以及UI上的按钮枚举和ID
/// </summary>
public class UIEnum
{
    public enum WndType
    {
        Null=0,                         //没有类型

        // GameMain
        GameMenu=100,                     //右下角的游戏菜单
        MainMid = 101,                    //快捷栏
        MinMap = 102,                     //小地图
        NPCTalk = 103,                    //npc对话框
        NpcSpeech= 104,          
        ServantTalk = 105,
        MissionTrack = 106,               //任务追踪
        WorldMapCtrl = 107,               //大地图
        
        //系统菜单
        SystemMenu = 108,                 //系统菜单
        Option = 109,                     //选项菜单
        SaveLoad = 110,                   //保存和加载
        
        //游戏菜单
        PlayerInfo = 111,                 //玩家信息
        ItemPackage = 112,                //玩家背包
        NpcStorage = 113,
        Compound = 114,                   //玩家复制机
        Servant = 115,                    //仆从界面
        NpcWnd = 116,                     
        Mission = 117,                    //任务界面
        ItemGet = 118,
        ItemOp = 119,                         
        Shop = 120,                       //NPC商店
        ItemBox = 121,         
        Repair = 122,                     //修理机
        PowerPlantSolar = 123,
        Revive = 124,                     //复活界面
        Warehouse = 125,
        CSUI_Main = 126,                  //基地界面
        BuildBlock = 127,                 //建筑界面
        Scan = 128,                       //探矿器界面
        Help = 129,                       //帮助
        MonoRail= 130,                    //轨道
        Camp =131,                        //阵营
        Message=132,                      //系统消息
        MonsterHandbook =133,             //怪物图鉴
        Radio = 147,                      //收音机界面   //lz-2016.12.16 添加收音机界面ID


        Skill = 134,
        WorkShop = 135,                   //WorkShop
        CSUI_TeamInfo = 136,
        RailwayPointGui = 137,            //
        MallWnd = 138,                    //商场
        Tips = 139,                       //提示
        TipRecords = 140,                 //提示列表

        // AdminUI
        Adminstrator = 141,

        // Driving UI
        Driving = 142,                    //載具驾驶界面

        StopwatchList = 143,              //
        NpcDialog = 144,                  //
        MissionGoal = 145,                //
        CustomMissionTrack = 146,         //
    };

    //下面是一个栗子，如果想添加按钮ID，ID以UI的ID开头，比如CustomMissionTrack=141，就是141***，***是保留三位整数，依次递增
    //141
    public enum CustomMissionTrackBtnType
    {
        Start=141001,
    };

}
