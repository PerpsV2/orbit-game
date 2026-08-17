#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define SAMPLES 10
#define TAU 6.28318531
#define PI 3.14159265358969323
#define HPI 1

matrix Projection;
matrix World;

float2 LightCenter;
float LightRadius;
float2 OccluderCenter;


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

float CalculateOcclusion(float2 tex) 
{
    float2 pixel = float2(tex.x * ScreenSize.x, tex.y * ScreenSize.y);
    float pixelOccluderDistance = length(OccluderCenter - pixel);
    float pixelZ = 0;
    if (pixelOccluderDistance < OccluderRadius)
        pixelZ = 1.0001 * OccluderRadius * sin(acos(pixelOccluderDistance / OccluderRadius));
    float3 lightPos = float3(LightCenter.x - pixel.x, LightCenter.y - pixel.y, -pixelZ);
    float3 occluderPos = float3(OccluderCenter.x - pixel.x, OccluderCenter.y - pixel.y, -pixelZ);
    
    if (length(lightPos) < LightRadius) return 0;
    if (length(occluderPos) < OccluderRadius) return 1;
    
    float lightAngularRadius = asin(LightRadius / length(lightPos));
    float lightSolidAngle = TAU * (1 - cos(lightAngularRadius));
    float occluderAngularRadius = asin(OccluderRadius / length(occluderPos));
    float occluderSolidAngle = TAU * (1 - cos(occluderAngularRadius));
    
    float lightAzimuthAngle = atan2(lightPos.y, lightPos.x);
    float lightAltitudeAngle = acos(lightPos.z / length(lightPos));
    float occluderAzimuthAngle = atan2(occluderPos.y, occluderPos.x);
    float occluderAltitudeAngle = acos(occluderPos.z / length(occluderPos));
    
    float angleDifference = acos(
        cos(lightAltitudeAngle) * cos(occluderAltitudeAngle) + 
        sin(lightAltitudeAngle) * sin(occluderAltitudeAngle) * cos(lightAzimuthAngle - occluderAzimuthAngle)
        );
    
    float s = 0.5 * (lightAngularRadius + occluderAngularRadius + angleDifference);
    float k = sqrt((sin(s - lightAngularRadius) * sin(s - occluderAngularRadius) * sin(s - angleDifference)) / sin(s));
    float A = 2 * atan(k / sin(s - lightAngularRadius));
    float B = 2 * atan(k / sin(s - occluderAngularRadius));
    float C = 2 * atan(k / sin(s - angleDifference));
    float delta1 = A + B + C - PI;
    float delta2 = A + B + C - PI;
    float area1 = 2 * B * (1 - cos(lightAngularRadius));
    float area2 = 2 * A * (1 - cos(occluderAngularRadius));
    float eclipseSolidAngle = area1 + area2 - (delta1 + delta2);
    
    if (angleDifference >= lightAngularRadius + occluderAngularRadius) return 0;
    else if (angleDifference <= lightAngularRadius - occluderAngularRadius) 
        return 1 - (lightSolidAngle - occluderSolidAngle) / lightSolidAngle;
    else if (angleDifference <= occluderAngularRadius - lightAngularRadius) return 1;
    else return 1 - (lightSolidAngle - eclipseSolidAngle) / lightSolidAngle;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float occlusion = CalculateOcclusion(input.TexCoords);
    float pixelZ = CalculatePixelHeight(float2(input.TexCoords.x * ScreenSize.x, input.TexCoords.y * ScreenSize.y));
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