#version 330 core

// Interpolated values from the vertex shaders
in vec3 dist;

// Ouput data
layout(location = 0) out vec4 color;
layout(location = 1) out int id;

// Values that stay constant for the whole mesh.
uniform int levelObjectType;
uniform int levelObjectNumber;
uniform vec4 incolor;

void main() {
    // Get distance to nearest edge
    float edge_intensity = min(min(dist.x,dist.y),dist.z);

    if (edge_intensity > 0.01f) {
        discard;
    }

	color = vec4(incolor.rgb, 1.0f);
	id = (levelObjectType << 24) | levelObjectNumber;
}
