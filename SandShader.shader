Shader "Custom/SandShader"
{
    Properties
    {
        _BumpScale ("Bumpiness Scale", Float) = 2.5
        _SandColor ("Sand Color", Color) = (0.1, 0.8, 0.8, 1.0) 
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Include URP libraries
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Properties from the Unity editor
            float _BumpScale;
            half4 _SandColor;

            /* PERLIN NOISE */  

            float2 grad2(int hash) {
            int h = hash & 3; 
            if (h == 0) return float2(1.0, 0.0); 
            if (h == 1) return float2(-1.0, 0.0); 
            if (h == 2) return float2(0.0, 1.0); 
            return float2(0.0, -1.0); 
            }

        
            float dotGridGradient(float2 gridPoint, float2 samplePoint, int hash) {
            float2 gradient = grad2(hash);
            float2 offset = samplePoint - gridPoint;
            return dot(gradient, offset);
        
            }

            float smoothstep5(float t) {
                 return t * t * t * (t * (t * 6 - 15) + 10);
            }

            // Hash function
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            // Hash function to generate pseudo-random integers
            int permHash(float2 p) {
                return int(frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123) * 256.0);
            }


            // Simplified Perlin noise function
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


            // Perlin noise function from lab 
            float perlinNoise3(float2 p) {
             

                float2 i = floor(p);
                float2 f = frac(p);

                float2 u = f * f * (3.0 - 2.0 * f);

                float2 g00 = grad2(permHash(i));
                float2 g10 = grad2(permHash(i + float2(1.0, 0.0)));
                float2 g01 = grad2(permHash(i + float2(0.0, 1.0)));
                float2 g11 = grad2(permHash(i + float2(1.0, 1.0)));

                float n00 = dot(g00, f - float2(0.0, 0.0));
                float n10 = dot(g10, f - float2(1.0, 0.0));
                float n01 = dot(g01, f - float2(0.0, 1.0));
                float n11 = dot(g11, f - float2(1.0, 1.0));

             
                float nx0 = lerp(n00, n10, u.x);
                float nx1 = lerp(n01, n11, u.x);
                return lerp(nx0, nx1, u.y); 
            }

          
            float rand1dTo1d(float2 value, float mutator = 0.546){
                float random = frac(sin(value + mutator) * 143758.5453);
                return random;
            }
            float easeIn(float interpolator){
                        return interpolator * interpolator;
            }

            float easeOut(float interpolator){
                return 1 - easeIn(1 - interpolator);
            }

            float easeInOut(float interpolator){
                float easeInValue = easeIn(interpolator);
                float easeOutValue = easeOut(interpolator);
                return lerp(easeInValue, easeOutValue, interpolator);
            }

            // Perlin Noise from Ronjas tutorials 
            float perlinNoise2(float2 value){
            float fraction = frac(value);
            float interpolator = easeInOut(fraction);

            float previousCellInclination = rand1dTo1d(floor(value)) * 2 - 1;
            float previousCellLinePoint = previousCellInclination * fraction;

            float nextCellInclination = rand1dTo1d(ceil(value)) * 2 - 1;
            float nextCellLinePoint = nextCellInclination * (fraction - 1);

            return lerp(previousCellLinePoint, nextCellLinePoint, interpolator);
            
            }

            /* SIMPLEX NOISE */

            float simplexNoise(float2 v)
            {
                const float K1 = 0.36602540378; 
                const float K2 = 0.2113248654;  

                float2 i = floor(v + dot(v, float2(K1, K1)));
                float2 x0 = v - i + dot(i, float2(K2, K2));
              
                float2 i1;
                i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);

                float2 x1 = x0 - i1 + float2(K2, K2);
                float2 x2 = x0 - float2(1.0, 1.0) + 2.0 * float2(K2, K2);

                float n = hash(i);
                float n1 = hash(i + i1);
                float n2 = hash(i + float2(1.0, 1.0));

                float t0 = 0.5 - dot(x0, x0);
                float t1 = 0.5 - dot(x1, x1);
                float t2 = 0.5 - dot(x2, x2);

                if (t0 < 0.0) n = 0.0; else t0 *= t0;
                if (t1 < 0.0) n1 = 0.0; else t1 *= t1;
                if (t2 < 0.0) n2 = 0.0; else t2 *= t2;

                return 70.0 * (t0 * t0 * dot(x0, float2(n, n)) +
                            t1 * t1 * dot(x1, float2(n1, n1)) +
                            t2 * t2 * dot(x2, float2(n2, n2)));
            }


            /* VORONOI NOISE*/

            float voronoi(float2 v)
            {
                float2 p = floor(v);
                float2 f = frac(v);

                float res = 8.0; 
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 g = float2(x, y);
                        float2 offset = g + hash(p + g);
                        res = min(res, length(offset - f));
                    }
                }
                return res;
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
            };

            // Vertex Shader
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex); 
                o.uv = v.uv;

                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                return o;
            }



            // Fragment Shader
            half4 frag(v2f i) : SV_Target
            {
             
                half4 sandColor = _SandColor;

                // Combine Perlin, Simplex, and Voronoi noise
                float perlinA = perlinNoise2(i.uv * 50.0);
                float perlinB = perlinNoise(i.uv * 50.0);
                float perlinC = perlinNoise3(i.uv*50.0);
                float simplex = simplexNoise(i.uv * 10.0);
                float voronoiPattern = voronoi(i.uv * 6.0);
                float combinedNoise = (0.3*perlinA + 0.5*perlinC + 0.01*simplex + voronoiPattern*0.4 + perlinB*0.8) * _BumpScale;
    
              
                // Perturb normal
                float3 perturbedNormal = normalize(i.worldNormal + float3(combinedNoise, combinedNoise, 0.4));
           
                // Get main directional light 
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 lightColor = mainLight.color;

                //Diffuse lighting
                float diff = max(0.0, dot(-perturbedNormal, -lightDir));
                sandColor.rgb *= diff * lightColor;


                return half4(sandColor);
               

            }
            ENDHLSL
        }
    }
}

