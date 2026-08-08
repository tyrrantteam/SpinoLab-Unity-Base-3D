namespace Base.Core.Services
{
    public class AdsEvent
    {
        public static AdsEvent RewardOffer     { get; set; }
        public static AdsEvent RewardClick     { get; set; }
        public static AdsEvent RewardShow      { get; set; }
        public static AdsEvent RewardFail      { get; set; }
        public static AdsEvent RewardComplete  { get; set; }
        public static AdsEvent RewardLoad      { get; set; }
        public static AdsEvent InterFail       { get; set; }
        public static AdsEvent InterLoad       { get; set; }
        public static AdsEvent InterShow       { get; set; }
        public static AdsEvent InterClick      { get; set; }
        public static AdsEvent InterImpression { get; set; }
    }
}