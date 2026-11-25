Shader "UI/MirrorShine"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [Header(Mirror Shine Specular)]
        [HDR] _ShineColor ("Shine Color", Color) = (1, 1, 1, 1)
        _ShineBrightness ("Shine Brightness", Range(0.0, 5.0)) = 1.8
        _ShineWidth ("Main Beam Width", Range(1.0, 300.0)) = 35.0
        _ShineSoftness ("Edge Softness", Range(0.01, 1.0)) = 0.45
        _ShineAngle ("Light Angle (Degrees)", Range(-180.0, 180.0)) = 35.0
        _GlossBoost ("Mirror Gloss Core Boost", Range(0.0, 5.0)) = 1.2

        [Header(Secondary Accent Beam)]
        [Toggle] _EnableSecondaryBeam ("Enable Secondary Beam", Float) = 1
        _SecondaryOffset ("Secondary Offset (Pixels)", Range(-200.0, 200.0)) = -25.0
        _SecondaryWidth ("Secondary Width (Pixels)", Range(1.0, 150.0)) = 12.0
        _SecondaryIntensity ("Secondary Intensity", Range(0.0, 2.0)) = 0.5

        [Header(Animation Controls)]
        [Toggle] _AutoPlay ("Auto Play Sweep", Float) = 1
        _SweepSpeed ("Sweep Speed (Cycles / Sec)", Range(0.1, 5.0)) = 1.0
        _SweepInterval ("Delay Between Sweeps (Seconds)", Range(0.0, 10.0)) = 2.0
        _ManualProgress ("Manual Progress (0 to 1)", Range(0.0, 1.0)) = 0.0

        [Header(Coordinate Mode)]
        [Toggle] _UseUVMode ("Use UV Mode (0 = Local Pixels, 1 = UV)", Float) = 0
        _SweepRange ("Local Pixel Sweep Range", Range(50.0, 2000.0)) = 350.0

        [Header(UI Stencil and Masking)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                half2  texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float2 localPos      : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            fixed4 _ShineColor;
            half _ShineBrightness;
            half _ShineWidth;
            half _ShineSoftness;
            half _ShineAngle;
            half _GlossBoost;

            half _EnableSecondaryBeam;
            half _SecondaryOffset;
            half _SecondaryWidth;
            half _SecondaryIntensity;

            half _AutoPlay;
            half _SweepSpeed;
            half _SweepInterval;
            half _ManualProgress;

            half _UseUVMode;
            half _SweepRange;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = IN.vertex;
                OUT.localPos = IN.vertex.xy;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 baseColor = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                baseColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (baseColor.a - 0.001);
                #endif

                if (baseColor.a <= 0.001)
                {
                    return baseColor;
                }

                // Compute sweep progress [0..1]
                half progress = 0.0;
                if (_AutoPlay > 0.5)
                {
                    half speed = max(_SweepSpeed, 0.01);
                    half sweepDuration = 1.0 / speed;
                    half totalPeriod = sweepDuration + max(_SweepInterval, 0.0);
                    half timeInPeriod = fmod(_Time.y, totalPeriod);

                    if (timeInPeriod < sweepDuration)
                    {
                        progress = timeInPeriod / sweepDuration;
                    }
                    else
                    {
                        progress = -999.0; // Idle pause between sweeps
                    }
                }
                else
                {
                    progress = _ManualProgress;
                }

                if (progress < -100.0)
                {
                    return baseColor;
                }

                half rad = radians(_ShineAngle);
                half2 dir = half2(cos(rad), sin(rad));

                half proj = 0.0;
                half center = 0.0;
                half w1 = _ShineWidth;
                half w2 = _SecondaryWidth;
                half offset2 = _SecondaryOffset;

                if (_UseUVMode > 0.5)
                {
                    half2 centeredUV = IN.texcoord - half2(0.5, 0.5);
                    proj = dot(centeredUV, dir);
                    center = lerp(-0.8, 0.8, progress);
                    w1 = max(_ShineWidth * 0.01, 0.001);
                    w2 = max(_SecondaryWidth * 0.01, 0.001);
                    offset2 = _SecondaryOffset * 0.01;
                }
                else
                {
                    proj = dot(IN.localPos, dir);
                    half range = max(_SweepRange, 10.0);
                    center = lerp(-range, range, progress);
                    w1 = max(_ShineWidth, 0.1);
                    w2 = max(_SecondaryWidth, 0.1);
                }

                // Main light streak
                half d1 = abs(proj - center);
                half factor1 = saturate(1.0 - d1 / w1);
                half smooth1 = smoothstep(0.0, 1.0, factor1);
                half beam1 = pow(smooth1, max(_ShineSoftness * 2.0, 0.05));
                half core1 = pow(smooth1, 6.0) * _GlossBoost;
                half totalLight = beam1 + core1;

                // Secondary parallel accent streak
                if (_EnableSecondaryBeam > 0.5)
                {
                    half d2 = abs(proj - (center + offset2));
                    half factor2 = saturate(1.0 - d2 / w2);
                    half smooth2 = smoothstep(0.0, 1.0, factor2);
                    half beam2 = pow(smooth2, max(_ShineSoftness * 2.0, 0.05)) * _SecondaryIntensity;
                    totalLight += beam2;
                }

                // Apply specular shine light, masked to sprite silhouette
                half3 shineRGB = _ShineColor.rgb * (_ShineBrightness * totalLight * baseColor.a);
                half4 result;
                result.rgb = baseColor.rgb + shineRGB;
                result.a = baseColor.a;

                return result;
            }
        ENDCG
        }
    }
}
