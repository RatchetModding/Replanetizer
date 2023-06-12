#version 330 core

// Interpolated values from the vertex shaders
in vec2 UV;

// Ouput data
layout(location = 0) out vec4 color;
layout(location = 1) out int id;

uniform sampler2D myTextureSampler;
uniform int levelObjectType;
uniform int levelObjectNumber;
uniform int selected;

void main() {
    vec4 textureColor = texture(myTextureSampler, UV);

    if (textureColor.w < 1.0f) discard;

    if (selected > 0) {
        textureColor.xz = vec2(1.0f, 1.0f);
    }

    color = textureColor;

    id = (levelObjectType << 24) | levelObjectNumber;
}
