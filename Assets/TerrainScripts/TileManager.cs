using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

public class TileManager : MonoBehaviour
{
    Vector3 _lastPos = Vector3.zero;
    float sqrThreshold;
    public float MovementThreshold;
    public int tilesToLoad;
    public float TileSize;
    public GameObject template;
    public float HeightToSpawn;
    Dictionary<GameObject, Vector3> loadedTiles = new Dictionary<GameObject, Vector3>();
    public static Func<Vector3> OffsetFunc = () => (Vector3.zero);
    float d = 0;
    void Start()
    {
        sqrThreshold = MovementThreshold * MovementThreshold;
        UpdateTiles();
#if !UNITY_EDITOR
        OffsetFunc = () => ServiceProvider.Instance.GameWorld.FloatingOriginOffset;
#endif
    }
    void Update()
    {
        Vector3 tp = Camera.main.transform.position.Game2Map();
        tp.y = 0;
        d = (_lastPos - tp).sqrMagnitude;
        if (d > sqrThreshold)
        {
            _lastPos = tp;
            UpdateTiles();
        }
        
    }
    void UpdateTiles()
    {
        Vector3 gamePos = Camera.main.transform.position;
        Vector3 mapPos = gamePos.Game2Map();
        int tilex = (int)Math.Floor(mapPos.x / TileSize) + 1;
        int tilez = (int)Math.Floor(mapPos.z / TileSize);
     //   Debug.Log(tilex + ", " + tilez);
        List<Vector3> tta = new List<Vector3>();
        for (int x = tilex - tilesToLoad; x < tilex + tilesToLoad + 1; x++)
        {
            for (int z = tilez - (tilesToLoad); z < tilez + tilesToLoad + 1; z++)
            {
                Vector3 map = new Vector3((x-0.5f) * TileSize, 0, (z+0.5f) * TileSize);
                tta.Add(map + (Vector3.up * HeightToSpawn));
            }
        }
     //   string st = "";
     //   foreach (Vector3 v in loadedTiles.Values)
     //   {
     //       st += v.ToString() + ", ";
     //   }
     //   Debug.Log(st);
      //  st = "";
       // foreach (Vector3 v in tta)
       // {
      //      st += v.ToString() + ", ";
       // }
        //Debug.Log(st);
       // int i = 0;
        foreach (var kvp in new Dictionary<GameObject, Vector3>(loadedTiles))
        {
            if (!tta.Contains(kvp.Value))
            {
                RemoveTile(kvp.Key);
          //      i++;
            }
        }
        //Debug.Log(i + " tiles removed");
        //i = 0;
        foreach (Vector3 v in tta)
        {
            if (!loadedTiles.ContainsValue(v))
            {
                CreateTile(v);
               // i++;
            }
        }
       // Debug.Log(i + " tiles created");
    }
    void CreateTile(Vector3 pos)
    {
        if (loadedTiles.ContainsValue(pos)) { return; }
        var g = (GameObject)Instantiate(template, pos.Map2Game(), Quaternion.identity);
        loadedTiles.Add(g, pos);
        g.SetActive(true);
    }
    void RemoveTile(GameObject t)
    {
        if (loadedTiles.ContainsKey(t)) { loadedTiles.Remove(t); }
        Destroy(t);
    }
}

