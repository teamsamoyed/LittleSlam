using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
	void Start ()
    {
        var cam = Camera.main;
        var sprite = GetComponent<SpriteRenderer>();

        var camHeight = cam.orthographicSize * 2;
        var camWidth = cam.aspect * camHeight;

        var newScale = new Vector3();
        newScale.x = camWidth / sprite.bounds.size.x;
        newScale.y = camHeight / sprite.bounds.size.y;
        newScale.z = 1.0f;

        transform.localScale = newScale;
	}

    void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}
