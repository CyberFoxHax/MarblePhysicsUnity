// References:
// P-Invoke for strings:
//   http://stackoverflow.com/questions/370079/pinvoke-for-c-function-that-returns-char/370519#370519
//
// Note:
// This script must execute before other classes can be used. Make sure the execution is prior
// to the default time.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class Dif : MonoBehaviour {

	public string filePath;
	public Material DefaultMaterial;

    public bool ClickToGenerate = false;
    public bool ClickToCreateObj = false;

    public void Awake() {
        DefaultMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
    }

    private void Update() {
        if (ClickToGenerate) {
            ClickToGenerate = false;
            GenerateMesh();
        }
        if (ClickToCreateObj) {
            ClickToCreateObj = false;
            CreateObj();
        }
    }

    public void CreateObj() {
        var fullDifPath = Path.Combine(Application.streamingAssetsPath, filePath);
        var dif = DifResourceManager.getResource(fullDifPath);
        if (dif == null) {
            Debug.LogError("Dif decode failed");
            return;
        }

        var stringBuildler = new System.Text.StringBuilder();

        var numberFormat = "0.####";

        stringBuildler.AppendLine("## Dif to Obj encoder, by the marble dumbfucks who never give up ##");
        stringBuildler.AppendLine("mtllib " + Path.GetFileNameWithoutExtension(fullDifPath) + ".mtl");

        for (var i = 0; i < dif.vertices.Length; i++) {
            stringBuildler.AppendLine("v " +
                (-dif.vertices[i].x).ToString(numberFormat) + " " +
                (-dif.vertices[i].y).ToString(numberFormat) + " " +
                dif.vertices[i].z.ToString(numberFormat)
            );
        }
        stringBuildler.AppendLine();
        for (var i = 0; i < dif.normals.Length; i++) {
            stringBuildler.AppendLine("vn " +
                (-dif.normals[i].x).ToString(numberFormat) + " " +
                (-dif.normals[i].y).ToString(numberFormat) + " " +
                dif.normals[i].z.ToString(numberFormat)
            );
        }
        stringBuildler.AppendLine();
        for (var i = 0; i < dif.uvs.Length; i++) {
            stringBuildler.AppendLine("vt " +
                dif.uvs[i].x.ToString(numberFormat) + " " +
                dif.uvs[i].y.ToString(numberFormat)
            );
        }
        stringBuildler.AppendLine();
        stringBuildler.AppendLine();
        
		for (int i = 0; i < dif.triangleIndices.Length; i++) {
            stringBuildler.AppendLine();
            stringBuildler.AppendLine("usemtl " + dif.materials[i]);
            for (int ii = 0; ii < dif.triangleIndices[i].Length; ii+=3) {
                var i0 = dif.triangleIndices[i][ii+0]+1;
                var i1 = dif.triangleIndices[i][ii+1]+1;
                var i2 = dif.triangleIndices[i][ii+2]+1;
                stringBuildler.AppendLine($"f {i2}/{i2}/{i2} {i1}/{i1}/{i1} {i0}/{i0}/{i0}");
            }
		}

        File.WriteAllText(Path.Combine(Application.dataPath, Path.GetFileNameWithoutExtension(fullDifPath) + ".obj"), stringBuildler.ToString());
        File.WriteAllText(Path.Combine(Application.dataPath, Path.GetFileNameWithoutExtension(fullDifPath) + ".mtl"), 
            string.Join("\r\n", dif.materials.Distinct().Select(p=>
                "newmtl "+p+
@"
    Ka 1 1 1
    Kd 1 1 1
    Ks 1 1 1
    d 1
    illum 4
"
            ))
        );
        AssetDatabase.ImportAsset("Assets/" + Path.GetFileNameWithoutExtension(fullDifPath) + ".obj");
        AssetDatabase.ImportAsset("Assets/" + Path.GetFileNameWithoutExtension(fullDifPath) + ".mtl");
        var asset = AssetDatabase.LoadAssetAtPath<Object>("Assets/" + Path.GetFileNameWithoutExtension(fullDifPath) + ".obj");
        EditorGUIUtility.PingObject(asset);
    }

	public void GenerateMesh()
	{
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
		var resource = DifResourceManager.getResource(Path.Combine(Application.streamingAssetsPath, filePath));
        if (resource == null) {
            Debug.LogError("Dif decode failed");
            return;
        }


		// create mesh!
		Mesh mesh = new Mesh();
        mesh.name = Path.GetFileNameWithoutExtension(filePath);
		mesh.vertices = resource.vertices.Select(p=>new Vector3(p.x,-p.y, p.z)).ToArray();
		mesh.normals = resource.normals;
		mesh.uv = resource.uvs;
		mesh.tangents = resource.tangents;

		mesh.subMeshCount = resource.triangleIndices.Length;
		for (int i = 0; i < resource.triangleIndices.Length; i++) {
			mesh.SetTriangles(resource.triangleIndices[i], i);
		}

		Material[] materials = new Material[resource.materials.Length];
		for (var index = 0; index < resource.materials.Length; index++)
		{
			var material = ResolveTexturePath(resource.materials[index]);
			materials[index] = DefaultMaterial;

			if (File.Exists(material))
			{
				byte[] fileData = File.ReadAllBytes(material);
				var tex = new Texture2D(2, 2);
				tex.LoadImage(fileData); // This will auto-resize the texture dimensions.
				materials[index] = Instantiate(DefaultMaterial);
				materials[index].mainTexture = tex;
			}
		}

		mesh.Optimize();
		GetComponent<MeshFilter>().mesh = mesh;
		if (GetComponent<MeshCollider>())
			GetComponent<MeshCollider>().sharedMesh = mesh;
		if (GetComponent<MeshRenderer>())
			GetComponent<MeshRenderer>().materials = materials;
	}

	private string ResolveTexturePath(string texture)
	{
		var basePath = Path.GetDirectoryName(filePath);
		while (!string.IsNullOrEmpty(basePath))
		{
			var assetPath = Path.Combine(Application.streamingAssetsPath, basePath);
			var possibleTextures = new List<string>
			{
				Path.Combine(assetPath, texture + ".png"),
				Path.Combine(assetPath, texture + ".jpg"),
				Path.Combine(assetPath, texture + ".jp2"),
				Path.Combine(assetPath, texture + ".bmp"),
				Path.Combine(assetPath, texture + ".bm8"),
				Path.Combine(assetPath, texture + ".gif"),
				Path.Combine(assetPath, texture + ".dds"),
			};
			foreach (var possibleTexture in possibleTextures)
			{
				if (File.Exists(possibleTexture))
				{
					return possibleTexture;
				}
			}

			basePath = Path.GetDirectoryName(basePath);
		}

		return texture;
	}
}
#endif
