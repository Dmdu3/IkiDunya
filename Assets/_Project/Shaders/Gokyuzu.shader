// Gece gökyüzü (yıldızlar, ay): ışıksız, sissiz, tepe noktası rengiyle. Sis gökyüzünü örtmesin diye ayrı shader.
Shader "IkiDunya/Gokyuzu"
{
    Properties
    {
        _BaseColor ("Renk", Color) = (1, 1, 1, 1)
        [Toggle] _Yuvarlak ("Yuvarlak (yıldız: ortası parlak, kenarı erir)", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-100" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Yuvarlak;
            CBUFFER_END

            struct Girdi
            {
                float4 konum : POSITION;
                half4 renk : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Cikti
            {
                float4 konum : SV_POSITION;
                half4 renk : COLOR;
                float2 uv : TEXCOORD0;
            };

            Cikti vert(Girdi g)
            {
                Cikti c;
                c.konum = TransformObjectToHClip(g.konum.xyz);
                c.renk = g.renk * _BaseColor;
                c.uv = g.uv;
                return c;
            }

            half4 frag(Cikti c) : SV_Target
            {
                half4 renk = c.renk;
                // Yıldız dörtgeni kare görünmesin: ortadan kenara erir.
                half uzaklik = length(c.uv * 2 - 1);
                renk.a *= lerp(1, saturate(1 - uzaklik) * saturate(1 - uzaklik), _Yuvarlak);
                return renk;
            }
            ENDHLSL
        }
    }
}
