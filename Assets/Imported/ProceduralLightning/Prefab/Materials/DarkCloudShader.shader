Shader "Custom/CloudShader"
{
    Properties
	{
		_MainTex ("Color (RGB) Alpha (A)", 2D) = "gray" {}
		_TintColor ("Tint Color (RGB)", Color) = (1, 1, 1, 1)
		_PointSpotLightMultiplier ("Point/Spot Light Multiplier", Range (0, 10)) = 2
		_DirectionalLightMultiplier ("Directional Light Multiplier", Range (0, 10)) = 1
		_EmissiveColor ("Emissive Color (RGB)", Color) = (0.1, 0.1, 0.1, 1)
		_AmbientLightMultiplier ("Ambient light multiplier", Range (0, 2)) = 0.1
    }
    SubShader
	{
        Tags 
		{ 
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
			"RenderPipeline" = "UniversalPipeline"
		}
		Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
 
        Pass
		{
			Name "ForwardLit"
			Tags { "LightMode" = "UniversalForward" }
			
			Cull Off
			ZWrite Off
			ColorMask RGBA
				 
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				half4 _TintColor;
				float _DirectionalLightMultiplier;
				float _PointSpotLightMultiplier;
				half3 _EmissiveColor;
				half _AmbientLightMultiplier;
			CBUFFER_END

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			struct Attributes
			{
				float4 positionOS : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float3 normalOS : NORMAL;
			};
 
            struct Varyings
            {
                float2 uv : TEXCOORD0;
				float3 positionWS : TEXCOORD1;
				float3 normalWS : TEXCOORD2;
                half4 color : COLOR0;
                float4 positionCS : SV_POSITION;
				float fogFactor : TEXCOORD3;
            };
			 
            Varyings vert(Attributes input)
            {
                Varyings output;
				
				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
				VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
				
                output.positionCS = vertexInput.positionClipSpace;
				output.positionWS = vertexInput.positionWS;
				output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
				output.fogFactor = ComputeFogFactor(output.positionCS.z);
				
				// Calculate lighting
				Light mainLight = GetMainLight();
				half3 lightColor = _EmissiveColor + (half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) * _AmbientLightMultiplier);
				
				// Main directional light
				half3 mainLightDiffuse = saturate(dot(normalInput.normalWS, mainLight.direction));
				lightColor += mainLight.color * mainLightDiffuse * _DirectionalLightMultiplier;
				
				// Additional lights
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					uint pixelLightCount = GetAdditionalLightsCount();
					for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
					{
						Light light = GetAdditionalLight(lightIndex, vertexInput.positionWS);
						half3 attenuatedLightColor = light.color * light.distanceAttenuation;
						half3 lightDiffuse = saturate(dot(normalInput.normalWS, light.direction));
						lightColor += attenuatedLightColor * lightDiffuse * _PointSpotLightMultiplier;
					}
				#endif
				
                output.color = half4(lightColor, 1) * input.color * _TintColor;
                return output; 
            }
  
            half4 frag (Varyings input) : SV_Target
			{
                // Sample texture
				half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 finalColor = texColor * input.color;
				
				// Apply additional per-pixel lights if enabled
				#ifdef _ADDITIONAL_LIGHTS
					uint pixelLightCount = GetAdditionalLightsCount();
					for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
					{
						Light light = GetAdditionalLight(lightIndex, input.positionWS);
						half3 attenuatedLightColor = light.color * light.distanceAttenuation;
						half3 lightDiffuse = saturate(dot(input.normalWS, light.direction));
						finalColor.rgb += texColor.rgb * attenuatedLightColor * lightDiffuse * _PointSpotLightMultiplier;
					}
				#endif
				
				// Apply fog
				finalColor.rgb = MixFog(finalColor.rgb, input.fogFactor);
				
                return finalColor;
            }
            ENDHLSL
        }
		
		// Shadow caster pass for URP
		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			
			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull Off
			
			HLSLPROGRAM
			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			
			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
			};
			
			struct Varyings
			{
				float4 positionCS : SV_POSITION;
			};
			
			float3 _LightDirection;
			
			Varyings ShadowPassVertex(Attributes input)
			{
				Varyings output;
				float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
				float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
				output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
				return output;
			}
			
			half4 ShadowPassFragment(Varyings input) : SV_TARGET
			{
				return 0;
			}
			ENDHLSL
		}
		
		// Depth only pass for URP
		Pass
		{
			Name "DepthOnly"
			Tags { "LightMode" = "DepthOnly" }
			
			ZWrite On
			ColorMask 0
			Cull Off
			
			HLSLPROGRAM
			#pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			
			struct Attributes
			{
				float4 positionOS : POSITION;
			};
			
			struct Varyings
			{
				float4 positionCS : SV_POSITION;
			};
			
			Varyings DepthOnlyVertex(Attributes input)
			{
				Varyings output;
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				return output;
			}
			
			half4 DepthOnlyFragment(Varyings input) : SV_TARGET
			{
				return 0;
			}
			ENDHLSL
		}
    }
 
    Fallback "Universal Render Pipeline/Particles/Unlit"
}