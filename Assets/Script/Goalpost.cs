using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goalpost : MonoBehaviour
{
    public int Team;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Goal");
        //if (other.tag == Tags.Ball)
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
