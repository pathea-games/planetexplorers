using UnityEngine;
using System;
using System.Collections.Generic;

namespace WhiteCat
{
	public enum ArmorType
	{
		Head = 0,
		Body = 1,
		ArmAndLeg = 2,
		HandAndFoot = 3,
		Decoration = 4,

		None = 255
	}


	[Serializable]
	public class BoneGroup
	{
		public Transform bonePivot;
		public GameObject maleModel;
		public GameObject femalModel;
	}


	public class VCPArmorPivot : VCPart
	{
		[SerializeField] ArmorType _armorType;
		[SerializeField] List<BoneGroup> _boneGroups;

		bool _isMale;
		int _showIndex;

		[SerializeField][HideInInspector] bool _destroyed = false;


#if UNITY_EDITOR
		void Reset()
		{
			try
			{
				_boneGroups = new List<BoneGroup>(4);

				for (int i=0; i<transform.childCount; i++)
				{
					var child = transform.GetChild(i);
                    if (child.gameObject.name != "Editor Tools")
					{
						var grounp = new BoneGroup();
						grounp.bonePivot = child;
						grounp.maleModel = child.FindChild("MaleModel").gameObject;
						grounp.femalModel = child.FindChild("FemaleModel").gameObject;
						_boneGroups.Add(grounp);
                    }
				}
			}
			catch
			{
				UnityEditor.EditorUtility.DisplayDialog("Error", "层级异常", "OK");
			}
		}
#endif


		protected override void Awake()
		{
			base.Awake();

			if (!_destroyed)
			{
				for (int i = 0; i < _boneGroups.Count; i++)
				{
					_boneGroups[i].maleModel.SetActive(false);
					_boneGroups[i].femalModel.SetActive(false);
				}
				_isMale = true;
				_showIndex = 0;
				_boneGroups[0].maleModel.SetActive(true);
			}
		}


		public void DestroyModels()
		{
			if (!_destroyed)
			{
				for (int i = 0; i < _boneGroups.Count; i++)
				{
					Destroy(_boneGroups[i].femalModel);
					Destroy(_boneGroups[i].maleModel);
				}
				_destroyed = true;
            }
		}


		public ArmorType armorType
		{
			get { return _armorType; }
		}


		public Transform GetPivot(int index)
		{
			return _boneGroups[index].bonePivot;
        }


		public bool isMale
		{
			get { return _isMale; }
			set
			{
				if(_isMale != value)
				{
					_isMale = value;
					_boneGroups[_showIndex].femalModel.SetActive(!value);
					_boneGroups[_showIndex].maleModel.SetActive(value);
                }
			}
		}


		public int showIndex
		{
			get { return _showIndex; }
			set
			{
				if(_showIndex != value)
				{
					if (_isMale) _boneGroups[_showIndex].maleModel.SetActive(false);
					else _boneGroups[_showIndex].femalModel.SetActive(false);

					_showIndex = value;

					if (_isMale) _boneGroups[_showIndex].maleModel.SetActive(true);
					else _boneGroups[_showIndex].femalModel.SetActive(true);
				}
			}
		}
	}
}