using UnityEngine;
using System.Collections;

namespace Pathea
{
	public static class SplineUtils 
	{

		/// <summary>
		/// Calculate the Bezier spline position
		/// </summary>
		/// <param name="t">the time (0-1) of the curve to sample</param>
		/// <param name="p">the start point of the curve</param>
		/// <param name="a">control point from p</param>
		/// <param name="b">control point from q</param>
		/// <param name="q">the end point of the curve</param>
		public static Vector3 CalculateBezier(float t, Vector3 p, Vector3 a, Vector3 b, Vector3 q)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			float u = 1.0f - t;
			float u2 = u * u;
			float u3 = u2 * u;
			
			Vector3 output = u3 * p + 3 * u2 * t * a + 3 * u * t2 * b + t3 * q;
			
			return output;
		}

		/// <summary>
		/// Calculate the CatmullRom spline position
		/// </summary>
		/// <param name="t">the time (0-1) of the curve to sample</param>
		/// <param name="p">previous of a </param>
		/// <param name="a">the start point of the curve</param>
		/// <param name="b">the end point of the curve</param>
		/// <param name="q">post of b</param>
		public static Vector3 CalculateCatmullRom(float t, Vector3 p, Vector3 a, Vector3 b, Vector3 q)
		{
			var t2 = t * t;
			
			var a0 = -0.5f * p + 1.5f * a - 1.5f * b + 0.5f * q;
			var a1 = p - 2.5f * a + 2f * b - 0.5f * q;
			var a2 = -0.5f * p + 0.5f * b;
			var a3 = a;
			
			return (a0 * t * t2) + (a1 * t2) + (a2 * t) + a3;
		}


		#region Quaternion

		/// <summary>
		/// Calculate Cubic Rotation
		/// </summary>
		/// <param name="p">point we start with</param>
		/// <param name="a">the point immediately before p</param>
		/// <param name="b">Tthe point immediately after q</param>
		/// <param name="q">next point</param>
		/// <param name="t">time (0-1) of the curve pq to sample</param>
		public static Quaternion CalculateCubic(Quaternion p, Quaternion a, Quaternion b, Quaternion q, float t)
		{
			// Ensure all the quaternions are proper for interpolation - thanks Jeff!
			if (Quaternion.Dot(p, q) < 0.0f)
				q = new Quaternion(-q.x, -q.y, -q.z, -q.w);
			
			if (Quaternion.Dot(p, a) < 0.0f)
				a = new Quaternion(-a.x, -a.y, -a.z, -a.w);
			
			if (Quaternion.Dot(p, b) < 0.0f)
				b = new Quaternion(-b.x, -b.y, -b.z, -b.w);
			
			Quaternion a1 = SquadTangent(a, p, q);
			Quaternion b1 = SquadTangent(p, q, b);
			float slerpT = 2.0f * t * (1.0f - t);
			Quaternion sl = Slerp(Slerp(p, q, t), Slerp(a1, b1, t), slerpT);
			return sl;
		}

		//calculate the Squad tangent for use in Cubic Rotation Interpolation
		public static Quaternion SquadTangent(Quaternion before, Quaternion center, Quaternion after)
		{
			Quaternion l1 = LnDif(center, before);
			Quaternion l2 = LnDif(center, after);
			Quaternion e = Quaternion.identity;
			for (int i = 0; i < 4; ++i)
			{
				e[i] = -0.25f * (l1[i] + l2[i]);
			}
			return center * (Exp(e));
		}


		
		public static Quaternion LnDif(Quaternion a, Quaternion b)
		{
			Quaternion dif = Quaternion.Inverse(a) * b;
			Normalize(dif);
			return Log(dif);
		}

		public static Quaternion Normalize(Quaternion q)
		{
			float norm = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
			if (norm > 0.0f)
			{
				q.x /= norm;
				q.y /= norm;
				q.z /= norm;
				q.w /= norm;
			}
			else
			{
				q.x = 0.0f;
				q.y = 0.0f;
				q.z = 0.0f;
				q.w = 1.0f;
			}
			return q;
		}

		public static Quaternion Log(Quaternion q)
		{
			float len = Mathf.Sqrt(q[0] * q[0] + q[1] * q[1] + q[2] * q[2]);
			
			if (len < 1E-6)
			{
				return new Quaternion(q[0], q[1], q[2], 0.0f);
			}
			//else
			float coef = Mathf.Acos(q[3]) / len;
			return new Quaternion(q[0] * coef, q[1] * coef, q[2] * coef, 0.0f);
		}

		public static Quaternion Exp(Quaternion q)
		{
			float theta = Mathf.Sqrt(q[0] * q[0] + q[1] * q[1] + q[2] * q[2]);
			
			if (theta < 1E-6)
			{
				return new Quaternion(q[0], q[1], q[2], Mathf.Cos(theta));
			}
			//else
			float coef = Mathf.Sin(theta) / theta;
			return new Quaternion(q[0] * coef, q[1] * coef, q[2] * coef, Mathf.Cos(theta));
		}

		public static Quaternion Slerp(Quaternion p, Quaternion q, float t)
		{
			Quaternion ret;
			float cos = Quaternion.Dot(p, q);
			float fCoeff0, fCoeff1;
			if ((1.0f + cos) > 0.00001f)
			{
				if ((1.0f - cos) > 0.00001f)
				{
					float omega = Mathf.Acos(cos);
					float somega = Mathf.Sin(omega);
					float invSin = (Mathf.Sign(somega) * 1.0f) / somega;
					fCoeff0 = Mathf.Sin((1.0f - t) * omega) * invSin;
					fCoeff1 = Mathf.Sin(t * omega) * invSin;
				}
				else
				{
					fCoeff0 = 1.0f - t;
					fCoeff1 = t;
				}
				ret.x = fCoeff0 * p.x + fCoeff1 * q.x;
				ret.y = fCoeff0 * p.y + fCoeff1 * q.y;
				ret.z = fCoeff0 * p.z + fCoeff1 * q.z;
				ret.w = fCoeff0 * p.w + fCoeff1 * q.w;
			}
			else
			{
				fCoeff0 = Mathf.Sin((1.0f - t) * Mathf.PI * 0.5f);
				fCoeff1 = Mathf.Sin(t * Mathf.PI * 0.5f);
				
				ret.x = fCoeff0 * p.x - fCoeff1 * p.y;
				ret.y = fCoeff0 * p.y + fCoeff1 * p.x;
				ret.z = fCoeff0 * p.z - fCoeff1 * p.w;
				ret.w = p.z;
			}
			return ret;
		}

		#endregion

	}
}
