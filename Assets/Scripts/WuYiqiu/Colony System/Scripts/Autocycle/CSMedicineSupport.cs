//------------------------------------------------------------------------------
//2016-9-13 19:56:19
//by Pugee
//------------------------------------------------------------------------------
using System;
using Mono.Data.SqliteClient;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class MedicineSupply{
	public int id;
	public int protoId;
	public int count;
	public float rounds;
}
public class CSMedicineSupport
{
	static List<MedicineSupply> medicineData = new List<MedicineSupply>();

	public static void LoadData(){
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AbnormalDruggery");
		
		while (reader.Read())
		{
			MedicineSupply ms = new MedicineSupply();
			ms.id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
			ms.protoId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("prototypeitem_id")));
			ms.count= Convert.ToInt32(reader.GetString(reader.GetOrdinal("count")));
			ms.rounds = Convert.ToSingle(reader.GetString(reader.GetOrdinal("rounds")));

			medicineData.Add(ms);
		}
	}

	public static List<MedicineSupply> AllMedicine{
		get{return medicineData;}
	}
}

