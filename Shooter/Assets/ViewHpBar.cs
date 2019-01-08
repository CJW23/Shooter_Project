using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewHpBar : MonoBehaviour {

    public Image currentHealthbar;

    private float hitpoint = 100;
    private float maxHitpoint = 100;

    private GameOverScript GameOver;

    // Use this for initialization
    void Start () {
        UpdateHealthbar();
        GameOver = GameObject.Find("GameOverImage").GetComponent<GameOverScript>();
    }
	
	// Update is called once per frame
	void UpdateHealthbar () {
        float ratio = hitpoint / maxHitpoint;
        currentHealthbar.rectTransform.localScale = new Vector3(ratio, 2, 1);
	}
    
    public void TakeDamge(float damege)
    {
        hitpoint -= damege;
        if (hitpoint<0)
        {
            hitpoint = 0;
            Die();
        }
        UpdateHealthbar();
    }

   void Die()
    {
        GameOver.gameEndFlag = true;
    }
}
