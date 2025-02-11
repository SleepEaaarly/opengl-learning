#version 330 core

#define EPS 0.0

in VS_OUT {
    vec3 fragPos;
    vec3 normal;
    vec2 texCoords;
    vec4 fragPosLightSpace;
} fs_in;

uniform sampler2D diffuseTexture;
uniform sampler2D shadowMap;            // R(ed) channel store value 

uniform vec3 lightPos;
uniform vec3 viewPos;

const float gamma = 2.2;

float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 light_vec) {
    // covert value into [0, 1]
    vec3 coords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    coords = (coords + 1.0f) / 2.0f;

    // area beyond the far plane are set to no shadow
    if (coords.z > 1.0) {
        return 0.0;
    }

    float shadow = 0.0;
    vec2 texel_size = 1.0 / textureSize(shadowMap, 0);      // size of texture mipmap of level 0 
    float depth = coords.z;
    float bias = max(0.05 * (1 - dot(normal, light_vec)), 0.005);
    
    for (int x = -1; x <= 1; x++) {
        for (int y = -1; y <= 1; y++) {
            float buf_depth = texture(shadowMap, coords.xy + vec2(x, y) * texel_size).r;
            shadow += depth - buf_depth > bias ? 1.0 : 0.0;
        }
    }
    shadow /= 9.0;

    // float buf_depth = texture(shadowMap, coords.xy).r;
    // if (depth - buf_depth > bias) {
    //     return 1.0;
    // }

    return shadow;
}

void main() {
    vec3 color = pow(texture(diffuseTexture, fs_in.texCoords).rgb, vec3(gamma));
    vec3 normal = normalize(fs_in.normal);
    float amb_intensity = 0.2f, diff_intensity = 0.2f, spec_intensity = 0.4f;

    // ambient light
    vec3 ambient = amb_intensity * color;

    // diffuse light
    float diff;
    vec3 light_vec = normalize(lightPos - fs_in.fragPos);
    diff = max(dot(light_vec, normal), 0.0);
    vec3 diffuse = diff * diff_intensity * color;

    // spec light
    float spec;
    vec3 reflect_vec = reflect(-light_vec, normal);
    vec3 view_vec = normalize(viewPos - fs_in.fragPos);
    vec3 half_vec = normalize(light_vec + view_vec);
    spec = pow(max(dot(half_vec, normal), 0), 32.0f);
    vec3 specular = spec * spec_intensity * color;

    // shadow  1.0 repre shadow  0.0 repre light
    float shadow = ShadowCalculation(fs_in.fragPosLightSpace, normal, light_vec);

    // final color
    vec3 final_color = ambient + (1 - shadow) * (diffuse + specular);
    final_color = pow(final_color, vec3(1 / gamma));

    // output
    gl_FragColor = vec4(final_color, 1.0f);
}
