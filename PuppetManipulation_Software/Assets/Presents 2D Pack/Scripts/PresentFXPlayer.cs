using UnityEngine;
using System.Collections;

public class PresentFXPlayer : MonoBehaviour
{
    [SerializeField]
    Transform effectsRoot;

    [SerializeField]
    FX[] idleFX, breathFX, jumpFX, openFX, explodeFX;
    
    public void PlayIdleFX() { PlayFX(idleFX); }
    public void PlayBreathFX() { PlayFX(breathFX); }
    public void PlayJumpFX() { PlayFX(jumpFX); }
    public void PlayOpenFX() { PlayFX(openFX); }
    public void PlayExplodeFX() { PlayFX(explodeFX); }

    void PlayFX(FX[] fxs)
    {
        if (fxs == null || fxs.Length == 0) return;

        foreach (FX fx in fxs)
        {
            if (fx.fx == null) continue;

            ParticleSystem newParticle = Instantiate(fx.fx);
            newParticle.transform.position += this.transform.position;
            if (effectsRoot != null)
            {
                newParticle.transform.SetParent(effectsRoot);
                newParticle.transform.rotation = newParticle.transform.rotation * effectsRoot.rotation;
                newParticle.transform.localPosition = fx.Offset;
                newParticle.Play();
            }
            else
            {
                newParticle.transform.SetParent(this.transform);
                newParticle.transform.rotation = newParticle.transform.rotation * this.transform.rotation;
                newParticle.transform.localPosition = fx.Offset;
                newParticle.Play();
            }
            
            
        }
    }
}

[System.Serializable]
public class FX
{
    public ParticleSystem fx;
    public Vector3 Offset;
}
