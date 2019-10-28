using UnityEngine;
using System.Collections;

namespace Pathea
{
	public class PEActionParam { }
	
	public class PEActionParamNV : PEActionParam
	{
		public int n;
		public Vector3 vec;
		
		PEActionParamNV(){}
		static PEActionParamNV gParam = new PEActionParamNV();
		public static PEActionParamNV param { get { return gParam; } }
	}

	public class PEActionParamNVB : PEActionParam
	{
		public int n;
		public Vector3 vec;
		public bool b;
		
		PEActionParamNVB(){}
		static PEActionParamNVB gParam = new PEActionParamNVB();
		public static PEActionParamNVB param { get { return gParam; } }
	}

	public class PEActionParamVVF : PEActionParam
	{
		public Vector3 vec1;
		public Vector3 vec2;
		public float f;

		PEActionParamVVF(){}
		static PEActionParamVVF gParam = new PEActionParamVVF();
		public static PEActionParamVVF param { get { return gParam; } }
	}

	public class PEActionParamVFNS : PEActionParam
	{
		public Vector3 vec;
		public float f;
		public int n;
		public string str;
		PEActionParamVFNS(){}
		static PEActionParamVFNS gParam = new PEActionParamVFNS();
		public static PEActionParamVFNS param { get { return gParam; } }
	}
		
	public class PEActionParamV : PEActionParam
	{
		public Vector3 vec;
		PEActionParamV(){}
		static PEActionParamV gParam = new PEActionParamV();
		public static PEActionParamV param { get { return gParam; } }
	}
	
	public class PEActionParamVVN : PEActionParam
	{
		public Vector3 vec1;
		public Vector3 vec2;
		public int n;
		PEActionParamVVN(){}
		static PEActionParamVVN gParam = new PEActionParamVVN();
		public static PEActionParamVVN param { get { return gParam; } }
	}
	
	public class PEActionParamVQNS : PEActionParam
	{
		public Vector3 vec;
		public Quaternion q;
		public int n;
		public string str;
		PEActionParamVQNS(){}
		static PEActionParamVQNS gParam = new PEActionParamVQNS();
		public static PEActionParamVQNS param { get { return gParam; } }
	}
	
	public class PEActionParamVQ : PEActionParam
	{
		public Vector3 vec;
		public Quaternion q;
		PEActionParamVQ(){}
		static PEActionParamVQ gParam = new PEActionParamVQ();
		public static PEActionParamVQ param { get { return gParam; } }
	}

	public class PEActionParamS : PEActionParam
	{
		public string str;
		PEActionParamS(){}
		static PEActionParamS gParam = new PEActionParamS();
		public static PEActionParamS param { get { return gParam; } }
	}

	public class PEActionParamVQS : PEActionParam
	{
		public Vector3 vec;
		public Quaternion q;
		public string str;
		PEActionParamVQS(){}
		static PEActionParamVQS gParam = new PEActionParamVQS();
		public static PEActionParamVQS param { get { return gParam; } }
	}

	public class PEActionParamVVNN : PEActionParam
	{
		public Vector3 vec1;
		public Vector3 vec2;
		public int n1;
		public int n2;
		PEActionParamVVNN(){}
		static PEActionParamVVNN gParam = new PEActionParamVVNN();
		public static PEActionParamVVNN param { get { return gParam; } }
	}

	public class PEActionParamN : PEActionParam
	{
		public int n;
		PEActionParamN(){}
		static PEActionParamN gParam = new PEActionParamN();
		public static PEActionParamN param { get { return gParam; } }
	}

	public class PEActionParamB : PEActionParam
	{
		public bool b;
		PEActionParamB(){}
		static PEActionParamB gParam = new PEActionParamB();
		public static PEActionParamB param { get { return gParam; } }
	}

	public class PEActionParamVQN : PEActionParam
	{
		public Vector3 vec;
		public Quaternion q;
		public int n;
		PEActionParamVQN(){}
		static PEActionParamVQN gParam = new PEActionParamVQN();
		public static PEActionParamVQN param { get { return gParam; } }
	}

	public class PEActionParamFVFS : PEActionParam
	{
		public float f1;
		public Vector3 vec;
		public float f2;
		public string str;
		PEActionParamFVFS(){}
		static PEActionParamFVFS gParam = new PEActionParamFVFS();
		public static PEActionParamFVFS param { get { return gParam; } }
	}
	
	public class PEActionParamVQSN : PEActionParam
	{
		public Vector3 vec;
		public Quaternion q;
		public string str;
		public int n;
		PEActionParamVQSN(){}
		static PEActionParamVQSN gParam = new PEActionParamVQSN();
		public static PEActionParamVQSN param { get { return gParam; } }
	}

	public class PEActionParamVBB : PEActionParam
	{
		public Vector3 vec;
		public bool b1;
		public bool b2;
		PEActionParamVBB(){}
		static PEActionParamVBB gParam = new PEActionParamVBB();
		public static PEActionParamVBB param { get { return gParam; } }
	}

	public class PEActionParamVFVFS : PEActionParam
	{
		public Vector3 vec1;
		public float f1;
		public Vector3 vec2;
		public float f2;
		public string str;
		PEActionParamVFVFS(){}
		static PEActionParamVFVFS gParam = new PEActionParamVFVFS();
		public static PEActionParamVFVFS param { get { return gParam; } }
	}

	public class PEActionParamDrive : PEActionParam
	{
		public WhiteCat.CarrierController controller;
		public int seatIndex;
		PEActionParamDrive(){}
		static PEActionParamDrive gParam = new PEActionParamDrive();
		public static PEActionParamDrive param { get { return gParam; } }
	}

    public class PEActionParamVQSNS : PEActionParam
    {
        public Vector3 vec;
        public Quaternion q;
        public string strAnima;
        public int enitytID;
        public string boneStr;
        PEActionParamVQSNS() { }
        static PEActionParamVQSNS gParam = new PEActionParamVQSNS();
        public static PEActionParamVQSNS param { get { return gParam; } }
    }
}