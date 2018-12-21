using UnityEngine;
[RequireComponent(typeof(Terrain))]
public class neighbours : MonoBehaviour
{
    public Terrain top = null;
    public Terrain bottom = null;
    public Terrain left = null;
    public Terrain right = null;
    void Start()
    {
        var t = GetComponent<Terrain>();
        t.SetNeighbors(left == t ? null : left, top == t ? null : top, right == t ? null : right, bottom == t ? null : bottom);
    }
}
