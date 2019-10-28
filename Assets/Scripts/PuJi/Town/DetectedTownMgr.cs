using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Pathea;
using PeMap;

public class DetectedTown
{
    public int campId;
    public Vector3 pos;
    public string name;

    public IntVector2 PosCenter
    {
        get { return new IntVector2(Mathf.RoundToInt(pos.x),Mathf.RoundToInt(pos.z)); }
    }
	public static IntVector2 GetPosCenter(Vector3 pos){
		return new IntVector2(Mathf.RoundToInt(pos.x),Mathf.RoundToInt(pos.z));
	}

    public DetectedTown(Vector3 pos,string name,int campId)
    {
        this.pos = pos;
        this.name = name;
        this.campId = campId;
    }

    public DetectedTown(VArtifactTown vat)
    {
        this.pos = new Vector3(vat.PosCenter.x,0,vat.PosCenter.y);
		this.name = PELocalization.GetString(vat.townNameId);
        this.campId = vat.templateId;
    }
}
public class DetectedTownMgr : Pathea.MonoLikeSingleton<DetectedTownMgr>
{
    //static DetectedTownMgr mInstance;
    //public static DetectedTownMgr Instance
    //{
    //    get
    //    {
    //        if (mInstance == null)
    //            mInstance = new DetectedTownMgr();
    //        return mInstance; }
    //}
    public void RegistAtFirst()
    {
		LabelMgr.Instance.eventor.Subscribe(AddStoryCampByLabel);
    }

    public List<IntVector2> detectedTowns = new List<IntVector2>();
    public Dictionary<IntVector2, DetectedTown> DTownsDict = new Dictionary<IntVector2, DetectedTown>();


    public List<DetectedTown> AllTowns
    {
        get { return DTownsDict.Values.ToList(); }
    }
    public DetectedTown GetTown(IntVector2 posCenter)
    {
        if (DTownsDict.ContainsKey(posCenter))
        {
            return DTownsDict[posCenter];
        }
        return null;
    }

    public delegate void AddOrRemoveDetectedTownResult(IntVector2 PosCenter);
	public  event AddOrRemoveDetectedTownResult AddDetectedTownListener;
	public  event AddOrRemoveDetectedTownResult RemoveDetectedTownListener;
	public  void RegisterAddDetectedTownListener(AddOrRemoveDetectedTownResult listener)
    {
        AddDetectedTownListener += listener;
    }

	public  void UnregisterAddDetectedTownListener(AddOrRemoveDetectedTownResult listener)
    {
        AddDetectedTownListener -= listener;
    }
	
	public  void RegisterRemoveDetectedTownListener(AddOrRemoveDetectedTownResult listener)
	{
		RemoveDetectedTownListener += listener;
	}
	
	public  void UnregisterRemoveDetectedTownListener(AddOrRemoveDetectedTownResult listener)
	{
		RemoveDetectedTownListener -= listener;
	}

	public void AddDetectedTown(VArtifactTown vat)
    {
		if(VArtifactTownManager.Instance.IsCaptured(vat.townId))
			return;
        if (detectedTowns.Contains(vat.PosCenter))
            return;
        detectedTowns.Add(vat.PosCenter);
        DetectedTown dt = new DetectedTown(vat);
        DTownsDict.Add(dt.PosCenter, dt);
        if (AddDetectedTownListener!=null)
            AddDetectedTownListener(dt.PosCenter);
    }
	public void RemoveDetectedTown(VArtifactTown vat)
	{
		if (!detectedTowns.Contains(vat.PosCenter))
			return;
		detectedTowns.Remove(vat.PosCenter);
		DTownsDict.Remove(vat.PosCenter);
		if (RemoveDetectedTownListener!=null)
			RemoveDetectedTownListener(vat.PosCenter);
	}
	//story
	public void AddStoryCampByLabel(object sender, LabelMgr.Args arg)
    {
        if (arg.add == true)
        {
            StaticPoint sp = arg.label as StaticPoint;
            if (sp != null)
            {
                if (sp.campId <= 0)
                    return;
                int campId = sp.campId;
				if(!CampTradeIdData.IsStoryDetectTradeCamp(campId))
					return;
                Vector3 pos = sp.position;
                Camp camp = Camp.GetCamp(campId);
				if (camp == null)
                    return;
                DetectedTown dt = new DetectedTown(pos, camp.Name,campId);
                DTownsDict.Add(dt.PosCenter, dt);
                if (AddDetectedTownListener != null)
                    AddDetectedTownListener(dt.PosCenter);
            }
        }
    }

	//story mission
	public void AddStoryCampByMission(int campId){
		if (campId <= 0|| !CampTradeIdData.IsStoryMissionTradeCamp(campId))
			return;
		Camp camp = Camp.GetCamp(campId);
		if (camp == null)
			return;
		DetectedTown dt = new DetectedTown(camp.Pos, camp.Name,campId);
		DTownsDict.Add(dt.PosCenter, dt);
		if (AddDetectedTownListener != null)
			AddDetectedTownListener(dt.PosCenter);
	}

	//public void RemoveStoryCampByMission(int campId){
	//	if (campId <= 0|| !CampTradeIdData.IsStoryMissionTradeCamp(campId))
	//		return;
	//	Camp camp = Camp.GetCamp(campId);
	//	if (camp == null)
	//		return;
	//	IntVector2 keyPos = DetectedTown.GetPosCenter(camp.Pos);
	//	if(!DTownsDict.ContainsKey(keyPos))
	//		return;
	//	DTownsDict.Remove(keyPos);
	//	if (RemoveDetectedTownListener != null)
	//		RemoveDetectedTownListener(keyPos);
	//}
}

