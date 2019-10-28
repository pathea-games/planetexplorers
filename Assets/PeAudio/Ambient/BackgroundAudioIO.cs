using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Pathea.Maths;

namespace PeAudio
{
	public partial class BackgroundAudio : MonoBehaviour
	{
		public void LoadConfig ()
		{
			string path = Application.streamingAssetsPath + "/ambient.xml";

			try
			{
				XmlDocument xmldoc = new XmlDocument ();
				xmldoc.Load(path);
				_sectionDescs = new Dictionary<int, SectionDesc>();
				foreach (XmlNode node in xmldoc.DocumentElement.ChildNodes)
				{
					if (node.Name == "SECTION")
					{
						SectionDesc section = new SectionDesc ();
						int id = XmlConvert.ToInt32(node.Attributes["id"].Value);
						string name = node.Attributes["name"].Value;
						section.id = id;
						section.name = name;
						List<SPOTDesc> spotlist = new List<SPOTDesc> ();
						foreach (XmlNode childnode in node.ChildNodes)
						{
							if (childnode.Name == "BGM" && section.bgmDesc == null)
							{
								section.bgmDesc = new BGMDesc ();
								section.bgmDesc.path = childnode.Attributes["path"].Value;
								section.bgmDesc.musicCount = XmlConvert.ToInt32(childnode.Attributes["musiccnt"].Value);
								section.bgmDesc.changeTime = StrToRange(childnode.Attributes["changetime"].Value);
							}
							else if (childnode.Name == "BGA" && section.bgaDesc == null)
							{
								section.bgaDesc = new BGADesc ();
								section.bgaDesc.path = childnode.Attributes["path"].Value;
							}
							else if (childnode.Name == "SPOT")
							{
								SPOTDesc desc = new SPOTDesc ();
								desc.path = childnode.Attributes["path"].Value;
								desc.density = XmlConvert.ToSingle(childnode.Attributes["density"].Value);
								desc.heightRange = StrToRange(childnode.Attributes["height"].Value);
								desc.minDistRange = StrToRange(childnode.Attributes["mindistance"].Value);
								desc.maxDistRange = StrToRange(childnode.Attributes["maxdistance"].Value);
								spotlist.Add(desc);
							}
						}
						section.spotDesc = new SPOTDesc[spotlist.Count] ;

						for (int i = 0; i < spotlist.Count; ++i)
							section.spotDesc[i] = spotlist[i];

						_sectionDescs[section.id] = section;
					}
				}
			}
			catch (Exception)
			{
				Debug.LogWarning("Load ambient.xml failed");
				_sectionDescs = new Dictionary<int, SectionDesc>();
			}
		}

		static Range1D StrToRange (string s)
		{
			string[] segs = s.Split(new string[] {","}, System.StringSplitOptions.RemoveEmptyEntries);
			Range1D r = new Range1D ();
			if (segs.Length == 1)
			{
				float f = System.Convert.ToSingle(segs[0].Trim());
				r.SetMinMax(f, f);
			}
			else if (segs.Length > 1)
			{
				r.SetMinMax(System.Convert.ToSingle(segs[0].Trim()), System.Convert.ToSingle(segs[1].Trim()));
			}
			else
			{
				throw new System.Exception("Invalid range string");
			}
			return r;
		}
	}
}