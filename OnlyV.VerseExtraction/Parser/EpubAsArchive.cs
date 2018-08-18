namespace OnlyV.VerseExtraction.Parser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using Exceptions;
    using Models;
    using Serilog;

    internal sealed class EpubAsArchive : IDisposable
    {
        private const string MetaInfFolderName = "META-INF";
        private const string ContainerFileName = "container.xml";
        private const string NavigationDocumentName = "biblebooknav.xhtml";
        private const string NavigationDocumentOldName = "BIBLE_00.xhtml";

        private readonly Lazy<ZipArchive> _zip;
        private readonly Lazy<string> _rootPath;
        private readonly Lazy<XDocument> _navigationDocument;
        private readonly string _epubPath;
        private EpubStyle _epubStyle;

        public EpubAsArchive(string epubPath)
        {
            _epubPath = epubPath;

            _zip = new Lazy<ZipArchive>(() => ZipFile.OpenRead(epubPath));
            _rootPath = new Lazy<string>(GetRootFilePath);
            _navigationDocument = new Lazy<XDocument>(GetBibleNavigationDoc);
        }

        public void Dispose()
        {
            if (_zip.IsValueCreated)
            {
                _zip.Value.Dispose();
            }
        }


        public List<BookChapter> GenerateBibleChaptersList(IReadOnlyList<BibleBook> bibleBooks)
        {
            Log.Logger.Information("Initialising chapters");

            var result = new List<BookChapter>();

            foreach (var book in bibleBooks)
            {
                Log.Logger.Debug($"Book = {book.BookNumber}, {book.BookName}, {book.FullPath}");

                if (book.HasSingleChapter)
                {
                    var bc = new BookChapter
                    {
                        FullPath = book.FullPath,
                        Book = book,
                        Chapter = 1,
                        VerseRange = GetVerseNumbers(book.FullPath)
                    };

                    Log.Logger.Debug($"Chapter = {bc.Chapter}, {bc.FullPath}");
                    result.Add(bc);
                }
                else
                {
                    var x = GetXmlFile(book.FullPath);

                    var attr = x?.Root?.Attribute("xmlns");
                    if (attr != null)
                    {
                        XNamespace ns = attr.Value;

                        var body = x.Root.Descendants(ns + "body").SingleOrDefault();
                        if (body != null)
                        {
                            result.AddRange(GetBookChapters(ns, book, body));
                        }
                    }
                }
            }

            return result;
        }

        public IReadOnlyList<BibleBook> GenerateBibleBooksList()
        {
            Log.Logger.Information("Initialising books");

            var nav = _navigationDocument.Value;

            var result = new List<BibleBook>();

            var attr = nav.Root?.Attribute("xmlns");
            if (attr != null)
            {
                XNamespace ns = attr.Value;

                var body = nav.Root.Descendants(ns + "body").SingleOrDefault();
                if (body != null)
                {
                    var books = body.Descendants(ns + "a").Where(n => n.Attribute("href") != null);
                    int bookNum = 1;

                    foreach (var book in books)
                    {
                        string href = book.Attribute("href")?.Value;
                        if (href != null && href.EndsWith(".xhtml"))
                        {
                            var bb = new BibleBook
                            {
                                FullPath = GetFullPathInArchive(href),
                                BookName = book.Value.Trim(),
                                BookNumber = bookNum++
                            };

                            if (bb.HasSingleChapter)
                            {
                                var scp = GetSingleChapterPath(bb.FullPath);
                                if (!string.IsNullOrEmpty(scp))
                                {
                                    // older formats...
                                    bb.FullPath = GetFullPathInArchive(GetSingleChapterPath(bb.FullPath));
                                }
                            }

                            Log.Logger.Debug($"Book = {bb.BookNumber}, {bb.BookName}, {bb.FullPath}");

                            result.Add(bb);
                        }
                    }

                    if (result.Count == BibleBooksMetaData.NumBibleBooksGreek)
                    {
                        // renumber...
                        foreach (var book in result)
                        {
                            book.BookNumber = book.BookNumber + BibleBooksMetaData.NumBibleBooksHebrew;
                        }
                    }
                }
            }

            return result;
        }

        private XDocument GetXmlFile(string entryPath)
        {
            XDocument result = null;

            var entry = _zip.Value.GetEntry(entryPath);
            if (entry != null)
            {
                using (var stream = entry.Open())
                {
                    result = XDocument.Load(stream);
                }
            }

            return result;
        }

        private string GetFullPathInArchive(string path)
        {
            return $"{_rootPath.Value}/{path}";
        }

        private string GetRootFilePath()
        {
            string result = null;

            var entryPath = string.Concat(MetaInfFolderName, "/", ContainerFileName);

            var x = GetXmlFile(entryPath);
            var attr = x?.Root?.Attribute("xmlns");
            if (attr != null)
            {
                XNamespace ns = attr.Value;

                var elements = (from item in x.Descendants(ns + "rootfile") select item).ToArray();
                if (elements.Length == 1)
                {
                    var path = elements[0].Attribute("full-path");
                    if (path != null)
                    {
                        // we assume that the content.opf file is in the same folder as the rest of the content
                        result = Path.GetDirectoryName(path.Value);
                    }
                }
            }

            return result;
        }

        private XDocument GetBibleNavigationDoc()
        {
            var x = GetXmlFile(GetFullPathInArchive(NavigationDocumentName));
            if (x != null)
            {
                _epubStyle = EpubStyle.New;
            }
            else
            {
                x = GetXmlFile(GetFullPathInArchive(NavigationDocumentOldName));
                _epubStyle = x != null ? EpubStyle.Old : EpubStyle.Unknown;
            }

            if (x == null)
            {
                throw new EpubCompatibilityException();
            }

            return x;
        }

        private string GetSingleChapterPath(string path)
        {
            string result = null;

            var x = GetXmlFile(path);

            var attr = x?.Root?.Attribute("xmlns");
            if (attr != null)
            {
                XNamespace ns = attr.Value;

                var body = x.Root.Descendants(ns + "body").SingleOrDefault();

                var chapterToken = "#chapter1_verse";
                var verseElem = body?.Descendants(ns + "a").FirstOrDefault(n =>
                {
                    var href = n.Attribute("href");
                    return href != null && href.Value.Contains(chapterToken);
                });

                if (verseElem != null)
                {
                    var href = verseElem.Attribute("href");
                    if (href != null)
                    {
                        var docRef = href.Value;

                        var index = docRef.IndexOf("#", StringComparison.OrdinalIgnoreCase);
                        if (index > -1)
                        {
                            result = docRef.Substring(0, index);
                        }
                    }
                }
            }

            return result;
        }

        private VerseRange GetVerseNumbers(string fullPath)
        {
            switch (_epubStyle)
            {
                case EpubStyle.New:
                    return GetVerseNumbersNewStyle(fullPath);

                case EpubStyle.Old:
                    return GetVerseNumbersOldStyle(fullPath);
            }

            return null;
        }

        private VerseRange GetVerseNumbersNewStyle(string fullPath)
        {
            VerseRange result = null;

            var x = GetXmlFile(fullPath);

            var attr = x?.Root?.Attribute("xmlns");
            if (attr != null)
            {
                XNamespace ns = attr.Value;

                var body = x.Root.Descendants(ns + "body").SingleOrDefault();
                var verseRange = body?.Descendants(ns + "a").FirstOrDefault(n =>
                {
                    var xAttribute = n.Attribute("href");
                    return xAttribute != null && xAttribute.Value.StartsWith("bibleversenav");
                });

                if (verseRange != null)
                {
                    result = GetVerseNumbers(verseRange.Value);
                }
            }

            return result;
        }

        private VerseRange GetVerseNumbersOldStyle(string fullPath)
        {
            VerseRange result = null;

            var x = GetXmlFile(fullPath);

            var attr = x?.Root?.Attribute("xmlns");
            if (attr != null)
            {
                XNamespace ns = attr.Value;

                var body = x.Root.Descendants(ns + "body").SingleOrDefault();

                var chapterToken = "chapter";
                var verseElems = body?.Descendants(ns + "a").Where(n =>
                {
                    var idAttr = n.Attribute("id");
                    return idAttr != null && idAttr.Value.StartsWith(chapterToken);
                });

                if (verseElems != null)
                { 
                    int chapterNumber = 0;
                    foreach (var verseElem in verseElems)
                    {
                        var idElem = verseElem.Attribute("id");
                        if (idElem != null)
                        {
                            if (chapterNumber == 0)
                            {
                                var chapterNumberString = idElem.Value.Substring(chapterToken.Length);
                                int.TryParse(chapterNumberString, out chapterNumber);
                            }
                            else
                            {
                                var token = $"{chapterToken}{chapterNumber}_verse";
                                if (idElem.Value.StartsWith(token))
                                {
                                    var verseNumberString = idElem.Value.Substring(token.Length);

                                    if (int.TryParse(verseNumberString, out var verseNum))
                                    {
                                        if (result == null)
                                        {
                                            result = new VerseRange { FirstVerse = int.MaxValue };
                                        }

                                        result.FirstVerse = Math.Min(result.FirstVerse, verseNum);
                                        result.LastVerse = Math.Max(result.LastVerse, verseNum);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private List<BookChapter> GetBookChaptersNewStyle(XNamespace ns, BibleBook book, XElement body)
        {
            var result = new List<BookChapter>();

            if (_epubStyle == EpubStyle.New)
            {
                var chapters = body.Descendants(ns + "a").Where(n => n.Attribute("href") != null);
                
                foreach (var chapter in chapters)
                {
                    var href = chapter.Attribute("href")?.Value;

                    if (NumberParser.TryParseNumber(chapter.Value, out var chapterNumber))
                    { 
                        var bc = new BookChapter
                        {
                            FullPath = GetFullPathInArchive(href),
                            Book = book,
                            Chapter = chapterNumber
                        };

                        bc.VerseRange = GetVerseNumbers(bc.FullPath);

                        Log.Logger.Debug($"Chapter = {bc.Chapter}, {bc.FullPath}");
                        result.Add(bc);
                    }
                }
            }

            return result;
        }

        private List<BookChapter> GetBookChaptersOldStyle(XNamespace ns, BibleBook book, XElement body)
        {
            var result = new List<BookChapter>();

            var linkToken = "link";

            var chapters = body.Descendants(ns + "a").Where(n =>
            {
                if (n.Parent != null)
                {
                    var idAttr = n.Attribute("id");
                    if (idAttr != null)
                    {
                        var parentClassAttr = n.Parent.Attribute("class");
                        if (parentClassAttr != null)
                        {
                            return idAttr.Value.StartsWith(linkToken) &&
                                   n.Parent.Name.Equals(ns + "p") &&
                                   parentClassAttr.Value.Equals("se");
                        }
                    }
                }

                return false;
            });

            foreach (var chapter in chapters)
            {
                var chapterNumStr = chapter.Attribute("id")?.Value.Substring(linkToken.Length);
                if (!string.IsNullOrEmpty(chapterNumStr))
                {
                    if (int.TryParse(chapterNumStr, out var chapterNum))
                    {
                        ++chapterNum;
                        var nextNode = chapter.NextNode;
                        if (nextNode != null && nextNode.NodeType == XmlNodeType.Element)
                        {
                            var nodeElem = (XElement)nextNode;
                            var xAttribute = nodeElem.Attribute("href");
                            if (xAttribute != null)
                            {
                                int pos = xAttribute.Value.IndexOf("#", StringComparison.Ordinal);
                                if (pos > 0)
                                {
                                    string href = xAttribute.Value.Substring(0, pos);

                                    BookChapter bc = new BookChapter
                                    {
                                        FullPath = GetFullPathInArchive(href),
                                        Book = book,
                                        Chapter = chapterNum
                                    };

                                    bc.VerseRange = GetVerseNumbers(bc.FullPath);

                                    Log.Logger.Debug($"Chapter = {bc.Chapter}, {bc.FullPath}");
                                    result.Add(bc);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
        
        private List<BookChapter> GetBookChapters(XNamespace ns, BibleBook book, XElement body)
        {
            if (_epubStyle == EpubStyle.New)
            {
                return GetBookChaptersNewStyle(ns, book, body);
            }

            if (_epubStyle == EpubStyle.Old)
            {
                return GetBookChaptersOldStyle(ns, book, body);
            }

            return null;
        }
    }
}
