using ItemAsset;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FarmPlantLogic : DragItemLogic, ISaveDataInScene
{
    GameObject _mainGo;
    public int mPlantInstanceId
    {
        set { itemDrag = ItemMgr.Instance.Get(value).GetCmpt<Drag>(); }
        get { return itemDrag.itemObj.instanceId; }
    }

    public int protoTypeId{
        get { return itemDrag.itemObj.protoId; }
    }
	int					mTypeID;
    public Vector3 mPos
    {
        get { return transform.position; }
    }
	public double		mPutOutGameTime;
	public double		mLife;
	public double		mWater;
	public double		mClean;
	public bool			mDead;
	public int			mGrowTimeIndex;
	
	public double		mCurGrowTime;
	public byte			mTerrianType;
	public float	    mGrowRate = 1;
	public float        mExtraGrowRate = 0.0f;
	public float		mNpcGrowRate=0;
	
	public PlantInfo	mPlantInfo;
	public Bounds       mPlantBounds;
	public double		mLastUpdateTime;
    public double       mNextUpdateTime;
	bool				mInit = false;
	

	public bool			NeedWater;
	public bool			NeedClean;
	public bool			IsRipe;

    //double OneDaySecond = 93600.0;
	// Plant Event Listenser
	public const int cEvent_NeedWater = 1;
	public const int cEvent_NoNeedWater = 2;
	public const int cEvent_NeedClean = 3;
	public const int cEvent_NoNeedClean = 4;
	public const int cEvent_Dead = 5;
	public const int cEvent_Ripe = 6;
	public delegate void EventDel(FarmPlantLogic plant, int event_type);
	private static event EventDel EventListener;

	const double VarPerOp = 30.0;
	const int Version000 = 20150409;
	const int Version = 20160222;//
	public const int Version001 = 2016110100;
	public int curVersion =Version001;
    //const float cMaxGrowTimePerDay = 93600;
    //const float cMinGrowTimePerDay = 18720;

    const float LifeUp = 20;//per day
    const float LifeDeMin = 0;
    const float LifeDeMax = 40;
    const double LifeMax = 100; 


	public static void RegisterEventListener(EventDel eventListener)
	{
		EventListener += eventListener;
	}
	public static void UnregisterEventListener(EventDel eventListener)
	{
		EventListener -= eventListener;
	}

	public static void ExcuteEventListener(FarmPlantLogic plant, int event_type)
	{
		if (EventListener != null)
			EventListener(plant, event_type);
	}

	public int	_PlantType
	{
		get{return mTypeID;}
		set
		{
			mTypeID = value;
			mPlantInfo = PlantInfo.GetInfo(mTypeID);
			mPlantBounds = PlantInfo.GetPlantBounds(protoTypeId,mPos);
		}
	}

	public void InitGrowRate(float extraRate){
		float base_rate = 0f;
		switch (mTerrianType)
		{
		case PlantConst.DIRTY_TYPE0:
			base_rate = 1.0f;
			break;
		case PlantConst.DIRTY_TYPE1:
			base_rate = 1.3f;
			break;
		default:
			base_rate = 1.0f;
			break;
		}

		mGrowRate = base_rate + extraRate+mNpcGrowRate;
		mExtraGrowRate = extraRate;
	}

	public void UpdateGrowRate(float extraRate, bool bReset = true)
	{
		UpdateStatus();
		float base_rate = 0f;
		switch (mTerrianType)
		{
		case PlantConst.DIRTY_TYPE0:
			base_rate = 1.0f;
			break;
		case PlantConst.DIRTY_TYPE1:
			base_rate = 1.3f;
			break;
		default:
			base_rate = 1.0f;
			break;
		}

		if (bReset)
		{
			mGrowRate = base_rate + extraRate+mNpcGrowRate;
			mExtraGrowRate = extraRate;
		}
		else
		{
			mGrowRate = base_rate + mExtraGrowRate + extraRate+mNpcGrowRate;
			mExtraGrowRate += extraRate;
		}
		UpdateStatus();
	}

	public void UpdateNpcGrowRate(float npcRate){
		UpdateStatus();
		float base_rate = 0f;
		switch (mTerrianType)
		{
		case PlantConst.DIRTY_TYPE0:
			base_rate = 1.0f;
			break;
		case PlantConst.DIRTY_TYPE1:
			base_rate = 1.3f;
			break;
		default:
			base_rate = 1.0f;
			break;
		}

		mGrowRate = base_rate + mExtraGrowRate+ npcRate;
		mNpcGrowRate = npcRate;
		UpdateStatus();
	}

	public float GetRipePercent()
	{
		float lastGrowTime = (float)mCurGrowTime;
        float currentGrowTime = (float)(GameTime.Timer.Second - mLastUpdateTime) * mGrowRate + lastGrowTime;
		float totalGrowTime = (float)mPlantInfo.mGrowTime[mPlantInfo.mGrowTime.Length - 1];
        return Mathf.Clamp(currentGrowTime / totalGrowTime, 0, 1);
	}

	public int GetWaterItemCount ()
    {
        UpdateStatus();
		int needNum = (int)((mPlantInfo.mWaterLevel[1] - mWater)/VarPerOp);
		return needNum;
	}

	public void GetRain(){
		if(NeedWater)
		{
			UpdateStatus();
			int needNum = (int)((mPlantInfo.mWaterLevel[1] - mWater)/VarPerOp);
			if(needNum>0)
			{
				mWater += VarPerOp * needNum; 
				UpdateStatus();
			}
		}
	}

	public void Watering(int water_count)
    {
        UpdateStatus();
        mWater += VarPerOp * water_count; 
        UpdateStatus();
	}

	public int GetCleaningItemCount ()
    {
        UpdateStatus();
		int needNum = (int)((mPlantInfo.mCleanLevel[1] - mClean)/VarPerOp);
		return needNum;
	}

	public void Cleaning(int weeding_count)
    {
        UpdateStatus();
        mClean += VarPerOp * weeding_count;
        UpdateStatus();
	}

	public int GetHarvestItemNum (float harvestAbility=1)
    {
        UpdateStatus();
		int itemGetNum = (int)(((int)(mLife / 20f) + 1) * 0.2f * mPlantInfo.mItemGetNum*harvestAbility);
		return itemGetNum;
	}

	public Dictionary<int, int> GetHarvestItemIds (int itemGetNum)
	{
		Dictionary<int, int> retItems = new Dictionary<int, int>();
		for (int i = 0; i < itemGetNum; i++)
		{
			float randomVar = UnityEngine.Random.Range(0f, 1f);
			for (int j = 0; j < mPlantInfo.mItemGetPro.Count; j++)
			{
				if (randomVar < mPlantInfo.mItemGetPro[j].m_probablity)
				{
					if (!retItems.ContainsKey(mPlantInfo.mItemGetPro[j].m_id))
						retItems[mPlantInfo.mItemGetPro[j].m_id] = 0;
					retItems[mPlantInfo.mItemGetPro[j].m_id] += 1;
				}
			}
		}
		return retItems;
	}

    public int GetHarvestItemNumMax()
    {
        int itemGetNum = (int)(((int)(LifeMax / 20f) + 1) * 0.2f * mPlantInfo.mItemGetNum);
        return itemGetNum;
    }
    public Dictionary<int, int> GetHarvestItemIdsMax(int itemGetNum)
    {
        Dictionary<int, int> retItems = new Dictionary<int, int>();
        for (int i = 0; i < itemGetNum; i++)
        {
            float randomVar = 0;
            for (int j = 0; j < mPlantInfo.mItemGetPro.Count; j++)
            {
                if (randomVar < mPlantInfo.mItemGetPro[j].m_probablity)
                {
                    if (!retItems.ContainsKey(mPlantInfo.mItemGetPro[j].m_id))
                        retItems[mPlantInfo.mItemGetPro[j].m_id] = 0;
                    retItems[mPlantInfo.mItemGetPro[j].m_id] += 1;
                }
            }
        }
        return retItems;
    }
    #region update event
    
    public void UpdateStatus()
    {
		if (mDead || IsRipe)
		{
			mNextUpdateTime = -1;
			return;
		}
		double nowSecond = GameTime.Timer.Second;
        double timePassedSecond = nowSecond - mLastUpdateTime;
        double timePassedDay = timePassedSecond / GameTime.Timer.Day2Sec;
        mLastUpdateTime = nowSecond;

        UpdateLife(timePassedDay);
        UpdateGrowTime(timePassedSecond);
        UpdateGrowIndex();
        UpdateEvent();
        UpdateModel();
        if (mDead || IsRipe)
        {
            mNextUpdateTime = -1;
        }
        else
        {
            double nextTime = CountNextUpdateTime();
            mNextUpdateTime = mLastUpdateTime + nextTime;
        }
    }
    /// <summary>
    /// unit: day
    /// </summary>
    /// <param name="timePassed"></param>
    public void UpdateWater(double timePassedDay)
    {
        mWater -= timePassedDay * mPlantInfo.mWaterDS;
    }

    public void UpdateClean(double timePassedDay)
    {
        mClean -= timePassedDay * mPlantInfo.mCleanDS;
    }

    public void UpdateGrowTime(double timePassedSecond)
    {
        if(!mDead){
            mCurGrowTime += timePassedSecond * mGrowRate;
        }
    }

    public void UpdateGrowIndex()
    {
        if (!mDead)
        {
            while ((mGrowTimeIndex < mPlantInfo.mGrowTime.Length)
                   && mCurGrowTime > mPlantInfo.mGrowTime[mGrowTimeIndex])
            {
                mGrowTimeIndex++;
            }
        }
    }

    /// <summary>
    /// this will update water and clean at the same time
    /// </summary>
    /// <param name="timePassedDay"></param>
    public void UpdateLife(double timePassedDay)
    {
        double nextWaterTime = CountNextWatertime();
        double nextCleanTime = CountNextCleanTime();
        bool stage2 = false;
        bool stage3 = false;

        double minTime;
        #region stage1
        //1.stage 1, both:T1-T2
        if (nextWaterTime >= 0 && nextCleanTime >= 0)
        {
            minTime = nextWaterTime > nextCleanTime ? nextCleanTime : nextWaterTime;
            //1)timePassed
            if (minTime>=timePassedDay)
            {
                minTime = timePassedDay;
                timePassedDay = 0;
            }
            else
            {
                timePassedDay -= minTime;
            }
            //2)update the data , time pass minTime
            //water
            mWater -= mPlantInfo.mWaterDS * minTime;
            mWater = mWater < 0 ? 0 : mWater;
            //clean
            mClean -= mPlantInfo.mCleanDS * minTime;
            mClean = mClean < 0 ? 0 : mClean;
            //life
            mLife += LifeUp * minTime * 2;
            if (mLife > 100)
            {
                mLife = 100;
            }
            stage2 = true;
        }
        else if (nextWaterTime >= 0 && nextCleanTime < -2)
        {
            minTime = nextWaterTime;

            if (minTime>=timePassedDay)
            {
                minTime = timePassedDay;
                timePassedDay = 0;
            }
            else
            {
                timePassedDay -= minTime;
            }

            mWater -= mPlantInfo.mWaterDS * minTime;
            mWater = mWater < 0 ? 0 : mWater;

            mClean -= mPlantInfo.mCleanDS * minTime;
            mClean = mClean < 0 ? 0 : mClean;

            mLife += LifeUp * minTime * 2;
            if (mLife > 100)
            {
                mLife = 100;
            }
            stage2 = true;
        }
        #endregion

        if (timePassedDay < PETools.PEMath.Epsilon)
        {
            return;
        }

        #region stage2
        //2.stage 2, 0=<A<T1,T1<=B<=T2,A!=0; 
        //1) water is A
        if (stage2||(nextWaterTime < nextCleanTime && nextWaterTime > -2 && nextCleanTime > -2))
        {
            double waterTo0 = mWater / mPlantInfo.mWaterDS;
            double cleanToT1 = (mClean - mPlantInfo.mCleanLevel[0]) / mPlantInfo.mCleanDS;
            //double waterLifeDeStart = (LifeDeMax - LifeDeMin) *(mPlantInfo.mWaterLevel[0]- waterValue) / mPlantInfo.mWaterLevel[0] + LifeDeMin;
            if (waterTo0 >= cleanToT1)
            {
                //in this period ,  life is up then down
                minTime = cleanToT1;
                if (minTime >= timePassedDay)
                {
                    minTime = timePassedDay;
                    timePassedDay = 0;
                }
                else
                {
                    timePassedDay -= minTime;
                }
                //double waterLifeDeEnd = (LifeDeMax - LifeDeMin) * (mPlantInfo.mWaterLevel[0] - (waterValue - minTime * mPlantInfo.mWaterDS)) / mPlantInfo.mWaterLevel[0] + LifeDeMin;
                double lifeWater = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mWaterLevel[0] * 2 - mWater * 2 + minTime * mPlantInfo.mWaterDS) / mPlantInfo.mWaterLevel[0]) + LifeDeMin * 2) * minTime / 2;
                //double lifeWater = (waterLifeDeStart + waterLifeDeEnd) * minTime / 2;
                double lifeClean = LifeUp * minTime;
                if (mLife + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    //return the time that make lifeValue=0
                    //the formula: lifeValue -  (lifeDeMax-lifeDeMin)* minTime * mPlantInfo.mWaterDS/mPlantInfo.mWaterLevel[0]/2* t^2 
                    //-(LifeDeMax-LifeDeMin)*((mPlantInfo.mWaterLevel[0]-waterValue)/ mPlantInfo.mWaterLevel[0]+ lifeDeMin)* t + LifeUp* t=0
                    //double a = -(LifeDeMax - LifeDeMin) * mPlantInfo.mWaterDS / mPlantInfo.mWaterLevel[0] / 2;
                    //double b = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mWaterLevel[0] - mWater) / mPlantInfo.mWaterLevel[0] - LifeDeMin + LifeUp;
                    //double c = mLife;
                    //double t = (-b + Mathf.Sqrt((float)(b * b - 4 * a * c))) / (2 * a);
                    mLife = 0;
                    mDead = true;
                    return;
                }
                else
                {
                    //update the data , time pass minTime
                    mWater -= mPlantInfo.mWaterDS * minTime;
                    mWater = mWater < 0 ? 0 : mWater;
                    //clean
                    mClean -= mPlantInfo.mCleanDS * minTime;
                    mClean = mClean < 0 ? 0 : mClean;
                    //life
                    mLife += lifeWater + lifeClean;
                    if (mLife > 100)
                    {
                        mLife = 100;
                    }
                }
            }
            else
            {//waterTo0<CleanToT1
                double minTime1 = waterTo0;
                if (minTime1 >= timePassedDay)
                {
                    minTime1 = timePassedDay;
                    timePassedDay = 0;
                }
                else
                {
                    timePassedDay -= minTime1;
                }
                //step1 minTime1
                double lifeWater = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mWaterLevel[0] * 2 - mWater * 2 + minTime1 * mPlantInfo.mWaterDS) / mPlantInfo.mWaterLevel[0]) + LifeDeMin * 2) * minTime1 / 2;
                double lifeClean = LifeUp * minTime1;
                if (mLife + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    //double a = -(LifeDeMax - LifeDeMin) * mPlantInfo.mWaterDS / mPlantInfo.mWaterLevel[0] / 2;
                    //double b = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mWaterLevel[0] - mWater) / mPlantInfo.mWaterLevel[0] - LifeDeMin + LifeUp;
                    //double c = mLife;
                    //double t = (-b + Mathf.Sqrt((float)(b * b - 4 * a * c))) / (2 * a);
                    mLife = 0;
                    mDead = true;
                    return;
                }
                else
                {
                    //update the data , time pass minTime
                    mWater -= mPlantInfo.mWaterDS * minTime1;
                    mWater = mWater < 0 ? 0 : mWater;
                    //clean
                    mClean -= mPlantInfo.mCleanDS * minTime1;
                    mClean = mClean < 0 ? 0 : mClean;
                    //life
                    mLife += lifeWater + lifeClean;
                    if (mLife > 100)
                    {
                        mLife = 100;
                    }
                }

                if (timePassedDay < PETools.PEMath.Epsilon)
                {
                    return;
                }

                double minTime2 = cleanToT1 - waterTo0;

                if (minTime2 >= timePassedDay)
                {
                    minTime2 = timePassedDay;
                    timePassedDay = 0;
                }
                else
                {
                    timePassedDay -= minTime2;
                }

                //step2 minTime2,water is 0
                lifeWater = -LifeDeMax * minTime2;
                lifeClean = LifeUp * minTime2;
                if (mLife + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    //double t = mLife / (LifeDeMax - LifeUp);
                    //surviveTime += t;
                    //return surviveTime;
                    mLife = 0;
                    mDead = true;
                    return;
                }
                else
                {
                    //update the data , time pass minTime
                    mWater -= mPlantInfo.mWaterDS * minTime2;
                    mWater = mWater < 0 ? 0 : mWater;
                    //clean
                    mClean -= mPlantInfo.mCleanDS * minTime2;
                    mClean = mClean < 0 ? 0 : mClean;
                    //life
                    mLife += lifeWater + lifeClean;
                    if (mLife > 100)
                    {
                        mLife = 100;
                    }
                }
            }
            stage3 = true;
        }
        else if (nextWaterTime >= nextCleanTime && nextWaterTime != -2 && nextCleanTime > -2)
        {
            double cleanTo0 = mClean / mPlantInfo.mCleanDS;
            double waterToT1 = (mWater - mPlantInfo.mWaterLevel[0]) / mPlantInfo.mWaterDS;
            //double waterLifeDeStart = (LifeDeMax - LifeDeMin) *(mPlantInfo.mWaterLevel[0]- waterValue) / mPlantInfo.mWaterLevel[0] + LifeDeMin;
            if (cleanTo0 >= waterToT1)
            {
                //in this period ,  life is up then down
                minTime = waterToT1;
                if (minTime >= timePassedDay)
                {
                    minTime = timePassedDay;
                    timePassedDay = 0;
                }
                else
                {
                    timePassedDay -= minTime;
                }
                //double waterLifeDeEnd = (LifeDeMax - LifeDeMin) * (mPlantInfo.mWaterLevel[0] - (waterValue - minTime * mPlantInfo.mWaterDS)) / mPlantInfo.mWaterLevel[0] + LifeDeMin;
                double lifeClean = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mCleanLevel[0] * 2 - mClean * 2 + minTime * mPlantInfo.mCleanDS) / mPlantInfo.mCleanLevel[0]) + LifeDeMin * 2) * minTime / 2;
                //double lifeWater = (waterLifeDeStart + waterLifeDeEnd) * minTime / 2;
                double lifeWater = LifeUp * minTime;
                if (mLife + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    //double a = -(LifeDeMax - LifeDeMin) * mPlantInfo.mCleanDS / mPlantInfo.mCleanLevel[0] / 2;
                    //double b = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mCleanLevel[0] - mClean) / mPlantInfo.mCleanLevel[0] - LifeDeMin + LifeUp;
                    //double c = mLife;
                    //double t = (-b + Mathf.Sqrt((float)(b * b - 4 * a * c))) / (2 * a);
                    //surviveTime += t;
                    //return surviveTime;
                    mLife = 0;
                    mDead = true;
                    return;
                }
                else
                {
                    //update the data , time pass minTime
                    mWater -= mPlantInfo.mWaterDS * minTime;
                    mWater = mWater < 0 ? 0 : mWater;
                    //clean
                    mClean -= mPlantInfo.mCleanDS * minTime;
                    mClean = mClean < 0 ? 0 : mClean;
                    //life
                    mLife += lifeWater + lifeClean;
                    if (mLife > 100)
                    {
                        mLife = 100;
                    }
                }
            }
            else
            {
                //CleanTo0<waterToT1
                double minTime1 = cleanTo0;

                if (minTime1 >= timePassedDay)
                {
                    minTime1 = timePassedDay;
                    timePassedDay = 0;
                }
                else
                {
                    timePassedDay -= minTime1;
                }
                //step1 minTime1
                double lifeClean = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mCleanLevel[0] * 2 - mClean * 2 + minTime1 * mPlantInfo.mCleanDS) / mPlantInfo.mCleanLevel[0]) + LifeDeMin * 2) * minTime1 / 2;
                double lifeWater = LifeUp * minTime1;
                if (mLife + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    //double a = -(LifeDeMax - LifeDeMin) * mPlantInfo.mCleanDS / mPlantInfo.mCleanLevel[0] / 2;
                    //double b = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mCleanLevel[0] - mClean) / mPlantInfo.mCleanLevel[0] - LifeDeMin + LifeUp;
                    //double c = mLife;
                    //double t = (-b + Mathf.Sqrt((float)(b * b - 4 * a * c))) / (2 * a);
                    //surviveTime += t;
                    //return surviveTime;
                    mLife = 0;
                    mDead = true;
                    return;
                }
                else
                {
                    //update the data , time pass minTime
                    mWater -= mPlantInfo.mWaterDS * minTime1;
                    mWater = mWater < 0 ? 0 : mWater;
                    //clean
                    mClean -= mPlantInfo.mCleanDS * minTime1;
                    mClean = mClean < 0 ? 0 : mClean;
                    //life
                    mLife += lifeWater + lifeClean;
                    if (mLife > 100)
                    {
                        mLife = 100;
                    }
                }

                if (timePassedDay < PETools.PEMath.Epsilon)
                {
                    return;
                }
                //step2 minTime2,clean is 0
                double minTime2 = waterToT1 - cleanTo0;
                if (minTime2 >= timePassedDay)
                {
                    minTime2 = timePassedDay;
                    timePassedDay = 0;
                }
                else
                {
                    timePassedDay -= minTime2;
                }
                lifeClean = -LifeDeMax * minTime2;
                lifeWater = LifeUp * minTime2;
                if (mLife + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    //double t = mLife / (LifeDeMax - LifeUp);
                    //surviveTime += t;
                    //return surviveTime;
                    mLife = 0;
                    mDead = true;
                    return;
                }
                else
                {
                    //update the data , time pass minTime
                    mWater -= mPlantInfo.mWaterDS * minTime2;
                    mWater = mWater < 0 ? 0 : mWater;
                    //clean
                    mClean -= mPlantInfo.mCleanDS * minTime2;
                    mClean = mClean < 0 ? 0 : mClean;
                    //life
                    mLife += lifeWater + lifeClean; 
                    if (mLife > 100)
                    {
                        mLife = 100;
                    }
                }
            }
            stage3 = true;
        }
        else if (nextCleanTime < -2)
        {
            if (nextWaterTime < 0)
            {
                double waterTo0 = mWater / mPlantInfo.mWaterDS;
                if (waterTo0 > 0)
                {
                    minTime = waterTo0;
                    if (minTime >= timePassedDay)
                    {
                        minTime = timePassedDay;
                        timePassedDay = 0;
                    }
                    else
                    {
                        timePassedDay -= minTime;
                    }
                    double lifeWater = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mWaterLevel[0] * 2 - mWater * 2 + minTime * mPlantInfo.mWaterDS) / mPlantInfo.mWaterLevel[0]) + LifeDeMin * 2) * minTime / 2;
                    double lifeClean = LifeUp * minTime;
                    if (mLife + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                    {
                        //double a = -(LifeDeMax - LifeDeMin) * mPlantInfo.mWaterDS / mPlantInfo.mWaterLevel[0] / 2;
                        //double b = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mWaterLevel[0] - mWater) / mPlantInfo.mWaterLevel[0] - LifeDeMin + LifeUp;
                        //double c = mLife;
                        //double t = (-b + Mathf.Sqrt((float)(b * b - 4 * a * c))) / (2 * a);
                        //surviveTime += t;
                        //return surviveTime;
                        mLife = 0;
                        mDead = true;
                        return;
                    }
                    else
                    {
                        //update the data , time pass minTime
                        mWater -= mPlantInfo.mWaterDS * minTime;
                        mWater = mWater < 0 ? 0 : mWater;
                        //clean
                        mClean -= mPlantInfo.mCleanDS * minTime;
                        mClean = mClean < 0 ? 0 : mClean;
                        //life
                        mLife += lifeWater + lifeClean;
                        if (mLife > 100)
                        {
                            mLife = 100;
                        }
                    }
                }

                if (timePassedDay < PETools.PEMath.Epsilon)
                {
                    return;
                }

                mLife -= (LifeDeMax - LifeUp) * timePassedDay;
                if (mLife <= PETools.PEMath.Epsilon)
                {
                    mLife = 0;
                    mDead = true;
                }
                return;
                //double timeTmp = mLife / (LifeDeMax - LifeUp);
                //surviveTime += timeTmp;
                //return surviveTime;
            }
            stage3 = true;
        }
        else
        {
            //not exist
        }
        #endregion

        if (timePassedDay < PETools.PEMath.Epsilon)
        {
            return;
        }

        #region stage3
        //3.stage 3, 0<=A<T1,0<=B<T2
        if (stage3||(nextWaterTime < 0 && nextCleanTime < 0))
        {
            //1)neither 0
            if (mWater > 0 && mClean > 0)
            {
                double waterTimeTo0 = mWater / mPlantInfo.mWaterDS;
                double cleanTimeTo0 = mClean / mPlantInfo.mCleanDS;
                if (waterTimeTo0 > cleanTimeTo0)
                {
                    minTime = cleanTimeTo0;
                }
                else
                {
                    minTime = waterTimeTo0;
                }

                if (minTime >= timePassedDay)
                {
                    minTime = timePassedDay;
                    timePassedDay = 0;
                }
                else
                {
                    timePassedDay -= minTime;
                }

                double lifeWater = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mWaterLevel[0] * 2 - mWater * 2 + minTime * mPlantInfo.mWaterDS) / mPlantInfo.mWaterLevel[0]) + LifeDeMin * 2) * minTime / 2;
                double lifeClean = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mCleanLevel[0] * 2 - mClean * 2 + minTime * mPlantInfo.mCleanDS) / mPlantInfo.mCleanLevel[0]) + LifeDeMin * 2) * minTime / 2;
                if (mLife + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    //double a = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mWaterDS / mPlantInfo.mWaterLevel[0] + mPlantInfo.mCleanDS / mPlantInfo.mCleanLevel[0]) / 2;
                    //double b = -(LifeDeMax - LifeDeMin) * ((mPlantInfo.mWaterLevel[0] - mWater) / mPlantInfo.mWaterLevel[0] + (mPlantInfo.mCleanLevel[0] - mClean) / mPlantInfo.mCleanLevel[0]) - LifeDeMin * 2;
                    //double c = mLife;
                    //double t = (-b + Mathf.Sqrt((float)(b * b - 4 * a * c))) / (2 * a);
                    //surviveTime += t;
                    //return surviveTime;
                    mLife = 0;
                    mDead = true;
                    return;
                }
                else
                {
                    //update the data , time pass minTime
                    mWater -= mPlantInfo.mWaterDS * minTime;
                    mWater = mWater < 0 ? 0 : mWater;
                    //clean
                    mClean -= mPlantInfo.mCleanDS * minTime;
                    mClean = mClean < 0 ? 0 : mClean;
                    //life
                    mLife += lifeWater + lifeClean;
                    if (mLife > 100)
                    {
                        mLife = 100;
                    }
                }
            }
            if (timePassedDay < PETools.PEMath.Epsilon)
            {
                return;
            }
            //2)one is 0
            if (mWater > 0 && mClean <= PETools.PEMath.Epsilon)
            {
                minTime = mWater / mPlantInfo.mWaterDS;
                if (minTime >= timePassedDay)
                {
                    minTime = timePassedDay;
                    timePassedDay = 0;
                }
                else
                {
                    timePassedDay -= minTime;
                }
                double lifeWater = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mWaterLevel[0] * 2 - mWater * 2 + minTime * mPlantInfo.mWaterDS) / mPlantInfo.mWaterLevel[0]) + LifeDeMin * 2) * minTime / 2;
                double lifeClean = -LifeDeMax * minTime;
                if (mLife + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    //double a = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mWaterDS / mPlantInfo.mWaterLevel[0]) / 2;
                    //double b = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mWaterLevel[0] - mWater) / mPlantInfo.mWaterLevel[0] - LifeDeMin - LifeDeMax;
                    //double c = mLife;
                    //double t = (-b + Mathf.Sqrt((float)(b * b - 4 * a * c))) / (2 * a);
                    //surviveTime += t;
                    //return surviveTime;
                    mLife = 0;
                    mDead = true;
                }
                else
                {
                    //update the data , time pass minTime
                    mWater -= mPlantInfo.mWaterDS * minTime;
                    mWater = mWater < 0 ? 0 : mWater;
                    //clean
                    mClean -= mPlantInfo.mCleanDS * minTime;
                    mClean = mClean < 0 ? 0 : mClean;
                    //life
                    mLife += lifeWater + lifeClean;
                    if (mLife > 100)
                    {
                        mLife = 100;
                    }
                }
            }
            else if (mWater <= PETools.PEMath.Epsilon && mClean > 0)
            {
                minTime = mClean / mPlantInfo.mCleanDS; 
                if (minTime >= timePassedDay)
                {
                    minTime = timePassedDay;
                    timePassedDay = 0;
                }
                else
                {
                    timePassedDay -= minTime;
                }
                double lifeClean = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mCleanLevel[0] * 2 - mClean * 2 + minTime * mPlantInfo.mCleanDS) / mPlantInfo.mCleanLevel[0]) + LifeDeMin * 2) * minTime / 2;
                double lifeWater = -LifeDeMax * minTime;
                if (mLife + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    //double a = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mCleanDS / mPlantInfo.mCleanLevel[0]) / 2;
                    //double b = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mCleanLevel[0] - mClean) / mPlantInfo.mCleanLevel[0] - LifeDeMin - LifeDeMax;
                    //double c = mLife;
                    //double t = (-b + Mathf.Sqrt((float)(b * b - 4 * a * c))) / (2 * a);
                    //surviveTime += t;
                    //return surviveTime;
                    mLife = 0;
                    mDead = true;
                    return;
                }
                else
                {
                    //update the data , time pass minTime
                    mWater -= mPlantInfo.mWaterDS * minTime;
                    mWater = mWater < 0 ? 0 : mWater;
                    //clean
                    mClean -= mPlantInfo.mCleanDS * minTime;
                    mClean = mClean < 0 ? 0 : mClean;
                    //life
                    mLife += lifeWater + lifeClean;
                    if (mLife > 100)
                    {
                        mLife = 100;
                    }
                }
            }

            if (timePassedDay < PETools.PEMath.Epsilon)
            {
                return;
            }

            //3)both 0
            //double finalTime = mLife / (LifeDeMax * 2);
            mLife -= LifeDeMax * 2 * timePassedDay;
            if (mLife < PETools.PEMath.Epsilon)
            {
                mLife = 0;
                mDead = true;
            }
            return;
        }
        #endregion

    }

    public void UpdateEvent()
    {
        if (mDead)
        {
            ExcuteEventListener(this, cEvent_Dead);
        }
        bool oldNeedWater = NeedWater;
        NeedWater = mWater < mPlantInfo.mWaterLevel[0];
        if (NeedWater != oldNeedWater)
        {
            int type = NeedWater ? cEvent_NeedWater : cEvent_NoNeedWater;
            ExcuteEventListener(this, type);
        }

        bool oldNeedClean = NeedClean;
        NeedClean = mClean < mPlantInfo.mCleanLevel[0];
        if (NeedClean != oldNeedClean)
        {
            int type = NeedClean ? cEvent_NeedClean : cEvent_NoNeedClean;
            ExcuteEventListener(this, type);
        }

        bool oldRipe = IsRipe;
        IsRipe = (mGrowTimeIndex == mPlantInfo.mGrowTime.Length);
        if (oldRipe != IsRipe && IsRipe)
            ExcuteEventListener(this, cEvent_Ripe);
    }

    public void UpdateModel()
    {
		if(gameObject==null)
			return;
        ItemScript_Plant itemScript = GetComponentInChildren<ItemScript_Plant>();
        if (itemScript != null)
        {
            itemScript.UpdatModel();
        }
    }
    #endregion

    #region countNext
    /// <summary>
    /// get the recent event time second
    /// </summary>
    /// <returns></returns>
    public double CountNextUpdateTime()
    {
        if (!mDead && !IsRipe)
        {
            double nextSecond = 0;

            //--to do:
            double nextWaterTime = CountNextWatertime();
            double nextCleanTime = CountNextCleanTime();
            if (nextWaterTime < 0 && nextCleanTime > 0 )
            {
                nextSecond = nextCleanTime;
            }
            else if (nextCleanTime < 0 && nextWaterTime > 0)
            {
                nextSecond = nextWaterTime;
            }
            else if (nextCleanTime > 0 && nextWaterTime > 0)
            {
                nextSecond = (nextWaterTime > nextCleanTime ? nextCleanTime : nextWaterTime);
            }
            else
            {
                nextSecond = -1;
            }
			nextSecond *= GameTime.Timer.Day2Sec;
            double nextGrowthTime = CountNextGrowTime();//second
            if (nextGrowthTime < 0)
            {
                return -2;//isRipe
            }
            if ( nextGrowthTime < nextSecond || nextSecond < 0 )
            {
                nextSecond = nextGrowthTime;
            }
            if (!(nextWaterTime > 0 && nextCleanTime > 0))
            {
				double nextDeadTime = CountNextDeadTime()*GameTime.Timer.Day2Sec;
               if (nextDeadTime < nextSecond)
               {
                   nextSecond = nextDeadTime;
               }
            }
        
            return nextSecond;
        }
        if (mDead)
            return -1;
        return -2;//IsRipe
    }

    //unit: day
    public double CountNextWatertime()
    {
        //double dePerSecond = mPlantInfo.mWaterDS/PeEnv.NovaSettings.SecondsPerDay;
        if (mWater <= mPlantInfo.mWaterLevel[0])
        {
            return -1;
        }
        else
        {
            double volumeLeft = mWater - mPlantInfo.mWaterLevel[0];
            double timeLeft = volumeLeft / mPlantInfo.mWaterDS;
            return timeLeft;
        }
    }

    //unit: day
    public double CountNextCleanTime()
    {
        if (mPlantInfo.mCleanLevel[0]==0)
        {
            return -4;//infinity
        }
        //double dePerSecond = mPlantInfo.mCleanDS / PeEnv.NovaSettings.SecondsPerDay;
        if (mClean <= mPlantInfo.mCleanLevel[0])
        {
            return -1;
        }
        else
        {
            double volumeLeft = mClean - mPlantInfo.mCleanLevel[0];
            double dayLeft = volumeLeft / mPlantInfo.mCleanDS;
            return dayLeft;
        }
    }

    //unit: second
    public double CountNextGrowTime()
    {
        if (mCurGrowTime < mPlantInfo.mGrowTime[0])
        {
            return (mPlantInfo.mGrowTime[0]-mCurGrowTime)/mGrowRate;
        }else if(mCurGrowTime >= mPlantInfo.mGrowTime[0] && mCurGrowTime <mPlantInfo.mGrowTime[1])
        {
            return (mPlantInfo.mGrowTime[1] - mCurGrowTime) / mGrowRate;
        }
        else if (mCurGrowTime >= mPlantInfo.mGrowTime[1] && mCurGrowTime < mPlantInfo.mGrowTime[2])
        {
            return (mPlantInfo.mGrowTime[2] - mCurGrowTime) / mGrowRate;
        }
        return -1;
    }

    //unit: day
    public double CountNextDeadTime()
    {
        double surviveTime = 0;
        double nextWaterTime = CountNextWatertime();
        double nextCleanTime = CountNextCleanTime();
        bool stage2 = false;
        bool stage3 = false;
        //data now
        double waterValue = mWater;
        double cleanValue = mClean;
        double lifeValue = mLife;

        double minTime;
        //1.stage 1, both:T1-T2
        if(nextWaterTime>=0&&nextCleanTime>=0)
        {
            minTime = nextWaterTime > nextCleanTime ? nextCleanTime : nextWaterTime;
            //1)surviveTime
            surviveTime += minTime;
            //2)update the data , time pass minTime
            //water
            waterValue -= mPlantInfo.mWaterDS * minTime;
            waterValue = waterValue < 0 ? 0 : waterValue;
            //clean
            cleanValue -= mPlantInfo.mCleanDS * minTime;
            cleanValue = cleanValue < 0 ? 0 : cleanValue;
            //life
            lifeValue += LifeUp * minTime * 2;
            if (lifeValue > 100)
            {
                lifeValue = 100;
            }
            stage2 = true;
        }
        else if (nextWaterTime >= 0 && nextCleanTime < -2)
        {
            minTime = nextWaterTime;

            surviveTime += minTime;

            waterValue -= mPlantInfo.mWaterDS * minTime;
            waterValue = waterValue < 0 ? 0 : waterValue;

            cleanValue -= mPlantInfo.mCleanDS * minTime;
            cleanValue = cleanValue < 0 ? 0 : cleanValue;

            lifeValue += LifeUp * minTime * 2;
            if (lifeValue > 100)
            {
                lifeValue = 100;
            }
            stage2 = true;
        }

        //2.stage 2, 0=<A<T1,T1<=B<=T2,A!=0; 
        //1) water is A
        if (stage2||(nextWaterTime < nextCleanTime && nextWaterTime>-2 && nextCleanTime > -2))
        {
            double waterTo0 = waterValue / mPlantInfo.mWaterDS;
            double cleanToT1 = (cleanValue - mPlantInfo.mCleanLevel[0]) / mPlantInfo.mCleanDS;
            //double waterLifeDeStart = (LifeDeMax - LifeDeMin) *(mPlantInfo.mWaterLevel[0]- waterValue) / mPlantInfo.mWaterLevel[0] + LifeDeMin;
            if (waterTo0 >= cleanToT1)
            {
                //in this period ,  life is up then down
                minTime = cleanToT1;
                //double waterLifeDeEnd = (LifeDeMax - LifeDeMin) * (mPlantInfo.mWaterLevel[0] - (waterValue - minTime * mPlantInfo.mWaterDS)) / mPlantInfo.mWaterLevel[0] + LifeDeMin;
                double lifeWater = -((LifeDeMax-LifeDeMin)*((mPlantInfo.mWaterLevel[0]*2-waterValue*2+ minTime * mPlantInfo.mWaterDS)/mPlantInfo.mWaterLevel[0])+LifeDeMin*2)*minTime/2;
                //double lifeWater = (waterLifeDeStart + waterLifeDeEnd) * minTime / 2;
                double lifeClean = LifeUp * minTime;
                if (lifeValue + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    //return the time that make lifeValue=0
                    //the formula: lifeValue -  (lifeDeMax-lifeDeMin)* minTime * mPlantInfo.mWaterDS/mPlantInfo.mWaterLevel[0]/2* t^2 
                    //-(LifeDeMax-LifeDeMin)*((mPlantInfo.mWaterLevel[0]-waterValue)/ mPlantInfo.mWaterLevel[0]+ lifeDeMin)* t + LifeUp* t=0
                    double a= -(LifeDeMax-LifeDeMin)* mPlantInfo.mWaterDS/mPlantInfo.mWaterLevel[0]/2;
                    double b = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mWaterLevel[0] - waterValue) / mPlantInfo.mWaterLevel[0] - LifeDeMin + LifeUp;
                    double c= lifeValue;
                    double t = GetLargerResult(a,b,c);
                    surviveTime += t;
                    return surviveTime;
                }
                else
                {
                    //update the data , time pass minTime
                    waterValue -= mPlantInfo.mWaterDS * minTime;
                    waterValue = waterValue < 0 ? 0 : waterValue;
                    //clean
                    cleanValue -= mPlantInfo.mCleanDS * minTime;
                    cleanValue = cleanValue < 0 ? 0 : cleanValue;
                    //life
                    lifeValue += lifeWater + lifeClean;

                    surviveTime += minTime;
                }
            }
            else
            {//waterTo0<CleanToT1
                double minTime1 = waterTo0;
                double minTime2 = cleanToT1 - waterTo0;
                //step1 minTime1
                double lifeWater = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mWaterLevel[0] * 2 - waterValue * 2 + minTime1 * mPlantInfo.mWaterDS) / mPlantInfo.mWaterLevel[0]) + LifeDeMin * 2) * minTime1 / 2;
                double lifeClean = LifeUp * minTime1;
                if (lifeValue + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    double a = -(LifeDeMax - LifeDeMin) * mPlantInfo.mWaterDS / mPlantInfo.mWaterLevel[0] / 2;
                    double b = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mWaterLevel[0] - waterValue) / mPlantInfo.mWaterLevel[0] - LifeDeMin + LifeUp;
                    double c = lifeValue;
                    double t = GetLargerResult(a,b,c);
                    surviveTime += t;
                    return surviveTime;
                }
                else
                {
                    //update the data , time pass minTime
                    waterValue -= mPlantInfo.mWaterDS * minTime1;
                    waterValue = waterValue < 0 ? 0 : waterValue;
                    //clean
                    cleanValue -= mPlantInfo.mCleanDS * minTime1;
                    cleanValue = cleanValue < 0 ? 0 : cleanValue;
                    //life
                    lifeValue += lifeWater + lifeClean;

                    surviveTime += minTime1;
                }
                //step2 minTime2,water is 0
                lifeWater = -LifeDeMax*minTime2;
                lifeClean = LifeUp * minTime2;
                if (lifeValue + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    double t = lifeValue /(LifeDeMax-LifeUp);
                    surviveTime +=t;
                    return surviveTime;
                }
                else
                {
                    //update the data , time pass minTime
                    waterValue -= mPlantInfo.mWaterDS * minTime2;
                    waterValue = waterValue < 0 ? 0 : waterValue;
                    //clean
                    cleanValue -= mPlantInfo.mCleanDS * minTime2;
                    cleanValue = cleanValue < 0 ? 0 : cleanValue;
                    //life
                    lifeValue += lifeWater + lifeClean;

                    surviveTime += minTime2;
                }
            }
            stage3 = true;
        }
        else if (nextWaterTime >= nextCleanTime && nextWaterTime!=-2&&nextCleanTime > -2)
        {
            double cleanTo0 = cleanValue / mPlantInfo.mCleanDS;
            double waterToT1 = (waterValue - mPlantInfo.mWaterLevel[0]) / mPlantInfo.mWaterDS;
            //double waterLifeDeStart = (LifeDeMax - LifeDeMin) *(mPlantInfo.mWaterLevel[0]- waterValue) / mPlantInfo.mWaterLevel[0] + LifeDeMin;
            if (cleanTo0 >= waterToT1)
            {
                //in this period ,  life is up then down
                minTime = waterToT1;
                //double waterLifeDeEnd = (LifeDeMax - LifeDeMin) * (mPlantInfo.mWaterLevel[0] - (waterValue - minTime * mPlantInfo.mWaterDS)) / mPlantInfo.mWaterLevel[0] + LifeDeMin;
                double lifeClean = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mCleanLevel[0] * 2 - cleanValue * 2 + minTime * mPlantInfo.mCleanDS) / mPlantInfo.mCleanLevel[0]) + LifeDeMin * 2) * minTime / 2;
                //double lifeWater = (waterLifeDeStart + waterLifeDeEnd) * minTime / 2;
                double lifeWater = LifeUp * minTime;
                if (lifeValue + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    double a = -(LifeDeMax - LifeDeMin) * mPlantInfo.mCleanDS / mPlantInfo.mCleanLevel[0] / 2;
                    double b = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mCleanLevel[0] - cleanValue) / mPlantInfo.mCleanLevel[0] - LifeDeMin + LifeUp;
                    double c = lifeValue;
                    double t = GetLargerResult(a,b,c);
                    surviveTime += t;
                    return surviveTime;
                }
                else
                {
                    //update the data , time pass minTime
                    waterValue -= mPlantInfo.mWaterDS * minTime;
                    waterValue = waterValue < 0 ? 0 : waterValue;
                    //clean
                    cleanValue -= mPlantInfo.mCleanDS * minTime;
                    cleanValue = cleanValue < 0 ? 0 : cleanValue;
                    //life
                    lifeValue += lifeWater + lifeClean;

                    surviveTime += minTime;
                }
            }
            else
            {//CleanTo0<waterToT1
                double minTime1 = cleanTo0;
                double minTime2 = waterToT1 - cleanTo0;
                //step1 minTime1
                double lifeClean = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mCleanLevel[0] * 2 - cleanValue * 2 + minTime1 * mPlantInfo.mCleanDS) / mPlantInfo.mCleanLevel[0]) + LifeDeMin * 2) * minTime1 / 2;
                double lifeWater = LifeUp * minTime1;
                if (lifeValue + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    double a = -(LifeDeMax - LifeDeMin) * mPlantInfo.mCleanDS / mPlantInfo.mCleanLevel[0] / 2;
                    double b = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mCleanLevel[0] - cleanValue) / mPlantInfo.mCleanLevel[0] - LifeDeMin + LifeUp;
                    double c = lifeValue;
                    double t = GetLargerResult(a,b,c);
                    surviveTime += t;
                    return surviveTime;
                }
                else
                {
                    //update the data , time pass minTime
                    waterValue -= mPlantInfo.mWaterDS * minTime1;
                    waterValue = waterValue < 0 ? 0 : waterValue;
                    //clean
                    cleanValue -= mPlantInfo.mCleanDS * minTime1;
                    cleanValue = cleanValue < 0 ? 0 : cleanValue;
                    //life
                    lifeValue += lifeWater + lifeClean;
                    surviveTime += minTime1;
                }
                //step2 minTime2,clean is 0
                lifeClean = -LifeDeMax * minTime2;
                lifeWater = LifeUp * minTime2;
                if (lifeValue + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    double t = lifeValue / (LifeDeMax - LifeUp);
                    surviveTime += t;
                    return surviveTime;
                }
                else
                {
                    //update the data , time pass minTime
                    waterValue -= mPlantInfo.mWaterDS * minTime2;
                    waterValue = waterValue < 0 ? 0 : waterValue;
                    //clean
                    cleanValue -= mPlantInfo.mCleanDS * minTime2;
                    cleanValue = cleanValue < 0 ? 0 : cleanValue;
                    //life
                    lifeValue += lifeWater + lifeClean;
                    surviveTime += minTime2;
                }
            }
            stage3 = true;
        }
        else if (nextCleanTime < -2)
        {
            if (nextWaterTime < 0)
            {
                double waterTo0 = waterValue / mPlantInfo.mWaterDS;
                if (waterTo0 > 0)
                {
                    minTime = waterTo0;
                    double lifeWater = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mWaterLevel[0] * 2 - waterValue * 2 + minTime * mPlantInfo.mWaterDS) / mPlantInfo.mWaterLevel[0]) + LifeDeMin * 2) * minTime / 2;
                    double lifeClean = LifeUp * minTime;
                    if (lifeValue + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                    {
                        double a = -(LifeDeMax - LifeDeMin) * mPlantInfo.mWaterDS / mPlantInfo.mWaterLevel[0] / 2;
                        double b = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mWaterLevel[0] - waterValue) / mPlantInfo.mWaterLevel[0] - LifeDeMin + LifeUp;
                        double c = lifeValue;
                        double t = GetLargerResult(a,b,c);
                        surviveTime += t;
                        return surviveTime;
                    }
                    else
                    {
                        //update the data , time pass minTime
                        waterValue -= mPlantInfo.mWaterDS * minTime;
                        waterValue = waterValue < 0 ? 0 : waterValue;
                        //clean
                        cleanValue -= mPlantInfo.mCleanDS * minTime;
                        cleanValue = cleanValue < 0 ? 0 : cleanValue;
                        //life
                        lifeValue += lifeWater + lifeClean;
                        surviveTime += minTime;
                    }
                }
                double timeTmp = lifeValue / (LifeDeMax - LifeUp);
                surviveTime += timeTmp;
                return surviveTime;
            }
            stage3 = true;
        }
        else
        {
            //not exist
        }


        //3.stage 3, 0<=A<T1,0<=B<T2
        if (stage3||(nextWaterTime < 0 && nextCleanTime < 0))
        {
            //1)neither 0
            if(waterValue>0&&cleanValue>0)
            {
                double waterTimeTo0 = waterValue / mPlantInfo.mWaterDS;
                double cleanTimeTo0 = cleanValue / mPlantInfo.mCleanDS;
                if (waterTimeTo0 > cleanTimeTo0)
                {
                    minTime = cleanTimeTo0;
                }
                else
                {
                    minTime = waterTimeTo0;
                }
                double lifeWater = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mWaterLevel[0] * 2 - waterValue * 2 + minTime * mPlantInfo.mWaterDS) / mPlantInfo.mWaterLevel[0]) + LifeDeMin * 2) * minTime / 2;
                double lifeClean = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mCleanLevel[0] * 2 - cleanValue * 2 + minTime * mPlantInfo.mCleanDS) / mPlantInfo.mCleanLevel[0]) + LifeDeMin * 2) * minTime / 2;
                if (lifeValue + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    double a = -(LifeDeMax - LifeDeMin) *(mPlantInfo.mWaterDS / mPlantInfo.mWaterLevel[0]+mPlantInfo.mCleanDS/mPlantInfo.mCleanLevel[0]) / 2;
                    double b = -(LifeDeMax - LifeDeMin) * ((mPlantInfo.mWaterLevel[0] - waterValue) / mPlantInfo.mWaterLevel[0]+(mPlantInfo.mCleanLevel[0]-cleanValue)/mPlantInfo.mCleanLevel[0]) - LifeDeMin*2;
                    double c = lifeValue;
                    double t = GetLargerResult(a,b,c); 
                    surviveTime += t;
                    return surviveTime;
                }
                else
                {
                    //update the data , time pass minTime
                    waterValue -= mPlantInfo.mWaterDS * minTime;
                    waterValue = waterValue < 0 ? 0 : waterValue;
                    //clean
                    cleanValue -= mPlantInfo.mCleanDS * minTime;
                    cleanValue = cleanValue < 0 ? 0 : cleanValue;
                    //life
                    lifeValue += lifeWater + lifeClean;
                    surviveTime += minTime;
                }
            }
            //2)one is 0
            if(waterValue>0&&cleanValue<=PETools.PEMath.Epsilon)
            {
                minTime = waterValue / mPlantInfo.mWaterDS;
                double lifeWater = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mWaterLevel[0] * 2 - waterValue * 2 + minTime * mPlantInfo.mWaterDS) / mPlantInfo.mWaterLevel[0]) + LifeDeMin * 2) * minTime / 2;
                double lifeClean = -LifeDeMax * minTime;
                if (lifeValue + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    double a = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mWaterDS / mPlantInfo.mWaterLevel[0]) / 2;
                    double b = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mWaterLevel[0] - waterValue) / mPlantInfo.mWaterLevel[0] - LifeDeMin - LifeDeMax;
                    double c = lifeValue;
                    double t = GetLargerResult(a,b,c);
                    surviveTime += t;
                    return surviveTime;
                }
                else
                {
                    //update the data , time pass minTime
                    waterValue -= mPlantInfo.mWaterDS * minTime;
                    waterValue = waterValue < 0 ? 0 : waterValue;
                    //clean
                    cleanValue -= mPlantInfo.mCleanDS * minTime;
                    cleanValue = cleanValue < 0 ? 0 : cleanValue;
                    //life
                    lifeValue += lifeWater + lifeClean;
                    surviveTime += minTime;
                }
            }
            else if (waterValue <= PETools.PEMath.Epsilon && cleanValue > 0)
            {
                minTime = cleanValue / mPlantInfo.mCleanDS;
                double lifeClean = -((LifeDeMax - LifeDeMin) * ((mPlantInfo.mCleanLevel[0] * 2 - cleanValue * 2 + minTime * mPlantInfo.mCleanDS) / mPlantInfo.mCleanLevel[0]) + LifeDeMin * 2) * minTime / 2;
                double lifeWater = -LifeDeMax * minTime;
                if (lifeValue + lifeWater + lifeClean <= PETools.PEMath.Epsilon)
                {
                    double a = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mCleanDS / mPlantInfo.mCleanLevel[0]) / 2;
                    double b = -(LifeDeMax - LifeDeMin) * (mPlantInfo.mCleanLevel[0] - cleanValue) / mPlantInfo.mCleanLevel[0] - LifeDeMin - LifeDeMax;
                    double c = lifeValue;
                    double t = GetLargerResult(a,b,c);
                    surviveTime += t;
                    return surviveTime;
                }
                else
                {
                    //update the data , time pass minTime
                    waterValue -= mPlantInfo.mWaterDS * minTime;
                    waterValue = waterValue < 0 ? 0 : waterValue;
                    //clean
                    cleanValue -= mPlantInfo.mCleanDS * minTime;
                    cleanValue = cleanValue < 0 ? 0 : cleanValue;
                    //life
                    lifeValue += lifeWater + lifeClean;
                    surviveTime += minTime;
                }
            }


            //3)both 0
            double finalTime = lifeValue / (LifeDeMax * 2);
            surviveTime += finalTime;
        }


        return surviveTime;
    }
    #endregion


    #region save/Load data
    void ISaveDataInScene.ImportData(byte[] data)
    {
        MemoryStream ms = new MemoryStream(data);
        BinaryReader _in = new BinaryReader(ms);
        int saveVersion = _in.ReadInt32();
        switch (saveVersion)
        {
			case Version000:
                id = _in.ReadInt32();
                mPlantInstanceId = _in.ReadInt32();
                _PlantType = _in.ReadInt32();
                mPutOutGameTime = _in.ReadDouble();
                mLife = _in.ReadDouble();
                mWater = _in.ReadDouble();
                mClean = _in.ReadDouble();
                mDead = _in.ReadBoolean();
                mGrowTimeIndex = _in.ReadInt32();
                mCurGrowTime = _in.ReadDouble();
                mTerrianType = _in.ReadByte();
                mExtraGrowRate = _in.ReadSingle();
                mInit = _in.ReadBoolean();
                InitGrowRate(mExtraGrowRate);
                break;
			case Version:
				id = _in.ReadInt32();
				mPlantInstanceId = _in.ReadInt32();
				_PlantType = _in.ReadInt32();
				mPutOutGameTime = _in.ReadDouble();
				mLife = _in.ReadDouble();
				mWater = _in.ReadDouble();
				mClean = _in.ReadDouble();
				mDead = _in.ReadBoolean();
				mGrowTimeIndex = _in.ReadInt32();
				mCurGrowTime = _in.ReadDouble();
				mTerrianType = _in.ReadByte();
				mExtraGrowRate = _in.ReadSingle();
				mInit = _in.ReadBoolean();
				mLastUpdateTime = _in.ReadDouble();
				InitGrowRate(mExtraGrowRate);
				break;
			case Version001:
				id = _in.ReadInt32();
				mPlantInstanceId = _in.ReadInt32();
				_PlantType = _in.ReadInt32();
				mPutOutGameTime = _in.ReadDouble();
				mLife = _in.ReadDouble();
				mWater = _in.ReadDouble();
				mClean = _in.ReadDouble();
				mDead = _in.ReadBoolean();
				mGrowTimeIndex = _in.ReadInt32();
				mCurGrowTime = _in.ReadDouble();
				mTerrianType = _in.ReadByte();
				mGrowRate = _in.ReadSingle();
				mExtraGrowRate = _in.ReadSingle();
				mNpcGrowRate = _in.ReadSingle();
				mInit = _in.ReadBoolean();
				mLastUpdateTime = _in.ReadDouble();
				break;
        }

    }

    byte[] ISaveDataInScene.ExportData()
	{
//		UpdateStatus();
        MemoryStream ms = new MemoryStream();
        BinaryWriter _out = new BinaryWriter(ms);
		_out.Write(curVersion);

        _out.Write(id);
        _out.Write(mPlantInstanceId);
        _out.Write(_PlantType);
        _out.Write(mPutOutGameTime);
        _out.Write(mLife);
        _out.Write(mWater);
        _out.Write(mClean);
        _out.Write(mDead);
        _out.Write(mGrowTimeIndex);
        _out.Write(mCurGrowTime);
        _out.Write(mTerrianType);
		_out.Write(mGrowRate);
		_out.Write(mExtraGrowRate);
		_out.Write(mNpcGrowRate);
        _out.Write(mInit);
		_out.Write(mLastUpdateTime);
        _out.Close();
        ms.Close();
        byte[] retval = ms.ToArray();
        return retval;
    }

    //public void SetData(object data)
    //{
    //    DragItem item = data as DragItem;

    //    plantInstanceId = item.Id;
    //    protoTypeId = item.itemDrag.itemObj.protoId;
    //}

    
    #endregion

    #region itemscript
    public override void OnActivate()
    {
        base.OnActivate();
        ItemScript s = GetComponentInChildren<ItemScript>();
        if (null != s)
        {
            s.OnActivate();
        }
    }

    public override void OnDeactivate()
    {
        ItemScript s = GetComponentInChildren<ItemScript>();
        if (null != s)
        {
            s.OnDeactivate();
        }
    }

    public override void OnConstruct()
    {
        base.OnDeactivate();

        _mainGo = itemDrag.CreateViewGameObject(null);

        if (_mainGo == null)
        {
            return;
        }

        _mainGo.transform.parent = transform;

        _mainGo.transform.position = transform.position;
        _mainGo.transform.rotation = transform.rotation;
        _mainGo.transform.localScale = transform.localScale;
        
        ItemScript itemScript = _mainGo.GetComponent<ItemScript>();
        if (null != itemScript)
        {
            itemScript.SetItemObject(itemDrag.itemObj);
            itemScript.InitNetlayer(mNetlayer);
            itemScript.id = id;
            itemScript.OnConstruct();
        }
    }

    public override void OnDestruct()
    {
        ItemScript s = GetComponentInChildren<ItemScript>();
        if (null != s)
        {
            s.OnDestruct();
        }
        if (_mainGo != null)
        {
            GameObject.Destroy(_mainGo);
        }
        //use base
        base.OnDestruct();

    }
    #endregion

    #region tool
    public Double GetLargerResult(double a,double b,double c)
    {
        if (a > 0)
        {
            return (-b + Mathf.Sqrt((float)(b * b - 4 * a * c))) / (2 * a);
        }
        else
        {
            return (-b - Mathf.Sqrt((float)(b * b - 4 * a * c))) / (2 * a);
        }
    }
    #endregion


    #region multiMode

    public void InitInMultiMode()
    {
        mPlantInfo = PlantInfo.GetPlantInfoByItemId(protoTypeId);
		mPlantBounds = PlantInfo.GetPlantBounds(protoTypeId,mPos);
        FarmManager.Instance.AddPlant(this);
    }

    public void InitDataFromPlant(FarmPlantInitData plant)
    {
        mPlantInfo = PlantInfo.GetInfo(plant.mTypeID);
        mLife = plant.mLife;
        mWater =  plant.mWater;
        mClean = plant.mClean;
        mCurGrowTime = plant.mCurGrowTime;
        mDead = plant.mDead;
		mLastUpdateTime = plant.mLastUpdateTime;
		mPlantBounds = PlantInfo.GetPlantBounds(protoTypeId,mPos);
    }

    public void UpdateInMultiMode()
	{
		//mLastUpdateTime = GameTime.Timer.Second;
        UpdateGrowIndex();
        UpdateEvent();
        UpdateModel();
    }

    public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
    {
        try
        {
            int instanceId = stream.ReadInt32();
            double life = stream.ReadDouble();
            double water = stream.ReadDouble();
            double clean = stream.ReadDouble();
            double grow = stream.ReadDouble();
            bool dead = stream.ReadBoolean();
			double lastUpdate = stream.ReadDouble ();
            FarmPlantLogic plant = FarmManager.Instance.GetPlantByItemObjID(instanceId);
            if (plant != null)
            {
                plant.mLife = life;
                plant.mWater = water;
                plant.mClean = clean;
                plant.mCurGrowTime = grow;
                plant.mDead = dead;
				plant.mLastUpdateTime = lastUpdate;
            }
            return plant;
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }

    public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
    {
        try
        {
            FarmPlantLogic plant = value as FarmPlantLogic;
            stream.WriteInt32(plant.mPlantInstanceId);
            stream.WriteDouble(plant.mLife);
            stream.WriteDouble(plant.mWater);
            stream.WriteDouble(plant.mClean);
            stream.WriteDouble(plant.mCurGrowTime);
            stream.WriteBoolean(plant.mDead);
			stream.WriteDouble (plant.mLastUpdateTime);
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }
    #endregion

    #region init
    void Start()
    {
        if (!GameConfig.IsMultiMode)
        {
			mPlantInfo = PlantInfo.GetPlantInfoByItemId(protoTypeId);
			FarmManager.Instance.AddPlant(this);
			if (!mInit)
			{
                FarmManager.Instance.InitPlant(this);
                mInit = true;
            }

        	InitUpdateTime();
        }
    }

    public void InitUpdateTime()
	{
		if(mLastUpdateTime<PETools.PEMath.Epsilon&&mLastUpdateTime>-PETools.PEMath.Epsilon)
        	mLastUpdateTime = GameTime.Timer.Second;
		IsRipe= (mGrowTimeIndex == mPlantInfo.mGrowTime.Length);
		if(mDead||IsRipe)
			mNextUpdateTime=-1;
		else
			UpdateStatus();
    }
    #endregion
}
