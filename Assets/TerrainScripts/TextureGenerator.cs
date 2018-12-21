using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class TextureGenerator : MonoBehaviour
{
    public Gradient gradient;
    public float opacity = 1;
	public void SetTexture(float[,] values)
    {
        var k = gradient.alphaKeys;
        for (int d = 0; d < k.Length; d++)
        {
            k[d].alpha = opacity;
        }
        gradient.alphaKeys = k;
        var m = GetComponent<MeshRenderer>();
        Texture2D t = new Texture2D(values.GetLength(0), values.GetLength(1));
        Color[] cols = new Color[values.Length];
        int i = 0;
        for (int x = 0; x < values.GetLength(0); x++)
        {
            for (int y = 0; y < values.GetLength(1); y++)
            {
                cols[i] = gradient.Evaluate(values[x, y]);
                i++;
            }
        }
        t.filterMode = FilterMode.Point;
        t.SetPixels(cols);
        t.Apply();
        Material n = Instantiate(m.sharedMaterial);
        n.mainTexture = t;
        m.sharedMaterial = n;
    }
    public void SetTexture(Color[,] values)
    {
        var k = gradient.alphaKeys;
        for (int d = 0; d < k.Length; d++)
        {
            k[d].alpha = opacity;
        }
        gradient.alphaKeys = k;
        var m = GetComponent<MeshRenderer>();
        Texture2D t = new Texture2D(values.GetLength(0), values.GetLength(1));
        Color[] cols = new Color[values.Length];
        int i = 0;
        for (int x = 0; x < values.GetLength(0); x++)
        {
            for (int y = 0; y < values.GetLength(1); y++)
            {
                cols[i] = values[x, y];
                i++;
            }
        }
        t.filterMode = FilterMode.Point;
        t.SetPixels(cols);
        t.Apply();
        Material n = Instantiate(m.sharedMaterial);
        n.mainTexture = t;
        m.sharedMaterial = n;
    }
}
