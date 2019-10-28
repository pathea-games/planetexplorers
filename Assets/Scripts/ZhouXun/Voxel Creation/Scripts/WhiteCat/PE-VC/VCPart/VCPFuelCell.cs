using UnityEngine;

namespace WhiteCat
{
	public class VCPFuelCell : VCPart
	{
		[SerializeField] float _energyCapacity = 20000;
		public float energyCapacity { get { return _energyCapacity; } }
	}
}