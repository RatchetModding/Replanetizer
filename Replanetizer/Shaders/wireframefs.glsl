#version 330 core

// Interpolated values from the vertex shaders
noperspective in vec3 dist;

// Ouput data
layout(location = 0) out vec4 color;
layout(location = 1) out int id;

// Values that stay constant for the whole mesh.
uniform sampler2D myTextureSampler;
uniform int levelObjectType;
uniform int levelObjectNumber;
uniform vec4 incolor;

void main() {
    // Get distance to nearest edge
    float near_dist = min(min(dist[0], dist[1]), dist[2]);
    float edge_intensity = exp2(-1.0f * near_dist * near_dist);

    if (edge_intensity < 0.1f) {
        discard;
    }

	color =  vec4(incolor.rgb, 1.0f);
	id = (levelObjectType << 24) | levelObjectNumber;
}
