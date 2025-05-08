Shader "Custom/HologramWireframeMergedExplode_NoPixel_Disappear_Optimized" {
    Properties {
        // Shared texture
        _MainTex ("Main Texture", 2D) = "white" {}

        // Hologram effect properties
        _TintColor ("Tint Color", Color) = (0,0.5,1,1)
        _RimColor ("Rim Color", Color) = (0,1,1,1)
        _GlitchTime ("Glitches Over Time", Range(0.01,3.0)) = 1.0
        _HoloOpacity ("Hologram Opacity", Range(0,1)) = 0.8

        // Wireframe effect properties
        _MovingSlider ("Moving Slider", Range(-1, 4)) = 5
    }

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        Cull Off
        AlphaToMask On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            Name "MergedPass"
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma target 4.0

            #include "UnityCG.cginc"

            // --- Uniforms ---
            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Hologram uniforms
            float4 _TintColor;
            float4 _RimColor;
            float _GlitchTime;
            float _WorldScale;  // Note: _WorldScale is still expected to be set externally.
            float _HoloOpacity;

            // Wireframe uniforms
            float _WireThickness;  // still expected to be set if using wireframe effect
            float4 _WireColor;
            float _MovingSlider;
            float _Extrude;
            float _WireFrameStay;

            // --- Structures ---
            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2g {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float movingPos : TEXCOORD1;
                float3 wpos : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
            };

            struct g2f {
                float4 positionCS : SV_POSITION;
                float4 dist : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float movingPos : TEXCOORD2;
                float3 wpos : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
            };

            // --- Vertex Shader ---
            v2g vert(appdata v) {
                v2g o;

                // Glitch Effect: Trigger glitch when sin(_Time.w * _GlitchTime) > 0
                float optTime = sin(_Time.w * _GlitchTime);
                float glitchTime = step(0.0, optTime);
                float glitchVal = sin(_Time.y);
                float glitchPos = v.vertex.y + glitchVal;
                // Only apply small offset if vertex.y is within a small range
                float glitchClamp = step(0.0, glitchPos) * step(glitchPos, 0.2);
                v.vertex.xz += glitchClamp * 0.1 * glitchTime * glitchVal;

                // Compute movingPos (branch is resolved at compile time if INVERT is defined)
    #if INVERT
                float movingPos = v.vertex.y + _MovingSlider;
    #else
                float movingPos = 1.0 - v.vertex.y + _MovingSlider;
    #endif

                // Extrude the vertex along its normal based on movingPos.
                v.vertex.xyz -= saturate(1.0 - movingPos) * v.normal * _Extrude;

                o.positionCS = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.movingPos = movingPos;
                o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normalDir = normalize(mul((float3x3)unity_WorldToObject, v.normal));
                return o;
            }

            // --- Geometry Shader ---
            [maxvertexcount(3)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream) {
                // Compute triangle centroid in world space.
                float3 centroidWS = (input[0].wpos + input[1].wpos + input[2].wpos) / 3.0;
                // Compute explosion factor based on _MovingSlider.
                float explosionFactor = max(0.0, (5.0 - _MovingSlider)) * 0.5;

                float4 newClip[3];
                float3 newWPos[3];

                // Calculate new positions by exploding vertices outward from the centroid.
                for (int j = 0; j < 3; j++) {
                    float3 dirWS = normalize(input[j].wpos - centroidWS);
                    newWPos[j] = input[j].wpos + dirWS * explosionFactor;
                    newClip[j] = mul(UNITY_MATRIX_VP, float4(newWPos[j], 1.0));
                }

                // Pre-calculate clip-space positions for edge computations.
                float2 p[3];
                [unroll]
                for (int j = 0; j < 3; j++) {
                    p[j] = newClip[j].xy / newClip[j].w;
                }
                float2 edge0 = p[2] - p[1];
                float2 edge1 = p[2] - p[0];
                float2 edge2 = p[1] - p[0];
                float area = abs(edge1.x * edge2.y - edge1.y * edge2.x);
                float wireThick = 800.0 - _WireThickness;

                // For each vertex, compute the distance factor for wireframe overlay.
                for (int j = 0; j < 3; j++) {
                    g2f o;
                    o.positionCS = newClip[j];
                    float invW = 1.0 / newClip[j].w;
                    if (j == 0) {
                        o.dist = float4((area / length(edge0)), 0.0, 0.0, invW) * newClip[j].w * wireThick;
                    } else if (j == 1) {
                        o.dist = float4(0.0, (area / length(edge1)), 0.0, invW) * newClip[j].w * wireThick;
                    } else {
                        o.dist = float4(0.0, 0.0, (area / length(edge2)), invW) * newClip[j].w * wireThick;
                    }
                    o.uv = input[j].uv;
                    o.movingPos = input[j].movingPos;
                    o.wpos = newWPos[j];
                    o.normalDir = input[j].normalDir;
                    triStream.Append(o);
                }
            }

            // --- Fragment Shader ---
            fixed4 frag(g2f i) : SV_Target {
                // Hologram Base Color
                fixed4 texColor = tex2D(_MainTex, i.uv);
                float4 baseColor = texColor * _TintColor;
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.wpos);
                float rim = 1.0 - saturate(dot(viewDir, i.normalDir));

                // Adjust scanlines frequency by scaling _WorldScale.
                float fracLines = frac((i.wpos.y * _WorldScale * 0.5) + _Time.y);
                // Since fracLines is always less than 1, this always returns 1.
                float scanlines = 1.0;
                float bigFracLine = frac(i.wpos.y - _Time.x * 4.0);

                float4 holoColor = baseColor + (bigFracLine * 0.4 * _TintColor) + (rim * _RimColor);
                // Use the new opacity property (scanlines is now a constant 1.0).
                holoColor.a = _HoloOpacity * (1.0 + rim + bigFracLine);

                // Wireframe overlay computation
                float minDist = min(i.dist.x, min(i.dist.y, i.dist.z)) * i.dist.w;
                float fadeThreshold = 0.9 * (1.0 - saturate(i.movingPos - _WireFrameStay));
                float wireFactor = (_MovingSlider < 5.0) ? saturate(1.0 - (minDist / fadeThreshold)) : 0.0;
                float4 wireColorMod = _WireColor;
                fixed4 finalColor = lerp(holoColor, wireColorMod, wireFactor);

                // Compute fade factor (caching saturate result)
                float fadeFactor = saturate(_MovingSlider / 4.0) * 0.9;
                finalColor *= fadeFactor;

                return finalColor;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
