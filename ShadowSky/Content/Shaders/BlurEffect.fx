sampler2D TextureSampler : register(s0);
float2 texelSize;
float2 direction;

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 sum = float4(0,0,0,0);

    sum += tex2D(TextureSampler, texCoord - 4 * texelSize * direction) * 0.05;
    sum += tex2D(TextureSampler, texCoord - 2 * texelSize * direction) * 0.1;
    sum += tex2D(TextureSampler, texCoord)                         * 0.6;
    sum += tex2D(TextureSampler, texCoord + 2 * texelSize * direction) * 0.15;
    sum += tex2D(TextureSampler, texCoord + 4 * texelSize * direction) * 0.1;

    return sum;
}

technique Blur
{
    pass Pass1
    {
        PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
    }
}
