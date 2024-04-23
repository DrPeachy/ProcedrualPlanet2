#ifndef UTILS_CGINC
#define UTILS_CGINC

    #define TWO_PI 6.28318530718

    float3 TriColAmbient(float3 n, float3 uCol, float3 dCol, float3 sCol, float weakness) {
        float upMask = max(0.0, n.y);
        float downMask = max(0.0, -n.y);
        float sideMask  = 1 - upMask - downMask;
        float3 envCol = uCol * upMask + dCol * downMask + sCol * sideMask;
        envCol = lerp(envCol, float3(1, 1, 1), weakness);
        return envCol;
    }

    void Translation(inout float3 pos, float3 translation) {
        pos += translation;
    }

    void OscillateTrans(inout float3 pos, float amplitude, float frequency, float phase) {
        pos += amplitude * sin(frequency * _Time.y * TWO_PI + phase);
    }

    void Scale(inout float3 pos, float3 scale) {
        pos *= scale;
    }

    void OscillateScale(inout float3 pos, float amplitude, float frequency, float phase) {
        pos *= 1 + amplitude * sin(frequency * _Time.y * TWO_PI + phase);
    }
    void Rotation(inout float3 pos, float3 rotation) {
        float3x3 rotMat = float3x3(
            cos(rotation.y) * cos(rotation.z), cos(rotation.y) * sin(rotation.z), -sin(rotation.y),
            cos(rotation.x) * sin(rotation.z) + sin(rotation.x) * sin(rotation.y) * cos(rotation.z), cos(rotation.x) * cos(rotation.z) - sin(rotation.x) * sin(rotation.y) * sin(rotation.z), sin(rotation.x) * cos(rotation.y),
            sin(rotation.x) * sin(rotation.z) - cos(rotation.x) * sin(rotation.y) * cos(rotation.z), sin(rotation.x) * cos(rotation.z) + cos(rotation.x) * sin(rotation.y) * sin(rotation.z), cos(rotation.x) * cos(rotation.y)
        );
        pos = mul(rotMat, pos);
    }

    void OscillateRot(inout float3 pos, float amplitude, float frequency, float phase) {
        float angleY = amplitude * sin(frequency * _Time.y * TWO_PI + phase);

        float radY = radians(angleY);
        float sinY, cosY;
        sincos(radY, sinY, cosY);
        pos.yz = float2(pos.y * cosY - pos.z * sinY, pos.y * sinY + pos.z * cosY);
    }

    // Hash function
    float3 hash(float3 p, float seed)
    {
        p = float3(dot(p, float3(127.1, 311.7, 74.7) + seed),
                   dot(p, float3(269.5, 183.3, 246.1) + seed),
                   dot(p, float3(113.5, 271.9, 124.6) + seed));
    
        return -1.0 + 2.0 * frac(sin(p) * (43758.5453123 + seed));
    }

    // Linear interpolation
    float lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }

    // Smoothstep function
    float smoothstep(float edge0, float edge1, float x)
    {
        x = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
        return x * x * (3 - 2 * x);
    }

    // Perlin noise function
    float perlin(float3 p, float freq, float seed)
    {
        p *= freq;
        float3 pi = floor(p);
        float3 pf = p - pi;
        float3 w = pf * pf * (3 - 2 * pf);

        float n000 = dot(hash(pi + float3(0, 0, 0), seed), pf - float3(0, 0, 0));
        float n100 = dot(hash(pi + float3(1, 0, 0), seed), pf - float3(1, 0, 0));
        float n010 = dot(hash(pi + float3(0, 1, 0), seed), pf - float3(0, 1, 0));
        float n110 = dot(hash(pi + float3(1, 1, 0), seed), pf - float3(1, 1, 0));
        float n001 = dot(hash(pi + float3(0, 0, 1), seed), pf - float3(0, 0, 1));
        float n101 = dot(hash(pi + float3(1, 0, 1), seed), pf - float3(1, 0, 1));
        float n011 = dot(hash(pi + float3(0, 1, 1), seed), pf - float3(0, 1, 1));
        float n111 = dot(hash(pi + float3(1, 1, 1), seed), pf - float3(1, 1, 1));

        float nx00 = lerp(n000, n100, w.x);
        float nx01 = lerp(n001, n101, w.x);
        float nx10 = lerp(n010, n110, w.x);
        float nx11 = lerp(n011, n111, w.x);

        float nxy0 = lerp(nx00, nx10, w.y);
        float nxy1 = lerp(nx01, nx11, w.y);

        return lerp(nxy0, nxy1, w.z);
    }

    float3 hash3(float3 p) {
        p = frac(p * 0.3183099 + float3(0.1,0.9,0.5));
        p += dot(p, p + 19.19);
        return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
    }
    
    // Voronoi noise function
    float voronoi(float3 position, float scale) {
        float3 p = position * scale;
        float3 f = floor(p);
        float res = 8.0;  // Initial large distance
    
        for (int k = -1; k <= 1; k++) {
            for (int j = -1; j <= 1; j++) {
                for (int i = -1; i <= 1; i++) {
                    float3 b = float3(i, j, k);
                    float3 r = b - (frac(p) - hash3(f + b));
                    float d = dot(r, r);
    
                    res = min(res, d);
                }
            }
        }
        return res;
    }


#endif