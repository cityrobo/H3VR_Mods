using UnityEngine;
using FistVR;

public class Magazine_Tape_Proxy : MonoBehaviour
{
    public FVRFireArmMagazine Magazine_1;
    public FVRFireArmMagazine Magazine_2;
    public FVRPhysicalObject PhysicalObject;

    private void Awake()
    {
        FistVR.Magazine_Tape real = FistVR.Magazine_Tape.CopyFromObject(PhysicalObject, this.gameObject);
        real.Magazine_1 = Magazine_1;
        real.Magazine_2 = Magazine_2;
        this.gameObject.SetActive(true);      
    }
}
