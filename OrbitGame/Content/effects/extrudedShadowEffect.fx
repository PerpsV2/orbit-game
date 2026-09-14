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

half CalculateOcclusion(half2 tex)
{
    half2 pixel = half2(tex.x * ScreenSize.x, tex.y * ScreenSize.y);

    for (int i = 0; i < OccluderCount; i++)
        half4 v = OccluderDataTexture.Sample(OccluderDataTextureSampler, half2(1.0 / OccluderCount * (i + 0.5), 0.5));
    {
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