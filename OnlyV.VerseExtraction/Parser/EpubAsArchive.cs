namespace OnlyV.VerseExtraction.Parser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Exceptions;
    using Models;
    using Serilog;
    using Utils;

    internal sealed class EpubAsArchive : IDisposable
    {
        private const string MetaInfFolderName = "META-INF";
        private const string ContainerFileName = "container.xml";
        private const string NavigationDocumentName = "biblebooknav.xhtml";
        private const string NavigationDocumentOldName = "BIBLE_00.xhtml";
        private const string Ellipises = "...";

        private readonly Lazy<ZipArchive> _zip;
        private readonly Lazy<string> _rootPath;
        private readonly Lazy<XDocument> _navigationDocument;
        private EpubStyle _epubStyle;

        public EpubAsArchive(string epubPath)
        {
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

        public string GetBibleTexts(
            IReadOnlyList<BookChapter> chapers,
            int bibleBook,
            ChapterAndVersesSpec chapterAndVerses,
            FormattingOptions formattingOptions)
        {
            bool showVerseNumbers = formattingOptions.IncludeVerseNumbers;

            if (!chapterAndVerses.HasMultipleVerses())
            {
                showVerseNumbers = false;
            }

            var result = new StringBuilder();

            foreach (var vs in chapterAndVerses.ContiguousVerses)
            {
                if (result.Length > 0 && formattingOptions.ShowBreakInVerses)
                {
                    char lastChar = result.ToString().Trim().Last();
                    if (lastChar == '.')
                    {
                        // looks odd if we have an ellipses straight afetr a full stop!
                        var tmpStr = result.ToString().Trim();
                        tmpStr = tmpStr.Remove(tmpStr.Length - 1, 1);
                        result.Clear();
                        result.Append(tmpStr);
                        result.Append(Ellipises);
                        result.Append(" ");
                    }
                    else
                    {
                        result.Append(Ellipises);
                    }

                    result.Append(" ");
                }

                for (var verse = vs.StartVerse; verse <= vs.EndVerse; ++verse)
                {
                    string s = GetBibleText(chapers, bibleBook, vs.Chapter, verse, formattingOptions);
                    if (!string.IsNullOrEmpty(s))
                    {
                        if (verse > vs.StartVerse)
                        {
                            result.Append(" ");
                        }

                        if (showVerseNumbers)
                        {
                            result.Append($"|{verse}|");
                        }

                        result.Append(s);
                    }
                }
            }

            return TrimPunctuationAndQuotationMarks(result.ToString(), formattingOptions);
        }

        public string GetBibleText(
            IReadOnlyList<BookChapter> chapters, 
            int bibleBook, 
            int chapter, 
            int verse,
            FormattingOptions formattingOptions)
        {
            var x = GetChapter(chapters, bibleBook, chapter);

            var attr = x?.Root?.Attribute("xmlns");
            if (attr == null)
            {
                return null;
            }

            XNamespace ns = attr.Value;
            var body = x.Root.Descendants(ns + "body").SingleOrDefault();
            if (body == null)
            {
                return null;
            }
            
            var idThisVerse = $"chapter{chapter}_verse{verse}";
            var idNextVerse = $"chapter{chapter}_verse{verse + 1}";

            var elem = body.Descendants().SingleOrDefault(n =>
            {
                var xAttribute = n.Attribute("id");
                return xAttribute != null && xAttribute.Value.Equals(idThisVerse);
            });

            var parentPara = elem?.Parent;
            if (parentPara == null)
            {
                return null;
            }
            
            var sb = new StringBuilder();

            var paras = new List<XElement> { parentPara };
            paras.AddRange(parentPara.ElementsAfterSelf());

            bool started = false;
            bool finished = false;

            foreach (var para in paras)
            {
                var result = GetParagraph(
                    para, 
                    parentPara, 
                    ns, 
                    idThisVerse, 
                    idNextVerse, 
                    formattingOptions,
                    ref started,
                    ref finished);

                sb.Append(result.Text);

                if (result.Finished)
                {
                    break;
                }
            }

            return sb.ToString().Trim().Trim('~');
        }

        private (string Text, bool Finished) GetParagraph(
            XElement para, 
            XElement parentPara,
            XNamespace ns,
            string idThisVerse,
            string idNextVerse,
            FormattingOptions formattingOptions,
            ref bool started,
            ref bool finished)
        {
            var sb = new StringBuilder();

            if (para != parentPara)
            {
                // typically in the Psalms where a verse may contain 2 or more paras...
                sb.Append(formattingOptions.UseTildeSeparator ? " ~ " : " ");
            }

            var nodes = para.DescendantNodes();
            foreach (var node in nodes)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Element:
                        XElement elem2 = (XElement)node;
                        if (elem2.Name.Equals(ns + "div"))
                        {
                            finished = true;
                        }
                        else
                        {
                            var idAtt = elem2.Attribute("id");
                            if (idAtt != null)
                            {
                                if (started)
                                {
                                    finished = idAtt.Value.StartsWith(idNextVerse);
                                }
                                else
                                {
                                    started = idAtt.Value.StartsWith(idThisVerse);
                                }
                            }
                        }

                        break;

                    case XmlNodeType.Text:
                        if (started)
                        {
                            XText txtNode = (XText)node;
                            if (ShouldIncludeTextNode(txtNode, ns))
                            {
                                sb.Append(txtNode.Value);
                            }
                        }

                        break;
                }

                if (finished)
                {
                    break;
                }
            }

            return (sb.ToString(), finished);
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
                    result = GetVerseNumbers(verseRange);
                }
            }

            return result;
        }

        private VerseRange GetVerseNumbers(XElement verseHref)
        {
            VerseRange result = null;

            if (verseHref != null)
            {
                result = new VerseRange();

                string[] tokens = verseHref.Value.Split(' ');
                foreach (var token in tokens)
                {
                    if (NumberParser.TryParseNumber(token, out int verseNum))
                    {
                        if (result.FirstVerse == 0)
                        {
                            result.FirstVerse = verseNum;
                        }
                        else if (result.LastVerse == 0)
                        {
                            result.LastVerse = verseNum;
                        }
                    }

                    if (result.IsValid())
                    {
                        break;
                    }
                }
            }

            return result != null && result.IsValid() ? result : null;
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
                                var pos = xAttribute.Value.IndexOf("#", StringComparison.Ordinal);
                                if (pos > 0)
                                {
                                    var href = xAttribute.Value.Substring(0, pos);

                                    var bc = new BookChapter
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
            switch (_epubStyle)
            {
                case EpubStyle.New:
                    return GetBookChaptersNewStyle(ns, book, body);

                case EpubStyle.Old:
                    return GetBookChaptersOldStyle(ns, book, body);
            }

            return null;
        }

        private XDocument GetChapter(IReadOnlyList<BookChapter> chapters, int book, int chapter)
        {
            Log.Logger.Debug($"Get chapter (book={book}, chapter={chapter}");

            var c = chapters.FirstOrDefault(n => n.Book.BookNumber == book && n.Chapter == chapter);
            if (c != null)
            {
                return GetXmlFile(c.FullPath);
            }

            return null;
        }

        private bool ShouldIncludeTextNode(XText txtNode, XNamespace ns)
        {
            int dummy;

            if (!string.IsNullOrEmpty(txtNode.Value) && !int.TryParse(txtNode.Value, out dummy))
            {
                var parentNodeName = txtNode.Parent?.Name;
                if (parentNodeName == null || (!parentNodeName.Equals(ns + "strong") &&
                                               !parentNodeName.Equals(ns + "sup") &&
                                               !parentNodeName.Equals(ns + "a")))
                {
                    return true;
                }
            }

            return false;
        }

        private string TrimPunctuationAndQuotationMarks(
            string s, 
            FormattingOptions formattingOptions)
        {
            if (formattingOptions.TrimQuotes)
            {
                s = TrimQuotes(s);
            }

            if (formattingOptions.TrimPunctuation)
            {
                var punctChars = s.Where(char.IsPunctuation).ToArray();
                s = s.Trim(punctChars);
            }

            return s;
        }

        private string TrimQuotes(string s)
        {
            var leftQuote = "“";
            var rightQuote = "”";

            var leftCount = s.Count(x => x == leftQuote[0]);
            var rightCount = s.Count(x => x == rightQuote[0]);

            if (leftCount != rightCount)
            {
                while (leftCount > rightCount)
                {
                    s = ReplaceFirst(s, leftQuote, string.Empty);
                    --leftCount;
                }

                while (rightCount > leftCount)
                {
                    s = ReplaceLast(s, rightQuote, string.Empty);
                    --rightCount;
                }
            }

            return s;
        }

        private string ReplaceFirst(string text, string search, string replace)
        {
            var pos = text.IndexOf(search, StringComparison.Ordinal);
            if (pos < 0)
            {
                return text;
            }

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        private string ReplaceLast(string text, string search, string replace)
        {
            var pos = text.LastIndexOf(search, StringComparison.Ordinal);
            if (pos < 0)
            {
                return text;
            }

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}
