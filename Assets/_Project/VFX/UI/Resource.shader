Shader "Unlit/OrbUI"
{
    Properties
    {
        [Header(Shape)]  
        [PerRendererData]_MainTex("Shape Mask", 2D) = "white" {}
        _Alphacutoff("Alpha Cutoff", Range(0,1)) = 0.15
        _AlphaSmooth("Alpha Smoothness", Range(0,1)) = 0
        _DarkEdge("Dark Edge", Range(0,1)) = 0.1
 
        [Header(Noise)]
        _Noise ("Noise (RGB)", 2D) = "white" {}
        _Scale("Scale", Range(0,10)) = 0.5
        _MoveSpeed("Movespeed X,Y", Vector) = (1, -1, 0, 0)
 
        [Header(Colors)]
        [HDR]_Tint ("GradientMap Tint", Color) = (1,1,1,1)
        _GradientMap ("GradientMap (RGB)", 2D) = "white" {}
        _Stretch("Stretch", Range(-3,3)) = 2
        _Offset("Offset", Range(-3,3)) = 0
        
 
        [Header(Sparkles)]
        [HDR]_SparkleColor ("Sparkles Color", Color) = (1,1,1,1)   
        _Sparkles("Sparkles (RGB)", 2D) = "black" {}
        _ScaleSparkle("Scale Sparkles", Range(0,10)) = 0.2
        _SparkMoveSpeed("Sparkle Movespeed X,Y", Vector) = (1, 1, 0, 0)
        _SparkleOver("Sparkles Over Line", Range(0,1)) = 0.2
 
        
        [Header(Distortion)]
        _Distort("Sparkle Edge Distort", Range(-1,1)) = 0.2
        _SphereDist("Shape Distort", Range(-10,10)) = 5
        _LineSphereDist("Line Distort", Range(-1,1)) = -0.2
        _MaskStrength ("Shape Mask Strength", Range(0,1)) = 1
        
        [Header(Line)]
        _FillAmount ("Fill Amount", Range(0,1)) = 0.5
        _Speed("Speed", Range(0,100)) = 75     
        _Freq("Freq", Range(0,10)) = 0.5
        _Amplitude("Amplitude", Range(0,0.1)) = 0.05
         [Header(Line Width)]
        [HDR]_Color ("Line Color", Color) = (1,1,1,1)
        _LineWidth("Line Width", Range(0,1)) = 0.05
        _LineTop("Line Top Smoothness", Range(0,1)) = 0.05
        _LineBottom("Line Bottom Smoothness", Range(0,1)) = 0.05
 
 
        // UI Masking Stuff
 
        [HideInInspector]_StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil ("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask ("Stencil Read Mask", Float) = 255
 
        [HideInInspector]_ColorMask ("Color Mask", Float) = 15
 
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
 
        // UI Masking Stuff
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
 
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f
            {
                float2 uv_MainTex : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
 
            
            float4 _Noise_ST;
 
            sampler2D _Noise,_GradientMap;
            float _MaskStrength;
 
            // modified version of the Spherize UV node, takes in a SDF shapeMask
            float2 Unity_Spherize_float(float2 UV, float2 Center, float Strength, float2 Offset, float shapeMask)
            {
                float2 delta = saturate(UV*  (shapeMask * _MaskStrength) + 0.1)  - Center;
                float delta2 = dot(delta.xy, delta.xy);
                float delta4 = delta2 * delta2;
                float2 delta_offset = delta4 * Strength ;
 
                float2 Out = UV + delta * delta_offset + Offset;
                return Out;
            }
 
            float _Alphacutoff;
            float _FillAmount;
            fixed4 _Color;
            float _Speed;
            float2 _MoveSpeed,_SparkMoveSpeed;
            float _Stretch, _Offset;
            float _Distort;
            float _Freq;
            float _Amplitude;
            float _LineTop, _LineBottom, _LineWidth;
            float _SphereDist;
            float _Scale;
            sampler2D _MainTex,_Sparkles;
            float _LineSphereDist;
            float _SparkleOver;
            float _ScaleSparkle;
        
            float4 _SparkleColor;
            float _AlphaSmooth;
            float4 _Tint;
            float _DarkEdge;
          
 
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv_MainTex = TRANSFORM_TEX(v.uv, _Noise);
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {               
                // shape Mask
                float shapeMask = tex2D(_MainTex,i.uv_MainTex);
                
                // adjust uvs using the shape mask and movespeed
                float2 moveSpeed = (_Time.x * _MoveSpeed);
                float2 movingUV =  Unity_Spherize_float(i.uv_MainTex, float2(0.5,0.5),_SphereDist, moveSpeed , shapeMask );
                float2 movingUV2 =  Unity_Spherize_float(i.uv_MainTex, float2(0.5,0.5),_SphereDist, moveSpeed, shapeMask);
                
                // first noise layer
                fixed noise1 = tex2D (_Noise, movingUV * _Scale);
                // second noise layer
                fixed noise2 = tex2D (_Noise,movingUV2 * (_Scale * 0.5));
                // combined
                float noiseComb = (noise1 + noise2) * 0.5;
 
                // add colors via gradient map
                float2 colorMapUV = float2((noiseComb * _Stretch) + _Offset, 1);
                fixed4 gradientMap = tex2D (_GradientMap, colorMapUV);
 
                // create line with wobble
                float2 nonMovingUV = Unity_Spherize_float(i.uv_MainTex, float2(0.5,0.5),_LineSphereDist, float2(0,0), shapeMask );
                float wobble = sin(((nonMovingUV) * _Freq) + (_Speed * _Time.x)) * _Amplitude ;
                float wobbleLine = saturate((nonMovingUV.y) - wobble);
              
                float cutoffPoint = smoothstep(_FillAmount - _LineTop, _FillAmount , 1- wobbleLine);
 
                // line width 
                float lineWidth = saturate(1- smoothstep(_FillAmount + _LineWidth, _FillAmount + _LineWidth + _LineBottom , 1- wobbleLine)) * (cutoffPoint);
                
                // extra sparkles texture
                float2 sparkMoveSpeed = _Time.x * _SparkMoveSpeed;
                 float2 sparkUV = Unity_Spherize_float(i.uv_MainTex, float2(0.5,0.5),_Distort, moveSpeed, shapeMask);
                  float2 sparkUV2 = Unity_Spherize_float(i.uv_MainTex, float2(0.5,0.5),_Distort, sparkMoveSpeed, shapeMask );
                float4 sparkles = tex2D(_Sparkles, sparkUV * _ScaleSparkle) ;
                 float4 sparkles2 = tex2D(_Sparkles, sparkUV2 * _ScaleSparkle* 0.5) ;
                 sparkles = (sparkles * sparkles2) * 6;
                sparkles *= smoothstep(_FillAmount - _SparkleOver, _FillAmount , 1- wobbleLine);
              
 
                // combine
                float3 final = (gradientMap.rgb * _Tint * cutoffPoint) + (lineWidth * _Color);
 
                // add in sparkles
                final += sparkles * _SparkleColor * saturate((shapeMask) - 0.1);
 
                // darken edges
                final -= (1-saturate(shapeMask * 2 ))* _DarkEdge;
 
                // cut out mask shape
                float alpha = smoothstep(_Alphacutoff, _Alphacutoff + _AlphaSmooth, shapeMask * 2);
 
                // clip the alpha for any ui masking
                clip (alpha - 0.001);
 
                return float4(final,alpha);
            }
            ENDCG
        }
    }
}