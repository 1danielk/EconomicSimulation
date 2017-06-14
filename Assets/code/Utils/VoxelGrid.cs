﻿using UnityEngine;
using System.Collections.Generic;

[SelectionBase]
public class VoxelGrid : MonoBehaviour
{

    public int resolution;

    public GameObject voxelPrefab;

    public VoxelGrid xNeighbor, yNeighbor, xyNeighbor;

    private Voxel[] voxels;

    private float voxelSize, gridSize;

    private Material[] voxelMaterials;

    private MeshStructure mesh;

    //private List<Vector3> vertices;
    //private List<int> triangles;

    private Voxel dummyX, dummyY, dummyT;
    private Color analyzingColor;
    public void Initialize(int resolution, float size, Texture2D texture, Color color)
    {
        this.resolution = resolution;
        gridSize = size;
        voxelSize = size / resolution;
        voxels = new Voxel[resolution * resolution];
        voxelMaterials = new Material[voxels.Length];

        dummyX = new Voxel();
        dummyY = new Voxel();
        dummyT = new Voxel();

        analyzingColor = color;

        for (int i = 0, y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++, i++)
            {
                CreateVoxel(i, x, y, texture.GetPixel(x, y));
            }
        }

        //GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh = new MeshStructure();
        //mesh.name = "VoxelGrid Mesh";
        //vertices = new List<Vector3>();
        //triangles = new List<int>();
        Refresh();
    }
    public MeshStructure getMesh()
    {
        return mesh;
    }
    private void CreateVoxel(int i, int x, int y, Color state)
    {
        //GameObject o = Instantiate(voxelPrefab) as GameObject;
        //o.transform.parent = transform;
        //o.transform.localPosition = new Vector3((x + 0.5f) * voxelSize, (y + 0.5f) * voxelSize, -0.01f);
        //o.transform.localScale = Vector3.one * voxelSize * 0.1f;
        //voxelMaterials[i] = o.GetComponent<MeshRenderer>().material;
        voxels[i] = new Voxel(x, y, voxelSize, state);

    }

    private void Refresh()
    {
        //SetVoxelColors();
        Triangulate();
    }

    private void Triangulate()
    {
        
        //mesh.Clear();

        if (xNeighbor != null)
        {
            dummyX.BecomeXDummyOf(xNeighbor.voxels[0], gridSize);
        }
        TriangulateCellRows();
        if (yNeighbor != null)
        {
            TriangulateGapRow();
        }

        //mesh.vertices = vertices.ToArray();
        //mesh.triangles = triangles.ToArray();
    }

    private void TriangulateCellRows()
    {
        int cells = resolution - 1;
        for (int i = 0, y = 0; y < cells; y++, i++)
        {
            for (int x = 0; x < cells; x++, i++)
            {
                TriangulateCell(
                    voxels[i],
                    voxels[i + 1],
                    voxels[i + resolution],
                    voxels[i + resolution + 1]);
            }
            if (xNeighbor != null)
            {
                TriangulateGapCell(i);
            }
        }
    }

    private void TriangulateGapCell(int i)
    {
        Voxel dummySwap = dummyT;
        dummySwap.BecomeXDummyOf(xNeighbor.voxels[i + 1], gridSize);
        dummyT = dummyX;
        dummyX = dummySwap;
        TriangulateCell(voxels[i], dummyT, voxels[i + resolution], dummyX);
    }

    private void TriangulateGapRow()
    {
        dummyY.BecomeYDummyOf(yNeighbor.voxels[0], gridSize);
        int cells = resolution - 1;
        int offset = cells * resolution;

        for (int x = 0; x < cells; x++)
        {
            Voxel dummySwap = dummyT;
            dummySwap.BecomeYDummyOf(yNeighbor.voxels[x + 1], gridSize);
            dummyT = dummyY;
            dummyY = dummySwap;
            TriangulateCell(voxels[x + offset], voxels[x + offset + 1], dummyT, dummyY);
        }

        if (xNeighbor != null)
        {
            dummyT.BecomeXYDummyOf(xyNeighbor.voxels[0], gridSize);
            TriangulateCell(voxels[voxels.Length - 1], dummyX, dummyY, dummyT);
        }
    }

    private void TriangulateCell(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        int cellType = 0;
        if (a.state == analyzingColor)
        {
            cellType |= 1;
        }
        if (b.state == analyzingColor)
        {
            cellType |= 2;
        }
        if (c.state == analyzingColor)
        {
            cellType |= 4;
        }
        if (d.state == analyzingColor)
        {
            cellType |= 8;
        }
        switch (cellType)
        {
            case 0:
                return;
            case 1:
                AddTriangle(a.position, a.yEdgePosition, a.xEdgePosition);
                break;
            case 2:
                AddTriangle(b.position, a.xEdgePosition, b.yEdgePosition);
                break;
            case 3:
                AddQuad(a.position, a.yEdgePosition, b.yEdgePosition, b.position);
                break;
            case 4:
                AddTriangle(c.position, c.xEdgePosition, a.yEdgePosition);
                break;
            case 5:
                AddQuad(a.position, c.position, c.xEdgePosition, a.xEdgePosition);
                break;
            case 6:
                AddTriangle(b.position, a.xEdgePosition, b.yEdgePosition);
                AddTriangle(c.position, c.xEdgePosition, a.yEdgePosition);
                break;
            case 7:
                AddPentagon(a.position, c.position, c.xEdgePosition, b.yEdgePosition, b.position);
                break;
            case 8:
                AddTriangle(d.position, b.yEdgePosition, c.xEdgePosition);
                break;
            case 9:
                AddTriangle(a.position, a.yEdgePosition, a.xEdgePosition);
                AddTriangle(d.position, b.yEdgePosition, c.xEdgePosition);
                break;
            case 10:
                AddQuad(a.xEdgePosition, c.xEdgePosition, d.position, b.position);
                break;
            case 11:
                AddPentagon(b.position, a.position, a.yEdgePosition, c.xEdgePosition, d.position);
                break;
            case 12:
                AddQuad(a.yEdgePosition, c.position, d.position, b.yEdgePosition);
                break;
            case 13:
                AddPentagon(c.position, d.position, b.yEdgePosition, a.xEdgePosition, a.position);
                break;
            case 14:
                AddPentagon(d.position, b.position, a.xEdgePosition, a.yEdgePosition, c.position);
                break;
            case 15:
                AddQuad(a.position, c.position, d.position, b.position);
                break;
        }
        //detecting 3 color connecting
        if (is3ColorCornerUp(a, b, c, d) && a.state == analyzingColor)
            AddTriangle(c.xEdgePosition, b.yEdgePosition, a.yEdgePosition);

        if (is3ColorCornerDown(a, b, c, d) && c.state == analyzingColor)
            AddTriangle(b.yEdgePosition, a.xEdgePosition, a.yEdgePosition);

        if (is3ColorCornerLeft(a, b, c, d) && c.state == analyzingColor)
            AddTriangle(c.xEdgePosition, b.yEdgePosition, a.xEdgePosition);

        if (is3ColorCornerRight(a, b, c, d) && d.state == analyzingColor)
            AddTriangle(a.yEdgePosition, c.xEdgePosition, a.xEdgePosition);

        if (a.state != b.state && a.state != c.state && a.state != d.state
            && b.state != c.state && b.state != d.state
            && c.state != d.state
            && a.state == analyzingColor)
            AddQuad(a.yEdgePosition, c.xEdgePosition, b.yEdgePosition, a.xEdgePosition);
    }
    private bool is3ColorCornerUp(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        return a.state == b.state && a.state != c.state && a.state != d.state && c.state != d.state;
    }
    private bool is3ColorCornerDown(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        return c.state == d.state && c.state != a.state && c.state != b.state && a.state != b.state;
    }
    private bool is3ColorCornerLeft(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        return c.state == a.state && c.state != d.state && a.state != b.state && d.state != b.state;
    }
    private bool is3ColorCornerRight(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        return d.state == b.state && d.state != c.state && b.state != a.state && c.state != a.state;
    }
    private void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        int vertexIndex = mesh.vertices.Count;
        mesh.vertices.Add(a);
        mesh.vertices.Add(b);
        mesh.vertices.Add(c);
        mesh.triangles.Add(vertexIndex);
        mesh.triangles.Add(vertexIndex + 1);
        mesh.triangles.Add(vertexIndex + 2);
    }

    private void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        int vertexIndex = mesh.vertices.Count;
        mesh.vertices.Add(a);
        mesh.vertices.Add(b);
        mesh.vertices.Add(c);
        mesh.vertices.Add(d);
        mesh.triangles.Add(vertexIndex);
        mesh.triangles.Add(vertexIndex + 1);
        mesh.triangles.Add(vertexIndex + 2);
        mesh.triangles.Add(vertexIndex);
        mesh.triangles.Add(vertexIndex + 2);
        mesh.triangles.Add(vertexIndex + 3);
    }

    private void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
    {
        int vertexIndex = mesh.vertices.Count;
        mesh.vertices.Add(a);
        mesh.vertices.Add(b);
        mesh.vertices.Add(c);
        mesh.vertices.Add(d);
        mesh.vertices.Add(e);
        mesh.triangles.Add(vertexIndex);
        mesh.triangles.Add(vertexIndex + 1);
        mesh.triangles.Add(vertexIndex + 2);
        mesh.triangles.Add(vertexIndex);
        mesh.triangles.Add(vertexIndex + 2);
        mesh.triangles.Add(vertexIndex + 3);
        mesh.triangles.Add(vertexIndex);
        mesh.triangles.Add(vertexIndex + 3);
        mesh.triangles.Add(vertexIndex + 4);
    }

    private void SetVoxelColors()
    {
        for (int i = 0; i < voxels.Length; i++)
        {
          //  voxelMaterials[i].color = voxels[i].state ? Color.black : Color.white;
        }
    }

    //public void Apply(VoxelStencil stencil)
    //{
    //    int xStart = stencil.XStart;
    //    if (xStart < 0)
    //    {
    //        xStart = 0;
    //    }
    //    int xEnd = stencil.XEnd;
    //    if (xEnd >= resolution)
    //    {
    //        xEnd = resolution - 1;
    //    }
    //    int yStart = stencil.YStart;
    //    if (yStart < 0)
    //    {
    //        yStart = 0;
    //    }
    //    int yEnd = stencil.YEnd;
    //    if (yEnd >= resolution)
    //    {
    //        yEnd = resolution - 1;
    //    }

    //    for (int y = yStart; y <= yEnd; y++)
    //    {
    //        int i = y * resolution + xStart;
    //        for (int x = xStart; x <= xEnd; x++, i++)
    //        {
    //            voxels[i].state = stencil.Apply(x, y, voxels[i].state);
    //        }
    //    }
    //    Refresh();
    //}
}