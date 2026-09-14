#define VS_SHADERMODEL vs_6_0
#define PS_SHADERMODEL ps_6_0

#define PI 3.14159265359
#define TAU 6.28318530718

#define MAX_DISJOINT_INTERVALS 16

#define EPSILON 0.0001

matrix Projection;
matrix World;

half2 LightCenter;
half LightRadius;

Texture2D OccluderTextureBuffer : register(t0);
SamplerState OccluderTextureSampler : register(t0);
int OccluderCount;

Texture2D COccluderTextureBuffer : register(t1);
SamplerState COccluderTextureSampler : register(t1);
int COccluderCount;

half2 ScreenSize;

struct VertexShaderInput
{
    half4 Position : POSITION0;
    half4 TexCoords : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    half4 TexCoords : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, mul(World, Projection));
    output.TexCoords = input.TexCoords;

    return output;
}

half Mod(half value, half mod) 
{
    return value - mod * floor(value / mod);
}

half Wrap(half value)
{
    if (value >= 0 && value < TAU) return value;
    return Mod(value, TAU);
}

bool IntervalIsSubset(half s1, half e1, half s2, half e2)
{
    half a = Wrap(s1 - s2);
    half b = Wrap(e2 - s2);
    half w = Wrap(e1 - s1);
    return a <= b && a + w <= b;
}

bool IntervalIsDisjoint(half s1, half e1, half s2, half e2)
{
    half a = Wrap(s1 - s2);
    half b = Wrap(e2 - s2);
    half w = Wrap(e1 - s1);
    return a >= b && a + w <= TAU;
}

bool IntervalIntersects(half s1, half e1, half s2, half e2)
{
    return max(s1, s2) <= min(e1, e2);
}

int AddInterval(out half2 intervals[MAX_DISJOINT_INTERVALS], half2 newInterval, int numIntervals)
{
    if (numIntervals >= MAX_DISJOINT_INTERVALS - 1) return numIntervals;    
    
    int i = numIntervals - 1;
    while (i >= 0 && intervals[i].x > newInterval.x)
    {
        intervals[i + 1] = intervals[i];
        i--;
    }
    intervals[i + 1] = newInterval;
    numIntervals++;
        
    int c = 0;
    int j = 1;
    while (j < numIntervals)
    {
        if (intervals[j].x <= intervals[c].y || abs(intervals[j].x - intervals[c].y) < EPSILON)
            intervals[c].y = max(intervals[c].y, intervals[j].y);
        else
        {
            c++;
            intervals[c] = intervals[j];
        }
        j++;
    }
    
    return c + 1;
}

bool IsFullInterval(half2 interval, half lightHalfAngularRadius)
{
    return interval.x <= 0 && (abs(interval.y - 2 * lightHalfAngularRadius) < EPSILON || interval.y >= 2 * lightHalfAngularRadius);
}

half GetIntervalLength(half2 intervals[MAX_DISJOINT_INTERVALS], int numIntervals)
{
    half total = 0;
    for (int i = 0; i < numIntervals; ++i)
        total += intervals[i].y - intervals[i].x;
    return total;
}

bool IsInFrontLight(half2 v, half2 p, half pointAngle, half tangentAngle1, half tangentAngle2, half lightDistance)
{
    if (Wrap(pointAngle - tangentAngle2) > Wrap(tangentAngle1 - tangentAngle2)) return true;
    half a = v.x - LightCenter.x + p.x;
    half b = v.y - LightCenter.y + p.y;
    return v.x * v.x + v.y * v.y <= lightDistance * lightDistance - LightRadius * LightRadius && 
        a * a + b * b >= LightRadius * LightRadius;
}

half CalculateOcclusion(half2 tex)
{
    half2 pixel = half2(tex.x * ScreenSize.x, tex.y * ScreenSize.y);
    
    half2 lightRelPos = LightCenter - pixel;
    half lightDistance = length(lightRelPos);
    half lightHalfAngularRadius = asin(LightRadius / lightDistance);
    
    half lightCenterAngle = Wrap(atan2(lightRelPos.y, lightRelPos.x));
    half tangentAngle1 = Wrap(lightCenterAngle + lightHalfAngularRadius);
    half tangentAngle2 = Wrap(lightCenterAngle - lightHalfAngularRadius);
    
    half2 intervals[MAX_DISJOINT_INTERVALS];
    int numIntervals = 0;
    
    for (int i = 0; i < COccluderCount; i++)
    {
        half4 v = COccluderTextureBuffer.Sample(COccluderTextureSampler, half2(1.0 / COccluderCount * (i + 0.5), 0.5));
        
        half2 center = half2(v.x, v.y) - pixel;
        half radius = v.z;
        
        half occluderDistance = length(center);
        half occHalfAngularRadius = asin(radius / occluderDistance);
        half occluderCenterAngle = Wrap(atan2(center.y, center.x));
        half occTangentAngle1 = Wrap(occluderCenterAngle + occHalfAngularRadius);
        half occTangentAngle2 = Wrap(occluderCenterAngle - occHalfAngularRadius);
        
        if (!IsInFrontLight(center, pixel, occluderCenterAngle, 
            Wrap(tangentAngle1 + occHalfAngularRadius), 
            Wrap(tangentAngle2 - occHalfAngularRadius), 
            lightDistance)) continue;
        
        if (IntervalIsSubset(tangentAngle2, tangentAngle1, occTangentAngle2, occTangentAngle1)) return 1;
        
        if (!IntervalIsDisjoint(tangentAngle2, tangentAngle1, occTangentAngle2, occTangentAngle1))
        {
            half m1 = Wrap(occTangentAngle1 - tangentAngle2 + PI) - PI;
            half m2 = Wrap(occTangentAngle2 - tangentAngle2 + PI) - PI;
            
            numIntervals = AddInterval(intervals, half2(clamp(m2, 0, 2 * lightHalfAngularRadius), clamp(m1, 0, 2 * lightHalfAngularRadius)), numIntervals);
        }
        
        if (numIntervals > 0 && IsFullInterval(intervals[0], lightHalfAngularRadius)) return 1;
    }
    
    /*for (int i = 0; i < OccluderCount; i++)
    {
        half4 v = OccluderTextureBuffer.Sample(OccluderTextureSampler, half2(1.0 / OccluderCount * (i + 0.5), 0.5));

        half2 lineStart = half2(v.x, v.y) - pixel;
        half2 lineEnd = half2(v.z, v.w) - pixel;

        half pointAngle1 = Wrap(atan2(lineStart.y, lineStart.x));
        half pointAngle2 = Wrap(atan2(lineEnd.y, lineEnd.x));

        if (Wrap(pointAngle1 - pointAngle2) > PI) 
        {
            half temp = pointAngle1;
            pointAngle1 = pointAngle2;
            pointAngle2 = temp;
        }

        if (!IsInFrontLight(lineStart, pixel, pointAngle1, tangentAngle1, tangentAngle2, lightDistance) ||
            !IsInFrontLight(lineEnd, pixel, pointAngle2, tangentAngle1, tangentAngle2, lightDistance)) continue;
            
        if (IntervalIsSubset(tangentAngle2, tangentAngle1, pointAngle2, pointAngle1)) return 1;
        
        if (IntervalIsSubset(pointAngle2, pointAngle1, tangentAngle2, tangentAngle1))
        {
            half m1 = Wrap(pointAngle1 - tangentAngle2);
            half m2 = Wrap(pointAngle2 - tangentAngle2);
            
            numIntervals = AddInterval(intervals, half2(m2, m1), numIntervals);
        }
        
        else if (!IntervalIsDisjoint(tangentAngle2, tangentAngle1, pointAngle2, pointAngle1))
        {
            half m1 = Wrap(pointAngle1 - tangentAngle2 + PI) - PI;
            half m2 = Wrap(pointAngle2 - tangentAngle2 + PI) - PI;
            
            numIntervals = AddInterval(intervals, half2(clamp(m2, 0, 2 * lightHalfAngularRadius), clamp(m1, 0, 2 * lightHalfAngularRadius)), numIntervals);
        }
        
        if (numIntervals > 0 && IsFullInterval(intervals[0], lightHalfAngularRadius)) return 1;
    }*/
    
    return GetIntervalLength(intervals, numIntervals) / (lightHalfAngularRadius * 2);
}

half4 MainPS(VertexShaderOutput input) : SV_TARGET
{
    half occlusion = CalculateOcclusion(input.TexCoords);
    return half4(0, 0, 0, saturate(occlusion));
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};