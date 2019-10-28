using UnityEngine;
using System.Collections;

namespace Behave.Runtime.Action
{
    public class BTUtil : MonoBehaviour
    {
        public static string GetString(string data, int index)
        {
            string result = "";

            string[] args = PETools.PEUtil.ToArrayString(data, ' ');
            if (index >= 0 && index < args.Length)
                result = args[index];
            else
                Debug.LogError("[" + data + "]" + "["+ "index : " + index +"]");

            return result;
        }

        public static bool GetBool(string data, int index)
        {
            bool result = false;

            string[] args = PETools.PEUtil.ToArrayString(data, ' ');
            if (index >= 0 && index < args.Length)
            {
                try
                {
                    result = System.Convert.ToBoolean(args[index]);
                }
                catch (System.Exception)
                {
                    Debug.LogError("[" + data + "]" + "[" + args[index] + "]" + " is not boolean!");
                }
            }
            else
                Debug.LogError("[" + data + "]" + "[" + "index : " + index + "]");

            return result;
        }

        public static float GetFloat(string data, int index)
        {
            float result = 0.0f;

            string[] args = PETools.PEUtil.ToArrayString(data, ' ');
            if (index >= 0 && index < args.Length)
            {
                try
                {
                    result = System.Convert.ToSingle(args[index]);
                }
                catch (System.Exception)
                {
                    Debug.LogError("[" + data + "]" + "[" + args[index] + "]" + " is not float!");
                }
            }
            else
                Debug.LogError("[" + data + "]" + "[" + "index : " + index + "]");

            return result;
        }

        public static int GetInt32(string data, int index)
        {
            int result = 0;

            string[] args = PETools.PEUtil.ToArrayString(data, ' ');
            if (index >= 0 && index < args.Length)
            {
                try
                {
                    result = System.Convert.ToInt32(args[index]);
                }
                catch (System.Exception)
                {
                    Debug.LogError("[" + data + "]" + "[" + args[index] + "]" + " is not int!");
                }
            }
            else
                Debug.LogError("[" + data + "]" + "[" + "index : " + index + "]");

            return result;
        }

        public static Vector3 GetVector3(string data, int index)
        {
            Vector3 result = Vector3.zero;

            string[] args = PETools.PEUtil.ToArrayString(data, ' ');
            if (index >= 0 && index < args.Length)
            {
                try
                {
                    result = PETools.PEUtil.ToVector3(args[index], ',');
                }
                catch (System.Exception)
                {
                    Debug.LogError("[" + data + "]" + "[" + args[index] + "]" + " is not vector3!");
                }
            }
            else
                Debug.LogError("[" + data + "]" + "[" + "index : " + index + "]");

            return result;
        }
    }
}
