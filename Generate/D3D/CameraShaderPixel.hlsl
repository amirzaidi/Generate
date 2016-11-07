cbuffer PixelLight
{
    float4 LightColor;
    float3 LightDirection;
    float StartLight;
};

struct Pixel
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL;
    float4 LightViewPosition : TEXCOORD1;
};

Texture2D Texture : register(t0);
Texture2D DepthMapTexture : register(t1);

SamplerState Sampler : register(s0);

static const float PI = 3.14159265f;

float4 PS(Pixel Input) : SV_Target
{
    float4 TexColor = Texture.Sample(Sampler, Input.TexCoord);

    if (StartLight == 1)
    {
        return float4(TexColor.xyz, 1);
    }
    
    float2 LightProjectedCoords = float2(
        Input.LightViewPosition.x / Input.LightViewPosition.w / 2.0f + 0.5f,
        -Input.LightViewPosition.y / Input.LightViewPosition.w / 2.0f + 0.5f
    );

    float LightIntensity = StartLight;
    float4 LightC = LightColor;
    
    if ((saturate(LightProjectedCoords.x) == LightProjectedCoords.x) && (saturate(LightProjectedCoords.y) == LightProjectedCoords.y))
    {
        // Sample the shadow map depth value from the depth texture using the sampler at the projected texture coordinate location.
        float depthValue = DepthMapTexture.Sample(Sampler, LightProjectedCoords).r;

        // Calculate the depth of the light.
        float lightDepthValue = Input.LightViewPosition.z / Input.LightViewPosition.w - 0.0005;

        if (lightDepthValue < depthValue)
        {
            // Calculate the amount of light on this pixel.
            LightIntensity += saturate(dot(Input.Normal, LightDirection) * pow(
                cos(Input.LightViewPosition.x / Input.LightViewPosition.w * PI / 2.0f)
                * cos(Input.LightViewPosition.y / Input.LightViewPosition.w * PI / 2.0f)
            , 2)) * (1 - StartLight);
        }
    }
    
    return float4(saturate(TexColor * LightC * LightIntensity).xyz, 1);
}