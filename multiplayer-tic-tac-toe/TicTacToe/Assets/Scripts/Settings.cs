// Copyright (C) Microsoft Corporation. All rights reserved.

using PlayFab;

namespace TicTacToe
{
    public static class Settings
    {
        public static void UpdateSettings()
        {
            PlayFabSettings.TitleId = Constants.TITLE_ID;
            PlayFabSettings.CompressApiData = Constants.COMPRESS_API_DATA;
        }
    }
}
