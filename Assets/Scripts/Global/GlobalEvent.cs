using UnityEngine;
using System.Collections;

public class GlobalEvent : MonoBehaviour 
{
	static GlobalEvent mInstance;
	public static GlobalEvent Instance{ get { return mInstance;} }
	void Awake()
	{
		mInstance = this;
	}
	
	void OnDestroy()
	{
		OnPlayerGetOnTrain = null;
		OnPlayerGetOffTrain = null;
		OnPowerPlantStateChanged = null;
		OnMouseUnlock = null;
	}
	
	public delegate void VoidFunc();
	public delegate void IndexBaseFunc(int index);
	public delegate void PositionBaseFunc(Vector3 positon);
	
	public static event IndexBaseFunc OnPlayerGetOnTrain;
	public static void PlayerGetOnTrain(int index)
	{
		if(null != OnPlayerGetOnTrain)
			OnPlayerGetOnTrain(index);
	}
	
	public static event PositionBaseFunc OnPlayerGetOffTrain;
	public static void PlayerGetOffTrain(Vector3 position)
	{
		if(null != OnPlayerGetOffTrain)
			OnPlayerGetOffTrain(position);
	}

	public static event VoidFunc OnPowerPlantStateChanged;
	public static void NoticePowerPlantStateChanged()
	{
		if (null != OnPowerPlantStateChanged)
			OnPowerPlantStateChanged();
	}
	
	public static event VoidFunc OnMouseUnlock;
	public static void NoticeMouseUnlock()
	{
		if(null != OnMouseUnlock)
			OnMouseUnlock();
	}
}
