using System.Collections;
using UnityEngine;
using FistVR;

namespace Cityrobo
{
    [RequireComponent(typeof(ThermalBody))]
    public class HeatingUpAttachmentPart : MonoBehaviour
    {
        public FVRFireArmAttachment attachment;
        public float heatPerShot;
        public float heatDissipatedPerSecond;

        private FVRFireArmChamber chamber = null;

        private bool chamberLoaded = false;
        private float heat;

        private ThermalBody tB;
        private float tbMaxTemp;
        private float tbMinTemp;

        private bool logOnce = false;
#if!DEBUG
        void Start()
        {
            if (attachment == null) attachment = this.gameObject.GetComponent<FVRFireArmAttachment>();

            tB = this.gameObject.GetComponent<ThermalBody>();
            tB.enabled = false;
            tB.isVariable = true;
            tB.enabled = true;
            tbMaxTemp = tB.MaximumTemperature;
            tbMinTemp = tB.MinimumTemperature;
            heat = tbMaxTemp;
        }
        void Update()
        {
            if (heat > tbMaxTemp)
            {
                tB.MaximumTemperature = heat;
                tB.UpdateSubMaterialProperties();
                heat -= Time.deltaTime * heatDissipatedPerSecond;
                if (heat < tbMaxTemp) heat = tbMaxTemp;
                if (heat > 1f)
                {
                    tB.MinimumTemperature = heat - 1f + tbMinTemp;
                }
                else tB.MinimumTemperature = tbMinTemp;
            }
            if (attachment.curMount != null) getChamber(attachment.curMount.GetRootMount().MyObject as FVRFireArm);
            if (chamber != null)
            {
                if (!chamberLoaded && chamber.m_round != null && !chamber.m_round.IsSpent)
                {
                    chamberLoaded = true;
                }
                else if (chamberLoaded && ((chamber.m_round != null && chamber.m_round.IsSpent) || chamber.m_round == null))
                {
                    heat += heatPerShot;

                    chamberLoaded = false;
                }
            }
        }

        void getChamber(FVRFireArm fireArm)
        {
            if (fireArm == null)
            {
                chamber = null;
                return;
            }
            switch (fireArm)
            {
                case ClosedBoltWeapon w:
                    chamber = w.Chamber;
                    break;
                case OpenBoltReceiver w:
                    chamber = w.Chamber;
                    break;
                case Handgun w:
                    chamber = w.Chamber;
                    break;
                default:
                    if (!logOnce)Debug.LogError("Error in HeatingUpAttachmentPart script! Weapon type not yet supported!");
                    chamber = null;
                    logOnce = true;
                    return;
            }
            logOnce = false;
        }
#endif
    }
}