#version 330 core

in vec3 GouColor;

out vec4 FragColor;

void main() {
    FragColor = vec4(GouColor, 1.0);
}