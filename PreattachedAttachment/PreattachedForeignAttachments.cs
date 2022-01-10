using System;
using UnityEngine;
using FistVR;
using System.Collections;
using System.Collections.Generic;
namespace Cityrobo
{
	public class PreattachedForeignAttachments : MonoBehaviour
	{
		public FVRFireArmAttachmentMount mount;
		public List<ItemCallerSet> sets;


		private List<FVRFireArmAttachment> attachments;

#if !DEBUG
		public void Start()
		{
			attachments = new List<FVRFireArmAttachment>();
			SpawnAttachments();
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
		public void SpawnAttachments()
        {
			GameObject gameObject;
			FVRFireArmAttachment spawned_attachment;
			FVRObject obj;
			foreach (var set in sets)
			{
				gameObject = null;
				spawned_attachment = null;
				obj = null;
				try
				{
					obj = IM.OD[set.primaryItemID];
					gameObject = Instantiate(obj.GetGameObject(), set.attachmentPoint.position, set.attachmentPoint.rotation);
					spawned_attachment = gameObject.GetComponent<FVRFireArmAttachment>();
					attachments.Add(spawned_attachment);
				}
				catch
				{
					try
					{
						Debug.Log($"Item ID {set.primaryItemID} not found; attempting to spawn backupID");
						obj = IM.OD[set.backupID];
						gameObject = Instantiate(obj.GetGameObject(), set.attachmentPoint.position, set.attachmentPoint.rotation);
						spawned_attachment = gameObject.GetComponent<FVRFireArmAttachment>();
						attachments.Add(spawned_attachment);
					}
					catch
					{
						Debug.Log($"Item ID {set.backupID} not found; Continuing load with next object in list!");
					}
				}
			}

		}
#endif
	}

	[Serializable]
	public class ItemCallerSet
	{
		public string primaryItemID;
		[Tooltip("If your item fails to spawn, it will spawn the backup ID.")]
		public string backupID;
		[Tooltip("Position and Rotation to spawn the Attachment at.")]
		public Transform attachmentPoint;
	}
}
