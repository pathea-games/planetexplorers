using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public abstract class CSWorkerMachine:CSElectric
{
	public virtual void RecountCounter(){

	}
    public override bool AddWorker (CSPersonnel npc)
	{
		bool flag = base.AddWorker (npc);

		RecountCounter();
		return flag;
	}
	public override void RemoveWorker (CSPersonnel npc)
	{
		base.RemoveWorker (npc);
		RecountCounter();
	}
}