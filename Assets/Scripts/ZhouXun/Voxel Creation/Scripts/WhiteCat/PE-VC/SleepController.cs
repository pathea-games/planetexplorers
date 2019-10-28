using WhiteCat;
using UnityEngine;
using Pathea;
using Pathea.Operate;

public class SleepController : MonoBehaviour, FastTravelMgr.IEnable
{
	OperateCmpt character;
	PESleep peSleep;
	float maxSleepTime;
	float timeCount;
	float actionTime;
	bool isMainPlayer;

	public static void StartSleep(PESleep peSleep, PeEntity character, float hours = 12)
	{
		if (null != character)
		{
			SleepController sc = character.GetComponent<SleepController>();
			if (sc == null)
			{
				sc = character.gameObject.AddComponent<SleepController>();
				sc.isMainPlayer = Pathea.PeCreature.Instance.mainPlayer == character;

				if (sc.isMainPlayer)
				{
					MotionMgrCmpt mmc = character.GetCmpt<MotionMgrCmpt>();
					Action_Sleep actionSleep = mmc.GetAction<Action_Sleep>();
					actionSleep.startSleepEvt += sc.MainPlayerStartSleep;
					actionSleep.startSleepEvt += (i) => FastTravelMgr.Instance.Add(sc);
					actionSleep.endSleepEvt += sc.MainPlayerEndSleep;
					actionSleep.endSleepEvt += (i) => FastTravelMgr.Instance.Remove(sc);
				}
			}

			sc.character = character.GetComponent<OperateCmpt>();
			sc.maxSleepTime = hours * 3600f;
			sc.timeCount = 0;
			sc.peSleep = peSleep;
			sc.enabled = true;
			sc.actionTime = 2f;
			peSleep.StartOperate(sc.character, EOperationMask.Sleep);
//			peSleep.Do(sc.character);
		}
	}


	void MainPlayerStartSleep(int buffID)
	{
		MousePicker.Instance.enable = false;
		GameUI.Instance.mItemOp.ShowSleepingUI(() => { return 1f - (timeCount / maxSleepTime); });

        if (!PeGameMgr.IsMulti)
        {
			GameTime.Timer.ElapseSpeedBak = GameTime.Timer.ElapseSpeed;
            GameTime.Timer.ElapseSpeed = PEVCConfig.instance.sleepTimeScale;
            GameTime.AbnormalityTimePass = true;
        }
	}


	void MainPlayerEndSleep(int buffID)
	{
		MousePicker.Instance.enable = true;
		GameUI.Instance.mItemOp.HideSleepingUI();

        if (!PeGameMgr.IsMulti)
        {
			if(GameTime.Timer.ElapseSpeedBak >= 0f){
				GameTime.Timer.ElapseSpeed = GameTime.Timer.ElapseSpeedBak;
				GameTime.Timer.ElapseSpeedBak = -1f;
			}
            GameTime.AbnormalityTimePass = false;
        }
		peSleep = null;
	}


	void Update()
	{
		actionTime -= Time.deltaTime;
		timeCount += GameTime.DeltaTime;
		if (timeCount >= maxSleepTime || (isMainPlayer && actionTime < 0
		                                  && (PeInput.Get(PeInput.LogicFunction.InteractWithItem) || EntityMonsterBeacon.IsRunning())))
		{
			if(peSleep != null){
				peSleep.StopOperate(character, EOperationMask.Sleep);
//				peSleep.UnDo(character);
			}
			enabled = false;
		}
	}

	bool FastTravelMgr.IEnable.Enable()
	{
		return peSleep == null;
	}
}
