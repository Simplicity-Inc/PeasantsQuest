using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AiSteal : MonoBehaviour {

    NavMeshAgent navAgent;

    public float timeToSteal;

    public List<GameObject> targetItemsList = new List<GameObject>();

    private Vector3 wayPoint;

    public bool alerted;

    private float timer;

    private int i;

	// Use this for initialization
	void Start () {
        navAgent = GetComponent<NavMeshAgent>();
        i = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if (!alerted)
        {
            if (i < targetItemsList.Count)
            {
                wayPoint = targetItemsList[i].transform.position;
                navAgent.destination = wayPoint;
                if(Vector3.Distance(navAgent.transform.position,wayPoint)<1f)
                {
                    timer += Time.deltaTime;
                    if(timer >= timeToSteal)
                    {
                        targetItemsList[i].SetActive(false);
                        i++;
                        timer = 0;
                    }
                }
            }
        }
	}
}
