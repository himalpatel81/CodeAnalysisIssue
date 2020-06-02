using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public partial class EntityMetaData
{
    int ID { get; set; }

    string Entity { get; set; }

    string APIUrl { get; set; }

    string EntitySchema { get; set; }

    string FilterText { get; set; }

    List<EntityMetaData> MetaDatas { get; set; }


    public List<Entity> ChildEntities { get; set; }
    public string Name { get; internal set; }
    public string Type { get; internal set; }
    public JToken DefaultValue { get;  set; }
    public string DisplayName { get; set; }
    public bool RemoveSpaceFromValue { get; set; }
    public string SearchColumnName { get; set; }
    public string LookupEntityName { get; internal set; }
}