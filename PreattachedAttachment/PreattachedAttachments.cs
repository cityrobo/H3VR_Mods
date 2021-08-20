using UnityEngine;
using FistVR;
using System.Collections;

namespace Cityrobo
{
	public class PreattachedAttachments : MonoBehaviour
	{
		public FVRFireArmAttachment[] attachments;
		public FVRFireArmAttachmentMount mount;

		public void Start()
		{
			StartCoroutine("AttachAllToMount");
		}

		public IEnumerator AttachAllToMount()
        {
			yield return null;
			foreach (var attachment in attachments)
            {
				attachment.AttachToMount(mount, false);

				yield return null;
			}
        }
	}
}
