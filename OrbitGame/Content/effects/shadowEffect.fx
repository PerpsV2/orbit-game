#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_6_0
    #define PS_SHADERMODEL ps_6_0
#endif

#define SAMPLES 10
#define TAU 6.28318530718

matrix Projection;
matrix World;

float2 LightCenter;
float LightRadius;
float2 OccluderCenter;
float OccluderRadius;

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

float CalculateOcclusion(float2 tex) 
{
    float2 pixel = float2(tex.x * ScreenSize.x, tex.y * ScreenSize.y);
    
    float2 lightPosition = LightCenter - pixel;
    float2 occluderPosition = OccluderCenter - pixel;
    
    float lightDistance = length(lightPosition);
    float occluderDistance = length(occluderPosition);
    
    if (lightDistance < occluderDistance) return 0;
    if (occluderDistance < OccluderRadius) return 1;
    
    float lightCenterAngle = atan2(lightPosition.y, lightPosition.x);
    float occluderCenterAngle = atan2(occluderPosition.y, occluderPosition.x);
    
    float lightAngularRadius = asin(LightRadius / lightDistance);
    float occluderAngularRadius = asin(OccluderRadius / occluderDistance);
    
    float2 occluderInterval = float2(Mod(occluderCenterAngle - occluderAngularRadius, TAU), Mod(occluderCenterAngle + occluderAngularRadius, TAU));
    if (occluderInterval.x > occluderInterval.y) occluderInterval.y += TAU;
    
    float2 lightInterval = float2(Mod(lightCenterAngle - lightAngularRadius, TAU), Mod(lightCenterAngle + lightAngularRadius, TAU));
    if (lightInterval.x > lightInterval.y) lightInterval.y += TAU;
    
    float intersection = CalculateAngleIntervalIntersectionAmount(occluderInterval, lightInterval);
    float occlusion = intersection / (lightInterval.y - lightInterval.x);
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