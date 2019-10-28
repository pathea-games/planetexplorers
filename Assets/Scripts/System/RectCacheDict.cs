using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RectCacheDict<TValue>// : IDictionary<TKey,TValue>
{
    private Dictionary<IntVector2, TValue> _dict= new Dictionary<IntVector2, TValue>();
	
	private IntVector2[][] _posMap;
	private IntVector2 _lastRemovedKey;
	private TValue _lastRemovedValue;
	
	public IntVector2 LastRemovedKey{	get{return _lastRemovedKey;		}}
	public TValue LastRemovedValue{ 	get{return _lastRemovedValue;	}}
    public int MapSizeX { get; set; }
    public int MapSizeY { get; set; }

    public RectCacheDict(int sizeX, int sizeY)
    {
		MapSizeX = sizeX;
		MapSizeY = sizeY;
		_posMap = new IntVector2[MapSizeX][];
		for(int x = 0; x < MapSizeX; x++)
		{
			_posMap[x] = new IntVector2[MapSizeY];
		}
    }

    public void Add(IntVector2 key, TValue value)
    {
		int curX = key.x;
		while(curX >= MapSizeX)	curX -= MapSizeX;
		while(curX < 0) 		curX += MapSizeX;
		
		int curY = key.y;
		while(curY >= MapSizeY)	curY -= MapSizeY;
		while(curY < 0) 		curY += MapSizeY;
		
		_lastRemovedKey = _posMap[curX][curY];
		_posMap[curX][curY] = key;
		if(_lastRemovedKey != null)
		{
			_lastRemovedValue = _dict[_lastRemovedKey];
			_dict.Remove(_lastRemovedKey);
		}
		_dict.Add(key, value);
    }

    public bool TryGetValue(IntVector2 key, out TValue value)
    {
		return _dict.TryGetValue(key, out value);
    }

  //[rest of IDictionary not implemented yet]
}