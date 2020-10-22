using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    Slider slider;
    Image forntBar;
    public Gradient healthColor;

    public void Setup(float maxHealth)
    {
        slider = GetComponent<Slider>();
        forntBar = transform.Find("Front").GetComponent<Image>();
        slider.minValue = 0;
        slider.maxValue = maxHealth;
    }
    public void Set(float health)
    {
        int colorId = (int)(health * healthColor.colorKeys.Length / slider.maxValue);
        colorId = Mathf.Min(colorId, healthColor.colorKeys.Length - 1);
        forntBar.color = healthColor.colorKeys[colorId].color;
        slider.value = health;
    }
}