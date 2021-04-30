using System;

namespace VenturaSQLStudio.Pages
{
    class SnippetListItem
    {

        public string Title { get; set; }

        public Func<SnippetCreatorBase> DoCreate { get; set; }


    }
}
