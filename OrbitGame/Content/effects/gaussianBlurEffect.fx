#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix Projection;
matrix World;

float2 TexelSize;

texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

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

float4 BasicColourPS(VertexShaderOutput input) : COLOR
{
    return
    tex2D(SpriteTextureSampler, input.TexCoords + Texel(-2, -2)) / 273 * 1 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel(-2, -1)) / 273 * 4 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel(-2,  0)) / 273 * 7 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel(-2,  1)) / 273 * 4 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel(-2,  2)) / 273 * 1 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel(-1, -2)) / 273 * 4 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel(-1, -1)) / 273 * 16 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel(-1,  0)) / 273 * 26 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel(-1,  1)) / 273 * 16 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel(-1,  2)) / 273 * 4 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel( 0, -2)) / 273 * 7 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel( 0, -1)) / 273 * 26 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel( 0,  0)) / 273 * 41 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel( 0,  1)) / 273 * 26 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel( 0,  2)) / 273 * 7 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel( 1, -2)) / 273 * 4 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel( 1, -1)) / 273 * 16 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel( 1,  0)) / 273 * 26 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel( 1,  1)) / 273 * 16 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel( 1,  2)) / 273 * 4 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel( 2, -2)) / 273 * 1 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel( 2, -1)) / 273 * 4 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel( 2,  0)) / 273 * 7 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel( 2,  1)) / 273 * 4 +
    tex2D(SpriteTextureSampler, input.TexCoords + Texel( 2,  2)) / 273 * 1;
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL BasicColourPS();
    }
};