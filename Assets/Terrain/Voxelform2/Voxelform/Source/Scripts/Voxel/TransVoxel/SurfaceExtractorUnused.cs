using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Transvoxel.Lengyel;
using System.Diagnostics;

#if false

namespace Transvoxel.SurfaceExtractor
{
	public interface ISurfaceExtractor
	{
		Mesh GenLodCell(WorldChunk<sbyte> n);
	}

	public class TransvoxelExtractor : ISurfaceExtractor
	{
		public bool UseCache { get; set; }
		IVolumeData<sbyte> volume;
		RegularCellCache cache;

		public TransvoxelExtractor(IVolumeData<sbyte> data)
		{
			volume = data;
			cache = new RegularCellCache(volume.ChunkSize*10);
			UseCache = true;
		}

		public Mesh GenLodCell(WorldChunk<sbyte> chunk)
		{
			Mesh mesh = new Mesh();
			int lod = 1;

			for (int x = 0; x < volume.ChunkSize; x++)
			{
				for (int y = 0; y < volume.ChunkSize; y++)
				{
					for (int z = 0; z < volume.ChunkSize; z++)
					{
						Vector3i position; //new Vector3i(x, y, z);
						position.X = x;
						position.Y = y;
						position.Z = z;
						PolygonizeCell(chunk.GetPosition(), position, ref mesh, lod);
					}
				}
			}

			return mesh;
		}

		internal void PolygonizeCell(IntVector3 offsetPos, IntVector3 pos, ref Mesh mesh, int lod)
		{
			Debug.Assert(lod >= 1, "Level of Detail must be greater than 1");
			offsetPos += pos * lod;

			byte directionMask = (byte)((pos.x > 0 ? 1 : 0) | ((pos.z > 0 ? 1 : 0) << 1) | ((pos.y > 0 ? 1 : 0) << 2));

			sbyte[] density = new sbyte[8];

			for (int i = 0; i < density.Length; i++)
			{
				density[i] = volume[offsetPos + Tables.CornerIndex[i] * lod];
			}

			byte caseCode = getCaseCode(density);
			if ((caseCode ^ ((density[7] >> 7) & 0xFF)) == 0) //for this cases there is no triangulation
				return;

			Vector3f[] cornerNormals = new Vector3f[8];
			for (int i = 0; i < 8; i++)
			{
				var p = offsetPos + Tables.CornerIndex[i] * lod;
				float nx = (volume[p + Vector3i.UnitX] - volume[p - Vector3i.UnitX]) * 0.5f;
				float ny = (volume[p + Vector3i.UnitY] - volume[p - Vector3i.UnitY]) * 0.5f;
				float nz = (volume[p + Vector3i.UnitZ] - volume[p - Vector3i.UnitZ]) * 0.5f;
				//cornerNormals[i] = new Vector3f(nx, ny, nz);

				cornerNormals[i].x = nx;
				cornerNormals[i].y = ny;
				cornerNormals[i].z = nz;
				cornerNormals[i].Normalize();
			}

			byte regularCellClass = Tables.RegularCellClass[caseCode];
			ushort[] vertexLocations = Tables.RegularVertexData[caseCode];

			Tables.RegularCell c = Tables.RegularCellData[regularCellClass];
			long vertexCount = c.GetVertexCount();
			long triangleCount = c.GetTriangleCount();
			byte[] indexOffset = c.Indizes(); //index offsets for current cell
			ushort[] mappedIndizes = new ushort[indexOffset.Length]; //array with real indizes for current cell

			for (int i = 0; i < vertexCount; i++)
			{
				byte edge = (byte)(vertexLocations[i] >> 8);
				byte reuseIndex = (byte)(edge & 0xF); //Vertex id which should be created or reused 1,2 or 3
				byte rDir = (byte)(edge >> 4); //the direction to go to reach a previous cell for reusing 

				byte v1 = (byte)((vertexLocations[i]) & 0x0F); //Second Corner Index
				byte v0 = (byte)((vertexLocations[i] >> 4) & 0x0F); //First Corner Index

				sbyte d0 = density[v0];
				sbyte d1 = density[v1];

				//Vector3f n0 = cornerNormals[v0];
				//Vector3f n1 = cornerNormals[v1];

				Debug.Assert(v1 > v0);

				int t = (d1 << 8) / (d1 - d0);
				int u = 0x0100 - t;
				float t0 = t / 256f;
				float t1 = u / 256f;

				int index = -1;

				if (UseCache && v1 != 7 && (rDir & directionMask) == rDir)
				{
					Debug.Assert(reuseIndex != 0);
					ReuseCell cell = cache.GetReusedIndex(pos, rDir);
					index = cell.Verts[reuseIndex];
				}

				if (index == -1)
				{
					Vector3f normal = cornerNormals[v0] * t0 + cornerNormals[v1] * t1;
					GenerateVertex(ref offsetPos, ref pos, mesh, lod, t, ref v0, ref v1, ref d0, ref d1, normal);
					index = mesh.LatestAddedVertIndex();
				}

				if ((rDir & 8) != 0)
				{
					cache.SetReusableIndex(pos, reuseIndex, mesh.LatestAddedVertIndex());
				}

				mappedIndizes[i] = (ushort)index;
			}

			for (int t = 0; t < triangleCount; t++)
			{
				for (int i = 0; i < 3; i++)
				{
					mesh.AddIndex(mappedIndizes[c[t * 3 + i]]);
				}
			}
		}

		private void GenerateVertex(ref IntVector3 offsetPos, ref IntVector3 pos, Mesh mesh, int lod, long t, ref byte v0, ref byte v1, ref sbyte d0, ref sbyte d1, Vector3 normal)
		{
			IntVector3 iP0 = (offsetPos + Tables.CornerIndex[v0] * lod);
			Vector3 P0;// = new Vector3f(iP0.X, iP0.Y, iP0.Z);
			P0.x = iP0.x;
			P0.y = iP0.y;
			P0.z = iP0.z;

			IntVector3 iP1 = (offsetPos + Tables.CornerIndex[v1] * lod);
			Vector3 P1;// = new Vector3f(iP1.X, iP1.Y, iP1.Z);
			P1.x = iP1.x;
			P1.y = iP1.y;
			P1.z = iP1.z;

			//EliminateLodPositionShift(lod, ref d0, ref d1, ref t, ref iP0, ref P0, ref iP1, ref P1);


			Vector3 Q = InterpolateVoxelVector(t, P0, P1);

			mesh.AddVertex(Q, normal);
		}

		private void EliminateLodPositionShift(int lod, ref sbyte d0, ref sbyte d1, ref long t, ref IntVector3 iP0, ref Vector3 P0, ref IntVector3 iP1, ref Vector3 P1)
		{
			for (int k = 0; k < lod - 1; k++)
			{
				Vector3 vm = (P0 + P1) / 2.0f;
				IntVector3 pm = (iP0 + iP1) / 2;
				sbyte sm = volume[pm];

				if ((d0 & 0x8F) != (d1 & 0x8F))
				{
					P1 = vm;
					iP1 = pm;
					d1 = sm;
				}
				else
				{
					P0 = vm;
					iP0 = pm;
					d0 = sm;
				}
			}

			if (d1 == d0) // ?????????????
				return;
			t = (d1 << 8) / (d1 - d0); // recalc
		}

		/*private static ReuseCell getReuseCell(ReuseCell[, ,] cells, int lod, byte rDir, Vector3i pos)
		{
			int rx = rDir & 0x01;
			int rz = (rDir >> 1) & 0x01;
			int ry = (rDir >> 2) & 0x01;

			int dx = pos.X / lod - rx;
			int dy = pos.Y / lod - ry;
			int dz = pos.Z / lod - rz;

			ReuseCell ccc = cells[dx, dy, dz];
			return ccc;
		}*/

		internal static Vector3 InterpolateVoxelVector(long t, Vector3 P0, Vector3 P1)
		{
			long u = 0x0100 - t; //256 - t
			float s = 1.0f / 256.0f;
			Vector3 Q = P0 * t + P1 * u; //Density Interpolation
			Q *= s; // shift to shader ! 
			return Q;
		}

	   /* internal void mapIndizes2Vertice(int vertexNr, ushort index, ushort[] mappedIndizes, byte[] indexOffset)
		{
			for (int j = 0; j < mappedIndizes.Length; j++)
			{
				if (vertexNr == indexOffset[j])
				{
					mappedIndizes[j] = index;
				}
			}
		}*/

		private static byte getCaseCode(sbyte[] density)
		{
			byte code = 0;
			byte konj = 0x01;
			for (int i = 0; i < density.Length; i++)
			{
				code |= (byte)((density[i] >> (density.Length - 1 - i)) & konj);
				konj <<= 1;
			}

			return code;
		}
	}
}
#endif