using UnityEngine;
using System;

public class OnTriggerGorund : MonoBehaviour
{
    float delayTime;
    Action onEnter;
    Coroutine delayTimer;
    public void Create(Action onEnter, float delayTime = 0)
    {
        this.onEnter = onEnter;
        this.delayTime = delayTime;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9)//Ground
        {
            if (delayTimer != null)
                return;
            delayTimer = DelayEvent.Create(delayTime, () =>
            {
                onEnter?.Invoke();
                Destroy(this);
            });
        }
    }
}
