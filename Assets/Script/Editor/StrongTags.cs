using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StrongTags : MonoBehaviour {
    public static string TagPath = "Tags.cs";
    public static string LayerPath = "Layers.cs";

    private static double nextTick = 0;

    private static string[] lastLayers;
    private static string[] lastTags;

    [InitializeOnLoadMethod]
	static void Generate () {

        EditorApplication.update += Update;

        nextTick = EditorApplication.timeSinceStartup + 1.0;
        lastLayers = new string[32];
        lastTags = new string[] { };
    }
    
	static void Update () {
        if (EditorApplication.timeSinceStartup < nextTick)
            return;

        var tags = UnityEditorInternal.InternalEditorUtility.tags;
        if (CompareStringArray(lastTags, tags) == false)
        {
            var script = GenerateTag(tags);
            File.WriteAllText(Application.dataPath + "/" + TagPath, script);
            lastTags = tags;
        }

        var layers = new string[32];
        for (int i = 0; i < 32; i++)
            layers[i] = LayerMask.LayerToName(i);
        if (CompareStringArray(lastLayers, layers) == false)
        {
            var script = GenerateLayers(layers);
            File.WriteAllText(Application.dataPath + "/" + LayerPath, script);
            lastLayers = layers;
        }

        nextTick = EditorApplication.timeSinceStartup + 1.0;
    }

    static bool CompareStringArray(string[] a, string[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }
    static string GenerateTag(string[] tags)
    {
        var script = "public class Tags {\r\n";

        foreach (var tag in tags)
        {
            if (string.IsNullOrEmpty(tag)) continue;
            script += "    public static readonly string " + tag.Replace(" ", "") + " = \"" + tag + "\";\r\n";
        }

        script += "}";

        return script;
    }
    static string GenerateLayers(string[] layers)
    {
        var script = "[System.Flags]\r\npublic enum Layers {\r\n";

        var cnt = 0;
        foreach (var layer in layers)
        {
            if (string.IsNullOrEmpty(layer) == false)
                script += "    " + layer.Replace(" ", "") + " = " + cnt + ",\r\n";
            cnt++;
        }

        script += "}";

        return script;
    }
}
