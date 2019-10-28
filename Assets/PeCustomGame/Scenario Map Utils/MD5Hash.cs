using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.IO;

public class MD5Hash : MonoBehaviour
{
	public static string MD5Encoding(string rawPass)
	{
		MD5 md5 = MD5.Create();
		byte[] bs = Encoding.UTF8.GetBytes(rawPass);
		byte[] hs = md5.ComputeHash(bs);
		md5.Clear();
		
		return ByteArrayToHexString(hs);
	}
	
	public static string MD5Encoding(Stream stream)
	{
		MD5 md5 = MD5.Create();
		byte[] hs = md5.ComputeHash(stream);
		md5.Clear();
		
		return ByteArrayToHexString(hs);
	}

	public static string MD5Encoding(byte[] buffer, int offset, int count)
	{
		MD5 md5 = MD5.Create();
		byte[] hash = md5.ComputeHash(buffer, offset, count);
		md5.Clear();
		
		return ByteArrayToHexString(hash);
	}
	
	private static string ByteArrayToHexString(byte[] values)
	{
		string md5 = "";
		foreach (byte value in values)
			md5 += value.ToString("X").PadLeft(2, '0');
		return md5;
	}
}
