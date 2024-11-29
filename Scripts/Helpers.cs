using Shared;

namespace Scripts
{
    internal class Helpers
    {
        internal static string GetKeyFromPreferences(IPreferences preferences, string envSelect)
        {
            var key = string.Empty;
            switch (envSelect)
            {
                case Consts.Env.Dev:
                    key = preferences.Get(Consts.Settings.DevKey, string.Empty);
                    break;

                case Consts.Env.Test:
                    key = preferences.Get(Consts.Settings.TestKey, string.Empty);
                    break;

                case Consts.Env.Prod:
                    key = preferences.Get(Consts.Settings.ProdKey, string.Empty);
                    break;
            }

            return key;
        }
    }
}
