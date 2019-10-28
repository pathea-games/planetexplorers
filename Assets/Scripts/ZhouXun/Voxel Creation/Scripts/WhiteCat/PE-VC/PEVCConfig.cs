using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WhiteCat
{
    //lz-2016.06.21 用于本地化字段WorkShop中分类级别，暂时没有父子级关系需求，所以没加父子级关系处理
    public class WorkShopFilter
    {
        private int m_ID;
        private int m_Level; //1 , 2 , 3
        private string m_Tag;
        private readonly string m_LevelSpace="    ";


        public int ID { get { return m_ID; } }
        public int Level { get { return m_Level; } }
        public string Tag { get { return m_Tag; } }

        public WorkShopFilter(int id,int lv,string tag)
        {
            this.m_ID = id;
            this.m_Level = lv;
            this.m_Tag = tag;
        }

        public string GetNameByID()
        {
            string name = PELocalization.GetString(this.m_ID);
            if (!string.IsNullOrEmpty(name))
            {
                if (this.m_Level > 1)
                {
                    for (int i = 0; i < this.m_Level-1; i++)
                    {
                        name = this.m_LevelSpace + name;
                    }
                }
                return name;
            }
            return "";
        }
    }

	public class PEVCConfig : ScriptableObject
	{
		static PEVCConfig _instance;
		public static PEVCConfig instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = Resources.Load<PEVCConfig>("PEVCConfig");
				}
				return _instance;
			}
		}

		// VCCase
        #region Workshop Filter
        public static readonly List<WorkShopFilter> isoNames =  new List<WorkShopFilter>()
		{
            new WorkShopFilter(8000295,1,IsoTags.Creation),
            new WorkShopFilter(8000296,2,IsoTags.Equipment),
			new WorkShopFilter(8000297,3,IsoTags.Sword),
            new WorkShopFilter(8000298,3,IsoTags.Axe),
			new WorkShopFilter(8000299,3,IsoTags.Bow),
            new WorkShopFilter(8000300,3,IsoTags.Shield),
			new WorkShopFilter(8000301,3,IsoTags.Gun),
            new WorkShopFilter(8000302,2,IsoTags.Carrier),
			new WorkShopFilter(8000303,3,IsoTags.Vehicle),
            new WorkShopFilter(8000304,3,IsoTags.Ship),
			new WorkShopFilter(8000305,3,IsoTags.Aircraft),
            new WorkShopFilter(8000306,2,IsoTags.Armor),
			new WorkShopFilter(8000307,3,IsoTags.Head),
            new WorkShopFilter(8000308,3,IsoTags.Body),
			new WorkShopFilter(8000309,3,IsoTags.ArmAndLeg),
            new WorkShopFilter(8000310,3,IsoTags.HeadAndFoot),
			new WorkShopFilter(8000311,3,IsoTags.Decoration),
            new WorkShopFilter(8000312,2,IsoTags.Robot),
			new WorkShopFilter(8000313,2,IsoTags.AITurret),
            new WorkShopFilter(8000314,2,IsoTags.ObjectItem),
		};
		#endregion

		[Header("Common")] ////////////////////////////////////////////////////////////////

		public GameObject canvasObject;
		public PhysicMaterial physicMaterial;
		public Material lineMaterial;
		public Material handleMaterial;
		public LayerMask creationDraggingLayerMask;
		public float creationColliderScale = 0.85f;

		public Color dragValidLineColor = Color.green;
		public Color dragInvalidLineColor = Color.red;
		public Color dragValidPlaneColor = new Color(0f, 1f, 0f, 0.4f);
		public Color dragInvalidPlaneColor = new Color(1f, 0f, 0f, 0.4f);

		[Range(20, 100)] public float maxRigidbodySpeed = 40;
		[Range(0, 10f)] public float maxRigidbodyAngularSpeed = Mathf.PI;
        public AnimationCurve speedScaleCurve;
		public float minSyncSqrDistance = 0.0001f;
		public float minSyncAngle = 0.1f;
		public float minSyncSqrSpeed = 0.0001f;
		public float minSyncSqrAngularSpeed = 0.00005f;
		public float minSyncSqrAimPoint = 0.001f;
        public float netDataApplyDamping = 0.2f;

		public float maxSqrRigidbodySpeed { get { return maxRigidbodySpeed * maxRigidbodySpeed; } }

		[Header("Sword & Bow & Axe & armor & Object")] ////////////////////////////////////

		public float swordStandardWeight = 100;
		public float minStaminaCostRatioOfWeight = 0.5f;
		public float maxStaminaCostRatioOfWeight = 2f;

		public float bowDurabilityScale = 1.0f;
		public float bowDurabilityBase = 200f;

		public float axeDurabilityScale = 2.0f;
		public float axeAttackScale = 1f;
		public float axeCutDamageScale = 2f;

		public float armorDurabilityScale = 1.0f;
		public float maxArmorDurability = 100f;
		public float armorDamageRatio = 0.1f;
		public float maxArmorDefence = 10f;

		public float pivotRotateSpeed = 60f;
		public float bedLimitAngle = 30f;
		public float sleepTimeScale = 120;

		public AnimationCurve swordAnimSpeedToASPD;
		public AnimationCurve axeAnimSpeedToASPD;

		public static float equipDurabilityShowScale = 0.05f;

		[Header("AI Turret")] //////////////////////////////////////////////////////////////

		public float aiTurretDurabilityScale = 50f;
		public float aiTurretMassScale = 0.1f;
		public float aiTurretMinMass = 10;
		public float aiTurretMaxMass = 10000;

		[Header("Robot")] //////////////////////////////////////////////////////////////////

		public float robotDurabilityScale = 100f;
		public float robotAttackRange = 50f;
		public GameObject robotTrail;
		public float robotCureExpendEnergyPerSecond = 200f;

		public float robotMassScale = 0.1f;
		public float robotMinMass = 10;
		public float robotMaxMass = 50;

		public float robotStandardDrag = 0.1f;
		public float robotUnderwaterDrag = 1f;
		public float robotStandardAngularDrag = 0.1f;
		public float robotUnderwaterAngularDrag = 1f;

		public float robotMinHeight = 3f;
		public float robotMaxHeight = 5f;
		public float robotMinDistance = 2f;
		public float robotMaxDistance = 4f;

		public float robotSpeedScale = 1f;
		public float robotSwingRange = 0.1f;
		public float robotSwingPeriod = 1f;

		public float robotVelocityChangeSpeed = 5f;
		public float robotVelocityRotateSpeed = 3.14f;
		public float robotRotateSpeed = 90f;

		public float randomRobotDistance { get { return Random.Range(robotMinDistance, robotMaxDistance); } }
		public float randomRobotHeight { get { return Random.Range(robotMinHeight, robotMaxHeight); } }
		public float sqrRobotAttackRange { get { return robotAttackRange * robotAttackRange; } }

		[Header("Carrier")] ////////////////////////////////////////////////////////////////

		public LayerMask attackRayLayerMask = -1;
		public LayerMask getOffLayerMask = -1;
		public float treeHardness = 10000f;

		public float lockTargetDuration = 2f;

		public float maxJetAccelerate = 10;
		[Range(0, 1)] public float jetIncreaseSpeed = 0.2f;
		[Range(0, 1)] public float jetDecreaseSpeed = 0.2f;
		[Range(0, 5)] public float jetDecToIncInterval = 1f;

		public float minPassengerDamage = 0.01f;
		public float maxPassengerDamage = 0.1f;
		public float randomPassengerDamage { get { return Random.Range(minPassengerDamage, maxPassengerDamage); } }

		[Header("Vehicle")] /////////////////////////////////////////////////////////////////

		public float vehicleDurabilityScale = 1f;

		public float vehicleMassScale = 0.1f;
		public float vehicleMinMass = 1000;
		public float vehicleMaxMass = 10000;
		public Vector3 vehicleInertiaTensorScale;

		public float vehicleStandardDrag = 0.1f;
		public float vehicleUnderwaterDrag = 1f;
		public float vehicleStandardAngularDrag = 0.1f;
		public float vehicleUnderwaterAngularDrag = 1f;

		[Range(0,20f)] public float naturalFrequency = 10;
		[Range(0,3f)] public float dampingRatio = 0.8f;
		[Range(-0.1f, 0.1f)] public float wheelForceAppPointOffset = -0.03f;

		public float maxWheelSteerAngle = 20f;
		public float vehicleSteerRadiusBase = 4f;
		public float vehicleSteerRadiusExtend = 4f;
		public float motorcycleBiasAngle = 25f;
		public float motorcycleBalanceHelp = 0.1f;

		public float sideStiffnessBase = 0.5f;
		public float sideStiffnessFactor = 0.5f;
		public float fwdStiffnessBase = 0.5f;
		public float fwdStiffnessFactor = 0.5f;
		public AnimationCurve motorForce;
        public AnimationCurve speedToRotateFactor;

		[Header("Helicopter")] //////////////////////////////////////////////////////////////

		public float helicopterDurabilityScale = 1.0f;

		public float helicopterMassScale = 0.1f;
		public float helicopterMinMass = 1000;
		public float helicopterMaxMass = 200000;
		public Vector3 helicopterInertiaTensorScale;

		public float helicopterStandardDrag = 0.1f;
		public float helicopterUnderwaterDrag = 1f;
		public float helicopterStandardAngularDrag = 0.1f;
		public float helicopterUnderwaterAngularDrag = 1f;

		public float rotorSteerHelp = 0.08f;
		public float thrusterSteerHelp = 0.03f;
        public float rotorAccelerateFactor = 1200f;
		public float rotorDecelerateFactor = 960f;
		public float rotorMaxRotateSpeed = 2901f;
		public float rotorDeflectSpeed = 20f;
		[Range(0f, 1f)]public float rotorBalanceAdjust = 0.75f;
		public AnimationCurve rotorBalaceScale;
		public float helicopterMaxUpSpeed = 10f;
		public float helicopterMaxDownSpeed = 5f;
		public float helicopterBalanceHelp = 10f;
		public float helicopterMaxHeight = 150f;
		public AnimationCurve liftForceFactor;
		public float maxLiftAccelerate = 10f;
		public float rotorEnergySpeed = 0.001f;
		public float thrusterEnergySpeed = 0.001f;

		[Header("Boat")] ////////////////////////////////////////////////////////////////////

		public float boatDurabilityScale = 1.0f;

		public float boatMassScale = 0.1f;
		public float boatMinMass = 1000;
		public float boatMaxMass = 10000;
		public float boatPropellerEnergySpeed = 0.0005f;
		public Vector3 boatInertiaTensorScale;

		public float boatStandardDrag = 0.1f;
		public float boatUnderwaterDrag = 1f;
		public float boatStandardAngularDrag = 0.1f;
		public float boatUnderwaterAngularDrag = 1f;

		public float boatBalanceHelp = 20f;
        public float buoyancyFactor = 10000f;
		public float submarineMaxUpSpeed = 8f;
		public float submarineMaxDownSpeed = 6f;

		[Header("Sound")] ///////////////////////////////////////////////////////////////////

		public AudioClip crashSound;
		public AudioClip explotionSound;
		public float minWeaponSoundInterval = 0.05f;
		[Range(0, 1)] public float explotionVolume = 1f;
		[Range(0, 1)] public float crashVolume = 1f;

#if UNITY_EDITOR

		[MenuItem("Assets/Create/PEVC Config")]
		static void CreateConfig()
		{
			AssetDatabase.CreateAsset(CreateInstance<PEVCConfig>(), Internal.RootLocator.directory + "/PE-VC/Resources/PEVCConfig.asset");
		}
#endif
	}
}
