using UnityEngine;
using System.Collections;

public static class RoleHerderTexture
{
	static Texture2D mRoleHerder = null;
	public static void SetTexture(Texture2D _texture)
	{
		mRoleHerder = _texture;
	}

	public static Texture2D GetTexture()
	{
		return mRoleHerder;
	}
}
