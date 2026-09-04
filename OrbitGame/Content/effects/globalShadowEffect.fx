#define VS_SHADERMODEL vs_6_0
#define PS_SHADERMODEL ps_6_0

#define PI 3.14159265359
#define TAU 6.28318530718

#define MAX_DISJOINT_INTERVALS 16

#define EPSILON 0.0001f

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

int AddInterval(out float2 intervals[MAX_DISJOINT_INTERVALS], int numIntervals)
{
    for (int i = 0; i < numIntervals - 1; ++i)
    {
        for (int j = i + 1; j > 0; --j)
        {
            if (intervals[j - 1].x > intervals[j].x)
            {
                float2 temp = intervals[j - 1];
                intervals[j - 1] = intervals[j];
                intervals[j] = temp;
            }
        }
    }
    
    int c = 0;
    int i = 1;
    while (i < numIntervals)
    {
        if (intervals[i].x <= intervals[c].y || abs(intervals[i].x - intervals[c].y) < EPSILON)
        {
            intervals[c].y = max(intervals[c].y, intervals[i].y);
        }
        else
        {
            c++;
            intervals[c] = intervals[i];
        }
        i++;
    }
    
    return c + 1;
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
        
            if ((
                Wrap(pointAngle1 - tangentAngle2) > Wrap(tangentAngle1 - tangentAngle2) && 
                Wrap(pointAngle2 - tangentAngle2) > Wrap(tangentAngle1 - tangentAngle2)
                ) || (
                    pow(lineStart.x, 2) + pow(lineStart.y, 2) <= lightDistance * lightDistance - LightRadius * LightRadius &&
                    pow(lineEnd.x, 2) + pow(lineEnd.y, 2) <= lightDistance * lightDistance - LightRadius * LightRadius && 
                    pow(lineStart.x - LightCenter.x + pixel.x, 2) + pow(lineStart.y - LightCenter.y + pixel.y, 2) >= LightRadius * LightRadius &&
                    pow(lineEnd.x - LightCenter.x + pixel.x, 2) + pow(lineEnd.y - LightCenter.y + pixel.y, 2) >= LightRadius * LightRadius))
            {
                if (IntervalIsSubset(tangentAngle2, tangentAngle1, pointAngle2, pointAngle1)) occluded = true;
                else if (IntervalIsSubset(pointAngle2, pointAngle1, tangentAngle2, tangentAngle1))
                {
                    float m1 = Wrap(pointAngle1 - tangentAngle2);
                    float m2 = Wrap(pointAngle2 - tangentAngle2);
                    
                    intervals[numIntervals++] = float2(m2, m1);
                    numIntervals = AddInterval(intervals, numIntervals);
                    if (IsFullInterval(intervals[0], lightHalfAngularRadius)) occluded = true;
                }
                else if (!IntervalIsDisjoint(tangentAngle2, tangentAngle1, pointAngle2, pointAngle1))
                {
                    if (Wrap(tangentAngle2 - pointAngle2) <= PI)
                    {
                        float m1 = Wrap(pointAngle1 - tangentAngle2);
                        
                        intervals[numIntervals++] = float2(0, m1);
                        numIntervals = AddInterval(intervals, numIntervals);
                        if (IsFullInterval(intervals[0], lightHalfAngularRadius)) occluded = true;
                    }
                    else
                    {
                        float m2 = Wrap(pointAngle2 - tangentAngle2);
                        
                        intervals[numIntervals++] = float2(m2, lightHalfAngularRadius * 2);
                        numIntervals = AddInterval(intervals, numIntervals);
                        if (IsFullInterval(intervals[0], lightHalfAngularRadius)) occluded = true;
                    }
                }
            }
        }
    }
    
    float occlusion;
    if (occluded) occlusion = 1;
    //if (GetIntervalLength(intervals, numIntervals, lightHalfAngularRadius * 2) > lightHalfAngularRadius * 2) return 0;
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