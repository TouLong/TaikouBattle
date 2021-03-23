using System;
using UnityEngine;
using System.Collections;

public class DelayEvent : MonoBehaviour
{
    static readonly DelayEvent delay = new GameObject("DelayEvent").AddComponent<DelayEvent>();
    public static Coroutine Create(float time, Action onCompleted)
    {
        return delay.StartCoroutine(Run(time, onCompleted));
    }
    public static Coroutine Period(float time, Action onPeriod)
    {
        return delay.StartCoroutine(Repeat(time, onPeriod));
    }
    public static Coroutine CheckPeriod(float time, Func<bool> check, Action onCompleted = null)
    {
        return delay.StartCoroutine(Repeat(time, check, onCompleted));
    }
    public static void Cancel(Coroutine coroutine)
    {
        delay.StopCoroutine(coroutine);
    }
    static IEnumerator Run(float time, Action onCompleted)
    {
        yield return new WaitForSeconds(time);
        onCompleted();
    }
    static IEnumerator Repeat(float time, Action onPeriod)
    {
        yield return new WaitForSeconds(time);
        while (true)
        {
            onPeriod();
            yield return new WaitForSeconds(time);
        }
    }
    static IEnumerator Repeat(float time, Func<bool> check, Action onCompleted)
    {
        yield return new WaitForSeconds(time);
        while (true)
        {
            if (!check())
            {
                yield return new WaitForSeconds(time);
            }
            else
            {
                onCompleted();
                break;
            }
        }
    }
}
