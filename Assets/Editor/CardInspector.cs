using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(Card))]
public class CardInspector : Editor {
	
	public override void OnInspectorGUI() {
		//base.OnInspectorGUI();
		DrawDefaultInspector();
		
		if(GUILayout.Button("Create")) {			
			Mesh mesh = MeshBuilder.BuildMesh(new Mesh(), 8, 12);
			Card card = (Card)target;
            card.createTextureEditor(mesh);
            //card.setTexture(GameLogic.instance.cardLayout.createTexture(card.cardTileInfo));
        }
	}
}