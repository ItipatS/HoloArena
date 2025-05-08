Shader "Custom/CloudShadowOnTerrain"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _CloudTex ("Cloud Shadow", 2D) = "white" {}
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.5
        _ScrollSpeed ("Scroll Speed", Vector) = (0.01, 0, 0, 0)
        _Tiling ("Tiling", Float) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        sampler2D _CloudTex;
        float _ShadowStrength;
        float4 _ScrollSpeed;
        float _Tiling;

        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            float2 cloudUV = IN.worldPos.xz * _Tiling + _Time.y * _ScrollSpeed.xy;
            float shadowMask = tex2D(_CloudTex, cloudUV).r;
            float shadowFactor = 1 - (shadowMask * _ShadowStrength);

            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb * shadowFactor;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
