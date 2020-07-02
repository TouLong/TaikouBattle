using UnityEngine;

public static class TerrainHeight
{
    public static void Noise(ref float[,] noise, int size, int seed, int octaves, float persistance, float lacunarity, int noiseScale, Vector2 offset)
    {
        noise = new float[size, size];
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        float min = float.MaxValue;
        float max = float.MinValue;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + octaveOffsets[i].x) / noiseScale * frequency;
                    float sampleY = (y + octaveOffsets[i].y) / noiseScale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                noise[x, y] = noiseHeight;
                min = Mathf.Min(noise[x, y], min);
                max = Mathf.Max(noise[x, y], max);
            }
        }

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                noise[x, y] = Mathf.InverseLerp(min, max, noise[x, y]);
            }
        }
    }
    public static void Evaluate(out float[,] height, ref float[,] noise, Vector2Int offset, int size, int scale, int heightScale, AnimationCurve curve)
    {
        height = new float[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                height[x, y] = curve.Evaluate(noise[offset.x + x, offset.y + y]) * heightScale * scale;
            }
        }
    }
    public static void Evaluate(ref float[,] height, ref float[,] noise, int size, int scale, int heightScale, AnimationCurve curve)
    {
        height = new float[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                height[x, y] = curve.Evaluate(noise[x, y]) * heightScale * scale;
            }
        }
    }
}
