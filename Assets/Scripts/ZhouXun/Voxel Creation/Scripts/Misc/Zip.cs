using System.IO;
using System.IO.Compression;
using Ionic = Pathfinding.Ionic;

/// <summary>
/// Obsoleted !!!
/// </summary>
public static class Zip
{
	public const int BUFFER_SIZE = 4096;
	/// <summary>
	/// Compress the specified source stream to the dest stream.
	/// </summary>
	/// <param name='source'>
	/// Source stream (original data).
	/// </param>
	/// <param name='dest'>
	/// Destination stream (compressed data).
	/// </param>
	public static void Compress(Stream source, Stream dest)
	{
		using (GZipStream zipStream = new GZipStream(dest, CompressionMode.Compress, true))
		{
			byte[] buf = new byte[BUFFER_SIZE];
			int len;
			while ((len = source.Read(buf, 0, buf.Length)) > 0)
			{
				zipStream.Write(buf, 0, len);
			}
		}
	}
	
	/// <summary>
	/// Decompress the specified source stream to the dest stream.
	/// </summary>
	/// <param name='source'>
	/// Source stream (compressed data).
	/// </param>
	/// <param name='dest'>
	/// Destination stream (original data).
	/// </param>
	public static void Decompress(Stream source, Stream dest)
	{
		using (GZipStream zipStream = new GZipStream(source, CompressionMode.Decompress, true))
		{
			byte[] buf = new byte[BUFFER_SIZE];
			int len;
			while ((len = zipStream.Read(buf, 0, buf.Length)) > 0)
			{
				dest.Write(buf, 0, len);
			}
		}
	}
}

public static class IonicZlib
{
	public const int BUFFER_SIZE = 4096;
	/// <summary>
	/// Compress the specified source stream to the dest stream.
	/// </summary>
	/// <param name='source'>
	/// Source stream (original data).
	/// </param>
	/// <param name='dest'>
	/// Destination stream (compressed data).
	/// </param>
	public static void Compress(Stream source, Stream dest)
	{
		using ( Ionic.Zlib.ZlibStream inputStream = new Ionic.Zlib.ZlibStream(
			dest, 
			Ionic.Zlib.CompressionMode.Compress, 
			Ionic.Zlib.CompressionLevel.BestCompression,
			true) )
		{
			byte[] buf = new byte[BUFFER_SIZE];
			int len;
			while ((len = source.Read(buf, 0, buf.Length)) > 0)
			{
				inputStream.Write(buf, 0, len);
			}
		}
	}
	
	/// <summary>
	/// Decompress the specified source stream to the dest stream.
	/// </summary>
	/// <param name='source'>
	/// Source stream (compressed data).
	/// </param>
	/// <param name='dest'>
	/// Destination stream (original data).
	/// </param>
	public static void Decompress(Stream source, Stream dest)
	{
		using ( Ionic.Zlib.ZlibStream inputStream = new Ionic.Zlib.ZlibStream(
			dest, 
			Ionic.Zlib.CompressionMode.Decompress,
			Ionic.Zlib.CompressionLevel.BestCompression,
			true) )
		{
			byte[] buf = new byte[BUFFER_SIZE];
			int len;
			while ((len = source.Read(buf, 0, buf.Length)) > 0)
			{
				inputStream.Write(buf, 0, len);
			}
		}
	}
}