#define VS_SHADERMODEL vs_6_0
#define PS_SHADERMODEL ps_6_0

matrix Projection;
matrix World;

float2 LightCenter;
float4 LightColour;
float DistanceScale;
float2 ScreenSize;

Texture2D ShadowMask : register(t0);;
SamplerState ShadowMaskSampler : register(s0);;

Texture2D OccluderMask : register(t1);;
SamplerState OccluderMaskSampler : register(s1);;

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

float4 MainPS(VertexShaderOutput input) : SV_TARGET
{
    float distance = length(float2(input.TexCoords.x * ScreenSize.x, input.TexCoords.y * ScreenSize.y) - LightCenter);
    return saturate(LightColour / pow(distance * DistanceScale, 2) * 
        ShadowMask.Sample(ShadowMaskSampler, input.TexCoords) * 
        OccluderMask.Sample(OccluderMaskSampler, input.TexCoords));
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};