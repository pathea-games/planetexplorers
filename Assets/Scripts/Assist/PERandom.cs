using UnityEngine;
using System;
using System.Collections;
using Random = System.Random;

namespace PETools
{
    public class PERandom
    {
        static int Seed = 100;

        static Random s_Seed = new Random(Seed);

        static Random _BehaveSeed;

        static int[] s_BehaveSeeds = new int[] { 12, 45, 89, 56, 4164, 89898 };

        public static Random BehaveSeed
        {
            get
            {
                if (_BehaveSeed == null)
                {
                    _BehaveSeed = new Random(s_BehaveSeeds[s_Seed.Next(0, s_BehaveSeeds.Length)]);
                }

                return _BehaveSeed;
            }
        }
    }
}
