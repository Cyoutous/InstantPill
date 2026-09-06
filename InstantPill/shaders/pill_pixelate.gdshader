shader_type canvas_item;
render_mode unshaded;

// This is sampled from everything already rendered below the overlay, including combat UI.
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

// Measured in physical screen pixels. A value of 1 is visually lossless.
uniform float pixel_size = 1.0;
uniform float intensity : hint_range(0.0, 1.0) = 0.0;

void fragment() {
	vec4 original = textureLod(screen_texture, SCREEN_UV, 0.0);
	vec2 viewport_size = 1.0 / SCREEN_PIXEL_SIZE;
	vec2 grid_size = max(floor(viewport_size / max(pixel_size, 1.0)), vec2(1.0));
	vec2 pixelated_uv = (floor(SCREEN_UV * grid_size) + vec2(0.5)) / grid_size;
	vec4 pixelated = textureLod(screen_texture, pixelated_uv, 0.0);
	COLOR = mix(original, pixelated, intensity);
}
