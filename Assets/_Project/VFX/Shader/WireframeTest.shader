Shader "Custom/WireframeTransparentFade"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" { }
        _WireThickness ("Wire Thickness", Range(0, 800)) = 100
        [HDR] _WireColor ("Wire Color", Color) = (0,1,1,1)
        [Toggle(INVERT)] _INVERT("Invert", Float) = 1
        _MovingSlider ("Moving Slider", Range(-1, 5)) = 5
        _Extrude("Extrude Amount", Range(-10, 10)) = 10
        _WireFrameStay ("Wire Stay", Range(-1, 1)) = 0
    }

    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        LOD 200
        Cull Off
        AlphaToMask On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _WireThickness;
            float4 _WireColor;
            sampler2D _MainTex;
            float _MovingSlider;
            float _Extrude;
            float _WireFrameStay;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2g
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float movingPos : TEXCOORD1;
            };

            struct g2f
            {
                float4 positionCS : SV_POSITION;
                float4 dist : TEXCOORD1;
                float2 uv : TEXCOORD0;
                float movingPos : TEXCOORD2;
            };

            v2g vert(appdata v)
            {
                v2g o;
    #if INVERT
                float movingPos = v.vertex.y + _MovingSlider;
    #else
                float movingPos = 1 - v.vertex.y + _MovingSlider;
    #endif
                // Adjust the vertex position using the normal and extrude amount
                v.vertex.xyz -= saturate(1 - movingPos) * v.normal * _Extrude;
                o.positionCS = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.movingPos = movingPos;
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g i[3], inout TriangleStream<g2f> triStream)
            {
                // Convert clip space positions to normalized device coordinates
                float2 p0 = i[0].positionCS.xy / i[0].positionCS.w;
                float2 p1 = i[1].positionCS.xy / i[1].positionCS.w;
                float2 p2 = i[2].positionCS.xy / i[2].positionCS.w;

                float2 edge0 = p2 - p1;
                float2 edge1 = p2 - p0;
                float2 edge2 = p1 - p0;

                float area = abs(edge1.x * edge2.y - edge1.y * edge2.x);
                float wireThickness = 800 - _WireThickness;

                g2f o;
                o.positionCS = i[0].positionCS;
                o.dist.xyz = float3((area / length(edge0)), 0.0, 0.0) * o.positionCS.w * wireThickness;
                o.dist.w = 1.0 / o.positionCS.w;
                o.uv = i[0].uv;
                o.movingPos = i[0].movingPos;
                triStream.Append(o);

                o.positionCS = i[1].positionCS;
                o.dist.xyz = float3(0.0, (area / length(edge1)), 0.0) * o.positionCS.w * wireThickness;
                o.dist.w = 1.0 / o.positionCS.w;
                o.uv = i[1].uv;
                o.movingPos = i[1].movingPos;
                triStream.Append(o);

                o.positionCS = i[2].positionCS;
                o.dist.xyz = float3(0.0, 0.0, (area / length(edge2))) * o.positionCS.w * wireThickness;
                o.dist.w = 1.0 / o.positionCS.w;
                o.uv = i[2].uv;
                o.movingPos = i[2].movingPos;
                triStream.Append(o);
            }

            fixed4 frag(g2f i) : SV_Target
            {
                fixed4 mainTex = tex2D(_MainTex, i.uv);
                // If _MovingSlider is at or above 5, skip wireframe rendering entirely.
                if (_MovingSlider >= 5.0)
                {
                    return mainTex;
                }

                float minDistanceToEdge = min(i.dist.x, min(i.dist.y, i.dist.z)) * i.dist.w;
                // Compute fade threshold based on moving position and _WireFrameStay.
                float fadeThreshold = 0.9 * (1 - saturate(i.movingPos - _WireFrameStay));

                if (minDistanceToEdge > fadeThreshold)
                {
                    // Discard fragments close enough to the base so that only wireframe appears.
                    clip(i.movingPos - 0.2);
                    return mainTex;
                }
                // Otherwise, output the wire color.
                clip(i.movingPos);
                return _WireColor;
            }
            ENDCG
        }
    }
}
