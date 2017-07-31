using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemainTime : MonoBehaviour
{
	// Update is called once per frame
    void Update()
    {
        int min = (int)GameManager.Instance.RemainGameTime / 60;
        int sec = (int)GameManager.Instance.RemainGameTime % 60;
        var text = min + ":" + sec;
        GetComponent<Text>().text = text;
    }
}
