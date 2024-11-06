using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenLink : MonoBehaviour
{
    public string url = "";

    public void OpenWebpage()
    {
        if (url.Length > 0)
            Application.OpenURL(url);
        else
            Debug.Log("no link found!");
    }
}
