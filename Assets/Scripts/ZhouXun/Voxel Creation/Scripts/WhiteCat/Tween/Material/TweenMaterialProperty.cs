using UnityEngine;

namespace WhiteCat.Internal
{
	/// <summary>
	/// 材质属性插值
	/// </summary>
	public abstract class TweenMaterialProperty : TweenBase
	{
		// 如果引用了材质，则操作此材质；否则从 Renderer 获得材质
		[SerializeField] Material _refMaterial;

		public new Renderer renderer;
		public int materialIndex;
		public bool useSharedMaterial;

		[SerializeField] string _propertyName;
		int _propertyID = -1;


		public Material material
		{
			get
			{
				if (_refMaterial) return _refMaterial;

				if (renderer)
				{
					Material[] materials = useSharedMaterial || !Application.isPlaying ? renderer.sharedMaterials : renderer.materials;
					if (materialIndex < 0) materialIndex = 0;
					if (materialIndex >= materials.Length) materialIndex = materials.Length - 1;
					return materials[materialIndex];
				}

				return null;
			}
			set { _refMaterial = value; }
		}


		public string propertyName
		{
			get { return _propertyName; }
			set
			{
				_propertyName = value;
				_propertyID = -1;
			}
		}


		public int propertyID
		{
			get { return _propertyID == -1 ? _propertyID = Shader.PropertyToID(_propertyName) : _propertyID; }
		}



#if UNITY_EDITOR


		public abstract UnityEditor.ShaderUtil.ShaderPropertyType propertyType { get; }


		protected void Reset()
		{
			renderer = GetComponent<Renderer>();
			materialIndex = 0;
			useSharedMaterial = true;
			_propertyName = null;
			_propertyID = -1;
		}


#endif
	}
}