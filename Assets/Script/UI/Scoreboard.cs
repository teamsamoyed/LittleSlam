using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    public int Index;
    private void Update()
    {
        var text = GameManager.Instance.Score[Index].ToString();
        GetComponent<Text>().text = text;
    }
}
