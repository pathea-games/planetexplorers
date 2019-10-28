using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RandomMapIconMgr
{
	public static Dictionary<Vector3,TownLabel> npcTownLabel = new Dictionary<Vector3,TownLabel>();
	public static Dictionary<Vector3,TownLabel> nativeCampLabel = new Dictionary<Vector3,TownLabel>();
	public static Dictionary<Vector3,TownLabel> destroyedTownLabel = new Dictionary<Vector3, TownLabel>();
	public static bool HasTownLabel(Vector3 pos){
		PeMap.ILabel pl = PeMap.LabelMgr.Instance.Find(item =>
		                                               {
			TownLabel l = item as TownLabel;
			if (l != null && l.pos == pos)
				return true;
			return false;
		});
		if(pl!=null)
			return true;
		return false;
	}
    //public static void AddTownIconList(List<Vector3> posList)
    //{
    //    if (posList != null && posList.Count > 0)
    //    {
    //        foreach (Vector3 pos in posList)
    //        {
    //            NpcTownLabel tl = new NpcTownLabel(pos);
    //        }
    //    }
    //}
    //public static void AddTownIconList(Vector3[] posList)
    //{
    //    if (posList != null && posList.Length > 0)
    //    {
    //        foreach (Vector3 pos in posList)
    //        {
    //            NpcTownLabel tl = new NpcTownLabel(pos);
    //        }
    //    }
    //}
    public static void AddTownIcon(VArtifactTown vat)
    {
        if (vat != null)
        {
			if(vat.isEmpty)
				return;
			if(!NpcTownLabel.ContainsIcon(vat.TransPos))
			{
				UnknownLabel.Remove(vat.TransPos);
				NpcTownLabel tl = new NpcTownLabel(vat);
				if(!npcTownLabel.ContainsKey(tl.pos))
					npcTownLabel.Add(tl.pos,tl);
			}
        }
    }

	public static void AddNativeIcon(VArtifactTown vat)
	{
		if (vat != null)
		{
			if (vat.isEmpty)
				return;
			if (!NativeLabel.ContainsIcon(vat.TransPos))
			{
				UnknownLabel.Remove(vat.TransPos);
				NativeLabel nl = new NativeLabel(vat);
				if (!nativeCampLabel.ContainsKey(nl.pos))
					nativeCampLabel.Add(nl.pos, nl);
			}
		}
	}

	public static void DestroyTownIcon(VArtifactTown vat)
	{
		if (vat != null)
		{
			if (vat.type == VArtifactType.NpcTown)
				npcTownLabel.Remove(vat.TransPos);
			else
				nativeCampLabel.Remove(vat.TransPos);
			TownLabel.Remove(vat.TransPos);
			if (!DestroyedLabel.ContainsIcon(vat.TransPos))
			{
				UnknownLabel.Remove(vat.TransPos);
				DestroyedLabel dl = new DestroyedLabel(vat);
				if (!destroyedTownLabel.ContainsKey(dl.pos))
					destroyedTownLabel.Add(dl.pos, dl);
			}
		}
	}

	public static void AddDestroyedTownIcon(VArtifactTown vat)
	{
		if (vat != null)
		{
			if (vat.isEmpty)
				return;
			if (!DestroyedLabel.ContainsIcon(vat.TransPos))
			{
				UnknownLabel.Remove(vat.TransPos);
				DestroyedLabel dl = new DestroyedLabel(vat);
				if (!destroyedTownLabel.ContainsKey(dl.pos))
					destroyedTownLabel.Add(dl.pos, dl);
			}
		}
	}

    //public static void AddNativeIconList(Vector3 pos, NativeType type)
    //{
    //    NativeLabel nl = new NativeLabel(pos, type);
    //}

	public static void ClearAll(){
		TownLabel[] tls = npcTownLabel.Values.ToArray();
		TownLabel[] nls = nativeCampLabel.Values.ToArray();
		TownLabel[] dls = destroyedTownLabel.Values.ToArray();
		PeMap.LabelMgr.Instance.RemoveAll(it=>tls.Contains(it)||nls.Contains(it)||dls.Contains(it));

		npcTownLabel =new Dictionary<Vector3,TownLabel>();
		nativeCampLabel = new Dictionary<Vector3,TownLabel>();
		destroyedTownLabel = new Dictionary<Vector3, TownLabel>();
	}

}
public enum TownLabelType{
	NpcTown=0,
	NativeCamp,
	Unknown,
	Destroyed,
	Colony
}

public abstract class TownLabel : PeMap.ILabel
{
	public int allyId;
	public int allyColorId;
    public Vector3 pos;
    public string name;
	public TownLabelType townLabelType;
    public Vector3 GetPos()
    {
        return pos;
    }

    public new PeMap.ELabelType GetType()
    {
        return PeMap.ELabelType.Mark;
    }

    public bool NeedArrow()
    {
        return false;
    }

    public float GetRadius()
    {
        return -1;
    }

	public PeMap.EShow GetShow()
    {
        return PeMap.EShow.BigMap;
    }

    public abstract int GetIcon();

	public virtual string GetText(){
		return name;
	}

    public abstract bool FastTravel();
	public virtual int GetColor(){
		return 0;
	}
    //lz-2016.08.31 获取联盟颜色，0为白色
    public virtual int GetAllianceColor()
    {
		return allyColorId;
	}

    //lz-2016.08.31 获取友好度， -1:不显示(不显示友好度说明联盟被摧毁) 0:差  1:一般 2:好
    public virtual int GetFriendlyLevel()
    {
        return -1;
    }
	public static bool Remove(Vector3 pos)
	{
		PeMap.ILabel pl = PeMap.LabelMgr.Instance.Find(item =>
		                                               {
			TownLabel l = item as TownLabel;
			if (l != null && l.pos == pos)
				return true;
			return false;
		});
		if(pl!=null)
			return PeMap.LabelMgr.Instance.Remove(pl);
		return false;
	}
	#region lz-2016.06.02;pugee-2016-11-4 15:01:54
    public virtual bool CompareTo(PeMap.ILabel label)
    {
        if (label is TownLabel)
        {
            TownLabel townlabel = (TownLabel)label;
			if(townLabelType==townlabel.townLabelType){
				if (null != this.name&&null!=townlabel.name)
				{
					return (this.pos == townlabel.pos && this.name.Equals(townlabel.name));
				}
				else if(this.name==null&&townlabel.name==null)
				{
					return (this.pos == townlabel.pos);
				}else
					return false;
			}else
				return false;
            
        }
        else
            return false;
    }
    #endregion
}
public class NpcTownLabel : TownLabel
{
    public NpcTownLabel(VArtifactTown vat)
    {
		this.pos = vat.TransPos;
		townLabelType = TownLabelType.NpcTown;
		name =PELocalization.GetString(vat.townNameId);
		allyId = vat.allyId;
		allyColorId = vat.AllyColorId;
        PeMap.LabelMgr.Instance.Add(this);
    }
    public override int GetIcon()
    {
        return PeMap.MapIcon.AdventureCamp;
    }

    public override bool FastTravel()
    {
		return allyId==TownGenData.PlayerAlly;
    }
	public static bool ContainsIcon(Vector3 pos){
		PeMap.ILabel pl = PeMap.LabelMgr.Instance.Find(item =>
		                                               {
			NpcTownLabel l = item as NpcTownLabel;
			if (l != null && l.pos == pos)
				return true;
			return false;
		});
		if(pl!=null)
			return true;
		return false;
	}

}

public enum NativeType
{
    Puja=0,
    Paja=1
}

public class NativeLabel : TownLabel
{
    NativeType type;

    public NativeLabel(VArtifactTown vat)
    {
        this.pos = vat.TransPos;
        this.type = vat.nativeType;
		townLabelType = TownLabelType.NativeCamp;
		if(type==NativeType.Puja)
			name = PELocalization.GetString(8000737);//lz-2016.10.11 翻译【Puja Camp】
        else
			name = PELocalization.GetString(8000736);//lz-2016.10.11 翻译【Paja Camp】
		allyId = vat.allyId;
//		name = PELocalization.GetString(VATownGenerator.Instance.GetAllyName(allyId));
		allyColorId = vat.AllyColorId;
        PeMap.LabelMgr.Instance.Add(this);
    }
    public override int GetIcon()
    {
        switch (type)
        {
            case NativeType.Puja:
                return PeMap.MapIcon.PujaBase;
        }
        return PeMap.MapIcon.PajaBase;
    }

    public override string GetText()
    {
        return name;
    }

    public override bool FastTravel()
    {
        return false;
	}
	public static bool ContainsIcon(Vector3 pos){
		PeMap.ILabel pl = PeMap.LabelMgr.Instance.Find(item =>
		                                               {
			NativeLabel l = item as NativeLabel;
			if (l != null && l.pos == pos)
				return true;
			return false;
		});
		if(pl!=null)
			return true;
		return false;
	}
}
public class UnknownLabel : TownLabel
{
    public UnknownLabel(Vector3 pos)
    {
        this.pos = pos;
		name = PELocalization.GetString(8000695);
		townLabelType = TownLabelType.Unknown;
        PeMap.LabelMgr.Instance.Add(this);
    }
    public override int GetIcon()
    {
        return PeMap.MapIcon.unknownCamp;
    }

    public override bool FastTravel()
    {
        return false;
    }

    public static new void Remove(Vector3 pos)
    {
        PeMap.ILabel pl = PeMap.LabelMgr.Instance.Find(item =>
        {
            UnknownLabel l = item as UnknownLabel;
            if (l != null && l.pos == pos)
                return true;
            return false;
        });

        if (pl == null)
            return;
        PeMap.LabelMgr.Instance.Remove(pl);
    }
    public static bool HasMark(Vector3 pos)
    {
        return null!= PeMap.LabelMgr.Instance.Find(item =>
        {
            UnknownLabel l = item as UnknownLabel;
            if(l!=null&&l.pos==pos)
                return true;
            return false;
        });
    }
    public static void AddMark(Vector3 pos)
    {
        /*UnknownLabel ul = */new UnknownLabel(pos);
    }
}
public class DestroyedLabel : TownLabel
{
	VArtifactType townType;
	NativeType nativeType;
	public DestroyedLabel(VArtifactTown vat)
	{
		this.pos = vat.TransPos;
		name =PELocalization.GetString(vat.townNameId);
		townType = vat.type;
		nativeType = vat.nativeType;
		allyId = vat.allyId;
		allyColorId = vat.AllyColorId;
		PeMap.LabelMgr.Instance.Add(this);
	}
	public override int GetIcon()
	{
		if(townType == VArtifactType.NpcTown)
			return PeMap.MapIcon.HumanBrokenBase;
		else if(nativeType==NativeType.Puja)
			return PeMap.MapIcon.PujaBrokenBase;
		else
			return PeMap.MapIcon.PajaBrokenBase;
	}
	
	public override bool FastTravel()
	{
		return true;
	}
	public static bool ContainsIcon(Vector3 pos){
		PeMap.ILabel pl = PeMap.LabelMgr.Instance.Find(item =>
		                                               {
			DestroyedLabel l = item as DestroyedLabel;
			if (l != null && l.pos == pos)
				return true;
			return false;
		});
		if(pl!=null)
			return true;
		return false;
	}
}


public class ColonyLabel : TownLabel
{
    public ColonyLabel(Vector3 pos)
    {
		this.pos = pos;
		name = PELocalization.GetString(8000735);
		townLabelType = TownLabelType.Colony;
        PeMap.LabelMgr.Instance.Add(this);
    }
    public override int GetIcon()
    {
        return PeMap.MapIcon.PlayerBase;
    }

    public override bool FastTravel()
    {
        return true;
    }
    public static new bool Remove(Vector3 pos)
    {
        PeMap.ILabel pl = PeMap.LabelMgr.Instance.Find(item =>
        {
            ColonyLabel l = item as ColonyLabel;
            if (l != null && l.pos == pos)
                return true;
            return false;
        });
		if(pl!=null)
        	return PeMap.LabelMgr.Instance.Remove(pl);
        return false;
    }
	public static bool ContainsIcon(Vector3 pos){
		PeMap.ILabel pl = PeMap.LabelMgr.Instance.Find(item =>
		                                               {
			ColonyLabel l = item as ColonyLabel;
			if (l != null && l.pos == pos)
				return true;
			return false;
		});
		if(pl!=null)
			return true;
		return false;
	}
}

public class DungeonEntranceLabel : PeMap.ILabel
{
	Vector3 pos;
	string name;
	public DungeonEntranceLabel(Vector3 pos)
	{
		this.pos = pos;
		name = PELocalization.GetString(8000889);
		PeMap.LabelMgr.Instance.Add(this);
	}
	public Vector3 GetPos()
	{
		return pos;
	}
	
	public new PeMap.ELabelType GetType()
	{
		return PeMap.ELabelType.Mark;
	}
	
	public bool NeedArrow()
	{
		return false;
	}
	
	public float GetRadius()
	{
		return -1;
	}
	
	public PeMap.EShow GetShow()
	{
		return PeMap.EShow.BigMap;
	}


	public int GetIcon()
	{
		return PeMap.MapIcon.RandomDungeonEntrance;
	}
	
	public string GetText()
	{
		return name;
	}
	
	public bool FastTravel()
	{
		return false;
	}

	public static bool Remove(Vector3 pos)
	{
		PeMap.ILabel pl = PeMap.LabelMgr.Instance.Find(item =>
		                                               {
			DungeonEntranceLabel l = item as DungeonEntranceLabel;
			if (l != null && l.pos == pos)
				return true;
			return false;
		});
		if(pl!=null)
			return PeMap.LabelMgr.Instance.Remove(pl);
		return false;
	}

	public virtual bool CompareTo(PeMap.ILabel label)
	{
		if (label is DungeonEntranceLabel)
		{
			DungeonEntranceLabel dLabel = (DungeonEntranceLabel)label;
			if (null != this.name&&null!=dLabel.name)
			{
				return (this.pos == dLabel.pos && this.name.Equals(dLabel.name));
			}
			else
			{
				return (this.pos == dLabel.pos);
			}
		}
		else
			return false;
	}
}