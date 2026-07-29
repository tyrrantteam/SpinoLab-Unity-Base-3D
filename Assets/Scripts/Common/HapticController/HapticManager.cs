using DataAccount;
using JinGroup.Base.LoadData;
using Lofelt.NiceVibrations;
using System.Collections.Generic;
using UnityEngine;


public class HapticManager : SingletonMonoDontDestroy<HapticManager>
{
    public HapticManager(string className) : base(className)
    {
    }


    #region menu
     
    public void PlayHapticSelection()
    {
        if (!DataAccountPlayer.PlayerSettings.VibrationOff)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
        }
    }

    public void PlayHapticSuccess()
    {
        if (!DataAccountPlayer.PlayerSettings.VibrationOff)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
        }
    }

    public void PlayHapticWarning()
    {
        if (!DataAccountPlayer.PlayerSettings.VibrationOff)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Warning);
        }
    }

    public void PlayHapticFailure()
    {
        if (!DataAccountPlayer.PlayerSettings.VibrationOff)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);
        }
    }

    #endregion


    #region Physical
    public void PlayHapticLightImpact()
    {
        if (!DataAccountPlayer.PlayerSettings.VibrationOff)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
        }
    }

    public void PlayHapticMediumImpact()
    {
        if (!DataAccountPlayer.PlayerSettings.VibrationOff)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
        }
    }

    public void PlayHapticHeavyImpact()
    {
        if (!DataAccountPlayer.PlayerSettings.VibrationOff)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        }
    }

    public void PlayHapticRigidImpact()
    {
        if (!DataAccountPlayer.PlayerSettings.VibrationOff)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        }
    }

    public void PlayHapticSoft()
    {
        if (!DataAccountPlayer.PlayerSettings.VibrationOff)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.SoftImpact);
        }
    }

    #endregion
}
