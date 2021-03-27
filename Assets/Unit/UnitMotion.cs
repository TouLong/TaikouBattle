using System.Collections;
using System;
using UnityEngine;

public class UnitMotion : MonoBehaviour
{
    const float totalFrame = 17;
    struct MotionFrame
    {
        public float start;
        public float end;
        public float length;
        public MotionFrame(int start, int end)
        {
            this.start = start / totalFrame;
            this.end = end / totalFrame;
            length = end - start;
        }
    }
    MotionFrame idle = new MotionFrame(0, 1);
    MotionFrame ready = new MotionFrame(1, 5);
    MotionFrame attack = new MotionFrame(5, 9);
    MotionFrame reset = new MotionFrame(9, 16);
    Animation anim;
    AnimationState state;
    string stateName;
    float targetFrame;
    public void Setup(Unit unit)
    {
        anim = unit.model.GetComponent<Animation>();
        state = anim[unit.weapon.motionAnim];
        stateName = unit.weapon.motionAnim;
        anim.Play(stateName);
    }
    public void IdlePose()
    {
        state.normalizedTime = idle.end;
        state.speed = 0;
    }
    public void Attack(Action onCompleted)
    {
        state.normalizedTime = ready.start;
        state.speed = 1;
        targetFrame = reset.end;
        StartCoroutine(Playing(onCompleted));
    }
    IEnumerator Playing(Action onCompleted = null)
    {
        while (true)
        {
            if (state.speed > 0 && state.normalizedTime <= targetFrame)
                yield return null;
            else if (state.speed < 0 && state.normalizedTime >= targetFrame)
                yield return null;
            else
            {
                state.speed = 0;
                onCompleted?.Invoke();
                break;
            }
        }
    }
}
