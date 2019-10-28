using UnityEngine;

/// <summary>
/// 创造物部件 (功能默认关闭)
/// </summary>
namespace WhiteCat
{
	public enum Direction
	{
		Forward,
		Back,
		Left,
		Right,
		Up,
		Down
	}


	public abstract class VCPart : MonoBehaviour
	{
		// 用于创建系统的描述文本
		string _description;

		[SerializeField] int _descriptionID;
		[HideInInspector] public bool hiddenModel;


		public static Direction GetDirection(Vector3 vector)
		{
			int maxIndex = 0;	// X
			float maxValue = Mathf.Abs(vector.x);

			if (Mathf.Abs(vector.y) > maxValue)
			{
				maxIndex = 1;	// Y
				maxValue = Mathf.Abs(vector.y);
			}

			if (Mathf.Abs(vector.z) > maxValue)
			{
				maxIndex = 2;	// Z
			}

			if (maxIndex == 0)
			{
				return vector.x < 0 ? Direction.Left : Direction.Right;
			}
			else if (maxIndex == 1)
			{
				return vector.y < 0 ? Direction.Down : Direction.Up;
			}
			else
			{
				return vector.z < 0 ? Direction.Back : Direction.Forward;
			}
		}


		public void InvalidDescription()
		{
			_description = null;
		}


		public string description
		{
			get
			{
				if(_description == null)
				{
					_description = BuildDescription();
				}
				return _description;
			}
		}


		/// <summary>
		/// 创建描述文本
		/// </summary>
		protected virtual string BuildDescription()
		{
			return PELocalization.GetString(_descriptionID);
		}


		/// <summary>
		/// 初始化关闭功能
		/// </summary>
		protected virtual void Awake()
		{
			enabled = false;
		}
	}
}