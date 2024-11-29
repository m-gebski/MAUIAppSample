using RestSharp;
using Shared;
using Shared.RestModel.Entity;
using Shared.RestModel.Model;
using Shared.RestModel.Query;
using System.Collections.Concurrent;

namespace Scripts.ScriptLogic
{
    public class ChangeCvlKeyScriptLogic : BaseScriptLogic
    {
        public static async Task ChangeCvlKey(string cvlId, string cvlKeyOld, string cvlsKeyNew, string url, string apiKey, IProgress<double> progress)
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

            var fieldTypesContainingCvl = new ConcurrentDictionary<string, string>();

            //get all fieldTypeIds that are using modified CVL
            Parallel.ForEach(entityTypes, new ParallelOptions()
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

                    var fieldTypesWithCvl = fieldTypes.FindAll(f => f.CvlId != null && f.CvlId.Equals(cvlId, StringComparison.InvariantCultureIgnoreCase)).Select(f => f.Id);
                    foreach (var fieldType in fieldTypesWithCvl)
                    {
                        fieldTypesContainingCvl.TryAdd(fieldType, entityType.Id);
                    }
                }
                finally
                {
                    lock (lockTarget)
                    {
                        ++processed;
                        progress.Report(0 + 1d / 3d * (processed / entityTypes.Count));
                    }
                }
            });

            processed = 0d;
            var entityIdsFieldTypes = new ConcurrentDictionary<int, List<string>>();
            //get all entityIds are using modified CVL key, and change the corresponding field value to null
            Parallel.ForEach(fieldTypesContainingCvl, new ParallelOptions()
            {
                MaxDegreeOfParallelism = 3,
            },
            fieldTypeEntity =>
            {
                try
                {
                    var query = new QueryModel()
                    {
                        SystemCriteria =
                        [
                            new()
                            {
                                Type = "EntityTypeId",
                                Operator = "Equal",
                                Value = fieldTypeEntity.Value,
                            }
                        ],
                        DataCriteria =
                        [
                            new()
                            {
                                FieldTypeId = fieldTypeEntity.Key,
                                Operator = "ContainsAny",
                                Value = cvlKeyOld,
                            }
                        ],
                        DataCriteriaOperator = "And"
                    };

                    var queryRequest = new RestRequest("/v1.0.0/query");
                    queryRequest.AddBody(query);

                    var entities = restClient.PostAsync<EntityListModel>(queryRequest).Result;

                    var updateRequest = new RestRequest("/v1.0.0/entities/{entityId}/fieldvalues");
                    foreach (var entityId in entities.EntityIds)
                    {
                        entityIdsFieldTypes.AddOrUpdate(
                            entityId,
                            [fieldTypeEntity.Key],
                            (key, list) =>
                            {
                                list.Add(fieldTypeEntity.Key);
                                return list;
                            });

                        updateRequest.AddUrlSegment("entityId", entityId);
                        updateRequest.AddBody(
                            new List<FieldValueModel>
                            {
                                new()
                                {
                                    FieldTypeId = fieldTypeEntity.Key,
                                    Value = null,
                                }
                            });
                        _ = restClient.PutAsync<List<FieldValueModel>>(updateRequest).Result;
                    }
                }
                finally
                {
                    lock (lockTarget)
                    {
                        ++processed;
                        progress.Report(1d / 3d + 1d / 3d * (processed / fieldTypesContainingCvl.Count));
                    }
                }
            });

            //get model for old key
            var cvlRequest = new RestRequest("/v1.0.0/model/cvls/{cvlId}/values/{key}");
            cvlRequest.AddUrlSegment("cvlId", cvlId);
            cvlRequest.AddUrlSegment("key", cvlKeyOld);

            var cvlValueModel = restClient.GetAsync<CVLValueModel>(cvlRequest).Result;

            //delete old key
            _ = restClient.DeleteAsync(cvlRequest).Result;

            //add new key
            cvlValueModel.Key = cvlsKeyNew;
            cvlRequest = new RestRequest("/v1.0.0/model/cvls/{cvlId}/values");
            cvlRequest.AddUrlSegment("cvlId", cvlId);
            cvlRequest.AddBody(cvlValueModel);

            cvlValueModel = restClient.PostAsync<CVLValueModel>(cvlRequest).Result;

            //change all previously modified field values from null to new key
            processed = 0d;
            Parallel.ForEach(entityIdsFieldTypes, new ParallelOptions()
            {
                MaxDegreeOfParallelism = 3,
            },
            entityIdFieldTypes =>
            {
                try
                {
                    var updateRequest = new RestRequest("/v1.0.0/entities/{entityId}/fieldvalues");
                    foreach (var fieldType in entityIdFieldTypes.Value)
                    {
                        updateRequest.AddUrlSegment("entityId", entityIdFieldTypes.Key);
                        updateRequest.AddBody(
                            entityIdFieldTypes.Value.Select(f =>
                                new FieldValueModel()
                                {
                                    FieldTypeId = f,
                                    Value = cvlsKeyNew
                                }));
                        _ = restClient.PutAsync<List<FieldValueModel>>(updateRequest).Result;
                    }
                }
                finally
                {
                    lock (lockTarget)
                    {
                        ++processed;
                        progress.Report(2d / 3d + 1d / 3d * (processed / fieldTypesContainingCvl.Count));
                    }
                }
            });
        }
    }
}
