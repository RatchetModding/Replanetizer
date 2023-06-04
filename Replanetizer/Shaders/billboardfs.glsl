#version 330 core

// Interpolated values from the vertex shaders

// Ouput data
layout(location = 0) out vec4 color;
layout(location = 1) out int id;

uniform int levelObjectType;
uniform int levelObjectNumber;

void main() {
    color = vec4(1.0f, 0.0f, 1.0f, 1.0f);
    id = (levelObjectType << 24) | levelObjectNumber;
}
