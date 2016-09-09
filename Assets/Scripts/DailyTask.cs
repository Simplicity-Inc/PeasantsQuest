using UnityEngine;
using System.Collections;

public class DailyTask : MonoBehaviour {

    private float currTime;

    public Camera cam;

    public GameObject player;

    public MoneyManager moneyManager;

    

    // Use this for initialization
    void Start() {
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update() {
        if (Vector3.Distance(player.transform.position, transform.position) <= 5f)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("mouse down");
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.name == this.name)
                    {
                        Debug.DrawLine(ray.origin, hit.point);
                        Debug.Log("harvest begun");
                        Harvest(5);
                    }
                }
            }
        }
    }

    
    void Harvest(int finishValue)
    {
        Debug.Log("Harvesting complete");
        moneyManager.CurrMoney += finishValue;
         
        Debug.Log(moneyManager.CurrMoney);
    }
}
