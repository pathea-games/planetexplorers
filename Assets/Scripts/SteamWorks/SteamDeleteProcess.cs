using UnityEngine;
using System;
using System.Collections;
using Steamworks;
using System.Collections.Generic;
//steamdeletefileprocess
public class SteamDeleteProcess :ISteamGetFile
{

	public  SteamDeleteProcess(DeleteFileResultEventHandler callBackDeleteFileResult,string fileName,PublishedFileId_t publishID)
	{
		CallBackDeleteFileResult = callBackDeleteFileResult;
		DeleteFile( fileName,publishID);
	}

	DeleteFileResultEventHandler CallBackDeleteFileResult;
	void Finish(string fileName,PublishedFileId_t publishID,bool bOK)
	{

		if(CallBackDeleteFileResult != null)
			CallBackDeleteFileResult(fileName,publishID,bOK);
		ProcessList.Remove (this);

	}
	public void DeleteFile( string fileName,PublishedFileId_t publishID)
	{//DeleteFile step1
		try
		{
			if( (fileName == null || fileName.Length ==0) && publishID.m_PublishedFileId == 0)
			{
				Finish(fileName,publishID,false);
				return;
			}
			if ( fileName != null && fileName.Length !=0)
			{
				string preFileName = fileName + "_preview";
				//string cfgFileName = fileName + "_cfg";
				bool ok = SteamRemoteStorage.FileDelete (fileName);
				Debug.Log("--------------------------------------------------------------------" + ok);
				ok = SteamRemoteStorage.FileDelete (preFileName);
				Debug.Log(",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,," + ok);
			}
			if (publishID.m_PublishedFileId != 0)
			{
				SteamRemoteStorage.DeletePublishedFile ( publishID );
			}
			//SteamRemoteStorage.FileDelete (cfgFileName);


			Finish(fileName,publishID,true);

		}
		catch ( Exception e)
		{

			Finish(fileName,publishID,false);

			Debug.Log( "SteamDeleteProcess DeleteFile " + e.ToString() );

        }
	}
}

