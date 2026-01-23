using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform sword;         // The actual sword object
    public Transform handSocket;    // The empty object on the hand
    public Transform sheathSocket;  // The empty object on the hip

    // call this to put sword in hand
    public void DrawWeapon()
    {
        sword.SetParent(handSocket);
        sword.localPosition = Vector3.zero;
        sword.localRotation = Quaternion.identity; // Snaps to the socket's rotation
    }

    // call this to put sword on hip
    public void SheathWeapon()
    {
        sword.SetParent(sheathSocket);
        sword.localPosition = Vector3.zero;
        sword.localRotation = Quaternion.identity;
    }
}