using UnityEngine;
/*using System.Collections;
using System;
using System.Data;
using System.Data.Odbc;*/
public class ExcelRead : MonoBehaviour {
	/*public const string DataBaseFile = "DataBase/localData";

	DataTable table = new DataTable();
	void Start () {
		ReadTable("F:\\item.xls");
	}
	
	void Update () {
	
	}
	
	void ReadTable(string path){
		string conn = "Driver={Microsoft Excel Driver (*.xls)}; DriverId=790; Dbq=" + path + ";";
		string commandLine = "SELECT * FROM 'item'";
		OdbcConnection ocon = new OdbcConnection (conn);
		OdbcCommand ocmd = new OdbcCommand(commandLine, ocon);
		ocon.Open();
		OdbcDataReader odr = ocmd.ExecuteReader();
		table.Load(odr);
		odr.Close();
		ocon.Close();
	}*/
}
/*
using UnityEngine;
using System.Collections;
using System; 
using System.Data; 
using System.Data.Odbc; 

public class ExcelRead : MonoBehaviour {
	DataTable dtYourData = new DataTable("YourData"); 
	string str = "";
	// Use this for initialization
	void Start () {
		
		readXLS(Application.dataPath + "/Book1.xls");
		str = ""+dtYourData.Rows[2][dtYourData.Columns[2].ColumnName].ToString();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnGUI(){
		GUILayout.Label(str);
	}
	void readXLS( string filetoread)
	{
		// Must be saved as excel 2003 workbook, not 2007, mono issue really
		string con = "Driver={Microsoft Excel Driver (*.xls)}; DriverId=790; Dbq="+filetoread+";";
		Debug.Log(con);
		string yourQuery = "SELECT * FROM [Sheet1$]"; 
		// our odbc connector 
		OdbcConnection oCon = new OdbcConnection(con); 
		// our command object 
		OdbcCommand oCmd = new OdbcCommand(yourQuery, oCon);
		// table to hold the data 
		
		// open the connection 
		oCon.Open(); 
		// lets use a datareader to fill that table! 
		OdbcDataReader rData = oCmd.ExecuteReader(); 
		// now lets blast that into the table by sheer man power! 
		dtYourData.Load(rData); 
		// close that reader! 
		rData.Close(); 
		// close your connection to the spreadsheet! 
		oCon.Close(); 
		// wow look at us go now! we are on a roll!!!!! 
		// lets now see if our table has the spreadsheet data in it, shall we? 

		if(dtYourData.Rows.Count > 0) 
		{ 
			// do something with the data here 
			// but how do I do this you ask??? good question! 
			for (int i = 0; i < dtYourData.Rows.Count; i++) 
			{ 
				// for giggles, lets see the column name then the data for that column! 
				Debug.Log(dtYourData.Columns[0].ColumnName + " : " + dtYourData.Rows[i][dtYourData.Columns[0].ColumnName].ToString()); 
			} 
		} 
	}
}
 */
	