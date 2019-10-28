using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//you just need to copy the "Mono.Data.dll","Mono.Data.Sqlite.dll" 
//and "Mono.Data.SqliteClient.dll" files from "C:Program FilesUnityEditorDataMonolibmono2.0"

//using System.Data;	// A reference of System.Data is needed to add
using Mono.Data.SqliteClient;

public class SqliteAccessCS
{
	private SqliteConnection dbConnection;
	private SqliteCommand dbCommand;
	private SqliteDataReader reader;


	/// <summary>
	///  string to connect. The simpliest one looks like "URI=file:filename.db"
	/// </summary>
	public SqliteAccessCS (string connectionString)
	{
		OpenDB (connectionString);
		//throw new Exception ("Test");
	}

	/// <summary>
	///  The same as <see cref="DbAccess#Dbaccess" / >
	/// </summary>
	public void OpenDB (string connectionString)
	{
		dbConnection = new SqliteConnection ("URI=file:" + connectionString);
		dbConnection.Open ();
		Debug.Log ("Connected to db");
	}

	/// <summary>
	///  Executes query given by sqlQuery
	/// </summary>
	public SqliteDataReader ExecuteQuery (string sqlQuery)
	{
		dbCommand = (SqliteCommand)(dbConnection.CreateCommand ());
		dbCommand.CommandText = sqlQuery;
		
		reader = dbCommand.ExecuteReader ();
		return reader;
	}
	
	/// <summary>
	///  Get query result from sqlQuery
	/// </summary>
	public ArrayList GetQueryResult ()
	{
		ArrayList readArray = new ArrayList();
		while(reader.Read()){
			ArrayList lineArray = new ArrayList();
			for (int i = 0; i < reader.FieldCount; i++)
				lineArray.Add(reader.GetValue(i));
			readArray.Add(lineArray);
		}
		return readArray;
	}
	public ArrayList GetQueryResultSingle ()
	{
		ArrayList readArray = new ArrayList();
		while(reader.Read()){
			readArray.Add(reader.GetValue(0));
		}
		return readArray;
	}
	
	/// <summary>
	///  Selects everything from table
	/// </summary>
	public SqliteDataReader ReadFullTable (string tableName)
	{
		string query = "SELECT * FROM " + tableName;
		return ExecuteQuery (query);
	}

	// This function deletes all the data in the given table.  Forever.  WATCH OUT! Use sparingly, if at all
	public SqliteDataReader DeleteTableContents (string tableName)
	{
		string query = "DELETE FROM " + tableName;
		return ExecuteQuery (query);
	}

	// Create a table, name, column array, column type array	
	public SqliteDataReader CreateTable (string name, string[] col, string[] colType)
	{
		if (col.Length != colType.Length) {
			throw new SqliteSyntaxException ("columns.Length != colType.Length");
		}
		string query = "CREATE TABLE " + name + " (" + col[0] + " " + colType[0];
		for (int i = 1; i < col.Length; ++i) {
			query += ", " + col[i] + " " + colType[i];
		}
		query += ")";
		return ExecuteQuery (query);
	}

	/// <summary>
	/// Inserts data into table 
	/// </summary>
	public SqliteDataReader InsertInto (string tableName, string[] values)
	{
        string query = "INSERT INTO " + tableName + " VALUES (" + '"' + values[0] + '"';
        for (int i = 1; i < values.Length; ++i)
        {
            query += ", " + '"'+values[i]+'"';
        }
        query += ")";
		return ExecuteQuery (query);
	}
	public SqliteDataReader InsertIntoSpecific (string tableName, string[] cols, string[] values)
	{
		if (cols.Length != values.Length) {
			throw new SqliteSyntaxException ("columns.Length != values.Length");
		}
		string query = "INSERT INTO " + tableName + "(" + cols[0];
		for (int i = 1; i < cols.Length; ++i) {
			query += ", " + cols[i];
		}
		query += ") VALUES (" + values[0];
		for (int i = 1; i < values.Length; ++i) {
			query += ", " + values[i];
		}
		query += ")";
		return ExecuteQuery (query);
	}
	public SqliteDataReader InsertIntoSpecificSingle (string tableName, string col, string values)
	{
		string query = "INSERT INTO " + tableName + "(" + col + ")" + " VALUES (" + values + ")";
		return ExecuteQuery (query);
	}

	/// <summary>
	/// Selects from table with specified parameters.
	/// Ex: SelectWhere("puppies", new string[] = {"breed"}, new string[] = {"earType"}, new string[] = {"="}, new string[] = {"floppy"});
	/// the same as command: SELECT breed FROM puppies WHERE earType = floppy
	/// </summary>
	public SqliteDataReader SelectWhere (string tableName, string[] items, string[] cols, string[] operation, string[] values)
	{
		if (cols.Length != operation.Length || operation.Length != values.Length) {
			throw new SqliteSyntaxException ("col.Length != operation.Length != values.Length");
		}
		string query = "SELECT " + '"'+ items[0]+'"';
		for (int i = 1; i < items.Length; ++i) {
            query += ", " + '"' + items[i]+'"';  
		}
		query += " FROM " + tableName + " WHERE " + cols[0] + operation[0] + '"' + values[0] + '"';
		for (int i = 1; i < cols.Length; ++i) {
			query += " AND " + cols[i] + operation[i] + '"' + values[0] +'"';
		}
		return ExecuteQuery (query);
	}
	public SqliteDataReader SelectWhereSingle(string tableName, string item, string col, string operation, string values){ // Selects a single Item
		string query = "SELECT " + item + " FROM " + tableName + " WHERE " + col + operation + values; 
		return ExecuteQuery (query);
	}
	
	/// <summary>
	/// Closes connection to db
	/// </summary>
	public void CloseDB ()
	{
		if (dbCommand != null) {
			dbCommand.Dispose ();
		}
		dbCommand = null;
		if (reader != null) {
			reader.Dispose ();
		}
		reader = null;
		if (dbConnection != null) {
			dbConnection.Close ();
		}
		dbConnection = null;
		Debug.Log ("Disconnected from db.");
	}		
}
