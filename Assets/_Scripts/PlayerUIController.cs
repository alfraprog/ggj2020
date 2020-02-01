using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    public Slider health;

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
}
