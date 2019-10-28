//#define DELTA_ENABLE
#define INDICES_ENABLE	// This will make multimaterial info which is stored in norm2t unavailable

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Transvoxel.Lengyel;
using Transvoxel.SurfaceExtractor;

namespace Transvoxel.SurfaceExtractor
{
    internal static class TransvoxelExtractor2
    {
        public const int ChunkWidth = 32;
        public const int BlockWidth = 32;
        public const int Primary = 0;
        public const int Secondary = 1;
        public static readonly Vector3 Unused = new Vector3(1000, 1000, 1000);
		private const float S = 1.0f / 256.0f;
		private const byte IsoLevel = 128;

        private static byte HiNibble(byte b)	{	return (byte)(((b) >> 4) & 0x0F);	}
        private static byte LoNibble(byte b)	{	return (byte)(b & 0x0F);        	}
        private static int Sign(sbyte b)		{	return (b >> 7) & 1;				}

        private static Vector3 ComputeDelta(Vector3 v, int k, int s)
        {
            if (k < 1)		return Vector3.zero;
			
            float p2k = (float)(1<<k); 	// Pow(2,k) ---- k is lod index
            float wk = p2k * 0.25f; 	// Pow(2,k-2)--- the width of transitions cells for LOD index k
			float[] d = {0.0f, 0.0f, 0.0f};	// delta
			float[] p = {v.x, v.y, v.z};
			for(int i = 0; i < 3; i++)
			{
	            if (p[i] < p2k)
	            {
	                // The vertex is inside the minimum cell.
	                d[i] = (1.0f - p[i]/p2k) * wk;
	            }
	            else if (p[i] > (p2k * (s - 1)))
	            {
	                // The vertex is inside the maximum cell.
	                d[i] = ((p2k * s) - 1.0f - p[i]) * wk;
	            }
			}

            return new Vector3(d[0], d[1], d[2]);
        }

        private static Vector3 ProjectNormal(Vector3 n, Vector3 delta)
        {
            //return Vector3f.Cross(n, delta);
            var mat = new Matrix3X3(
                   1.0f - n.x * n.x, -n.x * n.y, -n.x * n.z,
                   -n.x * n.y, 1.0f - n.y * n.y, -n.y * n.z,
                   -n.x * n.z, -n.y * n.z, 1.0f - n.z * n.z);
            return mat * delta;
        }

        private static IntVector3 PrevOffset(byte dir)
        {
            return new IntVector3(-(dir & 1),
                                    -((dir >> 1) & 1),
                                    -((dir >> 2) & 1));
        }
		
		public static int deltaCnt = 0;
        public static int PolygonizeTransitionCell(
			int x, int y, 			// The x and y position of the cell within the block face.
			int dirIndex,			// ref to TransitionFaceCoords
			float cellSize,			// The width of a cell in world scale.
            VFVoxelChunkData chunkData, 
            TransVertices verts, List<int> indices,	// output verts' pos is relative to cur chunk
            TransitionCache cache)
        {
			int lod2Sample = chunkData.LOD;
			int sampleStep = 1;// << lod2Sample;
			int spacing = 1 << 1;	// Spacing between low-res corners.
			int scale = 1<<lod2Sample;
			IntVector3 xAxis = Tables.TransitionFaceCoords[dirIndex,0];
			IntVector3 yAxis = Tables.TransitionFaceCoords[dirIndex,1];
			IntVector3 zAxis = Tables.TransitionFaceCoords[dirIndex,2];
			IntVector3 axisExtend = Tables.TransitionFaceCoords[dirIndex,4]; // mesh width.
			IntVector3 relOrigin = Tables.TransitionFaceCoords[dirIndex,3]*ChunkWidth + xAxis * (x*spacing) + yAxis * (y*spacing); // Origin in sample space(current cell).
			
			// Rotate to change coordinate
            Matrix3X3 basis = new Matrix3X3(xAxis*sampleStep, yAxis*sampleStep, zAxis*sampleStep);
            IntVector3[] relPos = {
	            relOrigin + basis * Tables.TransitionCornerCoords[0x00], relOrigin + basis * Tables.TransitionCornerCoords[0x01], relOrigin + basis * Tables.TransitionCornerCoords[0x02],
	            relOrigin + basis * Tables.TransitionCornerCoords[0x03], relOrigin + basis * Tables.TransitionCornerCoords[0x04], relOrigin + basis * Tables.TransitionCornerCoords[0x05],
	            relOrigin + basis * Tables.TransitionCornerCoords[0x06], relOrigin + basis * Tables.TransitionCornerCoords[0x07], relOrigin + basis * Tables.TransitionCornerCoords[0x08],
	            relOrigin + basis * Tables.TransitionCornerCoords[0x09], relOrigin + basis * Tables.TransitionCornerCoords[0x0A],
	            relOrigin + basis * Tables.TransitionCornerCoords[0x0B], relOrigin + basis * Tables.TransitionCornerCoords[0x0C]
	        };
		
			// Compute case code(indexing as described in Figure4.17)
			VFVoxel[] voxels = {
				chunkData[relPos[0]],chunkData[relPos[1]],chunkData[relPos[2]],
				chunkData[relPos[3]],chunkData[relPos[4]],chunkData[relPos[5]],
				chunkData[relPos[6]],chunkData[relPos[7]],chunkData[relPos[8]],
			};
			uint caseCode = (uint)((voxels[0].Volume >> 7) & 0x001 |
                                   (voxels[1].Volume >> 6) & 0x002 |
                                   (voxels[2].Volume >> 5) & 0x004 |
                                   (voxels[5].Volume >> 4) & 0x008 |
                                   (voxels[8].Volume >> 3) & 0x010 |
                                   (voxels[7].Volume >> 2) & 0x020 |
                                   (voxels[6].Volume >> 1) & 0x040 |
                                   (voxels[3].Volume     ) & 0x080 |
                                   (voxels[4].Volume << 1) & 0x100);
            if (caseCode == 0 || caseCode == 511)
                return 0;
			
            cache[x, y].CaseIndex = (byte)caseCode;
			byte[] vols = {
				voxels[0].Volume,voxels[1].Volume,voxels[2].Volume,
				voxels[3].Volume,voxels[4].Volume,voxels[5].Volume,
				voxels[6].Volume,voxels[7].Volume,voxels[8].Volume,
				voxels[0].Volume,voxels[2].Volume,voxels[6].Volume,voxels[8].Volume
			};
			byte[] types = {
				voxels[0].Type,voxels[1].Type,voxels[2].Type,
				voxels[3].Type,voxels[4].Type,voxels[5].Type,
				voxels[6].Type,voxels[7].Type,voxels[8].Type,
				voxels[0].Type,voxels[2].Type,voxels[6].Type,voxels[8].Type
			};						
			// Compute normal based on volumes
            Vector3[] normals = new Vector3[13];
            for (int i = 0; i < 9; i++)
            {
                IntVector3 p = relPos[i];
                float nx = p.x >= 1 ? (chunkData[p + IntVector3.UnitX].Volume - chunkData[p - IntVector3.UnitX].Volume) : (chunkData[p + IntVector3.UnitX].Volume-vols[i]);
                float ny = p.y >= 1 ? (chunkData[p + IntVector3.UnitY].Volume - chunkData[p - IntVector3.UnitY].Volume) : (chunkData[p + IntVector3.UnitY].Volume-vols[i]);
                float nz = p.z >= 1 ? (chunkData[p + IntVector3.UnitZ].Volume - chunkData[p - IntVector3.UnitZ].Volume) : (chunkData[p + IntVector3.UnitZ].Volume-vols[i]);
                normals[i] = new Vector3(nx, ny, nz);
                //normals[i].Normalize();
            }
            normals[0x9] = normals[0];
            normals[0xA] = normals[2];
            normals[0xB] = normals[6];
            normals[0xC] = normals[8];
			
			// Compute which of the six faces of the block that the vertex 
            // is near. (near is defined as being in boundary cell.)
			byte near = 0;
            if (relOrigin.x ==          0){ near |= (byte)(1 << 0); }// Vertex close to negativeX face.
            if (relOrigin.x == ChunkWidth){ near |= (byte)(1 << 1); }// Vertex close to positiveX face.
            if (relOrigin.y ==          0){ near |= (byte)(1 << 2); }// Vertex close to negativeY face.
            if (relOrigin.y == ChunkWidth){ near |= (byte)(1 << 3); }// Vertex close to positiveY face.
            if (relOrigin.z ==          0){ near |= (byte)(1 << 4); }// Vertex close to negativeZ face.
            if (relOrigin.z == ChunkWidth){ near |= (byte)(1 << 5); }// Vertex close to positiveZ face.
			
			byte directionMask = (byte)((x > 0 ? 1 : 0) | ((y > 0 ? 1 : 0) << 1)); // Used to determine which previous cells that are available.on edge, dirmask will be cut
            byte classIndex = Tables.TransitionCellClass[caseCode]; // Equivalence class index.
            var data = Tables.TransitionRegularCellData[classIndex & 0x7F];
            bool inverse = (classIndex & 0x80) != 0;
            int[] localVertexMapping = new int[12];	//TransitionRegularCellData's hinibble means vertex count(max is 0x0c)

            int nv = (int)data.GetVertexCount();
            int nt = (int)data.GetTriangleCount();
            for (int i = 0; i < nv; i++)
            {
				// HiByte:reuse data shown in Figure 4.18; LoByte:2 end points shown in Figure 4.16
                ushort edgeCode = Tables.TransitionVertexData[caseCode][i];
				byte pointCode = (byte)edgeCode;
				byte reuseCode = (byte)(edgeCode >> 8);
				// v0, v1: 2 end points. v0<v1
                byte v0 = HiNibble(pointCode);
                byte v1 = LoNibble(pointCode);
                //Vector3 n0 = normals[v0];
                //Vector3 n1 = normals[v1];
                byte d0 = vols[v0];
                byte d1 = vols[v1];
                int t = ((IsoLevel - d0)<<8) / (d1 - d0);
                int u = 0x0100 - t;
				float t0 = u * S;
                float t1 = t * S;
		
				byte v_ = 0;
				byte dir,idx;
				bool bLowSide,bAddCache,bCorner;
                if ((t & 0x00ff) != 0)
				{
                    // Use the reuse information in transitionVertexData, shown in Figure 4.18
					// directionMask is voxel's dir in current processing planar: (byte)((x > 0 ? 1 : 0) | ((y > 0 ? 1 : 0) << 1))
					// dir is voxel's dir in current cell
					dir = HiNibble(reuseCode);
					idx = LoNibble(reuseCode);
					bLowSide = ((v0 > 8) && (v1 > 8));
					bAddCache = (dir & 8) != 0;
					bCorner = false;
				}
				else
				{
                    // Try to reuse corner vertex from a preceding cell.
                    // Use the reuse information in transitionCornerData.
                    v_ = t == 0 ? v0 : v1;
                    byte cornerData = Tables.TransitionCornerData[v_];
                    dir = HiNibble(cornerData);
                    idx = LoNibble((cornerData));
					bLowSide = v_ > 8;
					bAddCache = true;
					bCorner = true;
				}
                bool present = (dir & directionMask) == dir;	// dir is 1 or 2 && not a edge voxel, then the verts is available
                if (present)
                {
                    // The previous cell is available. Retrieve the cached cell 
                    // from which to retrieve the reused vertex index from.
                    var prev = cache[x - (dir & 1), y - ((dir >> 1) & 1)];
                    if (prev.CaseIndex == 0 || prev.CaseIndex == 511)
                    {
                        // Previous cell does not contain any geometry.
                        localVertexMapping[i] = -1;
                    }
                    else
                    {
                        // Reuse the vertex index from the previous cell.
                        localVertexMapping[i] = prev.Verts[idx];
                    }
                }
                if (!present || localVertexMapping[i] < 0)
                {
					localVertexMapping[i] = verts.Count;
                    if (bAddCache)	// The vertex can be reused.
                    {   
                        cache[x, y].Verts[idx] = localVertexMapping[i];
                    }
					
					verts.IsLowside.Add(bLowSide);	// half resolution side
					byte typev_ = types[v_];
					byte typev0 = types[v0];
					byte typev1 = types[v1];
					if(typev_ == 0 || typev0 == 0 || typev1 == 0) // type 0 will gen black tri
					{
						if(typev_ == 0) typev_ = typev0;
						if(typev_ == 0) typev_ = typev0 = typev1;
						if(typev_ == 0) typev_ = typev0 = typev1 = 1;
						else{	
							if(typev0 == 0) typev0 = typev_;
							if(typev1 == 0) typev1 = typev_;
						}
					}
					byte curType = bCorner ? typev_ : (t0 < 0.5f ? typev1 : typev0);
					Vector3 vNormal = normals[v1]*t1 + normals[v0]*t0;
					verts.Normal_t.Add(new Vector4(vNormal.x/256.0f, vNormal.y/256.0f, vNormal.z/256.0f, curType));
					Vector3 pi = bCorner ? (Vector3)relPos[v_] : 
						((Vector3)relPos[v1])*t1 + ((Vector3)relPos[v0])*t0;
                    if (bLowSide)
                    {
						// Variant algo for PE's lod data
                        // Necessary to translate the intersection point to the 
                        // high-res side so that it is transformed the same way 
                        // as the vertices in the regular cell.
						Vector3 offset = Vector3.zero;
                        switch (axisExtend.x)
                        {
                            case 0:
								offset.x = axisExtend.y*cellSize;
                                pi.x = (float)(relOrigin.x);
                                break;
                            case 1:
								offset.y = axisExtend.y*cellSize;
                                pi.y = (float)(relOrigin.y);
                                break;
                            case 2:
								offset.z = axisExtend.y*cellSize;
                                pi.z = (float)(relOrigin.z);
                                break;
                        }
#if DELTA_ENABLE
						deltaCnt ++;
						if(deltaCnt == 17)
							deltaCnt = 17;
                        Vector3 delta = ComputeDelta(pi, lodIndex, ChunkWidth);
                        Vector3 proj = ProjectNormal(vert.Normal, delta);
						verts.Near.Add(near);
                        verts.Position.Add((offset + pi + proj)*scale);
#else
						verts.Near.Add(near);
                        verts.Position.Add((offset + pi)*scale);
#endif
					}
                    else
                    {
                        // On high-resolution side.
                        verts.Near.Add(0); // Vertices on high-res side are never moved.
                        verts.Position.Add(pi*scale);
                    }
                }
            }

            for (int t = 0; t < nt; ++t)
            {
                if (inverse)
                {
                    indices.Add(localVertexMapping[data[t * 3 + 0]]);
                    indices.Add(localVertexMapping[data[t * 3 + 1]]);
                    indices.Add(localVertexMapping[data[t * 3 + 2]]);
                }		
                else	
                {		
                    indices.Add(localVertexMapping[data[t * 3 + 2]]);
                    indices.Add(localVertexMapping[data[t * 3 + 1]]);
                    indices.Add(localVertexMapping[data[t * 3 + 0]]);
                }
            }

            return nt;
        }

		// for reference:
	    public static void generateNegativeXTransitionCells(
	        VFVoxelChunkData chunkData,
	        float cellSize,		
	        TransVertices verts,
	        List<int> indices)
	    {
			int dirIndex = 1;
	        TransitionCache cache = new TransitionCache();
	
	        for (int y = 0; y < 16; ++y)	// random map only 64 unit height
	        {
	            for (int x = 0; x < 16; ++x)
	            {
	                PolygonizeTransitionCell(x, y, dirIndex, cellSize, chunkData, verts, indices, cache);
	            }
	        }
	    }
		public static void BuildTransitionCells(int faceMask, VFVoxelChunkData chunkData,
										        float cellSize,
										        TransVertices verts, List<int> indices)
		{
			for(int i = 0; i < 6; i++)
			{
				if(0 == (faceMask&(1<<i)))	continue;
				
				int dirIndex = i;
				TransitionCache cache = new TransitionCache();
				
				int len = 16; 
				for (int y = 0; y < len; ++y)	// random map only 64 unit height
				{
					for (int x = 0; x < len; ++x)
					{
	                	PolygonizeTransitionCell(x, y, dirIndex, cellSize, chunkData, verts, indices, cache);
					}
				}
	        }			
		}
	}
}
