using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Vector3 = UnityEngine.Vector3;

namespace PatheaScript
{
    [System.Serializable]
    public class VarValue
    {
        public enum EType
        {
            Int,
            Bool,
            Float,
            Vector3,
            String,
            Var,
            Max,
        }

        VarValue ConvertToInt
        {
            get
            {
                if (null == mValue)
                {
                    return new VarValue(0);
                }
                else
                {
                    return new VarValue(int.Parse((string)mValue));
                }
            }
        }

        VarValue ConvertToBool
        {
            get
            {
                if (null == mValue)
                {
                    return new VarValue(false);
                }
                else
                {
                    return new VarValue(bool.Parse((string)mValue));
                }
            }
        }

        VarValue ConvertToFloat
        {
            get
            {
                if (null == mValue)
                {
                    return new VarValue(0f);
                }
                else
                {
                    return new VarValue(float.Parse((string)mValue));
                }
            }
        }
        VarValue ConvertToVector3
        {
            get
            {
                if (null == mValue)
                {
                    return new VarValue(Vector3.zero);
                }
                else
                {
                    return new VarValue(Util.GetVector3((string)mValue));
                }
            }
        }

        VarValue ConvertToText
        {
            get
            {
                if (null == mValue)
                {
                    return new VarValue("");
                }
                else
                {
                    return new VarValue((string)mValue);
                }
            }
        }

        VarValue Convert(EType eType)
        {
            switch (eType)
            {
                case EType.Int:
                    return ConvertToInt;
                case EType.Bool:
                    return ConvertToBool;
                case EType.Float:
                    return ConvertToFloat;
                case EType.Vector3:
                    return ConvertToVector3;
                case EType.String:
                    return ConvertToText;
                default:
                    throw new System.Exception("no default value");
            }
        }

        object mValue;
        EType mEType;

        public VarValue(int v)
        {
            mValue = v;
            mEType = EType.Int;
        }

        public VarValue(bool v)
        {
            mValue = v;
            mEType = EType.Bool;
        }

        public VarValue(float v)
        {
            mValue = v;
            mEType = EType.Float;
        }

        public VarValue(Vector3 v)
        {
            mValue = v;
            mEType = EType.Vector3;
        }

        public VarValue(string v)
        {
            mValue = v;
            mEType = EType.String;
        }

        public VarValue(object v)
        {
            mValue = v;
            mEType = EType.Var;
        }

        public VarValue()
        {
            mValue = null;
            mEType = EType.Var;
        }

        public bool IsNil
        {
            get
            {
                return (null == mValue) && (mEType == EType.Var);
            }
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", mEType, (null == mValue) ? "null" : mValue);
        }

        public override bool Equals(object obj)
        {
            return this == (obj as VarValue);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region Operate
        #region 运算方法
        //冥
        public static VarValue Power(VarValue lhs, VarValue rhs)
        {
            if (false == lhs.IsNil || false == rhs.IsNil)
            {
                if (lhs.mEType == rhs.mEType)
                {
                    if (rhs.mEType == EType.Int)
                        return new VarValue((int)UnityEngine.Mathf.Pow((int)lhs.mValue, (int)rhs.mValue));
                    else if (rhs.mEType == EType.Float)
                        return new VarValue((float)UnityEngine.Mathf.Pow((float)lhs.mValue, (float)rhs.mValue));
                }
                else
                {
                    if (rhs.mEType == EType.Float)
                    {
                        if (lhs.mEType == EType.Int)
                            return new VarValue((int)UnityEngine.Mathf.Pow((int)lhs.mValue, (float)rhs.mValue));
                    }
                    else if (rhs.mEType == EType.Int)
                    {
                        if (lhs.mEType == EType.Float)
                            return new VarValue((float)UnityEngine.Mathf.Pow((float)lhs.mValue, (int)rhs.mValue));
                    }
                }
            }
            throw new System.NotSupportedException(lhs.ToString() + " Pow(" + rhs.ToString() + ") not supported.");
        }
        //xor 异或，后操作数为Bool类型，则作用为“非”
        public static VarValue operator ^(VarValue lhs, VarValue rhs)
        {
            if (false == lhs.IsNil || false == rhs.IsNil)
            {
                if (lhs.mEType == rhs.mEType)
                {
                    if (rhs.mEType == EType.Bool)
                        //后操作数为bool，作用为“非”
                        return new VarValue(!(bool)rhs.mValue);
                    else if (rhs.mEType == EType.Int)
                    {
                        return new VarValue((int)lhs.mValue ^ (int)rhs.mValue);
                    }
                }

            }
            throw new System.NotSupportedException(lhs.ToString() + " ^ " + rhs.ToString() + " not supported.");
        }
        #endregion

        public static VarValue operator +(VarValue lhs, VarValue rhs)
        {
            if (false == lhs.IsNil || false == rhs.IsNil)
            {
                if (lhs.mEType == rhs.mEType)
                {
                    if (EType.Int == lhs.mEType)
                    {
                        return new VarValue((int)lhs.mValue + (int)rhs.mValue);
                    }
                    else if (EType.Float == lhs.mEType)
                    {
                        return new VarValue((float)lhs.mValue + (float)rhs.mValue);
                    }
                    else if (EType.Vector3 == lhs.mEType)
                    {
                        return new VarValue((Vector3)lhs.mValue + (Vector3)rhs.mValue);
                    }
                    else if (EType.String == lhs.mEType)
                    {
                        return new VarValue(string.Format("{0}{1}", lhs.mValue, rhs.mValue));
                    }
                    else if (EType.Var == lhs.mEType)
                    {
                        return Util.TryParseVarValue((string)lhs.mValue) + Util.TryParseVarValue((string)rhs.mValue);
                    }
                }
                else
                {
                    if (EType.Var == lhs.mEType)
                    {
                        return lhs.Convert(rhs.mEType) + rhs;
                    }
                    else if (EType.Var == rhs.mEType)
                    {
                        return lhs + rhs.Convert(lhs.mEType);
                    }
                }
            }
            throw new System.NotSupportedException(lhs.ToString() + " + " + rhs.ToString() + "not supported.");
        }

        public static VarValue operator -(VarValue lhs, VarValue rhs)
        {
            if (lhs.mEType == rhs.mEType)
            {
                if (EType.Int == lhs.mEType)
                {
                    return new VarValue((int)lhs.mValue - (int)rhs.mValue);
                }
                else if (EType.Float == lhs.mEType)
                {
                    return new VarValue((float)lhs.mValue - (float)rhs.mValue);
                }
                else if (EType.Vector3 == lhs.mEType)
                {
                    return new VarValue((Vector3)lhs.mValue - (Vector3)rhs.mValue);
                }
                else if (EType.Var == lhs.mEType)
                {
                    return Util.TryParseVarValue((string)lhs.mValue) - Util.TryParseVarValue((string)rhs.mValue);
                }
            }
            else
            {
                if (EType.Var == lhs.mEType)
                {
                    return lhs.Convert(rhs.mEType) - rhs;
                }
                else if (EType.Var == rhs.mEType)
                {
                    return lhs - rhs.Convert(lhs.mEType);
                }
            }

            throw new System.NotSupportedException(lhs.ToString() + " - " + rhs.ToString() + "not supported.");
        }

        public static VarValue operator *(VarValue lhs, VarValue rhs)
        {
            if (lhs.mEType == rhs.mEType)
            {
                if (EType.Int == lhs.mEType)
                {
                    return new VarValue((int)lhs.mValue - (int)rhs.mValue);
                }
                else if (EType.Float == lhs.mEType)
                {
                    return new VarValue((float)lhs.mValue - (float)rhs.mValue);
                }
                else if (EType.Var == lhs.mEType)
                {
                    return Util.TryParseVarValue((string)lhs.mValue) * Util.TryParseVarValue((string)rhs.mValue);
                }
            }
            else
            {
                if (EType.Var == lhs.mEType)
                {
                    return lhs.Convert(rhs.mEType) * rhs;
                }
                else if (EType.Var == rhs.mEType)
                {
                    return lhs * rhs.Convert(lhs.mEType);
                }
            }

            throw new System.NotSupportedException(lhs.ToString() + " * " + rhs.ToString() + "not supported.");
        }

        public static VarValue operator /(VarValue lhs, VarValue rhs)
        {
            if (lhs.mEType == rhs.mEType)
            {
                if (EType.Int == lhs.mEType)
                {
                    return new VarValue((int)lhs.mValue / (int)rhs.mValue);
                }
                else if (EType.Float == lhs.mEType)
                {
                    return new VarValue((float)lhs.mValue / (float)rhs.mValue);
                }
                else if (EType.Var == lhs.mEType)
                {
                    return Util.TryParseVarValue((string)lhs.mValue) / Util.TryParseVarValue((string)rhs.mValue);
                }
            }
            else
            {
                if (EType.Var == lhs.mEType)
                {
                    return lhs.Convert(rhs.mEType) / rhs;
                }
                else if (EType.Var == rhs.mEType)
                {
                    return lhs / rhs.Convert(lhs.mEType);
                }
            }

            throw new System.NotSupportedException(lhs.ToString() + " / " + rhs.ToString() + "not supported.");
        }

        public static VarValue operator %(VarValue lhs, VarValue rhs)
        {
            if (lhs.mEType == rhs.mEType)
            {
                if (EType.Int == lhs.mEType)
                {
                    return new VarValue((int)lhs.mValue % (int)rhs.mValue);
                }
                else if (EType.Var == lhs.mEType)
                {
                    return Util.TryParseVarValue((string)lhs.mValue) % Util.TryParseVarValue((string)rhs.mValue);
                }
            }
            else
            {
                if (EType.Var == lhs.mEType)
                {
                    return lhs.Convert(rhs.mEType) % rhs;
                }
                else if (EType.Var == rhs.mEType)
                {
                    return lhs % rhs.Convert(lhs.mEType);
                }
            }

            throw new System.NotSupportedException(lhs.ToString() + " % " + rhs.ToString() + "not supported.");
        }

        public static bool operator ==(VarValue lhs, VarValue rhs)
        {
            if (object.ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            if (object.ReferenceEquals(lhs, null) || object.ReferenceEquals(rhs, null))
            {
                return false;
            }

            if (lhs.mEType == rhs.mEType)
            {
                if (lhs.mEType == EType.Int)
                {
                    return (int)lhs.mValue == (int)rhs.mValue;
                }
                else if (lhs.mEType == EType.Bool)
                {
                    return (bool)lhs.mValue == (bool)rhs.mValue;
                }
                else if (lhs.mEType == EType.Float)
                {
                    return (float)lhs.mValue == (float)rhs.mValue;
                }
                else if (lhs.mEType == EType.Vector3)
                {
                    return (Vector3)lhs.mValue == (Vector3)rhs.mValue;
                }
                else if (lhs.mEType == EType.String)
                {
                    return string.Equals(lhs.mValue, rhs.mValue);
                }
                else if (EType.Var == lhs.mEType)
                {
                    return Util.TryParseVarValue((string)lhs.mValue) == Util.TryParseVarValue((string)rhs.mValue);
                }
            }
            else
            {
                if (EType.Var == lhs.mEType)
                {
                    return lhs.Convert(rhs.mEType) == rhs;
                }
                else if (EType.Var == rhs.mEType)
                {
                    return lhs == rhs.Convert(lhs.mEType);
                }
            }

            throw new System.NotSupportedException(lhs.ToString() + " == " + rhs.ToString() + "not supported.");
        }

        public static bool operator !=(VarValue lhs, VarValue rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >(VarValue lhs, VarValue rhs)
        {
            if (lhs.mEType == rhs.mEType)
            {
                if (EType.Int == lhs.mEType)
                {
                    return (int)lhs.mValue > (int)rhs.mValue;
                }
                else if (EType.Float == lhs.mEType)
                {
                    return (float)lhs.mValue > (float)rhs.mValue;
                }
                else if (EType.Var == lhs.mEType)
                {
                    return Util.TryParseVarValue((string)lhs.mValue) > Util.TryParseVarValue((string)rhs.mValue);
                }
            }
            else
            {
                if (EType.Var == lhs.mEType)
                {
                    return lhs.Convert(rhs.mEType) > rhs;
                }
                else if (EType.Var == rhs.mEType)
                {
                    return lhs > rhs.Convert(lhs.mEType);
                }
            }

            throw new System.NotSupportedException(lhs.ToString() + " > " + rhs.ToString() + "not supported.");
        }

        public static bool operator >=(VarValue lhs, VarValue rhs)
        {
            return (lhs > rhs) || (lhs == rhs);
        }

        public static bool operator <(VarValue lhs, VarValue rhs)
        {
            if (lhs.mEType == rhs.mEType)
            {
                if (EType.Int == lhs.mEType)
                {
                    return (int)lhs.mValue < (int)rhs.mValue;
                }
                else if (EType.Float == lhs.mEType)
                {
                    return (float)lhs.mValue < (float)rhs.mValue;
                }
                else if (EType.Var == lhs.mEType)
                {
                    return Util.TryParseVarValue((string)lhs.mValue) < Util.TryParseVarValue((string)rhs.mValue);
                }
            }
            else
            {
                if (EType.Var == lhs.mEType)
                {
                    return lhs.Convert(rhs.mEType) < rhs;
                }
                else if (EType.Var == rhs.mEType)
                {
                    return lhs < rhs.Convert(lhs.mEType);
                }
            }

            throw new System.NotSupportedException(lhs.ToString() + " < " + rhs.ToString() + "not supported.");
        }

        public static bool operator <=(VarValue lhs, VarValue rhs)
        {
            return (lhs < rhs) || (lhs == rhs);
        }
        #endregion

        #region Convert
        public static implicit operator VarValue(int v)
        {
            return new VarValue(v);
        }

        public static explicit operator int(VarValue v)
        {
            if (EType.Int != v.mEType)
            {
                throw new System.NotSupportedException(v.ToString() + "convert to int not supported.");
            }

            return (int)v.mValue;
        }

        public static implicit operator VarValue(bool v)
        {
            return new VarValue(v);
        }

        public static explicit operator bool(VarValue v)
        {
            if (EType.Bool != v.mEType)
            {
                throw new System.NotSupportedException(v.ToString() + "convert to bool not supported.");
            }

            return (bool)v.mValue;
        }

        public static implicit operator VarValue(float v)
        {
            return new VarValue(v);
        }

        public static explicit operator float(VarValue v)
        {
            if (EType.Float != v.mEType)
            {
                throw new System.NotSupportedException(v.ToString() + "convert to float not supported.");
            }

            return (float)v.mValue;
        }

        public static implicit operator VarValue(Vector3 v)
        {
            return new VarValue(v);
        }

        public static explicit operator Vector3(VarValue v)
        {
            if (EType.Vector3 != v.mEType)
            {
                throw new System.NotSupportedException(v.ToString() + "convert to Vector3 not supported.");
            }

            return (Vector3)v.mValue;
        }

        public static implicit operator VarValue(string v)
        {
            return new VarValue(v);
        }

        public static explicit operator string(VarValue v)
        {
            if (EType.String != v.mEType)
            {
                throw new System.NotSupportedException(v.ToString() + "convert to string not supported.");
            }

            return (string)v.mValue;
        }
        #endregion
    }

    [System.Serializable]
    public class VariableMgr
    {
        Dictionary<string, Variable> mDicVar;

        public Variable GetVar(string varName)
        {
            if (null == mDicVar)
            {
                return null;
            }

            if (mDicVar.ContainsKey(varName))
            {
                return mDicVar[varName];
            }

            return null;
        }

        public bool AddVar(string varName, Variable var)
        {
            if (null == var)
            {
                Debug.LogError("add null variable to manager");
                return false;
            }

            if (null == mDicVar)
            {
                mDicVar = new Dictionary<string, Variable>(5);
            }

            if (mDicVar.ContainsKey(varName))
            {
                Debug.LogError("variable [" + varName + "] exist");
                return false;
            }

            mDicVar.Add(varName, var);
            return true;
        }

        public static byte[] Export(VariableMgr mgr)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter b = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            b.Serialize(stream, mgr);

            return stream.ToArray();
        }

        public static VariableMgr Import(byte[] data)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream(data, false);
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter b = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            VariableMgr mgr = b.Deserialize(stream) as VariableMgr;
            return mgr;
        }
    }

    [System.Serializable]
    public class Variable
    {
        public enum EScope
        {
            Gloabel,
            Script,
            Trigger,
            Max
        }

        VarValue mValue;

        public Variable(VarValue v)
        {
            mValue = v;
        }

        public Variable()
        {
            mValue = null;
        }

        public VarValue Value
        {
            get
            {
                if (null == mValue)
                {
                    mValue = new VarValue();
                }

                return mValue;
            }

            set
            {
                mValue = value;
            }
        }

        public override string ToString()
        {
            return string.Format("Variable:{0}", Value);
        }
    }

    public class VarRef
    {
        string mVarName;
        Trigger mTrigger;
        Variable mVariable;

        //variable reference
        public VarRef(string varName, Trigger trigger)
        {
            mVarName = varName;
            mTrigger = trigger;
        }

        public VarRef(VarValue v)
        {
            mVarName = null;
            mTrigger = null;

            Value = v;
        }

        //public void SetTrigger(Trigger trigger)
        //{
        //    mTrigger = trigger;
        //}

        public VarValue Value
        {
            get
            {
                return Var.Value;
            }

            private set
            {
                if (null == mVariable)
                {
                    Var = new Variable(value);
                }
                else
                {
                    Var.Value = value;
                }
            }
        }

        public Variable Var
        {
            get
            {
                if (null == mVariable)
                {
                    if (null == mTrigger)
                    {
                        throw new System.Exception("no trigger set");
                    }

                    mVariable = mTrigger.GetVar(mVarName);

                    if (null == mVariable)
                    {
                        throw new System.Exception("no variable found with name:" + mVarName);
                    }
                }

                return mVariable;
            }
            private set
            {
                mVariable = value;
            }
        }

        public string Name
        {
            get
            {
                return mVarName;
            }
        }

        //public static implicit operator VarValue(Variable v)
        //{
        //    return v.Value;
        //}

        public override string ToString()
        {
            return string.Format("Variable[{0}:{1}]", Name, Value);
        }

        public static VarRef Deserialize(Stream stream, Trigger trigger)
        {
            using (BinaryReader r = new BinaryReader(stream))
            {
                if (r.ReadBoolean())
                {
                    BinaryFormatter b = new BinaryFormatter();
                    VarValue varValue = b.Deserialize(stream) as VarValue;
                    return new VarRef(varValue);
                }
                else
                {
                    string name = r.ReadString();

                    return new VarRef(name, trigger);
                }
            }
        }

        public static bool Serialize(Stream stream, VarRef varRef)
        {
            using (BinaryWriter w = new BinaryWriter(stream))
            {
                if (string.IsNullOrEmpty(varRef.Name))
                {
                    w.Write(true);

                    BinaryFormatter b = new BinaryFormatter();
                    b.Serialize(stream, varRef.Value);
                }
                else
                {
                    w.Write(false);
                    w.Write(varRef.Name);
                }
                return true;
            }
        }
    }
}