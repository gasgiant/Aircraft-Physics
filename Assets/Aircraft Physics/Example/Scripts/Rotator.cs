using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float speed;

    private void Update()
    {
        transform.localRotation *= Quaternion.AngleAxis(speed * Time.deltaTime , Vector3.up);
    }
}
