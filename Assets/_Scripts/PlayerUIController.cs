using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    public Slider health;
    public Image healthFill;

    public Color[] colors;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitMaxHealth(float maxValue)
    {
        health.maxValue = maxValue;
    }

    public void UpdateHealth(float newHealthValue)
    {
        health.value = newHealthValue;
    }

    public void SetColor(int playerIndex)
    {
        healthFill.color = colors[playerIndex];
    }
}
