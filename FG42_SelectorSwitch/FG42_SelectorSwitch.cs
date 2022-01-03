using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FistVR;

namespace Cityrobo
{
    public class FG42_SelectorSwitch : MonoBehaviour
    {
        public OpenBoltReceiver weapon;
        
        public int semiAuto;
        public int fullAuto;

        public Transform closedBoltSearPosition;

        private OpenBoltReceiverBolt bolt;
        private Transform sear;
        private Vector3 uncockedPos;
        private Transform openBoltSearPosition;
        private string lastMessage = "";

        private enum BoltState
        {
            semiAuto,
            fullAuto,
            safe,
            uncocked
        }

        private bool waitForShot = false;
        BoltState boltState = new BoltState();

#if !DEBUG
        public void Start()
        {
            bolt = weapon.Bolt;
            sear = weapon.Bolt.Point_Bolt_LockPoint;

            uncockedPos = weapon.Bolt.Point_Bolt_Forward.localPosition;
            openBoltSearPosition = sear;

            //DebugOnce(uncockedPos.ToString());
            //DebugOnce(openBoltSearPosition.localPosition.ToString());
            //DebugOnce(closedBoltSearPosition.localPosition.ToString());
        }

        public void Update()
        {
            if (bolt.transform.localPosition == uncockedPos) boltState = BoltState.uncocked;
            else if (bolt.transform.localPosition == openBoltSearPosition.localPosition) boltState = BoltState.fullAuto;
            else if (bolt.transform.localPosition == closedBoltSearPosition.localPosition) boltState = BoltState.semiAuto;


            if (boltState == BoltState.uncocked && weapon.m_fireSelectorMode == fullAuto)
            {
                //DebugOnce("Moved sear to Full Auto Position");
                bolt.m_boltZ_lock = openBoltSearPosition.localPosition.z;
            }
            else if (boltState == BoltState.uncocked && weapon.m_fireSelectorMode == semiAuto)
            {
                //DebugOnce("Moved sear to Semi Auto Position");
                bolt.m_boltZ_lock = closedBoltSearPosition.localPosition.z;
            }
            else if (boltState == BoltState.semiAuto && weapon.m_fireSelectorMode == fullAuto)
            {
                waitForShot = true;
            }
            else if (boltState == BoltState.fullAuto && weapon.m_fireSelectorMode == semiAuto)
            {
                //DebugOnce("Moved sear to Semi Auto Position from Full Auto Position");
                bolt.m_boltZ_lock = closedBoltSearPosition.localPosition.z;
                bolt.LastPos = OpenBoltReceiverBolt.BoltPos.Rear;
                bolt.CurPos = OpenBoltReceiverBolt.BoltPos.LockedToRear;
            }


            if (waitForShot)
            {
                //DebugOnce("waiting for shot");
                if (bolt.LastPos == OpenBoltReceiverBolt.BoltPos.Forward)
                {
                    bolt.m_boltZ_lock = openBoltSearPosition.localPosition.z;
                    waitForShot = false;
                }
            }



        }

        public void DebugOnce(string message)
        {
            if (message != lastMessage)
            {
                Debug.Log(message);
            }
            lastMessage = message;

        }
#endif
    }
}
