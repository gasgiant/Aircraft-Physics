using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AirplaneController : MonoBehaviour
{
    [SerializeField]
    float rollControlSensitivity = 0.2f;
    [SerializeField]
    float pitchControlSensitivity = 0.2f;
    [SerializeField]
    float yawControlSensitivity = 0.2f;


    Rigidbody rb;
    float pitch;
    float yaw;
    float roll;

    float thrustPercent;

    AircraftPhysics aircraftPhysics;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        aircraftPhysics = GetComponent<AircraftPhysics>();
        SetThrust(1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }
    }

    private void SetThrust(float percent)
    {
        thrustPercent = Mathf.Clamp01(percent);
    }

    private void FixedUpdate()
    {
        pitch = pitchControlSensitivity * Input.GetAxis("Vertical");
        roll = rollControlSensitivity * Input.GetAxis("Horizontal");
        aircraftPhysics.SetControlSurfecesAngles(pitch, yaw, roll);
        aircraftPhysics.SetThrustPercent(thrustPercent);
    }
}
