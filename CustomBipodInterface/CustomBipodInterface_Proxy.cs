using UnityEngine;
using FistVR;

public class CustomBipodInterface_Proxy : MonoBehaviour
{
    public FVRFireArmAttachmentInterface FVRInterface;
    public FVRFireArmBipod Bipod;
    public Transform Object_to_rotate;
    public float rotation_duration;
    public Vector2 RotationRange = new Vector2(0f, 90f);

    private void Awake()
    {
        FistVR.CustomBipodInterface real = FistVR.CustomBipodInterface.CopyFromInterface(FVRInterface, this.gameObject);
        real.Bipod = Bipod;
        real.Object_to_rotate = Object_to_rotate;
        real.rotation_duration = rotation_duration;
        real.RotationRange = RotationRange;
        this.gameObject.SetActive(true);
    }
}