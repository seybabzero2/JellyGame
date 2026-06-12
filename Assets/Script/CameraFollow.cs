using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0f, 2f, -4f);
    public float smoothPositionTime = 0.15f;
    public float smoothRotationTime = 0.15f;

    [SerializeField] private Transform target;

    [Header("Juicy Shake Settings")]
    public float constantIdlingShake = 0.03f; 
    public float shakeFrequency = 15f;        

    private Vector3 positionVelocity = Vector3.zero;
    private float rotationVelocity;
    
    private float shakeTargetIntensity = 0f;
    private float shakeCurrentIntensity = 0f;
    private float shakeDuration = 0f;
    private float shakeTimer = 0f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.TransformPoint(offset);
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, smoothPositionTime);

        if (shakeTimer < shakeDuration)
        {
            shakeTimer += Time.deltaTime;
            shakeCurrentIntensity = Mathf.Lerp(shakeTargetIntensity, 0f, shakeTimer / shakeDuration);
        }
        else
        {
            shakeCurrentIntensity = 0f;
        }

        float totalShake = constantIdlingShake + shakeCurrentIntensity;

        float shakeX = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * totalShake;
        float shakeY = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * totalShake;
        Vector3 shakeOffset = new Vector3(shakeX, shakeY, 0f);

        transform.position = smoothedPosition + transform.TransformDirection(shakeOffset);

        float targetAngle = target.eulerAngles.y;
        float currentAngle = transform.eulerAngles.y;

        currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref rotationVelocity, smoothRotationTime);

        transform.rotation = Quaternion.Euler(15f, currentAngle, 0f);
    }

    public void AddShakeImpulse(float intensity, float durationInSeconds)
    {
        shakeTargetIntensity = intensity;
        shakeDuration = durationInSeconds;
        shakeTimer = 0f; 
    }
}