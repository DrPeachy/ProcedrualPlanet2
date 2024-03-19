using UnityEngine;

public static class PerlinNoise3D
{
    public static float Perlin3D(float x, float y, float z)
    {
        // The magic numbers below are arbitrary primes to minimize repetition
        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);

        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        // Find the average of the noise values of the six permutations of the input coordinates
        float abc = AB + BC + AC + BA + CB + CA;
        return abc / 6f;
    }
}

