cbuffer Light
{
    float4 Color;
    float3 Direction;
    float Padding;
};

struct Pixel
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL;
};

Texture2D Texture;
SamplerState Sampler;

float4 PS(Pixel Input) : SV_Target
{
	// Sample the pixel color from the texture using the sampler at this texture coordinate location.
    float Light = saturate(dot(Input.Normal, Direction));
    float4 TexColor = Texture.Sample(Sampler, Input.TexCoord);

    return saturate(saturate(Color * Light) * TexColor);
}