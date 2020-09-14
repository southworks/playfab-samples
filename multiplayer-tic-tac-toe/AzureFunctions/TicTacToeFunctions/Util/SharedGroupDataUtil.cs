// Copyright (C) Microsoft Corporation. All rights reserved.

using Newtonsoft.Json;
using PlayFab;
using PlayFab.ServerModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicTacToeFunctions.Models;

namespace TicTacToeFunctions.Util
{
    public static class SharedGroupDataUtil
    {
        public static async Task<CreateSharedGroupResult> CreateAsync(PlayFabAuthenticationContext context, string sharedGroupDataName)
        {
            var request = new CreateSharedGroupRequest { AuthenticationContext = context, SharedGroupId = sharedGroupDataName };
            var response = await GetPlayFabServerInstanceAPI().CreateSharedGroupAsync(request);

            if (response.Error != null)
            {
                throw new Exception($"An error occurred while creating the group {sharedGroupDataName}: Error: {response.Error.GenerateErrorReport()}");
            }

            return response.Result;
        }

        public static async Task<AddSharedGroupMembersResult> AddMembersAsync(PlayFabAuthenticationContext context, string sharedGroupId, List<string> members)
        {
            var request = new AddSharedGroupMembersRequest { AuthenticationContext = context, SharedGroupId = sharedGroupId, PlayFabIds = members };
            var response = await GetPlayFabServerInstanceAPI().AddSharedGroupMembersAsync(request);

            if (response.Error != null)
            {
                throw new Exception($"An error occurred while creating the group {sharedGroupId}: Error: {response.Error.GenerateErrorReport()}");
            }

            return response.Result;
        }

        public static async Task<TicTacToeSharedGroupData> UpdateAsync(PlayFabAuthenticationContext context, TicTacToeSharedGroupData data)
        {
            var request = new UpdateSharedGroupDataRequest
            {
                AuthenticationContext = context,
                Data = ConvertSharedGroupDataToStringDictionary(data, Constants.SHARED_GROUP_DATA_DICTIONARY_ENTRY_NAME),
                SharedGroupId = data.SharedGroupId
            };

            await GetPlayFabServerInstanceAPI().UpdateSharedGroupDataAsync(request);

            return data;
        }

        public static async Task DeleteAsync(PlayFabAuthenticationContext context, string groupId)
        {
            var request = new DeleteSharedGroupRequest
            {
                AuthenticationContext = context,
                SharedGroupId = groupId
            };

            await GetPlayFabServerInstanceAPI().DeleteSharedGroupAsync(request);
        }

        public static async Task<TicTacToeSharedGroupData> GetAsync(PlayFabAuthenticationContext context, string groupId)
        {
            var request = new GetSharedGroupDataRequest
            {
                AuthenticationContext = context,
                SharedGroupId = groupId
            };

            var response = await GetPlayFabServerInstanceAPI().GetSharedGroupDataAsync(request);
            var resultData = response?.Result?.Data ?? null;

            if (resultData == null)
            {
                return null;
            }

            return ParseFromSharedGroupData<TicTacToeSharedGroupData>(resultData, Constants.SHARED_GROUP_DATA_DICTIONARY_ENTRY_NAME);
        }

        private static PlayFabServerInstanceAPI GetPlayFabServerInstanceAPI()
        {
            return new PlayFabServerInstanceAPI(
                new PlayFabApiSettings
                {
                    TitleId = Environment.GetEnvironmentVariable(Constants.PLAYFAB_TITLE_ID, EnvironmentVariableTarget.Process),
                    VerticalName = Environment.GetEnvironmentVariable(Constants.PLAYFAB_CLOUD_NAME, EnvironmentVariableTarget.Process),
                    DeveloperSecretKey = Environment.GetEnvironmentVariable(Constants.PLAYFAB_DEV_SECRET_KEY, EnvironmentVariableTarget.Process)
                }
            );
        }

        private static Dictionary<string, string> ConvertSharedGroupDataToStringDictionary(TicTacToeSharedGroupData data, string entryName)
        {
            var dictionary = new Dictionary<string, string>();
            var JsonData = JsonConvert.SerializeObject(data);

            dictionary.Add(entryName, JsonData);

            return dictionary;
        }

        private static T ParseFromSharedGroupData<T>(Dictionary<string, SharedGroupDataRecord> dictionary, string entryName) where T : new()
        {
            dictionary.TryGetValue(entryName, out var sharedGroupDataRecord);

            return string.IsNullOrWhiteSpace(sharedGroupDataRecord?.Value ?? string.Empty) ?
                new T() :
                JsonConvert.DeserializeObject<T>(sharedGroupDataRecord.Value);
        }
    }
}