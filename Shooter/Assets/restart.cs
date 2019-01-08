using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class restart : MonoBehaviour {

    public void LoadByIndex(int sceneIndex)
    {
        ViewScore.scoreValue = 0;
        BodySourceView.rh = 0.0f;
        BodySourceView.recognition = false;
        BodySourceView.restartrecog = true;
        SceneManager.LoadScene(sceneIndex);
        
    }
}
