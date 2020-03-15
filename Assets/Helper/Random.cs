using UnityEngine;

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