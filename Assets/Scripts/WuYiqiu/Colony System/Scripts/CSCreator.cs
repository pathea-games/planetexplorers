
/**************************************************************
 *                       [CSCreator.cs]
 *
 *    Colony System Creator base class
 *
 *    To Create and Mange the CSEntity. 
 *
 *
 **************************************************************/

//--------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Pathea.PeEntityExtMotion_Move;
using Pathea.PeEntityExtFollow;
//--------------------------------------------------------------

public abstract class CSCreator : MonoBehaviour
{
    //multiMode only
    public int teamNum;

    /// <summary>
    /// Gets the assembly, if the Creator has.
    /// </summary>
    /// <value>The assembly.</value>
    public virtual CSAssembly Assembly { get { return null; } }

    /// <summary>
    /// The Creator timer shaft
    /// </summary>
    /// <value>The timer.</value>
    public virtual PETimer Timer { get { return null; } }

    public float Deltatime { get { return Timer == null ? 0 : Timer.ElapseSpeed * Time.deltaTime; } }

    public int ID { get { return m_DataInst.m_ID; } }

    /// <summary>
    /// The type of Creator, Dont set it if you really know what it is?
    /// </summary>
    public CSConst.CreatorType m_Type;

    /// <summary>
    ///	Data intance for this creator, use to keep some storable data information 
    /// for Entities. Set it in CSMain.CreateCreator(). 
    /// </summary>
    public CSDataInst m_DataInst;

    /// <summary>
    /// Event listenser for Entity
    /// </summary>
    public delegate void EventListenerDel(int event_type, CSEntity entiy);
    protected event EventListenerDel m_EventListenser;

    public void RegisterListener(EventListenerDel listener)
    {
        m_EventListenser += listener;
    }
    public void UnregisterListener(EventListenerDel listener)
    {
        m_EventListenser -= listener;
    }
    public void ExecuteEvent(int event_type, CSEntity entity)
    {
        if (m_EventListenser != null)
        {
            m_EventListenser(event_type, entity);
        }
    }

    /// <summary>
    /// Event listenser for Personnel
    /// </summary>
    public delegate void EventListenerPersonnelDel(int event_type, CSPersonnel p);
    protected event EventListenerPersonnelDel m_EventListenserPer;

    public void RegisterPersonnelListener(EventListenerPersonnelDel listener)
    {
        m_EventListenserPer += listener;
    }
    public void UnregisterPeronnelListener(EventListenerPersonnelDel listener)
    {
        m_EventListenserPer -= listener;
    }
    protected void ExecuteEventPersonnel(int event_type, CSPersonnel p)
    {
        if (m_EventListenserPer != null)
        {
            m_EventListenserPer(event_type, p);
        }
    }

    /// <summary>
    /// Creates the entity, and manage it. 
    /// </summary>
    /// <returns>The entity.</returns>
    /// <param name="attr">attribute of the entity</param>
    /// <param name="outEnti">output enti value</param>
    public abstract int CreateEntity(CSEntityAttr attr, out CSEntity outEnti);

    /// <summary>
    /// Removes the entity .
    /// </summary>
    /// <returns>The entity.</returns>
    /// <param name="id">Identifier of the specific entity </param>
    /// <param name="bRemoveData">If set to <c>true</c> it will remove the data.</param>
    public abstract CSEntity RemoveEntity(int id, bool bRemoveData = true);

    /// <summary>
    /// Gets the common entity of the specific id .
    /// </summary>
    public abstract CSCommon GetCommonEntity(int ID);
    
    public abstract int GetCommonEntityCnt();

    public abstract Dictionary<int, CSCommon> GetCommonEntities();

    public virtual int CanCreate(int type, Vector3 pos)
    {
        return CSConst.rrtSucceed;
    }

    public virtual bool CanAddNpc()
    {
        return true;
    }

    /// <summary>
    /// Adds the npc to the creator, the creator can manage it
    /// </summary>
    /// <returns><c>true</c>, if npc was added, <c>false</c> otherwise.</returns>
    /// <param name="npc">specific ai npc</param>
    /// <param name="bSetPos">If set to <c>true</c> it will refresh the npc postion</param>
    public abstract bool AddNpc(PeEntity npc, bool bSetPos = false);

    /// <summary>
    /// Removes the npc from the creator
    /// </summary>
    /// <param name="npc">Npc.</param>
    public abstract void RemoveNpc(PeEntity npc);

    public abstract CSPersonnel[] GetNpcs();

	public abstract CSPersonnel GetNpc(int id);

	public virtual void RemoveLogic(int id){}
	public virtual void AddLogic(int id,CSBuildingLogic csb){}

}
