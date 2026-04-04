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
float Eccentricity;
float SemiLatusRectum;
float ArgumentOfPeriapsis;
float2 Center;

float2 TexelSize;

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

bool WithinConic(float2 texCoords) 
{
    float angle = atan2(texCoords.x, texCoords.y);
    float conicDistance = SemiLatusRectum / (1 + Eccentricity * cos(angle - ArgumentOfPeriapsis));
    if (conicDistance < 0) return true;
    return length(texCoords) < conicDistance;
}

float4 MainPS_Pass1(VertexShaderOutput input) : COLOR
{
    float2 centerRelativeCoords = input.TexCoords - Center + TexelSize / 2;

    float2 rightTexel = centerRelativeCoords + float2(TexelSize.x, 0);
    float2 topTexel = centerRelativeCoords + float2(0, TexelSize.y);
    float2 leftTexel = centerRelativeCoords + float2(-TexelSize.x, 0);
    float2 bottomTexel = centerRelativeCoords + float2(0, -TexelSize.y);
    
    if (WithinConic(rightTexel) && WithinConic(topTexel) && WithinConic(leftTexel) && WithinConic(bottomTexel)) {
        return float4(0, 0, 0, 0);
    }
    if (!WithinConic(rightTexel) && !WithinConic(topTexel) && !WithinConic(leftTexel) && !WithinConic(bottomTexel)) {
        return float4(0, 0, 0, 0);
    }
    return Colour;
}

technique BasicOrbitDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS_Pass1();
    }
};