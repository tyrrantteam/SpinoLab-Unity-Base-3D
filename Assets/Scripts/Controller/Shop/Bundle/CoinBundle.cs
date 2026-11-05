using System.Collections.Generic;

public class CoinBundle : BundleBase
{
    public override void LoadBundle(List<BundleReward> reward, int index)
    {
        base.LoadBundle(reward, index);

        foreach (var initReward in reward)
        {
            bundleNameText.text = FormatValue(initReward.value);
        }
    }

    protected override void OnGrantProducts() => GrantAllRewards();

    private static string FormatValue(int value)
    {
        if (value >= 10000)
        {
            float k = value / 1000f;
            return (k % 1 == 0 ? $"{(int)k}K" : $"{k:0.#}K");
        }

        return value.ToString();
    }
}
