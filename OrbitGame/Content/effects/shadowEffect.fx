#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define SAMPLES 2
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

float CalculateOcclusion(float2 tex) 
{
    float result = 1;
    for (int i = 0; i < SAMPLES; i++) {
        float2 pixel = float2(tex.x * ScreenSize.x, tex.y * ScreenSize.y);
        float2 lightOrigin = LightCenter;
        if (length(pixel - lightOrigin) < LightRadius) return 1;
        float2 lightDisplacement = lightOrigin - pixel;
        float tangentDistance = sqrt(dot(lightDisplacement, lightDisplacement) - pow(LightRadius, 2));
        float centerAngle = atan2(-lightDisplacement.y, -lightDisplacement.x);
        float tangentAngle = atan(tangentDistance / LightRadius);
        float worldTangentAngle = 0;
        if (i == 0) worldTangentAngle = centerAngle + tangentAngle;
        if (i == 1) worldTangentAngle = centerAngle - tangentAngle;
        lightOrigin += LightRadius * float2(cos(worldTangentAngle), sin(worldTangentAngle));
        float2 ray = pixel;
        float2 diffVector = pixel - lightOrigin;
        float distance = length(diffVector);
        float2 direction = diffVector / distance;
        while (true) {
            float sdfDistance = SDFCircle(ray, OccluderCenter, OccluderRadius);
            if (sdfDistance <= 0) break;
            ray -= direction * sdfDistance;
            if (length(ray - pixel) >= distance) {
                result -= (float)1 / SAMPLES;
                break;
            }
        }
    }
    return result;
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