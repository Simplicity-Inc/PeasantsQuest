using UnityEngine;
using System.Collections;

public class MoneyManager : MonoBehaviour {

    private int currMoney;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public int CurrMoney
    {
        get
        {
            return currMoney;
        }
        set
        {
            currMoney = value;
        }
    }

}
