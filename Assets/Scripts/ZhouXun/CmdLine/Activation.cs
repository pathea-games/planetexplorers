using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using UnityEngine;

public class Activation
{
    private static Activation instance = null;
    public static Activation Instance
    {
        get
        {
            if (null == instance)
            {
                instance = new Activation();
            }
            return instance;
        }
    }

    public bool Activated { get; set; }

    private string peacRegisterName = "peac";

    public string UniqueIdentifier
    {
        get
        {
            return SystemInfo.deviceUniqueIdentifier;
        }
    }

    string GetActivationString()
    {
        string str1 = DESCrypto(UniqueIdentifier);
        if (string.IsNullOrEmpty(str1))
        {
			Debug.LogError("GetActivationstring return empty string.");
            return "";
        }

        return MD5Encoding(str1);
    }

    private DESCryptoServiceProvider desCryptoProvider = null;
    
    string DESCrypto(string plainText)  
    { 
        try
        {
            //加密  
            byte[] strs = System.Text.Encoding.Unicode.GetBytes(plainText);
 
            //DESCryptoServiceProvider desc = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();

            ICryptoTransform transform = desCryptoProvider.CreateEncryptor();//加密对象
            CryptoStream cStream = new CryptoStream(mStream, transform, CryptoStreamMode.Write);
            cStream.Write(strs, 0, strs.Length);
            cStream.FlushFinalBlock();
            return System.Convert.ToBase64String(mStream.ToArray());
        }
        catch (Exception)
        {
            Debug.Log("Crypto error");
            return "";
        }
    }
	
    string DESCryptoDe(string cipherText)  
    {
        try
        {
            //解密
            byte[] strs = System.Convert.FromBase64String(cipherText);

            // DESCryptoServiceProvider desc = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();

            ICryptoTransform transform = desCryptoProvider.CreateDecryptor();//解密对象

            CryptoStream cStream = new CryptoStream(mStream, transform, CryptoStreamMode.Write);
            cStream.Write(strs, 0, strs.Length);
            cStream.FlushFinalBlock();
            return System.Text.Encoding.Unicode.GetString(mStream.ToArray());
        }
        catch (Exception)
        {
            Debug.Log("CryptoDe error");
            return "";
        }
    }
    

    private Activation()
    {
        byte[] key = { 123, 223, 1, 81, 11, 243, 78, 16 };
        byte[] iv = { 120, 230, 10, 21, 10, 22, 31, 46 };

        desCryptoProvider = new DESCryptoServiceProvider();
        desCryptoProvider.Key = key;
        desCryptoProvider.IV = iv;
    }

    public string MD5Encoding(string rawPass)
    {
        try
        {
            MD5 md5 = MD5.Create();
            byte[] bs = Encoding.UTF8.GetBytes(rawPass);
            byte[] hs = md5.ComputeHash(bs);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hs)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
        catch(Exception ex)
        {
            Debug.Log(ex);
            return "";
        }
    }

    string GetRegisterKey()
    {
        try
        {
            RegistryKey softWareKey = Registry.CurrentUser.OpenSubKey(@"Software\Pathea");

            if (softWareKey != null)
            {
                object obj = softWareKey.GetValue(peacRegisterName);

                //Debug.Log("get register ["+itemName+":"+obj);

                return obj.ToString();
            }
			else
			{
				Debug.LogError("Get Stored Activate info failed.");
				return "";
			}
        }
        catch (System.Exception e)
        {
            //mainUI.strCommandLine = e.ToString();
            Debug.LogWarning(e);
            return "";
        }
    }

    bool WriteRegisterKey(string Value)
    {
        try
        {
            RegistryKey regWrite = Registry.CurrentUser.CreateSubKey(@"SoftWare\Pathea");

            regWrite.SetValue(peacRegisterName, Value);
			regWrite.Flush();
            regWrite.Close();
        }
        catch (System.Exception e)
        {
            Debug.Log("Write Key Failed.");
            Debug.LogWarning(e);
            //mainUI.strCommandLine = e.ToString();
            return false;
        }

        //Debug.Log("write register ["+itemName + ":" +Value);
        return true;
    }


    public void CheckActivatation()
    {
#if SteamVersion
        Activated = true;
#else
        Activated = false;
        string activationString = GetActivationString();

        if (string.IsNullOrEmpty(activationString))
        {
            return;
        }

        string peac = GetRegisterKey();
        if (string.IsNullOrEmpty(peac))
        {
            return;
        }

        if (activationString != peac)
        {
			Debug.Log("stored activate info not equal ");
            return;
        }

        Activated = true;
#endif
    }

    public bool Activate()
    {
        string activationString = GetActivationString();

        if (!string.IsNullOrEmpty(activationString))
        {
            WriteRegisterKey(activationString);
        }

        CheckActivatation();
        return Activated;
    }

    public void Deactivate()
    {
        WriteRegisterKey("");
        CheckActivatation();
    }
}