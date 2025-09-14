﻿#version 330 core

// MacOS seems to support this: https://www.geeks3d.com/20130611/apple-adds-opengl-4-support-in-os-x-10-9-mavericks/
#extension GL_ARB_sample_shading : require

// Interpolated values from the vertex shaders
in vec2 UV;
in vec3 lightColor;
in float fogBlend;

// Ouput data
layout(location = 0) out vec4 color;
layout(location = 1) out int id;

// Values that stay constant for the whole mesh.
uniform sampler2D mainTexture;
uniform sampler2D blueNoiseTexture;
uniform int levelObjectType;
uniform int levelObjectNumber;
uniform vec4 fogColor;
uniform float objectBlendDistance;
uniform int useTransparency;
uniform int useTexture;

#define BLUE_NOISE_TEXTURE_SIZE (128.0f)
#define ONE_OVER_GOLDEN_RATIO (2654435769u) /* 0.61803398875f */

bool computeDitheringDiscard(float alpha)
{
    if (alpha >= 1.0f)
        return false;

    vec2 pixel = vec2(gl_FragCoord.x, gl_FragCoord.y);
    vec2 blueNoiseIndex = pixel / BLUE_NOISE_TEXTURE_SIZE;
    float alphaThreshold = texture(blueNoiseTexture, blueNoiseIndex).x;

    uint offsetIndex = uint(1 + gl_SampleID + levelObjectNumber * gl_NumSamples);

    float objectDitherOffset = offsetIndex * ONE_OVER_GOLDEN_RATIO;
    objectDitherOffset = objectDitherOffset / 0xFFFFFFFFu;

    alphaThreshold = mod(alphaThreshold + objectDitherOffset, 1.0f);

    return (alphaThreshold > alpha);
}

/*
 * We use one shader for all shading types.
 * Terrain (1): RGBAs + DiffuseColor
 * Shrubs (2): StaticColor + DiffuseColor
 * Ties (3): Colorbytes + DiffuseColor
 * Mobies (4): StaticColor + DiffuseColor
 *
 * Some Notes about the rendering in the game:
 * - Ratchets helmet treats light in the opposite direction, i.e.
 *   if a directional light points up, it will be treated as if it was pointing down.
 * - Ratchets helmet and the ship seem to be the only objects that
 *   have specular highlights.
 * - Fog seems to be twice as bright for ties.
 * - Terrain does not use alpha cutoff.
 */
void main() {
	//color of the texture at the specified UV
    vec4 textureColor;
    if (useTexture == 1) {
        textureColor = texture(mainTexture, UV);
    }
    else {
        textureColor = vec4(1.0f, 1.0f, 1.0f, 0.5f);
    }

    float alpha = 1.0f;

    // If the object is further than renderDistance we blend out over distance using a dissolve pattern
    if ((levelObjectType == 2 || levelObjectType == 4) && objectBlendDistance > 0.0f) {
        alpha *= 1.0f - objectBlendDistance;
    }

    if (useTransparency == 1) {
        alpha *= textureColor.w;
    }

    if (computeDitheringDiscard(alpha)) discard;

	color.xyz = 1.5f * textureColor.xyz * lightColor * 2.0f;
	color.w = textureColor.w;

	color.xyz = (fogColor.xyz - color.xyz) * fogBlend + color.xyz;

	id = (levelObjectType << 24) | levelObjectNumber;
}
