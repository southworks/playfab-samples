using System;
using PlayFab;

namespace TicTacToeFunctions.Util
{
    public static class Settings
    {
        public static void TrySetSecretKey(PlayFabApiSettings settings)
        {
            var secretKey = Environment.GetEnvironmentVariable(Constants.PlayfabDevSecretKey, EnvironmentVariableTarget.Process);

            if (!string.IsNullOrEmpty(secretKey))
            {
                settings.DeveloperSecretKey = secretKey;
            }
        }

        public static void TrySetCloudName(PlayFabApiSettings settings)
        {
            var cloud = Environment.GetEnvironmentVariable(Constants.PlayFabCloudName, EnvironmentVariableTarget.Process);

            if (!string.IsNullOrEmpty(cloud))
            {
                settings.VerticalName = cloud;
            }
        }
    }
}
