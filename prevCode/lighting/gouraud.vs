#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec3 lightColor;
uniform vec3 lightPos;
uniform vec3 objectColor;

out vec3 GouColor;

void main()
{
    gl_Position = projection * view * model * vec4(aPos, 1.0);
    vec3 fragPos = vec3(view * model * vec4(aPos, 1.0));
    vec3 Normal = mat3(transpose(inverse(view * model))) * aNormal;

    float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * lightColor;

    float diffuseStrength = 1.0;
    vec3 norm = normalize(Normal);
    vec3 lPos = vec3(view * vec4(lightPos, 1.0));
    vec3 lightDir = normalize(fragPos - lightPos);
    vec3 diff = diffuseStrength * max(dot(norm, -lightDir), 0) * lightColor;

    float specularStrength = 1.0;
    vec3 viewDir = normalize(-fragPos);
    vec3 reflectDir = reflect(lightDir, norm);
    vec3 spec = specularStrength * pow(max(dot(reflectDir, viewDir), 0), 32) * lightColor;

    GouColor = ambient * objectColor + diff * objectColor + spec * objectColor;
}