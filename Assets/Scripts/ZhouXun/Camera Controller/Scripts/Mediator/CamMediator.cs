using UnityEngine;
using System.Collections;

public static class CamMediator
{
	private static Texture2D m_AimTex;

    static Transform mTrans = null;
	public static Transform Character
	{
		get
        {
            if (null == mTrans)
            {
                Pathea.PeTrans t = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PeTrans>();
                if (null != t)
                {
                    mTrans = t.trans;
                }
            }

            return mTrans;
        }
	}

	public static Camera MainCamera
	{
		get { return Camera.main; }
	}

	public static Texture2D AimTexture
	{
		get
		{
			if ( m_AimTex == null )
			{
				m_AimTex = Resources.Load("GUI/Atlases/Tex/HitPoint") as Texture2D;
			}
			return m_AimTex;
		}
	}

	public static void DrawAimTextureGUI (Vector2 pos)
	{
		if (AimTexture == null)
			return;
		Texture2D aim_tex = AimTexture;
		GUI.DrawTexture(new Rect(Mathf.Floor(Screen.width * pos.x - aim_tex.width * 0.5f), 
		                         Mathf.Floor(Screen.height * (1-pos.y) - aim_tex.height * 0.5f), 
		                         aim_tex.width, aim_tex.height), aim_tex);
	}
}
