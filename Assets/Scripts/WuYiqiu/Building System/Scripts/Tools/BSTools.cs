using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BSTools
{
	public struct IntBox
	{
		public short xMin;
		public short yMin;
		public short zMin;
		public short xMax;
		public short yMax;
		public short zMax;
		
		public long Hash
		{
			get { return (xMin) | (zMin << 10) | (yMin << 20) | (xMax << 32) | (zMax << 42) | (yMax << 52); }
			set
			{
				xMin = (short)(value & 0x3ff);
				zMin = (short)((value >> 10) & 0x3ff);
				yMin = (short)((value >> 20) & 0x3ff);
				xMax = (short)((value >> 32) & 0x3ff);
				zMax = (short)((value >> 42) & 0x3ff);
				yMax = (short)((value >> 52) & 0x3ff);
			}
		}

		public bool Contains(Vector3 point)
		{
			if (point.x >= xMin && point.x <= xMax
			    && point.y >= yMin && point.y <= yMax
			    && point.z >= zMin && point.z <= zMax)
			{
				return true;
			}

			return false;
		}


	}

	public class SelBox
	{
		public SelBox() {}
		public SelBox( long hash, byte val )
		{
			m_Box.Hash = hash;
			m_Val = val;
		}
		public IntBox m_Box;
		public byte m_Val;

		public static IntBox CalculateBound (List<SelBox> boxes)
		{
			int count = boxes.Count;
			int[] xMins  = new int[count];
			int[] yMins  = new int[count];
			int[] zMins  = new int[count];
			int[] xMaxes = new int[count];
			int[] yMaxes = new int[count];
			int[] zMaxes = new int[count];
			
			for (int i = 0; i < count; i++)
			{
				BSTools.IntBox box = boxes[i].m_Box;
				xMins[i] = box.xMin;
				yMins[i] = box.yMin;
				zMins[i] = box.zMin;
				xMaxes[i] = box.xMax;
				yMaxes[i] = box.yMax;
				zMaxes[i] = box.zMax;
			}
			
			IntBox output = new IntBox();
			
			output.xMin = (short)Mathf.Min(xMins);
			output.yMin = (short)Mathf.Min(yMins);
			output.zMin = (short)Mathf.Min(zMins);
			output.xMax = (short)Mathf.Max(xMaxes);
			output.yMax = (short)Mathf.Max(yMaxes);
			output.zMax = (short)Mathf.Max(zMaxes);
			
			return output;
			
		}
	}
	

	public static class LeastBox
	{
		public static int SortSelBox ( SelBox a, SelBox b ) { return (int)(a.m_Val) - (int)(b.m_Val); }
		public static void Calculate ( Dictionary<IntVector3, byte> bitmap, ref List<SelBox> leastboxes )
		{
			// Clear old result
			leastboxes.Clear();
			
			// Fetch key array
			IntVector3 [] keyarr = new IntVector3 [bitmap.Count];
			int index = 0;
			foreach (IntVector3 k in bitmap.Keys)
			{
				keyarr[index] = new IntVector3(k);
				index++;
			}
			
			// Unmark all voxels
			foreach ( IntVector3 key in keyarr )
			{
				(bitmap[key]) &= (0xfe); 
			}
			
			// Traverse all voxels
			foreach ( IntVector3 key in keyarr )
			{
				// bitmap[key] & 1 is the voxel mark.
				if ( (bitmap[key] & 1) == 0 )
				{
					// box value
					byte box_val = bitmap[key];
					box_val = (byte)((box_val == 0xfe) ? (0xfe) : (box_val & 0xe0));
					
					// Mark this voxel
					(bitmap[key]) |= (1);
					
					// Begin & end of this box
					IntVector3 begin = key;
					IntVector3 end = new IntVector3(begin);
					
					// Can extend on xyz direction
					bool can_extend_x = true;
					bool can_extend_y = true;
					bool can_extend_z = true;
					
					// Start extending, make box
					while ( can_extend_x || can_extend_y || can_extend_z )
					{
						// Try to extend x
						if ( can_extend_x )
						{
							bool ext_p = true;
							bool ext_n = true;
							List<IntVector3> ext_key_p = new List<IntVector3> ();
							List<IntVector3> ext_key_n = new List<IntVector3> ();
							for ( int y = begin.y; y <= end.y; y++ )
							{
								for ( int z = begin.z; z <= end.z; z++ )
								{
//									int _tst_key_p = VCIsoData.IPosToKey(end.x + 1,y,z);
//									int _tst_key_n = VCIsoData.IPosToKey(begin.x - 1,y,z);
									IntVector3 _tst_key_p = new IntVector3(end.x + 1,y,z);
									IntVector3 _tst_key_n = new IntVector3(begin.x - 1,y,z);
									byte _tst_val_p = 0xff;
									byte _tst_val_n = 0xff;
									if ( bitmap.ContainsKey(_tst_key_p) )
										_tst_val_p = bitmap[_tst_key_p];
									if ( bitmap.ContainsKey(_tst_key_n) )
										_tst_val_n = bitmap[_tst_key_n];
									_tst_val_p = (byte)((_tst_val_p == 0xfe) ? (0xfe) : (_tst_val_p & 0xe1));
									_tst_val_n = (byte)((_tst_val_n == 0xfe) ? (0xfe) : (_tst_val_n & 0xe1));
									if ( _tst_val_p != box_val )
										ext_p = false;
									else
										ext_key_p.Add(_tst_key_p);
									if ( _tst_val_n != box_val )
										ext_n = false;
									else
										ext_key_n.Add(_tst_key_n);
									if ( !ext_p && !ext_n )
										break;
								}
								if ( !ext_p && !ext_n )
									break;
							}
							if ( ext_p )
							{
								foreach ( IntVector3 __key in ext_key_p )
								{
									// Mark this voxel
									(bitmap[__key]) |= (1);
								}
								end.x++;
							}
							if ( ext_n )
							{
								foreach ( IntVector3 __key in ext_key_n )
								{
									// Mark this voxel
									(bitmap[__key]) |= (1);
								}
								begin.x--;
							}
							if ( !ext_p && !ext_n )
								can_extend_x = false;
							ext_key_p.Clear();
							ext_key_n.Clear();
						}
						// Try to extend z
						if ( can_extend_z )
						{
							bool ext_p = true;
							bool ext_n = true;
							List<IntVector3> ext_key_p = new List<IntVector3> ();
							List<IntVector3> ext_key_n = new List<IntVector3> ();
							for ( int y = begin.y; y <= end.y; y++ )
							{
								for ( int x = begin.x; x <= end.x; x++ )
								{
//									int _tst_key_p = VCIsoData.IPosToKey(x,y,end.z + 1);
//									int _tst_key_n = VCIsoData.IPosToKey(x,y,begin.z - 1);
									IntVector3 _tst_key_p = new IntVector3(x,y,end.z + 1);
									IntVector3 _tst_key_n = new IntVector3(x,y,begin.z - 1);
									byte _tst_val_p = 0xff;
									byte _tst_val_n = 0xff;
									if ( bitmap.ContainsKey(_tst_key_p) )
										_tst_val_p = bitmap[_tst_key_p];
									if ( bitmap.ContainsKey(_tst_key_n) )
										_tst_val_n = bitmap[_tst_key_n];
									_tst_val_p = (byte)((_tst_val_p == 0xfe) ? (0xfe) : (_tst_val_p & 0xe1));
									_tst_val_n = (byte)((_tst_val_n == 0xfe) ? (0xfe) : (_tst_val_n & 0xe1));
									if ( _tst_val_p != box_val )
										ext_p = false;
									else
										ext_key_p.Add(_tst_key_p);
									if ( _tst_val_n != box_val )
										ext_n = false;
									else
										ext_key_n.Add(_tst_key_n);
									if ( !ext_p && !ext_n )
										break;
								}
								if ( !ext_p && !ext_n )
									break;
							}
							if ( ext_p )
							{
								foreach ( IntVector3 __key in ext_key_p )
								{
									// Mark this voxel
									(bitmap[__key]) |= (1);
								}
								end.z++;
							}
							if ( ext_n )
							{
								foreach ( IntVector3 __key in ext_key_n )
								{
									// Mark this voxel
									(bitmap[__key]) |= (1);
								}
								begin.z--;
							}
							if ( !ext_p && !ext_n )
								can_extend_z = false;
							ext_key_p.Clear();
							ext_key_n.Clear();
						}
						// Try to extend y
						if ( can_extend_y )
						{
							bool ext_p = true;
							bool ext_n = true;
							List<IntVector3> ext_key_p = new List<IntVector3> ();
							List<IntVector3> ext_key_n = new List<IntVector3> ();
							for ( int x = begin.x; x <= end.x; x++ )
							{
								for ( int z = begin.z; z <= end.z; z++ )
								{
//									int _tst_key_p = VCIsoData.IPosToKey(x,end.y + 1,z);
//									int _tst_key_n = VCIsoData.IPosToKey(x,begin.y - 1,z);
									IntVector3 _tst_key_p = new IntVector3(x,end.y + 1,z);
									IntVector3 _tst_key_n = new IntVector3(x,begin.y - 1,z);
									byte _tst_val_p = 0xff;
									byte _tst_val_n = 0xff;
									if ( bitmap.ContainsKey(_tst_key_p) )
										_tst_val_p = bitmap[_tst_key_p];
									if ( bitmap.ContainsKey(_tst_key_n) )
										_tst_val_n = bitmap[_tst_key_n];
									_tst_val_p = (byte)((_tst_val_p == 0xfe) ? (0xfe) : (_tst_val_p & 0xe1));
									_tst_val_n = (byte)((_tst_val_n == 0xfe) ? (0xfe) : (_tst_val_n & 0xe1));
									if ( _tst_val_p != box_val )
										ext_p = false;
									else
										ext_key_p.Add(_tst_key_p);
									if ( _tst_val_n != box_val )
										ext_n = false;
									else
										ext_key_n.Add(_tst_key_n);
									if ( !ext_p && !ext_n )
										break;
								}
								if ( !ext_p && !ext_n )
									break;
							}
							if ( ext_p )
							{
								foreach ( IntVector3 __key in ext_key_p )
								{
									// Mark this voxel
									(bitmap[__key]) |= (1);
								}
								end.y++;
							}
							if ( ext_n )
							{
								foreach ( IntVector3 __key in ext_key_n )
								{
									// Mark this voxel
									(bitmap[__key]) |= (1);
								}
								begin.y--;
							}
							if ( !ext_p && !ext_n )
								can_extend_y = false;
							ext_key_p.Clear();
							ext_key_n.Clear();
						}
					}
					
					// Create box
					SelBox box = new SelBox ();
					box.m_Box.xMin = (short)(begin.x);
					box.m_Box.yMin = (short)(begin.y);
					box.m_Box.zMin = (short)(begin.z);
					box.m_Box.xMax = (short)(end.x);
					box.m_Box.yMax = (short)(end.y);
					box.m_Box.zMax = (short)(end.z);
					box.m_Val = (byte)(box_val | 0x07);
					leastboxes.Add(box);
				}
			}
			
			// Sort
			leastboxes.Sort(SortSelBox);
			
			// Increase all voxels
			foreach ( IntVector3 key in keyarr )
				(bitmap[key]) |= (0x01);
		}
	}
}
