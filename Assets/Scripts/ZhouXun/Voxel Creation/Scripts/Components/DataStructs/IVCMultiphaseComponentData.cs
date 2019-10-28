using UnityEngine;
using System.Collections;

public interface IVCMultiphaseComponentData
{
	int Phase { get; set; }
	void InversePhase ();
}
