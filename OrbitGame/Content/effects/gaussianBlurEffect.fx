#define VS_SHADERMODEL vs_6_0
#define PS_SHADERMODEL ps_6_0

matrix Projection;
matrix World;

float2 TexelSize;

Texture2D SpriteTexture : register(t0);;
SamplerState SpriteTextureSampler : register(s0);;

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

float2 Texel(int x, int y) 
{
    return float2(TexelSize.x * x, TexelSize.y * y);
}

float4 BasicColourPS(VertexShaderOutput input) : SV_TARGET
{
    return
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel(-2, -2)) / 273 * 1 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel(-2, -1)) / 273 * 4 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel(-2,  0)) / 273 * 7 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel(-2,  1)) / 273 * 4 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel(-2,  2)) / 273 * 1 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel(-1, -2)) / 273 * 4 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel(-1, -1)) / 273 * 16 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel(-1,  0)) / 273 * 26 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel(-1,  1)) / 273 * 16 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel(-1,  2)) / 273 * 4 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel( 0, -2)) / 273 * 7 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel( 0, -1)) / 273 * 26 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel( 0,  0)) / 273 * 41 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel( 0,  1)) / 273 * 26 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel( 0,  2)) / 273 * 7 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel( 1, -2)) / 273 * 4 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel( 1, -1)) / 273 * 16 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel( 1,  0)) / 273 * 26 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel( 1,  1)) / 273 * 16 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel( 1,  2)) / 273 * 4 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel( 2, -2)) / 273 * 1 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel( 2, -1)) / 273 * 4 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel( 2,  0)) / 273 * 7 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel( 2,  1)) / 273 * 4 +
    SpriteTexture.Sample(SpriteTextureSampler, input.TexCoords + Texel( 2,  2)) / 273 * 1;
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL BasicColourPS();
    }
};