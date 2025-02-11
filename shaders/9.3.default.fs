#version 330 core

in vec2 texCoords;

uniform sampler2D diffuse_texture;

void main() {
    gl_FragColor = texture(diffuse_texture, texCoords);
}

