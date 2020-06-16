using UnityEngine;

public struct ForceAndTorque
{
    public Vector3 force;
    public Vector3 torque;

    public ForceAndTorque(Vector3 force, Vector3 torque)
    {
        this.force = force;
        this.torque = torque;
    }

    public static ForceAndTorque operator+(ForceAndTorque a, ForceAndTorque b)
    {
        return new ForceAndTorque(a.force + b.force, a.torque + b.torque);
    }

    public static ForceAndTorque operator *(float f, ForceAndTorque a)
    {
        return new ForceAndTorque(f * a.force, f * a.torque);
    }

    public static ForceAndTorque operator *(ForceAndTorque a, float f)
    {
        return f * a;
    }
}
