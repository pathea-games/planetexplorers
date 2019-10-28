using UnityEngine;
using System.Collections;

public class CameraBreatheEffect : CamEffect
{
	public Vector3 PosAmp = Vector3.zero;
	public Vector3 PosT = Vector3.one;
	public Vector3 PosPhase = Vector3.zero;
	public Vector3 RotAmp = Vector3.zero;
	public Vector3 RotT = Vector3.one;
	public Vector3 RotPhase = Vector3.zero;

	private float t = 0;
	public override void Do ()
	{
		t += Mathf.Clamp(Time.deltaTime, 0.001f, 0.025f);
		PosT.x = Mathf.Abs(PosT.x);
		PosT.y = Mathf.Abs(PosT.y);
		PosT.z = Mathf.Abs(PosT.z);
		RotT.x = Mathf.Abs(RotT.x);
		RotT.y = Mathf.Abs(RotT.y);
		RotT.z = Mathf.Abs(RotT.z);
		if (PosT.x < 0.05f)
			PosT.x = 0.05f;
		if (PosT.y < 0.05f)
			PosT.y = 0.05f;
		if (PosT.z < 0.05f)
			PosT.z = 0.05f;
		if (RotT.x < 0.05f)
			RotT.x = 0.05f;
		if (RotT.y < 0.05f)
			RotT.y = 0.05f;
		if (RotT.z < 0.05f)
			RotT.z = 0.05f;
		float _2pi = Mathf.PI * 2f;
		Vector3 pos_bias = Vector3.zero;
		Vector3 rot_bias = Vector3.zero;

		pos_bias.x = PosAmp.x * Mathf.Sin((t / PosT.x + PosPhase.x) * _2pi);
		pos_bias.y = PosAmp.y * Mathf.Sin((t / PosT.y + PosPhase.y) * _2pi);
		pos_bias.z = PosAmp.z * Mathf.Sin((t / PosT.z + PosPhase.z) * _2pi);
		rot_bias.x = RotAmp.x * Mathf.Sin((t / RotT.x + RotPhase.x) * _2pi);
		rot_bias.y = RotAmp.y * Mathf.Sin((t / RotT.y + RotPhase.y) * _2pi);
		rot_bias.z = RotAmp.z * Mathf.Sin((t / RotT.z + RotPhase.z) * _2pi);

		m_TargetCam.transform.position += pos_bias;
		m_TargetCam.transform.eulerAngles += rot_bias;
	}
}
