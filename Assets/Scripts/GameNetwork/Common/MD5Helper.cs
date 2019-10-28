using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class MD5Helper
{
	public static string CretaeMD5(string fileName)
	{
		string hashStr = string.Empty;
        FileStream fs = null;

		try
		{
			fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            MD5 md5 = MD5.Create();
			byte[] hash = md5.ComputeHash(fs);
            md5.Clear();

			hashStr = ByteArrayToHexString(hash);
		}
		catch (Exception e)
		{
            LogManager.Error(e);
		}
        finally
        {
            if (null != fs)
            {
                fs.Close();
                fs.Dispose();
            }
        }

		return hashStr;
	}

	public static string CretaeMD5(Stream stream)
	{
        MD5 md5 = MD5.Create();
		byte[] hash = md5.ComputeHash(stream);
        md5.Clear();

		return ByteArrayToHexString(hash);
	}

	public static string CretaeMD5(byte[] buffer, int offset, int count)
	{
        MD5 md5 = MD5.Create();
		byte[] hash = md5.ComputeHash(buffer, offset, count);
        md5.Clear();

		return ByteArrayToHexString(hash);
	}

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

	private static string ByteArrayToHexString(byte[] values)
	{
		StringBuilder sb = new StringBuilder();
		foreach (byte value in values)
			sb.AppendFormat("{0:X2}", value);

		return sb.ToString();
	}
}
