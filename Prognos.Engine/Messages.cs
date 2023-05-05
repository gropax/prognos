using Prognos.Core;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Platform = Prognos.Core.Platform;

namespace Prognos.Engine
{
    public class RequestPage
    {
        public ScrapingCommand ScrapingCommand { get; }
        public RequestPage(ScrapingCommand scrapingCommand)
        {
            ScrapingCommand = scrapingCommand;
        }
    }

    public class PageGranted
    {
        public ScrapingCommand ScrapingCommand { get; }
        public Page Page { get; }
        public PageGranted(ScrapingCommand scrapingCommand, Page page)
        {
            ScrapingCommand = scrapingCommand;
            Page = page;
        }
    }

    public class PageDenied
    {
        public ScrapingCommand ScrapingCommand { get; }
        public PageDenied(ScrapingCommand scrapingCommand)
        {
            ScrapingCommand = scrapingCommand;
        }
    }

    public class RecyclePage
    {
        public IPage Page { get; }

        public RecyclePage(IPage page)
        {
            Page = page;
        }
    }
}
