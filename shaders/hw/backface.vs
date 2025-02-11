#version 330 core

layout (location = 0) in vec3 aPos;
out vec3 endPoint;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main() {
    vec4 endPoint_vec4 = view * model * vec4(aPos, 1.0f);
    endPoint = endPoint_vec4.xyz / endPoint_vec4.w;
    gl_Position = projection * endPoint_vec4;
}