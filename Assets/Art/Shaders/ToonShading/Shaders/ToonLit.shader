Shader "Toon/Lit" {
	Properties{
		_Color("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
	_Ramp("Toon Ramp (RGB)", 2D) = "gray" {}
	  _EmissionMap("Emission", 2D) = "white" { }
	 _Emission("Emission", float) = 0
	 _EmissionColor("Color", Color) = (0,0,0)
	  _Cutoff("Alpha Cutoff", Range(0.000000,1.000000)) = 0.0
	}

		SubShader{
			 Tags { "RenderType" = "Opaque" }

			  LOD 200

		 // Blend SrcAlpha OneMinusSrcAlpha
 CGPROGRAM

			 #pragma surface surf ToonRamp
		 #pragma vertex vert

			#include "UnityCG.cginc"

			  #include "Assets/VacuumShaders/Curved World/Shaders/cginc/CurvedWorld_Base.cginc"

			  sampler2D _Ramp;
			   sampler2D _EmissionMap;
			   fixed4 _EmissionColor;
			   fixed _Emission;
			   float _Cutoff;

			   // custom lighting function that uses a texture ramp based
			   // on angle between light direction and normal
			   #pragma lighting ToonRamp exclude_path:prepass
			   inline half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten)
			   {
				   #ifndef USING_DIRECTIONAL_LIGHT
				   lightDir = normalize(lightDir);
				   #endif

				   half d = dot(s.Normal, lightDir)*0.5 + 0.5;
				   half3 ramp = tex2D(_Ramp, float2(d,d)).rgb;

				   half4 c;
				   c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
				   c.a = 0;

				   return c;
			   }

			   sampler2D _MainTex;
			   float4 _Color;

			   struct Input {
				   float2 uv_MainTex : TEXCOORD0;
			   };

			   void vert(inout appdata_full v, out Input o)
			   {
				   UNITY_INITIALIZE_OUTPUT(Input, o);

				   //CurvedWorld vertex transform
				   V_CW_TransformPointAndNormal(v.vertex, v.normal, v.tangent);
			   }

			   inline void V_CW_TransformPointAndNormal(inout float4 vertex, inout float3 normal,
				   float4 tangent);

			   void surf(Input IN, inout SurfaceOutput o) {
					half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
					o.Albedo = c.rgb;

					  clip(c.a - _Cutoff);
					   o.Emission = _EmissionColor * _Emission *  tex2D(_EmissionMap, IN.uv_MainTex);
					  }
					  ENDCG
	 }

		 Fallback "Diffuse"
}