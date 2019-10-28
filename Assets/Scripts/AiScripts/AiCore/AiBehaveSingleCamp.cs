using UnityEngine;
using System.Collections;

public class AiBehaveSingleCamp : AiBehave 
{
	public override bool isSingle {
		get {
			return true;
		}
	}

	public override bool isMember {
		get {
			return false;
		}
	}

	public override bool isActive 
	{
		get 
		{
			return base.isActive;
		}
	}
}
