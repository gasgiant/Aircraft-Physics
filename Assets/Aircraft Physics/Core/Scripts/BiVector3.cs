using UnityEngine;

public struct BiVector3
{
    public Vector3 p;
    public Vector3 q;

    public BiVector3(Vector3 force, Vector3 torque)
    {
        this.p = force;
        this.q = torque;
    }

    public static BiVector3 operator+(BiVector3 a, BiVector3 b)
    {
        return new BiVector3(a.p + b.p, a.q + b.q);
    }

    public static BiVector3 operator *(float f, BiVector3 a)
    {
        return new BiVector3(f * a.p, f * a.q);
    }

    public static BiVector3 operator *(BiVector3 a, float f)
    {
        return f * a;
    }
}
