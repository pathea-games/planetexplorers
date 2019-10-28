using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DontDestroyFonts : MonoBehaviour
{
	public List<Font> m_DontDestroyFonts;
	
	// Awake init
	void Awake ()
	{
		foreach (Font ft in m_DontDestroyFonts)
		{
			Font.DontDestroyOnLoad(ft);
		}
	}
}
