// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Simple shader mapping a YUV video feed without any lighting model.
Shader "MobileRemoting/YUVFeedShader (unlit)"
{
    Properties
    {
        [HideInEditor][NoScaleOffset] _YPlane("Y plane", 2D) = "black" {}
        [HideInEditor][NoScaleOffset] _UPlane("U plane", 2D) = "gray" {}
        [HideInEditor][NoScaleOffset] _VPlane("V plane", 2D) = "gray" {}
        _BackgroundOpacity("Background Opacity", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
        Pass
        {
            // Our background (non-holographic areas) is black, i.e 0
            // Using OneMinusSrcAlpha means that we can use our alpha channel in the output of our
            // fragment function to control the appearance of this black background
            Blend One OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            sampler2D _YPlane;
            sampler2D _UPlane;
            sampler2D _VPlane;
            half _BackgroundOpacity;

            half3 yuv2rgb(half3 yuv)
            {
                // The YUV to RBA conversion, please refer to: http://en.wikipedia.org/wiki/YUV
                // Y'UV420p (I420) to RGB888 conversion section.
                half y_value = yuv[0];
                half u_value = yuv[1];
                half v_value = yuv[2];
                half r = y_value + 1.370705 * (v_value - 0.5);
                half g = y_value - 0.698001 * (v_value - 0.5) - (0.337633 * (u_value - 0.5));
                half b = y_value + 1.732446 * (u_value - 0.5);
                return half3(r, g, b);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half3 yuv;
                yuv.x = tex2D(_YPlane, i.uv).r;
                yuv.y = tex2D(_UPlane, i.uv).r;
                yuv.z = tex2D(_VPlane, i.uv).r;
                half3 rgb = yuv2rgb(yuv);
                return fixed4(rgb, _BackgroundOpacity);
                // return rgb;
            }
            ENDCG
        }
    }
}
