using UnityEngine;

namespace WhiteCat
{
	public struct VCUtility
	{
		public static float GetSwordAnimSpeed(float weight)
		{
			return Mathf.Clamp(-0.00004f * weight * weight + 1.5f, 0.5f, 1.5f);
        }


		public static float GetAxeAnimSpeed(float weight)
		{
			return Mathf.Clamp(-0.00004f * weight * weight + 1.5f, 0.5f, 1.5f);
		}


		public static int GetSwordAtkSpeedTextID(float animSpeed)
		{
			var aspd = PEVCConfig.instance.swordAnimSpeedToASPD.Evaluate(animSpeed);
			if (aspd <= 0.95f) return TextID.AtkSpeedFast;
			if (aspd >= 1.2f) return TextID.AtkSpeedSlow;
			return TextID.AtkSpeedNormal;
		}


		public static int GetAxeAtkSpeedTextID(float animSpeed)
		{
			var aspd = PEVCConfig.instance.axeAnimSpeedToASPD.Evaluate(animSpeed);
			if (aspd <= 1f) return TextID.AtkSpeedFast;
			if (aspd >= 1.3f) return TextID.AtkSpeedSlow;
			return TextID.AtkSpeedNormal;
		}


		public static float GetArmorDefence(float durability)
		{
			float d01 = Mathf.Clamp01(durability / PEVCConfig.instance.maxArmorDurability);
			return Interpolation.EaseOut(d01) * PEVCConfig.instance.maxArmorDefence;
		}
	}
}