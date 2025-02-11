#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;

out vec2 texCoords;

out VS_OUT {
    vec3 fragPos;
    vec3 normal;
    vec2 texCoords;
    vec4 fragPosLightSpace;     // clip coords ([-w, w]^3, w)
} vs_out;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;
uniform mat4 lightSpaceMatrix;

void main() {
    gl_Position = projection * view * model * vec4(aPos, 1.0f);
    vs_out.fragPos = vec3(model * vec4(aPos, 1.0f));
    vs_out.normal = transpose(inverse(mat3(model))) * aNormal;
    vs_out.texCoords = aTexCoords;
    vs_out.fragPosLightSpace = lightSpaceMatrix * vec4(aPos, 1.0f); // clip coords
}
