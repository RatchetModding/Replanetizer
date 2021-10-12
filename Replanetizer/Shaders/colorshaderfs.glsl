#version 330 core

// Interpolated values from the vertex shaders
//in vec2 UV;
in vec4 DiffuseColor;

// Ouput data
layout(location = 0) out vec4 color;
layout(location = 1) out int id;

// Values that stay constant for the whole mesh.
uniform sampler2D myTextureSampler;
uniform int levelObjectType;
uniform int levelObjectNumber;

void main() {
	color = vec4(DiffuseColor.rgb, 1.0f);
	id = (levelObjectType << 24) | levelObjectNumber;
}
