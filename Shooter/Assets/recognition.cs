using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class recognition : MonoBehaviour {

    Text rec;
	// Use this for initialization
	void Start () {
        rec = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        if (BodySourceView.recognition == true)
        {
            Debug.Log("last1");
            rec.enabled = true;
        }
        else {
            Debug.Log("last2");
            rec.enabled = false;
        }
            
	}
}
