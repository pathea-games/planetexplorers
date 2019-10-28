using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Behave.Runtime;

namespace Behave.Runtime
{
    public class BehaveAction : System.Attribute
    {
        System.Type m_Type;
        System.Type m_DataType;
        string m_Name;

        public string name { get { return m_Name; } }
        public System.Type type { get { return m_Type; } }
        public System.Type dataType { get { return m_DataType; } }

        public BehaveAction(System.Type argType, string argName)
        {
            m_Type = argType;
            m_DataType = argType.GetNestedType("Data", BindingFlags.Instance | BindingFlags.NonPublic);
            m_Name = argName;
        }
    }

    public class BehaveAttribute : System.Attribute
    {
        public BehaveAttribute()
        {
 
        }
    }

    public class BehaveLibrary
    {
        string m_LibraryName;
        string m_LibraryDllName;
        string m_LibraryTypeName;

        Assembly m_LiraryAssembly;
        System.Type m_LibraryType;
        System.Type m_TreeType;
        MethodInfo m_InstantiateFunc;

        Dictionary<string, System.Type> m_TreeTypes;
        Dictionary<string, List<string>> m_TreeActions;

        public BehaveLibrary(string libName)
        {
            m_LibraryName = libName;
            m_LibraryDllName = GetLibraryDll(m_LibraryName);
            m_LibraryTypeName = GetLibraryType(m_LibraryName);

            m_LiraryAssembly = Assembly.Load(m_LibraryDllName);
            if (m_LiraryAssembly != null)
            {
                m_LibraryType = m_LiraryAssembly.GetType(m_LibraryTypeName);
                if (m_LibraryType != null)
                {
                    m_TreeType = m_LibraryType.GetNestedType("TreeType");
                    m_InstantiateFunc = m_LibraryType.GetMethod("InstantiateTree", 
                                                                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, 
                                                                null, 
                                                                new Type[] { m_TreeType, typeof(IAgent)}, 
                                                                null);
                }
            }

            if(error == "")
            {
                InitTreeType();

                InitTreeActions();
            }
        }

        public string error
        {
            get
            {
                if (m_LiraryAssembly == null)
                {
                    return "Can't find assembly : " + m_LibraryDllName;
                }

                if (m_LibraryType == null)
                {
                    return "Can't find library : " + m_LibraryTypeName;
                }

                if (m_TreeType == null || !m_TreeType.IsSubclassOf(typeof(System.Enum)))
                {
                    return "Tree type of " + m_LibraryName + " is not right!";
                }

                return "";
            }
        }

        public bool Match(string libraryName)
        {
            return m_LibraryName.Equals(libraryName) && error == "";
        }

        public List<string> GetTrees()
        {
            return new List<string>(m_TreeActions.Keys);
        }

        public List<string> GetActions(string treeName)
        {
            return m_TreeActions.ContainsKey(treeName) ? m_TreeActions[treeName] : null;
        }

        public Tree Instantiate(string treeName)
        {
            if (error != "")
            {
                Debug.LogError(error);
                return null;
            }

            try
            {
                object obj = System.Enum.Parse(m_TreeType, treeName);
                return obj != null ? m_InstantiateFunc.Invoke(null, new object[] { obj, null }) as Tree : null;
            }
            catch (Exception ex)
            {
                Debug.LogError("[" + m_LibraryName + "]" + "[" + treeName + "]" + ex);
                return null;
            }
        }

        string GetLibraryDll(string libName)
        {
            return libName + "Build";
        }

        string GetLibraryType(string libName)
        {
            return "BL" + libName;
        }

        string GetTreeType(string treeName)
        {
            object treeType = GetTreeEnum(treeName);
            return treeType != null ? "BT" + m_LibraryName + "T" + (int)treeType : "";
        }

        object GetTreeEnum(string treeName)
        {
            try
            {
                return System.Enum.Parse(m_TreeType, treeName);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(ex);
                return null;
            }
        }

        void InitTreeType()
        {
            m_TreeTypes = new Dictionary<string, System.Type>();

            System.Type[] treeTypes = m_LiraryAssembly.GetTypes();
            foreach (System.Type treeType in treeTypes)
            {
                if (treeType.IsSubclassOf(typeof(Tree)))
                {
                    if (!m_TreeTypes.ContainsKey(treeType.Name))
                    {
                        m_TreeTypes.Add(treeType.Name, treeType);
                        //Debug.Log("Register tree type : " + treeType.Name);
                    }
                }
            }
        }

        void InitTreeActions()
        {
            m_TreeActions = new Dictionary<string, List<string>>();

            string[] treeNames = System.Enum.GetNames(m_TreeType);
            foreach (string treeName in treeNames)
            {
                if (treeName.Equals("Unknown"))
                    continue;

                string treeTypeName = GetTreeType(treeName);
                if (treeTypeName != "" && m_TreeTypes.ContainsKey(treeTypeName) && m_TreeTypes[treeTypeName] != null)
                {
                    if (!m_TreeActions.ContainsKey(treeName))
                        m_TreeActions.Add(treeName, new List<string>());

                    FieldInfo[] fields = m_TreeTypes[treeTypeName].GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (FieldInfo field in fields)
                    {
                        if (field.Name.Contains("ARF"))
                        {
                            string actionName = field.Name.Substring(("ARF").Length);
                            if (!m_TreeActions[treeName].Contains(actionName))
                            {
                                m_TreeActions[treeName].Add(actionName);
                                //Debug.Log("Library = " + m_LibraryName + " | " + "Tree = " + treeName + " | " + "Register action [" + actionName + "]");
                            }
                        }
                    }
                }
                else
                {
                    //Debug.LogError("Tree name : " + treeTypeName + " is wrong!");
                }
            }
        }
    }

    public class Reflecter
    {
        #region sington
        static Reflecter _Instance;
        public static Reflecter Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new Reflecter();
                }

                return _Instance;
            }
        }
        #endregion

        List<BehaveAction> m_BehaveActions;
        List<BehaveLibrary> m_BehaveLibrarys;

        public Reflecter()
        {
            InitAction();

            InitLibrary();
        }

        void InitAction()
        {
            m_BehaveActions = new List<BehaveAction>();

            System.Type[] types = Assembly.GetAssembly(typeof(BTAction)).GetTypes();
            foreach (System.Type type in types)
            {
                if (type.IsSubclassOf(typeof(BTAction)))
                {
                    System.Object[] attributes = type.GetCustomAttributes(true);
                    foreach (System.Object attribute in attributes)
                    {
                        if (typeof(BehaveAction).IsInstanceOfType(attribute))
                        {
                            m_BehaveActions.Add(attribute as BehaveAction);
                            //Debug.Log("Register action = " + " [" + type.Name + "]");
                        }
                    }
                }
            }
        }

        void InitLibrary()
        {
            m_BehaveLibrarys = new List<BehaveLibrary>();

            foreach (string libName in BTConfig.s_Librarys)
            {
                m_BehaveLibrarys.Add(new BehaveLibrary(libName));
            }
        }

        #region public interface

        public BTAction CreateAction(string actionName)
        {
            BehaveAction behave = m_BehaveActions.Find(ret => ret.name.Equals(actionName));
            System.Type actionType = (behave != null ? behave.type : null);

            if (actionType == null)
            {
                Debug.LogError("Can't find action : " + "[" + actionName + "]");
                return null;
            }
            BTAction action = System.Activator.CreateInstance(actionType) as BTAction;
            action.SetName(actionName);
            return action;
        }

        public Tree Instantiate(string libraryName, string treeName)
        {
            BehaveLibrary libarry = m_BehaveLibrarys.Find(ret => ret.Match(libraryName));
            return libarry != null ? libarry.Instantiate(treeName) : null;
        }

        public List<string> GetActionsOfTree(string libraryName, string treeName)
        {
            BehaveLibrary libarry = m_BehaveLibrarys.Find(ret => ret.Match(libraryName));
            return libarry != null ? libarry.GetActions(treeName) : null;
        }

        public List<string> GetTrees(string libraryName)
        {
            BehaveLibrary libarry = m_BehaveLibrarys.Find(ret => ret.Match(libraryName));
            return libarry != null ? libarry.GetTrees() : null;
        }

        public System.Type[] GetActions()
        {
            List<System.Type> tmpList = new List<System.Type>();
            foreach (BehaveAction behave in m_BehaveActions)
            {
                tmpList.Add(behave.type);
            }
            return tmpList.ToArray();
        }

        #endregion
    }
}
