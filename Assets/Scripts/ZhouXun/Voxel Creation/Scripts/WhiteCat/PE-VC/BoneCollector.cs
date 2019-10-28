using System;
using System.Collections.Generic;
using UnityEngine;
using WhiteCat.UnityExtension;

namespace WhiteCat
{
	public class BoneCollector : MonoBehaviour
	{
		[Serializable]
		public struct BoneGroup
		{
			public string name;
			public Transform model;
			public Transform ragdoll;
		}


		struct EquipGroup
		{
			public int bone;
			public Transform equip;
		}


		[SerializeField]
		List<BoneGroup> _boneGroups;
		List<EquipGroup> _equipGroup = new List<EquipGroup>(4);
		bool _isRagdoll = false;

		int FindBoneGroup(string boneName)
		{
			for(int i=0; i<_boneGroups.Count; i++)
			{
				if (_boneGroups[i].name == boneName) return i;
            }

			return -1;
		}

        void SetModelParent(Transform equipment, Transform parent)
        {
            equipment.SetParent(parent, false);

            //ConfigurableJoint joint = equipment.GetComponentInChildren<ConfigurableJoint>();
            //if(joint != null)
            //{
            //    joint.connectedBody = null;
            //}
        }

        void SetRagdollParent(Transform equipment, Transform parent)
        {
            equipment.SetParent(parent, false);

            //ConfigurableJoint joint = equipment.GetComponentInChildren<ConfigurableJoint>();
            //if(joint != null)
            //{
            //    joint.connectedBody = parent.GetComponentInParent<Rigidbody>();
            //}
        }


		public int FindEquipGroup(Transform equipment)
		{
			for (int i = 0; i < _equipGroup.Count; i++)
			{
				if (_equipGroup[i].equip == equipment) return i;
			}

			return -1;
		}


		public Transform FindModelBone(string boneName)
		{
			return _boneGroups[FindBoneGroup(boneName)].model;
        }


		public void AddEquipment(Transform equipment, string boneName)
		{
			int boneIndex = FindBoneGroup(boneName);
			if(boneIndex >= 0)
			{
				//equipment.SetParent(_isRagdoll ? _boneGroups[boneIndex].ragdoll : _boneGroups[boneIndex].model, false);

                if (isRagdoll)
                    SetRagdollParent(equipment, _boneGroups[boneIndex].ragdoll);
                else
                    SetModelParent(equipment, _boneGroups[boneIndex].model);

				EquipGroup equipGroup = new EquipGroup();
				equipGroup.bone = boneIndex;
				equipGroup.equip = equipment;
                _equipGroup.Add(equipGroup);
            }
		}


		public bool RemoveEquipment(Transform equipment)
		{
			int equipIndex = FindEquipGroup(equipment);
			if (equipIndex >= 0)
			{
				_equipGroup.RemoveAt(equipIndex);
				equipment.SetParent(null, false);
				return true;
            }
			return false;
        }


		public void SwitchBone(Transform equipment, string boneName)
		{
			if (RemoveEquipment(equipment))
			{
				AddEquipment(equipment, boneName);
			}
        }


		public bool isRagdoll
		{
			get { return _isRagdoll; }
			set
			{
				if (value != _isRagdoll)
				{
					_isRagdoll = value;
					EquipGroup equipGroup;
					for (int i = 0; i < _equipGroup.Count; i++)
					{
						equipGroup = _equipGroup[i];
						if (equipGroup.equip)
						{
							//equipGroup.equip.SetParent(value ? _boneGroups[equipGroup.bone].ragdoll : _boneGroups[equipGroup.bone].model, false);
                            if (_isRagdoll)
                                SetRagdollParent(equipGroup.equip, _boneGroups[equipGroup.bone].ragdoll);
                            else
                                SetModelParent(equipGroup.equip, _boneGroups[equipGroup.bone].model);
                        }
					}
				}
			}
		}


#if UNITY_EDITOR

		public void Reset()
		{
			_boneGroups = new List<BoneGroup>();
			BoneGroup group = new BoneGroup();
			string[] names = BoneConfig.instance.boneNames;
			Transform model = null, ragdoll = null;

			for(int i=0; i<transform.childCount; i++)
			{
				string name = transform.GetChild(i).name;

                if ((name.Contains("model") || name.Contains("Model")) && !name.Contains("defencemodel"))
				{
					if (model == null) model = transform.GetChild(i);
					else
					{
						Debug.LogError("Model Error: " + gameObject.name);
						return;
					}
				}

				if (name.Contains("ragdoll") || name.Contains("Ragdoll"))
				{
					if (ragdoll == null) ragdoll = transform.GetChild(i);
					else
					{
						Debug.LogError("Ragdoll Error: " + gameObject.name);
						return;
					}
				}
			}

			if (model == null || ragdoll == null)
			{
				Debug.LogError("Prefab Error: " + gameObject.name);
				return;
			}

			for(int i=0; i<names.Length; i++)
			{
				group.name = names[i];

				group.model = (Transform)model.TraverseHierarchy
					(
						(Transform t, int d) =>
						{
							if (t.gameObject.name == group.name) return t;
							return null;
						}
                    );

				group.ragdoll = (Transform)ragdoll.TraverseHierarchy
					(
						(Transform t, int d) =>
						{
							if (t.gameObject.name == group.name) return t;
							return null;
						}
					);

				int c = 0;
				if (group.model) c++;
				if (group.ragdoll) c++;

				if (c==1)
				{
					Debug.LogError(gameObject.name + " Bone Error: " + group.name);
					return;
				}

				if (c == 2)
				{
					_boneGroups.Add(group);
				}
            }
		}


		public bool isValid
		{
			get { return _boneGroups != null && _boneGroups.Count != 0; }
		}

#endif
	}
}