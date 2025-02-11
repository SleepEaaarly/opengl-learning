#version 330 core
out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec2 TexCoords;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
} fs_in;

uniform sampler2D diffuseMap;
uniform sampler2D normalMap;
uniform sampler2D depthMap;

uniform bool parallax;
uniform float height_scale;

vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir) { 
    float minLayer = 10, maxLayer = 20;

    // more angle, less dot-value, more layer
    float layer = mix(maxLayer, minLayer, max(0.0, dot(vec3(0.0, 0.0, 1.0), viewDir)));
    vec2 deltaCoords = viewDir.xy / viewDir.z * height_scale / layer;
    
    float currentDepth = 0.0;
    float deltaDepth = 1.0 / layer;

    float mapDepth = texture(depthMap, texCoords).r;
    float curTexCoords = texCoords;

    while(currentDepth < mapDepth) {
        curTexCoords -= deltaCoords;
        currentDepth += deltaDepth;
        mapDepth = texture(depthMap, curTexCoords).r;
    }

    float afterLayerDepth = currentDepth;
    vec2 afterTexCoords = curTexCoords;
    float afterMapDepth = mapDepth;
    float afterDeltaDepth = afterLayerDepth - afterMapDepth;

    float beforeLayerDepth = currentDepth - deltaDepth;
    vec2 beforeTexCoords = curTexCoords + deltaCoords;
    float beforeMapDepth = texture(depthMap, beforeTexCoords).r;
    float beforeDeltaDepth = beforeMapDepth - beforeLayerDepth;

    float weight = afterDeltaDepth / (afterDeltaDepth + beforeDeltaDepth);

    vec2 finalTexCoords = (1 - weight) * afterTexCoords + weight * beforeTexCoords;
}

void main()
{           
    // Offset texture coordinates with Parallax Mapping
    vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);
    vec2 texCoords = fs_in.TexCoords;
    if(parallax)
        texCoords = ParallaxMapping(fs_in.TexCoords,  viewDir);
        
    // discards a fragment when sampling outside default texture region (fixes border artifacts)
    if(texCoords.x > 1.0 || texCoords.y > 1.0 || texCoords.x < 0.0 || texCoords.y < 0.0)
        discard;

    // Obtain normal from normal map
    vec3 normal = texture(normalMap, texCoords).rgb;
    normal = normalize(normal * 2.0 - 1.0);   
   
    // Get diffuse color
    vec3 color = texture(diffuseMap, texCoords).rgb;
    // Ambient
    vec3 ambient = 0.1 * color;
    // Diffuse
    vec3 lightDir = normalize(fs_in.TangentLightPos - fs_in.TangentFragPos);
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = diff * color;
    // Specular    
    vec3 reflectDir = reflect(-lightDir, normal);
    vec3 halfwayDir = normalize(lightDir + viewDir);  
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);

    vec3 specular = vec3(0.2) * spec;
    FragColor = vec4(ambient + diffuse + specular, 1.0f);
}