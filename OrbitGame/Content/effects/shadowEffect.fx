#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define SAMPLES 10
#define TAU 6.28318

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

float SDFCircle(float2 pos, float2 center, float radius) 
{
    return length(pos - center) - radius;
}

float Rand(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

float CalculateOcclusionRay(float2 lightOrigin, float2 lightDestination) {
    float2 ray = lightDestination;
    float2 diffVector = lightDestination - lightOrigin;
    float distance = length(diffVector);
    float2 direction = diffVector / distance;
    while (true) {
        float sdfDistance = SDFCircle(ray, OccluderCenter, OccluderRadius);
        if (sdfDistance <= 0) return 1;
        ray -= direction * sdfDistance;
        if (length(ray - lightDestination) >= distance) return 0;
    }
    return 1;
}

float Modulo(float left, float right) {
    return left - right * floor(left / right);
}

float2 CalculateTangentIntersectionAtanAngle(float2 circleCenter, float radius, float2 externalPoint) {
    float2 displacement = circleCenter - externalPoint;
    float tangentDistance = sqrt(dot(displacement, displacement) - pow(radius, 2));
    float centerAngle = atan2(displacement.y, displacement.x);
    float tangentAngle = atan(radius / tangentDistance);
    float2 result = float2(Modulo(centerAngle - tangentAngle, TAU), Modulo(centerAngle + tangentAngle, TAU));
    if (result.y < result.x) result.y += TAU;
    return result;
}

float CalculateAngleIntervalIntersectionAmount(float2 interval1, float2 interval2) {
    float d1 = max(0, min(min(interval1.y, TAU), min(interval2.y, TAU)) - max(interval1.x, interval2.x));
    float d2 = max(0, min(min(interval1.y, TAU), max(0, interval2.y - TAU)) - max(interval1.x, 0));
    float d3 = max(0, min(max(0, interval1.y - TAU), min(interval2.y, TAU)) - max(0, interval2.x));
    float d4 = max(0, min(max(0, interval1.y - TAU), max(0, interval2.y - TAU)));
    return d1 + d2 + d3 + d4;
}

float CalculateOcclusion(float2 tex) 
{
    float2 pixel = float2(tex.x * ScreenSize.x, tex.y * ScreenSize.y);
    if (SDFCircle(pixel, OccluderCenter, OccluderRadius) <= 0) return 1;
    float2 lightOccluderDiff = OccluderCenter - LightCenter;
    float lightOccluderDist = length(lightOccluderDiff);
    float2 antiOccluderCenter = LightCenter + (2 * LightRadius - lightOccluderDist) / lightOccluderDist * lightOccluderDiff;
    if (length(antiOccluderCenter - pixel) < length(OccluderCenter - pixel)) return 0;
    float2 lightTangentAngles = CalculateTangentIntersectionAtanAngle(LightCenter, LightRadius, pixel);
    float2 occluderTangentAngles = CalculateTangentIntersectionAtanAngle(OccluderCenter, OccluderRadius, pixel);
    
    float intersectionAngle = CalculateAngleIntervalIntersectionAmount(lightTangentAngles, occluderTangentAngles);
    float lightConeAngle = lightTangentAngles.y - lightTangentAngles.x;
    return intersectionAngle / lightConeAngle;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float occlusion = CalculateOcclusion(input.TexCoords);
    return float4(0, 0, 0, occlusion);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};