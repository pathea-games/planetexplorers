using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Pathea;
using WhiteCat;

// Manage all creations in a client or server gamerecord
public static class CreationMgr
{
    public const int VERSION = 0x2001;
    private static Dictionary<int, CreationData> m_Creations;
    //static System.Timers.Timer m_Check;
    // Init the CreationMgr
    public static void Init()
    {
        Clear();
        m_Creations = new Dictionary<int, CreationData>();
        //		m_Check =new System.Timers.Timer(2000);
        //		m_Check.Elapsed += CreationMgr.CheckForDel;
        //		m_Check.AutoReset = true;
        //		m_Check.Enabled = true;
    }

    // Clear creations
    public static void Clear()
    {
        if (m_Creations != null)
        {
            foreach (KeyValuePair<int, CreationData> kvp in m_Creations)
            {
                if (kvp.Value != null)
                    kvp.Value.Destroy();
            }
            m_Creations.Clear();
        }
    }

    public static int QueryNewId()
    {
        int max = CreationData.ObjectStartID;
        if (m_Creations.Count == 0)
        {
            return CreationData.ObjectStartID + 1;
        }
        foreach (KeyValuePair<int, CreationData> kvp in m_Creations)
        {
            if (kvp.Value.m_ObjectID > max)
                max = kvp.Value.m_ObjectID;
        }
        return max + 1;
    }

    // Add a creation to CreationMgr
    public static void AddCreation(CreationData creation_data)
    {
        if (m_Creations == null)
        {
            Debug.LogError("CreationMgr haven't initialized!");
            return;
        }

        if (m_Creations.ContainsKey(creation_data.m_ObjectID))
        {
            Debug.LogWarning("You want to add a creation instance, but it already exist, the old creation_data has been replaced!");
            m_Creations[creation_data.m_ObjectID] = creation_data;
        }
        else
        {
            m_Creations.Add(creation_data.m_ObjectID, creation_data);
        }
    }

    // Add a creation to CreationMgr (multi-player)
    public static CreationData NewCreation(int object_id, ulong res_hash, float rand_seed)
    {
        CreationData creation_data = new CreationData();
        creation_data.m_ObjectID = object_id;
        creation_data.m_HashCode = res_hash;
        creation_data.m_RandomSeed = rand_seed;
        if (creation_data.LoadRes())
        {
            creation_data.GenCreationAttr();
            creation_data.BuildPrefab();
            creation_data.Register();
            CreationMgr.AddCreation(creation_data);
            //creation_data.m_Hp = creation_data.m_Attribute.m_Durability;
            //creation_data.m_Fuel = creation_data.m_Attribute.m_MaxFuel;
            Debug.Log("new creation succeed !");
            creation_data.UpdateUseTime();
            return creation_data;
        }
        else
            Debug.LogError("creation generate failed.");
        return null;
    }

    // Remove a creation from CreationMgr
    public static void RemoveCreation(int id)
    {
        if (m_Creations == null)
        {
            Debug.LogError("CreationMgr haven't initialized!");
            return;
        }

        if (m_Creations.ContainsKey(id))
        {
            CreationData crd = m_Creations[id];
            if (crd != null)
                crd.Destroy();
            //			for(int i = 0; i < crd.m_SkillList.Count; i++)
            //			{
            //				SkillAsset.EffSkill.s_tblEffSkills.Remove(crd.m_SkillList[i]);
            //			}
            m_Creations.Remove(id);
        }
        else
        {
            Debug.LogError("You want to remove a creation instance, but it doesn't exist!");
        }
    }

    // Read a creation from CreationMgr
    public static CreationData GetCreation(int id)
    {
        if (m_Creations == null)
        {
            Debug.LogError("CreationMgr haven't initialized!");
            return null;
        }

        if (m_Creations.ContainsKey(id))
        {
            m_Creations[id].UpdateUseTime();
            return m_Creations[id];
        }
        else
        {
            if (GameConfig.IsMultiMode)
            {
                return SteamWorkShop.GetCreation(id);
            }
            return null;
        }
    }

    // Import content from a byte buffer
    public static void Import(byte[] buffer)
    {
        if (buffer == null)
            return;
        if (buffer.Length < 8)
            return;
        Init();
        MemoryStream ms = new MemoryStream(buffer);
        BinaryReader r = new BinaryReader(ms);
        int version = r.ReadInt32();
        if (VERSION != version)
        {
            Debug.LogWarning("The version of CreationMgr is newer than the record.");
        }
        switch (version)
        {
            case 0x2001:
                {
                    int count = r.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        CreationData creation_data = new CreationData();
                        creation_data.m_ObjectID = r.ReadInt32();
                        creation_data.m_HashCode = r.ReadUInt64();
                        creation_data.m_RandomSeed = r.ReadSingle();
                        //creation_data.m_Hp = r.ReadSingle();
                        //creation_data.m_Fuel = r.ReadSingle();
                        //				if ( creation_data.m_Hp < 0 )
                        //				{
                        //					Debug.LogWarning("Creation " + creation_data.m_ObjectID.ToString() +
                        //						             " had been crashed, will not load any more..");
                        //					continue;
                        //				}
                        if (creation_data.LoadRes())
                        {
                            creation_data.GenCreationAttr();
                            creation_data.BuildPrefab();
                            creation_data.Register();
                            m_Creations.Add(creation_data.m_ObjectID, creation_data);
                        }
                        else
                        {
                            creation_data.Destroy();
                            creation_data = null;
                        }
                    }
                    break;
                }
            default:
                break;
        }
        r.Close();
        ms.Close();
    }


    // Export content to a byte buffer
	public static void Export(BinaryWriter w)
    {
        if (m_Creations == null)
        {
            Debug.LogError("CreationMgr haven't initialized!");
            return;
        }

        w.Write(VERSION);	// 0x2001 now
        w.Write(m_Creations.Count);
        foreach (KeyValuePair<int, CreationData> kvp in m_Creations)
        {
            w.Write(kvp.Value.m_ObjectID);
            w.Write(kvp.Value.m_HashCode);
            w.Write(kvp.Value.m_RandomSeed);
            //w.Write(kvp.Value.m_Hp);
            //w.Write(kvp.Value.m_Fuel);
        }
    }


	static CreationData lastCreationData = null;
	static GameObject lastProduct = null;


	static GameObject InstantiateCreationModel(CreationData crd, Action<Transform> initTransform)
    {
        if (crd == null) return null;
        if (crd.m_Prefab == null) return null;

		GameObject product = null;

		if (lastProduct)
		{
			var drag = lastProduct.GetComponent<ItemDraggingBase>();
			if (drag)
			{
				if (lastCreationData == crd)
				{
					UnityEngine.Object.Destroy(drag);
					product = lastProduct;
				}
				else
				{
					UnityEngine.Object.Destroy(lastProduct);
				}
			}
			lastProduct = null;
			lastCreationData = null;
		}

		if (!product)
        {
			product = UnityEngine.Object.Instantiate(crd.m_Prefab) as GameObject;
			lastProduct = product;
			lastCreationData = crd;
		}

		product.transform.SetParent(null, false);
		product.SetActive(true);

		if (initTransform != null)
		{
			initTransform(product.transform);
		}
		else
		{
			product.transform.position = Vector3.zero;
			product.transform.localScale = Vector3.one;
			product.transform.rotation = Quaternion.identity;
		}

		// 新创建物体初始化
		if (lastProduct)
		{
			var creationController = product.GetComponent<CreationController>();
			creationController.enabled = true;

			

            if(crd.m_IsoData.m_HeadInfo.Category == EVCCategory.cgDbSword)
            {
                // Mesh manager
                VCMeshMgr[] meshmgrs = product.GetComponentsInChildren<VCMeshMgr>();
                CreationMeshLoader[] meshloader = new CreationMeshLoader[2];
                for (int i = 0; i < meshmgrs.Length; i++)
                {
                    meshmgrs[i].m_ColorMap = crd.m_IsoData.m_Colors;
                    meshmgrs[i].Init();

                    meshloader[i] = meshmgrs[i].gameObject.AddComponent<CreationMeshLoader>();
                    meshloader[i].m_Meshdagger = crd.m_IsoData.m_HeadInfo.Category == EVCCategory.cgDbSword;
                    meshloader[i].Init(creationController);

                }

                // Mesh loader
                //meshloader[0].Init(creationController);
                //meshloader[1].Init(creationController);
                //meshloader[1].InitClone(creationController, meshloader[0].m_Computer.m_DataSource, meshmgrs[1]);
            }
            else
            {
                // Mesh manager
                VCMeshMgr meshmgr = product.GetComponentInChildren<VCMeshMgr>();
                meshmgr.m_ColorMap = crd.m_IsoData.m_Colors;
                meshmgr.Init();

                // Mesh loader
                CreationMeshLoader meshloader = meshmgr.gameObject.AddComponent<CreationMeshLoader>();
                meshloader.Init(creationController);
            }
			
            //    meshmgr.m_ColorMap = crd.m_IsoData.m_Colors;
            //meshmgr.Init();

            //// Mesh loader
            //CreationMeshLoader meshloader = meshmgr.gameObject.AddComponent<CreationMeshLoader>();
            //meshloader.Init(creationController);

			// Decal data
			VCDecalHandler[] dclhs = product.GetComponentsInChildren<VCDecalHandler>(true);
			foreach (VCDecalHandler dclh in dclhs) dclh.m_Iso = crd.m_IsoData;
		}

		return product;
    }


	// [VCCase] - Create Creaton Game Object
	public static GameObject InstantiateCreation(int objectid, int itemInstanceId, bool init, Action<Transform> initTransform)
    {
        CreationData crd = GetCreation(objectid);
        GameObject product = null;

        if (crd == null)
        {
            Debug.LogError("Can not find CreationData");
            return null;
        }

        if (crd.m_Attribute.m_Type == ECreation.Vehicle)
        {
            product = InstantiateCarrier<VehicleController>(itemInstanceId, init, crd, initTransform);
        }
        else if (crd.m_Attribute.m_Type == ECreation.Aircraft)
        {
            product = InstantiateCarrier<HelicopterController>(itemInstanceId, init, crd, initTransform);
        }
        else if (crd.m_Attribute.m_Type == ECreation.Boat)
        {
            product = InstantiateCarrier<BoatController>(itemInstanceId, init, crd, initTransform);
        }
        else if (crd.m_Attribute.m_Type == ECreation.SimpleObject)
        {
			product = InstantiateSimpleObject(itemInstanceId, init, crd, initTransform);
        }
		else if (crd.m_Attribute.m_Type == ECreation.Robot)
		{
			product = InstantiateRobot(itemInstanceId, init, crd, initTransform);
		}
		else if (crd.m_Attribute.m_Type == ECreation.AITurret)
		{
			product = InstantiateAITurret(itemInstanceId, init, crd, initTransform);
		}
		else if (crd.m_Attribute.m_Type == ECreation.ArmorArmAndLeg
			|| crd.m_Attribute.m_Type == ECreation.ArmorBody
			|| crd.m_Attribute.m_Type == ECreation.ArmorDecoration
			|| crd.m_Attribute.m_Type == ECreation.ArmorHandAndFoot
			|| crd.m_Attribute.m_Type == ECreation.ArmorHead)
		{
			product = InstantiateArmor(itemInstanceId, init, crd, initTransform);
		}
		else if (crd.m_Attribute.m_Type == ECreation.HandGun
            || crd.m_Attribute.m_Type == ECreation.Rifle
            || crd.m_Attribute.m_Type == ECreation.Shield
            || crd.m_Attribute.m_Type == ECreation.Sword
            || crd.m_Attribute.m_Type == ECreation.SwordLarge
            || crd.m_Attribute.m_Type == ECreation.SwordDouble 
		    || crd.m_Attribute.m_Type == ECreation.Axe
		    || crd.m_Attribute.m_Type == ECreation.Bow)
        {
			if (init)
			{
				var source = crd.m_Prefab.GetComponent<PEGun>();
				if(source && source.m_AmmoType == AmmoType.Energy)
				{
					product = new GameObject("Shit");
					var logic = product.AddComponent<PEEnergyGunLogic>();
					logic.m_Magazine = new Magazine(source.m_Magazine);
					logic.m_RechargeDelay = source.m_RechargeDelay;
					logic.m_RechargeEnergySpeed = source.m_RechargeEnergySpeed;
				}
				else return null;
			}
			else
			{
				product = InstantiateCreationModel(crd, initTransform);
			}
		}

        return product;
    }


	static GameObject InstantiateCarrier<T>(int itemInstanceId, bool init, CreationData crd, Action<Transform> initTransform)
        where T : CarrierController
    {
		GameObject product = InstantiateCreationModel(crd, initTransform);
		if (product == null) return null;

		if (init)
		{
			// 载具控制器
			var controller = product.AddComponent<T>();
			controller.InitController(itemInstanceId);

			var creation = product.GetComponent<CreationController>();
			creation.collidable = creation.visible = false;
		}
		else
		{
			// 拖动
			product.AddComponent<ItemDraggingCreation>();
		}

		return product;
    }


	static GameObject InstantiateRobot(int itemInstanceId, bool init, CreationData crd, Action<Transform> initTransform)
	{
		GameObject product = InstantiateCreationModel(crd, initTransform);
		if (product == null) return null;

		if (init)
		{
			// 机器人控制器
			var controller = product.AddComponent<RobotController>();
			controller.InitController(itemInstanceId);

			var creation = product.GetComponent<CreationController>();
			creation.collidable = creation.visible = false;
		}
		else
		{
			// 拖动
			product.AddComponent<ItemDraggingCreation>();
		}

		return product;
	}


	static GameObject InstantiateAITurret(int itemInstanceId, bool init, CreationData crd, Action<Transform> initTransform)
	{
		GameObject product = InstantiateCreationModel(crd, initTransform);
		if (product == null) return null;

		if (init)
		{
			// AI控制器
			var controller = product.AddComponent<AITurretController>();
			controller.InitController(itemInstanceId);

			var creation = product.GetComponent<CreationController>();
			creation.collidable = creation.visible = false;
		}
		else
		{
			// 拖动
			product.AddComponent<ItemDraggingCreation>();
		}

		return product;
	}


	static GameObject InstantiateSimpleObject(int itemInstanceId, bool init, CreationData crd, Action<Transform> initTransform)
	{
		GameObject product = InstantiateCreationModel(crd, initTransform);
		if (product == null) return null;

		if(init)
		{
			product.AddComponent<DragItemLogicCreationSimpleObj>();

			// 添加刚体
			var rigidbody = product.AddComponent<Rigidbody>();
			rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			rigidbody.drag = 0;
			rigidbody.angularDrag = Kit.Million;
			rigidbody.mass = Kit.Million;
			rigidbody.centerOfMass = Vector3.zero;

			// 体素操作 (Simple Object 每个部分有不同的操作菜单)
			var creation = product.GetComponent<CreationController>();
			var commonPart = creation.meshRoot.gameObject;
			commonPart.AddComponent<VCPCommonPart>();
			commonPart.AddComponent<DragItemMousePickCreationSimpleObject>();
			commonPart.AddComponent<ItemScript>();

			var beds = product.GetComponentsInChildren<VCPBed>();
			var pivot = product.GetComponentInChildren<VCPPivot>();
			var lights = product.GetComponentsInChildren<VCPSimpleLight>();

			// 床垫操作
			if (beds != null && beds.Length > 0)
			{
				var sleeps = product.AddComponent<Pathea.Operate.PEBed>().sleeps = new Pathea.Operate.PESleep[beds.Length];

				for(int i=0; i<beds.Length; i++)
				{
					sleeps[i] = beds[i].sleepPivot;

					beds[i].gameObject.AddComponent<DragItemMousePickCreationSimpleObject>()
						.overridePriority = MousePicker.EPriority.Level3;
					beds[i].gameObject.AddComponent<ItemScript>();
				}
			}

			// 转轴操作
			if (pivot != null)
			{
                pivot.gameObject.AddComponent<DragItemMousePickCreationSimpleObject>()
						.overridePriority = MousePicker.EPriority.Level3;
				pivot.gameObject.AddComponent<ItemScript>();
				pivot.Init(creation.partRoot.parent);
			}

			// 灯光操作
			if (lights != null && lights.Length > 0)
			{
                for (int i = 0; i < lights.Length; i++)
                {
                    lights[i].gameObject.AddComponent<DragItemMousePickCreationSimpleObject>()
						.overridePriority = MousePicker.EPriority.Level3;
					lights[i].gameObject.AddComponent<ItemScript>();
                }
			}

			creation.collidable = creation.visible = false;
		}
		else
		{
			// 拖动
			product.AddComponent<ItemDraggingCreation>();
		}

		return product;
	}


	static GameObject InstantiateArmor(int itemInstanceId, bool init, CreationData crd, Action<Transform> initTransform)
	{
		GameObject product = InstantiateCreationModel(crd, initTransform);
		if (product == null) return null;

		if (init)
		{
		}

		return product;
    }


	public static void CheckForDel()
    {
        if (m_Creations != null)
        {
            foreach (var iter in m_Creations)
            {
                if (iter.Value.TooLongNoUse())
                {
                    //RemoveCreation(iter.Key);
                    break;
                }
            }
        }
    }

}