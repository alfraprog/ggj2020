using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject playerSelectUI;

    private void Awake()
    {
        instance = this;


    }
    // Start is called before the first frame update
    void Start()
    {
        playerSelectUI.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HidePlayerSelection()
    {
        playerSelectUI.SetActive(false);
    }

}
