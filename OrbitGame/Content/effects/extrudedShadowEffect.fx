#define VS_SHADERMODEL vs_6_0
#define PS_SHADERMODEL ps_6_0

#define PI 3.14159265359
#define TAU 6.28318530718

#define MAX_DISJOINT_INTERVALS 16

#define EPSILON 0.0001

matrix Projection;
matrix World;

Texture2D OccluderDataTexture : register(t0);
SamplerState OccluderDataTextureSampler : register(t0);
int OccluderCount;

half2 LightCenter;

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

half Sec(half v)
{
    return 1.0 / cos(v);
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

half GetIntervalLength(half2 intervals[MAX_DISJOINT_INTERVALS], int numIntervals)
{
    half total = 0;
    for (int i = 0; i < numIntervals; ++i)
        total += intervals[i].y - intervals[i].x;
    return total;
}

half CalculateOcclusion(half2 tex)
{
    half2 pixel = half2(tex.x * ScreenSize.x, tex.y * ScreenSize.y);

    half2 intervals[MAX_DISJOINT_INTERVALS];
    int numIntervals = 0;
    
    for (int i = 0; i < OccluderCount; i++)
    {
        half4 v = OccluderDataTexture.Sample(OccluderDataTextureSampler, half2(1.0 / OccluderCount * (i + 0.5), 0.5));
        
        half occluderAngle = v.x;
        half lightRadius = v.y;
        half occluderRadius = v.z;
        
        half2 occPos = half2(cos(occluderAngle), sin(occluderAngle));
        
        half pixelAngle = atan2(LightCenter.y - pixel.y, LightCenter.x - pixel.x);
        
        half intervalScaleFactor = 1.0 / (2 * lightRadius) * sign(cos(pixelAngle));
        half a = Sec(pixelAngle);
        half b = tan(pixelAngle);
        half intervalStart = ((lightRadius - occluderRadius) * a + b * occPos.x - occPos.y) / abs(a) * intervalScaleFactor;
        half intervalEnd = ((lightRadius + occluderRadius) * a + b * occPos.x - occPos.y) / abs(a) * intervalScaleFactor;
        if (intervalStart > intervalEnd)
        {
            half temp = intervalStart;
            intervalStart = intervalEnd;
            intervalEnd = temp;
        }
        intervalStart = clamp(intervalStart, 0, 1);
        intervalEnd = clamp(intervalEnd, 0, 1);
        
        numIntervals = AddInterval(intervals, half2(intervalStart, intervalEnd), numIntervals);
    }
    
    return GetIntervalLength(intervals, numIntervals);
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