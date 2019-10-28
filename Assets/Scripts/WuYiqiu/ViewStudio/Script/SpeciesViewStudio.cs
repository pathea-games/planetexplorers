using UnityEngine;
using System.Collections;
using System;
using Pathea;
using System.Collections.Generic;
using System.Linq;
using PETools;

public class SpeciesViewStudio : MonoBehaviour
{

    #region static field
    private static SpeciesViewStudio m_Instance;
    public static SpeciesViewStudio Instance
    {
        get
        {
            if (m_Instance == null)
            {
                if (Application.isPlaying)
                {
                    GameObject go = Resources.Load<GameObject>("Prefab/SpeciesViewStudio");
                    if (go != null)
                    {
                        m_Instance = GameObject.Instantiate(go).GetComponent <SpeciesViewStudio>();
                    }
                }
            }
            return m_Instance;
        }
    }
    #endregion

    private int m_IDInitial = 0;

    /// <summary>
    /// 创建一个ViewController
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public static PeViewController CreateViewController(ViewControllerParam param)
    {
        GameObject resGo = Resources.Load("Prefab/ViewController") as GameObject;

        if (resGo == null)
            throw new Exception("Load view camera cursor prefab failed");

        PeViewController resController = resGo.GetComponent<PeViewController>();
        if (resController == null)
            throw new Exception("Load iso cursor prefab failed");

        PeViewController controller = PeViewController.Instantiate(resController);
        controller.transform.SetParent(Instance.transform);
        controller.transform.localPosition = Vector3.zero;
        controller.transform.localScale = Vector3.one;
        controller.transform.localRotation = Quaternion.identity;
        controller.SetParam(param);
        controller.ID = Instance.m_IDInitial;
        Instance.m_IDInitial++;
        return controller;

    }

    public static GameObject LoadMonsterModelByID(int modelID,ref Pathea.MovementField movementField)
    {
        Pathea.MonsterProtoDb.Item item=Pathea.MonsterProtoDb.Get(modelID);
        if(null==item)  return null;
        string modelPath=item.modelPath;
        if(string.IsNullOrEmpty(modelPath)) return null;
        GameObject monsterGo=AssetsLoader.Instance.InstantiateAssetImm(modelPath,Vector3.zero,Quaternion.identity,Vector3.one);
        monsterGo = GetOnlyModel(monsterGo);
        if (monsterGo == null) return null;
        ResetModel(monsterGo, item.movementField);
        movementField = item.movementField;
        return monsterGo;
    }

    public static GameObject GetOnlyModel(GameObject modelGo)
    {
        if (null == modelGo) return null;
        Animator animator = modelGo.GetComponentInChildren<Animator>();
        if(null==animator) return null;
        animator.transform.SetParent(modelGo.transform.parent);
        GameObject.Destroy(modelGo);
        return animator.gameObject;
    }

    public static void ResetModel(GameObject modelGo, Pathea.MovementField movementField)
    {
        modelGo.name = "CloneModel";
        modelGo.transform.parent = m_Instance.transform;
        modelGo.transform.localPosition = Vector3.zero;
        modelGo.transform.localRotation = Quaternion.identity;
        modelGo.transform.localRotation = Quaternion.identity;
        modelGo.layer = Layer.ShowModel;

        //normal_leisure0-2 陆地
        //normalsky_leisure0-2  空中

        Animator anim = modelGo.GetComponentInChildren<Animator>();
      	if (null!=anim)
		{
            string searchStr = movementField == Pathea.MovementField.Sky ? "normalsky_leisure" : "normal_leisure";
            if (movementField == Pathea.MovementField.Sky) anim.SetBool("Fly", true);
            anim.SetTrigger(searchStr+"0");
            AnimatorClipInfo[] idleClip = anim.GetCurrentAnimatorClipInfo(0);
            if(null!=idleClip&&idleClip.Length>0)
            {
                idleClip[0].clip.SampleAnimation(anim.gameObject, idleClip[0].clip.length * 0.5f);
            }
		}
        ClearModel(modelGo);
    }

    public static void ClearModel(GameObject viewGo)
    {
        List<Transform> transList=viewGo.GetComponentsInChildren<Transform>().ToList();
        transList.Add(viewGo.transform);
        for (int i = 0; i < transList.Count; i++)
        {
            Transform trans = transList[i];
            Component[] cmpts = trans.gameObject.GetComponents<Component>();
			foreach (Component cmpt in cmpts)
			{
				if (cmpt.GetType() == typeof(SkinnedMeshRenderer)
					|| cmpt.GetType() == typeof(WhiteCat.SkinnedMeshRendererHelper)
					|| cmpt.GetType() == typeof(Animator)
					|| cmpt.GetType() == typeof(Transform)
					|| cmpt.GetType() == typeof(MeshRenderer))
					continue;

				Component.Destroy(cmpt);
			}
            trans.gameObject.layer = Layer.ShowModel;
        }
	}

    /// <summary>
    /// lz-2016.07.21 获取可以显示全部模型整体的相机距离
    /// </summary>
    /// <param name="modelGo">模型</param>
    /// <param name="viewCamera">查看模型的相机</param>
    /// <param name="viewWndSize">查看模型的视角大小</param>
    /// <returns></returns>
    public static void GetCanViewModelFullDistance(GameObject modelGo, Camera viewCamera, Vector2 viewWndSize, out float distance, out float yaw)
    {
        //Bounds modelBounds = GetModelBounds(modelGo);
        //float[] array = new float[] { modelBounds.size.x, modelBounds.size.y, modelBounds.size.z }.OrderByDescending(a => a).ToArray();
        //float maxLine1 = array[0];
        //float maxLine2 = array[1];
        //float boundsDiagonalOf2Max = Mathf.Sqrt(maxLine1 * maxLine1 + maxLine2 * maxLine2); //lz-2016.07.21 Bounds最长两条边的对角线
        //float maxViewLine = maxLine1 == modelBounds.size.y ? viewWndSize.y : viewWndSize.x;//lz-2016.07.21  对应Bounds最长那条线的View长度
        //float pxViewLine = maxLine1 == modelBounds.size.y ? viewCamera.pixelHeight : viewCamera.pixelWidth; //lz-2016.07.21  对应Bounds最长那条线的Pixel长度
        //return (pxViewLine * boundsDiagonalOf2Max * 0.5f) / maxViewLine / Mathf.Tan(viewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad); 
        //lz-2016.07.22 下面这个不用排序，更优化一些
        //Bounds modelBounds = GetModelBounds(modelGo);
        //float lineX = modelBounds.size.x;
        //float lineY = modelBounds.size.y;
        //float lineZ = modelBounds.size.z;
        //float boundsDiagonalOf2Max = Mathf.Sqrt(lineX * lineX + lineY * lineY);
        //float maxViewLine = lineX < lineY ? viewWndSize.y : viewWndSize.x; 
        //float pxViewLine = lineX < lineY ? viewCamera.pixelHeight : viewCamera.pixelWidth;
        //float clipDis = (pxViewLine * boundsDiagonalOf2Max * 0.5f) / maxViewLine / Mathf.Tan(viewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        //return clipDis + lineZ * 0.5f;
        Bounds modelBounds = GetModelBounds(modelGo);
        Vector3 size = modelBounds.size;
        float sizeXZ = Mathf.Sqrt(size.x * size.x + size.z* size.z);

        float worldSize;
        float screenSize;
        if (size.y > sizeXZ)
        {
            worldSize = size.y;
            screenSize = viewWndSize.y*0.75f;
        }
        else
        {
            worldSize = sizeXZ;
            screenSize = viewWndSize.x * 0.75f;
        }
        yaw = -Mathf.Acos(size.x / sizeXZ)*Mathf.Rad2Deg;
        distance=(worldSize * viewCamera.pixelHeight * 0.5f) / (screenSize * Mathf.Tan(viewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad));
    }

    /// <summary>
    /// lz-2016.07.21 获取模型的Bounds
    /// </summary>
    /// <param name="modelGo">模型</param>
    /// <returns>Bounds</returns>
    public static Bounds GetModelBounds(GameObject modelGo)
    {
        SkinnedMeshRenderer mesh = modelGo.GetComponentInChildren<SkinnedMeshRenderer>();
        Bounds modelBounds=new Bounds();
        if (mesh == null) return modelBounds;
        mesh.updateWhenOffscreen = true;
        modelBounds = mesh.bounds;
        return modelBounds;
    }
}
