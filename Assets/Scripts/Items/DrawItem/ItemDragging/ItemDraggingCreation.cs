using UnityEngine;
using WhiteCat;
using Pathea.Maths;

public class ItemDraggingCreation : ItemDraggingArticle
{
	CreationController controller;
	Vector3[] locVecs;              // bounds 的 8 个顶点
	float boundsHeight;             // bounds 底部距离 pivot 的高度
	Vector3 boundsSize;				// bounds 大小

	int detectCount;				// 检测失败时不断尝试的总次数
	float detectStep;               // 每次重新尝试时向上移动的距离

	static Vector3[] vecs = new Vector3[8];

	// Used for GL

	bool _valid = false;
	float _lastOffsetY;
    bool _inDungeonMsgTip = false;


    public override void OnDragOut()
	{
		DraggingDistance = 50f;
		controller = GetComponent<CreationController>();
		transform.rotation = Quaternion.identity;

		var bounds = controller.bounds;
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		boundsHeight = min.y;
		boundsSize = max - min;

		locVecs = new Vector3[8];

		locVecs[0] = min;
		locVecs[1] = new Vector3(max.x, min.y, min.z);
		locVecs[2] = new Vector3(max.x, min.y, max.z);
		locVecs[3] = new Vector3(min.x, min.y, max.z);
		locVecs[4] = new Vector3(min.x, max.y, min.z);
		locVecs[5] = new Vector3(max.x, max.y, min.z);
		locVecs[6] = max;
		locVecs[7] = new Vector3(min.x, max.y, max.z);

		float offset = Mathf.Clamp((max.z - min.z) * 0.5f, 1f, 32f);
		if (offset > 5)
		{
			detectStep = 0.5f;
			detectCount = Mathf.RoundToInt(offset / detectStep);
		}
		else
		{
			detectCount = 10;
			detectStep = offset / detectCount;
		}

		controller.collidable = false;
	}


    



    public override bool OnDragging(Ray cameraRay)
	{
        if (RandomDungenMgrData.InDungeon)
        {
            //lz-2016.09.29 在副本中提示可以使用Iso載具和机器人
            if (!_inDungeonMsgTip)
            {
                PeTipMsg.Register(PELocalization.GetString(8000732),PeTipMsg.EMsgLevel.Warning);
                _inDungeonMsgTip = true;
            }
            return false;
        }

		// 限制只能拖出一个机器人
		if (RobotController.playerFollower && controller.category == EVCCategory.cgRobot)
		{
			_valid = false;
			mTooFar = true;
			return false;
		}

		// 因为 collidable 已经关闭，所以射线检测不需要排除自身
		if (Physics.Raycast(cameraRay, out fhitInfo, DraggingDistance, PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore))
		{
			mTooFar = false;

			_valid = SetPos(fhitInfo.point);
		
			if(!rootGameObject.activeSelf)
			{
				rootGameObject.SetActive(true);
			}

			return _valid;
		}
		else
		{
			_valid = false;
			mTooFar = true;
			return false;
		}
	}


	new bool SetPos(Vector3 newPos)
	{
		Vector3 origin = newPos;
		newPos.y = newPos.y - boundsHeight + 0.1f;
		transform.position = newPos;
		
		// 转换本地坐标到世界空间
		for (int i=0; i < 8; i++)
		{
			vecs[i] = transform.TransformPoint(locVecs[i]);
		}
		Vector3 dir0to3 = vecs[3] - vecs[0];
		Vector3 dir0to1 = vecs[1] - vecs[0];

		// 检测多个不同的高度
		_valid = false;

		for (int i=0; i < detectCount; i++)
		{
			_valid = true;

			// 检查位置是否有效
			if (Physics.Linecast(vecs[0], vecs[6], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore)
				|| Physics.Linecast(vecs[1], vecs[7], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore)
				|| Physics.Linecast(vecs[5], vecs[3], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore)
				|| Physics.Linecast(vecs[4], vecs[2], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore)
				|| Physics.CapsuleCast(vecs[0], vecs[1], 0.01f, dir0to3, boundsSize.z, PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore)
				|| Physics.CapsuleCast(vecs[0], vecs[4], 0.01f, dir0to3, boundsSize.z, PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore)
				|| Physics.CapsuleCast(vecs[1], vecs[5], 0.01f, dir0to3, boundsSize.z, PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore)
				|| Physics.CapsuleCast(vecs[0], vecs[4], 0.01f, dir0to1, boundsSize.x, PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore)
				|| Physics.CapsuleCast(vecs[3], vecs[7], 0.01f, dir0to1, boundsSize.x, PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore)
				|| Physics.CapsuleCast(vecs[4], vecs[5], 0.01f, dir0to3, boundsSize.z, PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore)
				|| Physics.Linecast(vecs[6], vecs[0], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore)
				|| Physics.Linecast(vecs[7], vecs[1], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore)
				|| Physics.Linecast(vecs[3], vecs[5], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore)
				|| Physics.Linecast(vecs[2], vecs[4], PEVCConfig.instance.creationDraggingLayerMask, QueryTriggerInteraction.Ignore))
			{
				_valid = false;

				newPos.y += detectStep;

				for (int j = 0; j < 8; j++)
				{
					vecs[j].y += detectStep;
				}
			}
			else break;
		}

		if (_valid)
		{
			transform.position = newPos;
			_lastOffsetY = newPos.y - origin.y;
		}
		else
		{
			origin.y += _lastOffsetY;
			transform.position = origin;
		}

		return _valid;
	}


	public override bool OnPutDown()
	{
		if (NetworkInterface.IsClient)
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				IntVector3 safePos = new IntVector3(transform.position + 0.1f * Vector3.down);
				byte mTerrianType = VFVoxelTerrain.self.Voxels.SafeRead(safePos.x, safePos.y, safePos.z).Type;
				PlayerNetwork.mainPlayer.RequestDragOut(itemDragging.itemObj.instanceId, transform.position, transform.localScale, transform.rotation, mTerrianType);
			}
		}
		else
		{
			PutDown(true);
		}

		return false;
	}


	void OnEnable()
	{
		Camera.onPostRender += OnGL;
	}


	void OnDisable()
	{
		Camera.onPostRender -= OnGL;
	}


	void OnGL(Camera camera)
	{
		if (camera == Camera.main && locVecs != null)
		{
			PEVCConfig.instance.handleMaterial.SetPass(0);
			GL.PushMatrix();
			GL.MultMatrix(transform.localToWorldMatrix);

			GL.Begin(GL.LINES);
			GL.Color(_valid ? PEVCConfig.instance.dragValidLineColor : PEVCConfig.instance.dragInvalidLineColor);

			GL.Vertex(locVecs[0]); GL.Vertex(locVecs[1]);
			GL.Vertex(locVecs[1]); GL.Vertex(locVecs[2]);
			GL.Vertex(locVecs[2]); GL.Vertex(locVecs[3]);
			GL.Vertex(locVecs[3]); GL.Vertex(locVecs[0]);
			GL.Vertex(locVecs[4]); GL.Vertex(locVecs[5]);
			GL.Vertex(locVecs[5]); GL.Vertex(locVecs[6]);
			GL.Vertex(locVecs[6]); GL.Vertex(locVecs[7]);
			GL.Vertex(locVecs[7]); GL.Vertex(locVecs[4]);
			GL.Vertex(locVecs[0]); GL.Vertex(locVecs[4]);
			GL.Vertex(locVecs[1]); GL.Vertex(locVecs[5]);
			GL.Vertex(locVecs[2]); GL.Vertex(locVecs[6]);
			GL.Vertex(locVecs[3]); GL.Vertex(locVecs[7]);

			GL.End();

			GL.Begin(GL.QUADS);
			GL.Color(_valid ? PEVCConfig.instance.dragValidPlaneColor : PEVCConfig.instance.dragInvalidPlaneColor);

			GL.Vertex(locVecs[0]); GL.Vertex(locVecs[1]); GL.Vertex(locVecs[2]); GL.Vertex(locVecs[3]);
			GL.Vertex(locVecs[4]); GL.Vertex(locVecs[5]); GL.Vertex(locVecs[6]); GL.Vertex(locVecs[7]);
			GL.Vertex(locVecs[0]); GL.Vertex(locVecs[1]); GL.Vertex(locVecs[5]); GL.Vertex(locVecs[4]);
			GL.Vertex(locVecs[3]); GL.Vertex(locVecs[2]); GL.Vertex(locVecs[6]); GL.Vertex(locVecs[7]);
			GL.Vertex(locVecs[1]); GL.Vertex(locVecs[2]); GL.Vertex(locVecs[6]); GL.Vertex(locVecs[5]);
			GL.Vertex(locVecs[0]); GL.Vertex(locVecs[3]); GL.Vertex(locVecs[7]); GL.Vertex(locVecs[4]);

			GL.End();

			GL.PopMatrix();
		}
	}
}