Shader"Unlit/Grayscale"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("ActivationColor", Color) = (0, 0, 0, 1)
        _Range ("Range", Range(0, 1)) = 0.4
    }
    SubShader
    {
        Tags {"RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque"}
        LOD 100
        ZTest Always ZWrite Off cull Off

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
            };
                        
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
                        
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            half _Range;
                        
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
                        
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                            
                float gray = col.r * 0.299 + col.g * 0.587 + col.b * 0.114;
                float4 grayscale = float4(gray, gray, gray, 1);
                            
                if (_Color.r > 0 || _Color.g > 0 || _Color.b > 0)
                {
                    if (_Color.r > 0)
                    {
                    
                        if ((col.g < 0.5f && col.r > _Range) || (col.b < 0.5f && col.r > _Range))
                        {
                            grayscale.r = col.r;
                            grayscale.g = col.g;
                            grayscale.b = col.b;
                        }
                    }
                    if (_Color.g > 0)
                    {
                        if ((col.r < 0.5f && col.g > _Range) || (col.b < 0.5f && col.g > _Range))
                        {
                            grayscale.r = col.r;
                            grayscale.g = col.g;
                            grayscale.b = col.b;
                        }
                    }
                    if (_Color.b > 0)
                    {
                        if ((col.g < 0.5f && col.b > _Range) || (col.r < 0.5f && col.b > _Range))
                        {
                            grayscale.r = col.r;
                            grayscale.g = col.g;
                            grayscale.b = col.b;
                        }
                    }
                }
                            
                return grayscale;
            }
            ENDHLSL 
        }
    }
}
