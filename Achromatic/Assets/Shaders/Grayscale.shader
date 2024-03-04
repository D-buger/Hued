Shader"Unlit/Grayscale"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("ActivationColor", Color) = (0, 0, 0, 1)
        _Smoothless ("Smoothless", Range(0, 1)) = 0.5
        _Sensitivity ("Sensitivity", Range(0, 1)) = 0.8
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            half _Smoothless;
            half _Sensitivity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
    
                float grey = dot(col.rgb, float3(0.299, 0.587, 0.114));
                fixed4 greyscale = float4(grey * float3(1.2, 1.2, 1.2), 1.0);
                greyscale = pow(col, float4(3.0, 3.0, 3.0, 3.0));
    
                if (_Color.r > 0 || _Color.g > 0 || _Color.b > 0)
                {
                    if ((col.g + col.b) < col.r && _Color.r > 0)
                    {
                         greyscale.r = col.r;
                    }
                    if ((col.r + col.b) < col.g && _Color.g > 0)
                    {
                         greyscale.g = col.g;
                    }
                    if ((col.r + col.g) < col.b && _Color.b > 0)
                    {
                        greyscale.b = col.b;
                    }
                }
    
                return greyscale;
            }
            ENDCG
        }
    }
}
