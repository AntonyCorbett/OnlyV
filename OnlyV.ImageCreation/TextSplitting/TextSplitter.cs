using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using OnlyV.ImageCreation.Exceptions;

namespace OnlyV.ImageCreation.TextSplitting
{
    internal class TextSplitter
    {
        private readonly string _originalText;
        private readonly Func<string, Size> _measureStringFunc;
        
        public TextSplitter(string originalText, Func<string, Size> measureStringFunc)
        {
            _originalText = originalText;
            _measureStringFunc = measureStringFunc;
        }
        
        public IReadOnlyCollection<string> GetLines(double width)
        {
            var result = new List<string>();

            var q = new Queue<string>();
            var words = _originalText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                q.Enqueue(word);
            }

            var sb = new StringBuilder();
            string currentLine = null;
            while (q.Any())
            {
                var word = q.Peek();
                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }

                sb.Append(word);

                var sz = _measureStringFunc(sb.ToString());

                if (sz.Width > width)
                {
                    // too big...
                    if (currentLine != null)
                    {
                        result.Add(currentLine.Trim());
                        currentLine = null;
                        sb.Clear();
                    }
                    else
                    {
                        throw new TextTooLargeException(Properties.Resources.TEXT_TOO_LARGE);
                    }
                }
                else
                {
                    q.Dequeue();
                    currentLine = sb.ToString();
                }
            }

            if (currentLine != null)
            {
                result.Add(currentLine.Trim());
            }

            return result;
        }
    }
}
