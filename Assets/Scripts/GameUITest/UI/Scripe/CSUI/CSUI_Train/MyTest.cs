using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

public class MyTest : MonoBehaviour
{
    public Texture[] linshiyong;
    // Use this for initialization
    void Awake()
    {

    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("创建数据开始");
            CSUI_Train.Instance.GetAllRandomNpcsEvent += SendAllRandomNpcs;
            CreatRandomNpcs(10);
        }
    }
    List<CSUIMyNpc> allRandomNpcs = new List<CSUIMyNpc>();
    void SendAllRandomNpcs()
    {
        if (allRandomNpcs.Count > 0)
            CSUI_Train.Instance.GetAllRandomNpcsMethod(allRandomNpcs);
    }
    void CreatRandomNpcs(int numbers)
    {
        if (numbers <= 5)
            return;
        //技能
        List<CSUIMySkill> skillLis = new List<CSUIMySkill>();
        //for (int i = 0; i < 4; i++)
        //{
        //    CSUIMySkill ms = new CSUIMySkill();
        //    ms.name = "npc_big_GerdyHooke";
        //    ms.iconImg = "npc_big_GerdyHooke";
        //    skillLis.Add(ms);
        //}
        CSUIMySkill ms1 = new CSUIMySkill();
        ms1.name = "skill1";
        ms1.iconImg = "npc_big_GerdyHooke";
        skillLis.Add(ms1);
        CSUIMySkill ms2 = new CSUIMySkill();
        ms2.name = "skill2";
        ms2.iconImg = "npc_big_AgnesCopperfield";
        skillLis.Add(ms2);
        CSUIMySkill ms3 = new CSUIMySkill();
        ms3.name = "skill3";
        ms3.iconImg = "npc_big_BenYin";
        skillLis.Add(ms3);
        CSUIMySkill ms4 = new CSUIMySkill();
        ms4.name = "skill4";
        ms4.iconImg = "npc_big_AvaAzniv";
        skillLis.Add(ms4);


        for (int i = 0; i < numbers - 6; i++)//学员
        {
            CSUIMyNpc randomNpc = new CSUIMyNpc();
            randomNpc.OwnSkills = skillLis;
            randomNpc.IsRandom = true;
            randomNpc.HasOccupation = false;
            randomNpc.Health = 90;
            randomNpc.HealthMax = 100;
            randomNpc.Name = "xueyuan";
            randomNpc.RandomNpcFace = linshiyong[i];
            randomNpc.Sex = PeSex.Female;
            if (i == 3)
                randomNpc.State = 10;
            allRandomNpcs.Add(randomNpc);
        }
        for (int i = 4; i < numbers; i++)//教练
        {
            CSUIMyNpc randomNpc = new CSUIMyNpc();
            randomNpc.OwnSkills = skillLis;
            randomNpc.IsRandom = true;
            randomNpc.HasOccupation = true;
            randomNpc.AddHealth = "6~10";
            randomNpc.AddHunger = "3~5";
            randomNpc.Name = "jiaolian";
            randomNpc.RandomNpcFace = linshiyong[i];
            randomNpc.Sex = PeSex.Male;
            allRandomNpcs.Add(randomNpc);
        }
        Debug.Log("生成的allRandomNpcs：" + allRandomNpcs.Count);
    }
}
