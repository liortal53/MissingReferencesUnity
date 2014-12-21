using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MissingReferencesFinder : MonoBehaviour 
{
	[MenuItem("Tools/Find Missing references in scene", false, 50)]
	public static void FindMissingReferences()
	{
		var objects = GetSceneObjects();
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
							ShowError(go, c.GetType().Name, sp.name);
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
	
	private const string err = "Missing Ref in: {0}. Component: {1}, Property: {2}";
	
	private static void ShowError (GameObject go, string c, string p)
	{
		Debug.LogError(string.Format(err, FullPath(go), c, p), go);
	}
	
	private static string FullPath(GameObject go)
	{
		return go.transform.parent == null
			? go.name
				: FullPath(go.transform.parent.gameObject) + "/" + go.name;
	}
}