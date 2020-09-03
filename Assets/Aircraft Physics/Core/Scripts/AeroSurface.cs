using System;
using UnityEngine;

public class AeroSurface : MonoBehaviour
{
    [SerializeField] AeroSurfaceConfig config = null;

    public AeroSurfaceConfig Config => config;
    public float FlapAngle => flapAngle;
    public Vector3 CurrentLift { get; private set; }
    public Vector3 CurrentDrag { get; private set; }
    public Vector3 CurrentTorque { get; private set; }
    public bool IsAtStall { get; private set; }

    float flapAngle;

    float area;
    float correctedLiftSlope;
    float zeroLiftAoA;
    float stallAngleHigh;
    float stallAngleLow;
    float fullStallAngleHigh;
    float fullStallAngleLow;

    bool initialized;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        area = config.chord * config.span;
        correctedLiftSlope = config.liftSlope * config.aspectRatio /
           (config.aspectRatio + 2 * (config.aspectRatio + 4) / (config.aspectRatio + 2));
        SetFlapAngle(0);
        initialized = true;
    }

    public void SetFlapAngle(float angle)
    {
        if (!gameObject.activeInHierarchy || config == null) return;

        flapAngle = Mathf.Clamp(angle, -Mathf.Deg2Rad * 50, Mathf.Deg2Rad * 50);

        float theta = Mathf.Acos(2 * config.flapFraction - 1);
        float flapEffectivness = 1 - (theta - Mathf.Sin(theta)) / Mathf.PI;
        float deltaLift = correctedLiftSlope * flapEffectivness * FlapEffectivnessCorrection(flapAngle) * flapAngle;

        float zeroLiftAoaBase = config.zeroLiftAoA * Mathf.Deg2Rad;
        zeroLiftAoA = zeroLiftAoaBase - deltaLift / correctedLiftSlope;

        float stallAngleHighBase = config.stallAngleHigh * Mathf.Deg2Rad;
        float stallAngleLowBase = config.stallAngleLow * Mathf.Deg2Rad;

        float clMaxHigh = correctedLiftSlope * (stallAngleHighBase - zeroLiftAoaBase) + deltaLift * LiftCoefficientMaxFraction(config.flapFraction);
        float clMaxLow = correctedLiftSlope * (stallAngleLowBase - zeroLiftAoaBase) + deltaLift * LiftCoefficientMaxFraction(config.flapFraction);

        stallAngleHigh = zeroLiftAoA + clMaxHigh / correctedLiftSlope;
        stallAngleLow = zeroLiftAoA + clMaxLow / correctedLiftSlope;

        float blendAngle = Mathf.Deg2Rad * Mathf.Lerp(8, 14, Mathf.Abs(Mathf.Abs(Mathf.Rad2Deg * flapAngle) - 50) / 50);
        fullStallAngleHigh = stallAngleHigh + blendAngle;
        fullStallAngleLow = stallAngleLow - blendAngle;
    }

    public BiVector3 CalculateForces(Vector3 worldAirVelocity, float airDensity, Vector3 relativePosition)
    {
        BiVector3 forceAndTorque = new BiVector3();
        if (!gameObject.activeInHierarchy || config == null) return forceAndTorque;

        if (!initialized) Initialize();

        Vector3 airVelocity = transform.InverseTransformDirection(worldAirVelocity);
        airVelocity = new Vector3(airVelocity.x, airVelocity.y);

        Vector3 dragDirection = transform.TransformDirection(airVelocity.normalized);
        Vector3 liftDirection = Vector3.Cross(dragDirection, transform.forward);

        float dynamicPressure = 0.5f * airDensity * airVelocity.sqrMagnitude;
        float angleOfAttack = Mathf.Atan2(airVelocity.y, -airVelocity.x);


        IsAtStall = !(angleOfAttack < stallAngleHigh && angleOfAttack > stallAngleLow);
        Vector3 aerodynamicCoefficients = CalculateCoefficients(angleOfAttack);
        CurrentLift = liftDirection * aerodynamicCoefficients.x * dynamicPressure * area;
        CurrentDrag = dragDirection * aerodynamicCoefficients.y * dynamicPressure * area;
        CurrentTorque = -transform.forward * aerodynamicCoefficients.z * dynamicPressure * area * config.chord;

        forceAndTorque.p += CurrentDrag + CurrentLift;
        forceAndTorque.q += Vector3.Cross(relativePosition, forceAndTorque.p);
        forceAndTorque.q += CurrentTorque;

        return forceAndTorque;
    }

    private Vector3 CalculateCoefficients(float angleOfAttack)
    {
        Vector3 aerodynamicCoefficients;
        if (angleOfAttack < stallAngleHigh && angleOfAttack > stallAngleLow)
        {
            aerodynamicCoefficients = CalculateCoefficientsAtLowAoA(angleOfAttack);
        }
        else
        {
            if (angleOfAttack > fullStallAngleHigh || angleOfAttack < fullStallAngleLow)
            {
                aerodynamicCoefficients = CalculateCoefficientsAtStall(angleOfAttack);
            }
            else
            {
                Vector3 aerodynamicCoefficientsLow;
                Vector3 aerodynamicCoefficientsStall;
                float lerpParam;

                if (angleOfAttack > stallAngleHigh)
                {
                    aerodynamicCoefficientsLow = CalculateCoefficientsAtLowAoA(stallAngleHigh);
                    aerodynamicCoefficientsStall = CalculateCoefficientsAtStall(fullStallAngleHigh);
                    lerpParam = (angleOfAttack - stallAngleHigh) / (fullStallAngleHigh - stallAngleHigh);
                }
                else
                {
                    aerodynamicCoefficientsLow = CalculateCoefficientsAtLowAoA(stallAngleLow);
                    aerodynamicCoefficientsStall = CalculateCoefficientsAtStall(fullStallAngleLow);
                    lerpParam = (angleOfAttack - stallAngleLow) / (fullStallAngleLow - stallAngleLow);
                }
                aerodynamicCoefficients = Vector3.Lerp(aerodynamicCoefficientsLow, aerodynamicCoefficientsStall, lerpParam);
            }
        }
        return aerodynamicCoefficients;
    }

    private Vector3 CalculateCoefficientsAtLowAoA(float angleOfAttack)
    {
        float liftCoefficient = correctedLiftSlope * (angleOfAttack - zeroLiftAoA);
        float inducedAngle = liftCoefficient / (Mathf.PI * config.aspectRatio);
        float effectiveAngle = angleOfAttack - zeroLiftAoA - inducedAngle;

        float tangentialCoefficient = config.skinFriction * Mathf.Cos(effectiveAngle);
        
        float normalCoefficient = (liftCoefficient +
            Mathf.Sin(effectiveAngle) * tangentialCoefficient) / Mathf.Cos(effectiveAngle);
        float dragCoefficient = normalCoefficient * Mathf.Sin(effectiveAngle) + tangentialCoefficient * Mathf.Cos(effectiveAngle);
        float torqueCoefficient = -normalCoefficient * TorqCoefficientProportion(effectiveAngle);

        return new Vector3(liftCoefficient, dragCoefficient, torqueCoefficient);
    }

    private Vector3 CalculateCoefficientsAtStall(float angleOfAttack)
    {
        float liftCoefficientLowAoA;
        if (angleOfAttack > stallAngleHigh)
        {
            liftCoefficientLowAoA = correctedLiftSlope * (stallAngleHigh - zeroLiftAoA);
        }
        else
        {
            liftCoefficientLowAoA = correctedLiftSlope * (stallAngleLow - zeroLiftAoA);
        }
        float inducedAngle = liftCoefficientLowAoA / (Mathf.PI * config.aspectRatio);

        float lerpParam;
        if (angleOfAttack > stallAngleHigh)
        {
            lerpParam = (Mathf.PI / 2 - Mathf.Clamp(angleOfAttack, -Mathf.PI / 2, Mathf.PI / 2))
                / (Mathf.PI / 2 - stallAngleHigh);
        }
        else
        {
            lerpParam = (-Mathf.PI / 2 - Mathf.Clamp(angleOfAttack, -Mathf.PI / 2, Mathf.PI / 2))
                / (-Mathf.PI / 2 - stallAngleLow);
        }
        inducedAngle = Mathf.Lerp(0, inducedAngle, lerpParam);
        float effectiveAngle = angleOfAttack - zeroLiftAoA - inducedAngle;
        

        float normalCoefficient = FrictionAt90Degrees() * Mathf.Sin(effectiveAngle) *
            (1 / (0.56f + 0.44f * Mathf.Abs(Mathf.Sin(effectiveAngle))) -
            0.41f * (1 - Mathf.Exp(-17 / config.aspectRatio)));
        float tangentialCoefficient = 0.5f * config.skinFriction * Mathf.Cos(effectiveAngle);

        float liftCoefficient = normalCoefficient * Mathf.Cos(effectiveAngle) - tangentialCoefficient * Mathf.Sin(effectiveAngle);
        float dragCoefficient = normalCoefficient * Mathf.Sin(effectiveAngle) + tangentialCoefficient * Mathf.Cos(effectiveAngle);
        float torqueCoefficient = -normalCoefficient * TorqCoefficientProportion(effectiveAngle);

        return new Vector3(liftCoefficient, dragCoefficient, torqueCoefficient);
    }

    private float TorqCoefficientProportion(float effectiveAngle)
    {
        return 0.25f - 0.175f * (1 - 2 * Mathf.Abs(effectiveAngle) / Mathf.PI);
    }

    private float FrictionAt90Degrees()
    {
        return 1.98f - 4.26e-2f * flapAngle * flapAngle + 2.1e-1f * flapAngle;
    }

    private float FlapEffectivnessCorrection(float flapAngle)
    {
        return Mathf.Lerp(0.8f, 0.4f, (flapAngle * Mathf.Rad2Deg - 10) / 50);
    }

    private float LiftCoefficientMaxFraction(float flapFraction)
    {
        return Mathf.Clamp01(1 - 0.5f * (flapFraction - 0.1f) / 0.3f);
    }
}
