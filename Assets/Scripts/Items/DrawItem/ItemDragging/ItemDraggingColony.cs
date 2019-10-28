using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSRecord;

public class ItemDraggingColony : ItemDraggingArticle
{
    [System.Serializable]
    public class FieldInfo
    {
        public Vector3 center;
        public float radius;
        public Pathea.PeGameMgr.ESceneMode mode;
    }

    public List<FieldInfo> m_LimitedField;

	CSEntityObject m_CEO;

	bool m_NotInWaterOrCave = true;

	void Awake()
	{
		m_CEO = GetComponentInChildren<CSEntityObject>();
		if(null != m_CEO){
			m_CEO.m_BoundState = 0;
		}
	}

    private bool bLimited = false;

	public override bool OnPutDown()
	{
		if (Pathea.PeGameMgr.IsMulti)
		{
			if (VArtifactUtil.IsInTownBallArea(transform.position))
			{
				new PeTipMsg(PELocalization.GetString(8000864), PeTipMsg.EMsgLevel.Warning);
				return true;
			}
		}

		return base.OnPutDown();
	}

	public override bool OnDragging(Ray cameraRay)
    {
        if (Pathea.SingleGameStory.curType != Pathea.SingleGameStory.StoryScene.MainLand
		    ||RandomDungenMgrData.InDungeon)
            return false;

        bool canPutOut = base.OnDragging(cameraRay);

        // limited field
        bLimited = false;
        for (int i = 0; i < m_LimitedField.Count; i++)
        {
            if (Pathea.PeGameMgr.sceneMode != m_LimitedField[i].mode)
                continue;

            Vector3 pos = transform.position;
            float sqr_dis = m_LimitedField[i].radius * m_LimitedField[i].radius;
            if ((pos - m_LimitedField[i].center).sqrMagnitude < sqr_dis)
            {
                bLimited = true;
                break;
            }
        }


        CSCreator creator = CSMain.GetCreator(CSConst.ciDefMgCamp);
        if (creator != null)
			canPutOut = canPutOut && (creator.CanCreate((int)m_CEO.m_Type, transform.position) == CSConst.rrtSucceed && !bLimited);

		m_NotInWaterOrCave = transform.position == Vector3.zero 
			|| (!CheckPosInCave() && !PETools.PEUtil.CheckPositionUnderWater(transform.position));
		if(canPutOut)
			canPutOut = canPutOut && m_NotInWaterOrCave;

        //if (m_CEO.HasStarted)
        //    OnItemOpGUIActive();

		if(null != itemBounds)
			itemBounds.activeState = canPutOut;
        return canPutOut;
    }

	bool CheckPosInCave()
	{
		int count = 0;
		int verticalNumber = 4;
		int horizontalNumber = 8;
		float coverage = 0.7f;
		float upDeviationAngle = 60f;
		

		for(int i = 0; i < verticalNumber; i++)
		{
			for (int j = 0; j < horizontalNumber; j++) 
			{
				Vector3 rayDirection = Quaternion.AngleAxis(360 * j / (float)horizontalNumber, Vector3.up) * Vector3.forward;
				//				Vector3 axis = Vector3.Cross(rayDirection, Vector3.up);
				//				rayDirection = Quaternion.AngleAxis(88 * i / (float)verticalNumber, axis) * rayDirection;
				rayDirection = Vector3.Slerp(Vector3.up, rayDirection, i * upDeviationAngle / verticalNumber / 90f);
				
				//Debug.DrawRay(position, rayDirection.normalized * 5, Color.red);
				RaycastHit[] hitInfos = Physics.RaycastAll(transform.position, rayDirection, 100f, AiUtil.voxelLayer);
				for(int k = 0; k < hitInfos.Length; ++k)
				{
					if(null == hitInfos[k].collider.GetComponent<Block45ChunkGo>())
					{
						count++;
						break;
					}
				}
			}
		}
		
		if(count >= verticalNumber * horizontalNumber * coverage)
			return true;
		else
			return false;
	}

    //bool bPut = false;
    public override bool OnCheckPutDown()
    {
        //OnItemOpGUIHide();
#if fasle
		HB_Entity entity = GetComponent<HB_Entity>();
		if (entity != null)
		{
			if (ColonyManager.Instance == null)
				return false;
			EntityMgrInst entityMgr = ColonyManager.Instance.GetEntityMgrSinglePlayer();
			BuildingState r = entityMgr.CanRegister(entity);
			if (r == BuildingState.Succeed)
			{
				entity.Register(entityMgr);
			}
			// Show message box
			else if (r == BuildingState.NoAssembly)
			{
				MessageBox_N.ShowOkBox(
					"You need to build an Assembly building with its static field before you can place down other buildings as per colony safety regulations!");
				return false;
			}
			else if (r == BuildingState.OutOfMaxCount)
			{
				MessageBox_N.ShowOkBox(
					"The number of buildings of this type allowed by safety regulations in the current static field has been reached!");
				return false;
			}
			else if (r == BuildingState.OutOfAssemblyRange)
			{
				MessageBox_N.ShowMsgBox(MsgBoxType.Msg_OK, MsgInfoType.Null,
					"The building cannot be placed outside the static field in compliance with Colony Regulations Article 3 Section 2.");
				return false;
			}
		}
#endif

        // Put off Colonies

		if(!m_NotInWaterOrCave)
		{
			MessageBox_N.ShowOkBox(PELocalization.GetString(82209011));
			return false;
		}

        if (m_CEO != null)
        {
            if (bLimited)
            {
                MessageBox_N.ShowOkBox(PELocalization.GetString(3002311));
                return false;
            }

            CSCreator creator = CSMain.GetCreator(CSConst.ciDefMgCamp);
            int result = creator.CanCreate((int)m_CEO.m_Type, transform.position);

            if (result == CSConst.rrtHasAssembly)
            {
                MessageBox_N.ShowOkBox(
                                        PELocalization.GetString(8000087));
                return false;
            }
            else if (result == CSConst.rrtNoAssembly)
            {
                MessageBox_N.ShowOkBox(
                                        PELocalization.GetString(8000088));
                return false;
            }
            else if (result == CSConst.rrtOutOfRange)
            {
                MessageBox_N.ShowOkBox(
                                        PELocalization.GetString(8000105));
                return false;
            }
            else if (result == CSConst.rrtOutOfRadius)
            {
                MessageBox_N.ShowOkBox(PELocalization.GetString(8000089));
                return false;
            }
            else if (result == CSConst.rrtTooCloseToNativeCamp)
            {
                MessageBox_N.ShowOkBox(PELocalization.GetString(ColonyErrorMsg.TOO_CLOSE_TO_NATIVE_CAMP0));
                return false;
            }
            else if (result == CSConst.rrtTooCloseToNativeCamp1)
            {
                MessageBox_N.ShowOkBox(PELocalization.GetString(ColonyErrorMsg.TOO_CLOSE_TO_NATIVE_CAMP1));
                return false;
            }
            else if (result == CSConst.rrtTooCloseToNativeCamp2)
            {
                MessageBox_N.ShowOkBox(PELocalization.GetString(ColonyErrorMsg.TOO_CLOSE_TO_NATIVE_CAMP2));
                return false;
			}
			else if (result == CSConst.rrtAreaUnavailable)
			{
//				MessageBox_N.ShowOkBox(PELocalization.GetString(ColonyErrorMsg.AREA_UNAVAILABLE));
				return false;
			}

            m_CEO.m_BoundState = 0;
        }

        //bPut = true;

        return true;
    }
}
