using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node
{
    public Node(bool _isWall, int _x, int _y)
    {
        isWall = _isWall;
        x = _x;
        y = _y;
        G = int.MaxValue;
        H = 0;
    }

    public bool isWall;
    public Node ParentNode;

    public int x, y, G, H;
    public int F { get { return G + H; } }
}

public class AStarManager : MonoBehaviour
{
    public TargetManager targetManager;
    public Vector2Int bottomLeft, topRight, startPos, targetPos;
    public List<Node> FinalNodeList;
    public bool allowDiagonal, dontCrossCorner;

    int sizeX, sizeY;
    Node[,] NodeArray;
    Node StartNode, TargetNode, CurNode;
    HashSet<Node> OpenList, ClosedList;

    private void Start()
    {
        // 초기화
    }

    public void SetTargetPos(Vector3 targetPosition)
    {
        targetPos = new Vector2Int(Mathf.RoundToInt(targetPosition.x), Mathf.RoundToInt(targetPosition.y));
    }

    public List<Vector3> PathFinding()
    {
        List<Vector3> path = new List<Vector3>();
        FinalNodeList = new List<Node>();

        sizeX = topRight.x - bottomLeft.x + 1;
        sizeY = topRight.y - bottomLeft.y + 1;
        NodeArray = new Node[sizeX, sizeY];

        // Grid 초기화 및 벽(Wall) 확인
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                bool isWall = false;
                foreach (Collider2D col in Physics2D.OverlapCircleAll(new Vector2(i + bottomLeft.x, j + bottomLeft.y), 0.4f))
                {
                    if (col.gameObject.layer == LayerMask.NameToLayer("Wall"))
                    {
                        isWall = true;
                        break;
                    }
                }

                NodeArray[i, j] = new Node(isWall, i + bottomLeft.x, j + bottomLeft.y);
            }
        }

        // 시작 지점과 목표 지점 확인
        if (!IsInBounds(startPos) || !IsInBounds(targetPos))
        {
            Debug.LogError($"Start or target position is out of bounds! StartPos: {startPos}, TargetPos: {targetPos}");
            return path;
        }

        StartNode = NodeArray[startPos.x - bottomLeft.x, startPos.y - bottomLeft.y];
        TargetNode = NodeArray[targetPos.x - bottomLeft.x, targetPos.y - bottomLeft.y];

        // 목표 지점이 벽일 경우, 벽을 무시하고 목표지점으로 이동하는 로직
        bool isTargetWall = TargetNode.isWall;
        if (isTargetWall)
        {
            TargetNode.isWall = false;
        }

        // 시작 지점이 벽일 경우 경로 계산을 허용
        if (StartNode.isWall)
        {
            StartNode.isWall = false;
        }

        OpenList = new HashSet<Node>() { StartNode };
        ClosedList = new HashSet<Node>();

        while (OpenList.Count > 0)
        {
            CurNode = null;
            foreach (var node in OpenList)
            {
                if (CurNode == null || node.F < CurNode.F || (node.F == CurNode.F && node.H < CurNode.H))
                {
                    CurNode = node;
                }
            }

            OpenList.Remove(CurNode);
            ClosedList.Add(CurNode);

            if (CurNode == TargetNode)
            {
                Node TargetCurNode = TargetNode;
                while (TargetCurNode != StartNode)
                {
                    FinalNodeList.Add(TargetCurNode);
                    path.Add(new Vector3(TargetCurNode.x, TargetCurNode.y, 0));
                    TargetCurNode = TargetCurNode.ParentNode;
                }
                FinalNodeList.Add(StartNode);
                path.Add(new Vector3(StartNode.x, StartNode.y, 0));
                path.Reverse();

                // 목적지 복원
                if (isTargetWall)
                {
                    TargetNode.isWall = true;
                }

                // 시작 지점 복원
                if (StartNode.isWall)
                {
                    StartNode.isWall = true;
                }

                return path;
            }

            // 현재 노드가 벽이 아닌지 확인
            if (CurNode.isWall && !(CurNode == TargetNode) && CurNode != StartNode)
            {
                Debug.LogError($"Current node is a wall: {CurNode.x}, {CurNode.y}");
                continue;
            }

            if (allowDiagonal)
            {
                OpenListAdd(CurNode.x + 1, CurNode.y + 1);
                OpenListAdd(CurNode.x - 1, CurNode.y + 1);
                OpenListAdd(CurNode.x - 1, CurNode.y - 1);
                OpenListAdd(CurNode.x + 1, CurNode.y - 1);
            }

            OpenListAdd(CurNode.x, CurNode.y + 1);
            OpenListAdd(CurNode.x + 1, CurNode.y);
            OpenListAdd(CurNode.x, CurNode.y - 1);
            OpenListAdd(CurNode.x - 1, CurNode.y);
        }

        Debug.LogError("Path not found!");

        // 목적지 복원
        if (isTargetWall)
        {
            TargetNode.isWall = true;
        }

        // 시작 지점 복원
        if (StartNode.isWall)
        {
            StartNode.isWall = true;
        }

        return path;

       
        
    }

    void OpenListAdd(int checkX, int checkY)
    {
        if (IsInBounds(new Vector2Int(checkX, checkY)) && !ClosedList.Contains(NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y]))
        {
            Node neighborNode = NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y];

            // 목표 지점이 벽이지만 통과할 수 있도록 함
            if (neighborNode.isWall && !(checkX == targetPos.x && checkY == targetPos.y))
            {
                return;
            }

            if (allowDiagonal)
            {
                if (NodeArray[CurNode.x - bottomLeft.x, checkY - bottomLeft.y].isWall && NodeArray[checkX - bottomLeft.x, CurNode.y - bottomLeft.y].isWall)
                    return;
            }

            if (dontCrossCorner)
            {
                if (NodeArray[CurNode.x - bottomLeft.x, checkY - bottomLeft.y].isWall || NodeArray[checkX - bottomLeft.x, CurNode.y - bottomLeft.y].isWall)
                    return;
            }

            const int STRAIGHT_COST = 10;
            const int DIAGONAL_COST = 14;
            int moveCost = CurNode.G + (CurNode.x - checkX == 0 || CurNode.y - checkY == 0 ? STRAIGHT_COST : DIAGONAL_COST);

            if (moveCost < neighborNode.G || !OpenList.Contains(neighborNode))
            {
                neighborNode.G = moveCost;
                neighborNode.H = (Mathf.Abs(neighborNode.x - TargetNode.x) + Mathf.Abs(neighborNode.y - TargetNode.y)) * STRAIGHT_COST;
                neighborNode.ParentNode = CurNode;

                OpenList.Add(neighborNode);
            }
        }
    }

    bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= bottomLeft.x && pos.x < topRight.x + 1 && pos.y >= bottomLeft.y && pos.y < topRight.y + 1;
    }

    void OnDrawGizmos()
    {
        if (FinalNodeList.Count != 0)
        {
            for (int i = 0; i < FinalNodeList.Count - 1; i++)
                Gizmos.DrawLine(new Vector2(FinalNodeList[i].x, FinalNodeList[i].y), new Vector2(FinalNodeList[i + 1].x, FinalNodeList[i + 1].y));
        }
    }
}