using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCtrl_FastSkleton : MonoBehaviour
{
    public Transform[] points; // 이동할 포인터 그룹가져옴
    public int nextIdx = 1;

    public float speed = 12.0f; //이동속도
    public float damping = 12.0f; //회전속도

    public float damage = 10; //공격력

    private Vector3 movePos;
    private bool isAttack = false; //공격판단
    private Animator anim;

    private Transform playerTr;
    private Transform tr;


    public ViewHpBar HealthBar;
    private bool flag = true;

    // Use this for initialization
    void Start()
    {
        tr = GetComponent<Transform>();
        playerTr = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        points = GameObject.Find("WayPointGroup1").GetComponentsInChildren<Transform>();
        anim = GetComponent<Animator>();

        HealthBar = GameObject.Find("Fill").GetComponent<ViewHpBar>();
    }

    // Update is called once per frame
    void Update()
    {

        float dist = Vector3.Distance(tr.position, playerTr.position);

        if (dist <= 2.0f)
        {
            isAttack = true;

        }
        else if (dist <= 5.0f)
        {
            movePos = playerTr.position;
            isAttack = false;
        }
        else
        {
            movePos = points[nextIdx].position;
            isAttack = false;
        }

        anim.SetBool("isAttack", isAttack);


        if (!isAttack)
        {
            Quaternion rot = Quaternion.LookRotation(movePos - tr.position);
            tr.rotation = Quaternion.Slerp(tr.rotation, rot, Time.deltaTime * damping);
            tr.Translate(Vector3.forward * Time.deltaTime * speed);
        }
        else
        {
            if (flag)
            {
                StartCoroutine(WaitAnim());

            }
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "WAY_POINT")
        {
            nextIdx = (++nextIdx >= points.Length) ? 1 : nextIdx;
        }
    }

    IEnumerator WaitAnim()
    {
        flag = false;
        yield return new WaitForSeconds(1.0f);
        HealthBar.TakeDamge(damage);
        flag = true;
    }

}
