using RestSharp;
using Shared;
using Shared.RestModel.Model;
using System.Collections.Concurrent;

namespace Scripts.ScriptLogic
{
    public class ChangeCvlIdScriptLogic : BaseScriptLogic
    {
        public static async Task ChangeCvlId(string cvlIdOld, string cvlIdNew, string url, string apiKey, IProgress<double> progress)
        {
            var restClient = SetupRest(url, apiKey);

            var processed = 0d;
            var lockTarget = new object();

            var entityTypesRequest = new RestRequest("/v1.0.1/model/entitytypes");
            var entityTypes = await restClient.GetAsync<List<EntityTypeModelV2>>(entityTypesRequest);

            if (entityTypes == null || entityTypes.Count == 0)
            {
                throw new HttpRequestException("Error when querying for EntityTypes");
            }

            progress.Report(0.01); //arbitrary value to indicate that we are indeed doing something

            //get old CVL
            var specificCvlRequest = new RestRequest("/v1.0.0/model/cvls/{cvlId}");
            specificCvlRequest.AddUrlSegment("cvlId", cvlIdOld);

            var cvlModel = await restClient.GetAsync<CVLModel>(specificCvlRequest);

            //get values for old CVL
            var cvlValuesRequest = new RestRequest("/v1.0.0/model/cvls/{cvlId}/values");
            cvlValuesRequest.AddUrlSegment("cvlId", cvlIdOld);

            var cvlValues = await restClient.GetAsync<List<CVLValueModel>>(specificCvlRequest);

            progress.Report(0.02); //arbitrary value to indicate that we are indeed doing something

            //create CVL with new id
            var newCvlRequest = new RestRequest("/v1.0.0/model/cvls");
            cvlModel.Id = cvlIdNew;
            newCvlRequest.AddBody(cvlModel);

            await restClient.PostAsync(newCvlRequest);

            //re-create values in new CVL
            Parallel.ForEach(cvlValues, new ParallelOptions
            {
                MaxDegreeOfParallelism = 3
            },
            cvlValue =>
            {
                try
                {
                    var valueRequest = new RestRequest("/v1.0.0/model/cvls/{cvlId}/values");
                    valueRequest.AddUrlSegment("cvlId", cvlIdNew);
                    valueRequest.AddBody(cvlValue);

                    _ = restClient.PostAsync(valueRequest).Result;
                }
                finally
                {
                    lock (lockTarget)
                    {
                        ++processed;
                        progress.Report(0 + 1d / 3d * (processed / cvlValues.Count));
                    }
                }
            });

            processed = 0d;
            var fieldTypesUsingCvl = new ConcurrentDictionary<int, (string entity, FieldTypeModelV2 field)>();

            //get all fieldTypeIds that are using old CVL
            Parallel.ForEach(entityTypes, new ParallelOptions
            {
                MaxDegreeOfParallelism = 3,
            },
            entityType =>
            {
                try
                {
                    var fieldTypesRequest = new RestRequest("/v1.0.1/model/entitytypes/{entityTypeId}/fieldTypes");
                    fieldTypesRequest.AddUrlSegment("entityTypeId", entityType.Id);

                    var fieldTypes = restClient.GetAsync<List<FieldTypeModelV2>>(fieldTypesRequest).Result;
                    if (fieldTypes == null || fieldTypes.Count == 0)
                    {
                        return;
                    }

                    var fieldTypesWithCvl = fieldTypes.FindAll(f => f.CvlId != null && f.CvlId.Equals(cvlIdOld, StringComparison.InvariantCultureIgnoreCase));
                    foreach (var fieldType in fieldTypesWithCvl)
                    {
                        fieldTypesUsingCvl.TryAdd(Random.Shared.Next(), (entityType.Id, fieldType));
                    }
                }
                finally
                {
                    lock (lockTarget)
                    {
                        ++processed;
                        progress.Report(1d / 3d + 1d / 3d * (processed / entityTypes.Count));
                    }
                }
            });

            processed = 0d;

            //update fieldTypes using CVL with new CVL Id
            Parallel.ForEach(fieldTypesUsingCvl.Values, new ParallelOptions
            {
                MaxDegreeOfParallelism = 3,
            },
            fieldTypeWithCvl =>
            {
                try
                {
                    var fieldTypeRequest = new RestRequest("/v1.0.1/model/entitytypes/{entityTypeId}/fieldTypes/{fieldTypeId}");
                    fieldTypeRequest.AddUrlSegment("entityTypeId", fieldTypeWithCvl.entity);
                    fieldTypeRequest.AddUrlSegment("fieldTypeId", fieldTypeWithCvl.field.Id);

                    fieldTypeWithCvl.field.CvlId = cvlIdNew;
                    fieldTypeRequest.AddBody(fieldTypeWithCvl.field);

                    _ = restClient.PutAsync(fieldTypeRequest).Result;
                }
                finally
                {
                    lock (lockTarget)
                    {
                        ++processed;
                        progress.Report(2d / 3d + 1d / 3d * (processed / (fieldTypesUsingCvl.Values.Count + 1))); //artificially increase count to avoid hitting progress value of 1
                    }
                }
            });

            var deleteCvlRequest = new RestRequest("/v1.0.0/model/cvls/{cvlId}");
            deleteCvlRequest.AddUrlSegment("cvlId", cvlIdOld);
            await restClient.DeleteAsync(deleteCvlRequest);

            progress.Report(1); //report completed progress
        }
    }
}
