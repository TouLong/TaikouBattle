using UnityEngine;
using System;

public class FreezeOnTriggerEnter : MonoBehaviour
{
    public float delayTime = 3;
    Coroutine freeze;
    public Action onEnter;
    public bool destroyGameObject;
    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Terrain Chunk")
        {
            if (freeze != null)
                Timer.Cancel(freeze);
            freeze = Timer.Set(delayTime, () =>
            {
                transform.parent.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                onEnter?.Invoke();
                if (destroyGameObject)
                    Destroy(gameObject);
                else
                    Destroy(this);
            });
        }
    }
}