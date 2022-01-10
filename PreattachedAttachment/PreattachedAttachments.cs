using UnityEngine;
using FistVR;
using System.Collections;

namespace Cityrobo
{
	public class PreattachedAttachments : MonoBehaviour
	{
		public FVRFireArmAttachment[] attachments;
		public FVRFireArmAttachmentMount mount;

#if !DEBUG
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
				if (attachment.GetType() == typeof(Suppressor))
				{
					Suppressor tempSup = attachment as Suppressor;
					tempSup.AutoMountWell();
				}
				yield return null;
			}
        }
#endif
	}
}
