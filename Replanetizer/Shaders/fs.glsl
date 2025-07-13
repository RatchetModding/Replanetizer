#version 330 core

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

#define BLUE_NOISE_TEXTURE_SIZE (128.0f)
#define ONE_OVER_GOLDEN_RATIO (2654435769u) /* 0.61803398875f */

bool computeDitheringDiscard(float alpha)
{
    if (alpha >= 1.0f)
        return false;

    vec2 pixel = vec2(gl_FragCoord.x, gl_FragCoord.y);
    vec2 blueNoiseIndex = pixel / BLUE_NOISE_TEXTURE_SIZE;
    float alphaThreshold = texture(blueNoiseTexture, blueNoiseIndex).x;

    float objectDitherOffset = uint(levelObjectNumber) * ONE_OVER_GOLDEN_RATIO;
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
	vec4 textureColor = texture(mainTexture, UV);

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
