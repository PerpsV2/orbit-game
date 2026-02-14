#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix projection;
matrix world;
float4 colour;

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

    output.Position = mul(input.Position, mul(world, projection));
    output.TexCoords = input.TexCoords;

    return output;
}

float4 BasicColourPS(VertexShaderOutput input) : COLOR
{
    return colour;
}

float4 RenderTargetPS(VertexShaderOutput input) : COLOR 
{
    return tex2D(SpriteTextureSampler, input.TexCoords);
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