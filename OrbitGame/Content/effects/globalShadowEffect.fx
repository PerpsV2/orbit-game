#define VS_SHADERMODEL vs_6_0
#define PS_SHADERMODEL ps_6_0

#define PI 3.14159265359
#define TAU 6.28318530718

#define MAX_DISJOINT_INTERVALS 16

#define EPSILON 0.0001f

#define CSWAP(i, j) if (intervals[i].x > intervals[j].x) { float2 temp = intervals[i]; intervals[i] = intervals[j]; intervals[j] = temp; }

matrix Projection;
matrix World;

float2 LightCenter;
float LightRadius;

Texture2D OccluderTextureBuffer : register(t0);
SamplerState OccluderTextureSampler : register(t0);
int OccluderCount;

Texture2D COccluderTextureBuffer : register(t1);
SamplerState COccluderTextureSampler : register(t1);
int COccluderCount;

float2 ScreenSize;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 TexCoords : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 TexCoords : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, mul(World, Projection));
    output.TexCoords = input.TexCoords;

    return output;
}

float Mod(float value, float mod) 
{
    return value - mod * floor(value / mod);
}

float Wrap(float value)
{
    if (value >= 0 && value < TAU) return value;
    return Mod(value, TAU);
}

bool IntervalIsSubset(float s1, float e1, float s2, float e2)
{
    float a = Wrap(s1 - s2);
    float b = Wrap(e2 - s2);
    float w = Wrap(e1 - s1);
    return a <= b && a + w <= b;
}

bool IntervalIsDisjoint(float s1, float e1, float s2, float e2)
{
    float a = Wrap(s1 - s2);
    float b = Wrap(e2 - s2);
    float w = Wrap(e1 - s1);
    return a >= b && a + w <= TAU;
}

bool IntervalIntersects(float s1, float e1, float s2, float e2)
{
    return max(s1, s2) <= min(e1, e2);
}

int AddInterval(out float2 intervals[MAX_DISJOINT_INTERVALS], float2 newInterval, int numIntervals)
{
    if (numIntervals >= MAX_DISJOINT_INTERVALS - 1) return numIntervals;
    
    if (numIntervals == 0 || newInterval.x > intervals[numIntervals].x) intervals[numIntervals++] = newInterval;
    else
    {
        for (int i = 0; i < numIntervals; ++i)
        {
            if (newInterval.x < intervals[i].x)
            {
                numIntervals++;
                for (int j = numIntervals; j > i; j--)
                    intervals[j] = intervals[j - 1];
                intervals[i] = newInterval;
                break;
            }
        }
    }
    
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
    
    return min(c + 1, MAX_DISJOINT_INTERVALS);
}

bool IsFullInterval(float2 interval, float lightHalfAngularRadius)
{
    return interval.x <= 0 && (abs(interval.y - 2 * lightHalfAngularRadius) < EPSILON || interval.y >= 2 * lightHalfAngularRadius);
}

float GetIntervalLength(float2 intervals[MAX_DISJOINT_INTERVALS], int numIntervals)
{
    float total = 0;
    for (int i = 0; i < numIntervals; ++i)
        total += intervals[i].y - intervals[i].x;
    return total;
}

bool IsInFrontLight(float2 v, float2 p, float pointAngle, float tangentAngle1, float tangentAngle2, float lightDistance)
{
    if (Wrap(pointAngle - tangentAngle2) > Wrap(tangentAngle1 - tangentAngle2)) return true;
    float a = v.x - LightCenter.x + p.x;
    float b = v.y - LightCenter.y + p.y;
    return v.x * v.x + v.y * v.y <= lightDistance * lightDistance - LightRadius * LightRadius && 
        a * a + b * b >= LightRadius * LightRadius;
}

float CalculateOcclusion(float2 tex)
{
    float2 pixel = float2(tex.x * ScreenSize.x, tex.y * ScreenSize.y);
    
    float2 lightRelPos = LightCenter - pixel;
    float lightDistance = length(lightRelPos);
    float lightHalfAngularRadius = asin(LightRadius / lightDistance);
    
    float lightCenterAngle = Wrap(atan2(lightRelPos.y, lightRelPos.x));
    float tangentAngle1 = Wrap(lightCenterAngle + lightHalfAngularRadius);
    float tangentAngle2 = Wrap(lightCenterAngle - lightHalfAngularRadius);
    
    bool occluded = false;
    
    float2 intervals[MAX_DISJOINT_INTERVALS];
    int numIntervals = 0;
    
    for (int i = 0; i < COccluderCount; i++)
    {
        if (occluded) continue;
        
        float4 v = COccluderTextureBuffer.Sample(COccluderTextureSampler, float2(1.0 / COccluderCount * (i + 0.5), 0.5));
        
        float2 center = float2(v.x, v.y) - pixel;
        float radius = v.z;
        
        float occluderDistance = length(center);
        float occHalfAngularRadius = asin(radius / occluderDistance);
        float occluderCenterAngle = Wrap(atan2(center.y, center.x));
        float occTangentAngle1 = Wrap(occluderCenterAngle + occHalfAngularRadius);
        float occTangentAngle2 = Wrap(occluderCenterAngle - occHalfAngularRadius);
        
        if (occluderDistance + radius > lightDistance + LightRadius) continue;
        
        if (IntervalIsSubset(tangentAngle2, tangentAngle1, occTangentAngle2, occTangentAngle1)) occluded = true;
        else if (IntervalIsSubset(occTangentAngle2, occTangentAngle1, tangentAngle2, tangentAngle1))
        {
            float m1 = Wrap(occTangentAngle1 - tangentAngle2);
            float m2 = Wrap(occTangentAngle2 - tangentAngle2);
                
            numIntervals = AddInterval(intervals, float2(m2, m1), numIntervals);
        }
        else if (!IntervalIsDisjoint(tangentAngle2, tangentAngle1, occTangentAngle2, occTangentAngle1))
        {
            if (Wrap(tangentAngle2 - occTangentAngle2) <= PI)
            {
                float m1 = Wrap(occTangentAngle1 - tangentAngle2);
                numIntervals = AddInterval(intervals, float2(0, m1), numIntervals);
            }
            else
            {
                float m2 = Wrap(occTangentAngle2 - tangentAngle2);
                numIntervals = AddInterval(intervals, float2(m2, lightHalfAngularRadius * 2), numIntervals);
            }
        }
        if (IsFullInterval(intervals[0], lightHalfAngularRadius)) occluded = true;
    }
    
    for (int i = 0; i < OccluderCount; i++)
    {
        if (occluded) continue;
        
        float4 v = OccluderTextureBuffer.Sample(OccluderTextureSampler, float2(1.0 / OccluderCount * (i + 0.5), 0.5));

        float2 lineStart = float2(v.x, v.y) - pixel;
        float2 lineEnd = float2(v.z, v.w) - pixel;

        float pointAngle1 = Wrap(atan2(lineStart.y, lineStart.x));
        float pointAngle2 = Wrap(atan2(lineEnd.y, lineEnd.x));

        if (Wrap(pointAngle1 - pointAngle2) > PI) 
        {
            float temp = pointAngle1;
            pointAngle1 = pointAngle2;
            pointAngle2 = temp;
        }

        if (IsInFrontLight(lineStart, pixel, pointAngle1, tangentAngle1, tangentAngle2, lightDistance) &&
            IsInFrontLight(lineEnd, pixel, pointAngle2, tangentAngle1, tangentAngle2, lightDistance))
        {
            if (IntervalIsSubset(tangentAngle2, tangentAngle1, pointAngle2, pointAngle1)) occluded = true;
            else if (IntervalIsSubset(pointAngle2, pointAngle1, tangentAngle2, tangentAngle1))
            {
                float m1 = Wrap(pointAngle1 - tangentAngle2);
                float m2 = Wrap(pointAngle2 - tangentAngle2);
                
                numIntervals = AddInterval(intervals, float2(m2, m1), numIntervals);
            }
            else if (!IntervalIsDisjoint(tangentAngle2, tangentAngle1, pointAngle2, pointAngle1))
            {
                if (Wrap(tangentAngle2 - pointAngle2) <= PI)
                {
                    float m1 = Wrap(pointAngle1 - tangentAngle2);
                    numIntervals = AddInterval(intervals, float2(0, m1), numIntervals);
                }
                else
                {
                    float m2 = Wrap(pointAngle2 - tangentAngle2);
                    numIntervals = AddInterval(intervals, float2(m2, lightHalfAngularRadius * 2), numIntervals);
                }
            }
            if (IsFullInterval(intervals[0], lightHalfAngularRadius)) occluded = true;
        }
    }
    
    float occlusion;
    if (occluded) occlusion = 1;
    else occlusion = GetIntervalLength(intervals, numIntervals) / (lightHalfAngularRadius * 2);
    return occlusion;
}

float4 MainPS(VertexShaderOutput input) : SV_TARGET
{
    float occlusion = CalculateOcclusion(input.TexCoords);
    return float4(0, 0, 0, saturate(occlusion));
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};