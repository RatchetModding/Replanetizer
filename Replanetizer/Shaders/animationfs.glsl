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
uniform int levelObjectNumber;
uniform vec4 fogColor;
uniform mat4 dissolvePattern;
uniform float objectBlendDistance;
uniform int useTransparency;

void main() {
	//color of the texture at the specified UV
	vec4 textureColor = texture(myTextureSampler, UV);

    float alpha = 1.0f;

    // If the object is further than renderDistance we blend out over distance using a dissolve pattern
    if (objectBlendDistance > 0.0f) {
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

	id = (4 << 24) | levelObjectNumber;
}
