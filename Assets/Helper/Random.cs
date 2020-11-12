using System.Collections.Generic;
using UnityEngine;

class NDRandom//NormalDistributionRandom
{
    //https://answers.unity.com/questions/421968/normal-distribution-random.html
    static public float Range(float min, float max)
    {
        float ret;
        do
        {
            float u, v, S;
            do
            {
                u = 2.0f * Random.value - 1.0f;
                v = 2.0f * Random.value - 1.0f;
                S = u * u + v * v;
            }
            while (S >= 1.0f);
            // Standard Normal Distribution
            float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

            // Normal Distribution centered between the min and max value
            // and clamped following the "three-sigma rule"
            float mean = (min + max) / 2.0f;
            float sigma = (max - mean) / 3.0f;
            ret = std * sigma + mean;
        }
        while (ret > max || ret < min);
        return ret;
    }
}

class V3Random
{
    static public Vector3 Range(float min, float max)
    {
        return new Vector3(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max));
    }
    static public Vector3 Range(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        return new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax));
    }
    static public Vector3 RangeXZ(float min, float max)
    {
        return new Vector3(Random.Range(min, max), 0, Random.Range(min, max));
    }
    static public Vector3 DirectionXZ()
    {
        Vector2 vector = Random.insideUnitCircle.normalized;
        return new Vector3(vector.x, 0, vector.y);
    }
}

class Q4Random
{
    static public Quaternion Value()
    {
        return new Quaternion(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
    }
}

class ListRandom
{
    static public T In<T>(IList<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }
}

class ColorRandom
{
    static public Color Palette(float fixed1, float fixed2)
    {
        float random = Random.value;
        Color[] colors = new Color[]
        {
            new Color(fixed1,fixed2,random),
            new Color(fixed2,fixed1,random),
            new Color(fixed1,random,fixed2),
            new Color(fixed2,random,fixed1),
            new Color(random,fixed1,fixed2),
            new Color(random,fixed2,fixed1),
        };
        return colors[Random.Range(0, colors.Length)];
    }
}