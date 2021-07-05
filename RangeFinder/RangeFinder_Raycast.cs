using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RangeFinder_Raycast : MonoBehaviour {

	public Transform direction;
	public LayerMask layerMask;
	public Text[] text_objects;

	public enum ChosenScreen
    {
		Up = 0,
		Left = 1,
		Down = 2,
		Right = 3
    }

	public ChosenScreen chosenScreen;
	// Use this for initialization
	void Start () {
		direction = this.gameObject.transform;
		//Debug.Log(layerMask);
		chosenScreen = ChosenScreen.Up;
	}
	
	public void ChangeActiveScreen()
    {
        for (int i = 0; i < 4; i++)
        {
			if (i == (int)chosenScreen) text_objects[i].gameObject.SetActive(true);
			else text_objects[i].gameObject.SetActive(false);

		}
    }

	void FixedUpdate () {
		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity,layerMask))
        {
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);

			float distance = hit.distance;

			if (distance < 10f)
			{
				text_objects[(int)chosenScreen].text = string.Format("{0:F3} {1}",distance,"m");
			}
			else if (distance < 100f)
			{
				text_objects[(int)chosenScreen].text = string.Format("{0:F2} {1}",distance,"m");
			}
			else if (distance < 1000f)
			{
				text_objects[(int)chosenScreen].text = string.Format("{0:F1} {1}",distance,"m");
			}
			else text_objects[(int)chosenScreen].text = string.Format("{0:F0} {1}",distance,"m");
            
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
			text_objects[(int)chosenScreen].text = "inf";
        }
	}
}
