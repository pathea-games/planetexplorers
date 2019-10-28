using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

[Serializable()]
public class ShaderPairsTbl
{
	[XmlArray("ShaderPairs")]
	[XmlArrayItem("Pair", typeof(ShaderPair))]
	public ShaderPair[] shaderPairs { get; set; }
}
[Serializable()]
public class ShaderPair
{
	[XmlAttribute("ShellShader")]
	public string shellName { get; set; }
	[XmlAttribute("EntityShader")]
	public string entityName { get; set; }

	// not in serialization
	public Shader shell;
	public Shader entity;
}

public class CorruptShaderReplacement : MonoBehaviour
{
	const string c_shaderReplaceTblName = "shaderReplaceTbl"; //.xml
	static ShaderPairsTbl s_shaderPairsTbl;

	public string[] corruptShaderNames = new string[0] ;
	public Shader[] validShaders = new Shader[0] ;

	// Use this for initialization
	void Start ()
	{
		ReadShaderPairsTbl (false, true);
	}

	public static Shader FindValidShader (string shadername)
	{
		if (s_shaderPairsTbl != null) {
			int n = s_shaderPairsTbl.shaderPairs.Length;
			for (int i = 0; i < n; ++i) {
				if (shadername == s_shaderPairsTbl.shaderPairs[i].shellName)
					return s_shaderPairsTbl.shaderPairs[i].entity;
			}
		}
		return null;
	}
	public static void ReadShaderPairsTbl(bool bFindShellShader, bool bFindEntityShader){
		TextAsset xmlResource = Resources.Load(c_shaderReplaceTblName) as TextAsset;
		StringReader reader = new StringReader(xmlResource.text);
		if (null == reader)
			return;
		
		XmlSerializer serializer = new XmlSerializer(typeof(ShaderPairsTbl));
		s_shaderPairsTbl = (ShaderPairsTbl)serializer.Deserialize(reader);
		reader.Close();

		if (s_shaderPairsTbl != null) {
			int n = s_shaderPairsTbl.shaderPairs.Length;
			if(bFindShellShader){
				for (int i = 0; i < n; ++i) {
					s_shaderPairsTbl.shaderPairs[i].shell = Shader.Find(s_shaderPairsTbl.shaderPairs[i].shellName);
				}
			}
			if(bFindEntityShader){
				for (int i = 0; i < n; ++i) {
					s_shaderPairsTbl.shaderPairs[i].entity = Shader.Find(s_shaderPairsTbl.shaderPairs[i].entityName);
				}
			}
		}
	}
	public static bool ReplaceShaderShellWithEntity(GameObject root)
	{
		if (s_shaderPairsTbl == null)
			return false;

		bool ret = false;
		int n = s_shaderPairsTbl.shaderPairs.Length;
		Renderer[] renders = root.GetComponentsInChildren<Renderer>(true);
		int nr = renders.Length;
		for (int j = 0; j < nr; j++) {
			Material[] mats = renders[j].sharedMaterials;
			int nm = mats.Length;
			for(int k = 0; k < nm; k++){
				if(mats[k]==null || mats[k].shader == null){
					if(mats[k]==null){
						Debug.LogError("Error on replacing "+renders[j].name+"'s MatNo."+k+" for it has not shader");
					} else {
						Debug.LogError("Error on replacing "+renders[j].name+"'s Mat "+mats[k].name+" for it has not shader");
					}
					continue;
				}
				string shaderName = mats[k].shader.name;
				for (int i = 0; i < n; ++i) {
					if (shaderName == s_shaderPairsTbl.shaderPairs[i].shellName){
						if(s_shaderPairsTbl.shaderPairs[i].entity == null || 
						   s_shaderPairsTbl.shaderPairs[i].entity.name != s_shaderPairsTbl.shaderPairs[i].entityName){
							s_shaderPairsTbl.shaderPairs[i].entity = Shader.Find(s_shaderPairsTbl.shaderPairs[i].entityName);
						}
						mats[k].shader = s_shaderPairsTbl.shaderPairs[i].entity;
						ret = true;
						break;
					}
				}
			}
		}
		return ret;
	}
	public static bool ReplaceShaderEntityWithShell(GameObject root)
	{
		if (s_shaderPairsTbl == null)
			return false;
		
		bool ret = false;
		int n = s_shaderPairsTbl.shaderPairs.Length;
		Renderer[] renders = root.GetComponentsInChildren<Renderer>(true);
		int nr = renders.Length;
		for (int j = 0; j < nr; j++) {
			Material[] mats = renders[j].sharedMaterials;
			int nm = mats.Length;
			for(int k = 0; k < nm; k++){
				if(mats[k]==null || mats[k].shader == null){
					if(mats[k]==null){
						Debug.LogError("Error on replacing "+renders[j].name+"'s MatNo."+k+" for it has not shader");
					} else {
						Debug.LogError("Error on replacing "+renders[j].name+"'s Mat "+mats[k].name+" for it has not shader");
					}
					continue;
				}
				string shaderName = mats[k].shader.name;
				for (int i = 0; i < n; ++i) {
					if (shaderName == s_shaderPairsTbl.shaderPairs[i].entityName){
						if(s_shaderPairsTbl.shaderPairs[i].shell == null || 
						   s_shaderPairsTbl.shaderPairs[i].shell.name != s_shaderPairsTbl.shaderPairs[i].shellName){
							s_shaderPairsTbl.shaderPairs[i].shell = Shader.Find(s_shaderPairsTbl.shaderPairs[i].shellName);
						}
						mats[k].shader = s_shaderPairsTbl.shaderPairs[i].shell;
						ret = true;
						break;
					}
				}
			}
		}
		return ret;
	}
}
