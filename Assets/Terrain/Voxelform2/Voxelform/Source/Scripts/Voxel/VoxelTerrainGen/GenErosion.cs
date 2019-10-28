using UnityEngine;
using System.Collections;

public class GenErosion {
	float[,] hmAverage;
	float[,] heightmap;
	public int HMWidth;
	public int HMHeight;
	public int NumDrops = 1;
	public GenErosion(int w, int h, float[,] _heightmap)
	{
		HMWidth = w;
		HMHeight = h;
		heightmap = _heightmap;
		
	}
	public void ApplyErosion()
	{
		int range = HMWidth - avgfilterSize * 2;
		
		int total = 1000;
		int root = Mathf.RoundToInt( Mathf.Sqrt ((float)total));
		int unitLength = Mathf.FloorToInt(range / root);
		for(int iter = 0; iter < total; iter++)
		{
			
			int lX = Mathf.FloorToInt(Random.value * range - 10 * 2) + 10;
			int lY = Mathf.FloorToInt(Random.value * range - 10 * 2) + 10;
			lX = (iter % root) * unitLength;
			lY = iter / root * unitLength;
			lX += 10;
			lY += 10;
			
			DropletX = lX;
			DropletY = lY;
			velocity = 1.0f;
			int cnt = 0;
			while(velocity > 0.001f)
			{
				//initAverageMap();
				FlowDroplet();
				cnt++;
			}
			
		}
	}
	int DropletX, DropletY;
	float velocity = 0.0f;
	
	float carriedSoil;
	float maxCapacity;
	
	int[] GradientDirection = new int[8]{
		1,0,-1,0,
		0,1,0,-1,
	};
	public int avgfilterSize = 2;
	float sampleAverageHeight(int x, int y)
	{
		float sum = 0.0f;
		for(int ay = 1; ay < avgfilterSize; ay++)
		{
			for(int ax = 1; ax < avgfilterSize; ax++)
			{
				sum += heightmap[x + ax, y + ay];
				sum += heightmap[x + ax, y - ay];
				
				sum += heightmap[x - ax, y + ay];
				sum += heightmap[x - ax, y - ay];
			}
		}
		for(int ax = 0; ax < avgfilterSize; ax++)
		{
			sum += heightmap[x + ax, y];
			sum += heightmap[x - ax, y];
			
			sum += heightmap[x, y + ax];
			sum += heightmap[x, y - ax];
		}
		sum -= heightmap[x, y] * 3;
		sum /= Mathf.Pow(avgfilterSize * 2 - 1, 2.0f);
		
		return sum;
	}
	public void initAverageMap()
	{
		
		hmAverage = new float[HMWidth, HMHeight];
		for(int y = avgfilterSize; y < HMHeight - avgfilterSize; y++ )
		{
			for(int x = avgfilterSize; x < HMWidth - avgfilterSize; x++)
			{
				float sum = 0.0f;
				for(int ay = 1; ay < avgfilterSize; ay++)
				{
					for(int ax = 1; ax < avgfilterSize; ax++)
					{
						sum += heightmap[x + ax, y + ay];
						sum += heightmap[x + ax, y - ay];
						
						sum += heightmap[x - ax, y + ay];
						sum += heightmap[x - ax, y - ay];
					}
				}
				for(int ax = 0; ax < avgfilterSize; ax++)
				{
					sum += heightmap[x + ax, y];
					sum += heightmap[x - ax, y];
					
					sum += heightmap[x, y + ax];
					sum += heightmap[x, y - ax];
				}
				sum -= heightmap[x, y] * 3;
				sum /= Mathf.Pow(avgfilterSize * 2 - 1, 2.0f);
				
				hmAverage[x, y] = sum;
			}
			
		}
		
	}
	float[,] GaussianCoeffs = new float[5,5]
	{
		{0.00079f, 0.0066f, 0.0133f, 0.0066f, 0.00079f},
		{0.0066f, 0.055f, 0.111f,  0.055f, 0.0066f},
		{0.0133f, 0.111f, 0.2251f, 0.111f, 0.0133f},
		{0.0066f, 0.055f, 0.111f,  0.055f, 0.0066f},
		{0.00079f, 0.0066f, 0.0133f, 0.0066f, 0.00079f}
	};
	void ApplyFilter(int x, int y, float strength)
	{
		for(int iy = -2; iy < 3; iy++)
		{
			for(int ix = -2; ix < 3; ix++)
			{
				heightmap[x + ix, y + iy] += strength * GaussianCoeffs[ix + 2, iy + 2];
			}
		}
	}
	void FlowDroplet()
	{
		int stepSize = 1;
		// determine which way to go
		if(DropletX < 2 + stepSize|| DropletX >= HMWidth - 2 - stepSize ||
			DropletY < 2 + stepSize || DropletY >= HMHeight - 2 - stepSize)
		{
			velocity = 0.0f;
			return;
		}
		float[] heightDiff = new float[4];
		int flowDirectionX = 0;
		int flowDirectionY = 0;
		int directionI = -1;
		float steepest = 0.0f;
		
		float baseHeight = sampleAverageHeight(DropletX, DropletY);
		for(int i = 0; i < 4; i++ )
		{
			heightDiff[i] = baseHeight - sampleAverageHeight(DropletX + GradientDirection[i*2] * stepSize, DropletY + GradientDirection[i*2 + 1] * stepSize);
			//heightDiff[i] = hmAverage[DropletX, DropletY] - hmAverage[DropletX + GradientDirection[i*2] * stepSize, DropletY + GradientDirection[i*2 + 1] * stepSize];
			if(heightDiff[i] > steepest && heightDiff[i] > 0.0f)
			{
				steepest = heightDiff[i];
				directionI = i;
				flowDirectionX = GradientDirection[i*2];
				flowDirectionY = GradientDirection[i*2 + 1];
				
			}
		}
		
		//float newHeight = hmAverage[DropletX + flowDirectionX, DropletY + flowDirectionY];
		if(directionI < 0)
		{
			// uphill
			velocity = 0.0f;
			
		}
		else
		{
			float oldHeight = baseHeight;
			float newHeight = heightDiff[directionI];
			if(newHeight < oldHeight - 0.1f)
			{
				// downhill
				//velocity += (newHeight - oldHeight);
				
				// carry some soil
				float carryAmount = -0.3f;
				if(oldHeight + carryAmount * 2> newHeight)
				{
					ApplyFilter(DropletX, DropletY, carryAmount);
					carriedSoil += carryAmount;
				}
				// drop some sediment
			}
			
		}
		
		DropletX += flowDirectionX;
		DropletY += flowDirectionY;
		
		
		
	}
}
