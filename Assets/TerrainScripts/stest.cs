using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Collections;

[RequireComponent(typeof(Terrain))]
public class stest : MonoBehaviour
{
    #region Parameters
    public TextureGenerator biome;
    public TextureGenerator heat;
    public TextureGenerator moisture;
    Terrain t;
    [Space(10)]
    public float blend = 0f;
    public float seaOffset = 0f;
    public float noiseScale = 1;
    [Space(10)]
    public float heatmapScale = 1;
    public float moistureScale = 1;
    public int width = 513;
    public int height = 513;
    [Space(10)]
    public float HeatmapBlend = 0.01f;
    public float HeatmapMiddleStart = 0.37f;
    public float HeatmapMiddleEnd = 0.6f;
    [Space(10)]
    public float MoistureBlend = 0.01f;
    public float MoistureMiddleStart = 0.37f;
    public float MoistureMiddleEnd = 0.6f;
    [Header("Sea")]
    public float SeaNoiseScale = 0.1f;
    public float SeaNoiseMagnitude = 0.1f;
    [Header("Desert")]
    public float DesertNoiseScale = 3f;
    public float DesertNoiseMagnitude = 0.02f;
    public float DesertHeight = 0.2f;
    [Header("Grassland")]
    public float GrasslandHeight = 0.3f;
    public float GrasslandNoiseScale = 3f;
    public float GrasslandMultiNoiseScale = 5f;
    public float GrasslandMagnitude = 0.075f;
    [Header("Mountain")]
    public float MountainNoiseScale = 0.5f;
    public float MountainNoiseScale2 = 0.05f;
    public float MountainNoiseScale3 = 0.05f;
    public float MountainMagnitude = 0.3f;
    public float MountainSnowThreshold = 0.5f;
    public float MountainHeight = 0.3f;
    public float MountainGrassThreshold = 0.2f;
    [Header("Swamp")]
    public float SwampNoiseScale1 = 0.001f;
    public float SwampNoiseScale2 = 0.005f;
    public float SwampNoiseCoefficient = 0.7f;
    public float SwampMetaNoiseScale = 0.002f;
    public float SwampMetaNoise2Scale = 0.05f;
    public float SwampMetaNoiseCoefficient = 0.3f;
    public float SwampMagnitude = 0.2f;
    public float SwampHeight = 0.13f;
    [Header("Tundra")]
    public float TundraNoiseScale = 0.002f;
    public float TundraMultiNoiseScale = 0.0002f;
    public float TundraMagnitude = 0.2f;
    public float TundraHeight = 0.23f;
    
    static Dictionary<BiomeType, Color> _biomeColours = null;
    public static Dictionary<BiomeType, Color> BiomeColors
    {
        get
        {
            if (_biomeColours == null)
            {
                _biomeColours = new Dictionary<BiomeType, Color>
                {
                    { BiomeType.Ice, rgb(186, 243, 252) },
                    { BiomeType.Mountain, rgb(175, 175, 175) },
                    { BiomeType.Grassland, rgb(78, 255, 68) },
                    { BiomeType.Swamp, rgb(61, 37, 0) },
                    { BiomeType.Desert, rgb(255, 255, 0) },
                    { BiomeType.Sea, rgb(2, 64, 209) },
                };
            }
            return _biomeColours;
        }
    }
    static BiomeType[,] biomeTable = new BiomeType[,]
    {
        //  dry                 mid                 wet
        { BiomeType.Mountain,   BiomeType.Ice,      BiomeType.Ice },        // cold
        { BiomeType.Grassland,  BiomeType.Swamp,    BiomeType.Sea },        // mid
        { BiomeType.Desert,     BiomeType.Swamp,   BiomeType.Sea }         // hot
    };
#endregion
    #region Events
    void Start()
    {
        t = GetComponent<Terrain>();
        UpdateButton();
    }
    public void UpdateButton()
    {
        float start = Time.realtimeSinceStartup;
        this.t = GetComponent<Terrain>();
        var gp = transform.position.Game2Map();
        var t = GenerateTile(new Vector3(gp.z, gp.x) * ((this.t.terrainData.heightmapResolution - 1) / this.t.terrainData.size.x), this.t.terrainData.heightmapResolution, this.t.terrainData.heightmapResolution);
        SetTerrainData(t.heightmap, t.alphamap);
        Debug.LogFormat("Tile loaded in {0} seconds", Time.realtimeSinceStartup - start);
    }
    #endregion
    #region Generation
    // alpha layers:
    // 0 sand
    // 1 grass
    // 2 snow
    // 3 rock
    // 4 rocky grass
    float getMoist(float x, float y)
    {
        Vector2 point = new Vector2(x, y) / heatmapScale;
        float v = (Simplex2D(point, 0.1f) * 0.475f) + (Simplex2D(point + (Vector2.one * moistureScale * 5), 0.1f) * 0.475f) + (Simplex2D(point, 0.5f) * 0.05f);
        if (v < MoistureMiddleStart - MoistureBlend) { return 0; } // all 0
        else if (v < MoistureMiddleStart + MoistureBlend) { return (v - (MoistureMiddleStart - MoistureBlend)) / (MoistureBlend * 2); }
        else if (v < MoistureMiddleEnd - MoistureBlend) { return 1; }
        else if (v < MoistureMiddleEnd + MoistureBlend) { return (v - (MoistureMiddleEnd - MoistureBlend)) / (MoistureBlend * 2) + 1; }
        else { return 2; }
    }
    float getHeat(float x, float y)
    {
        Vector2 point = new Vector2(x, y) / heatmapScale;
        float v = (Simplex2D(point, 0.1f) * 0.475f) + (Simplex2D(point + (Vector2.one * heatmapScale * 5), 0.1f) * 0.475f) + (Simplex2D(point, 0.5f) * 0.05f);
        if (v < HeatmapMiddleStart - HeatmapBlend) { return 0; } // all 0
        else if (v < HeatmapMiddleStart + HeatmapBlend) { return (v - (HeatmapMiddleStart - HeatmapBlend)) / (HeatmapBlend * 2); }
        else if (v < HeatmapMiddleEnd - HeatmapBlend) { return 1; }
        else if (v < HeatmapMiddleEnd + HeatmapBlend) { return (v - (HeatmapMiddleEnd - HeatmapBlend)) / (HeatmapBlend * 2) + 1; }
        else { return 2; }
    }
    float[] getBiome(float x, float y)
    {
        float[] ret = new float[6];
        float m = getMoist(x, y);
        float h = getHeat(x, y);
        bool mh = m % 1 < float.Epsilon; // is m whole
        bool hh = h % 1 < float.Epsilon; // is h whole
        if (mh)
        { // moisture is whole
            if (hh)
            { // both are whole
                ret[(int)biomeTable[(int)h, (int)m]] = 1;
            }
            else
            { // blend heat only
                float h1 = h % 1;
                ret[(int)biomeTable[Mathf.FloorToInt(h), (int)m]] += 1 - h1;
                ret[(int)biomeTable[Mathf.CeilToInt(h), (int)m]] += h1;
            }
        }
        else
        {
            if (hh)
            { // blend moisture only
                float m1 = m % 1;
                ret[(int)biomeTable[(int)h, Mathf.FloorToInt(m)]] += 1 - m1;
                ret[(int)biomeTable[(int)h, Mathf.CeilToInt(m)]] += m1;
            }
            else
            { // blend both
                float m1 = m % 1;
                float h1 = h % 1;
                ret[(int)biomeTable[Mathf.FloorToInt(h), Mathf.FloorToInt(m)]] += (1 - h1) * (1 - m1);
                ret[(int)biomeTable[Mathf.CeilToInt(h), Mathf.CeilToInt(m)]] += h1 * m1;
                ret[(int)biomeTable[Mathf.FloorToInt(h), Mathf.CeilToInt(m)]] += (1 - h1) * m1;
                ret[(int)biomeTable[Mathf.CeilToInt(h), Mathf.FloorToInt(m)]] += h1 * (1 - m1);
            }
        }

        return ret;
    }
    #region Biome Functions
    float GetIceHeight(Vector2 mp, float seaLevel)
    {
        return (Simplex2D(mp, TundraNoiseScale) * Simplex2D(mp, TundraMultiNoiseScale) * TundraMagnitude) + TundraHeight;
    }
    float GetMountainHeight(Vector2 mp, float seaLevel)
    {
        float v = 2 * (0.5f - Mathf.Abs(0.5f - Simplex2D(mp, MountainNoiseScale))); // ridged noise
        v += 2 * (0.5f - Mathf.Abs(0.5f - Simplex2D(mp, MountainNoiseScale2))) * v; // ridged noise
        v += 2 * (0.5f - Mathf.Abs(0.5f - Simplex2D(mp, MountainNoiseScale3))) * v; // ridged noise
        v *= MountainMagnitude;
        v += MountainHeight;
        return v;
    }
    float GetGrasslandHeight(Vector2 mp, float seaLevel)
    {
        return (Simplex2D(mp, GrasslandNoiseScale) * Simplex2D(mp, GrasslandMultiNoiseScale) * GrasslandMagnitude) + GrasslandHeight;
    }
    float GetSwampHeight(Vector2 mp, float seaLevel)
    {
        return (((((Simplex2D(mp, SwampNoiseScale1) * 0.3f) + (Simplex2D(mp, SwampNoiseScale2) * 0.7f)) * SwampNoiseCoefficient) + ((Simplex2D(mp, SwampMetaNoiseScale) * Simplex2D(mp, SwampMetaNoise2Scale)) * SwampMetaNoiseCoefficient)) * SwampMagnitude) + SwampHeight;
    }
    float GetDesertHeight(Vector2 mp, float seaLevel)
    {
        return (Simplex2D(mp, DesertNoiseScale) * DesertNoiseMagnitude) + DesertHeight;
    }
    float GetSeaHeight(Vector2 mp, float seaLevel)
    {
        return Simplex2D(mp, SeaNoiseScale) * SeaNoiseMagnitude;
    }
    float[] GetIceAlphas(Vector2 mp, float seaLevel, float height)
    {
        var res = new float[5];
        res[2] = 1;
        return res;
    }
    float[] GetMountainAlphas(Vector2 mp, float seaLevel, float height)
    {
        var res = new float[5];
        if (height > MountainSnowThreshold) { res[2] = 1; } // snow
        else if (height < MountainGrassThreshold) { res[4] = 1; } // rocky grass
        else { res[3] = 1; } // rock
        return res;
    }
    float[] GetGrasslandAlphas(Vector2 mp, float seaLevel, float height)
    {
        var res = new float[5];
        res[1] = 1;
        return res;
    }
    float[] GetSwampAlphas(Vector2 mp, float seaLevel, float height)
    {
        var res = new float[5];
        if (height < seaLevel - blend)
        {
            res[0] = 1; // all sand
            res[1] = 0; // no grass
        }
        else if (height < seaLevel + blend)
        {
            res[0] = ((seaLevel + blend) - height) / blend;
            res[1] = (seaLevel + height) / blend; // blended sand and grass
        }
        else
        {
            res[0] = 0;
            res[1] = 1;
        }
        return res;
    }
    float[] GetDesertAlphas(Vector2 mp, float seaLevel, float height)
    {
        var res = new float[5];
        res[0] = 1;
        return res;
    }
    float[] GetSeaAlphas(Vector2 mp, float seaLevel, float height)
    {
        var res = new float[5];
        res[0] = 1;
        return res;
    }
    #endregion
    TileData GenerateTile(Vector2 pos, int sx, int sy)
    {
        float seaLevel = 0.15f;
        float[,] heights = new float[sx, sy];
        float[,,] alphas = new float[sx, sy, 5]; // number of textures = 5
        for (int x = 0; x < sx; x++)
        {
            for (int y = 0; y < sy; y++)
            {
                Vector2 mp = pos;
                mp.x += x;
                mp.y += y;
                float h = 0;
                float[] alpha = new float[5];
                float[] biomes = getBiome(mp.x, mp.y);
                if (biomes[0] != 0)
                {
                    h += GetIceHeight(mp, seaLevel) * biomes[0];
                }
                if (biomes[1] != 0)
                {
                    h += GetMountainHeight(mp, seaLevel) * biomes[1];
                }
                if (biomes[2] != 0)
                {
                    h += GetGrasslandHeight(mp, seaLevel) * biomes[2];
                }
                if (biomes[3] != 0)
                {
                    h += GetSwampHeight(mp, seaLevel) * biomes[3];
                }
                if (biomes[4] != 0)
                {
                    h += GetDesertHeight(mp, seaLevel) * biomes[4];
                }
                if (biomes[5] != 0)
                {
                    h += GetSeaHeight(mp, seaLevel) * biomes[5];
                }

                heights[x, y] = h;

                if (biomes[0] != 0)
                {
                    alpha = Combine(alpha, GetIceAlphas(mp, seaLevel, h), biomes[0]);
                }
                if (biomes[1] != 0)
                {
                    alpha = Combine(alpha, GetMountainAlphas(mp, seaLevel, h), biomes[1]);
                }
                if (biomes[2] != 0)
                {
                    alpha = Combine(alpha, GetGrasslandAlphas(mp, seaLevel, h), biomes[2]);
                }
                if (biomes[3] != 0)
                {
                    alpha = Combine(alpha, GetSwampAlphas(mp, seaLevel, h), biomes[3]);
                }
                if (biomes[4] != 0)
                {
                    alpha = Combine(alpha, GetDesertAlphas(mp, seaLevel, h), biomes[4]);
                }
                if (biomes[5] != 0)
                {
                    alpha = Combine(alpha, GetSeaAlphas(mp, seaLevel, h), biomes[5]);
                }


                for (int i = 0; i < 5; i++)
                {
                    alphas[x, y, i] = alpha[i];
                }
            }
        }
        return new TileData(heights, alphas);
    }
    #endregion
    #region Helpers
    public static float Simplex2D(Vector2 point, float frequency)
    {
        return (Noise.Simplex2D(point, frequency).value + 1) / 2;
    }
    float[] Combine(float[] orig, float[] n, float factor)
    {
        for (int i = 0; i < orig.Length; i++)
        {
            orig[i] += n[i] * factor;
        }
        return orig;
    }
    static Color rgb(int r, int g, int b)
    {
        return new Color(r / 255f, g / 255f, b / 255f);
    }
    public void SetTerrainData(float[,] heights, float[,,] alphas)
    {
        t = GetComponent<Terrain>();
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
    #endregion
}

public enum BiomeType
{
    Ice,
    Mountain,
    Grassland,
    Swamp,
    Desert,
    Sea
}
