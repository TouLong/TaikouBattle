using System.Collections;
using System;
using UnityEngine;

public class UnitMotion : MonoBehaviour
{
    const float frameToSecond = 1f / 24f;
    struct MotionFrame
    {
        public float start;
        public float end;
        public float length;
        public MotionFrame(int start, int end)
        {
            this.start = start * frameToSecond;
            this.end = end * frameToSecond;
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
        anim = unit.GetComponentInChildren<Animation>();
        state = anim[unit.weapon.motionAnim];
        stateName = unit.weapon.motionAnim;
        anim.Play(stateName);
    }
    public void IdlePose()
    {
        state.time = idle.end;
        state.speed = 0;
    }
    public float Attack(Action onCompleted = null)
    {
        state.time = ready.start;
        state.speed = 1;
        targetFrame = reset.end;
        StartCoroutine(Playing(onCompleted));
        return targetFrame - state.time;
    }
    IEnumerator Playing(Action onCompleted = null)
    {
        while (true)
        {
            if (state.speed > 0 && state.time <= targetFrame)
                yield return null;
            else if (state.speed < 0 && state.time >= targetFrame)
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
