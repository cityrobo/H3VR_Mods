using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class SmartPistol : MonoBehaviour
    {
        public Handgun pistol;
		public MeshRenderer reticle;
		public float EngageRange = 15f;
		[Range(1f,179f)]
		public float EngageAngle = 45f;
		public float PrecisionAngle = 5f;

		public LayerMask LatchingMask;
		public LayerMask BlockingMask;

		//constants
		private string nameOfDistanceVariable = "_RedDotDist";

#if !DEBUG
		public void Update()
        {
            if (pistol.m_hand != null)
            {
				Vector3 target = FindTarget();

				if (target != new Vector3(0, 0, 0))
                {
                    Debug.Log(target);

					pistol.m_isSafetyEngaged = false;
					//Debug.DrawRay(pistol.MuzzlePos.position, target, Color.green);
					pistol.MuzzlePos.LookAt(target);
                    if (reticle != null)
                    {
						reticle.material.SetFloat(nameOfDistanceVariable, (target - pistol.MuzzlePos.position).magnitude);
					}
                }
				else
                {
					pistol.m_isSafetyEngaged = true;
					pistol.MuzzlePos.localEulerAngles = new Vector3(0, 0, 0);
				}
            }
        }
#endif
		private Vector3 FindTarget()
        {
			float radius = EngageRange * Mathf.Tan(0.5f * EngageAngle * Mathf.Deg2Rad);
			Collider[] array = Physics.OverlapCapsule(pistol.MuzzlePos.position, pistol.MuzzlePos.position + pistol.transform.forward * this.EngageRange, radius, this.LatchingMask);
			List<Rigidbody> list = new List<Rigidbody>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].attachedRigidbody != null && !list.Contains(array[i].attachedRigidbody))
				{
					list.Add(array[i].attachedRigidbody);
				}
			}
			bool flag = false;
			SosigLink sosigLink = null;
			SosigLink sosigLink2 = null;
			float num = EngageAngle;
			for (int j = 0; j < list.Count; j++)
			{
				SosigLink component = list[j].GetComponent<SosigLink>();
				if (!(component == null))
				{
					if (component.S.BodyState != Sosig.SosigBodyState.Dead)
					{
						if (true || component.S.E.IFFCode == 1)
						{
							Vector3 from = list[j].transform.position - pistol.MuzzlePos.position;
							float num2 = Vector3.Angle(from, pistol.transform.forward);

							Sosig s = component.S;
							if (num2 <= PrecisionAngle) sosigLink2 = s.Links[0];
							else sosigLink2 = s.Links[1];


							if (num2 < num &&  !Physics.Linecast(pistol.MuzzlePos.position, sosigLink2.transform.position, this.BlockingMask, QueryTriggerInteraction.Ignore))
							{
								sosigLink = sosigLink2;
								num = num2;
								flag = true;
							}
						}
					}
				}
			}
			if (flag)
			{
				return sosigLink.transform.position; ;
			}
            else
            {
				return new Vector3(0, 0, 0);
            }
		}

    }
}
