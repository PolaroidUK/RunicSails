shader_type canvas_item;
uniform sampler2D SCREEN_TEXTURE: hint_screen_texture, filter_linear_mipmap;
uniform sampler2D noise : repeat_enable;

uniform vec4 color : source_color = vec4(0.0, 0.4, 0.6, 0.5);

uniform float speed = 0.02;
uniform float strength = 0.1;


void fragment() {
	float noise_value = texture(noise, UV + TIME * speed).r;
    vec3 txt = texture(SCREEN_TEXTURE, SCREEN_UV - (strength / 2.0) + vec2(noise_value) * strength).rgb;
	
	vec3 mixed = mix(txt, color.rgb, color.a).rbg;
	COLOR = vec4(mixed, color.a);
}


