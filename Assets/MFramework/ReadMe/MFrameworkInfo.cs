namespace MFramework
{
    public class MFrameworkInfo
    {
        public const string Version = "v0.0.8";
        public const string LastModityDate = "20230519";
        public const string Author = "Mr_OldMao";
        public const string Github = "https://github.com/Mr-OldMao";
        public const string Email = "929764654@qq.com";

        public static void GetAllInfo()
        {
            UnityEngine.Debug.Log($"Version£º{Version}£¬LastModityDate£º{LastModityDate}£¬Author£º{Author}£¬Github£º{Github}£¬Email£º{Email}");
        }
    }
}
