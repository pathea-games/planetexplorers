using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pathea
{
    public class NameGenerater
    {
        class LastName
        {
            public string mText;
            public int mRace;
            public int mLabel;
        }

        class FirstName
        {
            public int mSex;
            public string mText;
            public int mLabel;
        }

        
        List<CharacterName> mList = new List<CharacterName>(10);

        public NameGenerater()
        {
            LoadFirstName();

            LoadLastName();
        }

        #region Static
        static List<LastName> sLastNamePool;
        static List<FirstName> sFirstNamePool;

        static List<LastName> GetLastName(int race, int label)
        {
            List<LastName> listName = new List<LastName>(10);
            foreach (LastName ite in sLastNamePool)
            {
                if (ite.mRace == race && ite.mLabel == label)
                {
                    listName.Add(ite);
                }
            }

            return listName;
        }

        static List<FirstName> GetFirstName(PeSex sex)
        {
            List<FirstName> listName = new List<FirstName>(10);
            foreach (FirstName ite in sFirstNamePool)
            {
                if (PeGender.Convert(ite.mSex + 1) == sex)
                {
                    listName.Add(ite);
                }
            }

            return listName;
        }

        static void Shuffle<T>(List<T> myList)
        {
            for (int i = 0; i < myList.Count; i++)
            {
                int index = Random.Range(0, myList.Count);

                if (index != i)
                {
                    T temp = myList[i];
                    myList[i] = myList[index];
                    myList[index] = temp;
                }
            }
        }

        static CharacterName GetRandomNpcName(PeSex sex, int race, List<CharacterName> exclude)
        {
            List<FirstName> firstNameArray = GetFirstName(sex);
            if (firstNameArray.Count <= 0)
            {
                Debug.LogError("random first name exhausted for sex:" + sex);
                return null;
            }

            Shuffle(firstNameArray);

            foreach (FirstName fn in firstNameArray)
            {
                List<LastName> lastNameArray = GetLastName(race, fn.mLabel);
                if (lastNameArray.Count <= 0)
                {
                    Debug.LogWarning("no last name can be used by race[" + race + "], which matchs the first name [" + fn.mText+"], label:"+fn.mLabel);
                    continue;
                }

                Shuffle(lastNameArray);

                foreach (LastName ln in lastNameArray)
                {
                    CharacterName characterName = new CharacterName(fn.mText, ln.mText);

                    if (!exclude.Exists(delegate(CharacterName name)
                    {
                        if (name.Equals(characterName))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }))
                    {
                        return characterName;
                    }
                }
            }

            Debug.LogError("random name exhausted for sex:"+sex+" race:"+race);
            return null;
        }

        static void LoadLastName()
        {
            if (null != sLastNamePool)
            {
                return;
            }

            sLastNamePool = new List<LastName>(20);

            Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Randname1");
            while (reader.Read())
            {
                LastName rand = new LastName();
                //rand.m_ID = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
                rand.mText = reader.GetString(reader.GetOrdinal("Fname"));
                rand.mRace = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("humantype")));
                rand.mLabel = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("recognize")));

                sLastNamePool.Add(rand);
            }
        }

        static void LoadFirstName()
        {
            if (null != sFirstNamePool)
            {
                return;
            }

            sFirstNamePool = new List<FirstName>(20);

            Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Randname2");
            while (reader.Read())
            {
                FirstName rand = new FirstName();
                //rand.m_ID = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
                rand.mSex = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("sex")));
                rand.mText = reader.GetString(reader.GetOrdinal("Rname"));
                rand.mLabel = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("recognize")));

                sFirstNamePool.Add(rand);
            }
        }
        #endregion

        public CharacterName Fetch(PeSex sex, int race)
        {
            CharacterName characterName = GetRandomNpcName(sex, race, mList);
            if(null == characterName)
            {
                Debug.LogWarning("no random name, return default:" + CharacterName.Default.ToString());
                return CharacterName.Default;
            }

            mList.Add(characterName);

            return characterName;
        }

        public byte[] Export()
        {
            return PETools.Serialize.Export((w) =>
            {
                w.Write(mList.Count);
                foreach (CharacterName item in mList)
                {
                    PETools.Serialize.WriteBytes(item.Export(), w);
                }
            });
        }

        public void Import(byte[] buffer)
        {
            PETools.Serialize.Import(buffer, (r) =>
            {
                int count = r.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    byte[] data = PETools.Serialize.ReadBytes(r);

                    CharacterName characterName = new CharacterName();
                    characterName.Import(data);
                    mList.Add(characterName);
                }
            });
        }
    }
}