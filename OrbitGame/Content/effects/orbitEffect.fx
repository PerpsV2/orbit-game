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
float Periapsis;
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

float4 ConicSection_PS(in VertexShaderInput input) : COLOR
{
    float2 conicPos = input.TexCoords - Center;
    float angle = atan2(conicPos.y, conicPos.x);
    float dist = length(conicPos);
    float cosValue = Eccentricity * cos(angle - Periapsis);
    if (cosValue < -1) return Colour;
    else if (cosValue > -1) {
        if (dist < SemiLatusRectum / (1 + Eccentricity * cos(angle - Periapsis))) return Colour;
    }
    return float4(0, 0, 0, 0);
}

float4 Outline_PS(in VertexShaderInput input) : COLOR 
{
    float4 pixelColour = tex2D(SpriteTextureSampler, input.TexCoords);
    if (all(pixelColour == Colour)) return Colour;
    float4 topPixelColour = tex2D(SpriteTextureSampler, input.TexCoords + float2(0, TexelSize.y));
    float4 leftPixelColour = tex2D(SpriteTextureSampler, input.TexCoords + float2(-TexelSize.x, 0));
    float4 rightPixelColour = tex2D(SpriteTextureSampler, input.TexCoords + float2(TexelSize.x, 0));
    float4 bottomPixelColour = tex2D(SpriteTextureSampler, input.TexCoords + float2(0, -TexelSize.y));
    if (all(topPixelColour == Colour) || all(bottomPixelColour == Colour) || 
        all(leftPixelColour == Colour) || all(rightPixelColour == Colour)) {
        return Colour;
    }
    return pixelColour;
}

float4 Mask_PS(in VertexShaderInput input) : COLOR 
{
    float4 pixelColour = tex2D(SpriteTextureSampler, input.TexCoords);
    float2 conicPos = input.TexCoords - Center;
    float angle = atan2(conicPos.y, conicPos.x);
    float dist = length(conicPos);
    float cosValue = Eccentricity * cos(angle - Periapsis);
    if (cosValue < -1) return float4(0, 0, 0, 0);
    else if (cosValue > -1) {
        if (dist < SemiLatusRectum / (1 + Eccentricity * cos(angle - Periapsis))) return float4(0, 0, 0, 0);
    }
    return pixelColour;
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL ConicSection_PS();
    }
    pass P1
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL Outline_PS();
    }
    pass P2
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL Mask_PS();
    }
};