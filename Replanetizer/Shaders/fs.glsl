#version 330 core

// Interpolated values from the vertex shaders
in vec2 UV;
in vec4 DiffuseColor;

// Ouput data
layout (location = 0) out vec4 color;
layout (location = 1) out int id;

// Values that stay constant for the whole mesh.
uniform sampler2D myTextureSampler;
uniform int useFog;
uniform vec4 fogColor;
uniform float fogNearDistance;
uniform float fogFarDistance;
uniform float fogNearIntensity;
uniform float fogFarIntensity;
uniform int levelObjectType;
uniform int levelObjectNumber;
uniform vec4 staticColor;

float get_depth() {
    float ndcDepth = (2.0 * gl_FragCoord.z - gl_DepthRange.near - gl_DepthRange.far) /
        (gl_DepthRange.far - gl_DepthRange.near);
    float clipDepth = ndcDepth / gl_FragCoord.w;
    return (clipDepth * 0.5f) + 0.5f;
}

/*
 * Degree 2 Taylor Expansion of exp function
 * Closest to what is used in the game that I could figure out
 */
float quick_exp(float x) {
    float y = x * x;
    float z = y * x;
    return 1.0f + x + 0.5f * y + 0.1666f * z;
}

/*
 * We use one shader for all shading types.
 * Terrain: (TODO) RGBAs
 * Shrubs: StaticColor + DiffuseColor
 * Ties: DiffuseColor + (TODO) Colorbytes
 * Mobies: StaticColor + DiffuseColor
 *
 * Weights of all these colors and the fog are probably not exact atm.
 */
void main(){
    //color of the texture at the specified UV
    vec4 textureColor = texture(myTextureSampler, UV);

    vec4 ambient = vec4(0.5f);

    if (levelObjectType == 2 || levelObjectType == 4) {
        ambient = staticColor;
    }

    vec4 diffuse = vec4(0.0f);

    if (levelObjectType >= 2 && levelObjectType <= 4) {
        diffuse = DiffuseColor;
    }

    color.xyz = textureColor.xyz * (diffuse.xyz + ambient.xyz);
    color.w = textureColor.w;

    if (useFog == 1) {
        float depth = clamp((fogFarDistance - quick_exp(get_depth())) / (fogFarDistance - fogNearDistance), 0.0f, 1.0f);

        float intensity = clamp(depth * fogNearIntensity + (1.0f - depth) * fogFarIntensity, 0.0f, 1.0f);

        color.xyz = intensity * color.xyz + (1.0f - intensity) * fogColor.xyz;
    }

    id = (levelObjectType << 24) | levelObjectNumber;
}