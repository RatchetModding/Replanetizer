#version 330 core

// Interpolated values from the vertex shaders
in vec2 UV;
in vec4 lightColor;

// Ouput data
layout(location = 0) out vec4 color;
layout(location = 1) out int id;

// Values that stay constant for the whole mesh.
uniform sampler2D myTextureSampler;
uniform float texAvailable;

void main() {
	vec4 textureColor = texture(myTextureSampler, UV);

	color.xyz = 1.5f * mix(lightColor.xyz, textureColor.xyz, texAvailable);
	color.w = textureColor.w * lightColor.w * 2.0f;

	id = (999 << 24);
}
