Shader "Custom/GlassShader_Mobile"
{
    Properties
    {
        _ReflectionCubemap ("Reflection Cubemap", Cube) = "_Skybox" {}
        _ReflectionIntensity ("Reflection Intensity", Range(0,1)) = 1.0
        _HighlightThreshold ("Highlight Threshold", Range(0,1)) = 0.8
        _Alpha ("Transparency Alpha", Range(0,1)) = 0.2
        _TintColor ("Tint Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha // Proper transparency blending
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };
            
            samplerCUBE _ReflectionCubemap;
            float _ReflectionIntensity;
            float _HighlightThreshold;
            float _Alpha;
            fixed4 _TintColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 reflectDir = reflect(-viewDir, normalize(i.worldNormal));
                
                fixed4 reflectionColor = texCUBE(_ReflectionCubemap, reflectDir) * _ReflectionIntensity;
                float reflectionStrength = max(max(reflectionColor.r, reflectionColor.g), reflectionColor.b);
                
                // Extract only the highlights based on the threshold
                float highlightMask = step(_HighlightThreshold, reflectionStrength);
                reflectionColor.rgb *= highlightMask;
                
                // Apply tint to the entire cubemap
                reflectionColor.rgb *= _TintColor.rgb;
                
                fixed4 finalColor = fixed4(reflectionColor.rgb, _Alpha);
                
                return finalColor;
            }
            ENDCG
        }
    }
}
