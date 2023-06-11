#version 330 core

// Interpolated values from the vertex shaders

// Ouput data
layout(location = 0) out vec4 color;
layout(location = 1) out int id;

uniform int levelObjectType;
uniform int levelObjectNumber;
uniform int selected;

void main() {
    if (selected > 0) {
        color = vec4(1.0f, 1.0f, 1.0f, 1.0f);
    } else {
        color = vec4(1.0f, 0.0f, 1.0f, 1.0f);
    }

    id = (levelObjectType << 24) | levelObjectNumber;
}
