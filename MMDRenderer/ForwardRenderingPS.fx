struct PixelInputType
{
	float4 position : SV_POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
};

struct PixelOutputType
{
	float4 color : SV_TARGET0;
};

Texture2D shaderTexture;
SamplerState SampleType;

PixelOutputType PS(PixelInputType input)
{
	PixelOutputType output;

	output.color = shaderTexture.Sample(SampleType, input.tex);

	return output;
}
