using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager singleton;
    public float timer;
    public KeyCode button;
    public Text timerText;
    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        int minutes = (int)(timer / 60);
        timerText.text = minutes+ ":"+(timer % 60).ToString(".00");
        if (Input.GetKeyDown(button)) PlayerController.singleton.ResetPosition();
    }
}
