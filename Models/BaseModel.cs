using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.Json.Serialization;
using System.Web;

namespace Models
{
    public class BaseModel<TItems, TLinks> where TItems : class 
                                           where TLinks: class
    {
        [JsonPropertyName("_items")]
        public IReadOnlyCollection<TItems> Items { get; set; } = new List<TItems>();

        [JsonPropertyName("_links")]
        public TLinks Links { get; set; }

        [JsonPropertyName("_metadata")]
        public dynamic Metadata { get; set; }
    }

    public class Link
    {
        public Uri Href { get; set; }
    }

    public class StoryLinks
    {
        public Uri PrevLink => Prev?.Href;
        public Link Prev { get; set; }
    }

    public class StoryViewModel : BaseModel<Story, StoryLinks>
    {
        public string BeforeParameter
        {
            get
            {
                if (Links?.PrevLink != null && HttpUtility.ParseQueryString(Links?.PrevLink.Query) is NameValueCollection parameters && parameters["before"] != null)
                {
                    return parameters["before"];
                }

                return null;
            }
        }
    }
}