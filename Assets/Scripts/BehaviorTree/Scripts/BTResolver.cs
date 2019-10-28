using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Reflection;

namespace Behave.Runtime
{
    public class BTResolver
    {
        public static Dictionary<string, BTAgent> s_btAgents = new Dictionary<string, BTAgent>();
        public static Dictionary<string, List<FieldInfo>> s_Fields = new Dictionary<string, List<FieldInfo>>();

		static bool _btCached = false;
        public static void RegisterToCache(string btPath)
        {
            if(!string.IsNullOrEmpty(btPath) && !s_btAgents.ContainsKey(btPath))
            {
				if(_btCached){
					BTAgent agent = Resolver(btPath);
					if (agent != null){
						s_btAgents.Add(btPath, agent);
					}
				} else {
					s_btAgents.Add(btPath, null);	// Placeholder
				}                
            }
        }
		public static IEnumerator ApplyCacheBt()
		{
			_btCached = true;
			List<string> keys = new List<string> (s_btAgents.Keys);
			int nKeys = keys.Count;	// about 160
			for (int i = 0; i < nKeys; i++) {
				string btPath = keys[i];
				if(s_btAgents[btPath] == null){
					s_btAgents[btPath] = Resolver(btPath);
					yield return 0;
				}
			}
			//Debug.LogError("All bt cached: "+nKeys);
		}

        public static BTAgent Pop(string btPath)
        {
            BTAgent agent = null;
            BTAgent agentCopy;
            if (s_btAgents.TryGetValue(btPath, out agentCopy) && agentCopy != null)
                agent = agentCopy.Clone();
            else
                Debug.Log("Can't find behave tree : " + btPath);

            return agent;
        }

        public static BTAgent Instantiate(string btPath, IAgent agent)
        {
            BTAgent newAgent = Pop(btPath);

            if (newAgent != null)
                newAgent.SetAgent(agent);

            return newAgent;
        }

        static BTAgent Resolver(string btPath)
        {
            BTAgent agent = null;

            TextAsset asset = UnityEngine.Resources.Load(btPath) as TextAsset;
            if (asset != null && !string.IsNullOrEmpty(asset.text))
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.IgnoreComments = true;
                    XmlReader reader = XmlReader.Create(new StringReader(asset.text), settings);
                    xmlDoc.Load(reader);

                    XmlElement root = xmlDoc.SelectSingleNode("Tree") as XmlElement;
                    string library = XmlUtil.GetAttributeString(root, "Library");
                    string tree = XmlUtil.GetAttributeString(root, "Tree");

                    agent = new BTAgent(btPath, library, tree);
                    agent.Tick();

                    //Profiler.BeginSample("XmlNode");
                    XmlNodeList xmlNodeList = root.GetElementsByTagName("Action");
                    foreach (XmlNode node in xmlNodeList)
                    {
                        XmlElement e = node as XmlElement;
                        string typeName = XmlUtil.GetAttributeString(e, "Type");

                        BTAction action = agent.GetAction(typeName);

                        if (action == null)
                        {
                            action = Reflecter.Instance.CreateAction(typeName);
                            agent.RegisterAction(action);
                        }

                        if (action != null)
                        {
                            InitData(btPath, action, e);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("["+ btPath +"]" + "<" + e + ">");
                }

            }

            return agent;
        }

        static void InitData(string btPath, BTAction action, XmlElement e)
        {
            if (!s_Fields.ContainsKey(action.Name))
                s_Fields.Add(action.Name, new List<FieldInfo>());

            if (action != null && e != null)
            {
                //foreach (FieldInfo field in action.GetType().GetFields())
                //{
                //    foreach (object attribute in field.GetCustomAttributes(true))
                //    {
                //        if (typeof(BehaveAttribute).IsInstanceOfType(attribute))
                //        {
                //            string error = SetValue(field, action, e);
                //            if (error != "")
                //            {
                //                //Debug.LogError("Action : " + action.Name + "["+ error +"]");
                //            }
                //            continue;
                //        }
                //    }
                //}

                //Profiler.BeginSample("Resolver");
                object[] members = action.GetType().GetCustomAttributes(true);
                for (int i = 0; i < members.Length; i++)
                {
                    if (typeof(BehaveAction).IsInstanceOfType(members[i]))
                    {
                        BehaveAction behave = members[i] as BehaveAction;
                        XmlNodeList nodes = e.GetElementsByTagName("Data");
                        for (int j = 0; j < nodes.Count; j++)
                        {
                            XmlElement d = nodes[j] as XmlElement;
                            if (behave.dataType != null)
                            {
                                object obj = System.Activator.CreateInstance(behave.dataType);
                                string dataName = XmlUtil.GetAttributeString(d, "Name");
                                FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
                                for (int k = 0; k < fields.Length; k++)
                                {
                                    FieldInfo field = fields[k];
                                    object[] attrs = field.GetCustomAttributes(true);
                                    for (int n = 0; n < attrs.Length; n++)
                                    {
                                        if (typeof(BehaveAttribute).IsInstanceOfType(attrs[n]))
                                        {
                                            if (s_Fields[action.Name].Find(ret => ret.Name == field.Name) == null)
                                                s_Fields[action.Name].Add(field);

                                            string error = SetValue(field, obj, d);
                                            if (error != "")
                                            {
                                                Debug.LogError(btPath + "-" + "Action:" + action.Name + "-" + "Data : " + dataName + "-" + "[" + error + "]");
                                            }
                                            continue;
                                        }
                                    }
                                }
                                action.AddData(dataName, obj);
                            }
                        }
                    }
                }
                //Profiler.EndSample();
            }
        }

        static string SetValue(FieldInfo field, object obj, XmlElement e)
        {
            if (!e.HasAttribute(field.Name))
            {
                //return "Don't find field : " + field.Name;
                return "";
            }

            try
            {
                if (field.FieldType.Equals(typeof(int)))
                    field.SetValue(obj, XmlUtil.GetAttributeInt32(e, field.Name));
                else if (field.FieldType.Equals(typeof(float)))
                    field.SetValue(obj, XmlUtil.GetAttributeFloat(e, field.Name));
                else if (field.FieldType.Equals(typeof(bool)))
                    field.SetValue(obj, XmlUtil.GetAttributeBool(e, field.Name));
                else if (field.FieldType.Equals(typeof(Vector3)))
                    field.SetValue(obj, XmlUtil.GetAttributeVector3(e, field.Name));
                else if (field.FieldType.Equals(typeof(string)))
                    field.SetValue(obj, XmlUtil.GetAttributeString(e, field.Name));
                else if (field.FieldType.Equals(typeof(int[])))
                    field.SetValue(obj, XmlUtil.GetAttributeInt32Array(e, field.Name));
                else if (field.FieldType.Equals(typeof(float[])))
                    field.SetValue(obj, XmlUtil.GetAttributeFloatArray(e, field.Name));
                else if (field.FieldType.Equals(typeof(string[])))
                    field.SetValue(obj, XmlUtil.GetAttributeStringArray(e, field.Name));
                else if (field.FieldType.Equals(typeof(Vector3[])))
                    field.SetValue(obj, XmlUtil.GetAttributeVector3Array(e, field.Name));
                else if (field.FieldType.Equals(typeof(List<int>)))
                    field.SetValue(obj, new List<int>(XmlUtil.GetAttributeInt32Array(e, field.Name)));
                else if (field.FieldType.Equals(typeof(List<float>)))
                    field.SetValue(obj, new List<float>(XmlUtil.GetAttributeFloatArray(e, field.Name)));
                else if (field.FieldType.Equals(typeof(List<string>)))
                    field.SetValue(obj, new List<string>(XmlUtil.GetAttributeStringArray(e, field.Name)));
                else if (field.FieldType.Equals(typeof(List<Vector3>)))
                    field.SetValue(obj, new List<Vector3>(XmlUtil.GetAttributeVector3Array(e, field.Name)));
                else
                    Debug.LogError("Can not find type : " + field.FieldType.ToString());
            }
            catch (System.Exception ex)
            {
                return "Obj : " + obj.GetType().Name + " --> " + "Field : " + field.Name + " --> " + ex;
            }

            return "";
        }
    }
}
