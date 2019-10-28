using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AiAsset;

public class AreaBattleManager : MonoBehaviour
{
	static AreaBattleManager mInstance;
	public static AreaBattleManager Instance{get{return mInstance;}}
	
	public BattleUnit mSelfUnit;
	
	public int	mSingleAreaLength;	// mAreaBattleRadius/mSingleAreaLength must be a odd
	public int	mAreaNum_OneSide;	// must be an odd
	public int 	mActiveAreaRadius;	// 
	
	//int 			mAreaBattleLength;  // 
	int				mLongestLength;
	
	public float	mUpdateInterval;
	float			mPassTime;
	
	public int 	 	mEnemyTotal;		// EnemyNum of this battle
	int				mFreeEnemyNum;		// EnemyNum has not been generated
	public int		mEnemyMax;			// Max EnemyNum in the sametime
	int				mCurrentEnemyNum;	// EnemyNum already in the battle
		
	public float	mEnemyRecoverPS;	// if mCurrentEnemyNum < mEnemyMax how many enemy convert from
	float			mReadyEnemyNum;
	
	public float	mGenInterval;		// Interval time between tow generation
	float			mGenPassTime;		// Time since last generat enemies
		
	public int		mEnemyGeneratedMax;	// Max EnemyNum Generated once
	public int		mEnemyGeneratedMin;	// Max EnemyNum Generated once
	
	bool		 	mActive = false;	// 
	
	public BattleUnit	mPerfab;
	
	public List<int>	mGenEnemyID;	//Size must same as mGenEnemyP 
	public List<int>	mGenEnemyP;		//Size must same as mGenEnemyID
	
	public int			mMaxShowModelNum; 	//
	
	[HideInInspector]
	public List<BattleUnit>	mCurrentShowMode;	
	
//	List<BattleUnit>	mEnemyList;
//	List<BattleUnit>	mUnactiveEnemyList;
	
	List<BattleUnit>	mEnergyUnitList;
	List<BattleUnit>	mAmmoUnitList;
	
	public bool			mStart = false;
	
	public class AreaUnit
	{
		public List<BattleUnit> mDefenderList = new List<BattleUnit>();	//
		public List<BattleUnit> mEnemyList = new List<BattleUnit>();	//
		
		public List<BattleUnit> mUnInstantiateEnemy = new List<BattleUnit>();
			
		Bounds mBounds;
		AreaBattleManager	mParent;
		List<AreaUnit>	mNearArea = new List<AreaUnit>();
		
		float DefenderBE = 0;		//
		float EnemyBE = 0;			//
		public float 	mDefenderForce = 100;
		public bool		mPlayerCtrlArea = false;
		
		float			mUpdateInterval = 5f;
		public float	mUpdatePassTime = 0;
		
		public float UsableDefenderBE() //
		{
			if(IsInBattle() && EnemyBE > 0)
				return DefenderBE * DefenderBE/EnemyBE;
			return DefenderBE;
		}
		
		public bool IsInBattle()
		{
			return mDefenderList.Count>0 && mEnemyList.Count > 0;
		}
		
		public float DefendWeight()
		{
			return IsInBattle()? DefenderBE/(DefenderBE + EnemyBE) : 1;
		}
		
		public void SetRange(Vector3 min, Vector3 max, AreaBattleManager parent)
		{
			mBounds.SetMinMax(min,max);
			mParent = parent;
		}
		
		public void AddNearArea(AreaUnit areaUnit)
		{
			mNearArea.Add(areaUnit);
		}
		
		public bool IsInArea(Vector3 pos)
		{
			return mBounds.Contains(pos);
		}
		
		public void AddBattleUnit(BattleUnit battleUnit)
		{
			if(battleUnit.mPlayerForce)
			{
				if(!mPlayerCtrlArea)
				{
					mDefenderList.Add(battleUnit);
				}
			}
			else
			{
				if(mPlayerCtrlArea)
					mUnInstantiateEnemy.Add(battleUnit);
				else
					mEnemyList.Add(battleUnit);
			}
		}

        static float DamageScale = 1f;
		public void Update()
		{
			mUpdatePassTime += Time.deltaTime;
			if(mUpdatePassTime > mUpdateInterval)
			{
				mUpdatePassTime -= mUpdateInterval;
				
				if(mPlayerCtrlArea)
				{
					CreatEnemyMode();
					return;
				}
				
				DefenderBE = 0;
				EnemyBE = 0;		
				bool inBattle = IsInBattle();
	
				//DefenderAction
				for(int i = mDefenderList.Count - 1;i >= 0; i--)
				{
					if(mDefenderList[i].mHp > 0)
					{
						//Atacker
						if(inBattle && mEnemyList.Count > 0 && Convert.ToBoolean(mDefenderList[i].mType & (1<<5)))
						{
							switch(mDefenderList[i].mAtkRange)
							{
							case EffectRange.Single:
								{
									BattleUnit target = mEnemyList[UnityEngine.Random.Range(0,mEnemyList.Count)];
									float damage = mDefenderList[i].mAtk * (1 - target.mDef/(target.mDef + DamageScale))
										* AiDamageTypeData.GetDamageScale(mDefenderList[i].mAtkType,target.mDefType);
									target.mHp -= damage * mUpdateInterval / mDefenderList[i].mAtkInterval;
								}
								break;
							case EffectRange.Range:
								foreach(BattleUnit enemy in mEnemyList)
								{
									float damage = mDefenderList[i].mAtk * (1 - enemy.mDef/(enemy.mDef + DamageScale))
									* AiDamageTypeData.GetDamageScale(mDefenderList[i].mAtkType,enemy.mDefType);
									enemy.mHp -= damage * mUpdateInterval / mDefenderList[i].mAtkInterval;
								}
								break;
							case EffectRange.spread:
								{
									BattleUnit target = mEnemyList[UnityEngine.Random.Range(0,mEnemyList.Count)];
									float damage = mDefenderList[i].mAtk * (1 - target.mDef/(target.mDef + DamageScale))
										* AiDamageTypeData.GetDamageScale(mDefenderList[i].mAtkType,target.mDefType);
									target.mHp -= damage * mUpdateInterval / mDefenderList[i].mAtkInterval;
								
									foreach(BattleUnit enemy in mEnemyList)
									{
										if(target != enemy)
										{
											damage = mDefenderList[i].mAtk * (1 - enemy.mDef/(enemy.mDef + DamageScale))
											* AiDamageTypeData.GetDamageScale(mDefenderList[i].mAtkType,enemy.mDefType) * mDefenderList[i].mSpreadFactor;
											enemy.mHp -= damage * mUpdateInterval / mDefenderList[i].mAtkInterval;
										}
									}
								}
								break;
							}
						}
						
						//Healer
						if(Convert.ToBoolean(mDefenderList[i].mType & (1<<6)))
						{
							switch(mDefenderList[i].mHealRange)
							{
							case EffectRange.Single:
								{
									BattleUnit target = null;
									float maxHpDT = 0;
									foreach(BattleUnit defender in mDefenderList)
									{
										if(defender.mHp < defender.mMaxHp && Convert.ToBoolean(defender.mType & mDefenderList[i].mHealType))
										{
											if(maxHpDT < defender.mMaxHp - defender.mHp)
											{
												maxHpDT = defender.mMaxHp - defender.mHp;
												target = defender;
											}
										}
									}
									if(target)
										target.mHp = Mathf.Clamp(target.mHp + mDefenderList[i].mHealPs * mUpdateInterval,0,target.mMaxHp);
								}
								break;
							case EffectRange.Range:
								foreach(BattleUnit defender in mDefenderList)
								{
									if(defender.mHp < defender.mMaxHp && Convert.ToBoolean(defender.mType & mDefenderList[i].mHealType))
										defender.mHp = Mathf.Clamp(defender.mHp + mDefenderList[i].mHealPs * mUpdateInterval,0,defender.mMaxHp);
								}
								break;
							case EffectRange.spread:
								{
									BattleUnit target = null;
									float maxHpDT = 0;
									foreach(BattleUnit defender in mDefenderList)
									{
										if(defender.mHp < defender.mMaxHp && Convert.ToBoolean(defender.mType & mDefenderList[i].mHealType))
										{
											if(maxHpDT < defender.mMaxHp - defender.mHp)
											{
												maxHpDT = defender.mMaxHp - defender.mHp;
												target = defender;
											}
										}
									}
									if(target)
										target.mHp = Mathf.Clamp(target.mHp + mDefenderList[i].mHealPs * mUpdateInterval,0,target.mMaxHp);
									foreach(BattleUnit defender in mDefenderList)
									{
										if(defender != target && defender.mHp < defender.mMaxHp && Convert.ToBoolean(defender.mType & mDefenderList[i].mHealType))
											defender.mHp = Mathf.Clamp(defender.mHp + mDefenderList[i].mHealPs * mUpdateInterval * mDefenderList[i].mSpreadFactor,0,defender.mMaxHp);
									}
								}
								break;
							}
						}
					}
				}
				
				
				//EnemyAction
				for(int i = mEnemyList.Count - 1;i >= 0; i--)
				{
					if(mEnemyList[i].mHp > 0)
					{
						//Atacker
						if(inBattle && mDefenderList.Count > 0 && Convert.ToBoolean(mEnemyList[i].mType & (1<<5)))
						{
							switch(mEnemyList[i].mAtkRange)
							{
							case EffectRange.Single:
								{
									BattleUnit target = mDefenderList[UnityEngine.Random.Range(0,mDefenderList.Count)];
									float damage = mEnemyList[i].mAtk * (1 - target.mDef/(target.mDef + DamageScale))
										* AiDamageTypeData.GetDamageScale(mEnemyList[i].mAtkType,target.mDefType);
									target.mHp -= damage * mUpdateInterval / mEnemyList[i].mAtkInterval;
								}
								break;
							case EffectRange.Range:
								foreach(BattleUnit defender in mDefenderList)
								{
									float damage = mEnemyList[i].mAtk * (1 - defender.mDef/(defender.mDef + DamageScale))
									* AiDamageTypeData.GetDamageScale(mEnemyList[i].mAtkType,defender.mDefType);
									defender.mHp -= damage * mUpdateInterval / mEnemyList[i].mAtkInterval;
								}
								break;
							case EffectRange.spread:
								{
									BattleUnit target = mDefenderList[UnityEngine.Random.Range(0,mDefenderList.Count)];
									float damage = mEnemyList[i].mAtk * (1 - target.mDef/(target.mDef + DamageScale))
										* AiDamageTypeData.GetDamageScale(mEnemyList[i].mAtkType,target.mDefType);
									target.mHp -= damage * mUpdateInterval / mEnemyList[i].mAtkInterval;
								
									foreach(BattleUnit defender in mDefenderList)
									{
										if(defender != target)
										{
											damage = mEnemyList[i].mAtk * (1 - defender.mDef/(defender.mDef + DamageScale))
											* AiDamageTypeData.GetDamageScale(mEnemyList[i].mAtkType,defender.mDefType);
											defender.mHp -= damage * mUpdateInterval / mEnemyList[i].mAtkInterval;
										}
									}
								}
								break;
							}
						}
						
						//Healer
						if(Convert.ToBoolean(mEnemyList[i].mType & (1<<6)))
						{
							switch(mEnemyList[i].mHealRange)
							{
							case EffectRange.Single:
								{
									BattleUnit target = null;
									float maxHpDT = 0;
									foreach(BattleUnit enemy in mEnemyList)
									{
										if(enemy.mHp < enemy.mMaxHp && Convert.ToBoolean(enemy.mType & mEnemyList[i].mHealType))
										{
											if(maxHpDT < enemy.mMaxHp - enemy.mHp)
											{
												maxHpDT = enemy.mMaxHp - enemy.mHp;
												target = enemy;
											}
										}
									}
									if(target)
										target.mHp = Mathf.Clamp(target.mHp + mEnemyList[i].mHealPs * mUpdateInterval,0,target.mMaxHp);
								}
								break;
							case EffectRange.Range:
								foreach(BattleUnit enemy in mEnemyList)
								{
									if(enemy.mHp < enemy.mMaxHp && Convert.ToBoolean(enemy.mType & mEnemyList[i].mHealType))
										enemy.mHp = Mathf.Clamp(enemy.mHp + mEnemyList[i].mHealPs * mUpdateInterval,0,enemy.mMaxHp);
								}
								break;
							case EffectRange.spread:
								{
									BattleUnit target = null;
									float maxHpDT = 0;
									foreach(BattleUnit enemy in mEnemyList)
									{
										if(enemy.mHp < enemy.mMaxHp && Convert.ToBoolean(enemy.mType & mEnemyList[i].mHealType))
										{
											if(maxHpDT < enemy.mMaxHp - enemy.mHp)
											{
												maxHpDT = enemy.mMaxHp - enemy.mHp;
												target = enemy;
											}
										}
									}
									if(target)
										target.mHp = Mathf.Clamp(target.mHp + mEnemyList[i].mHealPs * mUpdateInterval,0,target.mMaxHp);
									foreach(BattleUnit enemy in mEnemyList)
									{
										if(enemy != target && enemy.mHp < enemy.mMaxHp && Convert.ToBoolean(enemy.mType & mEnemyList[i].mHealType))
											enemy.mHp = Mathf.Clamp(enemy.mHp + mEnemyList[i].mHealPs * mUpdateInterval * mEnemyList[i].mSpreadFactor,0,enemy.mMaxHp);
									}
								}
								break;
							}
						}
					}
				}
				
				//release things that need be remove,and count the BE
				for(int i = mDefenderList.Count - 1;i >= 0; i--)
				{
					if(mDefenderList[i].mHp > 0)
						DefenderBE += mDefenderList[i].mBE;
					else
					{
						if(mDefenderList[i] == mParent.mSelfUnit)
							mParent.OnBattleFail();

                        RemoveFromDragItemMgr(mDefenderList[i].gameObject);
//						foreach(ItemPackGui_N.DrawItem item in ItemPackGui_N.mDrawItemList)
//						{
//							if (GameObject.Equals(mDefenderList[i].gameObject,item.mObject))
//							{
//								mDefenderList.RemoveAt(i);
//								ItemPackGui_N.mDrawItemList.Remove(item);
//								break;
//							}
//						}
					}
				}
				
				for(int i = mEnemyList.Count - 1;i >= 0; i--)
				{
					if(mEnemyList[i].mHp > 0)
						EnemyBE += mEnemyList[i].mBE;
					else
					{
						GameObject.Destroy(mEnemyList[i].gameObject);
						mEnemyList.RemoveAt(i);
					}
				}
				
				if(DefenderBE == 0 && EnemyBE > 0)
				{
					AreaUnit targetArea = null;
					float minForce = 100000000f;
					//enemy go to the near area which has the lessest force but higher force than self
					foreach(AreaUnit area in mNearArea)
					{
						if(area.mDefenderForce > mDefenderForce && area.mDefenderForce < minForce)
						{
							targetArea = area;
							minForce = area.mDefenderForce;
						}
					}
					// if can find area than choose the hightest force one
					if(null == targetArea)
					{
						minForce = 0;
						foreach(AreaUnit area in mNearArea)
						{
							if(area.mDefenderForce > minForce)
							{
								targetArea = area;
								minForce = area.mDefenderForce;
							}
						}
					}
					
					for(int i = mEnemyList.Count - 1; i >= 0; i--)
					{
						if(Convert.ToBoolean(mEnemyList[i].mType & 1) && mEnemyList[i].CanMove())
						{
							mEnemyList[i].Move();
							targetArea.AddBattleUnit(mEnemyList[i]);
							mEnemyList.RemoveAt(i);
						}
					}
				}
				// MoveAble unit go to other area which is inbattle and is nearest one
				else if(EnemyBE == 0 && DefenderBE > 0)
				{
					
				}
			}
		}

        void RemoveFromDragItemMgr(GameObject obj)
        {
            ItemScript script = obj.GetComponent<ItemScript>();
            if (null == script)
            {
                Debug.LogError("has no itemscript");
                return;
            }

            DragArticleAgent.Destory(script.id);
        }

		public void ActiveAsPlayerCtrl()
		{
			mPlayerCtrlArea = true;
			//Change the virtual things to waitList
			SynchBattleUnit();
		}
		
		public void UnactiveAsAutoCtrl()
		{
			mPlayerCtrlArea = false;
			RecountBattleUnit();
		}
		
		public void CreatEnemyMode()
		{
			Dictionary<int,List<BattleUnit>> entityList = new Dictionary<int, List<BattleUnit>>();
			foreach(BattleUnit enemy in mUnInstantiateEnemy)
			{
				if(entityList.ContainsKey(enemy.mID))
					entityList[enemy.mID].Add(enemy);
				else
				{
					entityList[enemy.mID] = new List<BattleUnit>();
					entityList[enemy.mID].Add(enemy);
				}
			}
			
			foreach(int enemyID in entityList.Keys)
			{
				if(mParent.CanCreateModeNum() > 0)
				{
					BattleUnitData bud = BattleUnitData.GetBattleUnitData(enemyID);
					
					//just use befor monster creatfunction finished
					int count = (entityList[enemyID].Count <= mParent.CanCreateModeNum()) 
									? entityList[enemyID].Count : mParent.CanCreateModeNum();
					
					for(int i = 0; i < count; i++)
					{
						string[] strList = bud.mPerfabPath.Split(',');
						
						GameObject obj = GameObject.Instantiate(Resources.Load(strList[1])) as GameObject;
						obj.transform.parent = mParent.transform;
						obj.transform.position = GetSaveCreatePos();
						obj.transform.rotation = Quaternion.identity;
						mParent.CreatMode(obj.GetComponent<BattleUnit>());
						
                        //AiMonster aimonster = obj.GetComponent<AiMonster>();
                        //if(aimonster)
                        //    aimonster.life = (int)entityList[enemyID][i].mHp;
						
						mUnInstantiateEnemy.Remove(entityList[enemyID][i]);
						GameObject.Destroy(entityList[enemyID][i].gameObject);
					}
					//test end
				}
				else
					break;
			}
		}
		
		Vector3 GetSaveCreatePos()
		{
            //Vector3 pos = new Vector3(UnityEngine.Random.Range(mBounds.min.x,mBounds.max.x),
            //                PlayerFactory.mMainPlayer.transform.position.y + 500,UnityEngine.Random.Range(mBounds.min.z,mBounds.max.z));
            //RaycastHit rayHit;
            //if(Physics.Raycast(pos,Vector3.down,out rayHit, 1000f,-1))
            //    return rayHit.point + Vector3.up;
            //pos.y = PlayerFactory.mMainPlayer.transform.position.y;
            //return pos;
            return Vector3.zero;
		}
		
		void SynchBattleUnit()
		{
			//Enmeies synch property when creating
			mUnInstantiateEnemy.AddRange(mEnemyList);
			mEnemyList.Clear();
			
//			foreach(BattleUnit defender in mDefenderList)
//			{
//                //AiTower tower = defender.GetComponent<AiTower>();
//                //if(tower)
//                //    tower.life = (int)defender.mHp;
//                //else
//                //{
//                //    //NpcCommon_N npc = defender.GetComponent<NpcCommon_N>();
//                //    //AiNpcObject npc = defender.GetComponent<AiNpcObject>();
//                //    //if(npc)
//                //    //    npc.life = (int)defender.mHp;
//                //}
//			}
			mDefenderList.Clear();
		}
		
		//Recount BattleUnit num in this Area.Called in Init and the time when player go to other Area
		public void RecountBattleUnit()
		{
			mDefenderList.Clear();
			
            //List<DragItemMgr.Item> drawItemList = DragItemMgr.Instance.GetItemList();
			
            //foreach(DragItemMgr.Item item in drawItemList)
            //{
            //    BattleUnit defender = item.itemScript.GetComponent<BattleUnit>();
            //    if(defender && mBounds.Contains(defender.transform.position))
            //    {
            //        mDefenderList.Add(defender);
            //        AiMonster tow = item.itemScript.GetComponent<AiMonster>();
            //        if(tow)
            //            defender.mHp = tow.life;
            //    }
            //}
			
			List<BattleUnit> showModelEnemies = mParent.mCurrentShowMode;
			for(int i = showModelEnemies.Count - 1; i>=0; i--)
			{
				if(showModelEnemies[i])//enemy aready dead but not remove yet
				{
					if(mBounds.Contains(showModelEnemies[i].transform.position))
					{
						BattleUnit createBU = MonoBehaviour.Instantiate(mParent.mPerfab) as BattleUnit;
						createBU.SetData(BattleUnitData.GetBattleUnitData(showModelEnemies[i].mID));
                        //AiMonster monster = showModelEnemies[i].GetComponent<AiMonster>();
                        //if(monster)
                        //    createBU.mHp = monster.life;
                        //else
                        //    createBU.mHp = showModelEnemies[i].mHp;
						GameObject.Destroy(showModelEnemies[i].gameObject);
						showModelEnemies.RemoveAt(i);
						mEnemyList.Add(createBU);
					}
				}
			}
		}
	}
	
	AreaUnit[,]	mAreaUnitList;
	
	public class AreaCube
	{
		public AreaUnit 	mAreaUnit;
		public GameObject	mCube;
		public void Update()
		{
			if(null != mAreaUnit && null != mCube)
			{
				float DefendWeight = mAreaUnit.DefendWeight();
				mCube.GetComponent<Renderer>().material.color = new Color(1f - DefendWeight, DefendWeight, 0, Mathf.Clamp01(mAreaUnit.mDefenderForce/10000000f));
			}
		}
	}
	AreaCube[,] mAreaCubeList;
	
	List<AreaUnit> mBorderlineArea;
		
	// Use this for initialization
	void Start () {
		mSelfUnit = GetComponent<BattleUnit>();
		mInstance = this;
	}
	
	public void InitBattle()
	{
		mActive = true;
		
		//mAreaBattleLength = mAreaNum_OneSide * mSingleAreaLength;
		mLongestLength = (mAreaNum_OneSide - 1) * 2;
		
		mFreeEnemyNum = mEnemyTotal;
			
		mAreaUnitList = new AreaUnit[mAreaNum_OneSide,mAreaNum_OneSide];
		
		if(Application.isEditor)
			mAreaCubeList = new AreaCube[mAreaNum_OneSide,mAreaNum_OneSide];
		
		mBorderlineArea = new List<AreaUnit>();
		for(int i=0;i<mAreaNum_OneSide;i++)
		{
			for(int j=0;j<mAreaNum_OneSide;j++)
			{
				mAreaUnitList[i,j] = new AreaUnit();
				Vector3 minPos = Vector3.zero;
				minPos.x = transform.position.x + (i - mAreaNum_OneSide/2 - 0.5f) * mSingleAreaLength;
				minPos.z = transform.position.z + (j - mAreaNum_OneSide/2 - 0.5f) * mSingleAreaLength;
				minPos.y = 0;
				Vector3 maxPos = new Vector3(minPos.x + mSingleAreaLength,3000f,minPos.z + mSingleAreaLength);
				
				mAreaUnitList[i,j].SetRange(minPos,maxPos,this);
				mAreaUnitList[i,j].mUpdatePassTime = i * 0.9f;
				mAreaUnitList[i,j].RecountBattleUnit();
				if(i == 0 || i == mAreaNum_OneSide-1 || j == 0 || j == mAreaNum_OneSide-1)
					mBorderlineArea.Add(mAreaUnitList[i,j]);
				
				// Test cube
				if(Application.isEditor)
				{
					mAreaCubeList[i,j] = new AreaCube();
					mAreaCubeList[i,j].mAreaUnit = mAreaUnitList[i,j];
					mAreaCubeList[i,j].mCube = GameObject.CreatePrimitive (PrimitiveType.Cube);
					mAreaCubeList[i,j].mCube.GetComponent<Renderer>().material = new Material (Shader.Find ("Transparent/Diffuse"));
					mAreaCubeList[i,j].mCube.GetComponent<Renderer>().material.color = new Color (0, 1, 0, 0.25f);
					GameObject.Destroy (mAreaCubeList[i,j].mCube.GetComponent<BoxCollider> ());
					mAreaCubeList[i,j].mCube.transform.parent = transform;
					mAreaCubeList[i,j].mCube.transform.localScale = new Vector3(0.5f,0.1f,0.5f);
					mAreaCubeList[i,j].mCube.transform.position = transform.position + new Vector3(5f/mAreaNum_OneSide *(i - mAreaNum_OneSide/2),2f,5f/mAreaNum_OneSide *(j - mAreaNum_OneSide/2));
					mAreaCubeList[i,j].Update();
				}
			}
		}
		
		//Attach near area to each area
		for(int i=0;i<mAreaNum_OneSide;i++)
		{
			for(int j=0;j<mAreaNum_OneSide;j++)
			{
				if(i > 0)
					mAreaUnitList[i,j].AddNearArea(mAreaUnitList[i-1,j]);
				if(i < mAreaNum_OneSide - 1)
					mAreaUnitList[i,j].AddNearArea(mAreaUnitList[i+1,j]);
				if(j > 0)
					mAreaUnitList[i,j].AddNearArea(mAreaUnitList[i,j-1]);
				if(j < mAreaNum_OneSide - 1)
					mAreaUnitList[i,j].AddNearArea(mAreaUnitList[i,j+1]);
			}
		}
		
		mPassTime = 0;
	}
	
	public int CanCreateModeNum()
	{
		return mMaxShowModelNum - mCurrentShowMode.Count; 
	}
	
	public void CreatMode(BattleUnit bu)
	{
		mCurrentShowMode.Add(bu);
	}
		
	// Update is called once per frame
	void Update ()
	{
		if(mActive)
		{
			GeneratNewEnemy();
			UpdateAreaUnit();
			ClearDeathEnemy();
			
			mPassTime += Time.deltaTime;
			if(mPassTime > mUpdateInterval)
			{
				mPassTime -= mUpdateInterval;
				UpdatePlayerCtrlAreaState();
				UpdateAreaForce();
			}
		}
		
		if(mStart)
		{
			mStart = false;
			if(!mActive)
				InitBattle();
		}
	}
	
	void GeneratNewEnemy()
	{
		mReadyEnemyNum += mEnemyRecoverPS * mUpdateInterval;
		
		mGenPassTime += Time.deltaTime;
		
		if(mGenPassTime >= mGenInterval)
		{
			int generatNum = 0;
			
			if(mReadyEnemyNum >= mFreeEnemyNum)
			{
				generatNum = mFreeEnemyNum;
				mFreeEnemyNum = 0;
			}
			else if(mReadyEnemyNum >= mEnemyGeneratedMax)
			{
				generatNum = mEnemyGeneratedMax;
				mFreeEnemyNum -= generatNum;
			}
			else if(mReadyEnemyNum >= mEnemyGeneratedMin)
			{
				generatNum = (int)(mFreeEnemyNum - mReadyEnemyNum);
				mFreeEnemyNum -= generatNum;
			}
			else // don't generat anything
				return;
			
			// if generat enemies reset mGenPassTime
			mGenPassTime -= mGenInterval;
			
			float totalForce = 0;
			for(int i = 0; i < mBorderlineArea.Count; i++)
				totalForce += 1f/mBorderlineArea[i].mDefenderForce;
			float random = UnityEngine.Random.Range(0f,1f);
			int areaIndex = 0;
			float currentSum = 0;
			for(int i = 0; i < mBorderlineArea.Count; i++)
			{
				if(currentSum < random && currentSum + 1f/mBorderlineArea[i].mDefenderForce/totalForce >= random)
				{
					areaIndex = i;
					break;
				}
				else
					currentSum += 1f/mBorderlineArea[i].mDefenderForce/totalForce;
			}
			
			int sumP = 0;
			int hitP = 0;
			for(int i = 0; i < mGenEnemyP.Count; i++)
				sumP += mGenEnemyP[i];
			
			Dictionary<int,int> generatEnemies = new Dictionary<int, int>();
			
			for(int i = 0; i < generatNum; i++)
			{
				hitP = UnityEngine.Random.Range(0,sumP);
				currentSum = 0;
				
				for(int j = 0; j < mGenEnemyP.Count; j++)
				{
					if(currentSum <= hitP && currentSum + mGenEnemyP[j] > hitP)
					{
						if(generatEnemies.ContainsKey(j))
							generatEnemies[j]++;
						else
							generatEnemies[j] = 1;
						break;
					}
					else
						currentSum += mGenEnemyP[j];
				}
			}
			
			// 
			foreach(int enmyIndex in generatEnemies.Keys)
			{
				BattleUnit bU = MonoBehaviour.Instantiate(mPerfab) as BattleUnit;
				bU.transform.parent = transform;
				bU.gameObject.SetActive(false);
				bU.SetData(BattleUnitData.GetBattleUnitData(mGenEnemyID[enmyIndex]));
				mBorderlineArea[areaIndex].AddBattleUnit(bU);
			}
		}
	}
	
	void UpdatePlayerCtrlAreaState()
	{
        //if(PlayerFactory.mMainPlayer)
        //{
        //    Vector3 playerPos = PlayerFactory.mMainPlayer.transform.position;
			
        //    int xIdx = (int)((playerPos.x - transform.position.x) / mSingleAreaLength + mAreaNum_OneSide/2f);
        //    int zIdx = (int)((playerPos.z - transform.position.z) / mSingleAreaLength + mAreaNum_OneSide/2f);
			
        //    for(int i=0;i<mAreaNum_OneSide;i++)
        //    {
        //        for(int j=0;j<mAreaNum_OneSide;j++)
        //        {
        //            if(Math.Abs(xIdx - i) + Math.Abs(zIdx - j) > mActiveAreaRadius)
        //            {
        //                if(mAreaUnitList[i,j].mPlayerCtrlArea)
        //                    mAreaUnitList[i,j].UnactiveAsAutoCtrl();
        //            }
        //            else
        //            {
        //                if(!mAreaUnitList[i,j].mPlayerCtrlArea)
        //                    mAreaUnitList[i,j].ActiveAsPlayerCtrl();
        //            }
        //        }
        //    }
        //}
	}
	
	void UpdateAreaUnit()
	{
		for(int i=0;i<mAreaNum_OneSide;i++)
			for(int j=0;j<mAreaNum_OneSide;j++)
				mAreaUnitList[i,j].Update();
	}
	
	void UpdateAreaForce()
	{
		for(int i=0;i<mAreaNum_OneSide;i++)
			for(int j=0;j<mAreaNum_OneSide;j++)
				mAreaUnitList[i,j].mDefenderForce = 0;
		
		for(int i=0;i<mAreaNum_OneSide;i++)
		{
			for(int j=0;j<mAreaNum_OneSide;j++)
			{
				float usableBE = mAreaUnitList[i,j].UsableDefenderBE();
				if(usableBE > 0)
				{
					for(int m=0;m<mAreaNum_OneSide;m++)
						for(int n=0;n<mAreaNum_OneSide;n++)
							mAreaUnitList[m,n].mDefenderForce += usableBE * (mLongestLength - Mathf.Abs(m-i) - Mathf.Abs(n-j))/mLongestLength;
				}
			}
		}
		if(Application.isEditor)
		{
			for(int i=0;i<mAreaNum_OneSide;i++)
				for(int j=0;j<mAreaNum_OneSide;j++)
					mAreaCubeList[i,j].Update();
		}
	}
	
	void ClearDeathEnemy()
	{
		for(int i = mCurrentShowMode.Count - 1; i >= 0; i--)
			if(mCurrentShowMode[i] == null)
				mCurrentShowMode.RemoveAt(i);
	}
	
	public void OnBattleFail()
	{
		mActive = false;
	}
	
	public void AddTower(BattleUnit bU)
	{
		for(int i=0;i<mAreaNum_OneSide;i++)
		{
			for(int j=0;j<mAreaNum_OneSide;j++)
			{
				if(mAreaUnitList[i,j].IsInArea(bU.transform.position))
				{
					mAreaUnitList[i,j].AddBattleUnit(bU);
					return;
				}
			}
		}
	}
}
