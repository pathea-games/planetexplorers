using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

// This implementation is copied from ZauberCraft and mod to gen terrain like ZauberCraft.
public class PerlinNoise
{
	const int YSTEP = 16;
	const int YSTEP2 = 32;
	const int YSTEP3 = 48;
	const int YSHIFT = 4;
	const int ZSTEP = 256;
	const int ZSTEP2 = 512;
	const int ZSTEP3 = 768;
	const int ZSHIFT = 8;
	
	[MethodImpl(MethodImplOptions.NoInlining)]	
	static float Lerp (float Y0, float Y1, float Y2, float Y3, float Blend)
	{
		float A0 = Y3 - Y2 - Y0 + Y1;
		float A1 = Y0 - Y1 - A0;
		float A2 = Y2 - Y0;
		float A3 = Y1;
	
		float BlendSq = Blend * Blend;

		return A0 * Blend * BlendSq + A1 * BlendSq + A2 * Blend + A3;
	}
	
	private float[] Randoms = new float[0x10000];
	
	public PerlinNoise()			{	Init();		}
	public PerlinNoise(int seed)	{	Init(seed);	}
	
	private void Init()
	{
		System.Random rand = new System.Random();
		for(int i = 0; i < 0x10000; i++)
		{
			Randoms[i] = (float)(rand.NextDouble()*2 - 1);	//[-1,1)
		}
	}
	private void Init(int seed)
	{
		System.Random rand = new System.Random(seed);
		for(int i = 0; i < 0x10000; i++)
		{
			Randoms[i] = (float)(rand.NextDouble()*2 - 1);	//[-1,1)
		}
	}	
	public float Noise1DFBM (float X, int Octaves)
	{
		float Result = 0.0f;
		float Amplitude = 0.5f;
	
		if (X < 0.0f)
			X = -X;
	
		int IntX = (int)X;
		float FracX = X - IntX;
	
		for (int I = 0; I < Octaves; I++) {
			float Y0 = Randoms [(IntX) & 0xFFFF];
			float Y1 = Randoms [(IntX + 1) & 0xFFFF];
			float Y2 = Randoms [(IntX + 2) & 0xFFFF];
			float Y3 = Randoms [(IntX + 3) & 0xFFFF];
		
			Result += Lerp (Y0, Y1, Y2, Y3, FracX) * Amplitude;
		
			Amplitude *= 0.5f;
			IntX <<= 1;
			FracX *= 2.0f;
		
			if (FracX > 1.0f) {
				IntX++;
				FracX -= 1.0f;
			}
		}
	
		return Result;
	}

	public float Noise2DFBM (float X, float Y, int Octaves)
	{
		float Result = 0.0f;
		float Amplitude = 0.5f;
	
		int IntX = (int)X;
		int IntY = (int)Y;
		if (X < 0.0f)			IntX--;
		if (Y < 0.0f)			IntY--;
		float FracX = X - IntX;
		float FracY = Y - IntY;
	
		for (int I = 0; I < Octaves; I++) {
			int Offset = IntX + (IntY << YSHIFT);

			float X0 = Lerp (Randoms [(Offset) & 0xFFFF], Randoms [(Offset + 1) & 0xFFFF], Randoms [(Offset + 2) & 0xFFFF], Randoms [(Offset + 3) & 0xFFFF], FracX);
			float X1 = Lerp (Randoms [(Offset + YSTEP) & 0xFFFF], Randoms [(Offset + 1 + YSTEP) & 0xFFFF], Randoms [(Offset + 2 + YSTEP) & 0xFFFF], Randoms [(Offset + 3 + YSTEP) & 0xFFFF], FracX);
			float X2 = Lerp (Randoms [(Offset + YSTEP2) & 0xFFFF], Randoms [(Offset + 1 + YSTEP2) & 0xFFFF], Randoms [(Offset + 2 + YSTEP2) & 0xFFFF], Randoms [(Offset + 3 + YSTEP2) & 0xFFFF], FracX);
			float X3 = Lerp (Randoms [(Offset + YSTEP3) & 0xFFFF], Randoms [(Offset + 1 + YSTEP3) & 0xFFFF], Randoms [(Offset + 2 + YSTEP3) & 0xFFFF], Randoms [(Offset + 3 + YSTEP3) & 0xFFFF], FracX);
		
			Result += Lerp (X0, X1, X2, X3, FracY) * Amplitude;
		
			Amplitude *= 0.5f;
			IntX <<= 1;
			IntY <<= 1;
			FracX *= 2.0f;
			FracY *= 2.0f;
		
			if (FracX > 1.0f) {
				IntX++;
				FracX -= 1.0f;
			}

			if (FracY > 1.0f) {
				IntY++;
				FracY -= 1.0f;
			}
		}
	
		return Result;
	}

	public float Noise3DFBM (float X, float Y, float Z, int Octaves)
	{
		float Result = 0.0f;
		float Amplitude = 0.5f;
	
		int IntX = (int)X;
		int IntY = (int)Y;
		int IntZ = (int)Z;
		if (X < 0.0f)			IntX--;
		if (Y < 0.0f)			IntY--;
		if (Z < 0.0f)			IntZ--;		
		float FracX = X - IntX;
		float FracY = Y - IntY;
		float FracZ = Z - IntZ;
	
		for (int I = 0; I < Octaves; I++) {
			int Offset = IntX + (IntY << YSHIFT) + (IntZ << ZSHIFT);

			float X0 = Lerp (Randoms [(Offset) & 0xFFFF], Randoms [(Offset + 1) & 0xFFFF], Randoms [(Offset + 2) & 0xFFFF], Randoms [(Offset + 3) & 0xFFFF], FracX);
			float X1 = Lerp (Randoms [(Offset + YSTEP) & 0xFFFF], Randoms [(Offset + 1 + YSTEP) & 0xFFFF], Randoms [(Offset + 2 + YSTEP) & 0xFFFF], Randoms [(Offset + 3 + YSTEP) & 0xFFFF], FracX);
			float X2 = Lerp (Randoms [(Offset + YSTEP2) & 0xFFFF], Randoms [(Offset + 1 + YSTEP2) & 0xFFFF], Randoms [(Offset + 2 + YSTEP2) & 0xFFFF], Randoms [(Offset + 3 + YSTEP2) & 0xFFFF], FracX);
			float X3 = Lerp (Randoms [(Offset + YSTEP3) & 0xFFFF], Randoms [(Offset + 1 + YSTEP3) & 0xFFFF], Randoms [(Offset + 2 + YSTEP3) & 0xFFFF], Randoms [(Offset + 3 + YSTEP3) & 0xFFFF], FracX);

			Offset += ZSTEP;

			float X4 = Lerp (Randoms [(Offset) & 0xFFFF], Randoms [(Offset + 1) & 0xFFFF], Randoms [(Offset + 2) & 0xFFFF], Randoms [(Offset + 3) & 0xFFFF], FracX);
			float X5 = Lerp (Randoms [(Offset + YSTEP) & 0xFFFF], Randoms [(Offset + 1 + YSTEP) & 0xFFFF], Randoms [(Offset + 2 + YSTEP) & 0xFFFF], Randoms [(Offset + 3 + YSTEP) & 0xFFFF], FracX);
			float X6 = Lerp (Randoms [(Offset + YSTEP2) & 0xFFFF], Randoms [(Offset + 1 + YSTEP2) & 0xFFFF], Randoms [(Offset + 2 + YSTEP2) & 0xFFFF], Randoms [(Offset + 3 + YSTEP2) & 0xFFFF], FracX);
			float X7 = Lerp (Randoms [(Offset + YSTEP3) & 0xFFFF], Randoms [(Offset + 1 + YSTEP3) & 0xFFFF], Randoms [(Offset + 2 + YSTEP3) & 0xFFFF], Randoms [(Offset + 3 + YSTEP3) & 0xFFFF], FracX);

			Offset += ZSTEP;

			float X8 = Lerp (Randoms [(Offset) & 0xFFFF], Randoms [(Offset + 1) & 0xFFFF], Randoms [(Offset + 2) & 0xFFFF], Randoms [(Offset + 3) & 0xFFFF], FracX);
			float X9 = Lerp (Randoms [(Offset + YSTEP) & 0xFFFF], Randoms [(Offset + 1 + YSTEP) & 0xFFFF], Randoms [(Offset + 2 + YSTEP) & 0xFFFF], Randoms [(Offset + 3 + YSTEP) & 0xFFFF], FracX);
			float X10 = Lerp (Randoms [(Offset + YSTEP2) & 0xFFFF], Randoms [(Offset + 1 + YSTEP2) & 0xFFFF], Randoms [(Offset + 2 + YSTEP2) & 0xFFFF], Randoms [(Offset + 3 + YSTEP2) & 0xFFFF], FracX);
			float X11 = Lerp (Randoms [(Offset + YSTEP3) & 0xFFFF], Randoms [(Offset + 1 + YSTEP3) & 0xFFFF], Randoms [(Offset + 2 + YSTEP3) & 0xFFFF], Randoms [(Offset + 3 + YSTEP3) & 0xFFFF], FracX);

			Offset += ZSTEP;

			float X12 = Lerp (Randoms [(Offset) & 0xFFFF], Randoms [(Offset + 1) & 0xFFFF], Randoms [(Offset + 2) & 0xFFFF], Randoms [(Offset + 3) & 0xFFFF], FracX);
			float X13 = Lerp (Randoms [(Offset + YSTEP) & 0xFFFF], Randoms [(Offset + 1 + YSTEP) & 0xFFFF], Randoms [(Offset + 2 + YSTEP) & 0xFFFF], Randoms [(Offset + 3 + YSTEP) & 0xFFFF], FracX);
			float X14 = Lerp (Randoms [(Offset + YSTEP2) & 0xFFFF], Randoms [(Offset + 1 + YSTEP2) & 0xFFFF], Randoms [(Offset + 2 + YSTEP2) & 0xFFFF], Randoms [(Offset + 3 + YSTEP2) & 0xFFFF], FracX);
			float X15 = Lerp (Randoms [(Offset + YSTEP3) & 0xFFFF], Randoms [(Offset + 1 + YSTEP3) & 0xFFFF], Randoms [(Offset + 2 + YSTEP3) & 0xFFFF], Randoms [(Offset + 3 + YSTEP3) & 0xFFFF], FracX);
		
			float Y0 = Lerp (X0, X1, X2, X3, FracY);
			float Y1 = Lerp (X4, X5, X6, X7, FracY);
			float Y2 = Lerp (X8, X9, X10, X11, FracY);
			float Y3 = Lerp (X12, X13, X14, X15, FracY);

			Result += Lerp (Y0, Y1, Y2, Y3, FracZ) * Amplitude;
		
			Amplitude *= 0.5f;
			IntX <<= 1;
			IntY <<= 1;
			IntZ <<= 1;
			FracX *= 2.0f;
			FracY *= 2.0f;
			FracZ *= 2.0f;
		
			if (FracX > 1.0f) {
				IntX++;
				FracX -= 1.0f;
			}

			if (FracY > 1.0f) {
				IntY++;
				FracY -= 1.0f;
			}

			if (FracZ > 1.0f) {
				IntZ++;
				FracZ -= 1.0f;
			}
		}
	
		return Result;
	}
}
