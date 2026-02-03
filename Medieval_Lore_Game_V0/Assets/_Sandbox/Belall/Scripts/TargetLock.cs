using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Required for Image colors

public class TargetLock : MonoBehaviour
{
    public Transform currentTarget;
    public float scanRadius = 20f;
    public bool isLocked = false;

    [Header("UI Reference")]
    public Image crosshairImage; // <--- The new slot for your UI

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        // Check Middle Mouse Button
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            if (isLocked) Unlock();
            else ScanForTarget();
        }

        if (isLocked)
        {
            if (currentTarget == null) Unlock(); // Target died
            else LockOnLogic();
        }
    }

    void ScanForTarget()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, scanRadius);
        float bestAngle = -1f;
        Transform bestTarget = null;

        foreach (Collider col in enemies)
        {
            if (col.CompareTag("Enemy"))
            {
                Vector3 dirToEnemy = (col.transform.position - mainCam.transform.position).normalized;
                float angle = Vector3.Dot(mainCam.transform.forward, dirToEnemy);

                if (angle > 0.5f && angle > bestAngle)
                {
                    bestAngle = angle;
                    bestTarget = col.transform;
                }
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            isLocked = true;
            // Turn RED when locked
            if (crosshairImage != null) crosshairImage.color = Color.red;
        }
    }

    void Unlock()
    {
        isLocked = false;
        currentTarget = null;
        // Turn WHITE when unlocked
        if (crosshairImage != null) crosshairImage.color = Color.white;
    }

    void LockOnLogic()
    {
        // Rotate Player
        Vector3 dirToTarget = currentTarget.position - transform.position;
        dirToTarget.y = 0;
        Quaternion lookRot = Quaternion.LookRotation(dirToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);

        // Rotate Camera
        Vector3 targetCenter = currentTarget.position + Vector3.up * 1.5f;
        Vector3 cameraDir = targetCenter - mainCam.transform.position;
        Quaternion camLookRot = Quaternion.LookRotation(cameraDir);
        mainCam.transform.rotation = Quaternion.Slerp(mainCam.transform.rotation, camLookRot, Time.deltaTime * 5f);
    }
}