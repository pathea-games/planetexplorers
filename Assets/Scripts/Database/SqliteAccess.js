//import System.Data;  // here use ver2.0.5
import Mono.Data.SqliteClient; // here use ver2.0.0

class SqliteAccess {
	// variables for basic query access
	private var connection : String;
	private var dbcon : SqliteConnection;
	private var dbcmd : SqliteCommand;
	private var reader : SqliteDataReader;

	function SqliteAccess(p : String){
		OpenDB(p);
	}
	
	function OpenDB(p : String){
		connection = "URI=file:" + p; // we set the connection to our database
		dbcon = new SqliteConnection(connection);
		dbcon.Open();
	}
	
	// Run a Sqlite query
	function ExecuteQuery(q : String){
		dbcmd = dbcon.CreateCommand(); // create empty command
		dbcmd.CommandText = q; // fill the command
		reader = dbcmd.ExecuteReader(); // execute command which returns a reader
		return reader; // return the reader
	}
	
	// Get query result from sqlQuery
	function GetQueryResult()
	{
		var readArray : Array = new Array();
		while(reader.Read()){
			var lineArray : Array = new Array();
			for (var i : int = 0; i < reader.FieldCount; i++)
				lineArray.Add(reader.GetValue(i));
			readArray.Add(lineArray);
		}
		return readArray;
	}
	function GetQueryResultSingle()
	{
		var readArray : Array = new Array();
		while(reader.Read()){
			readArray.Add(reader.GetValue(0));
		}
		return readArray;
	}
	
	// This returns a 2 dimensional ArrayList with all the
	//  data from the table requested
	function ReadFullTable(tableName : String){
		var query : String;
		query = "SELECT * FROM " + tableName;   
		dbcmd = dbcon.CreateCommand();
		dbcmd.CommandText = query; 
		reader = dbcmd.ExecuteReader();
		return reader;
	}
	
	// This function deletes all the data in the given table.  Forever.  WATCH OUT! Use sparingly, if at all
	function DeleteTableContents(tableName : String){
		var query : String;
		query = "DELETE FROM " + tableName;
		dbcmd = dbcon.CreateCommand();
		dbcmd.CommandText = query; 
		reader = dbcmd.ExecuteReader();
	}
	
	// Create a table, name, column array, column type array
	function CreateTable(name : String, col : Array, colType : Array){
		if (col.Length != colType.Length) {
			throw new SqliteSyntaxException("columns.Length != colType.Length");
		}
		
		var query : String;
		query  = "CREATE TABLE " + name + "(" + col[0] + " " + colType[0];
		for(var i=1; i<col.length; i++){
			query += ", " + col[i] + " " + colType[i];
		}
		query += ")";
		dbcmd = dbcon.CreateCommand(); // create empty command
		dbcmd.CommandText = query; // fill the command
		reader = dbcmd.ExecuteReader(); // execute command which returns a reader	
	}
	
	// basic Insert with just values
	function InsertInto(tableName : String, values : Array){
		var query : String;
		query = "INSERT INTO " + tableName + " VALUES (" + values[0];
		for(i=1; i<values.length; i++){
			query += ", " + values[i];
		}
		query += ")";
		dbcmd = dbcon.CreateCommand();
		dbcmd.CommandText = query; 
		reader = dbcmd.ExecuteReader(); 
	}
	
	function InsertIntoSpecific(tableName : String, cols : Array, values : Array){ // Specific insert with col and values
		if (cols.Length != values.Length) {
			throw new SqliteSyntaxException ("columns.Length != values.Length");
		}
		
		var query : String;
		query = "INSERT INTO " + tableName + "(" + cols[0];
		for(i=1; i<cols.length; i++){
			query += ", " + cols[i];
		}
		query += ") VALUES (" + values[0];
		for(i=1; i<values.length; i++){
			query += ", " + values[i];
		}
		query += ")";
		dbcmd = dbcon.CreateCommand();
		dbcmd.CommandText = query; 
		reader = dbcmd.ExecuteReader();
	}
	function InsertIntoSpecificSingle(tableName : String, col : String, values : String){
		var query : String;
		query = "INSERT INTO " + tableName + "(" + col + ")" + " VALUES (" + values + ")";
		dbcmd = dbcon.CreateCommand();
		dbcmd.CommandText = query; 
		reader = dbcmd.ExecuteReader(); 
	}
	
	// Selects from table with specified parameters.
	// Ex: SelectWhere("puppies", new string[] = {"breed"}, new string[] = {"earType"}, new string[] = {"="}, new string[] = {"floppy"});
	// the same as command: SELECT breed FROM puppies WHERE earType = floppy
	function SelectWhere (tableName : String, itemSToSelect : Array, wCols : Array, wOpers : Array, wValues : Array)
	{
		if (wCols.Length != wOpers.Length || wOpers.Length != wValues.Length) {
			throw new SqliteSyntaxException ("col.Length != operation.Length != values.Length");
		}
		var query;
		query = "SELECT " + itemSToSelect[0];
		for (i = 1; i < itemSToSelect.Length; ++i) {
			query += ", " + itemSToSelect[i];
		}
		query += " FROM " + tableName + " WHERE " + wCols[0] + wOpers[0] + "'" + wValues[0] + "' ";
		for (i = 1; i < wCols.Length; ++i) {
			query += " AND " + wCols[i] + wOpers[i] + "'" + wValues[0] + "' ";
		}
		
		dbcmd = dbcon.CreateCommand();
		dbcmd.CommandText = query; 
		reader = dbcmd.ExecuteReader();
		return reader;
	}
	// This function reads a single column
	//  wCol is the WHERE column, 
	//  wPar is the operator you want to use to compare with, 
	//  wValue is the value you want to compare against.
	//  Example: SingleSelectWhere(tableName, "firstName", "lastName","=","'Sagat'")
	//  returns reader: SELECT firstName FROM tableName WHERE lastName = "Sagat";
	function SelectWhereSingle(tableName : String, itemToSelect : String, wCol : String, wOper : String, wValue : String){ // Selects a single Item
		var query : String;
		query = "SELECT " + itemToSelect + " FROM " + tableName + " WHERE " + wCol + wOper + wValue; 
		dbcmd = dbcon.CreateCommand();
		dbcmd.CommandText = query; 
		reader = dbcmd.ExecuteReader();
		return reader;
	}
	
	function CloseDB(){
		reader.Close(); // clean everything up
		reader = null; 
		dbcmd.Dispose(); 
		dbcmd = null; 
		dbcon.Close(); 
		dbcon = null; 
	}
}
