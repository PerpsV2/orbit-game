#define VS_SHADERMODEL vs_6_0
#define PS_SHADERMODEL ps_6_0

#define PI 3.14159265359
#define TAU 6.28318530718

#define MAX_DISJOINT_INTERVALS 8

#define EPSILON 0.0001f

#define CSWAP(i, j) if (intervals[i].x > intervals[j].x) { float2 temp = intervals[i]; intervals[i] = intervals[j]; intervals[j] = temp; }

matrix Projection;
matrix World;

float2 LightCenter;
float LightRadius;

Texture2D OccluderTextureBuffer : register(t0);;
SamplerState OccluderTextureSampler : register(t0);;

int OccluderCount;

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

void SortIntervals2(inout float2 intervals[MAX_DISJOINT_INTERVALS])
{
    CSWAP(0, 1);
}

void SortIntervals3(inout float2 intervals[MAX_DISJOINT_INTERVALS])
{
    CSWAP(0, 2);
    CSWAP(0, 1);
    CSWAP(1, 2);
}

void SortIntervals4(inout float2 intervals[MAX_DISJOINT_INTERVALS])
{
    CSWAP(0, 2); CSWAP(1, 3);
    CSWAP(0, 1); CSWAP(2, 3);
    CSWAP(1, 2);
}

void SortIntervals5(inout float2 intervals[MAX_DISJOINT_INTERVALS])
{
    CSWAP(0, 3); CSWAP(1, 4);
    CSWAP(0, 2); CSWAP(1, 3);
    CSWAP(0, 1); CSWAP(2, 4);
    CSWAP(1, 2); CSWAP(3, 4);
    CSWAP(2, 3);
}

void SortIntervals6(inout float2 intervals[MAX_DISJOINT_INTERVALS])
{
    CSWAP(0, 5); CSWAP(1, 3); CSWAP(2, 4);
    CSWAP(1, 2); CSWAP(3, 4);
    CSWAP(0, 3); CSWAP(2, 5);
    CSWAP(0, 1); CSWAP(2, 3); CSWAP(4, 5);
    CSWAP(1, 2); CSWAP(3, 4);
}

void SortIntervals7(inout float2 intervals[MAX_DISJOINT_INTERVALS])
{
    CSWAP(0, 6); CSWAP(2, 3); CSWAP(4, 5);
    CSWAP(0, 2); CSWAP(1, 4); CSWAP(3, 6);
    CSWAP(0, 1); CSWAP(2, 5); CSWAP(3, 4);
    CSWAP(1, 2); CSWAP(4, 6);
    CSWAP(2, 3); CSWAP(4, 5);
    CSWAP(1, 2); CSWAP(3, 4); CSWAP(5, 6);
}

void SortIntervals8(inout float2 intervals[MAX_DISJOINT_INTERVALS])
{
    CSWAP(0, 2); CSWAP(1, 3); CSWAP(4, 6); CSWAP(5, 7);
    CSWAP(0, 4); CSWAP(1, 5); CSWAP(2, 6); CSWAP(3, 7);
    CSWAP(0, 1); CSWAP(2, 3); CSWAP(4, 5); CSWAP(6, 7);
    CSWAP(2, 4); CSWAP(3, 5);
    CSWAP(1, 4); CSWAP(3, 6);
    CSWAP(1, 2); CSWAP(3, 4); CSWAP(5, 6);
}

void SortIntervals9(inout float2 intervals[MAX_DISJOINT_INTERVALS])
{
    
}

void SortIntervals10(inout float2 intervals[MAX_DISJOINT_INTERVALS])
{
    
}

void SortIntervals11(inout float2 intervals[MAX_DISJOINT_INTERVALS])
{
    
}

void SortIntervals12(inout float2 intervals[MAX_DISJOINT_INTERVALS])
{
    
}

void SortIntervals13(inout float2 intervals[MAX_DISJOINT_INTERVALS])
{
    
}

void SortIntervals14(inout float2 intervals[MAX_DISJOINT_INTERVALS])
{
    
}

void SortIntervals15(inout float2 intervals[MAX_DISJOINT_INTERVALS])
{
    
}

void SortIntervals16(inout float2 intervals[MAX_DISJOINT_INTERVALS])
{
    
}

int AddInterval(out float2 intervals[MAX_DISJOINT_INTERVALS], float2 newInterval, int numIntervals)
{
    if (numIntervals >= MAX_DISJOINT_INTERVALS - 1) return MAX_DISJOINT_INTERVALS - 1;
    
    if (numIntervals == 0)
    {
        intervals[0] = newInterval;
        numIntervals++;
    }
    else if (newInterval.x > intervals[numIntervals - 1].x)
    {
        intervals[numIntervals] = newInterval;
        numIntervals++;
    }
    else
    {
        for (int i = 0; i < numIntervals; i++)
        {
            if (newInterval.x < intervals[i].x)
            {
                for (int j = numIntervals; j > i; j--)
                    intervals[j] = intervals[j - 1];
                intervals[i] = newInterval;
                numIntervals++;
                break;
            }
        }
    }
    
    int c = 0;
    int i = 1;
    while (i < numIntervals)
    {
        if (intervals[i].x <= intervals[c].y || abs(intervals[i].x - intervals[c].y) < EPSILON)
            intervals[c].y = max(intervals[c].y, intervals[i].y);
        else
        {
            c++;
            intervals[c] = intervals[i];
        }
        i++;
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
    
    for (int i = 0; i < OccluderCount; i++)
    {
        if (!occluded)
        {
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