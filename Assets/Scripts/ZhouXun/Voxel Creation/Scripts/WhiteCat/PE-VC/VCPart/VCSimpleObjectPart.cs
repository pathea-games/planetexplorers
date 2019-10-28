using UnityEngine;

/// <summary>
/// 部件 (功能默认关闭)
/// </summary>
namespace WhiteCat
{
    public abstract class VCSimpleObjectPart : VCPart
    {
		bool init = false;
		Pathea.Operate.PEBed _pePed;
		Pathea.Operate.PEBed PeBed
		{
			get
			{
				if (!init)
				{
					init = true;
					_pePed = GetComponentInParent<Pathea.Operate.PEBed>();
				}
				return _pePed;
			}
		}


		CmdList cmdList = new CmdList();


        public bool CanRecycle()
        {
			return PeBed == null || PeBed.IsIdle();
        }


		public virtual CmdList GetCmdList()
		{
			cmdList.Clear();
			return cmdList;
		}


		public bool CanRotateObject()
		{
			return PeBed == null || PeBed.IsIdle();
		}

    }
}