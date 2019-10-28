using UnityEngine;
using System;

namespace Pathea
{
	namespace Maths
	{
		public struct INTVECTOR2
		{
			public int x;
			public int y;

			public int hash
			{
				get { return (x & 0xffff) | ((y & 0xffff) << 16); }
				set
				{
					x = (int)((short)(value & 0xffff));
					y = (int)((short)((value >> 16) & 0xffff));
				}
			}
			public int hash_u	// Use this if intvector is always positive
			{
				get { return hash; }
				set
				{
					x = value & 0xffff;
					y = (value >> 16) & 0xffff;
				}
			}

			public static INTVECTOR2 zero { get { return new INTVECTOR2(0,0); } }
			public static INTVECTOR2 one { get { return new INTVECTOR2(1,1); } }
			public static INTVECTOR2 minusone { get { return new INTVECTOR2(-1,-1); } }
			
			public static INTVECTOR2 unit_x { get{ return new INTVECTOR2 (1,0); } }
			public static INTVECTOR2 unit_y { get{ return new INTVECTOR2 (0,1); } }

			public INTVECTOR2 (INTVECTOR2 vec)
			{
				x = vec.x;
				y = vec.y;
			}
			public INTVECTOR2 (int x_, int y_)
			{
				x = x_;
				y = y_;
			}
			public INTVECTOR2 (float x_, float y_)
			{
				x = Mathf.FloorToInt(x_ + Maths.Epsilon);
				y = Mathf.FloorToInt(y_ + Maths.Epsilon);
			}

			public static bool operator == (INTVECTOR2 lhs, INTVECTOR2 rhs)
			{
				return lhs.x == rhs.x && lhs.y == rhs.y;
			}
			public static bool operator != (INTVECTOR2 lhs, INTVECTOR2 rhs)
			{
				return lhs.x != rhs.x || lhs.y != rhs.y;
			}
			public static INTVECTOR2 operator * (INTVECTOR2 lhs, INTVECTOR2 rhs)
			{
				return new INTVECTOR2 (lhs.x*rhs.x, lhs.y*rhs.y);
			}
			public static INTVECTOR2 operator * (INTVECTOR2 lhs, int rhs)
			{
				return new INTVECTOR2 (lhs.x*rhs, lhs.y*rhs);
			}
			public static INTVECTOR2 operator / (INTVECTOR2 lhs, int rhs)
			{
				return new INTVECTOR2 (lhs.x/rhs, lhs.y/rhs);
			}
			public static INTVECTOR2 operator - (INTVECTOR2 lhs, INTVECTOR2 rhs)
			{
				return new INTVECTOR2 (lhs.x-rhs.x, lhs.y-rhs.y);
			}
			public static INTVECTOR2 operator + (INTVECTOR2 lhs, INTVECTOR2 rhs)
			{
				return new INTVECTOR2 (lhs.x+rhs.x, lhs.y+rhs.y);
			}
			public static implicit operator INTVECTOR2 (Vector2 vec2)
			{
				return new INTVECTOR2 (Mathf.FloorToInt(vec2.x + Maths.Epsilon), 
				                       Mathf.FloorToInt(vec2.y + Maths.Epsilon));
			}
			public static implicit operator Vector2 (INTVECTOR2 vec2)
			{
				return new Vector2 ((float)vec2.x, (float)vec2.y); 
			}
			public float sqrMagnitude { get { return x*x + y*y; } }
			public float magnitude { get { return Mathf.Sqrt(sqrMagnitude); } }
			public float Distance(INTVECTOR2 vec)
			{
				return Mathf.Sqrt( (vec.x - x) * (vec.x - x) + 
				                   (vec.y - y) * (vec.y - y) );
			}

			public override bool Equals (object obj)
			{
				if (null == obj)
					return false;
				if (obj is INTVECTOR2)
				{
					INTVECTOR2 vec = (INTVECTOR2) obj;
					return x == vec.x && y == vec.y;
				}
				return false;
			}
			public override int GetHashCode ()
			{
				return hash;
			}
			public override string ToString ()
			{
				return string.Format("[{0},{1}]", x,y);
			}
		}

		public struct INTVECTOR3
		{
			public int x;
			public int y;
			public int z;
			
			public int hash
			{
				get { return (x & 0x7ff) | ((z & 0x7ff) << 11) | ((y & 0x3ff) << 22); }
				set
				{
					x = value & 0x7ff;
					z = (value >> 11) & 0x7ff;
					y = (value >> 22) & 0x3ff;
					if ( x >= 0x400 ) x -= 0x800;
					if ( y >= 0x200 ) y -= 0x400;
					if ( z >= 0x400 ) z -= 0x800;
				}
			}
			public int hash_u	// Use this if intvector is always positive
			{
				get { return hash; }
				set
				{
					x = value & 0x7ff;
					z = (value >> 11) & 0x7ff;
					y = (value >> 22) & 0x3ff;
				}
			}

			public static INTVECTOR3 zero { get { return new INTVECTOR3(0,0,0); } }
			public static INTVECTOR3 one { get { return new INTVECTOR3(1,1,1); } }
			public static INTVECTOR3 minusone { get { return new INTVECTOR3(-1,-1,-1); } }

			public static INTVECTOR3 unit_x { get{ return new INTVECTOR3 (1,0,0); } }
			public static INTVECTOR3 unit_y { get{ return new INTVECTOR3 (0,1,0); } }
			public static INTVECTOR3 unit_z { get{ return new INTVECTOR3 (0,0,1); } }
			
			public INTVECTOR2 xy { get { return new INTVECTOR2(x, y); } }
			public INTVECTOR2 yx { get { return new INTVECTOR2(y, x); } }
			public INTVECTOR2 zx { get { return new INTVECTOR2(z, x); } }
			public INTVECTOR2 xz { get { return new INTVECTOR2(x, z); } }
			public INTVECTOR2 yz { get { return new INTVECTOR2(y, z); } }
			public INTVECTOR2 zy { get { return new INTVECTOR2(z, y); } }

			public INTVECTOR3 (INTVECTOR3 vec)
			{
				x = vec.x;
				y = vec.y;
				z = vec.z;
			}
			public INTVECTOR3 (int x_, int y_, int z_)
			{
				x = x_;
				y = y_;
				z = z_;
			}
			public INTVECTOR3 (float x_, float y_, float z_)
			{
				x = Mathf.FloorToInt(x_ + Maths.Epsilon);
				y = Mathf.FloorToInt(y_ + Maths.Epsilon);
				z = Mathf.FloorToInt(z_ + Maths.Epsilon);
			}

			public static bool operator == (INTVECTOR3 lhs, INTVECTOR3 rhs)
			{
				return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
			}
			public static bool operator != (INTVECTOR3 lhs, INTVECTOR3 rhs)
			{
				return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
			}
			public static INTVECTOR3 operator * (INTVECTOR3 lhs, INTVECTOR3 rhs)
			{
				return new INTVECTOR3 (lhs.x*rhs.x, lhs.y*rhs.y, lhs.z*rhs.z);
			}
			public static INTVECTOR3 operator * (INTVECTOR3 lhs, int rhs)
			{
				return new INTVECTOR3 (lhs.x*rhs, lhs.y*rhs, lhs.z*rhs);
			}
			public static INTVECTOR3 operator / (INTVECTOR3 lhs, int rhs)
			{
				return new INTVECTOR3 (lhs.x/rhs, lhs.y/rhs, lhs.z/rhs);
			}
			public static INTVECTOR3 operator - (INTVECTOR3 lhs, INTVECTOR3 rhs)
			{
				return new INTVECTOR3 (lhs.x-rhs.x, lhs.y-rhs.y, lhs.z-rhs.z);
			}
			public static INTVECTOR3 operator + (INTVECTOR3 lhs, INTVECTOR3 rhs)
			{
				return new INTVECTOR3 (lhs.x+rhs.x, lhs.y+rhs.y, lhs.z+rhs.z);
			}
			public static implicit operator INTVECTOR3 (Vector3 vec3)
			{
				return new INTVECTOR3 (Mathf.FloorToInt(vec3.x + Maths.Epsilon), 
				                       Mathf.FloorToInt(vec3.y + Maths.Epsilon),
				                       Mathf.FloorToInt(vec3.z + Maths.Epsilon));
			}
			public static implicit operator Vector3 (INTVECTOR3 vec3)
			{
				return new Vector3 ((float)vec3.x, (float)vec3.y, (float)vec3.z);
			}
			public float sqrMagnitude { get { return x*x + y*y + z*z; } }
			public float magnitude { get { return Mathf.Sqrt(sqrMagnitude); } }
			public float Distance(INTVECTOR3 vec)
			{
				return Mathf.Sqrt( (vec.x - x) * (vec.x - x) + 
				                   (vec.y - y) * (vec.y - y) + 
				                   (vec.z - z) * (vec.z - z) );
			}
			public override bool Equals (object obj)
			{
				if (null == obj)
					return false;
				if (obj is INTVECTOR3)
				{
					INTVECTOR3 vec = (INTVECTOR3) obj;
					return x == vec.x && y == vec.y && z == vec.z;
				}
				return false;
			}
			public override int GetHashCode ()
			{
				return hash;
			}
			public override string ToString ()
			{
				return string.Format("[{0},{1},{2}]", x,y,z);
			}	
		}
		
		public struct INTVECTOR4
		{
			public int x;
			public int y;
			public int z;
			public int w;

			public int hash
			{
				get { return (x & 0x3ff) | ((z & 0x3ff) << 10) | ((y & 0x1ff) << 20) | (((w + 1) & 0x7) << 29); }
				set
				{
					x = value & 0x3ff;
					z = (value >> 10) & 0x3ff;
					y = (value >> 20) & 0x1ff;
					w = ((value >> 29) & 0x7) - 1;
					if ( x >= 0x200 ) x -= 0x400;
					if ( y >= 0x100 ) y -= 0x200;
					if ( z >= 0x200 ) z -= 0x400;
				}
			}
			public int hash_u	// Use this if intvector is always positive
			{
				get { return hash; }
				set
				{
					x = value & 0x3ff;
					z = (value >> 10) & 0x3ff;
					y = (value >> 20) & 0x1ff;
					w = ((value >> 29) & 0x7) - 1;
				}
			}

			public static INTVECTOR4 zero { get { return new INTVECTOR4(0,0,0,0); } }
			public static INTVECTOR4 one { get { return new INTVECTOR4(1,1,1,1); } }
			public static INTVECTOR4 minusone { get { return new INTVECTOR4(-1,-1,-1,-1); } }
			
			public static INTVECTOR4 unit_x { get{ return new INTVECTOR4 (1,0,0,0); } }
			public static INTVECTOR4 unit_y { get{ return new INTVECTOR4 (0,1,0,0); } }
			public static INTVECTOR4 unit_z { get{ return new INTVECTOR4 (0,0,1,0); } }
			public static INTVECTOR4 unit_w { get{ return new INTVECTOR4 (0,0,0,1); } }

			public INTVECTOR4 (int x_, int y_, int z_, int w_)
			{
				x = x_;
				y = y_;
				z = z_;
				w = w_;
			}
			public INTVECTOR4 (INTVECTOR3 v3, int w_)
			{
				x = v3.x;
				y = v3.y;
				z = v3.z;
				w = w_;
			}

			public INTVECTOR3 xyz { get { return new INTVECTOR3(x, y, z); } }
			public Vector3 xyzf { get { return new Vector3((float)x,(float)y,(float)z); } }

			public static bool operator == (INTVECTOR4 lhs, INTVECTOR4 rhs)
			{
				return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.w == rhs.w;
			}
			public static bool operator != (INTVECTOR4 lhs, INTVECTOR4 rhs)
			{
				return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z || lhs.w != rhs.w;
			}
			public static INTVECTOR4 operator * (INTVECTOR4 lhs, INTVECTOR4 rhs)
			{
				return new INTVECTOR4 (lhs.x*rhs.x, lhs.y*rhs.y, lhs.z*rhs.z, lhs.w*rhs.w);
			}
			public static INTVECTOR4 operator * (INTVECTOR4 lhs, int rhs)
			{
				return new INTVECTOR4 (lhs.x*rhs, lhs.y*rhs, lhs.z*rhs, lhs.w*rhs);
			}
			public static INTVECTOR4 operator / (INTVECTOR4 lhs, int rhs)
			{
				return new INTVECTOR4 (lhs.x/rhs, lhs.y/rhs, lhs.z/rhs, lhs.w/rhs);
			}
			public static INTVECTOR4 operator - (INTVECTOR4 lhs, INTVECTOR4 rhs)
			{
				return new INTVECTOR4 (lhs.x-rhs.x, lhs.y-rhs.y, lhs.z-rhs.z, lhs.w-rhs.w);
			}
			public static INTVECTOR4 operator + (INTVECTOR4 lhs, INTVECTOR4 rhs)
			{
				return new INTVECTOR4 (lhs.x+rhs.x, lhs.y+rhs.y, lhs.z+rhs.z, lhs.w+rhs.w);
			}
			public static implicit operator INTVECTOR4 (Vector4 vec4)
			{
				return new INTVECTOR4 (Mathf.FloorToInt(vec4.x + Maths.Epsilon), 
				                       Mathf.FloorToInt(vec4.y + Maths.Epsilon),
				                       Mathf.FloorToInt(vec4.z + Maths.Epsilon),
				                       Mathf.FloorToInt(vec4.w + Maths.Epsilon));
			}
			public static implicit operator Vector4 (INTVECTOR4 vec4)
			{
				return new Vector4 ((float)vec4.x, (float)vec4.y, (float)vec4.z, (float)vec4.w);
			}
			public float sqrMagnitude { get { return x*x + y*y + z*z + w*w; } }
			public float magnitude { get { return Mathf.Sqrt(sqrMagnitude); } }

			public override bool Equals (object obj)
			{
				if (null == obj)
					return false;
				if (obj is INTVECTOR4)
				{
					INTVECTOR4 vec = (INTVECTOR4) obj;
					return x == vec.x && y == vec.y && z == vec.z && w == vec.w;
				}
				return false;
			}
			public override int GetHashCode ()
			{
				return hash;
			}
			public override string ToString ()
			{
				return string.Format("[{0},{1},{2},{3}]", x,y,z,w);
			}
		}
	}
}
