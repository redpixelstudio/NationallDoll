// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "IL3DN/Terrain First-Pass"
{
	Properties
	{
		[HideInInspector]_Control("Control", 2D) = "white" {}
		[HideInInspector]_Splat3("Splat3", 2D) = "white" {}
		[HideInInspector]_Splat2("Splat2", 2D) = "white" {}
		[HideInInspector]_Splat1("Splat1", 2D) = "white" {}
		[HideInInspector]_Splat0("Splat0", 2D) = "white" {}
		_Color0("Color 0", Color) = (0,0,0,0)
		_Color1("Color 1", Color) = (0,0,0,0)
		_Color2("Color 2", Color) = (0,0,0,0)
		_Color3("Color 3", Color) = (0,0,0,0)
		[Toggle(_SNOW_ON)] _Snow("Snow", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry-100" "SplatCount"="4" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma shader_feature _SNOW_ON
		#pragma surface surf Standard keepalpha vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
		};

		uniform sampler2D _Control;
		uniform float4 _Control_ST;
		uniform float SnowTerrainFloat;
		uniform float4 _Color0;
		uniform sampler2D _Splat0;
		uniform float4 _Splat0_ST;
		uniform float4 _Color1;
		uniform sampler2D _Splat1;
		uniform float4 _Splat1_ST;
		uniform float4 _Color2;
		uniform sampler2D _Splat2;
		uniform float4 _Splat2_ST;
		uniform float4 _Color3;
		uniform sampler2D _Splat3;
		uniform float4 _Splat3_ST;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float localCalculateTangentsStandard99_g4 = ( 0.0 );
			{
			v.tangent.xyz = cross ( v.normal, float3( 0, 0, 1 ) );
			v.tangent.w = -1;
			}
			float3 temp_cast_0 = (localCalculateTangentsStandard99_g4).xxx;
			v.vertex.xyz += temp_cast_0;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Control = i.uv_texcoord * _Control_ST.xy + _Control_ST.zw;
			float4 tex2DNode5_g4 = tex2D( _Control, uv_Control );
			float dotResult20_g4 = dot( tex2DNode5_g4 , float4(1,1,1,1) );
			float SplatWeight22_g4 = dotResult20_g4;
			float localSplatClip74_g4 = ( SplatWeight22_g4 );
			float SplatWeight74_g4 = SplatWeight22_g4;
			{
			#if !defined(SHADER_API_MOBILE) && defined(TERRAIN_SPLAT_ADDPASS)
				clip(SplatWeight74_g4 == 0.0f ? -1 : 1);
			#endif
			}
			float4 SplatControl26_g4 = ( tex2DNode5_g4 / ( localSplatClip74_g4 + 0.001 ) );
			float3 ase_worldNormal = i.worldNormal;
			#ifdef _SNOW_ON
				float staticSwitch88_g4 = ( ase_worldNormal.y * SnowTerrainFloat );
			#else
				float staticSwitch88_g4 = 0.0;
			#endif
			float2 uv_Splat0 = i.uv_texcoord * _Splat0_ST.xy + _Splat0_ST.zw;
			float2 uv_Splat1 = i.uv_texcoord * _Splat1_ST.xy + _Splat1_ST.zw;
			float2 uv_Splat2 = i.uv_texcoord * _Splat2_ST.xy + _Splat2_ST.zw;
			float2 uv_Splat3 = i.uv_texcoord * _Splat3_ST.xy + _Splat3_ST.zw;
			float4 weightedBlendVar9_g4 = SplatControl26_g4;
			float4 weightedBlend9_g4 = ( weightedBlendVar9_g4.x*saturate( ( staticSwitch88_g4 + ( _Color0 * tex2D( _Splat0, uv_Splat0 ) ) ) ) + weightedBlendVar9_g4.y*saturate( ( staticSwitch88_g4 + ( _Color1 * tex2D( _Splat1, uv_Splat1 ) ) ) ) + weightedBlendVar9_g4.z*saturate( ( staticSwitch88_g4 + ( _Color2 * tex2D( _Splat2, uv_Splat2 ) ) ) ) + weightedBlendVar9_g4.w*saturate( ( staticSwitch88_g4 + ( _Color3 * tex2D( _Splat3, uv_Splat3 ) ) ) ) );
			float4 MixDiffuse28_g4 = weightedBlend9_g4;
			o.Albedo = MixDiffuse28_g4.xyz;
			o.Alpha = 1;
		}

		ENDCG
	}

	Dependency "BaseMapShader"="ASESampleShaders/SimpleTerrainBase"
	Fallback "Nature/Terrain/Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18901
1402;130;1787;1451;962.7813;658.62;1;True;True
Node;AmplifyShaderEditor.FunctionNode;20;-445.678,-166.3597;Inherit;False;IL3DN - Four Splats First Pass Terrain;0;;4;c3e2b73ed8eb8c64681162a2abca2a89;0;2;59;FLOAT4;0,0,0,0;False;60;FLOAT4;0,0,0,0;False;2;FLOAT4;0;FLOAT;100
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;89,-171;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;IL3DN/Terrain First-Pass;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;-100;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;Nature/Terrain/Diffuse;-1;-1;-1;-1;1;SplatCount=4;False;1;BaseMapShader=ASESampleShaders/SimpleTerrainBase;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;0;0;20;0
WireConnection;0;11;20;100
ASEEND*/
//CHKSM=6C1D1D12CD4A6AC9A00B48D875576A053CEC89A1