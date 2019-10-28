using System.IO;
using System;

public enum EFileStatus
{
    NULL = 1,
    OPEN,
    CHECKING,
    SENDING,
    RECEIVEING,
    COMPLETE
}

internal class FileConst
{
    internal static readonly int BlockSize = 500 * 1024;
}

public delegate void CommonEventHandler(object sender, EventArgs e);