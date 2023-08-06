using UnityEngine;

namespace LunarApostles
{
  [RequireComponent(typeof(MeshFilter))]
  public class ReverseNormals : MonoBehaviour
  {
    private void Start()
    {
      MeshFilter component = GetComponent(typeof(MeshFilter)) as MeshFilter;
      if (component != null)
      {
        Mesh mesh = component.mesh;
        Vector3[] normals = mesh.normals;
        for (int index = 0; index < normals.Length; ++index)
          normals[index] = -normals[index];
        mesh.normals = normals;
        for (int submesh = 0; submesh < mesh.subMeshCount; ++submesh)
        {
          int[] triangles = mesh.GetTriangles(submesh);
          for (int index = 0; index < triangles.Length; index += 3)
            (triangles[index + 1], triangles[index]) = (triangles[index], triangles[index + 1]);
          mesh.SetTriangles(triangles, submesh);
        }
      }
      GetComponent<MeshCollider>().sharedMesh = component.mesh;
    }
  }
}
