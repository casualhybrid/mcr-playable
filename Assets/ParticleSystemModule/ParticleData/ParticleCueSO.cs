using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ParticleCueSO", menuName = "ScriptableObjects/Particles/ParticleCueSO")]
public class ParticleCueSO : ScriptableObject
{
    [SerializeField] private ParticlesGroup[] _particleGroups = default;

    public GameObject[] GetParticles()
    {
        int numberOfParticles = _particleGroups.Length;
        GameObject[] resultingParticles = new GameObject[numberOfParticles];

        for (int i = 0; i < numberOfParticles; i++)
        {
            resultingParticles[i] = _particleGroups[i].GetNextParticle();
        }

        return resultingParticles;
    }
}

/// <summary>
/// Represents a group of AudioClips that can be treated as one, and provides automatic randomisation or sequencing based on the <c>SequenceMode</c> value.
/// </summary>
[Serializable]
public class ParticlesGroup
{
    public SequenceMode sequenceMode = SequenceMode.RandomNoImmediateRepeat;
    public GameObject[] particles;

    private int _nextParticleToPlay = -1;
    private int _lastParticlePlayed = -1;

    /// <summary>
    /// Chooses the next particle in the sequence, either following the order or randomly.
    /// </summary>
    /// <returns>A reference to a Particle</returns>
    public GameObject GetNextParticle()
    {
        // Fast out if there is only one particle to play
        if (particles.Length == 1)
            return particles[0];

        if (_nextParticleToPlay == -1)
        {
            // Index needs to be initialised: 0 if Sequential, random if otherwise
            _nextParticleToPlay = (sequenceMode == SequenceMode.Sequential) ? 0 : UnityEngine.Random.Range(0, particles.Length);
        }
        else
        {
            // Select next clip index based on the appropriate SequenceMode
            switch (sequenceMode)
            {
                case SequenceMode.Random:
                    _nextParticleToPlay = UnityEngine.Random.Range(0, particles.Length);
                    break;

                case SequenceMode.RandomNoImmediateRepeat:
                    do
                    {
                        _nextParticleToPlay = UnityEngine.Random.Range(0, particles.Length);
                    } while (_nextParticleToPlay == _lastParticlePlayed);
                    break;

                case SequenceMode.Sequential:
                    _nextParticleToPlay = (int)Mathf.Repeat(++_nextParticleToPlay, particles.Length);
                    break;
            }
        }

        _lastParticlePlayed = _nextParticleToPlay;

        return particles[_nextParticleToPlay];
    }

    public enum SequenceMode
    {
        Random,
        RandomNoImmediateRepeat,
        Sequential,
    }
}