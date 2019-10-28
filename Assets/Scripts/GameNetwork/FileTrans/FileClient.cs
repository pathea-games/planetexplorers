using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;

public class FileClient : IDisposable
{
    private TcpClient _client;
    private Stream _stream;
	internal FileData _fileData = new FileData();
	private byte[] _buffer = new byte[FileConst.BlockSize];
	private string _path;
    private string _ext;
    private string _account;
	
    private long _pos;
    //private EFileStatus _status;
    //private bool _disposed;

    internal TcpClient Client { get { return _client; } }
	internal string FilePath { get { return _path; } }
    internal string FileExt { get { return _ext; } }
    internal long Position { get { return _pos; } }
	internal ulong HashCode { get { return _fileData.HashCode; } }
	internal string FileName { get { return _fileData.FileName; } }
	internal long FileLength { get { return _fileData.FileLength; } }
    internal string Account { get { return _account; } }

    //internal event CommonEventHandler SendFileEvent;
    internal event CommonEventHandler DisposedEvent;
    internal event CommonEventHandler CompleteEvent;
    internal event CommonEventHandler DataReceivedEvent;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="FileClient"/> class.
	/// </summary>
    internal FileClient()
    {
        //_status = EFileStatus.NULL;
        //_disposed = false;

        CompleteEvent += OnComplete;
    }

    internal void SetPath(string path, string ext)
    {
        _ext = ext;
        _path = path;
    }

    internal void SetAccount(string account)
    {
        _account = account;
    }
	
	/// <summary>
	/// Receive data(client side).
	/// </summary>
	/// <exception cref='Exception'>
	/// Is thrown when the exception.
	/// </exception>
    void Receive()
    {
        try
        {
			if (null == DataReceivedEvent)
				throw new Exception("null == DataReceivedEvent");
			
            DataReceivedEvent(this, null);
        }
        catch (Exception e)
        {
            Close();
            LogManager.Error(e);
        }
    }
	
	/// <summary>
	/// Sends the file.
	/// </summary>
	/// <param name='filePath'>
	/// File path where file exists.
	/// </param>
	/// <param name='host'>
	/// Server host.
	/// </param>
	/// <param name='port'>
	/// Server port.
	/// </param>
    internal void SendFile(string filePath, string host, int port)
    {
        try
        {
            FileInfo fileInfo = new FileInfo(filePath);
            SendFile(fileInfo, host, port);
        }
        catch (Exception e)
        {
			Close();
            LogManager.Error(e);
        }
    }
	
	/// <summary>
	/// Sends the file.
	/// </summary>
	/// <param name='fileInfo'>
	/// File info.
	/// </param>
	/// <param name='host'>
	/// Server host.
	/// </param>
	/// <param name='port'>
	/// Server port.
	/// </param>
    internal void SendFile(FileInfo fileInfo, string host, int port)
    {
        try
        {
            _fileData.FileName = fileInfo.Name;
            _fileData.FileLength = fileInfo.Length;
			_stream = fileInfo.OpenRead();
			
			_client = new TcpClient();
            _client.BeginConnect(host, port, new AsyncCallback(EndSendConnect), null);
			LogManager.Info("Connect to the iso server ip:", host);
        }
        catch (Exception e)
        {
			Close();
            LogManager.Error(e);
        }
    }
	
	/// <summary>
	/// Sends the file data.
	/// </summary>
	/// <param name='name'>
	/// File name.
	/// </param>
	/// <param name='data'>
	/// File data.
	/// </param>
	/// <param name='host'>
	/// Server host.
	/// </param>
	/// <param name='port'>
	/// Server port.
	/// </param>
	internal void SendFile(string name, byte[] data, string host, int port)
	{
		try
		{
			//_status = EFileStatus.OPEN;
			
	        _fileData.FileLength = data.Length;
			_fileData.FileName = name + ".~vcres";
			_stream = new MemoryStream(data);
			
			_client = new TcpClient();
			_client.BeginConnect(host, port, new AsyncCallback(EndSendConnect), null);
			LogManager.Info("Connect to the iso server ip:", host);
		}
		catch (Exception e)
		{
			Close();
			LogManager.Error(e);
		}
	}

    void EndSendConnect(IAsyncResult ar)
	{
		try
		{
			_client.EndConnect(ar);
			
			//_status = EFileStatus.CHECKING;
			
			_fileData.HashCode = CRC64.Compute(_stream);
			_stream.Position = 0;
			
			//_status = EFileStatus.SENDING;
			
			NetworkStream netStream = _client.GetStream();
            BinaryWriter bw = new BinaryWriter(netStream);
			
			byte head = 0xDD;
			
            bw.Write(head);
			bw.Write(_fileData.HashCode);
            bw.Write(_fileData.FileName);
            bw.Write(_fileData.FileLength);
            bw.Write(_account);
			
			ReadStream();
		}
		catch (Exception e)
		{
			Close();
			LogManager.Error(e);
		}
	}
	
	void ReadStream()
	{
		try
		{
			int readLength = _stream.Read(_buffer, 0, FileConst.BlockSize);
			if (0 == readLength)
            {
                if (null != CompleteEvent)
                    CompleteEvent(this, new FileCompleteEventArgs(true));

                return;
            }

            _pos += readLength;
			
			NetworkStream netStream = _client.GetStream();
			BinaryWriter bw = new BinaryWriter(netStream);
			bw.Write(_buffer, 0, readLength);
			ReadStream();
//            netStream.BeginWrite(_buffer, 0, readLength, new AsyncCallback(SendCallback), null);
		}
		catch (Exception e)
		{
			Close();
			LogManager.Error(e);
		}
	}

//    void SendCallback(IAsyncResult ar)
//    {
//        try
//        {
//            NetworkStream netStream = _client.GetStream();
//            netStream.EndWrite(ar);
//			
//            ReadStream();
//        }
//        catch (Exception e)
//        {
//            Close();
//            LogManager.Error(e);
//        }
//    }

    void OnComplete(object sender, EventArgs args)
    {
        Close();
    }

    internal void DownloadFile(byte head, ulong hashCode, string host, int port)
    {
        try
        {
            _fileData.HashCode = hashCode;
			
			_client = new TcpClient();
            _client.BeginConnect(host, port, new AsyncCallback(EndRecvConnect), head);
			LogManager.Info("Connect to the iso server ip:", host);
        }
        catch (Exception e)
        {

			Close();
			throw e;
        }
    }

    void EndRecvConnect(IAsyncResult ar)
    {
        try
        {
            byte head = (byte)ar.AsyncState;
            _client.EndConnect(ar);
			
            NetworkStream netStream = _client.GetStream();
            BinaryWriter bw = new BinaryWriter(netStream);
            bw.Write(head);
            bw.Write(_fileData.HashCode);
			
			Receive();
        }
        catch (Exception e)
        {
            Close();
			throw e;
        }
    }

    internal void ReceiveFile()
    {
        try
        {
            NetworkStream netStream = _client.GetStream();
            BinaryReader br = new BinaryReader(netStream);
			
			_fileData.HashCode = br.ReadUInt64();
            _fileData.FileName = br.ReadString();
            _fileData.FileLength = br.ReadInt64();
			
			string fileName = _path + _fileData.FileName + ".tmp";
            FileInfo fileInfo = new FileInfo(fileName);
            _stream = fileInfo.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

            netStream.BeginRead(_buffer, 0, FileConst.BlockSize, new AsyncCallback(RecveiveCallback), null);
        }
        catch (Exception e)
        {
            Close();
			throw e;
        }
    }

    void RecveiveCallback(IAsyncResult ar)
    {
        try
        {
            NetworkStream netStream = _client.GetStream();
            int recvLength = netStream.EndRead(ar);
			
            _stream.Write(_buffer, 0, recvLength);
			_pos += recvLength;

            if (_pos < _fileData.FileLength)
            {
                netStream.BeginRead(_buffer, 0, FileConst.BlockSize, new AsyncCallback(RecveiveCallback), null);
            }
            else
            {
				_stream.Position = 0;
				ulong hashCode = CRC64.Compute(_stream);
				bool successed = hashCode == _fileData.HashCode;
				
				if (null != _stream)
                {
                    _stream.Close();
                    _stream = null;
                }
				
				if (successed)
				{
	                string fileName = _path + _fileData.FileName;
                    if (File.Exists(fileName))
						File.Delete(fileName);

					string fileTmp = _path + _fileData.FileName + ".tmp";
					File.Move(fileTmp, fileName);
				}
				
                if (null != CompleteEvent)
                    CompleteEvent(this, new FileCompleteEventArgs(successed));
            }
        }
        catch (Exception e)
        {
            Close();
			throw e;
        }
    }

    internal void Close()
    {
        Dispose();
    }

    protected virtual void Dispose(bool dispose)
    {
        //_disposed = true;

        if (null != DisposedEvent)
            DisposedEvent(this, null);

        if (null != _stream)
        {
            _stream.Close();
            _stream = null;
        }

        if (null != _client)
        {
            _client.Client.Close();
            _client.Close();
            _client = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}