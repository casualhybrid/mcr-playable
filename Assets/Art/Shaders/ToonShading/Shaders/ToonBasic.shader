// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Toon/Basic" {
	Properties{
		_Color("Main Color", Color) = (.5,.5,.5,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_ToonShade("ToonShader Cubemap(RGB)", CUBE) = "" { }
		_EmissionMap("Emission", 2D) = "white" { }
		 _Emission("Emission", float) = 0
		 _EmissionColor("Color", Color) = (0,0,0)
	}

		SubShader{
			Tags { "RenderType" = "Opaque" }
			Pass {
				Name "BASE"
			//	Cull Off

				CGPROGRAM
				#pragma vertex vert
			 	#pragma fragment frag
			    #pragma multi_compile_instancing
				#pragma multi_compile_fog
	


			 #include "Assets/VacuumShaders/Curved World/Shaders/cginc/CurvedWorld_Base.cginc"


				sampler2D _MainTex;
				samplerCUBE _ToonShade;
				float4 _MainTex_ST;
				float4 _Color;

				sampler2D _EmissionMap;
				fixed4 _EmissionColor;
				fixed _Emission;



				struct appdata {
					float4 vertex : POSITION;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					float3 normal : NORMAL;
				
				};

				struct v2f {
					float4 pos : SV_POSITION;
					UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					float3 cubenormal : TEXCOORD1;
					UNITY_FOG_COORDS(2)
				};

				inline void V_CW_TransformPoint(inout float4 vertex);

				v2f vert(appdata v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					V_CW_TransformPoint(v.vertex);
					o.pos = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.cubenormal = mul(UNITY_MATRIX_MV, float4(v.normal,0));

					UNITY_TRANSFER_FOG(o,o.pos);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = _Color * tex2D(_MainTex, i.texcoord);
					fixed4 cube = texCUBE(_ToonShade, i.cubenormal);
					fixed4 c = fixed4(2.0f * cube.rgb * col.rgb * i.color.rgb, col.a);

					half4 emission = tex2D(_EmissionMap, i.texcoord) * _EmissionColor * _Emission;
					c.rgb += emission.rgb;

					UNITY_APPLY_FOG(i.fogCoord, c);


					return c;
				}
				ENDCG
			}
		 }

			 Fallback "Unlit/Texture"
}