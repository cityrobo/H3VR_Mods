using UnityEngine;
using FistVR;
using System.Collections;

namespace Cityrobo
{
	public class PreattachedAttachment : MonoBehaviour
	{
		public FVRFireArmAttachment attachment;
		public FVRFireArmAttachmentMount mount;

		public void Start()
		{
			StartCoroutine("AttachToMount");
		}

		public IEnumerator AttachToMount()
		{
			yield return null;
			attachment.AttachToMount(mount, false);
		}
	}
}
