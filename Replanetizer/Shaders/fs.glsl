#version 330 core

// Interpolated values from the vertex shaders
in vec2 UV;
in vec3 lightColor;
in float fogBlend;

// Ouput data
layout(location = 0) out vec4 color;
layout(location = 1) out int id;

// Values that stay constant for the whole mesh.
uniform sampler2D myTextureSampler;
uniform int levelObjectType;
uniform int levelObjectNumber;
uniform vec4 fogColor;
uniform mat4 dissolvePattern;
uniform float objectBlendDistance;
uniform int useTransparency;

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
	vec4 textureColor = texture(myTextureSampler, UV);

    float alpha = 1.0f;

    // If the object is further than renderDistance we blend out over distance using a dissolve pattern
    if ((levelObjectType == 2 || levelObjectType == 4) && objectBlendDistance > 0.0f) {
        alpha *= 1.0f - objectBlendDistance;
    }

    if (useTransparency == 1) {
        alpha *= textureColor.w;
    }

    if (alpha < 1.0f) {
        vec2 pixel = vec2(gl_FragCoord.x, gl_FragCoord.y);
        ivec2 patternPos = ivec2(int(mod(pixel.x + levelObjectNumber,4.0f)), int(mod(pixel.y,4.0f)));
        if (dissolvePattern[patternPos.x][patternPos.y] > alpha) discard;
    }

	color.xyz = 1.5f * textureColor.xyz * lightColor * 2.0f;
	color.w = textureColor.w;

	color.xyz = (fogColor.xyz - color.xyz) * fogBlend + color.xyz;

	id = (levelObjectType << 24) | levelObjectNumber;
}
