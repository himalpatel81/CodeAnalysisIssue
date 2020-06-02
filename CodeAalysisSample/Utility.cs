using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Data;
using System.Threading.Tasks;
using System;
using System.Linq;

public class Utility
{
    private async Task<JArray> ConvertDataTableToJSONArray(DataSet lineItems, string tableName, Entity entity, EntityMetaData entityMetaData, ConcurrentDictionary<string, string> customFields)
    {
        JArray jsonArrayForTableValues = new JArray();
#pragma warning disable IDE0063 // Use simple 'using' statement
        using (DataTable dtLineItem = lineItems.Tables[tableName])
#pragma warning restore IDE0063 // Use simple 'using' statement
        {
            if (dtLineItem != null && dtLineItem.Rows.Count > 0)
            {
                foreach (DataRow drItem in dtLineItem.Rows)
                {
                    JObject jsonObjectForTableValues = new JObject();
                    foreach (EntityMetaData metaDataForTableValues in entity.MetaDatas)
                    {
                        if (metaDataForTableValues.ChildEntities != null && metaDataForTableValues.ChildEntities.Count() > 0)
                        {
                            foreach (Entity childentity in metaDataForTableValues.ChildEntities)
                            {
                                if (childentity.MetaDatas != null && childentity.MetaDatas.Count() > 0)
                                {
                                    if (metaDataForTableValues.Name.ToLower() != "distribution" && metaDataForTableValues.Name.ToLower() != "address")
                                    {
                                        if (metaDataForTableValues.Type.ToLower() == "objectarray")
                                        {
                                            JArray childJsonArray = new JArray();
                                            childJsonArray = await ConvertDataTableToJSONArray(lineItems, metaDataForTableValues.DisplayName, childentity, metaDataForTableValues, customFields);
                                            jsonObjectForTableValues[metaDataForTableValues.Name] = childJsonArray;
                                        }
                                    }
                                    else if (metaDataForTableValues.Name.ToLower() == "address")
                                    {
                                        JObject nestedJsonObject = new JObject();
                                        JArray nestedJsonArray = new JArray();
                                        foreach (EntityMetaData childEntityMetadata in childentity.MetaDatas)
                                        {
                                            if (!drItem.Table.Columns.Contains(childEntityMetadata.DisplayName))
                                                nestedJsonObject[childEntityMetadata.Name] = childEntityMetadata.DefaultValue;
                                            else
                                                nestedJsonObject[childEntityMetadata.Name] = childEntityMetadata.RemoveSpaceFromValue ? Convert.ToString(drItem[childEntityMetadata.DisplayName]).Replace(" ", "") : Convert.ToString(drItem[childEntityMetadata.DisplayName]);
                                        }
                                        nestedJsonArray.Add(nestedJsonObject);
                                        jsonObjectForTableValues[metaDataForTableValues.Name] = nestedJsonArray;
                                    }
                                    else if (metaDataForTableValues.Name.ToLower() == "distribution")
                                    {
                                        JObject nestedJsonObject = new JObject();
                                        foreach (EntityMetaData childEntityMetadata in childentity.MetaDatas)
                                        {
                                            if (childEntityMetadata.ChildEntities != null && childEntityMetadata.ChildEntities.Count > 0)
                                            {
                                                foreach (Entity nestedChildBBEntity in childEntityMetadata.ChildEntities)
                                                {
                                                    if (nestedChildBBEntity.MetaDatas != null && nestedChildBBEntity.MetaDatas.Count > 0)
                                                    {
                                                        if (metaDataForTableValues.Type.ToLower() == "objectarray")
                                                        {
                                                            JArray nestedChildJsonArray = new JArray();
                                                            //nestedChildJsonArray = await GenerateTransactionCodesObjectArray(drItem, nestedChildBBEntity, skyAPIAuthenticationAttributes, skyAPIClientAttributes);
                                                            nestedJsonObject[childEntityMetadata.Name] = nestedChildJsonArray;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (childEntityMetadata.Type.ToLower() == "object" && childEntityMetadata.ChildEntities != null && childEntityMetadata.ChildEntities.Count > 0)
                                                {
                                                    foreach (Entity childEntityForTableValues in childEntityMetadata.ChildEntities)
                                                    {
                                                        JArray nestedChildJsonArrayForDataTableVlues = new JArray();
                                                        JObject nestedChildJsonObjectForDataTableValues = new JObject();
                                                        if (childEntityForTableValues.MetaDatas != null && childEntityForTableValues.MetaDatas.Count > 0)
                                                        {
                                                            foreach (EntityMetaData nestedChildMetaForTableValues in childEntityForTableValues.MetaDatas)
                                                            {
                                                                if (nestedChildMetaForTableValues.ChildEntities == null || (nestedChildMetaForTableValues.ChildEntities == null && nestedChildMetaForTableValues.ChildEntities.Count == 0))
                                                                {
                                                                    if (childEntityMetadata.Type.ToLower() == "objectarray" && !string.IsNullOrEmpty(nestedChildMetaForTableValues.DisplayName) && drItem[nestedChildMetaForTableValues.DisplayName] != null)
                                                                    {
                                                                        nestedJsonObject[nestedChildMetaForTableValues.Name] = entityMetaData.RemoveSpaceFromValue ? Convert.ToString(customFields[entityMetaData.DisplayName]).Replace(" ", "") : Convert.ToString(customFields[entityMetaData.DisplayName]);
                                                                    }
                                                                    else if (childEntityMetadata.Type.ToLower() == "object" && !string.IsNullOrEmpty(nestedChildMetaForTableValues.DisplayName) && drItem[nestedChildMetaForTableValues.DisplayName] != null)
                                                                    {
                                                                        nestedChildJsonObjectForDataTableValues[nestedChildMetaForTableValues.Name] = nestedChildMetaForTableValues.RemoveSpaceFromValue ? Convert.ToString(drItem[nestedChildMetaForTableValues.DisplayName]).Replace(" ", "") : Convert.ToString(drItem[nestedChildMetaForTableValues.DisplayName]);
                                                                        nestedJsonObject[childEntityMetadata.Name] = nestedChildJsonObjectForDataTableValues;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        if (entityMetaData.Type.ToLower() == "objectarray")
                                                        {
                                                            nestedChildJsonArrayForDataTableVlues.Add(nestedChildJsonObjectForDataTableValues);
                                                            nestedJsonObject[entityMetaData.Name] = nestedChildJsonArrayForDataTableVlues;
                                                        }
                                                    }
                                                }
                                                else if (dtLineItem.Columns.Contains(childEntityMetadata.DisplayName) && drItem[childEntityMetadata.DisplayName] != null)
                                                {
                                                    if (!string.IsNullOrEmpty(childEntityMetadata.LookupEntityName) && !string.IsNullOrEmpty(value: childEntityMetadata.SearchColumnName))
                                                    {
                                                    }
                                                    else
                                                        nestedJsonObject[childEntityMetadata.Name] = drItem[childEntityMetadata.DisplayName].ToString();
                                                }
                                                else
                                                    nestedJsonObject.Remove(childEntityMetadata.Name);
                                            }
                                        }
                                        if (metaDataForTableValues.Name.ToLower() == "distribution")
                                        {
                                            JArray distributionSplitsJsonArray = new JArray();
                                            distributionSplitsJsonArray.Add(nestedJsonObject);
                                            jsonObjectForTableValues[metaDataForTableValues.Name] = distributionSplitsJsonArray;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (metaDataForTableValues.Type.ToLower() == "object" && metaDataForTableValues.ChildEntities != null && metaDataForTableValues.ChildEntities.Count > 0)
                            {
                                foreach (Entity childEntityForTableValues in metaDataForTableValues.ChildEntities)
                                {
                                    JArray nestedChildJsonArrayForDataTableVlues = new JArray();
                                    JObject nestedChildJsonObjectForDataTableValues = new JObject();
                                    if (childEntityForTableValues.MetaDatas != null && childEntityForTableValues.MetaDatas.Count > 0)
                                    {
                                        foreach (EntityMetaData nestedChildMetaForTableValues in childEntityForTableValues.MetaDatas)
                                        {
                                            if (nestedChildMetaForTableValues.ChildEntities == null || (nestedChildMetaForTableValues.ChildEntities == null && nestedChildMetaForTableValues.ChildEntities.Count == 0))
                                            {
                                                if (metaDataForTableValues.Type.ToLower() == "objectarray" && !string.IsNullOrEmpty(nestedChildMetaForTableValues.DisplayName) && drItem[nestedChildMetaForTableValues.DisplayName] != null)
                                                {
                                                    jsonObjectForTableValues[nestedChildMetaForTableValues.Name] = entityMetaData.RemoveSpaceFromValue ? Convert.ToString(customFields[entityMetaData.DisplayName]).Replace(" ", "") : Convert.ToString(customFields[entityMetaData.DisplayName]);
                                                }
                                                else if (metaDataForTableValues.Type.ToLower() == "object" && !string.IsNullOrEmpty(nestedChildMetaForTableValues.DisplayName) && drItem[nestedChildMetaForTableValues.DisplayName] != null)
                                                {
                                                    nestedChildJsonObjectForDataTableValues[nestedChildMetaForTableValues.Name] = nestedChildMetaForTableValues.RemoveSpaceFromValue ? Convert.ToString(drItem[nestedChildMetaForTableValues.DisplayName]).Replace(" ", "") : Convert.ToString(drItem[nestedChildMetaForTableValues.DisplayName]);
                                                    jsonObjectForTableValues[metaDataForTableValues.Name] = nestedChildJsonObjectForDataTableValues;
                                                }
                                            }
                                        }
                                    }
                                    if (entityMetaData.Type.ToLower() == "objectarray")
                                    {
                                        nestedChildJsonArrayForDataTableVlues.Add(nestedChildJsonObjectForDataTableValues);
                                        jsonObjectForTableValues[entityMetaData.Name] = nestedChildJsonArrayForDataTableVlues;
                                    }
                                }
                            }
                            else if (dtLineItem.Columns.Contains(metaDataForTableValues.DisplayName) && drItem[metaDataForTableValues.DisplayName] != null)
                            {
                                if (!string.IsNullOrEmpty(metaDataForTableValues.LookupEntityName) && !string.IsNullOrEmpty(metaDataForTableValues.SearchColumnName))
                                {
                                    
                                }
                                else if (metaDataForTableValues.Type.ToLower() == "boolean")
                                    jsonObjectForTableValues[metaDataForTableValues.Name] = Convert.ToString(drItem[metaDataForTableValues.DisplayName]).ToLower() == "yes" || Convert.ToString(drItem[metaDataForTableValues.DisplayName]).ToLower() == "true" ? "true" : "false";
                                else if (metaDataForTableValues.Name.ToLower() == "state" || metaDataForTableValues.Name.ToLower() == "county")
                                {
                                    EntityMetaData countryEntityMetaData = entity.MetaDatas.FirstOrDefault(md => md.Name.ToLower() == "country");
                                    if (drItem != null && drItem.Table.Columns.Contains(countryEntityMetaData.DisplayName))
                                    {
                                        string stateOrCounty = string.Empty;
                                        switch (drItem[countryEntityMetaData.DisplayName].ToString().ToLower())
                                        {
                                            case "united kingdom":
                                            case "scotland":
                                            case "germany":
                                                stateOrCounty = "county";
                                                break;
                                            default:
                                                stateOrCounty = "state";
                                                break;
                                        }
                                        jsonObjectForTableValues[stateOrCounty] = Convert.ToString(drItem[metaDataForTableValues.DisplayName]);
                                    }
                                }
                                else
                                    jsonObjectForTableValues[metaDataForTableValues.Name] = metaDataForTableValues.RemoveSpaceFromValue ? drItem[metaDataForTableValues.DisplayName].ToString().Replace(" ", "") : drItem[metaDataForTableValues.DisplayName].ToString();
                            }
                            else
                                jsonObjectForTableValues.Remove(metaDataForTableValues.Name);
                        }
                    }
                    jsonArrayForTableValues.Add(jsonObjectForTableValues);
                }
            }
            return jsonArrayForTableValues;
        }
    }

    private Task<JArray> ConvertDataTableToJSONArray(DataSet lineItems, object displayName, Entity childentity, EntityMetaData metaDataForTableValues, ConcurrentDictionary<string, string> customFields)
    {
        throw new NotImplementedException();
    }
}