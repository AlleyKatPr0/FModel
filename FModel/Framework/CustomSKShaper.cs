using System;
using HarfBuzzSharp;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using Buffer = HarfBuzzSharp.Buffer;

namespace FModel.Framework;

public class CustomSKShaper : SKShaper
{
    private const int _FONT_SIZE_SCALE = 512;
    private readonly Font _font;
    private readonly Buffer _buffer = new Buffer();

    private SKPoint[] _points = [];
    private uint[] _clusters = [];
    private uint[] _codepoints = [];

    public CustomSKShaper(SKTypeface typeface) : base(typeface)
    {
        using var blob = Typeface.OpenStream(out var index).ToHarfBuzzBlob();
        using var face = new Face(blob, index);
        face.Index = index;
        face.UnitsPerEm = Typeface.UnitsPerEm;

        _font = new Font(face);
        _font.SetScale(_FONT_SIZE_SCALE, _FONT_SIZE_SCALE);
        _font.SetFunctionsOpenType();
    }

    public new Result Shape(Buffer buffer, float xOffset, float yOffset, SKPaint paint)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        if (paint == null)
            throw new ArgumentNullException(nameof(paint));

        // do the shaping
        _font.Shape(buffer);

        // get the shaping results
        var len = buffer.Length;
        var info = buffer.GlyphInfos;
        var pos = buffer.GlyphPositions;

        // get the sizes
        var textSizeY = paint.TextSize / _FONT_SIZE_SCALE;
        var textSizeX = textSizeY * paint.TextScaleX;

        if (len != _points.Length)
        {
            _points = new SKPoint[len];
            _clusters = new uint[len];
            _codepoints = new uint[len];
        }

        for (var i = 0; i < len; i++)
        {
            // move the cursor
            xOffset += pos[i].XAdvance * textSizeX;
            yOffset += pos[i].YAdvance * textSizeY;

            _codepoints[i] = info[i].Codepoint;
            _clusters[i] = info[i].Cluster;
            _points[i] = new SKPoint(xOffset + pos[i].XOffset * textSizeX, yOffset - pos[i].YOffset * textSizeY);
        }

        return new Result(_codepoints, _clusters, _points, _points[^1].X);
    }

    public new Result Shape(string text, SKPaint paint) => Shape(text, 0, 0, paint);

    public new Result Shape(string text, float xOffset, float yOffset, SKPaint paint)
    {
        if (string.IsNullOrEmpty(text))
            return new Result();

        _buffer.Reset();
        switch (paint.TextEncoding)
        {
            case SKTextEncoding.Utf8:
                _buffer.AddUtf8(text);
                break;
            case SKTextEncoding.Utf16:
                _buffer.AddUtf16(text);
                break;
            case SKTextEncoding.Utf32:
                _buffer.AddUtf32(text);
                break;
            default:
                throw new NotSupportedException("TextEncoding of type GlyphId is not supported.");
        }

        _buffer.GuessSegmentProperties();
        return Shape(_buffer, xOffset, yOffset, paint);
    }
}
