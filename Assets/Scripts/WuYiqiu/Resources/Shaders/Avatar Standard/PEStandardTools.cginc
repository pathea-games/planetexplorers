
#include "UnityCG.cginc"

half CaclDepthAlpha(float depth)
{	
//	return pow( clamp(depth * 1.4, 0, 1), 8);
return  clamp(pow( depth * 1.2 , 4.5), 0, 1);
}