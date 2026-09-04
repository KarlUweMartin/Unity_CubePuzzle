using UnityEngine;

public class SmoothRotationWobble : MonoBehaviour
{
    [Header("Rotation Ranges (Degrees)")]
    public float xRotationRange = 10f;
    public float yRotationRange = 10f;
    public float zRotationRange = 10f;

    [Header("Wobble Speed Multipliers")]
    public float xSpeed = 1f;
    public float ySpeed = 1.2f;
    public float zSpeed = 0.8f;

    private Vector3 initialRotation;
    private float timeOffsetX, timeOffsetY, timeOffsetZ;

    void Start()
    {
        initialRotation = transform.localEulerAngles;

        // Randomize phase offsets to make motion independent
        timeOffsetX = Random.Range(0f, 100f);
        timeOffsetY = Random.Range(0f, 100f);
        timeOffsetZ = Random.Range(0f, 100f);
    }

    void Update()
    {
        float time = Time.time;

        float xRotation = initialRotation.x + Mathf.Sin(time * xSpeed + timeOffsetX) * xRotationRange;
        float yRotation = initialRotation.y + Mathf.Sin(time * ySpeed + timeOffsetY) * yRotationRange;
        float zRotation = initialRotation.z + Mathf.Sin(time * zSpeed + timeOffsetZ) * zRotationRange;

        transform.localEulerAngles = new Vector3(xRotation, yRotation, zRotation);
    }
}
