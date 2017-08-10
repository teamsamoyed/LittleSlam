using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeoutUI : MonoBehaviour
{	
	void Update ()
    {
        int remain = (int)GameManager.Instance.TimeOut;

        var text = remain.ToString();

        if (text.Length == 1)
            text = "0" + text;

        GetComponent<Text>().text = text;
	}
}
