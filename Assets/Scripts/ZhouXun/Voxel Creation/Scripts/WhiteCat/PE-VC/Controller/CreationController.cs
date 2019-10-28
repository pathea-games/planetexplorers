using System;
using UnityEngine;
using Pathea;

namespace WhiteCat
{
	struct IntV3
	{
		public int x, y, z;
		public IntV3(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}


	public sealed class CreationController : MonoBehaviour, ICloneModelHelper
	{
		[SerializeField] Transform _partRoot;
		[SerializeField] Transform _meshRoot;
		[SerializeField] Transform _decalRoot;
		[SerializeField] Transform _effectRoot;
		[SerializeField] Transform _centerObject;

		[SerializeField] bool _visible = true;
		[SerializeField] bool _collidable = true;

		[SerializeField] int _creationID = -1; // For Serialize
		CreationData _creationData;
		[SerializeField] EVCCategory _category;

		[SerializeField] Bounds _bounds;
		[SerializeField] float _robotRadius;

		[SerializeField] float _buildTime = -1f;
		[SerializeField] bool _buildFinished = false;


		public event Action onUpdate;
		Action _onBuildFinish;


        public int armorBoneIndex
        {
            get
            {
                return  creationData.m_IsoData.m_Components[0].m_ExtendData;
            }
        }


		public void Init(
			Transform partRoot,
			Transform meshRoot,
			Transform decalRoot,
			Transform effectRoot,
			CreationData creationData)
		{
			_partRoot = partRoot;
			_meshRoot = meshRoot;
			_decalRoot = decalRoot;
			_effectRoot = effectRoot;
            _creationData = creationData;
            _creationID = _creationData == null ? -1 : _creationData.m_ObjectID;

			_category = creationData.m_IsoData.m_HeadInfo.Category;

			// bounds
			CalcVoxelsBounds();
			ExtendPartsBounds();
			_bounds.Expand(0.05f);

			// 为机器人矫正 Pivot 并添加碰撞球
			if (_category == EVCCategory.cgRobot)
			{
				var root = transform.GetChild(0);
				root.SetParent(null, true);
				transform.position = boundsCenterInWorld;
				root.SetParent(transform, true);
				_bounds.center = Vector3.zero;

				float max = _bounds.size.x;
				if (_bounds.size.y > max) max = _bounds.size.y;
				if (_bounds.size.z > max) max = _bounds.size.z;

				_robotRadius = Mathf.Clamp(max * 0.5f - 0.08f, 0.1f, 0.3f);
				gameObject.AddComponent<SphereCollider>().radius = _robotRadius;
			}

			// 添加中心物体
			_centerObject = new GameObject("Center Object").transform;
			_centerObject.SetParent(transform, false);
			_centerObject.localPosition = _bounds.center;
			_centerObject.gameObject.layer = VCConfig.s_ProductLayer;
        }


		public Transform partRoot { get { return _partRoot; } }
		public Transform meshRoot { get { return _meshRoot; } }
		public Transform decalRoot { get { return _decalRoot; } }
		public Transform effectRoot { get { return _effectRoot; } }
		public Transform centerObject { get { return _centerObject; } }


        /// <summary>
        /// 可见性
        /// 每一块 mesh 生成的时候, 需设置自身的 Renderer 可见性与此相同
        /// </summary>
        public bool visible
		{
			get { return _visible; }
			set
			{
				if (_visible != value)
				{
					_visible = value;

					if (value)
					{
						for (int i = 0; i < _partRoot.childCount; i++)
						{
							var part = _partRoot.GetChild(i).GetComponent<VCPart>();
							if (part)
							{
								Renderer[] renderers = part.GetComponentsInChildren<Renderer>(true);
								foreach (Renderer r in renderers)
								{
									if (r is TrailRenderer ||
										r is ParticleRenderer ||
										r is ParticleSystemRenderer ||
										r is LineRenderer ||
										r is SpriteRenderer)
										r.enabled = true;
									else
										r.enabled = !part.hiddenModel;
								}
							}
						}
					}
					else
					{
						Renderer[] renderers = _partRoot.GetComponentsInChildren<Renderer>();
						for (int i = 0; i < renderers.Length; i++)
						{
							renderers[i].enabled = false;
						}
					}

					for (int i = 0; i < _meshRoot.childCount; i++)
					{
						_meshRoot.GetChild(i).GetComponent<MeshRenderer>().enabled = value;
					}

					_decalRoot.gameObject.SetActive(value);

					_effectRoot.gameObject.SetActive(value);
				}
			}
		}


		/// <summary>
		/// 可碰撞性
		/// 每一块 mesh 生成的时候, 需设置自身的 Collider 开关与此相同
		/// </summary>
		public bool collidable
		{
			get { return _collidable; }
			set
			{
				if (_collidable != value)
				{
					_collidable = value;

					var colliders = GetComponentsInChildren<Collider>(true);
					for (int i=0; i<colliders.Length; i++)
					{
						colliders[i].enabled = value;
					}
				}
			}
		}


		/// <summary>
		/// CreationData
		/// </summary>
		public CreationData creationData
		{
			get
            {
                if (_creationData == null && _creationID >= 0)
                {
                    _creationData = CreationMgr.GetCreation(_creationID);
                }
                return _creationData;
            }
		}


		/// <summary>
		/// Category
		/// </summary>
		public EVCCategory category
		{
			get { return _category; }
		}


		/// <summary>
		/// 本地边界框
		/// </summary>
		public Bounds bounds
		{
			get { return _bounds; }
		}


		/// <summary>
		/// 边界框半径
		/// </summary>
		public float BoundsRadius
		{
			get { return _bounds.extents.magnitude; }
		}


		public float robotRadius
		{
			get { return _robotRadius; }
		}


		/// <summary>
		/// 世界空间 bounds 中心
		/// </summary>
		public Vector3 boundsCenterInWorld
		{
			get { return transform.TransformPoint(_bounds.center); }
		}


		/// <summary>
		/// 体素是否创建完成
		/// </summary>
		public bool isBuildFinished
		{
			get { return _buildFinished; }
		}


		/// <summary>
		/// 返回边界框上（内）最接近指定点的世界坐标
		/// </summary>
		public Vector3 ClosestWorldPoint(Vector3 point)
		{
			return transform.TransformPoint(
				_bounds.ClosestPoint(
					transform.InverseTransformPoint(point)));
		}


		/// <summary>
		/// 添加体素创建完成的事件
		/// </summary>
		public void AddBuildFinishedListener(Action action)
		{
			if (!_buildFinished)
			{
				_onBuildFinish += action;
			}
			else
			{
				if (action != null) action();
			}
		}


		/// <summary>
		/// 移除体素创建完成的事件
		/// </summary>
		public void RemoveBuildFinishedListener(Action action)
		{
			if (_onBuildFinish != null)
			{
				_onBuildFinish -= action;
			}
		}


		void CalcVoxelsBounds()
		{
			IntV3 min = new IntV3(int.MaxValue, int.MaxValue, int.MaxValue);
			IntV3 max = new IntV3(int.MinValue, int.MinValue, int.MinValue);
			IntV3 cur = new IntV3();

			foreach (var pair in creationData.m_IsoData.m_Voxels)
			{
				if (pair.Value.Volume > 0)
				{
					cur.x = pair.Key & 0x3ff;
					cur.y = pair.Key >> 20;
					cur.z = (pair.Key >> 10) & 0x3ff;

					min.x = Mathf.Min(min.x, cur.x);
					min.y = Mathf.Min(min.y, cur.y);
					min.z = Mathf.Min(min.z, cur.z);

					max.x = Mathf.Max(max.x, cur.x);
					max.y = Mathf.Max(max.y, cur.y);
					max.z = Mathf.Max(max.z, cur.z);
				}
			}

			float vsize = creationData.m_IsoData.m_HeadInfo.FindSceneSetting().m_VoxelSize;
			Vector3 minPoint = new Vector3(min.x * vsize, min.y * vsize, min.z * vsize);
			Vector3 maxPoint = new Vector3(max.x * vsize, max.y * vsize, max.z * vsize);

			Vector3 offset = transform.GetChild(0).localPosition;
			_bounds.SetMinMax(minPoint + offset, maxPoint + offset);
		}


		void ExtendPartsBounds()
		{
			var renderers = GetComponentsInChildren<MeshRenderer>(true);

			if (renderers != null && renderers.Length > 0)
			{
				Quaternion originRotation = transform.localRotation;

				Vector3 currentMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				Vector3 currentMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
				Vector3 min, max;

				for (int i = 0; i < renderers.Length; i++)
				{
					var r = renderers[i];
					min = r.bounds.min;
					max = r.bounds.max;

					currentMin.x = Mathf.Min(min.x, currentMin.x);
					currentMin.y = Mathf.Min(min.y, currentMin.y);
					currentMin.z = Mathf.Min(min.z, currentMin.z);

					currentMax.x = Mathf.Max(max.x, currentMax.x);
					currentMax.y = Mathf.Max(max.y, currentMax.y);
					currentMax.z = Mathf.Max(max.z, currentMax.z);
				}

				currentMin = transform.InverseTransformPoint(currentMin);
				currentMax = transform.InverseTransformPoint(currentMax);
				min = _bounds.min;
				max = _bounds.max;

				currentMin.x = Mathf.Min(min.x, currentMin.x);
				currentMin.y = Mathf.Min(min.y, currentMin.y);
				currentMin.z = Mathf.Min(min.z, currentMin.z);

				currentMax.x = Mathf.Max(max.x, currentMax.x);
				currentMax.y = Mathf.Max(max.y, currentMax.y);
				currentMax.z = Mathf.Max(max.z, currentMax.z);

				_bounds.SetMinMax(currentMin, currentMax);

				transform.localRotation = originRotation;
			}
		}


		public void OnNewMeshBuild(MeshFilter mf)
		{
			_buildTime = -1f;
			mf.GetComponent<MeshRenderer>().enabled = _visible;

			var collider = mf.GetComponent<Collider>();
			if (collider) collider.enabled = _collidable;
		}


		void UpdateBuild()
		{
			if (_buildTime < -4.5f)
			{
				_buildTime = 0.6f;
			}
			else if (_buildTime < 0f)
			{
				_buildTime -= 1f;
			}
			else
			{
				_buildTime -= Time.unscaledDeltaTime;
				if (_buildTime < 0f)
				{
					_buildFinished = true;
					if (_onBuildFinish != null)
					{
						try { _onBuildFinish(); } catch { }
						_onBuildFinish = null;
					}
				}
			}
		}


		void Update()
		{
			if (!_buildFinished) UpdateBuild();

			if (onUpdate != null) try { onUpdate(); } catch { }
		}


		void ICloneModelHelper.ResetView()
		{
			visible = true;
		}


#if UNITY_EDITOR

		void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;

			Vector3 bdl = _bounds.min;
			Vector3 fru = _bounds.max;

			Gizmos.DrawLine(transform.TransformPoint(bdl), transform.TransformPoint(fru));

			Gizmos.color = Color.cyan * 0.5f;
			Gizmos.DrawSphere(_centerObject.position, _robotRadius);
		}

#endif
	}
}