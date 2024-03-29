using UnityEngine;
using FistVR;
using System.Collections;

namespace Cityrobo
{
	public class PreattachedAttachment : MonoBehaviour
	{
		public FVRFireArmAttachment attachment;
		public FVRFireArmAttachmentMount mount;
#if!DEBUG
		public void Awake()
		{
            gameObject.SetActive(false);
            OpenScripts2.PreattachedAttachment newComponent = gameObject.AddComponent<OpenScripts2.PreattachedAttachment>();
			newComponent.Attachment = attachment;
			newComponent.AttachmentMount = mount;
            gameObject.SetActive(true);

            Destroy(this);
        }

		//public void Start()
		//{
		//	StartCoroutine("AttachToMount");
		//}

		//public IEnumerator AttachToMount()
		//{
		//	yield return null;
		//	attachment.AttachToMount(mount, false);
  //          if (attachment.GetType() == typeof(Suppressor))
  //          {
		//		Suppressor tempSup = attachment as Suppressor;
		//		tempSup.AutoMountWell();
  //          }
		//}
#endif
	}
}
