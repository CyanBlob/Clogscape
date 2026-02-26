shader_type canvas_item;

uniform sampler2D mask_tex : repeat_disable, filter_nearest;
uniform vec2 grid_size;
uniform vec2 origin_world;
uniform vec2 cell_size;
uniform float fog_alpha = 1.0;

uniform vec2 cam_top_left_world;
uniform vec2 viewport_size;
uniform vec2 cam_zoom = vec2(1.0);

uniform vec3 fog_rgb = vec3(0.0, 0.0, 0.0);

void fragment() {
    // Screen -> world
    vec2 world = cam_top_left_world + (SCREEN_UV * viewport_size) / cam_zoom;

    // World -> grid cell
    vec2 cell = floor((world - origin_world) / cell_size);

    float a = fog_alpha; // default: full fog

    bool inside =
        cell.x >= 0.0 && cell.y >= 0.0 &&
        cell.x < grid_size.x && cell.y < grid_size.y;

    if (inside) {
        vec2 uv = (cell + vec2(0.5)) / grid_size;
        float revealed = texture(mask_tex, uv).r; // 0 hidden, 1 revealed
        a = fog_alpha * (1.0 - revealed);
    }

    COLOR = vec4(fog_rgb, a);
}