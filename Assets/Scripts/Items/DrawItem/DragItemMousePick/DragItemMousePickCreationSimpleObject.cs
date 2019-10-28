
public class DragItemMousePickCreationSimpleObject : DragItemMousePickCreation 
{
    WhiteCat.VCSimpleObjectPart mSimpleObjPart;
    WhiteCat.VCSimpleObjectPart simpleObjPart
    {
        get
        {
            if (mSimpleObjPart == null)
            {
                mSimpleObjPart = GetComponent<WhiteCat.VCSimpleObjectPart>();
            }

            return mSimpleObjPart;
        }
    }


	public MousePicker.EPriority overridePriority = MousePicker.EPriority.Level2;


	protected override void OnStart()
	{
		base.OnStart();
		priority = overridePriority;
    }


	protected override void InitCmd(CmdList cmdList)
	{
        //base.InitCmd(cmdList);
        
        if (simpleObjPart.CanRotateObject())
        {
            cmdList.Add("Turn", Turn90Degree);
        }

        if (simpleObjPart.CanRecycle())
        {
            cmdList.Add("Get", OnGetBtn);
        }

        //if (!GameConfig.IsMultiMode)
        //{
			CmdList list = simpleObjPart.GetCmdList();
			for (int i = 0; i < list.count; i++)
			{
				cmdList.Add(list.Get(i));
			}
       // }
	}

    protected override void CheckOperate()
	{
		base.CheckOperate ();

		//if(!GameConfig.IsMultiMode)
		//{
		if(PeInput.Get(PeInput.LogicFunction.InteractWithItem) && CanCmd())
			{
				/*
                if (Operatable() && CheckSleepEnable())
                {
                    DoSleep(12f);
                }
				 * */

				CmdList list = simpleObjPart.GetCmdList();

				if (list.count <= 0)
				{
					return;
				}

				CmdList.Cmd cmd = list.Get(0);
				if (cmd == null)
				{
					return;
				}

				cmd.exe();
			}
		//}
	}

    protected override string tipsText
    {
		get
        {
			/*
            if (Operatable())
            {
                return base.tipsText + "\n" + PELocalization.GetString(8000120);
            }

			return "";
			 * */

			CmdList list = simpleObjPart.GetCmdList();

			if (list.count <= 0)
			{
				return "";
			}

			CmdList.Cmd cmd = list.Get(0);
			if (cmd == null)
			{
				return "";
			}

			return cmd.name + " - [5CB0FF]E[-]";
		}
	}
}
