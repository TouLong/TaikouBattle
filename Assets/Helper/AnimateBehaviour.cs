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
    public void Play(string name)
    {
        animator.enabled = true;
        animator.CrossFade(name, 0.1f);
        state = State.Ready;
        StartCoroutine("UpdateState");
    }
    public void Play(string name, Action action, float time = 1)
    {
        animationEvent = action;
        eventTime = time;
        Play(name);
    }
    IEnumerator UpdateState()
    {
        while (true)
        {
            float normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            //print(normalizedTime);
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