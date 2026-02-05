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

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 FragmentPosition : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, mul(world, projection));
    output.FragmentPosition = input.Position;
    output.Color = input.Color;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    if (length(input.FragmentPosition - float4(0, 0, 0, 1)) < 1) {
        return float4(1, 1, 1, 1);
    }
    return float4(0, 0, 0, 0);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};