#version 330 core

// Ouput data
layout(location = 0) out vec4 color;
layout(location = 1) out int id;

// Values that stay constant for the whole mesh.
uniform int levelObjectType;
uniform int levelObjectNumber;
uniform vec4 incolor;

void main() {
	color = vec4(incolor.rgb, 1.0f);
	id = (levelObjectType << 24) | levelObjectNumber;
}
