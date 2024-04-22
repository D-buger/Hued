Shader"Unlit/Grayscale"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("ActivationColor", Color) = (0, 0, 0, 1)
        _Filter ("VisibleColor", Vector) = (0.5, 0.5, 0, 0)
    }
    SubShader
    {
        Tags {"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        
        Pass
        {
            HLSLPROGRAM

            #pragma prefer_hlslcc gles   
            #pragma exclude_renderers d3d11_9x 

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 screenPos : TEXCOORD1;
            };
                        
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 screenPos : TEXCOORD1;
            };
                        
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _Filter;
                     
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }
                             
            float3 rgb2hsv(float3 rgb)
            {
                float4 k = float4(0.0f, -1.0f / 3.0f, 2.0f / 3.0f, -1.0f);
                float4 p = lerp(float4(rgb.bg, k.wz), float4(rgb.gb, k.xy), step(rgb.z, rgb.g));
                float4 q = lerp(float4(p.xyw, rgb.r), float4(rgb.r, p.yzx), step(p.x, rgb.r));
                
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }
            
            float3 hsv2rgb(float3 hsv)
            {
                float4 k = float4(1.0f, 2.0f / 3.0f, 1.0f / 3.0f, 3.0f);
                float3 p = abs(frac(hsv.xxx + k.xyz) * 6.0f - k.www);
                return hsv.z * lerp(k.xxx, saturate(p - k.xxx), hsv.y);
            
            }      
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                if(distance(i.screenPos.xy, _Filter.xy) > 0.3){
                    float gray = col.r * 0.299 + col.g * 0.587 + col.b * 0.114;
                    float4 grayscale = float4(gray, gray, gray, 1);
                    //h = ����, s = ä��, v = ����
                      float3 hsv = rgb2hsv(col.xyz);
                    
                      if (_Color.r > 0)
                      {
                          if (hsv.r < 0.25f || hsv.r > 0.75f)
                          {
                              grayscale.r = col.r;
                              grayscale.g = col.g;
                              grayscale.b = col.b;
                          }
                      }
                      if (_Color.g > 0)
                      {
                          if (hsv.r > 0.1f && hsv.r < 0.6f)
                          {
                              grayscale.r = col.r;
                              grayscale.g = col.g;
                              grayscale.b = col.b;
                          }
                      }
                      if (_Color.b > 0)
                      {
                          if (hsv.r > 0.4f && hsv.r < 0.9f)
                          {
                              grayscale.r = col.r;
                              grayscale.g = col.g;
                              grayscale.b = col.b;
                          }
                      }

                    return grayscale;
                }
                else{
                    return col;
                }
            }
            ENDHLSL 
        }
        // Pass
        // {
        //     HLSLPROGRAM
        //     #pragma vertex vert
        //     #pragma fragment frag

        //     float _UpperFeather;
        //     float _BottomFeather;

        //     fixed4 frag(v2f i) : SV_Target
        //     {
        //         fixed4 col = tex2D(_MainTex, i.uv);
        //         float2 newVU = i.uv * 2 - 1;
        //         float ring = length(newVU);

        //         return fixed4(ring.xxx, 1);
        //     }


        //     ENDHLSL 
        // }
    }
    Fallback "Diffuse"
}
