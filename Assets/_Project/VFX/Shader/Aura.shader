Shader "Custom/SimpleOutline" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.03
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        // --- Outline Pass ---
        Pass {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            Cull Front                 // Render backfaces to create a silhouette
            ZWrite Off                 // Do not write to the depth buffer
            ZTest LEqual
            Offset -1, -1              // Push the outline behind the base geometry
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
            };

            float _OutlineWidth;
            fixed4 _OutlineColor;

            v2f vertOutline(appdata v) {
                v2f o;
                // Expand vertices along their normals.
                float3 norm = normalize(v.normal);
                float4 pos = v.vertex;
                pos.xyz += norm * _OutlineWidth;
                o.pos = UnityObjectToClipPos(pos);
                return o;
            }

            fixed4 fragOutline(v2f i) : SV_Target {
                return _OutlineColor;
            }
            ENDCG
        }

        // --- Base Pass ---
        Pass {
            Name "BASE"
            Tags { "LightMode" = "ForwardBase" }
            Cull Back
            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vertBase
            #pragma fragment fragBase
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;

            v2f vertBase(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 fragBase(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
