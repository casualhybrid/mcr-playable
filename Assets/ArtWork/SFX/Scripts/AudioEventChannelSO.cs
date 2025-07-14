using FMOD.Studio;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Event on which <c>AudioPlayer</c> components send a message to play SFX and music. <c>AudioManager</c> listens on these events, and actually plays the sound.
/// </summary>
[CreateAssetMenu(menuName = "AudioModule/Audio Event Channel")]
public class AudioEventChannelSO : ScriptableObject
{
	public UnityAction<GameObject, string> OnAudioRequested;
	public UnityAction<GameObject, string, string, float> OnAudioWithParameterRequested;
	public UnityAction<GameObject, string, AudioInstanceConfig> OnAudioWithConfigurationRequsted;


	public void RaiseEvent(GameObject obj, string strObj)
	{
		if (OnAudioRequested != null)
		{
			OnAudioRequested.Invoke(obj, strObj);
		}
		else
		{
			UnityEngine.Console.LogWarning("An Audio was requested, but nobody picked it up.");
		}
	}

	public void RaiseEvent(GameObject obj, string strObj, string parameter, float parameterValue)
	{
		if (OnAudioRequested != null)
		{
			OnAudioWithParameterRequested.Invoke(obj, strObj, parameter, parameterValue);
		}
		else
		{
			UnityEngine.Console.LogWarning("An Audio was requested, but nobody picked it up.");
		}
	}

	public void RaiseEvent(GameObject obj, string strObj, AudioInstanceConfig audioInstanceConfig)
	{
		if (OnAudioRequested != null)
		{
			OnAudioWithConfigurationRequsted.Invoke(obj, strObj, audioInstanceConfig);
		}
		else
		{
			UnityEngine.Console.LogWarning("An Audio was requested, but nobody picked it up.");
		}
	}
}
