using UnityEngine;
using System.IO;
using System;

public class BufferHelper
{
    private static byte[] Buffer = new byte[8192];

    public static byte[] Export(Action<BinaryWriter> writer)
    {
        MemoryStream stream = null;
        BinaryWriter w = null;

        try
        {
            stream = new MemoryStream(Buffer);
            w = new BinaryWriter(stream);
            writer(w);
            return stream.ToArray();
        }
        catch
        {
            return null;
        }
        finally
        {
            if (null != w)
            {
                w.Close();
                w = null;
            }

            if (null != stream)
            {
                stream.Close();
                stream = null;
            }
        }
    }

    public static byte[] ExportThreadSafe(Action<BinaryWriter> writer)
    {
        return ExportThreadSafe(writer, 256);
    }

    public static byte[] ExportThreadSafe(Action<BinaryWriter> writer, int capacity)
    {
        MemoryStream stream = null;
        BinaryWriter w = null;

        try
        {
            stream = new MemoryStream(capacity);
            w = new BinaryWriter(stream);
            writer(w);
            return stream.ToArray();
        }
        catch
        {
            return null;
        }
        finally
        {
            if (null != w)
            {
                w.Close();
                w = null;
            }

            if (null != stream)
            {
                stream.Close();
                stream = null;
            }
        }
    }

    public static void Import(byte[] buffer, Action<BinaryReader> reader)
    {
        MemoryStream stream = null;
        BinaryReader r = null;

        try
        {
            stream = new MemoryStream(buffer);
            r = new BinaryReader(stream);
            reader(r);
        }
        finally
        {
            if (null != r)
            {
                r.Close();
                r = null;
            }

            if (null != stream)
            {
                stream.Close();
                stream = null;
            }
        }
    }

    /// <summary>
    /// Serialize to BinaryWriter
    /// </summary>
    /// <param name="_writer"></param>
    /// <param name="_value"></param>
    /// 

    internal static void Serialize(BinaryWriter _writer, string _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, bool _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, char _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, byte _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, sbyte _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, short _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, ushort _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, int _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, uint _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, long _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, ulong _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, float _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, double _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, decimal _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, byte[] _value)
	{
		if (null == _value || _value.Length <= 0)
		{
			_writer.Write(0);
		}
		else
		{
			_writer.Write(_value.Length);
			_writer.Write(_value);
		}
	}

	internal static void Serialize(BinaryWriter _writer, byte[] _value, int _index, int _count)
	{
		_writer.Write(_value, _index, _count);
	}

	internal static void Serialize(BinaryWriter _writer, char[] _value)
	{
		if (null == _value || _value.Length <= 0)
		{
			_writer.Write(0);
		}
		else
		{
			_writer.Write(_value.Length);
			_writer.Write(_value);
		}
	}

	internal static void Serialize(BinaryWriter _writer, char[] _value, int _index, int _count)
	{
		_writer.Write(_value, _index, _count);
	}

	internal static void Serialize(BinaryWriter _writer, IntVector3 _value)
	{
		_writer.Write(_value.x);
		_writer.Write(_value.y);
		_writer.Write(_value.z);
	}

	internal static void Serialize(BinaryWriter _writer, Vector3 _value)
	{
		_writer.Write(_value.x);
		_writer.Write(_value.y);
		_writer.Write(_value.z);
	}

    internal static void Serialize(BinaryWriter _writer, Vector2 _value)
    {
        _writer.Write(_value.x);
        _writer.Write(_value.y);
    }

    internal static void Serialize(BinaryWriter _writer, Quaternion _value)
	{
		_writer.Write(_value.x);
		_writer.Write(_value.y);
		_writer.Write(_value.z);
		_writer.Write(_value.w);
	}

	internal static void Serialize(BinaryWriter _writer, B45Block _value)
	{
		_writer.Write(_value.blockType);
		_writer.Write(_value.materialType);
	}

	internal static void Serialize(BinaryWriter _writer, VFVoxel _value)
	{
		_writer.Write(_value.Type);
		_writer.Write(_value.Volume);
	}

	internal static void Serialize(BinaryWriter _writer, BSVoxel _value)
	{
		_writer.Write(_value.value0);
		_writer.Write(_value.value1);
	}

	internal static void Serialize(BinaryWriter _writer, Color _value)
	{
		_writer.Write(_value.a);
		_writer.Write(_value.r);
		_writer.Write(_value.g);
		_writer.Write(_value.b);
	}

    public static void Serialize(BinaryWriter writer, PeCustom.OBJECT obj)
    {
        writer.Write(obj.Id);
        writer.Write(obj.Group);
        writer.Write((byte)obj.type);
    }

    public static void Serialize(BinaryWriter writer, PeCustom.RANGE obj)
    {
        Serialize(writer, obj.center);
        Serialize(writer, obj.extend);
        Serialize(writer, obj.radius);
        Serialize(writer, (byte)obj.type);
        Serialize(writer, obj.inverse);
    }

    public static void Serialize(BinaryWriter writer, PeCustom.DIRRANGE obj)
    {
        Serialize(writer, obj.directrix);
        Serialize(writer, obj.error);
        Serialize(writer, (byte)obj.type);
        Serialize(writer, obj.inverse);
    }

    /// <summary>
    /// Deserialize from BinaryReader
    /// </summary>
    /// <param name="_reader"></param>
    /// <returns></returns>
    /// 

    internal static string ReadString(BinaryReader _reader)
	{
		return _reader.ReadString();
	}

	internal static bool ReadBoolean(BinaryReader _reader)
	{
		return _reader.ReadBoolean();
	}

	internal static char ReadChar(BinaryReader _reader)
	{
		return _reader.ReadChar();
	}

	internal static byte ReadByte(BinaryReader _reader)
	{
		return _reader.ReadByte();
	}

	internal static sbyte ReadSByte(BinaryReader _reader)
	{
		return _reader.ReadSByte();
	}

	internal static short ReadInt16(BinaryReader _reader)
	{
		return _reader.ReadInt16();
	}

	internal static ushort ReadUInt16(BinaryReader _reader)
	{
		return _reader.ReadUInt16();
	}

	internal static int ReadInt32(BinaryReader _reader)
	{
		return _reader.ReadInt32();
	}

	internal static uint ReadUInt32(BinaryReader _reader)
	{
		return _reader.ReadUInt32();
	}

	internal static long ReadInt64(BinaryReader _reader)
	{
		return _reader.ReadInt64();
	}

	internal static ulong ReadUInt64(BinaryReader _reader)
	{
		return _reader.ReadUInt64();
	}

	internal static float ReadSingle(BinaryReader _reader)
	{
		return _reader.ReadSingle();
	}

	internal static double ReadDouble(BinaryReader _reader)
	{
		return _reader.ReadDouble();
	}

	internal static decimal ReadDecimal(BinaryReader _reader)
	{
		return _reader.ReadDecimal();
	}

	internal static byte[] ReadBytes(BinaryReader _reader)
	{
		int length = _reader.ReadInt32();
		if (0 == length)
			return null;

		return _reader.ReadBytes(length);
	}

	internal static char[] ReadChars(BinaryReader _reader)
	{
		int length = _reader.ReadInt32();
		if (0 == length)
			return null;

		return _reader.ReadChars(length);
	}

	internal static void ReadIntVector3(BinaryReader _reader, out IntVector3 v)
	{
		v = new IntVector3();
		v.x = _reader.ReadInt32();
		v.y = _reader.ReadInt32();
		v.z = _reader.ReadInt32();
	}

	internal static void ReadVector3(BinaryReader _reader, out Vector3 _value)
	{
		_value.x = _reader.ReadSingle();
		_value.y = _reader.ReadSingle();
		_value.z = _reader.ReadSingle();
	}

    internal static void ReadVector2(BinaryReader _reader, out Vector2 _value)
    {
        _value.x = _reader.ReadSingle();
        _value.y = _reader.ReadSingle();
    }

    internal static void ReadQuaternion(BinaryReader _reader, out Quaternion _value)
	{
		_value.x = _reader.ReadSingle();
		_value.y = _reader.ReadSingle();
		_value.z = _reader.ReadSingle();
		_value.w = _reader.ReadSingle();
	}

	internal static void ReadB45Block(BinaryReader _reader, out B45Block _value)
	{
		_value.blockType = _reader.ReadByte();
		_value.materialType = _reader.ReadByte();
	}

	internal static void ReadVFVoxel(BinaryReader _reader, out VFVoxel _value)
	{
		_value.Type = _reader.ReadByte();
		_value.Volume = _reader.ReadByte();
	}

	internal static void ReadBSVoxel(BinaryReader _reader, out BSVoxel _value)
	{
		byte value0 = _reader.ReadByte();
		byte value1 = _reader.ReadByte();

		_value = new BSVoxel(value0, value1);
	}

	internal static void ReadColor(BinaryReader _reader, out Color _value)
	{
		_value.a = _reader.ReadSingle();
		_value.r = _reader.ReadSingle();
		_value.g = _reader.ReadSingle();
		_value.b = _reader.ReadSingle();
	}

    public static void ReadObject(BinaryReader _reader, out PeCustom.OBJECT obj)
    {
        obj.Id = _reader.ReadInt32();
        obj.Group = _reader.ReadInt32();
        obj.type = (PeCustom.OBJECT.OBJECTTYPE)_reader.ReadByte();
    }

    public static void ReadRange(BinaryReader _reader, out PeCustom.RANGE obj)
    {
        ReadVector3(_reader, out obj.center);
        ReadVector3(_reader, out obj.extend);
        obj.radius = _reader.ReadSingle();
        obj.type = (PeCustom.RANGE.RANGETYPE)_reader.ReadByte();
        obj.inverse = _reader.ReadBoolean();
    }

    public static void ReadDirRange(BinaryReader _reader, out PeCustom.DIRRANGE obj)
    {
        ReadVector3(_reader, out obj.directrix);
        ReadVector2(_reader, out obj.error);
        obj.type = (PeCustom.DIRRANGE.DIRRANGETYPE)_reader.ReadByte();
        obj.inverse = _reader.ReadBoolean();
    }
}
