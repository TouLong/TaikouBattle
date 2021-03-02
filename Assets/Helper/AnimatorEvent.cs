using System;
using UnityEngine;
using System.Collections;

public class AnimatorLayer
{
    public Animator animator;
    public int layer = 0;
    public AnimatorLayer(Animator animator, int layer)
    {
        this.animator = animator;
        this.layer = layer;
    }
    public void Play(string stateName)
    {
        animator.Play(stateName, layer);
    }
    public void Play(string stateName, Action action, float actionTime)
    {
        Play(stateName);
        AnimatorEvent.Create(this, stateName, action, actionTime);
    }
}

public class AnimatorEvent : MonoBehaviour
{
    static readonly AnimatorEvent animatorTool = new GameObject("AnimatorTool").AddComponent<AnimatorEvent>();
    static public void Create(AnimatorLayer animator, string stateName, Action action, float actionTime)
    {
        animatorTool.StartCoroutine(animatorTool.UpdateState(animator, stateName, action, actionTime));
    }
    IEnumerator UpdateState(AnimatorLayer animatorLayer, string stateName, Action action, float actionTime)
    {
        while (true)
        {
            AnimatorStateInfo curStateInfo = animatorLayer.animator.GetCurrentAnimatorStateInfo(animatorLayer.layer);
            if (curStateInfo.IsName(stateName) && curStateInfo.normalizedTime >= actionTime)
            {
                action();
                break;
            }
            yield return null;
        }
    }
}