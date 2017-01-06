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

float4 PS(Pixel Input) : SV_Target
{
    float4 TexColor = Texture.Sample(Sampler, Input.TexCoord);

    float Random1 = cos(Input.LightViewPosition.x * Input.LightViewPosition.y * 123456789) * Factor + LightColor.r * (1 - Factor);
    float Random2 = cos(Input.LightViewPosition.x * Input.LightViewPosition.y * 12345678) * Factor + LightColor.g * (1 - Factor);
    float Random3 = cos(Input.LightViewPosition.x * Input.LightViewPosition.y * 1234567) * Factor + LightColor.b * (1 - Factor);
    float Random = float3(Random1, Random2, Random3);
    
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
    
    if (LightIntensity < 1.0f && (saturate(DepthCoords.x) == DepthCoords.x) && (saturate(DepthCoords.y) == DepthCoords.y))
    {
        // Sample the shadow map depth value from the depth texture using the sampler at the projected texture coordinate location.
        float depthValue = DepthMapTexture.Sample(Sampler, DepthCoords).r;

        // Calculate the depth of the light.
        float lightDepthValue = Input.LightViewPosition.z / Input.LightViewPosition.w - ShadowBias; //0.0001;

        if (lightDepthValue < depthValue)
        {
            // Calculate the amount of light on this pixel.
            LightIntensity = saturate(LightIntensity + 
                dot(Input.Normal, LightDirection) * 
                pow(cos(Input.LightViewPosition.x / Input.LightViewPosition.w * HalfPI) *
                cos(Input.LightViewPosition.y / Input.LightViewPosition.w * HalfPI), 2)
            );
        }
    }
    
    return float4(Input.FogIntensity * Random + (1 - Input.FogIntensity) * saturate(TexColor * LightColor * LightIntensity).xyz, 1);

}