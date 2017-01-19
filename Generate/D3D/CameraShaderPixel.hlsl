cbuffer PixelLight
{
    float4 LightColor;
    float3 LightDirection;
    float StartLight;
};

cbuffer PixelFog
{
    float Factor;
    float BackgroundFactor;
    float ShadowBias;
};

struct Pixel
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL;
    float4 LightViewPosition : TEXCOORD1;
    float FogIntensity : FOG;
};

Texture2D Texture : register(t0);
Texture2D DepthMapTexture : register(t1);

SamplerState Sampler : register(s0);

static const float HalfPI = 1.57079632679f;

float Rand(float3 Seed)
{
    return frac(sin(dot(Seed.xyz, float3(12.9898, 28.233, 45.5432))) * 42558.5453) * 2 - 1;
}

float4 PS(Pixel Input) : SV_Target
{
    float4 TexColor = Texture.Sample(Sampler, Input.TexCoord);

    float Z = abs(LightDirection.x * sqrt(2) + LightDirection.y * sqrt(3) + LightDirection.z * sqrt(5)) + 1;
    
    float R1 = Rand(float3(Input.Position.xy + float2(630, 625), Z));
    float R2 = Rand(float3(Input.Position.xy + float2(655, 650), Z));
    float R3 = Rand(float3(Input.Position.xy + float2(680, 675), Z));

    float3 Random = float3(R1, R2, R3) * Factor + LightColor.rgb * (1 - Factor);
    
    if (StartLight == 1)
    {
        Input.FogIntensity *= BackgroundFactor;
        return float4(Input.FogIntensity * Random + (1 - Input.FogIntensity) * TexColor.xyz, 1);
    }
    
    float2 DepthCoords = float2(
        Input.LightViewPosition.x / Input.LightViewPosition.w / 2.0f + 0.5f,
        -Input.LightViewPosition.y / Input.LightViewPosition.w / 2.0f + 0.5f
    );

    float LightIntensity = StartLight;
    if (saturate(DepthCoords.x) == DepthCoords.x && saturate(DepthCoords.y) == DepthCoords.y)
    {
        // Sample the shadow map depth value from the depth texture using the sampler at the projected texture coordinate location.
        float depthValue = DepthMapTexture.Sample(Sampler, DepthCoords).r;

        // Calculate the depth of the light.
        float lightDepthValue = Input.LightViewPosition.z / Input.LightViewPosition.w - ShadowBias;

        if (lightDepthValue < depthValue)
        {
            // Calculate the amount of light on this pixel.
            LightIntensity = saturate(LightIntensity +
                dot(Input.Normal, LightDirection) *
                pow(
                    cos(Input.LightViewPosition.x / Input.LightViewPosition.w * HalfPI) *
                    cos(Input.LightViewPosition.y / Input.LightViewPosition.w * HalfPI), 
                2)
            );
        }
    }
    
    return float4(Input.FogIntensity * Random + (1 - Input.FogIntensity) * (TexColor * LightColor * LightIntensity).xyz, 1);

}