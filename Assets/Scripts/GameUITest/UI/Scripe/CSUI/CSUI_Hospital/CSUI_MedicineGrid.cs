using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

public class CSUI_MedicineGrid : MonoBehaviour
{

    public bool m_CanDragMedicine = true;

    [SerializeField]
    private Grid_N m_GridPrefab;


    public Grid_N m_Grid;

    [SerializeField]
    public UILabel m_NeedCntLb;

    private int m_NeedCnt;
    public int NeedCnt    //需要的数量
    {
        get
        {
            return m_NeedCnt;
        }

        set
        {
            m_NeedCnt = value;
            m_NeedCntLb.text = "X " + m_NeedCnt.ToString();
        }
    }

    public int ItemNum   //库存数量
    {
        get
        {
            if (m_Grid.Item == null)
                return 0;
            return m_Grid.Item.stackCount;
        }

        set
        {
            if (m_Grid.Item != null)
                m_Grid.Item.stackCount = value;
        }
    }

    public int ItemID    //物品ID
    {
        get
        {
            if (m_Grid.Item == null)
                return 0;
            return m_Grid.Item.protoId;
        }

        set
        {
            if (value <= 0)
                m_Grid.SetItem(null);
            else
                m_Grid.SetItem(new ItemSample(value, 0));
        }
    }


    //List<int> MedicineLis = new List<int>() { 27, 81, 82000, 83000 };


    public delegate void RealOp(Grid_N grid);
    public event RealOp mRealOp;

    void OnDropItem(Grid_N grid)
    {
        if (!m_CanDragMedicine)
            return;
        //if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_Bag && grid.ItemObj == null && ItemCheck(SelectItem_N.Instance.ItemObj.protoData.itemClassId))
        //{
        //    SelectItem_N.Instance.RemoveOriginItem();
        //    grid.SetItem(SelectItem_N.Instance.ItemObj);
        //    SelectItem_N.Instance.SetItem(null);
        //    MedicineDragInEvent(grid);
        //}
        //else
        //    SelectItem_N.Instance.SetItem(null);
        if (mRealOp != null)
            mRealOp(grid);

    }

    //private bool ItemCheck(int _id)
    //{
    //    if (MedicineLis.Contains(_id))
    //        return true;
    //    return false;
    //}

    void OnLeftMouseClicked(Grid_N grid)
    {
        if (!m_CanDragMedicine)
            return;
        if (grid.Item == null)
            return;
        SelectItem_N.Instance.SetItemGrid(grid);
    }

    void onRemoveOriginItem(Grid_N grid)
    {
        //ItemObject oldItem = grid.ItemObj;
        grid.SetItem(null);
        //MedicineDragOutEvent(grid);
    }



    void Awake()
    {
        //m_Grid = Instantiate(m_GridPrefab) as Grid_N;
        //m_Grid.transform.parent = transform;
        //m_Grid.transform.localPosition = Vector3.zero;
        //m_Grid.transform.localScale = Vector3.one;
        //m_Grid.transform.localRotation = Quaternion.identity;
    }

    void Start()
    {
        m_Grid.SetItemPlace(ItemPlaceType.IPT_Hospital, 0);
        m_Grid.onDropItem = OnDropItem;
        m_Grid.onLeftMouseClicked = OnLeftMouseClicked;
        m_Grid.onRemoveOriginItem = onRemoveOriginItem;
    }


    void Update()
    {

    }


    public delegate void MedicineDragDel(Grid_N grid);
    //public event MedicineDragDel MedicineDragInEvent;
    //public event MedicineDragDel MedicineDragOutEvent;




}
