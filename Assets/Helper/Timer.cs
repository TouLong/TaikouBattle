using System;
using UnityEngine;
using System.Collections;

public class Timer : MonoBehaviour
{
    static readonly GameObject timerGameObject = new GameObject("Timer");
    static readonly Timer timer = timerGameObject.AddComponent<Timer>();
    public static Coroutine Set(float time, Action onCompleteAction)
    {
        return timer.StartCoroutine(timer.Action(time, onCompleteAction));
    }

    public static void Cancel(Coroutine coroutine)
    {
        timer.StopCoroutine(coroutine);
    }

    public static void Period(float time, Action onPeriod)
    {
        timer.StartCoroutine(timer.Repeat(time, onPeriod));
    }

    public static void Period(float time, Func<bool> check)
    {
        timer.StartCoroutine(timer.Repeat(time, check, null));
    }

    public static void Period(float time, Func<bool> check, Action onComplete)
    {
        timer.StartCoroutine(timer.Repeat(time, check, onComplete));
    }

    IEnumerator Action(float time, Action onCompletedAction)
    {
        yield return new WaitForSeconds(time);
        onCompletedAction();
    }

    IEnumerator Repeat(float time, Action onPeriod)
    {
        yield return new WaitForSeconds(time);
        while (true)
        {
            onPeriod();
            yield return new WaitForSeconds(time);
        }
    }

    IEnumerator Repeat(float time, Func<bool> check, Action onComplete)
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
                onComplete();
                break;
            }
        }
    }
}
