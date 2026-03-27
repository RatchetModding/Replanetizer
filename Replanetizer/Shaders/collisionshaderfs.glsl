#version 330 core

// Interpolated values from the vertex shaders
flat in vec4 diffuseColors;
in float fogBlend;
in vec3 v_worldPos;
in vec3 v_cameraPos;

// Ouput data
layout(location = 0) out vec4 color;
uniform vec4 fogColor;

void main() {
    vec4 normalizedColor = diffuseColors / 255.0;
    vec3 N = normalize(cross(dFdy(v_worldPos), dFdx(v_worldPos)));
    vec3 lightDir = normalize(v_cameraPos - v_worldPos);

    float ambient = 0.4; 
    float diffuse = max(abs(dot(N, lightDir)), 0.0);
    
    float brightness = ambient + (diffuse * 0.5);

    // Gray on the back sides, otherwise use material color
    if(!gl_FrontFacing) {
        color = vec4(0.5 * brightness, 0.5 * brightness, 0.5 * brightness, 0.7);
    }
    else {
        color = vec4(normalizedColor.rgb * brightness, 1.0);
    }

    color = vec4(clamp(color.rgb, 0.0, 1.0), 1.0);
    color.xyz = (fogColor.xyz - color.xyz) * fogBlend + color.xyz;
}
