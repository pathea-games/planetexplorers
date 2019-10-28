using UnityEngine;

namespace WhiteCat
{
	public class VCPSimpleLight : VCSimpleObjectPart
	{
		[SerializeField] new Light light;
		[SerializeField] new Renderer renderer;


		public Color color
		{
			get { return light.color; }
			set
			{
				light.color = value;
				renderer.material.SetColor("_Color", value);
				renderer.material.SetColor("_HoloColor", value);
			}
		}


		public override CmdList GetCmdList()
		{
			var list = base.GetCmdList();
			list.Add(light.enabled ? "Turn Off" : "Turn On", () => light.enabled = !light.enabled);
			return list;
		}
	}
}