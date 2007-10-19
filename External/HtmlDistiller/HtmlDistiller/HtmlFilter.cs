#region BuildTools License
/*---------------------------------------------------------------------------------*\

	BuildTools distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2007 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion BuildTools License

using System;

namespace BuildTools.HtmlDistiller.Filters
{
	/// <summary>
	/// Defines a set of HTML tag/attribute/style filters
	/// </summary>
	public interface IHtmlFilter
	{
		#region Methods

		/// <summary>
		/// Filters tags, optionally allowing altering of tag
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <returns>if tag should be rendered</returns>
		bool FilterTag(HtmlTag tag);

		/// <summary>
		/// Filters attributes, optionally allowing altering of attribute value
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <param name="attribute">attribute name</param>
		/// <param name="value">attribute value</param>
		/// <returns>if attribute should be rendered</returns>
		bool FilterAttribute(string tag, string attribute, ref string value);

		/// <summary>
		/// Filters styles, optionally allowing altering of style value
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <param name="attribute">style name</param>
		/// <param name="value">style value</param>
		/// <returns>if style should be rendered</returns>
		bool FilterStyle(string tag, string style, ref string value);

		/// <summary>
		/// Filters literals, optionally allowing replacement of literal value
		/// </summary>
		/// <param name="source">original string</param>
		/// <param name="start">starting index inclusive</param>
		/// <param name="end">ending index exclusive</param>
		/// <param name="replacement">a replacement string</param>
		/// <returns>true if <paramref name="replace"/> should be used to replace literal</returns>
		/// <remarks>
		/// This uses the original source string, start and end rather than passing a substring
		/// in order to not generate a strings for every literal.  The internals of HtmlDistiller
		/// do not produce extra strings for literals so for efficiency sake, care should be taken
		/// so that filters do not produce excessive extra strings either.
		/// </remarks>
		bool FilterLiteral(string source, int start, int end, out string replacement);

		#endregion Methods
	}

	/// <summary>
	/// HtmlFilter which strips all tags
	/// </summary>
	public class StripHtmlFilter : IHtmlFilter
	{
		#region IHtmlFilter Members

		/// <summary>
		/// Strips all tags
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public virtual bool FilterTag(HtmlTag tag)
		{
			return false;
		}

		/// <summary>
		/// Strips all attributes
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual bool FilterAttribute(string tag, string attribute, ref string value)
		{
			return false;
		}

		/// <summary>
		/// Strips all styles
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual bool FilterStyle(string tag, string style, ref string value)
		{
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		public virtual bool FilterLiteral(string source, int start, int end, out string replacement)
		{
			replacement = null;
			return false;
		}

		#endregion IHtmlFilter Members
	}

	/// <summary>
	/// HtmlFilter which allows only simple tags/attributes
	/// </summary>
	public class StrictHtmlFilter : IHtmlFilter
	{
		#region IHtmlFilter Members

		/// <summary>
		/// Allows a restrictive set of simple tags
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public virtual bool FilterTag(HtmlTag tag)
		{
			// whitelist of safe tags
			switch (tag.TagName)
			{
				case "a":
				case "b":
				case "blockquote":
				case "br":
				case "em":
				case "i":
				case "img":
				case "li":
				case "ol":
				case "strong":
				case "u":
				case "ul":
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Allows a restrictive set of simple attributes
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="attribute"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual bool FilterAttribute(string tag, string attribute, ref string value)
		{
			attribute = attribute.ToLowerInvariant();

			// whitelist of safe attributes
			switch (tag.ToLowerInvariant())
			{
				case "a":
				{
					switch (attribute)
					{
						case "href":
						case "target":
						{
							return true;
						}
					}
					return false;
				}
				case "img":
				{
					switch (attribute)
					{
						case "alt":
						case "src":
						case "title":
						{
							return true;
						}
					}
					return false;
				}
			}
			return false;
		}

		/// <summary>
		/// Strips all styles
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual bool FilterStyle(string tag, string style, ref string value)
		{
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		public virtual bool FilterLiteral(string source, int start, int end, out string replacement)
		{
			replacement = null;
			return false;
		}

		#endregion IHtmlFilter Members
	}

	/// <summary>
	/// HtmlFilter which allows only safe tags/attributes/styles
	/// </summary>
	public class SafeHtmlFilter : IHtmlFilter
	{
		#region Constants

		private const string WordBreak = "<wbr />&shy;";
		private readonly int MaxWordLength;

		#endregion Constants

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public SafeHtmlFilter() : this(0)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="maxWordLength"></param>
		public SafeHtmlFilter(int maxWordLength)
		{
			this.MaxWordLength = maxWordLength;
		}

		#endregion Init

		#region IHtmlFilter Members

		/// <summary>
		/// Allows a permissive set of safe tags
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <returns></returns>
		/// <remarks>
		/// http://www.w3.org/TR/html401/index/elements.html
		/// </remarks>
		public virtual bool FilterTag(HtmlTag tag)
		{
			// whitelist of safe tags
			switch (tag.TagName)
			{
				//case "!--":// comments can hold IE conditional logic
				case "a":
				case "abbr":
				case "acronym":
				case "address":
				case "area":
				case "b":
				case "bdo":
				case "bgsound":
				case "big":
				case "blink":
				case "blockquote":
				case "br":
				case "caption":
				case "center":
				case "cite":
				case "code":
				case "col":
				case "colgroup":
				case "dd":
				case "del":
				case "dfn":
				case "dir":
				case "div":
				case "dl":
				case "dt":
				case "em":
				case "fieldset":
				case "font":
				case "h1":
				case "h2":
				case "h3":
				case "h4":
				case "h5":
				case "h6":
				case "hr":
				case "i":
				case "iframe":
				case "img":
				case "ins":
				case "isindex":
				case "kbd":
				case "label":
				case "legend":
				case "li":
				case "map":
				case "marquee":
				case "menu":
				//case "meta": // use for redirects, is this safe?
				case "nobr":
				case "ol":
				case "p":
				case "pre":
				case "q":
				case "s":
				case "samp":
				case "small":
				case "sound":
				case "span":
				case "strike":
				case "strong":
				case "sub":
				case "sup":
				case "table":
				case "tbody":
				case "td":
				case "tfoot":
				case "th":
				case "thead":
				case "tr":
				case "tt":
				case "u":
				case "ul":
				case "var":
				case "wbr":
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Allows a permissive set of safe attributes
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <param name="attribute">attribute name</param>
		/// <param name="value">attribute value</param>
		/// <returns></returns>
		/// <remarks>
		/// http://www.w3.org/TR/html401/index/attributes.html
		/// </remarks>
		public virtual bool FilterAttribute(string tag, string attribute, ref string value)
		{
			attribute = attribute.ToLowerInvariant();
			if (attribute == "id" || attribute.StartsWith("on"))
			{
				// deny for all tags
				return false;
			}
			if (attribute == "class")
			{
				// allow for all tags
				return true;
			}

			// whitelist of safe attributes
			switch (tag.ToLowerInvariant())
			{
				case "a":
				{
					switch (attribute)
					{
						case "href":
						case "target":
						{
							return true;
						}
					}
					return false;
				}
				case "bgsound":
				case "sound":
				{
					switch (attribute)
					{
						case "balance":
						case "src":
						case "loop":
						case "volume":
						{
							return true;
						}
					}
					return false;
				}
				case "div":
				{
					switch (attribute)
					{
						case "align":
						{
							return true;
						}
					}
					return false;
				}
				case "font":
				{
					switch (attribute)
					{
						case "color":
						case "face":
						case "size":
						{
							return true;
						}
					}
					return false;
				}
				case "hr":
				{
					switch (attribute)
					{
						case "align":
						case "color":
						case "noshade":
						case "size":
						case "width":
						{
							return true;
						}
					}
					return false;
				}
				case "img":
				{
					switch (attribute)
					{
						case "alt":
						case "border":
						case "height":
						case "lowsrc":
						case "src":
						case "title":
						case "width":
						{
							return true;
						}
					}
					return false;
				}
				case "p":
				{
					switch (attribute)
					{
						case "align":
						{
							return true;
						}
					}
					return false;
				}
				case "ol":
				case "ul":
				{
					switch (attribute)
					{
						case "type":
						{
							return true;
						}
					}
					return false;
				}
				case "table":
				{
					switch (attribute)
					{
						case "bgcolor":
						case "border":
						case "bordercolor":
						case "cellpadding":
						case "cellspacing":
						case "height":
						case "width":
						{
							return true;
						}
					}
					return false;
				}
				case "td":
				case "th":
				{
					switch (attribute)
					{
						case "align":
						case "colspan":
						case "rowspan":
						case "bgcolor":
						case "bordercolor":
						case "height":
						case "width":
						{
							return true;
						}
					}
					return false;
				}
			}
			return false;
		}

		/// <summary>
		/// Allows a permissive set of safe attributes
		/// </summary>
		/// <param name="tag">tag name</param>
		/// <param name="style">style name</param>
		/// <param name="value">style value</param>
		/// <returns></returns>
		/// <remarks>
		/// http://www.w3.org/TR/CSS21/propidx.html
		/// </remarks>
		public virtual bool FilterStyle(string tag, string style, ref string value)
		{
			if (value != null &&
				value.IndexOf("expression", StringComparison.InvariantCultureIgnoreCase) >= 0)
			{
				// IE CSS expressions are JavaScript
				return false;
			}

			// blacklist of unsafe styles
			switch (style.ToLowerInvariant())
			{
				case "display":
				case "position":
				case "z-index":
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Optionally allows breaking of long words
		/// </summary>
		/// <param name="source"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		public virtual bool FilterLiteral(string source, int start, int end, out string replacement)
		{
			replacement = null;

			if (this.MaxWordLength <= 0)
			{
				return false;
			}

			int lastOutput = start,
				lastSpace = start;

			for (int i=start; i<end; i++)
			{
				if (Char.IsWhiteSpace(source[i]))
				{
					lastSpace = i;
				}
				if (i-lastSpace > this.MaxWordLength)
				{
					// append all before break
					replacement += source.Substring(lastOutput, i-lastOutput);
					replacement += SafeHtmlFilter.WordBreak;
					lastSpace = lastOutput = i;
				}
			}

			if (replacement != null)
			{
				if (lastOutput < end)
				{
					// append remaining string
					replacement += source.Substring(lastOutput, end-lastOutput);
				}

				// a replacement was generated
				return true;
			}

			// don't replace since didn't need to
			return false;
		}

		#endregion IHtmlFilter Members
	}

	/// <summary>
	/// HtmlFilter which allows all tags/attributes/styles
	/// </summary>
	public class UnsafeHtmlFilter : IHtmlFilter
	{
		#region IHtmlFilter Members

		/// <summary>
		/// Allows all tags
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public virtual bool FilterTag(HtmlTag tag)
		{
			return true;
		}

		/// <summary>
		/// Allows all attributes
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual bool FilterAttribute(string tag, string attribute, ref string value)
		{
			return true;
		}

		/// <summary>
		/// Allows all styles
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual bool FilterStyle(string tag, string style, ref string value)
		{
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		public virtual bool FilterLiteral(string source, int start, int end, out string replacement)
		{
			replacement = null;
			return false;
		}

		#endregion IHtmlFilter Members
	}
}
