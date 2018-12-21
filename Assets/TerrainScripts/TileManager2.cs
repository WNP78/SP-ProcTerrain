using UnityEngine;
using System.Collections.Generic;

public class TileManager2 : MonoBehaviour
{
    TerrainGenerator gen;
    public GameObject Prefab;
    Terrain prefabTerrain;
    public int TilesNeeded = 9;
    TerrainTile[] tiles;
    public float TileSize;
    public float TileHeight;
    List<Vector3> tilesBeingGenerated = new List<Vector3>();
    List<Vector3> wantedTiles = new List<Vector3>();
    Vector3 _lastPos = Vector3.zero;
    public float MovementThreshold = 1000f;
    public float ms;
    void Start()
    {
        ms = MovementThreshold * MovementThreshold;
        prefabTerrain = Prefab.GetComponent<Terrain>();
        gen = GetComponent<TerrainGenerator>();
        tiles = new TerrainTile[TilesNeeded];
        for (int i = 0; i < TilesNeeded; i++) { tiles[i] = Instantiate(Prefab).GetComponent<TerrainTile>(); tiles[i].transform.parent = transform; tiles[i].gameObject.name = "Tile " + (i + 1).ToString(); }
        DoUpdate(false);
    }
    void Update()
    {
#if !UNITY_EDITOR
        Vector3 playerPos = ServiceProvider.Instance.PlayerAircraft.MainCockpitPosition;
#else
        Vector3 playerPos = Camera.main.transform.position;
#endif
        playerPos = playerPos.Game2Map();
        Vector3 p = playerPos;
        p.y = 0;
        if ((p - _lastPos).sqrMagnitude > ms) { DoUpdate(true); _lastPos = p; }
    }
    void DoUpdate(bool async)
    {
#if !UNITY_EDITOR
        Vector3 playerPos = ServiceProvider.Instance.PlayerAircraft.MainCockpitPosition;
#else
        Vector3 playerPos = Camera.main.transform.position;
#endif
        playerPos = playerPos.Game2Map();
        Vector3 p = playerPos;
        wantedTiles = new List<Vector3>();
        playerPos.x -= TileSize / 2;
        playerPos.z -= TileSize / 2;
        int tx = Mathf.RoundToInt(playerPos.x / TileSize);
        int ty = Mathf.RoundToInt(playerPos.z / TileSize);
        for (int x = tx - 1; x < tx + 2; x++)
        {
            for (int y = ty - 1; y < ty + 2; y++)
            {
                wantedTiles.Add(new Vector3(x * TileSize, TileHeight, y * TileSize));
            }
        } // decide which tiles we want
        List<Vector3> ps = new List<Vector3>(); // positions of existing tiles
        foreach (TerrainTile t in tiles)
        {
            if (!t.gameObject.activeSelf) { continue; }
            if (!wantedTiles.Contains(t.pos))
            {
                RemoveTile(t);
                continue;
            }
            ps.Add(t.pos);
        } // remove any existing tiles we don't want
        foreach (Vector3 wanted in wantedTiles)
        {
            if (!ps.Contains(wanted) && !tilesBeingGenerated.Contains(wanted))
            {
                tilesBeingGenerated.Add(wanted);
                GenTile(wanted, async);
            }
        } // add any ones we do want
    }
    void RemoveTile(TerrainTile t)
    {
        t.gameObject.SetActive(false);
        //Debug.Log("Remove" + t.transform.mapPosition());
    }
    void AddTile(Vector3 pos, TileData data, Vector2 cp)
    {
        //Debug.Log("Add" + pos);
        tilesBeingGenerated.Remove(pos);
        if (!wantedTiles.Contains(pos)) { return; } // tile is no longer wanted ;( all that work for nothing!
        foreach (var tile in tiles)
        {
            if (!tile.gameObject.activeSelf)
            {
                tile.transform.position = pos.Map2Game();
                tile.SetTerrainData(data.heightmap, data.alphamap, pos);
                tile.gameObject.SetActive(true);
                return;
            }
        }
        Debug.LogError("Ran out of tiles!");
        enabled = false; // failsafe. maybe. o.0
    }
    void GenTile(Vector3 pos, bool async = true)
    {
        //Debug.Log("Gen" + pos);
        int r = prefabTerrain.terrainData.heightmapResolution;
        if (async)
        {
            gen.GenerateTileAsync(gen.worldToTerrain(pos, prefabTerrain), r, r, (data, cp) => AddTile(pos, data, cp));
        }
        else
        {
            AddTile(pos, gen.GenerateTile(gen.worldToTerrain(pos, prefabTerrain), r, r), pos);
        }
    }
    #region Debug GUI
    //Rect ar = new Rect(Screen.width - 300, Screen.height - 25, 300, 25);
    //Rect dr = new Rect(Screen.width - 600, Screen.height - 300, 600, 275);
    //bool dbg = false;
    //void OnGUI()
    //{
    //    GUILayout.BeginArea(ar, GUI.skin.box);
    //    GUILayout.BeginHorizontal();
    //    GUILayout.Label("IN-DEVELOPMENT MOD BY WNP78");
    //    if (GUILayout.Button("Debug")) { dbg = !dbg; }
    //    GUILayout.EndHorizontal();
    //    GUILayout.EndArea();
    //    if (dbg)
    //    {
    //        GUILayout.BeginArea(dr, GUI.skin.box);
    //        GUILayout.BeginHorizontal();
    //        GUILayout.BeginVertical();
    //        GUILayout.Label("tilesBeingGenerated");
    //        foreach (Vector3 v in tilesBeingGenerated)
    //        {
    //            GUILayout.Label(v.magnitude.ToString());
    //        }
    //        GUILayout.EndVertical();
    //        GUILayout.BeginVertical();
    //        GUILayout.Label("wantedTiles");
    //        foreach (Vector3 v in wantedTiles)
    //        {
    //            GUILayout.Label(v.magnitude.ToString());
    //        }
    //        GUILayout.EndVertical();
    //        GUILayout.BeginVertical();
    //        GUILayout.Label("tiles");
    //        foreach (var t in tiles)
    //        {
    //            GUILayout.Label(t.transform.position.Game2Map().magnitude.ToString());
    //        }
    //        GUILayout.EndVertical();
    //        GUILayout.EndHorizontal();
    //        GUILayout.EndArea();
    //    }
    //}
    #endregion
}
public static class Extenions
{
    public static Vector3 Map2Game(this Vector3 a)
    {
        return a - ServiceProvider.Instance.GameWorld.FloatingOriginOffset;
    }
    public static Vector3 Game2Map(this Vector3 a)
    {
        return a + ServiceProvider.Instance.GameWorld.FloatingOriginOffset;
    }
    public static Vector3 mapPosition(this Transform t)
    {
        return t.position.Game2Map();
    }
}