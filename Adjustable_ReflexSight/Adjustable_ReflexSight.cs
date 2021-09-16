using FistVR;
using UnityEngine;
using UnityEngine.UI;

using Mono.Cecil;


namespace Cityrobo

{
    public class Adjustable_ReflexSight : MonoBehaviour
    {
        public FVRFireArmAttachment attachment;
        [Tooltip("This can be an AttachmentInterface or a standalone FVRInteractiveObject set to \" is simple interact \" ")]
        public FVRInteractiveObject reflexSightInterface;
        public MeshRenderer reticle;

        [Tooltip("Switch that moves with the selected texture")]
        public Transform buttonSwitch;

        public Vector3[] switchPositions;

        public Texture2D[] textures;

        public int currentTexture = 0;

        [Header("If you want a Screen above the scope that shows stuff, use this:")]
        public Transform textFrame;
        public Text reticleTextScreen;
        public Text zeroTextScreen;

        public string[] reticleText;

        [Tooltip("Index of the Array below, not the actual value")]
        public int currentZeroDistance = 3;
        public float[] zeroDistances = new float[7] { 2, 5, 10, 15, 25, 50, 100 };
        //public float dotSizeAt100mDistance = 2;

        private FVRViveHand hand;
        private int currentMenu = 0;

        private string nameOfTexture = "_RedDotTex";
        private string nameOfDistanceVariable = "_RedDotDist";
        private string nameOfXOffset = "_MuzzleOffsetX";
        private string nameOfYOffset = "_MuzzleOffsetY";
        //private string nameOfDotSizeVariable = "_RedDotSize";

        private Transform muzzlePos;

        private bool attached = false;
        public void Start()
        {
            if (currentTexture >= textures.Length) currentTexture = 0;
            reticle.material.SetTexture(nameOfTexture, textures[currentTexture]);
            reticle.material.SetFloat(nameOfDistanceVariable, zeroDistances[currentZeroDistance]);
            //lens.material.SetFloat(nameOfDotSizeVariable, dotSizeAt100mDistance * (zeroDistances[currentZeroDistance] / 100) );

            if (buttonSwitch != null) buttonSwitch.localPosition = switchPositions[currentTexture];

            if (reflexSightInterface.IsSimpleInteract) Hook();

            UpdateScreen();
        }
#if !(UNITY_EDITOR || UNITY_5)
        public void OnDestroy()
        {
            Unhook();
        }

        public void Update()
        {
            if (!reflexSightInterface.IsSimpleInteract)
            {
                hand = reflexSightInterface.m_hand;
                if (hand != null)
                {
                    if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f && currentMenu == 0) UsePreviousTexture();
                    else if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f && currentMenu == 0) UseNextTexture();
                    if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f && currentMenu == 1) UsePreviousZeroDistance();
                    else if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f && currentMenu == 1) UseNextZeroDistance();
                    else if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f) ShowNextMenu();
                }
            }
            if (attachment.curMount != null && !attached)
            {
                attached = true;
                FVRFireArm fireArm;
                try
                {
                    fireArm = (FVRFireArm)attachment.curMount.GetRootMount().MyObject;
                    muzzlePos = fireArm.MuzzlePos;

                    Vector3 muzzleOffset = muzzlePos.InverseTransformPoint(reticle.transform.position);
                    Debug.Log(muzzleOffset.x);
                    Debug.Log(muzzleOffset.y);
                    Debug.Log(muzzleOffset.z);

                    reticle.material.SetFloat(nameOfXOffset,  - muzzleOffset.x);
                    reticle.material.SetFloat(nameOfYOffset,  - muzzleOffset.y);

                }
                catch (System.Exception)
                {
                    Debug.Log("MyObject not type FVRFireArm");
                }
            }
            else if (attachment.curMount == null && attached)
            {
                attached = false;
            }
        }

        private void UseNextTexture()
        {
            currentTexture = (currentTexture + 1) % textures.Length;

            reticle.material.SetTexture(nameOfTexture, textures[currentTexture]);
            if (buttonSwitch != null) buttonSwitch.localPosition = switchPositions[currentTexture];
            UpdateScreen();
        }

        private void UsePreviousTexture()
        {
            currentTexture = (currentTexture + textures.Length - 1) % textures.Length;

            reticle.material.SetTexture(nameOfTexture, textures[currentTexture]);
            if (buttonSwitch != null) buttonSwitch.localPosition = switchPositions[currentTexture];
            UpdateScreen();
        }

        private void ShowNextMenu() 
        {
            currentMenu = (currentMenu + 1) % 2;
            UpdateScreen();
        }

        private void UpdateScreen()
        {

            if (reticleTextScreen != null && currentMenu == 0)
            {
                textFrame.localPosition = reticleTextScreen.transform.localPosition;
                reticleTextScreen.text = "Reticle: " + reticleText[currentTexture];
            }
            if (zeroTextScreen != null && currentMenu == 1)
            {
                textFrame.localPosition = zeroTextScreen.transform.localPosition;
                zeroTextScreen.text = "Zero Distance: " + zeroDistances[currentZeroDistance] + "m";
            }
            
        }

        private void UseNextZeroDistance()
        {
            if (currentZeroDistance < zeroDistances.Length) currentZeroDistance++;
            reticle.material.SetFloat(nameOfDistanceVariable, zeroDistances[currentZeroDistance]);
            //lens.material.SetFloat(nameOfDotSizeVariable, dotSizeAt100mDistance * (zeroDistances[currentZeroDistance] / 100));
            UpdateScreen();
        }

        private void UsePreviousZeroDistance()
        {
            if (currentZeroDistance > 0) currentZeroDistance--;
            reticle.material.SetFloat(nameOfDistanceVariable, zeroDistances[currentZeroDistance]);
            //lens.material.SetFloat(nameOfDotSizeVariable, dotSizeAt100mDistance * (zeroDistances[currentZeroDistance] / 100));
            UpdateScreen();
        }

        private void Hook()
        {
#if !DEBUG
            On.FistVR.FVRInteractiveObject.SimpleInteraction += FVRInteractiveObject_SimpleInteraction;
#endif
        }

        private void Unhook()
        {
#if !DEBUG
            On.FistVR.FVRInteractiveObject.SimpleInteraction -= FVRInteractiveObject_SimpleInteraction;
#endif
        }
#if !DEBUG
        private void FVRInteractiveObject_SimpleInteraction(On.FistVR.FVRInteractiveObject.orig_SimpleInteraction orig, FVRInteractiveObject self, FVRViveHand hand)
        {
            orig(self, hand);
            if (self == reflexSightInterface) UseNextTexture();
        }
#endif
#endif
    }
}
