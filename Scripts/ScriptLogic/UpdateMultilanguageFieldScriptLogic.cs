using RestSharp;
using Shared;
using Shared.RestModel.Model;

namespace Scripts.ScriptLogic
{
    public class UpdateMultilanguageFieldScriptLogic : BaseScriptLogic
    {
        public static async Task UpdateMultilanguageField(string entityTypeId, FieldTypeModelV2 fieldType, string newDescription, string url, string apiKey, IProgress<double> progress)
        {
            var restClient = SetupRest(url, apiKey);

            var fieldTypeRequest = new RestRequest("/v1.0.1/model/entitytypes/{entityTypeId}/fieldtypes/{fieldTypeId}");
            fieldTypeRequest.AddUrlSegment("entityTypeId", entityTypeId);
            fieldTypeRequest.AddUrlSegment("fieldTypeId", fieldType.Id);

            for (int i = 0; i < fieldType.Description.Count; i++)
            {
                var iKey = fieldType.Description.ElementAt(i).Key;
                fieldType.Description[iKey] = newDescription;
                progress.Report((double)i / fieldType.Description.Count);
            }

            fieldTypeRequest.AddBody(fieldType);

            _ = await restClient.PutAsync(fieldTypeRequest);
            progress.Report(1);
        }

        public static async Task<List<string>> GetEntityTypesAsync(string url, string apiKey)
        {
            var restClient = SetupRest(url, apiKey);

            var entityTypesRequest = new RestRequest("/v1.0.1/model/entitytypes");
            var entityTypes = await restClient.GetAsync<List<EntityTypeModelV2>>(entityTypesRequest);

            return entityTypes.Select(e => e.Id).ToList();
        }

        public static async Task<List<FieldTypeModelV2>> GetFieldTypesAsync(string entityTypeId, string url, string apiKey)
        {
            var restClient = SetupRest(url, apiKey);

            var fieldTypesRequest = new RestRequest("/v1.0.1/model/entitytypes/{entityTypeId}/fieldtypes");
            fieldTypesRequest.AddUrlSegment("entityTypeId", entityTypeId);

            var fieldTypes = await restClient.GetAsync<List<FieldTypeModelV2>>(fieldTypesRequest);

            return fieldTypes;
        }
    }
}
