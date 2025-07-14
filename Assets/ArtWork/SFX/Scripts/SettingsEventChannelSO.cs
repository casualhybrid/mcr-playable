using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "AudioModule/Settings Event Channel")]
public class SettingsEventChannelSO : ScriptableObject
{
	public UnityAction<bool> OnSettingChanged;

	public void RaiseEvent(bool settingStatus)
	{
		if (OnSettingChanged != null)
		{
			OnSettingChanged.Invoke(settingStatus);
		}
		else
		{
			UnityEngine.Console.LogWarning("An Settings was requested, but nobody picked it up.");
		}
	}
}
