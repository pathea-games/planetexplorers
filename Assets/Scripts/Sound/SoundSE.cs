using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Random = UnityEngine.Random;

namespace SoundAsset
{
	public class SESoundStory
	{
		public int type;
		public int colorR;
		public string soundInfoStr;

		public Dictionary<int, float> sounds;

		public static List<SESoundStory> s_tblSeSoundInfo;

		public static int GetRandomAudioClip(int type, float colorR)
		{
			SESoundStory story = s_tblSeSoundInfo.Find(ret => (ret.type == type && Mathf.Abs(ret.colorR - colorR * 255) < 2.0f));

			if(story == null)
				return -1;
			else
				return story.GetRandomAudioClip();
		}
		
		public static void LoadData()
		{
			s_tblSeSoundInfo = new List<SESoundStory>();

			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("soundspawn");
			while(reader.Read()){
				SESoundStory se = new SESoundStory();

				se.type = Convert.ToInt32(reader.GetString(reader.GetOrdinal("type")));
				se.colorR = Convert.ToInt32(reader.GetString(reader.GetOrdinal("num")));
				se.soundInfoStr = reader.GetString(reader.GetOrdinal("soundInfo"));

				se.StringToSounds();
				
				s_tblSeSoundInfo.Add(se);
			}
		}

		void StringToSounds()
		{
			sounds = new Dictionary<int, float>();
			
			string[] speciesData = soundInfoStr.Split(new char[] { ';' });
			foreach (string ite in speciesData)
			{
				string[] s = ite.Split(new char[] { ',' });
				if (s.Length != 2) continue;
				
				int sid = Convert.ToInt32(s[0]);
				float percent = Convert.ToSingle(s[1]);
				
				if(!sounds.ContainsKey(sid))
				{
					sounds.Add(sid, percent);
				}
			}
		}

		int GetRandomAudioClip()
		{
			int _id = -1;
			float _value = 0.0f;
			float _ranValue = Random.value;
			foreach (KeyValuePair<int, float> ite in sounds)
			{
				_value += ite.Value;
				if (_ranValue <= _value)
				{
					_id = ite.Key;
					break;
				}
			}
			
			return _id;
		}
	}

	public class SESoundBuff 
	{
        const string SoundPath = "Sound/";

		public int mID;
		public string mName;
        public bool mLoop;
        public int mAudioType;
        public float mDoppler;
        public float mSpatial;
        public float mVolume;
        public float mMinDistance;
        public float mMaxDistance;
        public AudioRolloffMode mMode = AudioRolloffMode.Linear;

        public static List<SESoundBuff> s_tblSeSoundBuffs;

		public static void LoadData()
		{
			s_tblSeSoundBuffs = new List<SESoundBuff>();
			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("sound");
			while(reader.Read()){
				SESoundBuff se = new SESoundBuff();
				se.mID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_id")));
	            se.mName = Convert.ToString(reader.GetString(reader.GetOrdinal("_name")));
                se.mLoop = Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("loop")));
                se.mAudioType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("soundType")));
                se.mMode = (AudioRolloffMode)Convert.ToInt32(reader.GetString(reader.GetOrdinal("rolloffType")));
                se.mDoppler = Convert.ToSingle(reader.GetString(reader.GetOrdinal("doppler")));
                se.mSpatial = Convert.ToSingle(reader.GetString(reader.GetOrdinal("spatial")));
                se.mVolume = Convert.ToSingle(reader.GetString(reader.GetOrdinal("volume")));
                se.mMinDistance = Convert.ToSingle(reader.GetString(reader.GetOrdinal("minDistance")));
                se.mMaxDistance = Convert.ToSingle(reader.GetString(reader.GetOrdinal("maxDistance")));

                se.mVolume = Mathf.Clamp01(se.mVolume);
				s_tblSeSoundBuffs.Add(se);
			}
		}

		public static bool MatchId(SESoundBuff iter, int id){
			return iter.mID == id;
		}

        public static SESoundBuff GetSESoundData(int id)
        {
            return s_tblSeSoundBuffs.Find(ret => MatchId(ret, id));
        }
	}
}
