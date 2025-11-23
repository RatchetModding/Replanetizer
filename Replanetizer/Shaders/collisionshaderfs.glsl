#version 330 core

// Interpolated values from the vertex shaders
in vec4 diffuseColors;
in float fogBlend;

// Ouput data
layout(location = 0) out vec4 color;

uniform vec4 fogColor;

void main() {
    color = diffuseColors / 255.0f;
    color.xyz = (fogColor.xyz - color.xyz) * fogBlend + color.xyz;
}
