#define VS_SHADERMODEL vs_6_0
#define PS_SHADERMODEL ps_6_0

matrix Projection;
matrix World;
float4 Colour;

Texture2D SpriteTexture;
sampler SpriteTextureSampler;

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

float4 BasicColourPS(VertexShaderOutput input) : SV_TARGET
{
    return Colour;
}

float4 RenderTargetPS(VertexShaderOutput input) : SV_TARGET 
{
    return SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL BasicColourPS();
    }
};

technique RenderTargetDrawing 
{
    pass P0 
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL RenderTargetPS();
    }
};