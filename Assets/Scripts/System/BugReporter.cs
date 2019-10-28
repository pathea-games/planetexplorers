// Note : use .Net2.0 subset would cause HELO, so we use .Net2.0(full ver)
//#define TstBugReport
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Collections;

public class BugReporter
{
	private static bool _isAlreadyRun = false;
	private static int _maxRetryCount = 5;
	private static int _currentRetryCount = 0;
	private static MailMessage _mailMessage = null; // to dispose
	private static FileStream _fsAttachment = null;
	private static Action _onFinSend = null;
	public static bool IsSending{ get { return _isAlreadyRun; } }
	
	public static void SendEmailAsync(string msg, int retryCount, Action onFinSend = null) {
		
		if (_isAlreadyRun) {
			Debug.LogError("EmailSender doesn't support multiple concurrent invocations.");
			return;
		}
		string subject = "Bug report";
#if !TstBugReport
	#if DemoVersion
		subject += " Demo";
	#endif
	#if UNITY_STANDALONE_LINUX
		subject += " Linux";
	#elif UNITY_STANDALONE_OSX
		subject += " Osx";
	#elif Win32Ver
		subject += " Win32";
	#else
		subject += " Win64";
	#endif
	#if SteamVersion
		subject += " Steam Version:" + GameConfig.GameVersion;
		try{
			Steamworks.CSteamID steamId = SteamMgr.steamId;
			subject += " from SteamId " + steamId.m_SteamID;
		} catch {
			subject += " from Unrecognized SteamId";
		}
	#else
		subject += " Pathea Version:" + GameConfig.GameVersion;
	#endif
#endif
		
		_isAlreadyRun = true;
		_onFinSend = onFinSend;
		_maxRetryCount = retryCount;
		
		_mailMessage = new MailMessage();		
		_mailMessage.From = new MailAddress("pe.bugreport@pathea.net");
		_mailMessage.To.Add("pe.bugreport@pathea.net");
		_mailMessage.Subject = subject;
		_mailMessage.Body = msg;
		_mailMessage.IsBodyHtml = false;
		_mailMessage.Priority = MailPriority.High;
#if !UNITY_EDITOR
		string userPath = Environment.GetFolderPath (Environment.SpecialFolder.Desktop)+"/..";//Environment.SpecialFolder.UserProfile
		try{
#if UNITY_STANDALONE_LINUX
			_fsAttachment = new FileStream (userPath+"/.config/unity3d/Pathea Games/Planet Explorers/Player.log", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
#elif UNITY_STANDALONE_OSX
			_fsAttachment = new FileStream (userPath+"/Library/Logs/Unity/Player.log", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
#else
			//UNITY_STANDALONE_WIN
			_fsAttachment = new FileStream (Application.dataPath + "/output_log.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
#endif
			_mailMessage.Attachments.Add (new Attachment (_fsAttachment, "output_log.txt"));	
		} catch(Exception exception){
			Debug.Log("Failed to attach:"+exception);
		}
#endif

        SmtpClient smtpServer = new SmtpClient("pathea.net");
		smtpServer.Port = 587; // 25 //for mx without Credentials & ssl(use nslookup can get name of domain mx)
		smtpServer.Credentials = new System.Net.NetworkCredential("pe.bugreport@pathea.net", "mailbox4pebugreport")as ICredentialsByHost;
		smtpServer.EnableSsl = true;
		ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
		//delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
		
		smtpServer.SendCompleted += SmtpClientSendCompleted;
		SendEmail (smtpServer);
	}
	private static void SendEmail(SmtpClient smtpServer) {
		try {
			if(_maxRetryCount > 0){
				smtpServer.SendAsync(_mailMessage, Guid.NewGuid());
			} else {
				smtpServer.Send(_mailMessage);
				EndProcessing(smtpServer);
				Debug.Log("Succeed");
			}
		} catch (Exception exception) {
			EndProcessing(smtpServer);
			Debug.Log("Failed:"+exception);
		}
	}
	private static void EndProcessing (SmtpClient client) {
		
		if (_mailMessage != null) {
			_mailMessage.Dispose();
		}
		
		if (_fsAttachment != null) {
			_fsAttachment.Dispose();
		}
		
		if (client != null) {
			client.SendCompleted -= SmtpClientSendCompleted;
		}

		if (_onFinSend != null) {
			_onFinSend();
		}
		
		_isAlreadyRun = false;
		_currentRetryCount = 0;	
	}
	
	private static void SmtpClientSendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
		var smtpClient = (SmtpClient)sender;
		
		if(e.Error == null || _currentRetryCount >= _maxRetryCount) {
			EndProcessing(smtpClient);
		} else {
			_currentRetryCount++;
			SendEmail(smtpClient);
		}
	}
}

