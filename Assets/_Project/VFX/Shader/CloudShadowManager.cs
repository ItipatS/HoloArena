using UnityEngine;

public class CloudShadowManager : MonoBehaviour
{
    public Texture2D cloudShadowTex;
    public Vector2 scrollSpeed = new Vector2(0.01f, 0f);
    public float tiling = 0.05f;

    private Vector2 offset;

    void Update()
    {
        offset += scrollSpeed * Time.deltaTime;
        Shader.SetGlobalTexture("_CloudShadowTex", cloudShadowTex);
        Shader.SetGlobalVector("_CloudShadowParams", new Vector4(offset.x, offset.y, tiling, 0));
    }
}
