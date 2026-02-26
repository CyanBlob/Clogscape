shader_type canvas_item;
render_mode unshaded, skip_vertex_transform;

//uniform sampler2D visibility_mask;
uniform sampler2D visibility_mask : repeat_disable, filter_linear;
uniform vec2 mask_origin_world = vec2(0.0, 0.0);
uniform vec2 grid_size_cells = vec2(128.0, 128.0);
uniform float cell_size_world = 32.0;
uniform vec4 fog_color : source_color = vec4(0.06, 0.08, 0.10, 1.0);
uniform float fog_opacity = 0.92;
uniform float noise_scale = 0.035;
uniform float noise_speed = 0.20;
uniform float edge_softness = 0.35;

varying vec2 v_world_pos;

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 345.45));
    p += dot(p, p + 34.345);
    return fract(p.x * p.y);
}

float value_noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    float a = hash21(i);
    float b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0));
    float d = hash21(i + vec2(1.0, 1.0));
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

float fbm(vec2 p) {
    float v = 0.0;
    float a = 0.5;
    for (int i = 0; i < 5; i++) {
        v += a * value_noise(p);
        p *= 2.01;
        a *= 0.5;
    }
    return v;
}

void vertex() {
    vec2 world = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
    v_world_pos = world - vec2(-37, -37);
    VERTEX = world;
}

float sample_visibility_max3x3(vec2 uv) {
    vec2 tex_size = grid_size_cells;
    vec2 px = 1.0 / tex_size;

    float m = 0.0;
    m = max(m, texture(visibility_mask, uv + px * vec2(-1.0, -1.0)).r);
    m = max(m, texture(visibility_mask, uv + px * vec2( 0.0, -1.0)).r);
    m = max(m, texture(visibility_mask, uv + px * vec2( 1.0, -1.0)).r);
    m = max(m, texture(visibility_mask, uv + px * vec2(-1.0,  0.0)).r);
    m = max(m, texture(visibility_mask, uv + px * vec2( 0.0,  0.0)).r);
    m = max(m, texture(visibility_mask, uv + px * vec2( 1.0,  0.0)).r);
    m = max(m, texture(visibility_mask, uv + px * vec2(-1.0,  1.0)).r);
    m = max(m, texture(visibility_mask, uv + px * vec2( 0.0,  1.0)).r);
    m = max(m, texture(visibility_mask, uv + px * vec2( 1.0,  1.0)).r);
    return m;
}

float sample_visibility_avg3x3(vec2 uv) {
    vec2 tex_size = grid_size_cells;
    vec2 px = 1.0 / tex_size;

    float s = 0.0;
    s += texture(visibility_mask, uv + px * vec2(-1.0, -1.0)).r;
    s += texture(visibility_mask, uv + px * vec2( 0.0, -1.0)).r;
    s += texture(visibility_mask, uv + px * vec2( 1.0, -1.0)).r;
    s += texture(visibility_mask, uv + px * vec2(-1.0,  0.0)).r;
    s += texture(visibility_mask, uv + px * vec2( 0.0,  0.0)).r;
    s += texture(visibility_mask, uv + px * vec2( 1.0,  0.0)).r;
    s += texture(visibility_mask, uv + px * vec2(-1.0,  1.0)).r;
    s += texture(visibility_mask, uv + px * vec2( 0.0,  1.0)).r;
    s += texture(visibility_mask, uv + px * vec2( 1.0,  1.0)).r;
    return s / 9.0;
}

void fragment() {
    // Treat mask_origin_world as the CENTER of the grid in world space.
    vec2 rel = v_world_pos - mask_origin_world;

    vec2 world_size = grid_size_cells * cell_size_world;
    vec2 uv = (rel / world_size) + vec2(0.5);

    float inside =
        step(0.0, uv.x) * step(0.0, uv.y) *
        step(uv.x, 1.0) * step(uv.y, 1.0);

    uv = clamp(uv, vec2(0.0), vec2(1.0));

    // Single sample; your CPU brush already provides softness.
    float vis = texture(visibility_mask, uv).r * inside;

    float edge = smoothstep(0.0, 1.0, vis);

    float t = TIME * noise_speed;
    vec2 p = v_world_pos * noise_scale;

    float n1 = fbm(p + vec2(t, -t * 0.7));
    float n2 = fbm(p * 1.7 + vec2(-t * 0.4, t));
    float fog_variation = clamp(0.55 + 0.55 * (n1 * 0.7 + n2 * 0.3), 0.0, 1.0);

    float fog_a = fog_opacity * (1.0 - edge) * mix(0.85, 1.05, fog_variation);
    COLOR = vec4(fog_color.rgb, clamp(fog_a, 0.0, 1.0));
}