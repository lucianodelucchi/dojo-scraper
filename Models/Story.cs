using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Models
{
    public enum StoryType
    {
        TextAndAttachment
    }

    public class Story
    {
        const string DojoSender = "Mojo";
        //this may or may not be a way to identify DojoSender
        const string DojoSenderId = "5d7eb2d2146d8efb2cf20603";

        [JsonPropertyName("_id")]
        public string Id { get; set; }
        [JsonPropertyName("time")]
        public DateTime PostedAt { get; set; }
        public string SenderName { get; set; }
        public string SenderId { get; set; }
        public StoryType Type { get; set; }
        public Content Contents { get; set; }
        public bool HasAttachments => Contents.Attachments.Any();
        public bool FromDojo => SenderName == DojoSender && SenderId == DojoSenderId;
    }

    public class Content
    {
        public string Body { get; set; }
        public IReadOnlyCollection<Attachment> Attachments { get; set; } = new List<Attachment>();

        public bool HasPhotos => Attachments.Any(a => a.IsPhoto);
    }

    public enum AttachmentType 
    {
        Photo
    }

    public class Attachment
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }
        public string ContentType { get; set; }
        public Uri Path { get; set; }
        public AttachmentType Type { get; set; }

        public bool IsPhoto => Type == AttachmentType.Photo;
        public string Filename => Path.Segments.Last() == "/" ? "filename-not-found" : Path.Segments.Last();
    }
}