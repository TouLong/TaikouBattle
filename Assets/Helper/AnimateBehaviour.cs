using System;
using UnityEngine;
using System.Collections;

public class AnimateBehaviour : MonoBehaviour
{
    enum State
    {
        Ready,
        Playing,
        Completed,
    }
    Animator animator;
    State state;
    Action animationEvent;
    float eventTime;
    public void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void Play(string name, int layer = 0)
    {
        animator.enabled = true;
        animator.CrossFade(name, 0.1f, layer);
        state = State.Ready;
        StartCoroutine("UpdateState", layer);
    }
    public void Play(string name, Action action, int layer = 0, float time = 1)
    {
        animationEvent = action;
        eventTime = time;
        Play(name, layer);
    }
    IEnumerator UpdateState(object layer)
    {
        while (true)
        {
            float normalizedTime = animator.GetCurrentAnimatorStateInfo((int)layer).normalizedTime;
            if (state == State.Ready)
            {
                if (normalizedTime < 1)
                {
                    state = State.Playing;
                }
            }
            else if (state == State.Playing)
            {
                if (animationEvent != null)
                {
                    if (normalizedTime >= eventTime)
                    {
                        animationEvent();
                        animationEvent = null;
                    }
                }
                else if (normalizedTime > 1)
                {
                    break;
                }
            }
            yield return null;
        }
    }
}