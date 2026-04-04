#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define TAU 6.28318530718

matrix Projection;
matrix World;

float4 Colour;
float Eccentricity;
float SemiLatusRectum;
float ArgumentOfPeriapsis;
float2 Center;

float StartAnomaly;
float EndAnomaly;

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

float Mod(float x, float y) {
    return x - y * floor(x / y);
}

bool WithinConic(float2 texCoords) 
{
    float angle = atan2(texCoords.x, texCoords.y);
    float trueAnomaly = angle - ArgumentOfPeriapsis;
    float conicDistance = SemiLatusRectum / (1 + Eccentricity * cos(trueAnomaly));
    if (conicDistance < 0) return true;
    return length(texCoords) < conicDistance;
}

float4 DefaultPS(VertexShaderOutput input) : COLOR
{
    float2 centerRelativeCoords = input.TexCoords - Center + TexelSize / 2;

    float trueAnomaly = Mod((atan2(centerRelativeCoords.x, centerRelativeCoords.y) - ArgumentOfPeriapsis), TAU);
    if (trueAnomaly < StartAnomaly || trueAnomaly > EndAnomaly) return float4(0, 0, 0, 0);

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

float4 DottedPS(VertexShaderOutput input) : COLOR
{
    float2 centerRelativeCoords = input.TexCoords - Center + TexelSize / 2;

    if (Mod(centerRelativeCoords.x, 10) >= 5 && Mod(centerRelativeCoords.y, 10) >= 5) return float4(0, 0, 0, 0);
    if (Mod(centerRelativeCoords.x + 10, 10) >= 5 && Mod(centerRelativeCoords.y + 10, 10) >= 5) return float4(0, 0, 0, 0);

    float trueAnomaly = Mod((atan2(centerRelativeCoords.x, centerRelativeCoords.y) - ArgumentOfPeriapsis), TAU);
    if (trueAnomaly < StartAnomaly || trueAnomaly > EndAnomaly) return float4(0, 0, 0, 0);

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
        PixelShader = compile PS_SHADERMODEL DefaultPS();
    }
};

technique DottedOrbitDrawing {
    pass P0 {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL DottedPS();
    }
}