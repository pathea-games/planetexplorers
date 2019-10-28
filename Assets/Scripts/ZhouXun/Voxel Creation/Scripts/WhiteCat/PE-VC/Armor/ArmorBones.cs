using UnityEngine;
using System;
using WhiteCat.UnityExtension;

namespace WhiteCat
{
	// 存储每个骨骼和装甲的引用
	// 运行时复制模型时, 为了保证所有对骨骼和装甲的引用仍然是有效的, 因此需要序列化
	[Serializable]
	public class BoneNode
	{
		public Transform bone;
		public Transform normal;
		public Transform decoration;
	}


	public class ArmorBones : MonoBehaviour
	{
		[SerializeField] BoneNode[] _nodes;

		static int[] groupFirst = new int[5] { 0, 1, 4, 12, 0 };


		public BoneNode nodes(int boneGroup, int boneIndex)
		{
			return _nodes[groupFirst[boneGroup] + boneIndex];
        }


		public BoneNode nodes(int index)
		{
			return _nodes[index];
        }


		// 获取装甲对象
		public Transform GetArmorPart(int index, bool isDecoration)
		{
			return isDecoration ? _nodes[index].decoration : _nodes[index].normal;
		}


		Transform FindChild(string name)
		{
			return transform.TraverseHierarchy(
					(t, i) => 
					{
						if (t.gameObject.name == name) return t;
						else return null;
                    }
				) as Transform;
		}


		public static string[][] boneNames = new string[4][]
		{
			new string[1]	// group 0
			{
				"Bip01 Head",		// bone 0
			},
			new string[3]	// group 1
			{
				"Bip01 Spine3",		// bone 0
				"Bip01 Spine2",		// bone 1
				"Bip01 Spine1",		// bone 2
			},
			new string[8]	// group 2
			{
				"Bip01 L UpperArm",	// bone 0
				"Bip01 R UpperArm",	// bone 1
				"Bip01 L Forearm",	// bone 2
				"Bip01 R Forearm",	// bone 3
				"Bip01 R Thigh",	// bone 4
				"Bip01 L Thigh",	// bone 5
				"Bip01 L Calf",		// bone 6
				"Bip01 R Calf",		// bone 7
			},
			new string[4]   // group 3
			{
				"Bip01 L Hand",		// bone 0
				"Bip01 R Hand",		// bone 1
				"Bip01 L Foot",		// bone 2
				"Bip01 R Foot",		// bone 3
			},
		};


		void Reset()
		{
			_nodes = new BoneNode[16];
			int index = 0;

			for (int i=0; i < boneNames.Length; i++)
			{
				for (int j=0; j < boneNames[i].Length; j++)
				{
					_nodes[index] = new BoneNode();
                    _nodes[index].bone = FindChild(boneNames[i][j]);
					index++;
                }
			}
		}
	}
}