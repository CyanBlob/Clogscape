using Godot;

public partial class FogRect : Sprite2D
{
    [Export] public int GridWidthCells = 128;
    [Export] public int GridHeightCells = 128;
    [Export] public float CellSizeWorld = 64;

    // World-space position of cell (0,0). Typically your map origin (top-left) or (0,0).
    [Export] public Vector2 MaskOriginWorld = Vector2.Zero;

    // Shader parameter names (must match shader uniforms)
    [Export] public string ParamVisibilityMask = "visibility_mask";
    [Export] public string ParamMaskOriginWorld = "mask_origin_world";
    [Export] public string ParamGridSizeCells = "grid_size_cells";
    [Export] public string ParamCellSizeWorld = "cell_size_world";

    private Image _maskImage = default!;
    private ImageTexture _maskTexture = default!;
    private ShaderMaterial _mat = default!;
    private bool _dirty;

    public override void _Ready()
    {
        _mat = Material as ShaderMaterial;
        if (_mat == null)
        {
            GD.PushError("FogOfWar2D: Material must be a ShaderMaterial.");
            SetProcess(false);
            return;
        }

        // Create R8 mask image: 0 = fog, 255 = visible
        _maskImage = Image.CreateEmpty(GridWidthCells, GridHeightCells, false, Image.Format.R8);
        _maskImage.Fill(new Color(0, 0, 0));

        _mat.SetShaderParameter("visibility_mask", _maskTexture);

        _maskTexture = ImageTexture.CreateFromImage(_maskImage);
        _mat.SetShaderParameter("visibility_mask", _maskTexture);

        // Push initial shader params
        _mat.SetShaderParameter(ParamVisibilityMask, _maskTexture);
        _mat.SetShaderParameter(ParamMaskOriginWorld, MaskOriginWorld);
        _mat.SetShaderParameter(ParamGridSizeCells, new Vector2(GridWidthCells, GridHeightCells));
        _mat.SetShaderParameter(ParamCellSizeWorld, CellSizeWorld);

        // If you want crisp cells, keep nearest filtering on the Sprite2D.
        // If you want softer blending, try Linear and increase Grid resolution.
        TextureFilter = TextureFilterEnum.Nearest;

        RevealCircle(new Vector2(0, 0), .5f, 1f);

        _dirty = true;
    }

    public override void _Process(double delta)
    {
        if (!_dirty) return;

        // Update the GPU texture with the modified Image data.
        // Godot recommends update() for frequent updates.  [oai_citation:2‡Godot Engine documentation](https://docs.godotengine.org/en/stable/classes/class_imagetexture.html)
        _maskTexture.Update(_maskImage);
        _dirty = false;
    }

    public void SetVisibleCell(int cellX, int cellY, bool visible)
    {
        //if (cellX < -GridWidthCells / 2 || cellY < -GridWidthCells / 2 || cellX >= GridWidthCells / 2 || cellY >= GridHeightCells / 2)
        //return;

        // For R8, only the red channel matters.
        _maskImage.SetPixel(cellX, cellY, visible ? new Color(1, 0, 0) : new Color(0, 0, 0));
        _dirty = true;
    }

    public void SetVisibleWorldPos(Vector2 worldPos, bool visible)
    {
        int sx = Mathf.FloorToInt(worldPos.X / CellSizeWorld);
        int sy = Mathf.FloorToInt(worldPos.Y / CellSizeWorld);

        int ix = sx + (GridWidthCells / 2);
        int iy = sy + (GridHeightCells / 2);

        SetVisibleCell(ix, iy, visible);
    }

    public void RevealCircle(Vector2 worldPos, float radiusCells, float softEdgeCells)
    {
        // Signed cell center
        int scx = Mathf.FloorToInt(worldPos.X / CellSizeWorld);
        int scy = Mathf.FloorToInt(worldPos.Y / CellSizeWorld);

        int cx = scx + (GridWidthCells / 2);
        int cy = scy + (GridHeightCells / 2);

        int r = Mathf.CeilToInt(radiusCells + softEdgeCells);

        for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
            {
                if (x < 0 || y < 0 || x >= GridWidthCells || y >= GridHeightCells)
                    continue;

                float dx = (x - cx);
                float dy = (y - cy);
                float d = Mathf.Sqrt(dx * dx + dy * dy);

                // 1 inside radius, then fades to 0 across softEdgeCells
                float v;
                if (d <= radiusCells) v = 1f;
                else if (d >= radiusCells + softEdgeCells) v = 0f;
                else
                {
                    float t = (d - radiusCells) / Mathf.Max(0.0001f, softEdgeCells);
                    v = 1f - t; // linear falloff; you can smoothstep this if you want
                }

                // Combine by max (never reduce visibility)
                float old = _maskImage.GetPixel(x, y).R;
                float nv = Mathf.Max(old, v);
                _maskImage.SetPixel(x, y, new Color(nv, 0, 0));
                _dirty = true;
            }
    }

    // Convenience: reveal a square radius (in cells) around a world position
    public void RevealSquare(Vector2 worldPos, int radiusCells)
    {
        Vector2 local = worldPos - MaskOriginWorld;
        int cx = Mathf.FloorToInt(local.X / CellSizeWorld);
        int cy = Mathf.FloorToInt(local.Y / CellSizeWorld);

        for (int y = cy - radiusCells; y <= cy + radiusCells; y++)
            for (int x = cx - radiusCells; x <= cx + radiusCells; x++)
                SetVisibleCell(x, y, true);
    }

    // If you change origin/size at runtime, call this:
    public void SyncShaderParams()
    {
        _mat.SetShaderParameter(ParamMaskOriginWorld, MaskOriginWorld);
        _mat.SetShaderParameter(ParamGridSizeCells, new Vector2(GridWidthCells, GridHeightCells));
        _mat.SetShaderParameter(ParamCellSizeWorld, CellSizeWorld);
    }
}