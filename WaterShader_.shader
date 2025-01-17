Shader "Custom/WaterShader_"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.2, 0.4, 1.0, 1.0)
        _WaveSpeed ("Wave Speed", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Include URP libraries
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
             #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _BaseColor; 
            float _WaveSpeed;  

            // Hash function for Perlin noise
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            // Perlin noise function
            float perlinNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float2 u = f * f * (3.0 - 2.0 * f);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
{
    float4 vertex : SV_POSITION;
    float3 worldNormal : TEXCOORD1;
    float3 worldPos : TEXCOORD2; 
    float2 uv : TEXCOORD0;
};

v2f vert(appdata v)
{
    v2f o;
    o.vertex = TransformObjectToHClip(v.vertex);        
    o.worldNormal = TransformObjectToWorldNormal(v.normal);
    o.worldPos = TransformObjectToWorld(v.vertex).xyz;    
    o.uv = v.uv;
    return o;
}


half4 frag(v2f i) : SV_Target
{
    // Perlin noise for waves
    float rippleNoise1 = perlinNoise(i.uv * 50.0 + _Time.y * _WaveSpeed);
    float rippleNoise2 = perlinNoise(i.uv * 100.0 + _Time.y * (_WaveSpeed * 0.5));
    float rippleNoise = rippleNoise1 * 0.7 + rippleNoise2 * 0.3;

    float3 perturbedNormal = normalize(i.worldNormal + float3(rippleNoise * 1.0, rippleNoise * 1.0, 1.0));

    // Get the main light direction
    Light mainLight = GetMainLight();
    float3 lightDir = normalize(mainLight.direction);

    float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);

    // Diffuse light
    float diff = saturate(dot(-perturbedNormal, -lightDir)) * 1.5;

    // Specular light
    float3 halfDir = normalize(lightDir + viewDir);      
    float spec = pow(saturate(dot(perturbedNormal, halfDir)), 35.0); 

    float3 ambientColor = float3(0.1, 0.2, 0.5);          
    float3 finalColor = ambientColor + (_BaseColor.rgb * diff) + spec * float3(2.0, 2.0, 2.0); 

    return half4(finalColor, _BaseColor.a);
}



            ENDHLSL
        }
    }
}
