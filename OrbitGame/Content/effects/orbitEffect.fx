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

float4 Colour;
float2 Center;
float Eccentricity;
float SemiLatusRectum;

float2 TexelSize;

texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoords : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoords : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, mul(World, Projection));
    output.TexCoords = input.TexCoords;

    return output;
}

float4 MainPS_Pass1(in VertexShaderInput input) : COLOR
{
    float2 conicPos = input.TexCoords - Center;
    float angle = atan2(conicPos.y, conicPos.x);
    float dist = length(conicPos);
    if (dist < SemiLatusRectum / (1 + Eccentricity * cos(angle))) {
        return Colour;
    }
    return float4(0, 0, 0, 0);
}

float4 MainPS_Pass2(in VertexShaderInput input) : COLOR
{
    float4 pixelColour = tex2D(SpriteTextureSampler, input.TexCoords);
    if (all(pixelColour == Colour)) {
        return Colour;
    }
    float4 topColour = tex2D(SpriteTextureSampler, input.TexCoords + float2(0, -TexelSize.y));
    float4 leftColour = tex2D(SpriteTextureSampler, input.TexCoords + float2(0, TexelSize.y));
    float4 rightColour = tex2D(SpriteTextureSampler, input.TexCoords + float2(TexelSize.x, 0));
    float4 bottomColour = tex2D(SpriteTextureSampler, input.TexCoords + float2(-TexelSize.x, 0));
    if (all(topColour == leftColour) && all(topColour == rightColour) && all(topColour == bottomColour)) {
        return float4(0, 0, 0, 0);
    }
    return float4(Colour.x, 0, 0, 1);
}

float4 MainPS_Pass3(in VertexShaderInput input) : COLOR {
    float4 pixelColour = tex2D(SpriteTextureSampler, input.TexCoords);
    if (all(pixelColour == Colour)) {
        return float4(0, 0, 0, 0);
    }
    return pixelColour;
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS_Pass1();
    }
    pass P1
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS_Pass2();
    }
    pass P2
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS_Pass3();
    }
};