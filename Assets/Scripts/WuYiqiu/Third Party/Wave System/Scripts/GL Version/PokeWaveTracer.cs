using UnityEngine;
using System.Collections;

public class  PokeWaveTracer : glWaveTracer 
{
	public float scale = 1.0f;

	public float strength = 2.0f;

	private Material material;

	#region Parent_Override_Func

	Vector3 v1, v2, v3, v4;

	public override void CustomUpdate ()
	{
		Vector3 center = Position;

		Vector2 dir2 = new Vector2(1, 0);
		Vector2 orth_dir = new Vector3(0, 1);
		Vector2 center2 = new Vector2(center.x, center.z);
		
		v1 = center2 - (dir2 + orth_dir) * scale * 0.5f;
		v2 = center2 + (dir2 - orth_dir) * scale * 0.5f;
		
		v3 = center2 + (dir2 + orth_dir) * scale * 0.5f;
		v4 = center2 - (dir2 - orth_dir) * scale * 0.5f;
	}
	
	public override void Draw ()
	{
		if (material == null)
		{
			material = Resources.Load("Materials/PokeWaveMat") as Material;
			if (material != null)
				material = Material.Instantiate(material);
		}

		if (material == null)
			return;


//		Material mat = Instantiate(material) as Material;
		Material mat  = material;

		mat.SetFloat("_Strength", strength);

		GL.PushMatrix();
		int pcnt = mat.passCount;
		for (int i = 0; i < pcnt; i ++)
		{
			mat.SetPass(i);
			
			GL.Begin(GL.QUADS);
			GL.Color(Color.white);
			
			
			GL.TexCoord2(0, 0);
			GL.Vertex(new Vector3( v1.x, WaterHeight, v1.y));
			GL.TexCoord2(1, 0);
			GL.Vertex(new Vector3(v2.x, WaterHeight, v2.y));
			GL.TexCoord2(1, 1);
			GL.Vertex(new Vector3(v3.x, WaterHeight, v3.y));
			GL.TexCoord2(0, 1);
			GL.Vertex(new Vector3(v4.x, WaterHeight, v4.y));
			GL.End();
		}
		GL.PopMatrix();
	}
	
	#endregion
}
