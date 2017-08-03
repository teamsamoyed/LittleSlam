using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemainTime : MonoBehaviour
{
    public bool isMinute;
	// Update is called once per frame
    void Update()
    {
        int min = (int)GameManager.Instance.RemainGameTime / 60;
        int sec = (int)GameManager.Instance.RemainGameTime % 60;

        if (isMinute)
        {
            string minStr = min.ToString();
            GetComponent<Text>().text = minStr;
        }
        else
        {
            string secStr = sec.ToString();
            if (sec < 10)
                secStr = "0" + secStr;

            GetComponent<Text>().text = secStr;
        }
    }
}
