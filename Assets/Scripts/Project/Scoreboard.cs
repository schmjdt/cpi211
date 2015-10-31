using UnityEngine;
using System.Collections;

public class Scoreboard : MonoBehaviour {

    public TextureInfo[] texInfo;


    void Start()
    {
        //buildMesh(GameLogic.instance);
        buildMesh(GameLogic.instance.gameLayout.scoreArea.mesh);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void buildMesh(Mesh mesh)
    {
        MeshFilter mesh_filter = GetComponent<MeshFilter>();
        MeshCollider mesh_collider = GetComponent<MeshCollider>();

        mesh_filter.mesh = mesh;
        mesh_collider.sharedMesh = mesh;

        createTexture();
    }



    public void createTexture()
    {
        texInfo = TextureBuilder.ChopUpAllTextures(texInfo);
        GetComponent<Renderer>().material.mainTexture = TextureBuilder.BuildTexture(texInfo); ;
    }
}
