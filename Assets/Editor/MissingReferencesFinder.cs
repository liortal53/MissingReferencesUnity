using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MissingReferencesFinder : MonoBehaviour 
{
	[MenuItem("Tools/Show Missing Object References in scene", false, 50)]
	public static void FindMissingReferencesInCurrentScene()
	{
		var objects = GetSceneObjects();
		FindMissingReferences(EditorApplication.currentScene, objects);
	}

	[MenuItem("Tools/Show Missing Object References in all scenes", false, 51)]
	public static void MissingSpritesInAllScenes()
	{
		foreach (var scene in EditorBuildSettings.scenes)
		{
			EditorApplication.OpenScene(scene.path);
			FindMissingReferences(scene.path, Resources.FindObjectsOfTypeAll<GameObject>());
		}
	}

	[MenuItem("Tools/Show Missing Object References in assets", false, 52)]
	public static void MissingSpritesInAssets()
	{
		var allAssets = AssetDatabase.GetAllAssetPaths();
		var objs = allAssets.Select(a => AssetDatabase.LoadAssetAtPath(a, typeof(GameObject)) as GameObject).Where(a => a != null).ToArray();
		
		FindMissingReferences("Project", objs);
	}

	private static void FindMissingReferences(string context, GameObject[] objects)
	{
		foreach (var go in objects)
		{
			var components = go.GetComponents<Component>();
			
			foreach (var c in components)
			{
				if (!c)
				{
					Debug.LogError("Missing Component in GO: " + FullPath(go), go);
					continue;
				}
				
				SerializedObject so = new SerializedObject(c);
				var sp = so.GetIterator();
				
				while (sp.NextVisible(true))
				{
					if (sp.propertyType == SerializedPropertyType.ObjectReference)
					{
						if (sp.objectReferenceValue == null
						    && sp.objectReferenceInstanceIDValue != 0)
						{
							ShowError(context, go, c.GetType().Name, ObjectNames.NicifyVariableName(sp.name));
						}
					}
				}
			}
		}
	}

	private static GameObject[] GetSceneObjects()
	{
		return Resources.FindObjectsOfTypeAll<GameObject>()
			.Where(go => string.IsNullOrEmpty(AssetDatabase.GetAssetPath(go))
			       && go.hideFlags == HideFlags.None).ToArray();
	}
	
	private const string err = "Missing Ref in: [{3}]{0}. Component: {1}, Property: {2}";
	
	private static void ShowError (string context, GameObject go, string c, string property)
	{
		Debug.LogError(string.Format(err, FullPath(go), c, property, context), go);
	}
	
	private static string FullPath(GameObject go)
	{
		return go.transform.parent == null
			? go.name
				: FullPath(go.transform.parent.gameObject) + "/" + go.name;
	}
}