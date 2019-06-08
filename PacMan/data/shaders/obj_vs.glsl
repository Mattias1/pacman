#version 130

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

in vec3 v_position;
in vec3 v_normal;
in vec2 v_texcoord;
in vec4 v_color;

out vec3 p_normal;
out vec2 p_texcoord;
out vec4 p_color;

void main()
{
	gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(v_position, 1.0);
    
    p_normal = normalize(mat3(modelMatrix) * v_normal);
    p_texcoord = v_texcoord;
    p_color = v_color;
}