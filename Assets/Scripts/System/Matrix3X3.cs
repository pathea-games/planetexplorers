using UnityEngine;

public struct Matrix3X3
{
    public float M11;
    public float M12;
    public float M13;
    public float M21;
    public float M22;
    public float M23;
    public float M31;
    public float M32;
    public float M33;

    public Matrix3X3(Vector3 col1, Vector3 col2, Vector3 col3)
    {
        M11 = col1.x;
        M12 = col2.x;
        M13 = col3.x;
        M21 = col1.y;
        M22 = col2.y;
        M23 = col3.y;
        M31 = col1.z;
        M32 = col2.z;
        M33 = col3.z;
    }

    public Matrix3X3(float m11,float m12,float m13,
                     float m21,float m22,float m23,
                     float m31,float m32,float m33)
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;
        M21 = m21;
        M22 = m22;
        M23 = m23;
        M31 = m31;
        M32 = m32;
        M33 = m33;
    }

    public Vector3 Col1
    {
        get { return new Vector3(M11, M21, M31); }
        set { M11 = value.x; M21 = value.y; M31 = value.z; }
    }
    public Vector3 Col2
    {
        get { return new Vector3(M12, M22, M32); }
        set { M12 = value.x; M22 = value.y; M32 = value.z; }
    }
    public Vector3 Col3
    {
        get { return new Vector3(M13, M23, M33); }
        set { M13 = value.x; M23 = value.y; M33 = value.z; }
    }

    public static Vector3 operator *(Matrix3X3 m, Vector3 v)
    {
        return new Vector3(
            m.M11 * v.x + m.M12 * v.y + m.M13 * v.z,
            m.M21 * v.x + m.M22 * v.y + m.M23 * v.z,
            m.M31 * v.x + m.M32 * v.y + m.M33 * v.z);
    }

    public static IntVector3 operator *(Matrix3X3 m, IntVector3 v)
    {
        return new IntVector3(
            (int)(m.M11 * v.x + m.M12 * v.y + m.M13 * v.z),
            (int)(m.M21 * v.x + m.M22 * v.y + m.M23 * v.z),
            (int)(m.M31 * v.x + m.M32 * v.y + m.M33 * v.z));
    }
}