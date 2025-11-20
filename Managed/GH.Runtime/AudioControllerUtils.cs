using System.Linq;
using ClockStone;
using UnityEngine.Video;

public static class AudioControllerUtils
{
	public static void PlaySound(string audioItem, bool optional = false)
	{
		if (!string.IsNullOrEmpty(audioItem))
		{
			Play(audioItem, optional);
		}
	}

	private static AudioObject Play(string audioItem, bool optional = false)
	{
		if (AudioController.IsValidAudioID(audioItem))
		{
			return AudioController.Play(audioItem);
		}
		if (!optional)
		{
			Debug.LogErrorGUI("Audio item not found: " + audioItem);
		}
		return null;
	}

	public static void PlaySeqSounds(string audioItemFirst, string audioItemSecond)
	{
		if (string.IsNullOrEmpty(audioItemFirst))
		{
			PlaySound(audioItemSecond);
			return;
		}
		AudioObject audioObject = Play(audioItemFirst);
		if (audioObject == null)
		{
			PlaySound(audioItemSecond);
		}
		else if (!string.IsNullOrEmpty(audioItemSecond))
		{
			audioObject.PlayAfter(audioItemSecond);
		}
	}

	public static void StopSound(string audioItem, float fadeOutDuration = -1f)
	{
		if (!string.IsNullOrEmpty(audioItem) && AudioController.IsValidAudioID(audioItem))
		{
			AudioController.Stop(audioItem, fadeOutDuration);
		}
	}

	public static bool IsPlaying(string audioItem)
	{
		if (!string.IsNullOrEmpty(audioItem))
		{
			return AudioController.IsPlaying(audioItem);
		}
		return false;
	}

	public static bool AdjustMasterVolume(VideoPlayer videoPlayer, float volume)
	{
		if (TryAdjustVolume())
		{
			float num = volume / 100f;
			AudioController.SetGlobalVolume(num);
			AudioController.SetCategoryVolume("Ambience", num);
			if (videoPlayer != null)
			{
				videoPlayer.SetDirectAudioVolume(0, num);
			}
			return true;
		}
		return false;
	}

	public static bool AdjustMusicVolume(float volume)
	{
		if (TryAdjustVolume())
		{
			AudioController.SetCategoryVolume("Music", volume / 100f);
			return true;
		}
		return false;
	}

	public static bool AdjustPingVolume(float volume)
	{
		if (TryAdjustVolume())
		{
			float volume2 = volume / 100f;
			AudioController.SetCategoryVolume("MultiplayerPing_SFX", volume2);
			return true;
		}
		return false;
	}

	public static bool AdjustStoryVolume(float volume)
	{
		if (TryAdjustVolume())
		{
			float volume2 = volume / 100f;
			foreach (AudioCategory item in SingletonMonoBehaviour<AudioController>.Instance.AudioCategories.Where((AudioCategory it) => it.Name.StartsWith("VONarration")))
			{
				AudioController.SetCategoryVolume(item.Name, volume2);
			}
			return true;
		}
		return false;
	}

	public static bool AdjustUIVolume(float volume)
	{
		if (TryAdjustVolume())
		{
			float volume2 = volume / 100f;
			AudioController.SetCategoryVolume("UI", volume2);
			return true;
		}
		return false;
	}

	public static bool AdjustEffectsVolume(float volume)
	{
		if (TryAdjustVolume())
		{
			float volume2 = volume / 100f;
			AudioController.SetCategoryVolume("GAIN", volume2);
			AudioController.SetCategoryVolume("Elementalist_SFX", volume2);
			AudioController.SetCategoryVolume("Voidwarden_SFX", volume2);
			AudioController.SetCategoryVolume("Hatchet_SFX", volume2);
			AudioController.SetCategoryVolume("Demolitionist_SFX", volume2);
			AudioController.SetCategoryVolume("BloodEnemy_SFX", volume2);
			AudioController.SetCategoryVolume("RedGuard_SFX", volume2);
			AudioController.SetCategoryVolume("Savvas_SFX", volume2);
			AudioController.SetCategoryVolume("CityGuard_SFX", volume2);
			AudioController.SetCategoryVolume("HumansAndOrchids_SFX", volume2);
			AudioController.SetCategoryVolume("Hail_SFX", volume2);
			AudioController.SetCategoryVolume("Colorless_SFX", volume2);
			AudioController.SetCategoryVolume("ElderDrake_SFX", volume2);
			AudioController.SetCategoryVolume("Fish_SFX", volume2);
			AudioController.SetCategoryVolume("PrimeDemon_SFX", volume2);
			AudioController.SetCategoryVolume("Redthorn_SFX", volume2);
			AudioController.SetCategoryVolume("TheBetrayer_SFX", volume2);
			AudioController.SetCategoryVolume("TheGloom_SFX", volume2);
			AudioController.SetCategoryVolume("Totem_SFX", volume2);
			AudioController.SetCategoryVolume("VocalChords_SFX", volume2);
			AudioController.SetCategoryVolume("SightlessEye_SFX", volume2);
			AudioController.SetCategoryVolume("DeepTerror_SFX", volume2);
			AudioController.SetCategoryVolume("Merc_SFX", volume2);
			AudioController.SetCategoryVolume("Sawbones_SFX", volume2);
			AudioController.SetCategoryVolume("DarkRider_SFX", volume2);
			AudioController.SetCategoryVolume("WingedHorror_SFX", volume2);
			AudioController.SetCategoryVolume("DoomStalker_SFX", volume2);
			AudioController.SetCategoryVolume("Nightshroud_SFX", volume2);
			AudioController.SetCategoryVolume("BeastTyrant_SFX", volume2);
			AudioController.SetCategoryVolume("BlackImp_SFX", volume2);
			AudioController.SetCategoryVolume("Lurker_SFX", volume2);
			AudioController.SetCategoryVolume("Jerkserah_SFX", volume2);
			AudioController.SetCategoryVolume("JadeFalcon_SFX", volume2);
			AudioController.SetCategoryVolume("Ooze_SFX", volume2);
			AudioController.SetCategoryVolume("Sunkeeper_SFX", volume2);
			AudioController.SetCategoryVolume("Summoner_SFX", volume2);
			AudioController.SetCategoryVolume("MercilessOverseer_SFX", volume2);
			AudioController.SetCategoryVolume("Quartermaster_SFX", volume2);
			AudioController.SetCategoryVolume("EarthDemon_SFX", volume2);
			AudioController.SetCategoryVolume("AncientArtillery_SFX", volume2);
			AudioController.SetCategoryVolume("Berserker_SFX", volume2);
			AudioController.SetCategoryVolume("Harrower_SFX", volume2);
			AudioController.SetCategoryVolume("GiantViper_SFX", volume2);
			AudioController.SetCategoryVolume("FrostDemon_SFX", volume2);
			AudioController.SetCategoryVolume("CaveBear_SFX", volume2);
			AudioController.SetCategoryVolume("Drake_SFX", volume2);
			AudioController.SetCategoryVolume("StoneGolem_SFX", volume2);
			AudioController.SetCategoryVolume("SoothSinger_SFX", volume2);
			AudioController.SetCategoryVolume("ForestImp_SFX", volume2);
			AudioController.SetCategoryVolume("FlameWindDemon_SFX", volume2);
			AudioController.SetCategoryVolume("Hound_SFX", volume2);
			AudioController.SetCategoryVolume("Inox_SFX", volume2);
			AudioController.SetCategoryVolume("Idle_SFX", volume2);
			AudioController.SetCategoryVolume("Mindthief_SFX", volume2);
			AudioController.SetCategoryVolume("Vermling_SFX", volume2);
			AudioController.SetCategoryVolume("SFX", volume2);
			AudioController.SetCategoryVolume("Bandit_SFX", volume2);
			AudioController.SetCategoryVolume("BoneRanger_SFX", volume2);
			AudioController.SetCategoryVolume("Brute_SFX", volume2);
			AudioController.SetCategoryVolume("Cragheart_SFX", volume2);
			AudioController.SetCategoryVolume("Cultist_SFX", volume2);
			AudioController.SetCategoryVolume("HighCultist_SFX", volume2);
			AudioController.SetCategoryVolume("LivingCorpse_SFX", volume2);
			AudioController.SetCategoryVolume("LivingBones_SFX", volume2);
			AudioController.SetCategoryVolume("LivingSpirit_SFX", volume2);
			AudioController.SetCategoryVolume("NightDemon_SFX", volume2);
			AudioController.SetCategoryVolume("Scoundrel_SFX", volume2);
			AudioController.SetCategoryVolume("SFX_Environment", volume2);
			AudioController.SetCategoryVolume("SFX_OnHits", volume2);
			AudioController.SetCategoryVolume("SpellWeaver_SFX", volume2);
			AudioController.SetCategoryVolume("SunDemon_SFX", volume2);
			AudioController.SetCategoryVolume("Tinkerer_SFX", volume2);
			AudioController.SetCategoryVolume("UndeadCommander_SFX", volume2);
			AudioController.SetCategoryVolume("SFX Reverb", volume2);
			AudioController.SetCategoryVolume("SFX_Footsteps", volume2);
			AudioController.SetCategoryVolume("Ambience", volume2);
			return true;
		}
		return false;
	}

	public static bool AdjustHapticVolume(float volume)
	{
		if (TryAdjustVolume())
		{
			AudioController.SetHapticVolume(volume / 100f);
			return true;
		}
		return false;
	}

	public static bool AdjustHapticVibration(float vibrationStrength)
	{
		if (TryAdjustVolume())
		{
			AudioController.SetHapticVibration(vibrationStrength / 100f);
			return true;
		}
		return false;
	}

	private static bool TryAdjustVolume()
	{
		if ((bool)SingletonMonoBehaviour<AudioController>.DoesInstanceExist())
		{
			return true;
		}
		Debug.LogError("AudioSettings: Trying to adjust volume but no instance of AudioController exists.");
		return false;
	}
}
