#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define PI 3.14159265359
#define TAU 6.28318530718

matrix Projection;
matrix World;

float2 LightCenter;
float LightRadius;

Texture2D OccluderTextureBuffer;
SamplerState OccluderTextureSampler 
{
    Texture = <OccluderTextureBuffer>;
};
int OccluderCount;

float2 ScreenSize;

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

float Mod(float value, float mod) 
{
    return value - mod * floor(value / mod);
}

float Wrap(float value)
{
    if (value >= 0 && value < TAU) return value;
    return Mod(value, TAU);
}

bool IntervalIsSubset(float s1, float e1, float s2, float e2)
{
    float a = Wrap(s1 - s2);
    float b = Wrap(e2 - s2);
    float w = Wrap(e1 - s1);
    return a <= b && a + w <= b;
}

bool IntervalIsDisjoint(float s1, float e1, float s2, float e2)
{
    float a = Wrap(s1 - s2);
    float b = Wrap(e2 - s2);
    float w = Wrap(e1 - s1);
    return a >= b && a + w <= TAU;
}

float CalculateOcclusion(float2 tex)
{
    float2 pixel = float2(tex.x * ScreenSize.x, tex.y * ScreenSize.y);
    
    float2 lightRelPos = LightCenter - pixel;
    float lightDistance = length(lightRelPos);
    float lightHalfAngularRadius = asin(LightRadius / lightDistance);
    
    float lightCenterAngle = Wrap(atan2(lightRelPos.y, lightRelPos.x));
    float tangentAngle1 = Wrap(lightCenterAngle + lightHalfAngularRadius);
    float tangentAngle2 = Wrap(lightCenterAngle - lightHalfAngularRadius);
    
    bool occluded = false;
    
    float intersectionAngle1 = 0;
    float intersectionAngle2 = lightHalfAngularRadius * 2;
    
    for (int i = 0; i < OccluderCount; i++)
    {
        if (!occluded)
        {
            float4 v = tex2D(OccluderTextureSampler, float2(1.0 / OccluderCount * (i + 0.5), 0.5));
        
            float2 lineStart = float2(v.x, v.y) - pixel;
            float2 lineEnd = float2(v.z, v.w) - pixel;
        
            float pointAngle1 = Wrap(atan2(lineStart.y, lineStart.x));
            float pointAngle2 = Wrap(atan2(lineEnd.y, lineEnd.x));
        
            if (Wrap(pointAngle1 - pointAngle2) > PI) 
            {
                float temp = pointAngle1;
                pointAngle1 = pointAngle2;
                pointAngle2 = temp;
            }
        
            if ((
                Wrap(pointAngle1 - tangentAngle2) > Wrap(tangentAngle1 - tangentAngle2) && 
                Wrap(pointAngle2 - tangentAngle2) > Wrap(tangentAngle1 - tangentAngle2)
                ) || (
                    pow(lineStart.x, 2) + pow(lineStart.y, 2) <= lightDistance * lightDistance - LightRadius * LightRadius &&
                    pow(lineEnd.x, 2) + pow(lineEnd.y, 2) <= lightDistance * lightDistance - LightRadius * LightRadius && 
                    pow(lineStart.x - LightCenter.x + pixel.x, 2) + pow(lineStart.y - LightCenter.y + pixel.y, 2) >= LightRadius * LightRadius &&
                    pow(lineEnd.x - LightCenter.x + pixel.x, 2) + pow(lineEnd.y - LightCenter.y + pixel.y, 2) >= LightRadius * LightRadius))
            {
                if (IntervalIsSubset(tangentAngle2, tangentAngle1, pointAngle2, pointAngle1)) occluded = true;
                else if (IntervalIsSubset(pointAngle2, pointAngle1, tangentAngle2, tangentAngle1))
                {
                    float m1 = Wrap(pointAngle1 - tangentAngle2);
                    float m2 = Wrap(pointAngle2 - tangentAngle2);
                
                    if (m1 > intersectionAngle1) intersectionAngle1 = m1;
                    if (m2 < intersectionAngle2) intersectionAngle2 = m2;
                }
                else if (!IntervalIsDisjoint(tangentAngle2, tangentAngle1, pointAngle2, pointAngle1))
                {
                    if (Wrap(tangentAngle2 - pointAngle2) <= PI)
                    {
                        float m1 = Wrap(pointAngle1 - tangentAngle2);
                        if (m1 > intersectionAngle1) intersectionAngle1 = m1;
                        intersectionAngle2 = 0;
                    }
                    else
                    {
                        float m1 = Wrap(pointAngle2 - tangentAngle2);
                        intersectionAngle1 = lightHalfAngularRadius * 2;
                        if (m1 < intersectionAngle2) intersectionAngle2 = m1;
                    }
                }
            }
        
            /*if (IntervalIsSubset(pointAngle2, pointAngle1, tangentAngle2, tangentAngle1))
            {
                float m1 = Wrap(pointAngle1 - tangentAngle2);
                float m2 = Wrap(pointAngle2 - tangentAngle2);
                
                if (m1 > intersectionAngle1) intersectionAngle1 = m1;
                if (m2 < intersectionAngle2) intersectionAngle2 = m2;
                continue;
            }*/
        
            /*if (Wrap(tangentAngle2 - pointAngle2) <= PI)
            {
                float m1 = Wrap(pointAngle1 - tangentAngle2);
                if (m1 > intersectionAngle1) intersectionAngle1 = m1;
                intersectionAngle2 = 0;
            }
            else
            {
                float m1 = Wrap(pointAngle2 - tangentAngle2);
                intersectionAngle1 = lightHalfAngularRadius * 2;
                if (m1 < intersectionAngle2) intersectionAngle2 = m1;
            }*/
        }
    }
    
    //if (occlusion != 1) return max(intersectionAngle1 - intersectionAngle2, 0) / (lightHalfAngularRadius * 2);
    float occlusion;
    if (occluded) occlusion = 1;
    else occlusion = max(intersectionAngle1 - intersectionAngle2, 0) / (lightHalfAngularRadius * 2);
    return occlusion;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float occlusion = CalculateOcclusion(input.TexCoords);
    return float4(0, 0, 0, saturate(occlusion));
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};