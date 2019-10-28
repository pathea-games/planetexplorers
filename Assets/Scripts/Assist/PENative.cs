using UnityEngine;
using System.Collections;
using Pathea;

public class PENative : MonoBehaviour
{
    [SerializeField]
    NativeProfession profession;
    [SerializeField]
    NativeSex sex;
    [SerializeField]
    NativeAge age;

    public NativeProfession Profession
    {
        get { return profession; }
        set { profession = value; }
    }

    public NativeSex Sex { get { return sex; } }
    public NativeAge Age { get { return age; } }
}
