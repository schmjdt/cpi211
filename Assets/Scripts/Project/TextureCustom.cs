using UnityEngine;
using System.Collections;

public class TextureCustom {

}



[System.Serializable]
public class TextureInfo
{
    public Texture2D texSheet;
    public int resolution;
    public Color[][] texColors;

    public void chopTexture()
    {
        texColors = TextureBuilder.ChopUpTiles(this);
    }
}



public struct TextureBuilder
{
    public static Color[][] ChopUpTiles(TextureInfo texInfo)
    {
        Texture2D tex = texInfo.texSheet;
        int tileResolution = texInfo.resolution;

        int numTilesPerRow = tex.width / tileResolution;
        int numRows = tex.height / tileResolution;

        Color[][] tiles = new Color[numTilesPerRow * numRows][];

        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numTilesPerRow; x++)
            {
                tiles[y * numTilesPerRow + x] = tex.GetPixels(x * tileResolution,
                                                              y * tileResolution,
                                                              tileResolution,
                                                              tileResolution);
            }
        }

        return tiles;
    }

    public static TextureInfo[] ChopUpAllTextures(TextureInfo[] texInfos)
    {
        for (int i = 0; i < texInfos.Length; i++)
        {
            texInfos[i].chopTexture();
        }
        return texInfos;
    }



    /*
    public static Texture2D BuildTexture(TextureInfo[] texInfos)
    {
        return BuildTexture(texInfos, GameLogic.instance.gameLayout.scoreArea.getMeshSize(), null);
    }
    */

    public static Texture2D BuildTexture(CardInfo cardInfo)
    {
        return BuildTexture(cardInfo.texInfo, GameLogic.instance.cardLayout.getMeshSize(), cardInfo);
    }

    public static Texture2D BuildTexture(TextureInfo[] texInfos,
                                              int[] size_mesh,
                                              CardInfo cardInfo)
    {

        //TextureInfo texInfo = texInfos[0];  
        int size_x = size_mesh[0];
        int size_z = size_mesh[1];

        // HARD: Using the first textures resolution (for now) assuming all will be same (for now)
        int tileResolution = texInfos[0].resolution;
        int texWidth = size_x * tileResolution;
        int texHeight = size_z * tileResolution;
        Texture2D texture = new Texture2D(texWidth, texHeight);

        for (int y = 0; y < size_z; y++)
        {
            for (int x = 0; x < size_x; x++)
            {
                // Creating the color array, p
                // Color[] p = texInfo.texColors[0];

                Color[] p;
                if (cardInfo != null)
                    p = getCardColors(x, y, size_x, size_z, texInfos, cardInfo);
                else
                    p = getColors(x, y, size_x, size_z, texInfos);

                // Setting the texture's pixels using the color array, p
                texture.SetPixels(x * tileResolution,
                                  y * tileResolution,
                                  tileResolution,
                                  tileResolution,
                                  p);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        return texture;
    }

    // FUTURE USE: Will be used for customization
    public static Color[] getTexColors()
    {
        Color[] p = { new Color(0f, 0f, 0f, 1f) };

        return p;
    }

    static Color[] getColors(int x, int y, int size_x, int size_z, TextureInfo[] texInfos)
    {
        Color[] p;
        if (x == 0 && y == 0)
            p = texInfos[0].texColors[0];
        else if (x == 3 && y == 2)
            p = texInfos[0].texColors[21];
        else
            p = texInfos[0].texColors[11];
        return p;
    }

    static Color[] getCardColors(int x, int y, int size_x, int size_z, TextureInfo[] texInfos, CardInfo cardInfo)
    {
        Color[] p;
        // HARD: Using magic values for now knowing the size of textures
        //      NEED: x, z, size_x, size_z, texInfos, cardInfo
        // ::Texture [0]::
        // Cost:
        if (x == 1 && y == size_z - 2)
        {
            //p = texInfos[0].texColors[Math.Abs(9 - cardInfo.cost)];
            p = texInfos[0].texColors[cardInfo.cost];
        }
        // Points:
        else if (x == size_x - 2 && y == size_z - 2)
        {
            p = texInfos[0].texColors[cardInfo.score];
        }
        // ::Texture [1]::
        // Sides:
        else if (y == 1 && (x > 0 && x < size_x - 1))
        {
            //p = texInfos[1].texColors[Math.Abs(9 - cardInfo.sideID[Math.Abs(5 - (x - 1))])];
            p = texInfos[1].texColors[cardInfo.sideID[x - 1]];
        }
        // ::Solid Colors::
        // outsideColor:
        else if (x == 0 || y == 0 || y == size_z - 1 || x == size_x - 1)
        {
            p = new Color[texInfos[0].texColors[0].Length];
            for (int i = 0; i < texInfos[0].texColors[0].Length; i++)
            {
                //p[i] = new Color(cardInfo.outsideColorRGB[0], cardInfo.outsideColorRGB[1], cardInfo.outsideColorRGB[2], 1);
                p[i] = cardInfo.outsideColor;
            }
        }
        // insideColor:
        else
        {
            p = new Color[texInfos[0].texColors[0].Length];
            for (int i = 0; i < texInfos[0].texColors[0].Length; i++)
            {
                //p[i] = new Color(cardInfo.insideColorRGB[0], cardInfo.insideColorRGB[1], cardInfo.insideColorRGB[2], 1);
                p[i] = cardInfo.insideColor;
            }
        }

        return p;
    }
}

