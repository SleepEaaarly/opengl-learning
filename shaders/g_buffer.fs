#version 330 core

layout (location = 0) out vec3 gPosition;
layout (location = 1) out vec3 gNormal;
layout (location = 2) out vec4 gAlbedoSpec;

in VS_OUT {
    vec3 position;
    vec3 normal;
    vec2 texCoords;
} fs_in;

uniform sampler2D texture_diffuse1;
uniform sampler2D texture_specular1;

void main() {
    gPosition = fs_in.position;
    gNormal = normalize(fs_in.normal);
    gAlbedoSpec.rgb = texture(texture_diffuse1, fs_in.texCoords).rgb;
    gAlbedoSpec.a = texture(texture_specular1, fs_in.texCoords).r;
}
