using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Behave.Assets;
using Behave.Runtime;
using Tree = Behave.Runtime.Tree;

public class AiBehave : AiBehaveTree, IAgent
{
    public string treeName;

    bool m_Pause;
    bool m_isActive;
    Tree m_tree;
	BehaveResult m_result;

    public override void Reset()
    {
        base.Reset();

        if (m_tree != null)
        {
            m_tree.Reset();
        }
    }

	public virtual bool isActive
	{
        get { return m_isActive && !m_Pause; }
		set
		{
			if(!value && m_result == BehaveResult.Running)
				return;

			m_isActive = value;
		}
	}

    public virtual bool isPause
    {
        set { m_Pause = value; }
    }

	public bool running
	{
		get { return m_result == BehaveResult.Running; }
	}

    void Start()
    {
		m_tree = null;
		m_isActive = true;
		m_result = BehaveResult.Failure;

        //CreateBehaveTree();

        //if (AiManager.Manager != null)
        //{
        //    AiManager.Manager.RegisterAiBehave(this);
        //}

        gameObject.layer = Pathea.Layer.Default;
    }

    void OnDestroy()
    {
        //if (AiManager.Manager != null)
        //{
        //    AiManager.Manager.RemoveAiBehave(this);
        //}
    }

    void OnEnable()
    {
        ActiveBehaveTree(true);
    }

    void OnDisable()
    {
        ActiveBehaveTree(false);
    }

	//void CreateBehaveTree()
	//{
        //BLLibrary.TreeType type = (BLLibrary.TreeType)System.Enum.Parse(typeof(BLLibrary.TreeType), treeName, true);
        //if (m_tree == null)
        //{
        //    try {
        //        m_tree = BLLibrary.InstantiateTree(type, this);
				
        //        if (m_tree.Frequency <= 0)
        //        {
        //            Debug.LogWarning(m_tree.Name + "'s frequency is less than 1, will not be ticked.");
        //        }
        //    } catch (System.Exception ex) {
        //        Debug.LogError("treeName = " + treeName + " : " + " { " + ex.Message + " }");
        //        m_tree = null;
        //    }
        //}
		
		//if (m_tree == null)
		//	return;
		
        //AiImplement[] implements = GetComponentsInChildren<AiImplement>();
        //foreach (AiImplement item in implements)
        //{
        //    item.SetupForward(m_tree);
        //}
		
		//ActiveBehaveTree(true);
	//}

	void ActiveBehaveTree(bool value)
	{
		if (value)
		{
			StopAllCoroutines();
			StartCoroutine(Logic());
		}
		else
		{
			if (null != m_tree)
			{
				m_tree.Reset();
			}

			StopAllCoroutines();
		}
	}

    IEnumerator Logic()
    {
        while (Application.isPlaying && m_tree != null)
        {
			if (isActive)
            {
                m_result = AiUpdate();
            }
            yield return new WaitForSeconds(1.0f / m_tree.Frequency);
        }
    }

    BehaveResult AiUpdate() 
	{
        return m_tree.Tick();
        //try
        //{
        //    return m_tree.Tick();
        //}
        //catch (System.Exception ex)
        //{
        //    Debug.LogError("Ai Tick : { " + ex.Message + " }");
        //    return BehaveResult.Failure;
        //}
	}

    public BehaveResult Tick(Tree sender) { return BehaveResult.Success; }
    public void Reset(Tree sender) { }
    public int SelectTopPriority(Tree sender, params int[] IDs) { return IDs[0]; }
}
