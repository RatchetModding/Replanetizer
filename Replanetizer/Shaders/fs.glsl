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
 * - In RaC 3, ties do not use alpha cutoff. Replanetizer uses the RaC 1 and 2 behaviour.
 */
void main() {
	//color of the texture at the specified UV
	vec4 textureColor = texture(myTextureSampler, UV);

    // If the object is further than renderDistance we blend out over distance using a dissolve pattern
    if ((levelObjectType == 2 || levelObjectType == 4) && objectBlendDistance > 0.0f) {
        vec2 pixel = vec2(gl_FragCoord.x, gl_FragCoord.y);
        float alpha = objectBlendDistance;
        ivec2 patternPos = ivec2(int(mod(pixel.x,4.0f)),int(mod(pixel.y,4.0f)));
        if (dissolvePattern[patternPos.x][patternPos.y] < alpha) discard;
    }

	if (textureColor.w < 0.1f && levelObjectType >= 2) discard;

	color.xyz = textureColor.xyz * lightColor;
	color.w = textureColor.w;

	color.xyz = mix(color.xyz, fogColor.xyz, fogBlend);

	id = (levelObjectType << 24) | levelObjectNumber;
}
