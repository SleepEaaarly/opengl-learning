#version 330 core

layout (location = 0) in vec3 aPos;

out vec3 stPoint;
out vec2 texCoords;
out vec3 sphereCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec3 sCoord;

void main() {
    vec4 stPoint_vec4 = view * model * vec4(aPos, 1.0f);
    stPoint = stPoint_vec4.xyz / stPoint_vec4.w;
    gl_Position = projection * stPoint_vec4;
    texCoords = gl_Position.xy / gl_Position.w;
    texCoords = (texCoords + 1.0) / 2.0;
    sphereCoord = vec3(view * model * vec4(sCoord, 1.0f));
}
