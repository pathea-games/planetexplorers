using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrassPrototypeMgr : MonoBehaviour
{
	
	public const int s_PrototypeRowCount = 8;
	public const int s_PrototypeColCount = 8;
	public const int s_MapResolution = 256;
	public const int s_PrototypeCount = s_PrototypeRowCount * s_PrototypeColCount;
	[SerializeField] public List<GrassPrototype> m_Prototypes;
	[SerializeField] public Texture2D m_DiffuseMap;
	[SerializeField] public Texture2D m_ParticleMap;
	[SerializeField] public Texture2D m_PropertyMap;
	[SerializeField] public string m_DiffuseMapFileName = "Diffuse2048";
	[SerializeField] public string m_ParticleMapFileName = "Particle2048";
	[SerializeField] public string m_PropertyMapFileName = "Property64x4";
	public void GenerateTextures ()
	{
		for ( int i = 0; i < s_PrototypeCount; ++i )
		{
			if ( m_Prototypes[i].m_Diffuse != null )
			{
				m_DiffuseMap.SetPixels((i%s_PrototypeRowCount)*s_MapResolution, (i/s_PrototypeRowCount)*s_MapResolution, s_MapResolution, s_MapResolution, 
					m_Prototypes[i].m_Diffuse.GetPixels(0,0,s_MapResolution,s_MapResolution));
			}
			else
			{
				m_DiffuseMap.SetPixels((i%s_PrototypeRowCount)*s_MapResolution, (i/s_PrototypeRowCount)*s_MapResolution, s_MapResolution, s_MapResolution, 
					new Color[s_MapResolution*s_MapResolution]);
			}
		}
//		for ( int i = 0; i < s_PrototypeCount; ++i )
//		{
//			if ( m_Prototypes[i].m_Particle != null )
//			{
//				m_ParticleMap.SetPixels((i%s_PrototypeRowCount)*s_MapResolution, (i/s_PrototypeRowCount)*s_MapResolution, s_MapResolution, s_MapResolution, 
//					m_Prototypes[i].m_Particle.GetPixels(0,0,s_MapResolution,s_MapResolution));
//			}
//			else
//			{
//				m_ParticleMap.SetPixels((i%s_PrototypeRowCount)*s_MapResolution, (i/s_PrototypeRowCount)*s_MapResolution, s_MapResolution, s_MapResolution, 
//					new Color[s_MapResolution*s_MapResolution]);
//			}
//		}
		for ( int i = 0; i < s_PrototypeCount; ++i )
		{
			m_PropertyMap.SetPixel(i,0,new Color(m_Prototypes[i].m_MinSize.x*0.5f, m_Prototypes[i].m_MinSize.y*0.5f, m_Prototypes[i].m_MaxSize.x*0.5f, m_Prototypes[i].m_MaxSize.y*0.5f));
			m_PropertyMap.SetPixel(i,1,new Color(m_Prototypes[i].m_BendFactor*2, (m_Prototypes[i].m_LODBias + 4.0f)/8.0f, 0, 1));
			m_PropertyMap.SetPixel(i,2,m_Prototypes[i].m_ParticleTintColor);
			m_PropertyMap.SetPixel(i,3,new Color(0,0,0,1));
		}
		m_DiffuseMap.Apply();
//		m_ParticleMap.Apply();
		byte[] pixels = null;
        pixels = m_DiffuseMap.EncodeToPNG();
   		System.IO.File.WriteAllBytes("Assets/Scripts/ZhouXun/Grass Billboards/Textures/" + m_DiffuseMapFileName + ".png", pixels);
//   		pixels = m_ParticleMap.EncodeToPNG();
//   		System.IO.File.WriteAllBytes("Assets/Scripts/ZhouXun/Grass Billboards/Textures/" + m_ParticleMapFileName + ".png", pixels);
   		pixels = m_PropertyMap.EncodeToPNG();
   		System.IO.File.WriteAllBytes("Assets/Scripts/ZhouXun/Grass Billboards/Textures/" + m_PropertyMapFileName + ".png", pixels);
	}
}
