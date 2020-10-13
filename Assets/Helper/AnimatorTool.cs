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
    public void CrossFade(string stateName)
    {
        animator.CrossFade(stateName, 0.1f, layer);
    }
    public void CrossFadeEvent(string stateName, Action action, float actionTime)
    {
        animator.CrossFade(stateName, 0.1f, layer);
        AnimatorTool.Event(this, stateName, action, actionTime);
    }
}

public class AnimatorTool : MonoBehaviour
{
    static readonly AnimatorTool animatorTool = new GameObject("AnimatorTool").AddComponent<AnimatorTool>();
    static public void Event(AnimatorLayer animator, string stateName, Action action, float actionTime)
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