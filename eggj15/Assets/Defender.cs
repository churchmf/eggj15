using UnityEngine;
using System.Collections;

public class Defender : MonoBehaviour
{
		// Use this for initialization
		void Start ()
		{
	
		}
	
		void Update ()
		{
	
		}

		public void OnGUI ()
		{
				GUI.Label (new Rect (Screen.width / 2 - Screen.width / 10, Screen.height / 2 - Screen.height / 20, Screen.width / 5, Screen.height / 20), "Defender");
		}
}
