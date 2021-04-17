using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi{
	
	/**
	 * Custom inspector for ObiRope components.
	 * Allows particle selection and constraint edition. 
	 * 
	 * Selection:
	 * 
	 * - To select a particle, left-click on it. 
	 * - You can select multiple particles by holding shift while clicking.
	 * - To deselect all particles, click anywhere on the object except a particle.
	 * 
	 * Constraints:
	 * 
	 * - To edit particle constraints, select the particles you wish to edit.
	 * - Constraints affecting any of the selected particles will appear in the inspector.
	 * - To add a new pin constraint to the selected particle(s), click on "Add Pin Constraint".
	 * 
	 */
	[CustomEditor(typeof(ObiSoftbodySkinner)), CanEditMultipleObjects] 
	public class ObiSoftbodySkinnerEditor : Editor
	{
		
		ObiSoftbodySkinner skinner;
		protected IEnumerator routine;
		
		public void OnEnable(){
			skinner = (ObiSoftbodySkinner)target;
		}
		
		public void OnDisable(){
			EditorUtility.ClearProgressBar();
		}

		private void BakeMesh(){

			SkinnedMeshRenderer skin = skinner.GetComponent<SkinnedMeshRenderer>();

			if (skin != null && skin.sharedMesh != null){

				Mesh baked = new Mesh();
				skin.BakeMesh(baked);

				ObiEditorUtils.SaveMesh(baked,"Save extruded mesh","rope mesh",false,true);
			}
		}

        protected void NonReadableMeshWarning(Mesh mesh)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            Texture2D icon = EditorGUIUtility.Load("icons/console.erroricon.png") as Texture2D;
            EditorGUILayout.LabelField(new GUIContent("The renderer mesh is not readable. Read/Write must be enabled in the mesh import settings.", icon), EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Fix now", GUILayout.MaxWidth(100), GUILayout.MinHeight(32)))
            {
                string assetPath = AssetDatabase.GetAssetPath(mesh);
                ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (modelImporter != null)
                {
                    modelImporter.isReadable = true;
                }
                modelImporter.SaveAndReimport();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        protected bool ValidateRendererMesh()
        {
            SkinnedMeshRenderer skin = skinner.GetComponent<SkinnedMeshRenderer>();

            if (skin != null && skin.sharedMesh != null)
            {
                if (!skin.sharedMesh.isReadable)
                {
                    NonReadableMeshWarning(skin.sharedMesh);
                    return false;
                }
                return true;
            }
            return false;
        }

        public override void OnInspectorGUI() {
			
			serializedObject.Update();		

			GUI.enabled = skinner.Source != null && ValidateRendererMesh();
            if (GUILayout.Button("Bind skin")){

					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
					CoroutineJob job = new CoroutineJob();
					routine = job.Start(skinner.BindSkin());
					EditorCoroutine.ShowCoroutineProgressBar("Binding to particles...",ref routine);
					EditorGUIUtility.ExitGUI();

			}
			if (GUILayout.Button("Bake Mesh")){
				BakeMesh();
			}
			GUI.enabled = true;

			if (skinner.Source == null){
				EditorGUILayout.HelpBox("No source softbody present.",MessageType.Info);
			}

			skinner.Source = EditorGUILayout.ObjectField("Source softbody",skinner.Source, typeof(ObiSoftbody), true) as ObiSoftbody;

			Editor.DrawPropertiesExcluding(serializedObject,"m_Script");
			
			// Apply changes to the serializedProperty
			if (GUI.changed){
				serializedObject.ApplyModifiedProperties();
			}
			
		}
	}
}


