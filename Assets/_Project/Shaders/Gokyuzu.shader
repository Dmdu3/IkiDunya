// Gece gökyüzü (yıldızlar, ay): ışıksız, sissiz, tepe noktası rengiyle. Sis gökyüzünü örtmesin diye ayrı shader.
Shader "IkiDunya/Gokyuzu"
{
    Properties
    {
        _BaseColor ("Renk", Color) = (1, 1, 1, 1)
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
            CBUFFER_END

            struct Girdi
            {
                float4 konum : POSITION;
                half4 renk : COLOR;
            };

            struct Cikti
            {
                float4 konum : SV_POSITION;
                half4 renk : COLOR;
            };

            Cikti vert(Girdi g)
            {
                Cikti c;
                c.konum = TransformObjectToHClip(g.konum.xyz);
                c.renk = g.renk * _BaseColor;
                return c;
            }

            half4 frag(Cikti c) : SV_Target
            {
                return c.renk;
            }
            ENDHLSL
        }
    }
}
