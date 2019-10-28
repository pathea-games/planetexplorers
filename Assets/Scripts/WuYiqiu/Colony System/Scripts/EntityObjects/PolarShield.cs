using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using PETools;

[RequireComponent (typeof (SphereCollider))]
public class PolarShield:MonoBehaviour
{
	public GameObject  m_Model;
	public int[] levelSkills = new int[]{0,0,0};
	public float m_Radius;
	public float target_Radius;
	public float min_Radius=35;//CSInfoMgr.m_AssemblyInfo.m_Levels[0].radius;
	public int m_Level;

	
	public int GetSkillID{
		get{return levelSkills[m_Level];}
	}
	static List<PolarShield> allShileds = new List<PolarShield> ();
    List<PeEntity> monsterList = new List<PeEntity>();
   

	private SphereCollider m_Collider;
	private Transform 	   m_Trans;

	public bool IsEmpty;

    public delegate void TriggerEvent(PeEntity peEntity, int skillId);
    public event TriggerEvent onEnterTrigger;
	
	public delegate void TriggerExitEvent(PeEntity peEntity);
	public event TriggerExitEvent onExitTrigger;

    public static bool IsInsidePolarShield(Vector3 pos, int level)
    {
        for (int i = 0; i < allShileds.Count; i++)
        {
            if (allShileds[i].Inside(pos) && allShileds[i].Difference(level) >= 2)
                return true;
        }

        return false;
    }

    public static bool GetPolarShield(Vector3 pos, int level, out Vector3 center, out float radius)
    {
        
        for (int i = 0; i < allShileds.Count; i++)
        {
            if (allShileds[i].Inside(pos) && allShileds[i].Difference(level) >= 2)
            {
                radius = allShileds[i].m_Radius;
                center = allShileds[i].Pos;
                return true;
            }
        }

        radius = 0.0f;
        center = Vector3.zero;
        return false;
    }

    public static Vector3 GetRandomPosition(Vector3 pos, int level)
    {
        for (int i = 0; i < allShileds.Count; i++)
        {
            if (allShileds[i].Inside(pos) && allShileds[i].Difference(level) >= 2)
            {
                return PEUtil.GetRandomPosition(allShileds[i].Pos, pos - allShileds[i].Pos, allShileds[i].m_Radius*1.2f, allShileds[i].m_Radius*1.5f, -75.0f, 75.0f);
            }
        }

        return Vector3.zero;
    }

    public Vector3 Pos 
	{
		get { return m_Trans.position; }
		set { m_Trans.position = value; }
	}
	
	public void ShowModel(bool mShow){
		m_Model.SetActive(mShow);
	}
	
	public void SetLerpRadius(float radius){
		if(m_Radius<radius){
			if(m_Radius<min_Radius){
				m_Radius=min_Radius;
				m_Trans.localScale= new Vector3 (m_Radius*2,m_Radius*2,m_Radius*2);
			}
			target_Radius = radius;
		}
	}

	public void SetRadius(float radius){
		target_Radius = radius;
		m_Radius=target_Radius;
		m_Trans.localScale= new Vector3 (m_Radius*2,m_Radius*2,m_Radius*2);
	}

	public void SetLevel(int level){
		m_Level = level;
	}

    public bool Inside(Vector3 pos)
    {
        return (m_Trans.position - pos).sqrMagnitude <= m_Radius * m_Radius;
    }

    public int Difference (int lv)
    {
        return m_Level + 1 - lv;
    }

	void Awake(){
		m_Collider = GetComponent<Collider>() as SphereCollider;
		m_Collider.isTrigger = true;
		
		m_Trans = transform;
		allShileds.Add(this);

	}

	void Start(){
	}

    int counter=0;
	void Update () 
	{
        counter++;
        if (counter % 24 == 0) {
            List<PeEntity> removeList = monsterList.FindAll(it => it.IsDeath());
            for (int i = removeList.Count - 1; i >= 0; i--)
            {
				if (onExitTrigger != null)
					onExitTrigger(removeList[i]);

				monsterList.RemoveAt(i);
            }
            counter = 0;
        }
		IsEmpty = monsterList.Count==0;

		if(m_Radius<target_Radius)
		{
			m_Radius +=0.2f;
			m_Trans.localScale= new Vector3 (m_Radius*2,m_Radius*2,m_Radius*2);
		}
	}

	void OnTriggerEnter (Collider other)
	{
        PeEntity peEntity = other.GetComponentInParent<PeEntity>();
		if(peEntity==null)
			return;
		if(peEntity.proto!=EEntityProto.Monster)
			return;
        if (monsterList.Contains(peEntity))
			return;

        monsterList.Add(peEntity);
        int skillId = GetSkillID;
        if (onEnterTrigger != null)
            onEnterTrigger(peEntity,skillId);
	}

	void OnTriggerExit (Collider other)
	{
        PeEntity peEntity = other.GetComponentInParent<PeEntity>();
		if(peEntity==null)
			return;
		if(peEntity.proto!=EEntityProto.Monster)
			return;
        if (!monsterList.Contains(peEntity))
			return;
		
		//--to do:cancel skill
		//1.if other shield Contains it
		//--return
		//


        monsterList.Remove(peEntity);
        if (onExitTrigger != null)
            onExitTrigger(peEntity);

	}

    public void AfterUpdate() {
        foreach(PeEntity pe in monsterList ){
			if (onExitTrigger != null)
				onExitTrigger(pe);
        }
        int skillId = GetSkillID;
		foreach (PeEntity peEntity in monsterList) {
			if (onEnterTrigger != null)
				onEnterTrigger(peEntity,skillId);
        }
    }


	void OnDestroy(){
		allShileds.Remove(this);
        foreach(PeEntity pe in monsterList)
			if (onExitTrigger != null)
				onExitTrigger(pe);
	}

}