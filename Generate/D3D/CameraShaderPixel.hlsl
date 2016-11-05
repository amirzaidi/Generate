cbuffer PixelLight
{
    float4 LightColor;
};

struct Pixel
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL;
    float4 LightViewPosition : TEXCOORD1;
    float3 LightPos : TEXCOORD2;
};

Texture2D Texture : register(t0);
Texture2D DepthMapTexture : register(t1);

SamplerState Sampler : register(s0);

static const float PI = 3.14159265f;

float4 PS(Pixel Input) : SV_Target
{
    float4 TexColor = Texture.Sample(Sampler, Input.TexCoord);
    
    float2 projectTexCoord = float2(
        Input.LightViewPosition.x / Input.LightViewPosition.w / 2.0f + 0.5f,
        -Input.LightViewPosition.y / Input.LightViewPosition.w / 2.0f + 0.5f
    );

    float LightIntensity = 0.05;

    if ((saturate(projectTexCoord.x) == projectTexCoord.x) && (saturate(projectTexCoord.y) == projectTexCoord.y))
    {
        // Sample the shadow map depth value from the depth texture using the sampler at the projected texture coordinate location.
        float depthValue = DepthMapTexture.Sample(Sampler, projectTexCoord).r;

        // Calculate the depth of the light.
        float lightDepthValue = Input.LightViewPosition.z / Input.LightViewPosition.w - 0.002;

        if (lightDepthValue < depthValue)
        {
            // Calculate the amount of light on this pixel.
            LightIntensity += saturate(dot(Input.Normal, float3(0, 1, 0)) * pow(
                cos(Input.LightViewPosition.x / Input.LightViewPosition.w * PI / 2.0f)
                * cos(Input.LightViewPosition.y / Input.LightViewPosition.w * PI / 2.0f)
            , 2))
            * 0.95;
        }
    }
    
    return float4(saturate(TexColor * saturate(LightColor * LightIntensity)).xyz, 1);
}