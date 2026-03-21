using Godot;

public partial class HexNode : Control
{
    [Signal] public delegate void DragStartedEventHandler(HexNode node);
    [Signal] public delegate void DragEnteredEventHandler(HexNode node);
    [Signal] public delegate void HoveredEventHandler(HexNode node);
    [Signal] public delegate void UnhoveredEventHandler(HexNode node);

    public int Id { get; private set; }
    public int Q { get; private set; }
    public int R { get; private set; }

    [Export]
    public float Radius { get; set; } = 10f;

    public bool IsHovered { get; private set; }
    public bool IsSelected { get; set; }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    public void Initialize(int id, int q, int r, Vector2 center)
    {
        Id = id;
        Q = q;
        R = r;

        Size = new Vector2(Radius * 2f, Radius * 2f);
        CustomMinimumSize = Size;
        Position = center - (Size / 2f);

        QueueRedraw();
    }

    public Vector2 GetCenterPosition()
    {
        return Position + (Size / 2f);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton &&
            mouseButton.ButtonIndex == MouseButton.Left &&
            mouseButton.Pressed)
        {
            EmitSignal(SignalName.DragStarted, this);
        }
    }

    public override void _Draw()
    {
        Vector2 localCenter = Size / 2f;

        Color color = Colors.White;

        if (IsSelected)
            color = Colors.LimeGreen;
        else if (IsHovered)
            color = Colors.Gold;

        DrawCircle(localCenter, Radius, color);
    }

    private void OnMouseEntered()
    {
        IsHovered = true;
        QueueRedraw();

        EmitSignal(SignalName.Hovered, this);
        EmitSignal(SignalName.DragEntered, this);
    }

    private void OnMouseExited()
    {
        IsHovered = false;
        QueueRedraw();

        EmitSignal(SignalName.Unhovered, this);
    }
}