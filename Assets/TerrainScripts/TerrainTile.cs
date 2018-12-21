using UnityEngine;

public class TerrainTile : MonoBehaviour
{
    public Vector3 pos;
    public void SetTerrainData(float[,] heights, float[,,] alphas, Vector3 pos)
    {
        this.pos = pos;
        Terrain t = GetComponent<Terrain>();
        TerrainData data = new TerrainData();
        TerrainData od = t.terrainData;

        data.alphamapResolution = od.alphamapResolution;
        data.baseMapResolution = od.baseMapResolution;
        data.heightmapResolution = od.heightmapResolution;
        data.size = od.size;
        data.splatPrototypes = od.splatPrototypes;
        data.thickness = od.thickness;

        t.terrainData = data;
        GetComponent<TerrainCollider>().terrainData = data;
        data.alphamapResolution = data.heightmapResolution;

        data.SetHeights(0, 0, heights);
        data.SetAlphamaps(0, 0, alphas);
        t.Flush();
    }
}