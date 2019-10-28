using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;


public abstract class VEObject
{
    [XMLIO(Order = -99, Attr = "ID", Necessary = true)]
    public int ID
    {
        get;
        set;
    }

    [XMLIO(Order = -1, Attr = "name", DefaultValue = "")]
    public string ObjectName
    {
        get;
        set;
    }

	public virtual string ToXML()
	{
		return VEObjectAsm.ToXML(this);
	}

	public virtual void Parse(XmlElement xmlelem)
	{
		VEObjectAsm.Parse(this, xmlelem);
	}
}
