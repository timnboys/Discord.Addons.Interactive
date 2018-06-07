using System.Collections.Generic;

namespace Discord.Addons.Interactive
{
    public class PaginatedMessage
    {
        public IEnumerable<Page> Pages { get; set; }

        public string Content { get; set; } = "";

        public EmbedAuthorBuilder Author { get; set; } = null;
        public Color Color { get; set; } = Color.Default;
        public string Title { get; set; } = "";
        public string DefaultImageUrl { get; set; } = null;

        public class Page
        {
            public string Description { get; set; } = "";
            public string ImageUrl { get; set; } = null;
            public string Title { get; set; } = null;
            public string Url { get; set; } = null;
            public List<EmbedFieldBuilder> Fields { get; set; } = new List<EmbedFieldBuilder>();
        }
        public PaginatedAppearanceOptions Options { get; set; } = PaginatedAppearanceOptions.Default;
    }
}
