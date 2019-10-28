using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public interface IBoundInScene
{
	bool Intersection(ref Bounds bound, bool tstY);
	bool Contains(Vector3 point, bool tstY);
}

public class RadiusBound : IBoundInScene
{
	float _r;
	float _cx;
	float _cy;
	float _cz;

	public RadiusBound(float r, float cx, float cy, float cz){
		_r = r;
		_cx = cx;
		_cy = cy;
		_cz = cz;
	}
	public RadiusBound(RadiusBound local, Transform parent)
	{
		_r = local._r * parent.lossyScale.x;	// assume each axis has same scale
		Vector3 center = parent.TransformPoint(local._cx, local._cy, local._cz);
		_cx = center.x;
		_cy = center.y;
		_cz = center.z;
	}
	public bool Contains(Vector3 pos, bool tstY)
	{
		float sqr = (_cx - pos.x) * (_cx - pos.x) + (_cz - pos.z) * (_cz - pos.z);
		if (tstY) {	sqr += (_cy - pos.y) * (_cy - pos.y);		}
		return sqr < _r * _r;
	}
	public bool Intersection(ref Bounds bound, bool tstY)
	{
		Vector3 min = bound.min;
		Vector3 max = bound.max;
		if (tstY) {
			//code Bounds.Intersects
			return (_cx-_r) <= max.x && (_cx+_r) >= min.x && (_cy-_r) <= max.y && (_cy+_r) >= min.y && (_cz-_r) <= max.z && (_cz+_r) >= min.z;
		}
		//code from Rect.Overlap
		return max.x > (_cx-_r) && min.x < (_cx+_r) && max.z > (_cz-_r) && min.z < (_cz+_r);
	}
}
