using Pathea;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public abstract class CSHealth:CSElectric
{
    public List<PeEntity> allPatients = new List<PeEntity>();
    public virtual void RemoveDeadPatient(int npcId)
    {
    }
	public virtual bool IsDoingYou(PeEntity npc){
		return false;
	}
}
