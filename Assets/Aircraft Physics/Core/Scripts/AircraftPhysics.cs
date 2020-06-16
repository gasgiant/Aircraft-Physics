using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AircraftPhysics : MonoBehaviour
{
    const float PREDICTION_TIMESTEP_FRACTION = 0.5f;

    [SerializeField] 
    float thrust = 0;
    [SerializeField] 
    List<AeroSurface> aerodynamicSurfaces = null;
    [SerializeField]
    List<ControlSurface> controlSurfaces = null;

    Rigidbody rb;
    float thrustPercent;
    ForceAndTorque currentForceAndTorque;

    public void SetThrustPercent(float percent)
    {
        thrustPercent = percent;
    }

    public void SetControlSurfecesAngles(float pitch, float yaw, float roll)
    {
        foreach (var controlSurface in controlSurfaces)
        {
            switch (controlSurface.type)
            {
                case ControlSurfaceType.Pitch:
                    controlSurface.surface.SetFlapAngle(pitch * controlSurface.flapAngle);
                    break;
                case ControlSurfaceType.Yaw:
                    controlSurface.surface.SetFlapAngle(yaw * controlSurface.flapAngle);
                    break;
                case ControlSurfaceType.Roll:
                    controlSurface.surface.SetFlapAngle(roll * controlSurface.flapAngle);
                    break;
            }
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        ForceAndTorque forceAndTorqueThisFrame = 
            CalculateAerodynamicForces(rb.velocity, rb.angularVelocity, Vector3.zero, 1.2f, rb.worldCenterOfMass);

        Vector3 velocityPrediction = PredictVelocity(forceAndTorqueThisFrame.force);
        Vector3 angularVelocityPrediction = PredictAngularVelocity(forceAndTorqueThisFrame.torque);

        ForceAndTorque forceAndTorquePrediction = 
            CalculateAerodynamicForces(velocityPrediction, angularVelocityPrediction, Vector3.zero, 1.2f, rb.worldCenterOfMass);

        currentForceAndTorque = (forceAndTorqueThisFrame + forceAndTorquePrediction) * 0.5f;
        rb.AddForce(currentForceAndTorque.force);
        rb.AddTorque(currentForceAndTorque.torque);

        rb.AddForce(transform.forward * thrust * thrustPercent);
    }

    private ForceAndTorque CalculateAerodynamicForces(Vector3 velocity, Vector3 angularVelocity, Vector3 wind, float airDensity, Vector3 centerOfMass)
    {
        ForceAndTorque forceAndTorque = new ForceAndTorque();
        foreach (var surface in aerodynamicSurfaces)
        {
            Vector3 relativePosition = surface.transform.position - centerOfMass;
            forceAndTorque += surface.CalculateForces(-velocity + wind
                -Vector3.Cross(angularVelocity,
                relativePosition),
                airDensity, relativePosition);
        }
        return forceAndTorque;
    }

    private Vector3 PredictVelocity(Vector3 force)
    {
        return rb.velocity + Time.fixedDeltaTime * PREDICTION_TIMESTEP_FRACTION * force / rb.mass;
    }

    private Vector3 PredictAngularVelocity(Vector3 torque)
    {
        Quaternion inertiaTensorWorldRotation = rb.rotation * rb.inertiaTensorRotation;
        Vector3 torqueInDiagonalSpace = Quaternion.Inverse(inertiaTensorWorldRotation) * torque;
        Vector3 angularVelocityChangeInDiagonalSpace;
        angularVelocityChangeInDiagonalSpace.x = torqueInDiagonalSpace.x / rb.inertiaTensor.x;
        angularVelocityChangeInDiagonalSpace.y = torqueInDiagonalSpace.y / rb.inertiaTensor.y;
        angularVelocityChangeInDiagonalSpace.z = torqueInDiagonalSpace.z / rb.inertiaTensor.z;

        return rb.angularVelocity + Time.fixedDeltaTime * PREDICTION_TIMESTEP_FRACTION
            * (inertiaTensorWorldRotation * angularVelocityChangeInDiagonalSpace);
    }

#if UNITY_EDITOR
    public void CalculateCenterOfLift(out Vector3 center, out Vector3 force, Vector3 displayAirVelocity, float displayAirDensity, float pitch, float yaw, float roll)
    {
        Vector3 com;
        ForceAndTorque forceAndTorque;
        if (aerodynamicSurfaces == null)
        {
            center = Vector3.zero;
            force = Vector3.zero;
            return;
        }

        if (rb == null)
        {
            com = GetComponent<Rigidbody>().worldCenterOfMass;
            foreach (var surface in aerodynamicSurfaces)
            {
                surface.Initialize();
            }
            SetControlSurfecesAngles(pitch, yaw, roll);
            forceAndTorque = CalculateAerodynamicForces(-displayAirVelocity, Vector3.zero, Vector3.zero, displayAirDensity, com);
        }
        else
        {
            com = rb.worldCenterOfMass;
            forceAndTorque = currentForceAndTorque;
        }

        force = forceAndTorque.force;
        center = com + Vector3.Cross(forceAndTorque.force, forceAndTorque.torque) / forceAndTorque.force.sqrMagnitude;
    }
#endif
}

[System.Serializable]
public class ControlSurface
{
    public AeroSurface surface;
    public float flapAngle;
    public ControlSurfaceType type;
}

public enum ControlSurfaceType { Pitch, Yaw, Roll }
