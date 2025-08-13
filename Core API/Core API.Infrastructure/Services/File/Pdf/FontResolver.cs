using PdfSharp.Fonts;

namespace Core_API.Infrastructure.Services.File.Pdf
{
    public class FontResolver(string fontDirectory) : IFontResolver
    {
        private readonly string _fontDirectory = fontDirectory ?? throw new ArgumentNullException(nameof(fontDirectory));
        private readonly Dictionary<string, byte[]> _fontCache = [];

        public byte[] GetFont(string faceName)
        {
            string fontFileName = faceName.ToLower() switch
            {
                "open sans regular" => "OpenSans-Regular.ttf",
                "open sans bold" => "OpenSans-Bold.ttf",
                _ => throw new ArgumentException($"Font '{faceName}' not supported.")
            };

            if (_fontCache.TryGetValue(fontFileName, out var fontData))
            {
                return fontData;
            }

            var fontPath = Path.Combine(_fontDirectory, fontFileName);
            if (!System.IO.File.Exists(fontPath)) // You could also use the fully qualified name
            {
                throw new FileNotFoundException($"Font file not found: {fontPath}");
            }

            fontData = System.IO.File.ReadAllBytes(fontPath);
            _fontCache[fontFileName] = fontData;
            return fontData;
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("Open Sans", StringComparison.OrdinalIgnoreCase))
            {
                if (isBold && !isItalic)
                    return new FontResolverInfo("Open Sans Bold");
                return new FontResolverInfo("Open Sans Regular");
            }

            throw new ArgumentException($"Font family '{familyName}' not supported.");
        }
    }
}
