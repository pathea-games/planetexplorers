
// VCCase
// Voxel Creation Enums

// Main category of creations
public enum EVCCategory : int
{
	cgAbstract = 0x0,
	cgSword = 0x1,
	cgShield = 0x2,
	cgGun = 0x3,
	cgVehicle = 0x4,
	cgAircraft = 0x5,
	cgBoat = 0x6,

	cgBow = 0x7,
	cgAxe = 0x8,

	cgRobot = 0x9,
	cgAITurret = 0xA,
    
    cgLgSword = 0x10,
    cgDbSword = 0x11,

	// Armor
	cgHeadArmor = 0xE0,
	cgBodyArmor = 0xE1,
	cgArmAndLegArmor = 0xE2,
	cgHandAndFootArmor = 0xE3,
	cgDecorationArmor = 0xE4,

	cgObject = 0xFE,
}

// Component types
public enum EVCComponent : int
{
	cpAbstract = 0x0,
	cpSwordHilt = 0x1,
	cpShieldHandle = 0x2,
	cpGunHandle = 0x3,
	cpGunMuzzle = 0x4,
	cpVehicleCockpit = 0x5,
	cpVehicleWheel = 0x6,
	cpVehicleFuelCell = 0x7,
	cpVehicleEngine = 0x8,
	cpVtolCockpit = 0x9,
	cpVtolRotor = 0xA,
	cpVtolFuelCell = 0xB,
	cpLandingGear = 0xC,
	cpSideSeat = 0xD,
	cpHeadLight = 0xE,
	cpCtrlTurret = 0xF,
	cpFrontCannon = 0x10,
	cpMissileLauncher = 0x11,
	cpAITurretWeapon = 0x12,
	cpJetExhaust = 0x13,
	cpShipCockpit = 0x14,
	cpShipPropeller = 0x15,
	cpShipRudder = 0x16,
	cpSubmarineBallastTank = 0x17,

	cpBowGrip = 0x18,
	cpAxeHilt = 0x19,

	cpRobotController = 0x1A,
	cpRobotBattery = 0x1B,
	cpRobotWeapon = 0x1C,

    //swordHilt
    cpLgSwordHilt = 0x66,//102
    cpDbSwordHilt = 0x67,//103

	// Other
	cpAirshipThruster = 0x80,

	//object
	cpBed = 0xC0,
	cpLight = 0xC1,
	cpPivot = 0xC2,

	// Armor
	cpHeadPivot = 0xE0,
	cpBodyPivot = 0xE1,
	cpArmAndLegPivot = 0xE2,
	cpHandAndFootPivot = 0xE3,
	cpDecorationPivot = 0xE4,

	cpEffect = 0xF0,
	cpDecal = 0xF1,
}

// Creation types
public enum ECreation : int
{
	Null = 0x00,
	Sword = 0x10,
	Shield = 0x11,
	Bow = 0x12,
	Axe = 0x13,
	HandGun = 0x20,
	Rifle = 0x21,
	Vehicle = 0x90,
	Aircraft = 0xA0,
	Boat = 0xB0,

	Robot = 0x30,
	AITurret = 0x40,

    SwordLarge = 0x50,
    SwordDouble = 0x51,

	ArmorHead = 0xE0,
	ArmorBody = 0xE1,
	ArmorArmAndLeg = 0xE2,
	ArmorHandAndFoot = 0xE3,
	ArmorDecoration = 0xE4,

	SimpleObject = 0xFFFE,
}