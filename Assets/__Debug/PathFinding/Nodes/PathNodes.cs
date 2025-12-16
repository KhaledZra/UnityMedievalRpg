using System.Collections.Generic;
using System.Numerics;

namespace PathFinding.Nodes
{
    // Mostly for debugging
    public enum NodeState
    {
        Default,
        Visited,
        Path
    }
    
    public class BasePathNode
    {
        public Vector3 Coordinates { get; set; }
        public BasePathNode Parent { get; set; }
        public HashSet<BasePathNode> Neighbors { get; set; } = new();
        
        public NodeState State { get; set; }
        
        public BasePathNode(Vector3 pos)
        {
            Coordinates = pos;
            Parent = null;
            State = NodeState.Default;
        }
    }

    public class PathNodeData
    {
        public BasePathNode Node { get; set; }
        public float GScore { get; set; }
        public float HScore { get; set; }

        public PathNodeData(BasePathNode node)
        {
            Node = node;
            GScore = float.MaxValue;
            HScore = 0f;
        }

        public float FScore()
        {
            return GScore + HScore;
        }
    }
}