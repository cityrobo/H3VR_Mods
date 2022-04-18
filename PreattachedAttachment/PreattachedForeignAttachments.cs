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
		public string[] primaryItemIDs;
		[Tooltip("If your item fails to spawn, it will spawn the backup ID.")]
		public string[] backupIDs;
		[Tooltip("Position and Rotation to spawn the Attachment at.")]
		public Transform[] attachmentPoints;

		private List<FVRFireArmAttachment> attachments;
		private List<ItemCallerSet> _sets;

#if !DEBUG
		public void Start()
		{
			attachments = new List<FVRFireArmAttachment>();
			_sets = new List<ItemCallerSet>();
			for (int i = 0; i < primaryItemIDs.Length; i++)
			{
				_sets.Add(new ItemCallerSet(primaryItemIDs[i], backupIDs[i], attachmentPoints[i]));
				//Debug.Log(string.Format("Added to Sets: {0}/{1} at position {2}.", _sets[i].primaryItemID, _sets[i].backupID, _sets[i].attachmentPoint));
			}
            
            SpawnAttachments();
			StartCoroutine("AttachAllToMount");
		}

		public IEnumerator AttachAllToMount()
        {
			yield return null;
			
            foreach (var attachment in attachments)
            {
                //Debug.Log("Attaching: " + attachment.name);

                attachment.AttachToMount(mount, false);
				if (attachment is Suppressor)
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
			foreach (var set in _sets)
			{
				gameObject = null;
				spawned_attachment = null;
				obj = null;
				try
				{
					obj = IM.OD[set.primaryItemID];
					gameObject = Instantiate(obj.GetGameObject(), set.attachmentPoint.position, set.attachmentPoint.rotation);
					spawned_attachment = gameObject.GetComponent<FVRFireArmAttachment>();
                    //Debug.Log("Spawned: " + spawned_attachment.name);

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
						//Debug.Log("Spawned: " + spawned_attachment.name);
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
		public class ItemCallerSet
		{
			public ItemCallerSet(string primaryItemID, string backupID, Transform attachmentPoint)
			{
				this.primaryItemID = primaryItemID;
				this.backupID = backupID;
				this.attachmentPoint = attachmentPoint;
			}

			public string primaryItemID;
			[Tooltip("If your item fails to spawn, it will spawn the backup ID.")]
			public string backupID;
			[Tooltip("Position and Rotation to spawn the Attachment at.")]
			public Transform attachmentPoint;
		}
	}
}
