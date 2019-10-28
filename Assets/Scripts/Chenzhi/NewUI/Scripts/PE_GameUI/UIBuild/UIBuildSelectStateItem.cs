using UnityEngine;
using System.Collections;

public class UIBuildSelectStateItem : MonoBehaviour
{
    [SerializeField] UISprite m_IconSprite;

	public int type = 0;

	public void SetIcon(string sprit_name)
    {
        m_IconSprite.spriteName = sprit_name;
    }
}
