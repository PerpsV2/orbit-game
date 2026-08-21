#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define SAMPLES 10
#define PI 3.14159265359
#define TAU 6.28318530718

matrix Projection;
matrix World;

float2 LightCenter;
float LightRadius;
float2 OccluderVertices[16];
int VerticesActiveCount;

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

float CalculateAngleIntervalIntersectionAmount(float2 interval1, float2 interval2) {
    float d1 = max(0, min(TAU, min(interval1.y, interval2.y)) - max(interval1.x, interval2.x));
    float d2 = max(0, min(min(interval1.y, TAU), max(0, interval2.y - TAU)) - max(interval1.x, 0));
    float d3 = max(0, min(max(0, interval1.y - TAU), min(interval2.y, TAU)) - max(0, interval2.x));
    float d4 = max(0, min(max(0, interval1.y - TAU), max(0, interval2.y - TAU)));
    return d1 + d2 + d3 + d4;
}

float Mod(float value, float mod) 
{
    return value - mod * floor(value / mod);
}

float GetOccluderVertexAngle(int index, float2 pixel) 
{
    return Mod(atan2(OccluderVertices[index].y - pixel.y, OccluderVertices[index].x - pixel.x), TAU);
}

float GetAngleDifference(float start, float end)
{
    if (Mod(start - end, TAU) >= PI) return Mod(end - start, TAU);
    return -Mod(start - end, TAU);
}

float2 CalculateOccluderInterval(float2 pixel)
{
    float minAngle = 0;
    float maxAngle = 0;
    float startAngle = GetOccluderVertexAngle(0, pixel);
    float currAngle = startAngle;
    float nextAngle;
    float angleOffset = 0;
    for (int currIndex = 0; currIndex < VerticesActiveCount; ++currIndex) 
    {
        int nextIndex = Mod(currIndex + 1, VerticesActiveCount);
        nextAngle = GetOccluderVertexAngle(nextIndex, pixel);
        angleOffset += GetAngleDifference(currAngle, nextAngle);
        currAngle = nextAngle;
        if (angleOffset < minAngle) minAngle = angleOffset;
        if (angleOffset > maxAngle) maxAngle = angleOffset;
    }
    
    minAngle = Mod(startAngle + minAngle, TAU);
    maxAngle = Mod(startAngle + maxAngle, TAU);
    if (minAngle > maxAngle) maxAngle += TAU;
    
    return float2(minAngle, maxAngle);
}

float CalculateOcclusion(float2 tex) 
{
    float2 pixel = float2(tex.x * ScreenSize.x, tex.y * ScreenSize.y);
    
    float2 lightPosition = LightCenter - pixel;
    float lightDistance = length(lightPosition);
    float lightCenterAngle = atan2(lightPosition.y, lightPosition.x);
    float lightAngularRadius = asin(LightRadius / lightDistance);
    
    //float2 occluderPosition = OccluderVertices[0] - pixel;
    //float occluderDistance = length(occluderPosition);
    //float occluderCenterAngle = atan2(occluderPosition.y, occluderPosition.x);
    //float occluderAngularRadius = asin(length(OccluderVertices[VerticesActiveCount - 1] - OccluderVertices[0]) / occluderDistance);
    
    float2 occluderInterval = CalculateOccluderInterval(pixel);
    
    float2 lightInterval = float2(Mod(lightCenterAngle - lightAngularRadius, TAU), Mod(lightCenterAngle + lightAngularRadius, TAU));
    if (lightInterval.x > lightInterval.y) lightInterval.y += TAU;
    
    float intersection = CalculateAngleIntervalIntersectionAmount(occluderInterval, lightInterval);
    float occlusion = intersection / (lightInterval.y - lightInterval.x);
    return occlusion;
}

float4 MainPS(VertexShaderOutput input) : COLOR
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