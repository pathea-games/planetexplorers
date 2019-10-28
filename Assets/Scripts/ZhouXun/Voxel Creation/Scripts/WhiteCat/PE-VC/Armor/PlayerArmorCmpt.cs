using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using ItemAsset;
using Pathea;

namespace WhiteCat
{

	public class PlayerArmorCmpt : PeCmpt, IPeMsg
	{
		#region Classes -----------------------------------------------------------------------

		// 用于读取和保存装甲部件的数据结构
		class ArmorPartData
		{
			int _itemId;
			ArmorType _type;

			public int boneGroup;
			public int boneIndex;
			public bool mirrored;

			public Vector3 localPosition;
			public Vector3 localEulerAngles;
			public Vector3 localScale;


			public int itemId { get { return _itemId; } }
			public ArmorType type { get { return _type; } }


			private ArmorPartData() { }


			// 构造装甲部件数据
			public ArmorPartData(int itemId, ArmorType type)
			{
				_itemId = itemId;
				_type = type;

				boneGroup = (type == ArmorType.Decoration) ? 0 : ((int)type);
				boneIndex = 0;
				mirrored = false;
				localPosition = Vector3.zero;
				localEulerAngles = Vector3.zero;
				localScale = Vector3.one;
			}


			// 序列化
			public void Serialize(BinaryWriter w)
			{
				w.Write(_itemId);
				w.Write((byte)_type);

				// 7: mirrored; 654: boneGroup; 3210: boneIndex
				w.Write((byte)((mirrored ? 0x80 : 0x00) | (boneGroup << 4) | boneIndex));

				w.Write(localPosition.x);
				w.Write(localPosition.y);
				w.Write(localPosition.z);

				w.Write(localEulerAngles.x);
				w.Write(localEulerAngles.y);
				w.Write(localEulerAngles.z);

				w.Write(localScale.x);
				w.Write(localScale.y);
				w.Write(localScale.z);
			}


			// 反序列化
			public static ArmorPartData Deserialize(BinaryReader r)
			{
				var data = new ArmorPartData();

				data._itemId = r.ReadInt32();
				data._type = (ArmorType)(r.ReadByte());

				// 7: mirrored; 654: boneGroup; 3210: boneIndex
				byte tmp = r.ReadByte();
				data.mirrored = (tmp & 0x80) != 0;
				data.boneGroup = (tmp & 0x70) >> 4;
				data.boneIndex = tmp & 0x0F;

				data.localPosition.x = r.ReadSingle();
				data.localPosition.y = r.ReadSingle();
				data.localPosition.z = r.ReadSingle();

				data.localEulerAngles.x = r.ReadSingle();
				data.localEulerAngles.y = r.ReadSingle();
				data.localEulerAngles.z = r.ReadSingle();

				data.localScale.x = r.ReadSingle();
				data.localScale.y = r.ReadSingle();
				data.localScale.z = r.ReadSingle();

				return data;
			}
		}


		// 装甲对象
		// 一个装甲对象对应一个 ArmorPartData
		class ArmorPartObject
		{
			PlayerArmorCmpt _character;     // 角色对象
			ArmorPartData _data;            // 对应的数据
			ItemObject _item;               // 物品数据

			int _boneGroup;                 // 当前装备的骨骼组
			int _boneIndex;                 // 当前装备的骨骼索引

			Transform _armor;               // 装甲对象
			VCPArmorPivot _pivot;           // pivot 部件

			Durability _durability;
			SkillSystem.SkBuffInst _defence;

			static List<int> _defenceIndex;
			static List<float> _defenceValue;
			int[] _armorID = new int[1];


			static ArmorPartObject()
			{
				_defenceIndex = new List<int>(1);
				_defenceValue = new List<float>(1);
				_defenceIndex.Add(0);
				_defenceValue.Add(0f);
			}


			// 构造对象
			public ArmorPartObject(PlayerArmorCmpt character, ArmorPartData data, ItemObject item)
			{
				_character = character;
				_data = data;
				_item = item;
				_armorID[0] = item.instanceId;

				_boneGroup = -1;
				_boneIndex = -1;

				if (data.type == ArmorType.Decoration) _character._decorationCount++;
				_durability = _item.GetCmpt<Durability>();

				SyncAttachedBone();

				// 添加防御效果
				AddDefence();

				if (_character.hasModelLayer) CreateModel();

				_character.TriggerAddOrRemoveEvent();
			}


			// 在 Armor GameObject 创建后调用(或移动骨骼后)
			void AddToCharacterModel()
			{
				if (_armor)
				{
					_character._boneCollector.AddEquipment(_armor, ArmorBones.boneNames[_boneGroup][_boneIndex]);

					if (_data.type == ArmorType.Decoration)
					{
						_character._modelCmpt.nodes(_boneGroup, _boneIndex).decoration = _armor;
					}
					else
					{
						_character._modelCmpt.nodes(_boneGroup, _boneIndex).normal = _armor;
					}
				}
			}


			// 在 Armor GameObject 删除前调用(或移动骨骼前)
			void RemoveFromCharacterModel()
			{
				if (_armor)
				{
					_character._boneCollector.RemoveEquipment(_armor);

					if (_data.type == ArmorType.Decoration)
					{
						_character._modelCmpt.nodes(_boneGroup, _boneIndex).decoration = null;
					}
					else
					{
						_character._modelCmpt.nodes(_boneGroup, _boneIndex).normal = null;
					}
				}
			}


			// 创建 GameObject. 在收到 view build 消息时调用. ArmorPartObject 创建时, 如果 view 已经存在则调用
			public void CreateModel()
			{
				if (!_armor)
				{
					_armor = item.GetCmpt<Instantiate>().CreateViewGameObj(null).transform;
					_pivot = _armor.GetComponentInChildren<VCPArmorPivot>();
					_pivot.DestroyModels();

					SyncModelPivot();
					AddToCharacterModel();
					SyncModelFirstPersonMode();
				}
			}


			// 销毁 GameObject. 在需要删除 view 时调用 (实际上父级已被直接删除, 所以不需调用). ArmorPartObject 销毁时, 如果 view 已经存在则调用
			public void DestroyModel()
			{
				if (_armor)
				{
					RemoveFromCharacterModel();

					Destroy(_armor.gameObject);
					_armor = null;
					_pivot = null;
				}
			}


			// 角色被攻击时调用
			public void OnBeAttacked(float delta, SkillSystem.SkEntity caster)
			{
				if (PeGameMgr.IsMulti)
				{
					PlayerNetwork.mainPlayer.RequestArmorDurability(_character.Entity.Id, _armorID, delta, caster);
				}
				else
				{
					_durability.Expend(delta);
					if (_durability.floatValue.current == 0f) RemoveDefence();
				}
			}


			public bool isBroken
			{
				get { return _durability.floatValue.current == 0f; }
			}


			// 添加防御效果
			void AddDefence()
			{
				if (_defence == null && _durability.floatValue.current != 0f)
				{
					_defenceValue[0] = VCUtility.GetArmorDefence(_durability.valueMax);
					_defence = SkillSystem.SkEntity.MountBuff(_character.Entity.aliveEntity, 30200129, _defenceIndex, _defenceValue);
				}
			}


			// 移出防御效果
			public void RemoveDefence()
			{
				if (_defence != null)
				{
					SkillSystem.SkEntity.UnmountBuff(_character.Entity.aliveEntity, _defence);
					_defence = null;
				}
			}


			public ItemObject item { get { return _item; } }
			public ArmorPartData data { get { return _data; } }


			// 同步骨骼. ArmorPartObject 创建时, 或切换骨骼时调用
			public void SyncAttachedBone()
			{
				if (_boneGroup == _data.boneGroup && _boneIndex == _data.boneIndex)
				{
					return;
				}

				if (_boneGroup >= 0)
				{
					RemoveFromCharacterModel();

					if (_data.type == ArmorType.Decoration)
					{
						_character._boneNodes[_boneGroup][_boneIndex].decoration = null;
					}
					else
					{
						_character._boneNodes[_boneGroup][_boneIndex].normal = null;
					}
				}

				SyncModelPivot();
				_boneGroup = _data.boneGroup;
				_boneIndex = _data.boneIndex;

				AddToCharacterModel();

				if (_data.type == ArmorType.Decoration)
				{
					_character._boneNodes[_boneGroup][_boneIndex].decoration = this;
				}
				else
				{
					_character._boneNodes[_boneGroup][_boneIndex].normal = this;
				}
			}


			// 对齐 Pivot. 模型创建时, 或切换骨骼时调用
			public void SyncModelPivot()
			{
				// 装甲部件内部含有多个 bonePivot, 装备到不同 bone 时需对齐到对应 bonePivot 上
				// 对于非 decoration 类型, 其需要对齐的 bonePivot 索引等于装备到的 bone 在组内的索引
				// 对于 decoration 类型, 只含有一个 bonePivot, 只需要在创建时执行一次对齐即可

				if (_armor)
				{
					_armor.SetParent(null, false);
					_armor.localPosition = Vector3.zero;
					_armor.localRotation = Quaternion.identity;
					_armor.localScale = Vector3.one;

					int boneIndex = 0;
					if (_data.type != ArmorType.Decoration)
					{
						boneIndex = _data.boneIndex;
					}

					var root = _armor.GetChild(0);
					root.SetParent(null, true);

					var trans = _pivot.GetPivot(boneIndex);
					_armor.position = trans.position;
					_armor.rotation = trans.rotation;

					root.SetParent(_armor, true);

					SyncModelPosition();
					SyncModelEulerAngles();
					SyncModelScale();
				}
			}


			//
			// 根据装甲数据, 同步装甲对象的位置, 旋转和缩放
			//

			public void SyncModelPosition()
			{
				if (_armor) _armor.localPosition = _data.localPosition;
			}

			public void SyncModelEulerAngles()
			{
				if (_armor) _armor.localEulerAngles = _data.localEulerAngles;
			}

			public void SyncModelScale()
			{
				if (_armor) _armor.localScale = _data.localScale;
			}


			// 模型创建时, 或游戏 1rd-3rd 模式切换时调用
			public void SyncModelFirstPersonMode()
			{
				if (_armor) _armor.GetComponent<CreationController>().visible = !_character._isFirstPersonMode;
			}


			// 移除装甲部件
			public void RemoveArmorPart()
			{
				if (_data.type == ArmorType.Decoration)
				{
					_character._decorationCount--;
					_character._boneNodes[_boneGroup][_boneIndex].decoration = null;
				}
				else
				{
					_character._boneNodes[_boneGroup][_boneIndex].normal = null;
				}

				DestroyModel();
				RemoveDefence();
				_character.TriggerAddOrRemoveEvent();
			}
		}


		// 骨骼节点, 记录每个骨骼装甲情况
		class BoneNode
		{
			public ArmorPartObject normal;
			public ArmorPartObject decoration;
		}

		#endregion


		#region Fields ------------------------------------------------------------------------

		// 最多允许装备的装甲数量
		const int _maxDecorationCount = 4;

		// 请求计数. 当一个请求发生时, 此计数+1; 当请求得到响应后, 此计数-1
		// 当请求计数为 0 时, UI 才会更新和响应输入
		int _requestCount = 0;

		// 装甲套装表. 每套装甲都是一个 List<ArmorPartData> [存档数据]
		List<List<ArmorPartData>> _data;

		// 当前装备的套装索引 [存档数据]
		int _currentSuitIndex;

		// 装甲部件表, 每个对象与对应的 ArmorPartData 具有相同的索引
		List<ArmorPartObject> _armorObjects = new List<ArmorPartObject>(20); // Multi not init this

		// 装甲背包
		SlotList _slotList;

		// 骨骼节点表, 由 ArmorPartObject 维护
		BoneNode[][] _boneNodes;

		// Decoration 装甲总数, 由 ArmorPartObject 维护
		int _decorationCount = 0;


//		PlayerNetwork _net;


		// view --------

		// 骨骼组件, 由 ArmorPartObject 维护
		BoneCollector _boneCollector;

		// 模型上的组件, 由 ArmorPartObject 维护 (以保证模型复制后对装甲的引用仍然正确. 用于装甲界面)
		ArmorBones _modelCmpt;

		// 当前是否为第一人称模式
		bool _isFirstPersonMode = false;

		// view --------


		// 装甲部件添加或移除事件
		public event Action onAddOrRemove;


		void TriggerAddOrRemoveEvent()
		{
			if (onAddOrRemove != null) onAddOrRemove();
		}

		#endregion


		#region Multi Mode ------------------------------------------------------------

		Action<bool> _switchArmorSuitCallback;
		Action<bool> _equipArmorPartFromPackageCallback;
		Action<bool> _removeArmorPartCallback;
		Action<bool> _switchArmorPartMirrorCallback;


		public void C2S_SwitchArmorSuit(int newSuitIndex, Action<bool> callback)
		{
			//
			_requestCount++;
			_switchArmorSuitCallback = callback;

			// send request
			PlayerNetwork.RequestSwitchArmorSuit (newSuitIndex);
		}


		public void S2C_SwitchArmorSuit(int newSuitIndex, bool success)
		{
			if (success && newSuitIndex != _currentSuitIndex)
			{
				// 移除当前套装
				for (int i = _armorObjects.Count - 1; i >= 0; i--)
				{
					_armorObjects[i].RemoveArmorPart();
					_armorObjects.RemoveAt(i);
				}

				// 装备新套装
				_currentSuitIndex = newSuitIndex;
				var newSuit = _data[newSuitIndex];

				for (int i = 0; i < newSuit.Count; i++)
				{
					var item = ItemMgr.Instance.Get(newSuit[i].itemId);
					if (item != null)
					{
						_armorObjects.Add(new ArmorPartObject(this, newSuit[i], item));
					}
					else
					{
						newSuit.RemoveAt(i--);
					}
				}
			}

			//
			_requestCount--;
			if (_switchArmorSuitCallback != null)
			{
				_switchArmorSuitCallback(success);
			}
		}


		public void C2S_EquipArmorPartFromPackage(int itemID, int typeValue, int boneGroup, int boneIndex, Action<bool> callback)
		{
			//
			_requestCount++;
			_equipArmorPartFromPackageCallback = callback;

			// send request
			PlayerNetwork.RequestEquipArmorPart (itemID, typeValue, boneGroup, boneIndex);
		}


		public void S2C_EquipArmorPartFromPackage(int itemID, int typeValue, int boneGroup, int boneIndex, bool success)
		{
			if (success)
			{
				ItemObject item = ItemMgr.Instance.Get(itemID);
				ArmorType type = (ArmorType)typeValue;

				ArmorPartObject oldArmor = null;
				if (type == ArmorType.Decoration) oldArmor = _boneNodes[boneGroup][boneIndex].decoration;
				else oldArmor = _boneNodes[boneGroup][boneIndex].normal;

				// 回收旧部件

				if (oldArmor != null)
				{
					oldArmor.RemoveArmorPart();
					int index = _armorObjects.FindIndex(armor => armor == oldArmor);
					if (index >= 0)
					{
						_armorObjects.RemoveAt(index);
						_data[_currentSuitIndex].RemoveAt(index);
					}
				}

				// 装备新部件

				ArmorPartData partData = new ArmorPartData(item.instanceId, type);
				partData.boneGroup = boneGroup;
				partData.boneIndex = boneIndex;

				_data[_currentSuitIndex].Add(partData);
				_armorObjects.Add(new ArmorPartObject(this, partData, item));
			}

			//
			_requestCount--;
			if (_equipArmorPartFromPackageCallback != null)
			{
				_equipArmorPartFromPackageCallback(success);
			}
		}


		public void C2S_RemoveArmorPart(int boneGroup, int boneIndex, bool isDecoration, Action<bool> callback)
		{
			//
			_requestCount++;
			_removeArmorPartCallback = callback;

			// send request
			PlayerNetwork.RequestRemoveArmorPart (boneGroup, boneIndex, isDecoration);
		}


		public void S2C_RemoveArmorPart(int boneGroup, int boneIndex, bool isDecoration, bool success)
		{
			if (success)
			{
				var part = GetArmorPartObject(boneGroup, boneIndex, isDecoration);

				part.RemoveArmorPart();
				int index = _armorObjects.FindIndex(armor => armor == part);
				if (index >= 0)
				{
					_armorObjects.RemoveAt(index);
					_data[_currentSuitIndex].RemoveAt(index);
				}
			}

			//
			_requestCount--;
			if (_removeArmorPartCallback != null)
			{
				_removeArmorPartCallback(success);
			}
		}


		public void C2S_SwitchArmorPartMirror(int boneGroup, int boneIndex, bool isDecoration, Action<bool> callback)
		{
			//
			_requestCount++;
			_switchArmorPartMirrorCallback = callback;

			// send request
			PlayerNetwork.RequestSwitchArmorPartMirror (boneGroup, boneIndex, isDecoration);
		}


		public void S2C_SwitchArmorPartMirror(int boneGroup, int boneIndex, bool isDecoration, bool success)
		{
			if (success)
			{
				SwitchArmorPartMirror(boneGroup, boneIndex, isDecoration);
            }

			//
			_requestCount--;
			if (_switchArmorPartMirrorCallback != null)
			{
				_switchArmorPartMirrorCallback(success);
			}
		}


		public void C2S_SyncArmorPartPosition(int boneGroup, int boneIndex, bool isDecoration, Vector3 position)
		{
			// send request
			PlayerNetwork.SyncArmorPartPos (boneGroup, boneIndex, isDecoration, position);
		}


		public void S2C_SyncArmorPartPosition(int boneGroup, int boneIndex, bool isDecoration, Vector3 position)
		{
			SetArmorPartPosition(boneGroup, boneIndex, isDecoration, position);
        }


		public void C2S_SyncArmorPartRotation(int boneGroup, int boneIndex, bool isDecoration, Quaternion rotation)
		{
			// send request
			PlayerNetwork.SyncArmorPartRot (boneGroup, boneIndex, isDecoration, rotation);
		}


		public void S2C_SyncArmorPartRotation(int boneGroup, int boneIndex, bool isDecoration, Quaternion rotation)
		{
			SetArmorPartRotation(boneGroup, boneIndex, isDecoration, rotation);
		}


		public void C2S_SyncArmorPartScale(int boneGroup, int boneIndex, bool isDecoration, Vector3 scale)
		{
			// send request
			PlayerNetwork.SyncArmorPartScale (boneGroup, boneIndex, isDecoration, scale);
		}


		public void S2C_SyncArmorPartScale(int boneGroup, int boneIndex, bool isDecoration, Vector3 scale)
		{
			SetArmorPartScale(boneGroup, boneIndex, isDecoration, scale);
		}

		#endregion --------------------------------------------------------------------


		public int currentSuitIndex
		{
			get { return _currentSuitIndex; }
		}


		public bool hasModelLayer
		{
			get { return _boneCollector; }
		}


		public bool hasRequest
		{
			get { return _requestCount > 0; }
		}


		// 保存数据
		public override void Serialize(BinaryWriter w)
		{
			// 版本号
			w.Write((byte)1);

			// 当前装备
			w.Write((byte)_currentSuitIndex);

			// 套装总数
			int suitCount = _data.Count;
			w.Write((byte)suitCount);

			for (int i = 0; i < suitCount; i++)
			{
				// 部件总数
				int partCount = _data[i].Count;
				w.Write((byte)partCount);

				for (int j = 0; j < partCount; j++)
				{
					_data[i][j].Serialize(w);
				}
			}
		}


		// 读取数据
		public override void Deserialize(BinaryReader r)
		{
			// 版本号
			r.ReadByte();

			// 当前装备
			_currentSuitIndex = r.ReadByte();

			// 套装总数
			int suitCount = r.ReadByte();
			_data = new List<List<ArmorPartData>>(suitCount < 8 ? 8 : suitCount);

			for (int i = 0; i < suitCount; i++)
			{
				// 部件总数
				int partCount = r.ReadByte();
				_data.Add(new List<ArmorPartData>(20));

				for (int j = 0; j < partCount; j++)
				{
					_data[i].Add(ArmorPartData.Deserialize(r));
				}
			}
		}


		// 遍历骨骼
		// boneGroup, boneIndex, hasNormal, hasDecoration
		public void ForEachBone(Action<int, int, bool, bool> action)
		{
			for (int i = 0; i < _boneNodes.Length; i++)
			{
				var group = _boneNodes[i];
				for (int j = 0; j < group.Length; j++)
				{
					action(i, j, group[j].normal != null, group[j].decoration != null);
				}
			}
		}


        public void RemoveBufferWhenBroken(ItemObject item)
		{
			var armor = _armorObjects.Find(a => a.item == item);
			if (armor != null && armor.isBroken)
			{
				armor.RemoveDefence();
			}
		}


		// 初始化
		public override void Start()
		{
			base.Start();

			if(PeGameMgr.IsMulti) return;

			Init();
		}


		public void Init(PlayerNetwork net = null)
		{
//			_net = net;
			_slotList = GetComponent<PlayerPackageCmpt>().package.GetSlotList(ItemPackage.ESlotType.Armor);
			_boneNodes = new BoneNode[4][] { new BoneNode[1], new BoneNode[3], new BoneNode[8], new BoneNode[4] };
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < _boneNodes[i].Length; j++)
				{
					_boneNodes[i][j] = new BoneNode();
				}
			}

			// 如果没有存档, 在这里初始化存档
			if (_data == null)
			{
				_data = new List<List<ArmorPartData>>(5);
				for (int i = 0; i < 5; i++)
				{
					_data.Add(new List<ArmorPartData>(20));
				}
				_currentSuitIndex = 0;
			}

			// 装备存档中的装备
			if (_currentSuitIndex < _data.Count) {
				var suit = _data [_currentSuitIndex];
				for (int i = 0; i < suit.Count; i++) {
					var item = ItemMgr.Instance.Get (suit [i].itemId);
					if (item != null) {
						_armorObjects.Add (new ArmorPartObject (this, suit [i], item));
					} else {
						suit.RemoveAt (i--);
					}
				}
			}
		}


		public int SelectArmorPartToAttack()
		{
			int count = _armorObjects.Count;
			int selected = UnityEngine.Random.Range(0, count);

			for (int i = 0; i < count; i++)
			{
				if (_armorObjects[selected].isBroken)
				{
					selected = (selected + 1) % count;
				}
				else return selected;
			}

			return -1;
		}


		void IPeMsg.OnMsg(EMsg msg, params object[] args)
		{
			switch (msg)
			{
				case EMsg.View_Prefab_Build:
					{
						BiologyViewRoot viewRoot = (BiologyViewRoot)args[1];
						_boneCollector = (args[0] as BiologyViewCmpt).monoBoneCollector;
						_modelCmpt = viewRoot.armorBones;
                        if (_armorObjects != null)
                        {
                            for (int i = 0; i < _armorObjects.Count; i++)
                            {
                                _armorObjects[i].CreateModel();
                            }
                        }
						break;
					}

				case EMsg.Battle_BeAttacked:
					{
						int selected = SelectArmorPartToAttack();
						if (selected >= 0)
						{
							_armorObjects[selected].OnBeAttacked((float)args[0] * PEVCConfig.instance.armorDamageRatio, (SkillSystem.SkEntity)args[1]);
						}
						break;
					}

				case EMsg.View_FirstPerson:
					{
						if ((bool)args[0] != _isFirstPersonMode)
						{
							_isFirstPersonMode = !_isFirstPersonMode;
							if (_armorObjects != null)
							{
								for (int i = 0; i < _armorObjects.Count; i++)
								{
									_armorObjects[i].SyncModelFirstPersonMode();
								}
							}
						}
						break;
					}
			}
		}


		// 从背包和当前装备的套装中找出一个套装的所有部件物品 (非当前装备套装)
		// 找出的物品添加到 _armorItems 列表, 索引添加到 _armorIndexes
		// 如果一个物品存在于当前套装而不在背包, 索引添加为所在当前套装的 (索引+1) 的负值
		// 返回所有找到的物品数量.

		List<ItemObject> _armorItems = new List<ItemObject>(20);
		int[] _armorIndexes = new int[20];

		int FindArmorItemsInSlotListAndCurrentSuit(int suitIndex)
		{
			var suit = _data[suitIndex];
			_armorItems.Clear();
			int index;
			int validCount = 0;

			for (int i = 0; i < suit.Count; i++)
			{
				index = _slotList.FindItemIndexById(suit[i].itemId);

				if (index >= 0)
				{
					_armorItems.Add(_slotList[index]);
					validCount++;
				}
				else
				{
					index = _armorObjects.FindIndex(part => part.item.instanceId == suit[i].itemId);
					if (index >= 0)
					{
						_armorItems.Add(_armorObjects[index].item);
						index = -(index + 1);
						validCount++;
					}
					else _armorItems.Add(null);
				}

				_armorIndexes[i] = index;
			}

			return validCount;
		}


		// 切换装甲套装
		// 返回值表示是否成功执行. 如果失败说明背包空间不足
		public bool SwitchArmorSuit(int newSuitIndex)
		{
			if (newSuitIndex < 0 || newSuitIndex >= _data.Count)
			{
				return false;
			}

			if (_currentSuitIndex != newSuitIndex)
			{
				// 判断背包是否有足够容量回收
				if (_slotList.vacancyCount
					+ FindArmorItemsInSlotListAndCurrentSuit(newSuitIndex)
					< _armorObjects.Count)
				{
					_armorItems.Clear();
					return false;
				}

				// 从背包或当前套装中移出将要装备的装甲部件
				int index;
				for (int i = 0; i < _armorItems.Count; i++)
				{
					if (_armorItems[i] != null)
					{
						index = _armorIndexes[i];

						if (index >= 0)
						{
							_slotList[index] = null;
						}
						else
						{
							index = -index - 1;
							_armorObjects[index].RemoveArmorPart();
							_armorObjects[index] = null;
						}
					}
				}

				// 回收当前剩余的所有装备的装甲
				for (int i = _armorObjects.Count - 1; i >= 0; i--)
				{
					if (_armorObjects[i] != null)
					{
						_slotList.Add(_armorObjects[i].item);
						_armorObjects[i].RemoveArmorPart();
					}
					_armorObjects.RemoveAt(i);
				}

				// 装备新套装
				_currentSuitIndex = newSuitIndex;
				var newSuit = _data[newSuitIndex];

				for (int i = 0; i < _armorItems.Count; i++)
				{
					if (_armorItems[i] != null)
					{
						_armorObjects.Add(new ArmorPartObject(this, newSuit[i], _armorItems[i]));
					}
					else
					{
						newSuit.RemoveAt(i);
						_armorItems.RemoveAt(i);
						i--;
					}
				}
			}

			return true;
		}


		// 快速装备. 查找一个骨骼用以装备装甲
		// 优先选择无装备骨骼, 否则替换已装备骨骼
		public void QuickEquipArmorPartFromPackage(ItemObject item)
		{
			ArmorType type = CreationHelper.GetArmorType(item.instanceId);
			if (type != ArmorType.None)
			{
				int boneGroup = 0;
				int boneIndex = 0;

				if (type != ArmorType.Decoration)
				{
					boneGroup = (int)type;

                    // 优先选择设计的骨骼节点
                    boneIndex = CreationMgr.GetCreation(item.instanceId).creationController.armorBoneIndex;
                    if (_boneNodes[boneGroup][boneIndex].normal != null)
                    {
                        // 其次找可用的空节点
                        boneIndex = Array.FindIndex(_boneNodes[boneGroup], node => node.normal == null);

                        // 最后选择设计的节点
                        if (boneIndex < 0) boneIndex = CreationMgr.GetCreation(item.instanceId).creationController.armorBoneIndex;
                    }
				}
				else
				{
					if (_decorationCount == _maxDecorationCount)
					{
						// 如果装饰装甲已经有 4 个, 则随机替换一个旧装饰装甲
						int selected = UnityEngine.Random.Range(0, _maxDecorationCount);

						for (int i = 0; i < _armorObjects.Count; i++)
						{
							if (_armorObjects[i].data.type == ArmorType.Decoration)
							{
								if (selected == 0)
								{
									boneIndex = _armorObjects[i].data.boneIndex;
									boneGroup = _armorObjects[i].data.boneGroup;
									break;
								}
								else selected--;
							}
						}
					}
					else
					{
						for (boneGroup = 0; boneGroup < 4; boneGroup++)
						{
							boneIndex = Array.FindIndex(_boneNodes[boneGroup], node => node.decoration == null);
							if (boneIndex >= 0) break;
						}
					}
				}

				if (PeGameMgr.IsMulti)
				{
					if (hasRequest) return;

					C2S_EquipArmorPartFromPackage(
						item.instanceId,
						(int)type,
						boneGroup,
						boneIndex,
						null);
				}
				else
				{
					EquipArmorPartFromPackage(
						item,
						type,
						boneGroup,
						boneIndex);
				}
			}
		}


		// 装备
		// 如果该骨骼已经有同类型装备, 该装备将被回收
		// 装备失败的原因: 超过了 4 个 Decoration; 发生了其他的一些奇怪的事情...
		public bool EquipArmorPartFromPackage(ItemObject item, ArmorType type, int boneGroup, int boneIndex)
		{
			if (item == null || type == ArmorType.None) return false;

			if (type == ArmorType.Decoration)
			{
				if (_decorationCount == 4 && _boneNodes[boneGroup][boneIndex].decoration == null)
				{
					return false;
				}
			}
			else
			{
				if ((int)type != boneGroup) return false;
			}

			// 从背包移出部件
			int index = _slotList.FindItemIndexById(item.instanceId);
			if (index < 0) return false;
			_slotList[index] = null;

			ArmorPartObject oldArmor = null;
			if (type == ArmorType.Decoration) oldArmor = _boneNodes[boneGroup][boneIndex].decoration;
			else oldArmor = _boneNodes[boneGroup][boneIndex].normal;

			// 回收旧部件
			if (oldArmor != null)
			{
				_slotList.Add(oldArmor.item);
				oldArmor.RemoveArmorPart();
				index = _armorObjects.FindIndex(armor => armor == oldArmor);
				if (index >= 0)
				{
					_armorObjects.RemoveAt(index);
					_data[_currentSuitIndex].RemoveAt(index);
				}
			}

			// 装备新部件
			ArmorPartData partData = new ArmorPartData(item.instanceId, type);
			partData.boneGroup = boneGroup;
			partData.boneIndex = boneIndex;

			_data[_currentSuitIndex].Add(partData);
			_armorObjects.Add(new ArmorPartObject(this, partData, item));

			return true;
		}


		// 获取装甲对象
		ArmorPartObject GetArmorPartObject(int boneGroup, int boneIndex, bool isDecoration)
		{
			var node = _boneNodes[boneGroup][boneIndex];
			return isDecoration ? node.decoration : node.normal;
		}


		/// <summary>
		/// 移出装甲部件
		/// </summary>
		/// 如果背包已满, 或该部位没有装甲, 返回 false
		public bool RemoveArmorPart(int boneGroup, int boneIndex, bool isDecoration)
		{
			if (_slotList.vacancyCount <= 0) return false;

			var part = GetArmorPartObject(boneGroup, boneIndex, isDecoration);
			if (part == null) return false;

			_slotList.Add(part.item);
			part.RemoveArmorPart();
			int index = _armorObjects.FindIndex(armor => armor == part);
			if (index >= 0)
			{
				_armorObjects.RemoveAt(index);
				_data[_currentSuitIndex].RemoveAt(index);
			}

			return true;
		}


		/// <summary>
		/// 切换装甲部件的镜像属性
		/// </summary>
		public bool SwitchArmorPartMirror(int boneGroup, int boneIndex, bool isDecoration)
		{
			var part = GetArmorPartObject(boneGroup, boneIndex, isDecoration);
			if (part == null) return false;

			part.data.mirrored = !part.data.mirrored;
			float absScale = Mathf.Abs(part.data.localScale.z);
			part.data.localScale.z = part.data.mirrored ? -absScale : absScale;
			part.SyncModelScale();

			return true;
		}


		/// <summary>
		/// 移动装甲部件
		/// </summary>
		public bool SetArmorPartPosition(int boneGroup, int boneIndex, bool isDecoration, Vector3 value)
		{
			var part = GetArmorPartObject(boneGroup, boneIndex, isDecoration);
			if (part == null) return false;

			part.data.localPosition = value;
			part.SyncModelPosition();

			return true;
		}


		/// <summary>
		/// 旋转装甲部件
		/// </summary>
		public bool SetArmorPartRotation(int boneGroup, int boneIndex, bool isDecoration, Quaternion value)
		{
			var part = GetArmorPartObject(boneGroup, boneIndex, isDecoration);
			if (part == null) return false;

			part.data.localEulerAngles = value.eulerAngles;
			part.SyncModelEulerAngles();

			return true;
		}


		/// <summary>
		/// 缩放装甲部件
		/// </summary>
		public bool SetArmorPartScale(int boneGroup, int boneIndex, bool isDecoration, Vector3 value)
		{
			var part = GetArmorPartObject(boneGroup, boneIndex, isDecoration);
			if (part == null) return false;

			part.data.localScale = value;
			part.SyncModelScale();

			return true;
		}
	}

}