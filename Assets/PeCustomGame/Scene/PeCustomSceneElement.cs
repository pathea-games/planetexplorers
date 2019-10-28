using UnityEngine;
using System.Collections;

namespace PeCustom
{
	public partial class PeCustomScene
	{
		public class SceneElement
		{
			public PeCustomScene scene { get { return PeCustomScene.Self; } }

			public SceneElement ()
			{
				IMonoLike ml = this as IMonoLike;
				if (ml != null)
					scene.AddMonoLikeItems(ml);
			}
		}
	}
	
}
