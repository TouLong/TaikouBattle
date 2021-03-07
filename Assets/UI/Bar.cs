using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
    Slider slider;
    Image fornt;
    public Gradient healthColor;

    public void Setup(float maxHealth)
    {
        slider = GetComponent<Slider>();
        fornt = transform.Find("Front").GetComponent<Image>();
        slider.minValue = 0;
        slider.maxValue = maxHealth;
    }
    public void Set(float value)
    {
        int colorId = (int)(value * healthColor.colorKeys.Length / slider.maxValue);
        colorId = Mathf.Min(colorId, healthColor.colorKeys.Length - 1);
        fornt.color = healthColor.colorKeys[colorId].color;
        slider.value = value;
    }
}