#version 330 core

// Interpolated values from the vertex shaders
in vec2 UV;
in vec4 DiffuseColor;

// Ouput data
out vec4 color;

// Values that stay constant for the whole mesh.
uniform sampler2D myTextureSampler;
uniform int useFog;
uniform vec4 fogColor;
uniform float fogDistance;

float get_depth() {
    float ndcDepth = (2.0 * gl_FragCoord.z - gl_DepthRange.near - gl_DepthRange.far) /
        (gl_DepthRange.far - gl_DepthRange.near);
    float clipDepth = ndcDepth / gl_FragCoord.w;
    return (clipDepth * 0.5f) + 0.5f;
}

void main(){
    //color of the texture at the specified UV
    vec4 textureColor = texture(myTextureSampler, UV);

    if (useFog == 1) {
        float depth = clamp(get_depth() / fogDistance, 0.0f, 1.0f);

        color = depth * fogColor + (1.0f - depth) * textureColor;
        color.w = textureColor.w;
    }
    else {
        color = textureColor;
    }

}