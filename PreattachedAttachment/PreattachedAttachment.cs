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
			attachment.curMount = mount;
			attachment.StoreAndDestroyRigidbody();
			if (attachment.curMount.GetRootMount().ParentToThis)
			{
				attachment.SetParentage(attachment.curMount.GetRootMount().transform);
			}
			else
			{
				attachment.SetParentage(attachment.curMount.MyObject.transform);
			}
			if (attachment.IsBiDirectional)
			{
				if (Vector3.Dot(attachment.transform.forward, attachment.curMount.transform.forward) >= 0f)
				{
					attachment.transform.rotation = attachment.curMount.transform.rotation;
				}
				else
				{
					attachment.transform.rotation = Quaternion.LookRotation(-attachment.curMount.transform.forward, attachment.curMount.transform.up);
				}
			}
			else
			{
				attachment.transform.rotation = attachment.curMount.transform.rotation;
			}
			attachment.transform.position = attachment.GetClosestValidPoint(attachment.curMount.Point_Front.position, attachment.curMount.Point_Rear.position, attachment.transform.position);
			if (attachment.curMount.Parent != null)
			{
				attachment.curMount.Parent.RegisterAttachment(attachment);
			}
			attachment.curMount.RegisterAttachment(attachment);
			if (attachment.curMount.Parent != null && attachment.curMount.Parent.QuickbeltSlot != null)
			{
				attachment.SetAllCollidersToLayer(false, "NoCol");
			}
			else
			{
				attachment.SetAllCollidersToLayer(false, "Default");
			}
			if (attachment.AttachmentInterface != null)
			{
				attachment.AttachmentInterface.OnAttach();
				attachment.AttachmentInterface.gameObject.SetActive(true);
			}
			attachment.SetTriggerState(false);
		}
	}
}
