using UnityEngine;
using System.Collections;

//自动种树脚本对于目前
//自动种树脚本移动轨迹, 从左下角起点移动到右上角终点

//z轴正方向
//^
//||
//||
//||
//||
//=========>x轴正方向

//^=>-------------------------终点
//||
//^-----------------------------<=^
//                                          ||
//^=>----------------------------^
//||
//^----------------------------<=^
//                                          ||
//起点=>-------------------------^


//自动运行camera种树的功能
public class AutoRunCamera : MonoBehaviour
{
    public float m_moveSpd = 25.0f; //摄像机移动速度
    public float m_waitSec = 30.0f; //移动到指定位置后，等待时间
    Vector3 m_offset = new Vector3(0.0f, 30.0f, 0.0f); //camera相对高度
    public Vector3 m_curPos = new Vector3();
    public int m_curCol = 0; //当前subTerrain编号
    public int m_curRow = 0;

    public float m_distX = 0.0f;
    public float m_distZ = 0.0f;
    VoxelEditor m_voxelEditor;
    public float m_elps = 0.0f; //当前等待时间

    public float m_moveStep = 256.0f; //subTerrain范围
    public float  m_terrainSize = 24576.0f; //地形范围

    int m_maxRow = 0; //地形范围除以子地形范围
//    int m_maxRowDec1 = 0;
    int m_maxCol = 0; //地形范围除以子地形范围
//    int m_maxColDec1 = 0;

    //自动运行状态
    public enum AutoRunState
    {
        eIdle,
        eMove,
        eWaitFillVegetation,
    };

    //camera移动方向, 向+x轴，向-x轴，向+z轴
    public enum MoveDir
    {
        eRight,
        eLeft,
        eUp,
    };

    public MoveDir m_eMoveDir = MoveDir.eRight;
    public AutoRunState m_eRS = AutoRunState.eIdle;

    void Start()
    {
        m_voxelEditor = GameObject.Find("Voxel Terrain").GetComponent<VoxelEditor>();
        //int m_maxRow = (int)(m_terrainSize / m_moveStep); //地形范围除以子地形范围, 比如 24576.0f / 256.0f = 96
        //int m_maxRowDec1 = m_maxRow - 1;
//        int m_maxCol = m_maxRow; //地形范围除以子地形范围, 比如 24576.0f / 256.0f = 96
//        int m_maxColDec1 = m_maxRowDec1;
    }

    // Update is called once per frame
    void Update()
    {
        //如果当前状态时等待状态时
        if (m_eRS == AutoRunState.eIdle)
        {
            //按下键盘右键激活自动运行状态
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                m_curCol = (int)(transform.position.x / m_moveStep);
                m_curRow = (int)(transform.position.z / m_moveStep);
                m_curCol = Mathf.Clamp(m_curCol, 0, m_maxCol);
                m_curRow = Mathf.Clamp(m_curRow, 0, m_maxRow);

                if (m_curRow == m_maxRow && m_curCol == m_maxCol)
                    Debug.Log("AutoRunCamera::reach end point !!!");
                else
                {
                    m_curPos.x = m_curCol * m_moveStep;// +m_offset.x;
                    m_curPos.z = m_curRow * m_moveStep;// +m_offset.z;
                    m_curPos.y = transform.position.y;
                    transform.position = m_curPos;

                    if (m_curCol == m_maxCol)
                    {
                        m_eMoveDir = MoveDir.eUp;
                        m_distZ = m_curPos.z + m_moveStep;
                    }
                    else
                    {
                        m_eMoveDir = MoveDir.eRight;
                        m_distX = m_curPos.x + m_moveStep;
                        //m_eMoveDir = MoveDir.eLeft;
                        //m_distX = m_curPos.x - m_moveStep;
                    }

                    m_eRS = AutoRunState.eMove;
                }
            }
        }

        //如果当前状态是移动状态
        if (m_eRS == AutoRunState.eMove)
        {
            m_curPos = this.transform.position;
            float _step = m_moveSpd * Time.deltaTime;

            if (m_eMoveDir == MoveDir.eRight)  //如果移动方向向右
            {
                m_curPos.x += _step;

                if (m_curPos.x >= m_distX || m_curPos.x >= m_terrainSize) //8189.0f)
                {
                    m_curPos.x = m_distX;
                    m_eRS = AutoRunState.eWaitFillVegetation;
                    ++m_curCol;
                }
                        
            }
            else if (m_eMoveDir == MoveDir.eLeft) //如果移动方向向左
            {
                m_curPos.x -= _step;
                if (m_curPos.x <= m_distX || m_curPos.x <= 0.0f)
                {
                    m_curPos.x = m_distX;
                    m_eRS = AutoRunState.eWaitFillVegetation;
                    --m_curCol;
                }
            }
            else if (m_eMoveDir == MoveDir.eUp) //如果移动方向向
            {
                m_curPos.z += _step;
                if (m_curPos.z >= m_distZ)
                {
                    m_curPos.z = m_distZ;
                    m_eRS = AutoRunState.eWaitFillVegetation;
                    ++m_curRow;
                }
            }

            m_curPos.y = 1600.0f;
            Ray _ray = new Ray(m_curPos, new Vector3(0.0f, -1.0f, 0.0f));
            RaycastHit _raycastHit;
            if (Physics.Raycast(_ray, out _raycastHit, m_curPos.y))
            {
                m_curPos = _raycastHit.point;
                m_curPos.y += m_offset.y;
                this.transform.position = m_curPos;
            }
        }

        //如果当前状态时等待种植植被的状态
        if (m_eRS == AutoRunState.eWaitFillVegetation)
        {
			if (!VFVoxelTerrain.self.IsInGenerating)
            {
                m_elps += Time.deltaTime;
                if (m_elps >= 6.0f)
                {
                    m_voxelEditor.AutoFillVegetation();
                    m_elps = 0.0f;
                    if (m_curCol == m_maxCol && m_curRow == m_maxRow)
                        m_eRS = AutoRunState.eIdle;
                    else
                    {
                        if (m_eMoveDir == MoveDir.eRight)
                        {
                            if (m_curCol == m_maxCol)
                            {
                                m_eMoveDir = MoveDir.eUp;
                                m_distZ = m_curPos.z + m_moveStep;
                            }
                            else
                                m_distX = m_curPos.x + m_moveStep;
                        }
                        else if (m_eMoveDir == MoveDir.eLeft)
                        {
                            if (m_curCol == 0)
                            {
                                m_eMoveDir = MoveDir.eUp;
                                m_distZ = m_curPos.z + m_moveStep;
                            }
                            else
                                m_distX = m_curPos.x - m_moveStep;
                        }
                        else if (m_eMoveDir == MoveDir.eUp)
                        {
                            if (m_curCol == 0)
                            {
                                m_eMoveDir = MoveDir.eRight;
                                m_distX = m_curPos.x + m_moveStep;
                            }
                            else if (m_curCol == m_maxCol)
                            {
                                m_eMoveDir = MoveDir.eLeft;
                                m_distX = m_curPos.x - m_moveStep;
                            }
                        }

                        m_eRS = AutoRunState.eMove;
                    }
                }
            }

        }
    }
}
