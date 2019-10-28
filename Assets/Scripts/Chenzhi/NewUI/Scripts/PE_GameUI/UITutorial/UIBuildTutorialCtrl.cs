using UnityEngine;
using System.Collections;
using Pathea;

public class UIBuildTutorialCtrl : MonoBehaviour
{
    public enum BuildOpType
    {
        MatAndShape,    //材料和形状
        VoxelAndBlock,  //类型
        Brush,          //刷
        Select,         //选择
        Menu            //控制
    }

    [SerializeField]
    private BuildTutorialItem_N m_MatAndShapeTutorial;
    [SerializeField]
    private BuildTutorialItem_N m_VoxelAndBlockTutorial;
    [SerializeField]
    private BuildTutorialItem_N m_BrushTutorial;
    [SerializeField]
    private BuildTutorialItem_N m_SelectTutorial;
    [SerializeField]
    private BuildTutorialItem_N m_MenuTutorial;
    [SerializeField]
    private BoxCollider[] m_MatAndShapeColliders;
    [SerializeField]
    private BoxCollider[] m_VoxelAndBlockColliders;
    [SerializeField]
    private BoxCollider[] m_BrushColliders;
    [SerializeField]
    private BoxCollider[] m_SelectColliders;
    [SerializeField]
    private BoxCollider[] m_MenuColliders;
    [SerializeField]
    private float m_TweenShowTime = 2;

    private bool m_ShowAllBuildTutorial = false;
    private bool m_StartShowAll = false;
    private BuildTutorialItem_N m_BuildTutorialItemBackup;


    #region mono methods
    void Awake()
    {
        if (PeGameMgr.IsTutorial)
        {
            InitEvent();
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();

        if (m_MatAndShapeTutorial.IsShow)
            m_MatAndShapeTutorial.ShowTween(false);

        if (m_VoxelAndBlockTutorial.IsShow)
            m_VoxelAndBlockTutorial.ShowTween(false);

        if(m_BrushTutorial.IsShow)
            m_BrushTutorial.ShowTween(false);

        if (m_SelectTutorial.IsShow)
            m_SelectTutorial.ShowTween(false);

        if (m_MenuTutorial.IsShow)
            m_MenuTutorial.ShowTween(false);

        //lz-2016.11.02 如果开始显示全部了,但是没有显示完，就置为显示完了
        if (m_StartShowAll && !m_ShowAllBuildTutorial)
        {
            m_ShowAllBuildTutorial = true;
        }
    }

    #endregion

    #region private methods

    private IEnumerator ShowAllBuildTutorialInterator()
    {
        m_ShowAllBuildTutorial = false;
        m_StartShowAll = true;
        float waitTime = 0;
        waitTime = m_MatAndShapeTutorial.GetTweenTime() + m_TweenShowTime;
        m_MatAndShapeTutorial.ShowTween(true);
        yield return new WaitForSeconds(waitTime);
        m_MatAndShapeTutorial.ShowTween(false);

        waitTime = m_VoxelAndBlockTutorial.GetTweenTime() + m_TweenShowTime;
        m_VoxelAndBlockTutorial.ShowTween(true);
        yield return new WaitForSeconds(waitTime);
        m_VoxelAndBlockTutorial.ShowTween(false);


        waitTime = m_BrushTutorial.GetTweenTime() + m_TweenShowTime;
        m_BrushTutorial.ShowTween(true);
        yield return new WaitForSeconds(waitTime);
        m_BrushTutorial.ShowTween(false);


        waitTime = m_SelectTutorial.GetTweenTime() + m_TweenShowTime;
        m_SelectTutorial.ShowTween(true);
        yield return new WaitForSeconds(waitTime);
        m_SelectTutorial.ShowTween(false);

        waitTime = m_MenuTutorial.GetTweenTime() + m_TweenShowTime;
        m_MenuTutorial.ShowTween(true);
        yield return new WaitForSeconds(waitTime);
        m_MenuTutorial.ShowTween(false);

        yield return new WaitForSeconds(m_MenuTutorial.GetTweenTime());
        m_ShowAllBuildTutorial = true;
    }

    BuildTutorialItem_N GetBuildTutorialItemByType(BuildOpType type)
    {
        switch (type)
        {
            case BuildOpType.MatAndShape:
                return m_MatAndShapeTutorial;
            case BuildOpType.VoxelAndBlock:
                return m_VoxelAndBlockTutorial;
            case BuildOpType.Brush:
                return m_BrushTutorial;
            case BuildOpType.Select:
                return m_SelectTutorial;
            case BuildOpType.Menu:
                return m_MenuTutorial;
            default:
                return null;
        }
    }

    void PlayerTweenByType(BuildOpType type,bool show)
    {
        if (PeGameMgr.IsTutorial)
        {
            BuildTutorialItem_N tutorialTween = GetBuildTutorialItemByType(type);
            if (null != tutorialTween)
            {
                if (show)
                {
                    if (null != m_BuildTutorialItemBackup && m_BuildTutorialItemBackup.IsShow)
                    {
                        m_BuildTutorialItemBackup.ShowTween(false);
                    }
                    m_BuildTutorialItemBackup = tutorialTween;
                }
                tutorialTween.ShowTween(show);
            }
        }
    }


    void MatAndShapeHoverEvent(GameObject go,bool isHover)
    {
        if(m_ShowAllBuildTutorial)
            PlayerTweenByType(BuildOpType.MatAndShape,isHover);
    }

    void VoxelAndBlockHoverEvent(GameObject go, bool isHover)
    {
        if (m_ShowAllBuildTutorial)
            PlayerTweenByType(BuildOpType.VoxelAndBlock, isHover);
    }

    void BrushHoverEvent(GameObject go, bool isHover)
    {
        if (m_ShowAllBuildTutorial)
            PlayerTweenByType(BuildOpType.Brush, isHover);
    }

    void SelectHoverEvent(GameObject go, bool isHover)
    {
        if (m_ShowAllBuildTutorial)
            PlayerTweenByType(BuildOpType.Select, isHover);
    }

    void MenuHoverEvent(GameObject go, bool isHover)
    {
        if (m_ShowAllBuildTutorial)
            PlayerTweenByType(BuildOpType.Menu, isHover);
    }

    void InitEvent()
    {
        if (null != m_MatAndShapeColliders && m_MatAndShapeColliders.Length > 0)
        {
            for (int i = 0; i < m_MatAndShapeColliders.Length; i++)
            {
                UIEventListener.Get(m_MatAndShapeColliders[i].gameObject).onHover += MatAndShapeHoverEvent;
            }
        }

        if (null != m_VoxelAndBlockColliders && m_VoxelAndBlockColliders.Length > 0)
        {
            for (int i = 0; i < m_VoxelAndBlockColliders.Length; i++)
            {
                UIEventListener.Get(m_VoxelAndBlockColliders[i].gameObject).onHover += VoxelAndBlockHoverEvent;
            }
        }

        if (null != m_BrushColliders && m_BrushColliders.Length > 0)
        {
            for (int i = 0; i < m_BrushColliders.Length; i++)
            {
                UIEventListener.Get(m_BrushColliders[i].gameObject).onHover += BrushHoverEvent;
            }
        }

        if (null != m_SelectColliders && m_SelectColliders.Length > 0)
        {
            for (int i = 0; i < m_SelectColliders.Length; i++)
            {
                UIEventListener.Get(m_SelectColliders[i].gameObject).onHover += SelectHoverEvent;
            }
        }

        if (null != m_MenuColliders && m_MenuColliders.Length > 0)
        {
            for (int i = 0; i < m_MenuColliders.Length; i++)
            {
                UIEventListener.Get(m_MenuColliders[i].gameObject).onHover += MenuHoverEvent;
            }
        }
    }

    #endregion

    #region public methods

    public void ShowAllBuildTutorial()
    {
        if (!m_ShowAllBuildTutorial)
        {
            StopCoroutine("ShowAllBuildTutorialInterator");
            StartCoroutine("ShowAllBuildTutorialInterator");
        }
    }

    #endregion

}
