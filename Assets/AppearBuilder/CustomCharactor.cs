using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomCharactor
{	
	public enum ESex
	{
		Male = 0,
		Female,
		Max
	}

	public class CustomData
	{
		public static CustomData DefaultMale()
        {
            CustomData data = new CustomData();
            data.sex = ESex.Male;

            data.nudeAvatarData = new AvatarData();
            data.nudeAvatarData.SetMaleBody();

            data.appearData = new AppearBlendShape.AppearData();
            data.appearData.Default();

            data.charactorName = "PlayerName";
            return data;
        }

        public static CustomData DefaultFemale()
        {
            CustomData data = new CustomData();
            data.sex = ESex.Female;

            data.nudeAvatarData = new AvatarData();
            data.nudeAvatarData.SetFemaleBody();

            data.appearData = new AppearBlendShape.AppearData();
            data.appearData.Default();

            data.charactorName = "PlayerName";
            return data;
        }

        public static CustomData RandomMale(string excludeHead)
        {
            CustomData data = new CustomData();
            data.sex = ESex.Male;

            data.nudeAvatarData = new AvatarData();
            data.nudeAvatarData.RandSetMaleBody(excludeHead);

            data.appearData = new AppearBlendShape.AppearData();
            data.appearData.Default();

            data.charactorName = "PlayerName";
            return data;
        }

        public static CustomData RandomFemale(string excludeHead)
        {
            CustomData data = new CustomData();
            data.sex = ESex.Female;

            data.nudeAvatarData = new AvatarData();
            data.nudeAvatarData.RandSetFemaleBody(excludeHead);

            data.appearData = new AppearBlendShape.AppearData();
            data.appearData.Default();

            data.charactorName = "PlayerName";
            return data;
        }

		const int VERSION_0000 = 0;
        const int CURRENT_VERSION = VERSION_0000;

        public CustomData()
        {
            charactorName = "default";
            sex = ESex.Male;
        }

        public string charactorName
        {
            get;
            set;
        }

        public Texture2D headIcon
        {
            get;
            set;
        }

        public ESex sex
        {
            get;
            set;
        }

        public AvatarData nudeAvatarData
        {
            get;
            set;
        }

        public AppearBlendShape.AppearData appearData
        {
            get;
            set;
        }

        public byte[] Serialize()
        {
            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(200);
                using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms))
                {
                    bw.Write((int)CURRENT_VERSION);

                    bw.Write(charactorName);

                    if (null == headIcon)
                    {
                        PETools.Serialize.WriteBytes(null, bw);
                    }
                    else
                    {
                        PETools.Serialize.WriteBytes(headIcon.EncodeToPNG(), bw);
                    }

                    bw.Write((int)sex);

                    if (null == nudeAvatarData)
                    {
                        PETools.Serialize.WriteBytes(null, bw);
                    }
                    else
                    {
                        PETools.Serialize.WriteBytes(nudeAvatarData.Serialize(), bw);
                    }

                    if (null == appearData)
                    {
                        PETools.Serialize.WriteBytes(null, bw);
                    }
                    else
                    {
                        PETools.Serialize.WriteBytes(appearData.Serialize(), bw);
                    }
                }

                return ms.ToArray();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                return null;
            }
        }

        public bool Deserialize(byte[] data)
        {
            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(data, false);

                using (System.IO.BinaryReader br = new System.IO.BinaryReader(ms))
                {
                    int version = br.ReadInt32();

                    if (version > CURRENT_VERSION)
                    {
                        Debug.LogWarning("version:" + version + " greater than current version:" + CURRENT_VERSION);
                        return false;
                    }

                    charactorName = br.ReadString();
                    byte[] texBuff = PETools.Serialize.ReadBytes(br);
                    if (null != texBuff && texBuff.Length > 0)
                    {
                        headIcon = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                        headIcon.LoadImage(texBuff);
                        headIcon.Apply();
                    }

                    sex = (ESex)br.ReadInt32();

                    nudeAvatarData = new AvatarData();
                    nudeAvatarData.Deserialize(PETools.Serialize.ReadBytes(br));

                    appearData = new AppearBlendShape.AppearData();
                    appearData.Deserialize(PETools.Serialize.ReadBytes(br));

                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                return false;
            }

        }
    }

    public class AvatarData : IEnumerable<string>
    {
		const int VersionWithBaseModel = 150911;
        const int CURRENT_VERSION = 150911; //new Version

        public enum ESlot
        {
            HairF = 0,	//"HairF"
            HairT,	//"HairT"
            HairB,	//"HairB"
            Head,	//"Head"
            Torso,	//"Torso"
            Hands,	//"Hands"
            Legs,	//"Legs"
            Feet,	//"Feet"
            Max
		};
		const int c_nSlots = (int)ESlot.Max;
		public string _baseModel = string.Empty;
		string[] _avatarParts = new string[c_nSlots];
		
		public AvatarData(AvatarData avData = null){
			if (avData != null) {
				_baseModel = avData._baseModel;
				avData._avatarParts.CopyTo (_avatarParts, 0);
			}
		}

		public void SetPart(ESlot part, string info)
		{
			this[part] = info;
		}
		public string this[ESlot part] {  
			get {  
				return _avatarParts[(int)part];  
			}  
			set {   
				_avatarParts[(int)part] = value;  
			}  
		}  
		public override bool Equals (object obj)
		{
			AvatarData av = (AvatarData) obj;
			if (av._baseModel != _baseModel)
				return false;
			for (int i = 0; i < c_nSlots; i++){
				if(av._avatarParts[i] != _avatarParts[i])
					return false;
			}
			return true;
		}
		public override int GetHashCode ()
		{
			int hash = _baseModel.GetHashCode();
			for (int i = 0; i < c_nSlots; i++){
				if(!string.IsNullOrEmpty(_avatarParts[i]))
					hash += _avatarParts[i].GetHashCode();
			}
			return hash;
		}
#region IEnumerable<string>_IEnumerable
		public IEnumerator<string> GetEnumerator()  
		{  
			foreach (string s in _avatarParts)  
			{  
				yield return s;  
			}  
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()  
		{  
			return GetEnumerator();  
		}
#endregion  
		void SetBodyParts(CustomMetaData mdata, int idxHair, int idxHead)
		{
			_baseModel = mdata.baseModel;

			SetPart(ESlot.HairF, mdata.GetHair(idxHair).modelPath[0]);
			SetPart(ESlot.HairT, mdata.GetHair(idxHair).modelPath[1]);
			SetPart(ESlot.HairB, mdata.GetHair(idxHair).modelPath[2]);
			
			SetPart(ESlot.Head,	mdata.GetHead(idxHead).modelPath);
			
			SetPart(ESlot.Torso, mdata.torso);
			SetPart(ESlot.Legs,  mdata.legs);
			SetPart(ESlot.Hands, mdata.hands);
			SetPart(ESlot.Feet,  mdata.feet);
		}

		public void SetMaleBody(){			SetBodyParts (CustomMetaData.InstanceMale, 1, 0);       }
		public void SetFemaleBody(){		SetBodyParts (CustomMetaData.InstanceFemale, 1, 0);		}
        public void RandSetMaleBody(string excludeHead)
        {
            int hairIndex = UnityEngine.Random.Range(0, CustomMetaData.InstanceMale.GetHeadCount());
			while (true)
			{
				int headIndex = UnityEngine.Random.Range(0, CustomMetaData.InstanceMale.GetHeadCount());
				string path = CustomMetaData.InstanceMale.GetHead(headIndex).modelPath;
				if(path != excludeHead)
				{
					SetBodyParts (CustomMetaData.InstanceMale, hairIndex, headIndex);
					break;
				}
			}
        }
        public void RandSetFemaleBody(string excludeHead)
        {
			int hairIndex = UnityEngine.Random.Range(0, CustomMetaData.InstanceFemale.GetHeadCount());
 			while(true)
			{
				int headIndex = UnityEngine.Random.Range(0, CustomMetaData.InstanceFemale.GetHeadCount());
				string path = CustomMetaData.InstanceFemale.GetHead(headIndex).modelPath;
				if(path != excludeHead)
				{
					SetBodyParts (CustomMetaData.InstanceFemale, hairIndex, headIndex);
					break;
				}
			}
        }

		public bool IsInvalid()
		{
			return null == _avatarParts || string.IsNullOrEmpty(_avatarParts[3]) || "0" == _avatarParts[3];
		}

		public byte[] Serialize()
		{
			try
			{
				System.IO.MemoryStream ms = new System.IO.MemoryStream(100);
				using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms))
				{
					bw.Write((int)CURRENT_VERSION);
					bw.Write(0);	// cnt's placeholder

					bw.Write(_baseModel);

					int cnt = 0;
					int n = _avatarParts.Length;
					for(int i = 0; i < n; i++){
						if(!string.IsNullOrEmpty(_avatarParts[i])){
							bw.Write(i);
							bw.Write((string)_avatarParts[i]);
							cnt++;
						}
					}
					
					bw.Seek(sizeof(int), SeekOrigin.Begin);
					bw.Write(cnt);
				}
				
				return ms.ToArray();
			}
			catch (System.Exception e)
			{
				Debug.LogWarning(e);
				return null;
			}
		}
		public bool Deserialize(byte[] data)
		{
			try
			{
				System.IO.MemoryStream ms = new System.IO.MemoryStream(data, false);				
				using (System.IO.BinaryReader br = new System.IO.BinaryReader(ms))
				{
					int version = br.ReadInt32();
					int cnt = br.ReadInt32();
					if (version < VersionWithBaseModel) {
						bool bFemale = false;
						for (int i = 0; i < cnt; i++)
						{
							int part = br.ReadInt32();
							string info = br.ReadString();
                            if (part >= 0 && part < _avatarParts.Length)
                                _avatarParts[part] = info;
							bFemale |= info.ToLower().Contains("female");
						}
						_baseModel = bFemale ? CustomMetaData.InstanceFemale.baseModel : CustomMetaData.InstanceMale.baseModel;
					} else {
						_baseModel = br.ReadString();
						for (int i = 0; i < cnt; i++)
						{
							int part = br.ReadInt32();
							string info = br.ReadString();
                            if (part >= 0 && part < _avatarParts.Length)
                                _avatarParts[part] = info;
						}
					}					
					return true;
				}
			}
			catch (System.Exception e)
			{
				Debug.LogWarning("CustomData.Deserialize Error:"+e.Message);
				return false;
			}
		}

		public static IEnumerable<AvatarData.ESlot> GetSlot(int mask)
		{
			List<AvatarData.ESlot> list = new List<ESlot>(c_nSlots);
			
			if ((mask & (1 << 10)) != 0)                list.Add(AvatarData.ESlot.HairF);
			if ((mask & (1 << 0)) != 0)					list.Add(AvatarData.ESlot.HairT);
			if ((mask & (1 << 11)) != 0)                list.Add(AvatarData.ESlot.HairB);
			if ((mask & (1 << 12)) != 0)                list.Add(AvatarData.ESlot.Head);
			if ((mask & (1 << 1)) != 0)					list.Add(AvatarData.ESlot.Torso);
            if ((mask & (1 << 2)) != 0)					list.Add(AvatarData.ESlot.Hands);
            if ((mask & (1 << 6)) != 0)					list.Add(AvatarData.ESlot.Legs);
            if ((mask & (1 << 7)) != 0)					list.Add(AvatarData.ESlot.Feet);
            //if ((mask & (1 << 8)) != 0)					list.Add(AvatarData.ESlot.Back); //Back is deprecated
            return list;

        }

        public static IEnumerable<string> GetParts(AvatarData cur, AvatarData origin)
        {
            List<string> parts = new List<string>(10);

			for (int i = 0; i < c_nSlots; i++)
            {
                string temp = GetPart((ESlot)i, cur, origin);
                if (!string.IsNullOrEmpty(temp) && !parts.Contains(temp))
                {
                    parts.Add(temp);
                }
            }
			return parts;
        }
        static string GetPart(ESlot slot, AvatarData cur, AvatarData ori)
        {
			int i = (int)slot;
			return !string.IsNullOrEmpty(cur._avatarParts[i]) ? cur._avatarParts[i] : ori._avatarParts[i];
        }
		public static AvatarData Merge(AvatarData hPrior, AvatarData lPrior)
		{
			AvatarData ret = new AvatarData (lPrior);
			if(hPrior != null){
				for (int i = 0; i < c_nSlots; i++){
					if(!string.IsNullOrEmpty(hPrior._avatarParts[i])){
						ret._avatarParts[i] = hPrior._avatarParts[i];
					}
				}
			}
			return ret;
		}
    }
}