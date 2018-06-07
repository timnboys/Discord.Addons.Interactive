using System;
using System.Collections.Generic;

namespace Discord.Addons.Interactive
{
    public class PaginatedMessage
    {
        public IEnumerable<Page> Pages { get; set; }

        public string Content { get; set; } = "";
        public EmbedAuthorBuilder Author { get; set; } = null;
        public string Title { get; set; } = null;
        public string Url { get; set; } = null;

        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = null;
        public string ThumbnailUrl { get; set; } = null;

        public List<EmbedFieldBuilder> Fields { get; set; } = new List<EmbedFieldBuilder>();
        public EmbedFooterBuilder FooterOverride { get; set; } = null;
        public DateTimeOffset? TimeStamp { get; set; } = null;

        public Color Color = Color.Default;

        public class Page
        {
            //All content in here will override the 'Default' Paginated content
            public EmbedAuthorBuilder Author { get; set; } = null;
            public string Title { get; set; } = null;
            public string Url { get; set; } = null;

            public string Description { get; set; } = null;
            public string ImageUrl { get; set; } = null;
            public string ThumbnailUrl { get; set; } = null;

            public List<EmbedFieldBuilder> Fields { get; set; } = new List<EmbedFieldBuilder>();
            public EmbedFooterBuilder FooterOverride { get; set; } = null;
            public DateTimeOffset? TimeStamp { get; set; } = null;

            public Color? Color = null;
        }
        public PaginatedAppearanceOptions Options { get; set; } = PaginatedAppearanceOptions.Default;
    }
}
