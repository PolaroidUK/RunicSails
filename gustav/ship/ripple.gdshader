shader_type canvas_item;
uniform sampler2D SCREEN_TEXTURE: hint_screen_texture, filter_linear_mipmap;
uniform sampler2D noise : repeat_enable;

uniform float speed = 0.2;
uniform float strength = 0.1;


void fragment() {
	float noise_value = texture(noise, UV + TIME * speed).r;
    COLOR = texture(TEXTURE, SCREEN_UV - (strength / 2.0) + vec2(noise_value) * strength);
}


