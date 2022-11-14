// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "IL3DN/Leaf"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_AlphaCutoff("Alpha Cutoff", Range( 0 , 1)) = 0.5
		_MainTex("MainTex", 2D) = "white" {}
		[Toggle(_SNOW_ON)] _Snow("Snow", Float) = 1
		[Toggle(_WIND_ON)] _Wind("Wind", Float) = 1
		_WindStrenght("Wind Strenght", Range( 0 , 1)) = 0.5
		[Toggle(_WIGGLE_ON)] _Wiggle("Wiggle", Float) = 1
		_WiggleStrenght("Wiggle Strenght", Range( 0 , 1)) = 0.5
		_ColorVariationTexture("Color Variation Texture", 2D) = "white" {}
		_ColorVariationNoise("Color Variation Noise", Range( 0 , 1)) = 0
		_ColorVariationScale("Color Variation Scale", Range( 0 , 100)) = 50
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Off
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma multi_compile __ _WIND_ON
		#pragma multi_compile __ _SNOW_ON
		#pragma multi_compile __ _WIGGLE_ON
		#include "VS_indirect.cginc"
		#pragma instancing_options procedural:setup
		#pragma multi_compile GPU_FRUSTUM_ON__
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
		};

		uniform float3 WindDirection;
		uniform sampler2D NoiseTextureFloat;
		uniform float WindSpeedFloat;
		uniform float WindTurbulenceFloat;
		uniform float _WindStrenght;
		uniform float WindStrenghtFloat;
		uniform float SnowLeavesFloat;
		sampler2D _ColorVariationTexture;
		uniform float _ColorVariationScale;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform float LeavesWiggleFloat;
		uniform float _WiggleStrenght;
		uniform float _ColorVariationNoise;
		uniform float AlphaCutoffFloat;
		uniform float _AlphaCutoff;


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		inline float4 TriplanarSampling961( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 myWindDirection972 = WindDirection;
			float3 temp_output_927_0 = float3( (myWindDirection972).xz ,  0.0 );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 panner931 = ( 1.0 * _Time.y * ( temp_output_927_0 * WindSpeedFloat * 10.0 ).xy + (ase_worldPos).xy);
			float4 worldNoise905 = ( tex2Dlod( NoiseTextureFloat, float4( ( ( panner931 * WindTurbulenceFloat ) / float2( 10,10 ) ), 0, 0.0) ) * _WindStrenght * WindStrenghtFloat );
			float4 transform886 = mul(unity_WorldToObject,( float4( myWindDirection972 , 0.0 ) * ( ( v.color.a * worldNoise905 ) + ( worldNoise905 * v.color.g ) ) ));
			#ifdef _WIND_ON
				float4 staticSwitch897 = ( transform886 + float4( 0,0,0,0 ) );
			#else
				float4 staticSwitch897 = float4( 0,0,0,0 );
			#endif
			v.vertex.xyz += staticSwitch897.xyz;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Normal = float3(0,0,1);
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			#ifdef _SNOW_ON
				float staticSwitch917 = ( saturate( pow( ase_worldNormal.y , 1.0 ) ) * SnowLeavesFloat );
			#else
				float staticSwitch917 = 0.0;
			#endif
			float3 ase_worldPos = i.worldPos;
			float4 triplanar961 = TriplanarSampling961( _ColorVariationTexture, ase_worldPos, ase_worldNormal, 1.0, ( float2( 1,1 ) / _ColorVariationScale ), 1.0, 0 );
			float3 hsvTorgb955 = RGBToHSV( _Color.rgb );
			float4 normalizeResult957 = normalize( ( triplanar961 * hsvTorgb955.x ) );
			float3 hsvTorgb959 = HSVToRGB( float3(normalizeResult957.x,hsvTorgb955.y,hsvTorgb955.z) );
			float3 myWindDirection972 = WindDirection;
			float3 temp_output_927_0 = float3( (myWindDirection972).xz ,  0.0 );
			float2 panner931 = ( 1.0 * _Time.y * ( temp_output_927_0 * WindSpeedFloat * 10.0 ).xy + (ase_worldPos).xy);
			float4 worldNoise905 = ( tex2D( NoiseTextureFloat, ( ( panner931 * WindTurbulenceFloat ) / float2( 10,10 ) ) ) * _WindStrenght * WindStrenghtFloat );
			float cos945 = cos( ( tex2D( NoiseTextureFloat, worldNoise905.rg ) * LeavesWiggleFloat * _WiggleStrenght ).r );
			float sin945 = sin( ( tex2D( NoiseTextureFloat, worldNoise905.rg ) * LeavesWiggleFloat * _WiggleStrenght ).r );
			float2 rotator945 = mul( i.uv_texcoord - float2( 0.5,0.5 ) , float2x2( cos945 , -sin945 , sin945 , cos945 )) + float2( 0.5,0.5 );
			#ifdef _WIGGLE_ON
				float2 staticSwitch898 = rotator945;
			#else
				float2 staticSwitch898 = i.uv_texcoord;
			#endif
			float4 tex2DNode97 = tex2D( _MainTex, staticSwitch898 );
			float4 blendOpSrc960 = float4( hsvTorgb959 , 0.0 );
			float4 blendOpDest960 = ( _Color * tex2DNode97 );
			float4 lerpBlendMode960 = lerp(blendOpDest960,2.0f*blendOpDest960*blendOpSrc960 + blendOpDest960*blendOpDest960*(1.0f - 2.0f*blendOpSrc960),_ColorVariationNoise);
			o.Albedo = saturate( ( staticSwitch917 + ( saturate( lerpBlendMode960 )) ) ).rgb;
			o.Alpha = 1;
			#ifdef _SNOW_ON
				float staticSwitch921 = AlphaCutoffFloat;
			#else
				float staticSwitch921 = 1.0;
			#endif
			clip( ( tex2DNode97.a / staticSwitch921 ) - _AlphaCutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma exclude_renderers vulkan xbox360 psp2 n3ds wiiu 
		#pragma surface surf Lambert keepalpha fullforwardshadows nolightmap  nodirlightmap dithercrossfade vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18901
1251;81;2034;1453;-249.2757;-1149.422;2.089701;True;True
Node;AmplifyShaderEditor.Vector3Node;867;1175.415,1172.312;Float;False;Global;WindDirection;WindDirection;14;0;Create;True;0;0;0;False;0;False;0,0,0;-0.7071068,0,-0.7071068;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;972;1484.74,1170.026;Inherit;False;myWindDirection;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;973;886.6414,1708.318;Inherit;False;972;myWindDirection;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;924;1183.945,1429.519;Inherit;False;1616.924;639.8218;World Noise;15;950;930;928;937;934;935;936;933;931;932;929;927;926;925;946;World Noise;1,0,0.02020931,1;0;0
Node;AmplifyShaderEditor.SwizzleNode;925;1213.581,1704.855;Inherit;False;FLOAT2;0;2;1;2;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;950;1493.9,1973.622;Inherit;False;Constant;_Float0;Float 0;10;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TransformDirectionNode;927;1421.582,1704.855;Inherit;False;World;World;True;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;928;1210.048,1483.933;Float;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;926;1380.187,1880.807;Float;False;Global;WindSpeedFloat;WindSpeedFloat;3;0;Create;False;0;0;0;False;0;False;0.5;0.4;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;929;1505.27,1484.348;Inherit;False;FLOAT2;0;1;2;2;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;930;1671.314,1860.966;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PannerNode;931;1835.208,1492.412;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;932;1747.459,1662.68;Float;False;Global;WindTurbulenceFloat;WindTurbulenceFloat;4;0;Create;False;0;0;0;False;0;False;0.5;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;933;2038.291,1492.52;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;946;2203.88,1489.946;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;10,10;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;936;2349.719,1485.238;Inherit;True;Global;NoiseTextureFloat;NoiseTextureFloat;4;0;Create;False;0;0;0;False;0;False;-1;None;e5055e0d246bd1047bdb28057a93753c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;935;2278.888,1759.618;Float;False;Property;_WindStrenght;Wind Strenght;6;0;Create;False;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;934;2275.242,1865.899;Float;False;Global;WindStrenghtFloat;WindStrenghtFloat;3;0;Create;False;0;0;0;False;0;False;0.5;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;937;2652.036,1740.933;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;905;2872.686,1734.798;Float;False;worldNoise;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;907;1562.809,830.8305;Inherit;False;905;worldNoise;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;938;1781.895,748.6516;Inherit;False;1012.714;535.89;UV Animation;6;945;944;943;942;941;940;UV Animation;0.7678117,1,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;951;668.1337,-394.9345;Inherit;False;2135.505;586.8222;Use world noise to add color variation to the leaves.;9;961;959;958;957;956;955;954;953;952;Color Variation;0.9623866,0,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;941;1891.647,1193.185;Float;False;Property;_WiggleStrenght;Wiggle Strenght;8;0;Create;False;0;0;0;False;0;False;0.5;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;940;1893.621,1079.119;Float;False;Global;LeavesWiggleFloat;LeavesWiggleFloat;5;0;Create;False;0;0;0;False;0;False;0.5;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;942;1860.386,805.7994;Inherit;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;936;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;952;719.2756,-78.22074;Inherit;False;Property;_ColorVariationScale;Color Variation Scale;13;0;Create;True;0;0;0;False;0;False;50;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;953;837.1956,-277.3235;Inherit;False;Constant;_Vector0;Vector 0;13;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;911;1727.516,274.4578;Inherit;False;1075.409;358.2535;Snow;5;916;915;914;913;912;Snow;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;908;2199.436,2102.639;Inherit;False;608.7889;673.9627;Vertex Animation;5;857;854;855;856;853;Vertex Animation;0,1,0.8708036,1;0;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;954;1094.163,-274.3723;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;944;2288.745,817.4724;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;943;2365.907,1099.493;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;292;4011.016,958.6533;Float;False;Property;_Color;Color;0;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.7058823,0.5882353,0.1843136,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;746;3442.74,1028.462;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RGBToHSVNode;955;1455.525,-92.90081;Inherit;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;912;1763.288,348.2159;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RotatorNode;945;2533.803,951.7216;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexColorNode;856;2257.083,2590.475;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;853;2252.385,2177.409;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;906;1936.904,2418.457;Inherit;False;905;worldNoise;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TriplanarNode;961;1281.489,-322.9888;Inherit;True;Spherical;World;False;Color Variation Texture;_ColorVariationTexture;white;11;None;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;913;2039.218,394.2268;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;898;3693.987,1190.03;Float;False;Property;_Wiggle;Wiggle;7;0;Create;True;0;0;0;False;0;False;1;1;1;True;_WIND_ON;Toggle;2;Key0;Key1;Create;False;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;854;2522.822,2336.65;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;956;1794.923,-120.027;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;855;2525.823,2474.211;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalizeNode;957;1963.934,-118.7294;Inherit;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;974;2703.344,1336.603;Inherit;False;972;myWindDirection;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;914;2225.051,394.0062;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;857;2674.263,2397.85;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;97;3955.139,1167.211;Inherit;True;Property;_MainTex;MainTex;2;0;Create;True;0;0;0;False;0;False;-1;None;6ab0f5f5ed2482e43a5ace7eeced19e6;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;915;1750.874,543.1254;Inherit;False;Global;SnowLeavesFloat;SnowLeavesFloat;4;0;Create;True;0;0;0;False;0;False;1;1;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;293;4285.25,1058.456;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;872;3059.303,1341.415;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.HSVToRGBNode;959;2180.145,-64.91865;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;958;2127.307,98.4324;Inherit;False;Property;_ColorVariationNoise;Color Variation Noise;12;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;916;2451.704,524.162;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;917;3684.1,518.5128;Inherit;False;Property;_Snow;Snow;3;0;Create;True;0;0;0;False;0;False;1;1;1;True;;Toggle;2;Key0;Key1;Create;False;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;960;4381.472,-204.7664;Inherit;False;SoftLight;True;3;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;920;3644.657,1669.175;Inherit;False;Global;AlphaCutoffFloat;AlphaCutoffFloat;2;0;Create;False;0;0;0;False;0;False;2.1;2.1;1;2.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;919;3770.889,1559.604;Inherit;False;Constant;_Float1;Float 1;9;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldToObjectTransfNode;886;3253.275,1340.256;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;921;4156.484,1631.802;Inherit;False;Property;_Snow;Snow;3;0;Create;True;0;0;0;False;0;False;1;1;1;True;_SNOW_ON;Toggle;2;Key0;Key1;Reference;917;False;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;975;3522.705,1335.606;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;918;4673.59,628.6623;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;962;1424.083,2834.734;Inherit;False;1376.448;561.1524;Whole tree swing animations;8;971;970;969;967;965;978;979;980;Swing;0.02349341,1,0,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;976;1217.041,2653.109;Inherit;False;Property;_SwingAmount;Swing Amount;9;0;Create;True;0;0;0;False;0;False;0.1;10;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinTimeNode;978;1493.882,2915.372;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;977;1285.24,3113.934;Inherit;False;Property;_SwingSpeed;Swing Speed;10;0;Create;True;0;0;0;False;0;False;0.1;10;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;967;1875.376,3195.278;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;980;1964.211,2975.616;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;923;4835.925,1039.788;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;979;1696.84,2982.034;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;969;2227.564,2951.134;Inherit;False;False;4;0;FLOAT3;1,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0.01,0.01,0.01;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;922;4329.29,1262.201;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;910;3971.231,1403.038;Float;False;Property;_AlphaCutoff;Alpha Cutoff;1;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;897;3701.915,1311.666;Float;False;Property;_Wind;Wind;5;0;Create;True;0;0;0;False;0;False;1;1;1;True;;Toggle;2;Key0;Key1;Create;False;True;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;971;2629.236,2950.337;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldPosInputsNode;965;1629.054,3205.997;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PosVertexDataNode;970;2275.245,3171.653;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;5003.316,1043.412;Float;False;True;-1;2;;0;0;Lambert;IL3DN/Leaf;False;False;False;False;False;False;True;False;True;False;False;False;True;False;False;False;True;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;9;d3d9;d3d11_9x;d3d11;glcore;gles;gles3;metal;xboxone;ps4;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;892;-1;0;True;910;3;Include;VS_indirect.cginc;False;;Custom;Pragma;instancing_options procedural:setup;False;;Custom;Pragma;multi_compile GPU_FRUSTUM_ON__;False;;Custom;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;972;0;867;0
WireConnection;925;0;973;0
WireConnection;927;0;925;0
WireConnection;929;0;928;0
WireConnection;930;0;927;0
WireConnection;930;1;926;0
WireConnection;930;2;950;0
WireConnection;931;0;929;0
WireConnection;931;2;930;0
WireConnection;933;0;931;0
WireConnection;933;1;932;0
WireConnection;946;0;933;0
WireConnection;936;1;946;0
WireConnection;937;0;936;0
WireConnection;937;1;935;0
WireConnection;937;2;934;0
WireConnection;905;0;937;0
WireConnection;942;1;907;0
WireConnection;954;0;953;0
WireConnection;954;1;952;0
WireConnection;943;0;942;0
WireConnection;943;1;940;0
WireConnection;943;2;941;0
WireConnection;955;0;292;0
WireConnection;945;0;944;0
WireConnection;945;2;943;0
WireConnection;961;3;954;0
WireConnection;913;0;912;2
WireConnection;898;1;746;0
WireConnection;898;0;945;0
WireConnection;854;0;853;4
WireConnection;854;1;906;0
WireConnection;956;0;961;0
WireConnection;956;1;955;1
WireConnection;855;0;906;0
WireConnection;855;1;856;2
WireConnection;957;0;956;0
WireConnection;914;0;913;0
WireConnection;857;0;854;0
WireConnection;857;1;855;0
WireConnection;97;1;898;0
WireConnection;293;0;292;0
WireConnection;293;1;97;0
WireConnection;872;0;974;0
WireConnection;872;1;857;0
WireConnection;959;0;957;0
WireConnection;959;1;955;2
WireConnection;959;2;955;3
WireConnection;916;0;914;0
WireConnection;916;1;915;0
WireConnection;917;0;916;0
WireConnection;960;0;959;0
WireConnection;960;1;293;0
WireConnection;960;2;958;0
WireConnection;886;0;872;0
WireConnection;921;1;919;0
WireConnection;921;0;920;0
WireConnection;975;0;886;0
WireConnection;918;0;917;0
WireConnection;918;1;960;0
WireConnection;967;0;965;0
WireConnection;980;0;976;0
WireConnection;980;1;979;0
WireConnection;923;0;918;0
WireConnection;979;0;978;4
WireConnection;979;1;977;0
WireConnection;969;1;980;0
WireConnection;969;3;967;0
WireConnection;922;0;97;4
WireConnection;922;1;921;0
WireConnection;897;0;975;0
WireConnection;971;0;969;0
WireConnection;971;1;970;0
WireConnection;0;0;923;0
WireConnection;0;10;922;0
WireConnection;0;11;897;0
ASEEND*/
//CHKSM=851094EE382E9D7E6BC26C80B874E53C9BB0609F