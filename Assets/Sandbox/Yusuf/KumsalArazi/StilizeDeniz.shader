// Stilize deniz: kıyıyı tanıyan dalgalar, derinliğe göre renk, kırılma ve kıyı köpüğü.
// Dalgalar _KiyiHarita'dan zemin yüksekliğini ve kıyıya uzaklığı okur:
// tepeler kıyıya paralel ilerler, sığda yükselip dikleşir, kırılır ve kumsala tırmanıp geri çekilir.
// Renk ve köpük için derinlik dokusu gerekir: kamerada (ya da URP asset'inde) Depth Texture açık olmalı.
Shader "IkiDunya/Sandbox/StilizeDeniz"
{
    Properties
    {
        [Header(Renk)]
        _SigRenk ("Sığ su", Color) = (0.098, 0.745, 0.769, 1)
        _OrtaRenk ("Orta su", Color) = (0.035, 0.671, 0.506, 1)
        _DerinRenk ("Derin su", Color) = (0.086, 0.498, 0.631, 1)
        _GokRenk ("Gökyüzü yansıması", Color) = (0.486, 0.808, 0.867, 1)
        _OrtaDerinlik ("Orta suya geçiş (m)", Float) = 1.2
        _DerinDerinlik ("Derin suya geçiş (m)", Float) = 6
        _SigSaydamlik ("Kıyıda saydamlık", Range(0, 1)) = 0.35
        _SaydamlikDerinligi ("Opaklaşma derinliği (m)", Float) = 2.5

        [Header(Kiyi haritasi)]
        [NoScaleOffset] _KiyiHarita ("Kıyı haritası (R yükseklik, G kıyıya uzaklık)", 2D) = "black" {}
        _KiyiST ("Dünya xz -> harita uv (ölçek xz, kayma xz)", Vector) = (0.0111, 0.0128, 0.5, 0.5)
        _YukseklikAraligi ("Yükseklik aralığı (min, max)", Vector) = (-4, 8, 0, 0)
        _MesafeAraligi ("Kıyı uzaklığı aralığı (min, max)", Vector) = (-40, 60, 0, 0)
        _AcikEgim ("Harita dışında taban eğimi", Float) = 0.05

        [Header(Dalga)]
        _DalgaYuksekligi ("Açıkta dalga yüksekliği (m)", Float) = 0.12
        _DalgaBoyu ("Dalga boyu (m)", Float) = 9
        _DalgaHizi ("Dalga hızı", Float) = 0.5
        _Diklik ("Açıkta diklik", Range(0, 1)) = 0.35
        _CirpintiOrani ("Açıkta çırpıntı oranı", Range(0, 1)) = 0.35
        _SiglasmaDerinligi ("Sığlaşmanın başladığı derinlik (m)", Float) = 3
        _SiglasmaKazanci ("Sığda yükselme", Float) = 1.2
        _KirilmaOrani ("Kırılma: dalga yüksekliği / derinlik sınırı", Float) = 0.4
        _TepeAydinligi ("Dalga tepesinde açık renk", Range(0, 1)) = 0.35

        [Header(Kumsala tirmanma)]
        _TirmanmaDerinligi ("Tırmanmanın başladığı derinlik (m)", Float) = 1
        _Tirmanma ("Tırmanma yüksekliği (m)", Float) = 0.15
        _TirmanmaMesafesi ("Tırmanmanın bittiği kıyı uzaklığı (m)", Float) = 8
        _TirmanmaHizi ("Tırmanma süresi (dalga döngüsünün oranı)", Range(0.05, 0.9)) = 0.2

        [Header(Kopuk)]
        _KopukRenk ("Köpük", Color) = (0.961, 0.953, 0.937, 1)
        _KopukGenisligi ("Kıyı köpüğü genişliği (m)", Float) = 0.45
        _KopukOlcegi ("Köpük deseni ölçeği", Float) = 0.6
        _KopukHizi ("Köpük hızı", Float) = 0.35
        _KirilmaKopugu ("Kırılan dalga köpüğü", Range(0, 1)) = 0.85
        _ParlamaGucu ("Güneş parlaması", Range(0, 2)) = 0.6
        _Parlaklik ("Parlaklık üssü", Float) = 120
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Deniz"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_KiyiHarita);
            SAMPLER(sampler_KiyiHarita);

            CBUFFER_START(UnityPerMaterial)
                half4 _SigRenk, _OrtaRenk, _DerinRenk, _GokRenk, _KopukRenk;
                float _OrtaDerinlik, _DerinDerinlik, _SigSaydamlik, _SaydamlikDerinligi;
                float4 _KiyiST, _YukseklikAraligi, _MesafeAraligi;
                float _AcikEgim;
                float _DalgaYuksekligi, _DalgaBoyu, _DalgaHizi, _Diklik, _CirpintiOrani;
                float _SiglasmaDerinligi, _SiglasmaKazanci, _KirilmaOrani, _TepeAydinligi;
                float _TirmanmaDerinligi, _Tirmanma, _TirmanmaMesafesi, _TirmanmaHizi;
                float _KopukGenisligi, _KopukOlcegi, _KopukHizi, _KirilmaKopugu;
                float _ParlamaGucu, _Parlaklik;
            CBUFFER_END

            struct Girdi
            {
                float4 positionOS : POSITION;
            };

            struct Cikti
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 ekranPos : TEXCOORD2;
                float fog : TEXCOORD3;
                float3 kopuk : TEXCOORD4; // x: kırılan dalga tepesi, y: kumsala tırmanan su, z: dalga tepesi
            };

            // Haritadan zemin yüksekliği ve kıyıya uzaklık (denizde negatif, karada pozitif).
            // Harita dışında deniz tarafına doğru uzaklık ve derinlik artmaya devam eder.
            void Kiyi(float2 xz, out float zemin, out float mesafe)
            {
                float2 uv = xz * _KiyiST.xy + _KiyiST.zw;
                float4 h = SAMPLE_TEXTURE2D_LOD(_KiyiHarita, sampler_KiyiHarita, saturate(uv), 0);
                float disari = (min(uv.y, 0) + max(uv.y - 1, 0)) / abs(_KiyiST.y);
                zemin = lerp(_YukseklikAraligi.x, _YukseklikAraligi.y, h.r) + min(disari, 0) * _AcikEgim;
                mesafe = lerp(_MesafeAraligi.x, _MesafeAraligi.y, h.g) + disari;
            }

            float Gurultu(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = frac(sin(dot(i, float2(127.1, 311.7))) * 43758.5453);
                float b = frac(sin(dot(i + float2(1, 0), float2(127.1, 311.7))) * 43758.5453);
                float c = frac(sin(dot(i + float2(0, 1), float2(127.1, 311.7))) * 43758.5453);
                float d = frac(sin(dot(i + float2(1, 1), float2(127.1, 311.7))) * 43758.5453);
                float2 u = f * f * (3 - 2 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            Cikti Vert(Girdi g)
            {
                Cikti o;
                float3 ws = TransformObjectToWorld(g.positionOS.xyz);
                float t = _Time.y;

                float zemin, mesafe;
                Kiyi(ws.xz, zemin, mesafe);
                float derinlik = max(0, ws.y - zemin);

                // Kıyıya dik yön: uzaklık alanının eğimi
                float z1, mx1, mx0, mz1, mz0;
                Kiyi(ws.xz + float2(1, 0), z1, mx1);
                Kiyi(ws.xz - float2(1, 0), z1, mx0);
                Kiyi(ws.xz + float2(0, 1), z1, mz1);
                Kiyi(ws.xz - float2(0, 1), z1, mz0);
                float2 yon = float2(mx1 - mx0, mz1 - mz0);
                yon = dot(yon, yon) > 1e-6 ? normalize(yon) : float2(0, 1);
                float yan = dot(ws.xz, float2(-yon.y, yon.x));

                // Ana dalga: tepeler kıyıya paralel, kıyıya doğru ilerler
                float k = TWO_PI / _DalgaBoyu;
                float w = sqrt(9.8 * k) * _DalgaHizi;
                float sapma = 0.6 * sin(yan * 0.08 + t * 0.1);
                float faz = k * mesafe - w * t + sapma;

                // Sığlaşma: dalga yükselir. Yüksekliği derinliğe sığmayınca kırılır: tepe öne eğilir, köpürür
                float sig = saturate(1 - derinlik / _SiglasmaDerinligi);
                float aHam = _DalgaYuksekligi * (1 + _SiglasmaKazanci * sig);
                float aSinir = _KirilmaOrani * derinlik;
                float a = min(aHam, aSinir);
                float kirilan = saturate((aHam - aSinir) / (0.3 * aHam + 1e-4));
                float q = lerp(_Diklik, 1, kirilan);
                float s = sin(faz);
                float c = cos(faz);
                float3 p = ws;
                p.xz += yon * (q * a * c);
                p.y += a * s;
                float2 egim = a * k * c * yon;

                // Açıkta küçük çapraz çırpıntı; sığda söner
                float ca = _DalgaYuksekligi * _CirpintiOrani * (1 - sig);
                float2 d1 = normalize(float2(0.7, 0.3));
                float2 d2 = normalize(float2(-0.5, 0.6));
                float k1 = k * 1.9;
                float k2 = k * 2.7;
                float f1 = dot(d1, ws.xz) * k1 - t * sqrt(9.8 * k1) * _DalgaHizi;
                float f2 = dot(d2, ws.xz) * k2 - t * sqrt(9.8 * k2) * _DalgaHizi;
                p.y += ca * (sin(f1) + 0.6 * sin(f2));
                egim += ca * (k1 * cos(f1) * d1 + 0.6 * k2 * cos(f2) * d2);

                // Kumsala tırmanma: dalga kıyıya varınca su hızla yükselir, sonra yavaşça çekilir.
                // Su yüzeyi kumun altından geçtiği için ıslak çizgi kumsalda ileri geri gider.
                float dongu = frac((w * t - sapma + HALF_PI) / TWO_PI);
                float tir = dongu < _TirmanmaHizi
                    ? smoothstep(0, _TirmanmaHizi, dongu)
                    : 1 - smoothstep(_TirmanmaHizi, 1, dongu);
                float alan = saturate(1 - derinlik / _TirmanmaDerinligi) * saturate((_TirmanmaMesafesi - mesafe) / 3);
                p.y += _Tirmanma * tir * alan;

                o.positionWS = p;
                o.normalWS = normalize(float3(-egim.x, 1, -egim.y));
                o.positionCS = TransformWorldToHClip(p);
                o.ekranPos = ComputeScreenPos(o.positionCS);
                o.fog = ComputeFogFactor(o.positionCS.z);
                o.kopuk = float3(kirilan * smoothstep(0.2, 1, s), tir * alan, saturate(s) * saturate(a / (_DalgaYuksekligi * 2)));
                return o;
            }

            half4 Frag(Cikti c) : SV_Target
            {
                float2 uv = c.ekranPos.xy / c.ekranPos.w;
                float zeminDerinligi = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
                float derinlik = max(0, zeminDerinligi - c.ekranPos.w);

                half3 renk = lerp(_SigRenk.rgb, _OrtaRenk.rgb, saturate(derinlik / _OrtaDerinlik));
                renk = lerp(renk, _DerinRenk.rgb, saturate((derinlik - _OrtaDerinlik) / max(0.01, _DerinDerinlik - _OrtaDerinlik)));

                // Dalga tepesi ince olduğu için ışık geçirir: sığ su rengine doğru açılır
                renk = lerp(renk, _SigRenk.rgb * 1.15, c.kopuk.z * _TepeAydinligi);

                float3 n = normalize(c.normalWS);
                float3 v = normalize(GetWorldSpaceViewDir(c.positionWS));
                Light gunes = GetMainLight();
                half yumusak = saturate(dot(n, gunes.direction)) * 0.5 + 0.5;
                renk *= lerp(1, gunes.color, 0.5) * yumusak;
                half fresnel = pow(1 - saturate(dot(n, v)), 4);
                renk = lerp(renk, _GokRenk.rgb, fresnel * 0.6);
                float3 h = normalize(gunes.direction + v);
                renk += gunes.color * pow(saturate(dot(n, h)), _Parlaklik) * _ParlamaGucu;

                float t = _Time.y * _KopukHizi;
                float2 p = c.positionWS.xz * _KopukOlcegi;
                float desen = Gurultu(p + float2(0, t)) * 0.6 + Gurultu(p * 2.3 - float2(t, t * 0.5)) * 0.4;

                // Suyun kuma değdiği yer: ince, kırık çizgi (tırmanan suyla birlikte hareket eder)
                float bant = 1 - saturate(derinlik / _KopukGenisligi);
                float kiyiKopugu = smoothstep(0.62, 0.8, bant + (desen - 0.5) * 0.7);
                // Kırılan dalganın tepesi ve kumsala yayılan köpük örtüsü
                float tepe = smoothstep(0.62, 0.82, c.kopuk.x + (desen - 0.5) * 1.1) * _KirilmaKopugu;
                float yayilma = smoothstep(0.5, 0.75, c.kopuk.y * bant + desen * 0.4) * 0.8;

                float kopuk = max(kiyiKopugu, max(tepe, yayilma));
                renk = lerp(renk, _KopukRenk.rgb, kopuk);

                half alfa = lerp(_SigSaydamlik, 1, saturate(derinlik / _SaydamlikDerinligi));
                alfa = max(alfa, kopuk);
                renk = MixFog(renk, c.fog);
                return half4(renk, alfa);
            }
            ENDHLSL
        }
    }
}
