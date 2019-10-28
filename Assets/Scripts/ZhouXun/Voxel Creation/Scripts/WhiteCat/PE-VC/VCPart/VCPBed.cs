using UnityEngine;
using Pathea.Operate;
using Pathea;

namespace WhiteCat
{
	public class VCPBed : VCSimpleObjectPart
	{
		public PESleep sleepPivot;


		public override CmdList GetCmdList()
		{
			var list = base.GetCmdList();
			if(sleepPivot.CanOperateMask(EOperationMask.Sleep))
			{
				list.Add("Sleep", ()=>
                    {
                        if (!sleepPivot.CanOperateMask(EOperationMask.Sleep))
                        {
                            return;
                        }

						//SleepController.StartSleep(sleepPivot, MainPlayer.Instance.entity);
						//GameUI.Instance.mItemOp.HideMainUI();
                        GameUI.Instance.mItemOp.ShowSleepWnd(true,this,sleepPivot, Pathea.MainPlayer.Instance.entity);
                    });
			}
			return list;
		}


		//public void DoSleep(float gameTimeInHour)
		//{

		//	if (!IsSleeping())
		//	{
		//		bSleeping = true;

		//		float trueTime = gameTimeInHour + 6f;

		//		mOriginTimeElapseSpeed = GameTime.Timer.ElapseSpeed;
		//		GameTime.Timer.ElapseSpeed = (gameTimeInHour * 3600F / trueTime);

		//		GameTime.AbnormalityTimePass = true;
		//		Invoke("EndSleep", trueTime);

		//		sleepPivot.Do(playerOperate);

		//		MousePicker.Instance.enable = false;
		//	}

		//	GameUI.Instance.mItemOp.Hide();
		//}


		//public void EndSleep()
		//{
		//	if (sleepPivot == null)
		//	{
		//		return;
		//	}

		//	if (IsSleeping())
		//	{
		//		bSleeping = false;

		//		GameTime.Timer.ElapseSpeed = mOriginTimeElapseSpeed;

		//		GameTime.AbnormalityTimePass = false;

		//		sleepPivot.UnDo(playerOperate);

		//		MousePicker.Instance.enable = true;
		//	}
		//}


		//bool CheckSleepEnable()
		//{
		//	if (GameTime.AbnormalityTimePass)
		//	{
		//		MessageBox_N.ShowOkBox(UIMsgBoxInfo.CannotSleep.GetString());
		//		return false;
		//	}

		//	if (SPAutomatic.IsSpawning())
		//	{
		//		MessageBox_N.ShowOkBox(PELocalization.GetString(8000083));
		//		return false;
		//	}
		//	return true;
		//}


		//bool bSleeping = false;
		//bool IsSleeping()
		//{
		//	return bSleeping;
		//}


		//protected void Awake()
		//{
		//	GameTime.OnCancelTimePass += EndSleep;
		//}


		//protected void OnDestroy()
		//{
		//	GameTime.OnCancelTimePass -= EndSleep;
		//}


		//Pathea.OperateCmpt mPlayerOperate;
		//Pathea.OperateCmpt playerOperate
		//{
		//	get
		//	{
		//		if (mPlayerOperate == null)
		//		{
		//			Pathea.PeEntity mainPlayer = Pathea.MainPlayer.Instance.entity;
		//			if (mainPlayer != null)
		//			{
		//				mPlayerOperate = mainPlayer.GetCmpt<Pathea.OperateCmpt>();
		//			}
		//		}
		//		return mPlayerOperate;
		//	}
		//}
	}
}
