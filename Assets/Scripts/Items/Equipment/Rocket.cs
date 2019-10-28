using UnityEngine;
using System.Collections;
using SkillAsset;

public class Rocket : ShootEquipment
{
    public float coolDown;
    public Transform muzzle;
    public Transform muzzleParticle;

    float lastFireTime;

    void PlayMuzzleEffect()
    {
        if (muzzleParticle != null)
        {
            ParticleSystem[] particles = muzzleParticle.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particle in particles)
            {
                particle.Play();
            }
        }
    }

    bool CanFire()
    {
        return Time.time - lastFireTime >= coolDown;
    }

    public bool IsAimiAt(Vector3 target)
    {
        if (muzzle != null && CanFire())
        {
            float angle = Vector3.Angle(target - muzzle.position, muzzle.forward);
            return angle < 5;
        }

        return false;
    }

    public override bool CostSkill(SkillAsset.ISkillTarget target = null, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
    {
        if (mSkillMaleId.Count == 0 || mSkillFemaleId.Count == 0)
            return false;
        if (!base.CostSkill(target, sex, buttonDown, buttonPressed))
        {
            return false;
        }

        int castSkillId = 0;

        switch (sex)
        {
            case 1:
                castSkillId = mSkillFemaleId[0];
                for (int i = 0; i < mSkillFemaleId.Count - 1; i++)
                    if (mSkillRunner.IsEffRunning(mSkillFemaleId[i]))
                        castSkillId = mSkillFemaleId[i + 1];
                break;
            case 2:
                castSkillId = mSkillMaleId[0];
                for (int i = 0; i < mSkillMaleId.Count - 1; i++)
                    if (mSkillRunner.IsEffRunning(mSkillMaleId[i]))
                        castSkillId = mSkillMaleId[i + 1];
                break;
        }

        EffSkillInstance inst = CostSkill(mSkillRunner, castSkillId, target);

        if (inst != null)
        {
            lastFireTime = Time.time;
            PlayMuzzleEffect();
        }

        return inst != null;
    }
}
