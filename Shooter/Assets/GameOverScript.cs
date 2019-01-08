using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverScript : MonoBehaviour
{
    public static GameOverScript end;
    public bool replay = false;

    public float startTime = 0;
    public string second;
    public float t;
    bool c;
    int a = 20;
    public AudioClip gameOverSound;
    RectTransform transform1;
    Vector3 pos = new Vector3();
    bool gmov = false;
    public bool gameEndFlag = false;
    AudioSource source;
    // Use this for initialization
    void Start()
    {
        replay = false;
        end = this;
        source = GetComponent<AudioSource>();
        transform1 = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (gameEndFlag == true && !c)
        {
            startTime = Time.time;
            c = true;
        }
        if (gameEndFlag == true)
        {
            gmov = true;
            //transform.position += Vector3.down;
            this.transform1.Translate(Vector3.down * a * Time.deltaTime);
            t = Time.time - startTime;
            second = ((int)(t % 60)).ToString();
            if (second == "0")
                source.Play();
            if (second == "2")
            {
                a = 0;
                if (gmov == true)
                {
                    replay = true;
                    gmov = false;
                }
                a = 0;
            }

        }

    }
}
