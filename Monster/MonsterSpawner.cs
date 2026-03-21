using Godot;
using System;
using Godot.Collections;
using Array = System.Array;

public partial class MonsterSpawner : Node2D
{
    [Export] public float SpawnDelayInitial = 5f;
    [Export] public float SpawnDelay = 10f;
    [Export] public float SpawnDelayMin = 5f;
    [Export] public float SpawnDelayReductionPerSpawn = 0.5f;
    private float _lastSpawnTime = -999f;
    private Node2D _boat;

    [Export] public PackedScene MonsterTemplate;
    private Array<SpawnPoint> _spawnNodes = [];
    
    public override void _Ready()
    {
        Visible = false;
        _boat = GetNode<Node2D>("../Boat");
        Array<Node> array = GetTree().GetNodesInGroup("SpawnPoints");
        foreach (var node in array)
        {
            SpawnPoint spawnPoint = (SpawnPoint)node;
            if (spawnPoint != null)
            {
                _spawnNodes.Add(spawnPoint);
            }
        }
    }

    public override void _Process(double delta)
    {
        float time = Time.GetTicksMsec() / 1000f;
        
        if (time < SpawnDelayInitial) return;
        if (time < _lastSpawnTime + SpawnDelay) return;
        
        SpawnMonster();
        _lastSpawnTime = time;
        SpawnDelay = Mathf.Max(SpawnDelay - SpawnDelayReductionPerSpawn, SpawnDelayMin);
    }

    private void SpawnMonster()
    {
        Node2D nearestNode = null;
        var nearestDistance = float.MaxValue;
        foreach (var node in _spawnNodes)
        {
            if (node.IsOnScreen) continue;
        
            var distance = node.GlobalPosition.DistanceSquaredTo(_boat.GlobalPosition);
            
            if (nearestDistance < distance) continue;
            nearestNode = node;
            nearestDistance = distance;
        }
        
        if (nearestNode == null) return;
        Monster newMonster = MonsterTemplate.Instantiate<Monster>();
        newMonster.GlobalPosition = nearestNode.GlobalPosition;
        var enumArray = Enum.GetValues(typeof(Monster.EMonster));
        Random rng = new Random ();
        newMonster.MonsterType = (Monster.EMonster)enumArray.GetValue(rng.Next(enumArray.Length));
        GetParent().AddChild(newMonster);
    }
}
