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

Texture2D TextureA;
sampler2D TextureSamplerA = sampler_state
{
    Texture = <TextureA>;
};

float2 center;
float eccentricity;
float semiLatusRectum;

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

    output.Position = mul(input.Position, mul(world, projection));
    output.TexCoords = input.TexCoords;

    return output;
}

float4 MainPS_Pass1(in VertexShaderInput input) : COLOR
{
    float2 relative = input.TexCoords - center;
    float angle = atan2(relative.y, relative.x);
    float dist = length(relative);
    float conicDist = abs(semiLatusRectum / (1 + eccentricity * cos(angle)));
    if (dist < conicDist) {
        return colour;
    }
    return float4(0, 0, 0, 0);
}

float4 MainPS_Pass2(in VertexShaderInput input) : COLOR
{
    float2 relative = input.TexCoords - center;
    float angle = atan2(relative.y, relative.x);
    float dist = length(relative);
    float conicDist = abs(semiLatusRectum / (1 + eccentricity * cos(angle)));
    if (dist < conicDist - 0.01f) {
        return float4(colour.x, colour.y, 0, 1);
    }
    return float4(0, 0, 0, 0);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS_Pass1();
    }
    pass P1 {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS_Pass2();
    }
};