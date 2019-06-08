#version 130

uniform sampler2D diffuseTexture;

uniform vec3 lightDirection;
uniform float ambientIntensity;
uniform float diffuseIntensity;
uniform mat4 mixColor;

in vec3 p_normal;
in vec2 p_texcoord;
in vec4 p_color;

out vec4 fragColor;

void main()
{
    // Get the colour from the texture
    vec4 c = p_color * texture(diffuseTexture, p_texcoord);

    // The final colour now is some ambient lighting plus the lambertian lighting
    fragColor = c * ambientIntensity + c * diffuseIntensity * max(0, dot(p_normal, -lightDirection));
    fragColor = mixColor * fragColor;
}
