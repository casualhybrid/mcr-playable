using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="ParticleCueEventChannel" ,menuName = "ScriptableObjects/Particles/ParticleCueEventChannel")]
public class ParticleChannelEventSO  : ScriptableObject
{
	public delegate void ParticleCueRequestedHandler(List<InstanceIDUnique> _particlesSpawned, ParticleCueSO particleCue, Transform theTransform, Vector3 transformRelativePosition, bool onlyAssignParentPos);
	public event ParticleCueRequestedHandler OnParticleCueRequested;

	public void RaiseEvent(List<InstanceIDUnique> _particlesSpawned, ParticleCueSO particleCue, Transform theTransform, Vector3 transformRelativePosition, bool onlyAssignParentPos)
	{
		if (OnParticleCueRequested != null)
		{
			OnParticleCueRequested.Invoke(_particlesSpawned, particleCue, theTransform, transformRelativePosition, onlyAssignParentPos);
		}
		else
		{
            UnityEngine.Console.LogWarning("An particleCue was requested, but nobody picked it up .Check why there is no ParticleManager already loaded and make sure it's listening on this ParticleCue Event channel");
        }
	}
}