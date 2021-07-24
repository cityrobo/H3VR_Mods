using UnityEngine;
using FistVR;

namespace Cityrobo
{
	public class PreattachedAttachment : MonoBehaviour
	{
		public FVRFireArmAttachment attachment;
		public FVRFireArmAttachmentMount mount;

		public void Start()
		{
			attachment.AttachToMount(mount, false);
		}
	}
}
