using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

    public GameObject[] hazards;

    private float startWait = 3;
    private float spawnWait1 = 2f;
    private float spawnWait2 = 1.7f;
    private float spawnWait3 = 1.4f;
    private float spawnWait4 = 1.1f;
    private float spawnWait5 = 0.7f;
    private float spawnWait6 = 0.3f;

    private float wavetime = 30f;

    // Use this for initialization
    void Start () {
        //InvokeRepeating("SpawnMon_inboke", 2, 1);
    
        StartCoroutine(SpawnMon());
   

    }

  

    IEnumerator SpawnMon()
    {
        yield return new WaitForSeconds(startWait); //1초 대기후

        for (int i=0;i<wavetime/spawnWait1 ;i++)
        {
            GameObject hazard = hazards[0];
            Vector3 spawnPosition = new Vector3(Random.Range(50,-50), 0, 100);
            Quaternion spawnRotation = Quaternion.Euler(new Vector3(0, 180, 0));
            Instantiate(hazard, spawnPosition, spawnRotation);
            yield return new WaitForSeconds(spawnWait1); //1초 대기후
        }

        //wave2
        yield return new WaitForSeconds(startWait); //1초 대기후

        for (int i = 0; i < wavetime / spawnWait2 ; i++)
        {
            GameObject hazard = hazards[0];
            Vector3 spawnPosition = new Vector3(Random.Range(200, -200), 0, 100);
            Quaternion spawnRotation = Quaternion.Euler(new Vector3(0, 180, 0));
            Instantiate(hazard, spawnPosition, spawnRotation);

            yield return new WaitForSeconds(spawnWait2); //1초 대기후
        }

        //wave3
        yield return new WaitForSeconds(startWait); //1초 대기후

        int count = 0;
        for (int i = 0; i < wavetime / spawnWait3; i++)
        {
            GameObject hazard = hazards[0];
            GameObject FastSkeleton = hazards[1];
            Vector3 spawnPosition = new Vector3(Random.Range(200, -200), 0, 100);
            Quaternion spawnRotation = Quaternion.Euler(new Vector3(0, 180, 0));
            Instantiate(hazard, spawnPosition, spawnRotation);
            if(++count==5)
            {
                Instantiate(FastSkeleton, spawnPosition, spawnRotation);
                count = 0;
            }

            yield return new WaitForSeconds(spawnWait3); //1초 대기후
        }

        //wave4
        yield return new WaitForSeconds(startWait); //1초 대기후
        count = 0;

        for (int i = 0; i < wavetime / spawnWait4; i++)
        {
            GameObject hazard = hazards[0];
            GameObject FastSkeleton = hazards[1];
            Vector3 spawnPosition = new Vector3(Random.Range(200, -200), 0, 100);
            Quaternion spawnRotation = Quaternion.Euler(new Vector3(0, 180, 0));
            Instantiate(hazard, spawnPosition, spawnRotation);
            if (++count == 4)
            {
                Instantiate(FastSkeleton, spawnPosition, spawnRotation);
                count = 0;
            }

            yield return new WaitForSeconds(spawnWait4); //1초 대기후
        }

        //wave5
        yield return new WaitForSeconds(startWait); //1초 대기후
        count = 0;

        for (int i = 0; i < wavetime / spawnWait5; i++)
        {
            GameObject hazard = hazards[0];
            GameObject FastSkeleton = hazards[1];
            Vector3 spawnPosition = new Vector3(Random.Range(200, -200), 0, 100);
            Quaternion spawnRotation = Quaternion.Euler(new Vector3(0, 180, 0));
            Instantiate(hazard, spawnPosition, spawnRotation);
            if (++count == 3)
            {
                Instantiate(FastSkeleton, spawnPosition, spawnRotation);
                count = 0;
            }

            yield return new WaitForSeconds(spawnWait5); //1초 대기후
        }
    }

    /*
    void SpawnMon_inboke()
    {
        GameObject hazard = hazards[0];
        Vector3 spawnPosition = new Vector3(Random.Range(6, -6), 0, 20);
        Quaternion spawnRotation = Quaternion.Euler(new Vector3(0, 180, 0));
        Instantiate(hazard, spawnPosition, spawnRotation);

    }
    */

    // Update is called once per frame
    void Update () {
		
	}
}
