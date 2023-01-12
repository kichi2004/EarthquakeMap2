using System.Drawing.Drawing2D;
using System.Drawing.Text;
using EarthquakeLibrary;
using EarthquakeMap2.Objects;
using EewLibrary;

namespace EarthquakeMap2.Drawing;

public static class SideInfo
{
    private static Graphics GetGraphics(Bitmap bmp)
    {
        var g = Graphics.FromImage(bmp);
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        return g;
    }

    public static Bitmap DrawEew(EEW eew)
    {
        var koruriFont = new FontFamily("Koruri Regular");
        Font font9 = new(koruriFont, 9),
            font10 = new(koruriFont, 10),
            font12 = new(koruriFont, 12),
            font14 = new(koruriFont, 14),
            font16 = new(koruriFont, 16),
            font40 = new(koruriFont, 40),
            font20 = new(koruriFont, 20),
            font23 = new(koruriFont, 23),
            font20b = new(koruriFont, 20, FontStyle.Bold),
            robotoI = new(Form1.RobotoFont, 40, FontStyle.Bold | FontStyle.Italic);
        var sfCenterCenter = new StringFormat
            { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        var sfCenterFar = new StringFormat
            { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
        var secondary = Color.FromArgb(191, 191, 191);
        
        var bmp = new Bitmap(260, 160);
        var g = GetGraphics(bmp);

        var maxIntensity = eew.MaxInt;
        if (!Form1.Colors.TryGetValue(maxIntensity.EnumOrder, out var color)) color = Color.White;
        var textColor = maxIntensity.EnumOrder is -1 or >= 3 and <= 5 ? Color.Black : Color.White;
        g.FillRectangle(new SolidBrush(Color.FromArgb(130, Color.Black)), 0, 0, 260, 160);
        g.FillRectangle(new SolidBrush(color), 0, 56, 147, 55);
        var last = eew.Status is Statuses.Last or Statuses.CorrectionLast ? "(最終)" : "";
        TextRenderer.DrawText(g, "緊急地震速報", font23, new Rectangle(-4, 0, 260, 40), Color.White, TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, eew.IsWarn ? "（警報）" : "（予報）", font14, new Point(180, 25), Color.White, TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, $"第{eew.Number}報{last}", font9, new Point(0, 40), Color.White);
        var s = maxIntensity.ShortString.Replace("-", "").Replace("+", "");
        g.DrawString("最大震度", font14, new SolidBrush(textColor), 0, 85);
        if (maxIntensity.Equals(Intensity.Unknown))
        {
            g.DrawString("不明", font20, Brushes.Black, new Point(80, 67));
        }
        else if (maxIntensity.ShortString.Length == 2)
        {
            g.DrawString(s, robotoI, new SolidBrush(textColor),
                new Rectangle(79, 60, 35, 53), sfCenterCenter);
            var pmChar = maxIntensity.LongString.Last().ToString();
            g.DrawString(pmChar, font20b, new SolidBrush(textColor),
                new Rectangle(104, 62, 45, 53), sfCenterFar);
        }
        else
        {
            g.DrawString(s, robotoI, new SolidBrush(textColor),
                new Rectangle(74, 62, 80, 53), sfCenterCenter);
        }

        TextRenderer.DrawText(g, "M", font20, new Point(145, 76), Color.White);
        TextRenderer.DrawText(g, $"{eew.Magnitude:0.0}", font40, new Point(166, 47), Color.White);
        TextRenderer.DrawText(g, $"{eew.DetectionTime:HH:mm:ss}発生", font9,
            new Rectangle(0, 40, 260, 15), Color.White, TextFormatFlags.Right);
        TextRenderer.DrawText(g, "震源地", font10, new Point(1, 115), secondary);
        TextRenderer.DrawText(g, eew.Epicenter, eew.Epicenter!.Length < 8 ? font16 : font12,
            new Rectangle(1, 130, 170, 27), Color.White, TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, "深さ", font10, new Point(174, 115), secondary);
        TextRenderer.DrawText(g, $"{eew.Depth}km", font16, new Point(172, 129), Color.White);

        return bmp;
    }
    public static Bitmap? DrawInformation(EarthquakeInformation info)
    {
        var koruriFont = new FontFamily("Koruri Regular");
        Font font9 = new(koruriFont, 9),
            font10 = new(koruriFont, 10),
            font11 = new(koruriFont, 11),
            font12 = new(koruriFont, 12),
            font13 = new(koruriFont, 13),
            font14 = new(koruriFont, 14),
            font16 = new(koruriFont, 16),
            font20 = new(koruriFont, 20),
            font20B = new(koruriFont, 20, FontStyle.Bold),
            robotoI = new(Form1.RobotoFont, 40, FontStyle.Bold | FontStyle.Italic);
        var sfCenterCenter = new StringFormat
            { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        var sfCenterFar = new StringFormat
            { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
        var secondary = Color.FromArgb(191, 191, 191);

        var infoType = info.Type switch
        {
            EarthquakeInformationType.SeismicIntensityInformation => "震度速報",
            EarthquakeInformationType.HypocenterInformation => "震源情報",
            EarthquakeInformationType.HypocenterAndSeismicIntensityInformation => "各地の震度",
            _ => null
        };
        if (infoType == null) return null;
        
        if (info.Type == EarthquakeInformationType.SeismicIntensityInformation)
        {
            var img2 = new Bitmap(149, 80);
            using var g2 = GetGraphics(img2);
            g2.FillRectangle(new SolidBrush(Color.FromArgb(130, Color.Black)), 0, 0, 149, 80);
            g2.DrawRectangle(Pens.White, new Rectangle(8, 5, 131, 45));
            TextRenderer.DrawText(g2, "震度速報", font20, new Rectangle(2, 0, 145, 55),
                Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            TextRenderer.DrawText(g2, $"{info.Earthquake.OriginTime:HH時mm分}頃発生", font12, new Rectangle(0, 54, 140, 20), Color.White);
            return img2;
        }

        var isHuge = info.Earthquake.Magnitude == "8を超える巨大地震";
        var width = isHuge ? 455 : 360;
        var img = new Bitmap(width, 102);
        using var g = GetGraphics(img);

        g.FillRectangle(new SolidBrush(Color.FromArgb(130, Color.Black)), 0, 0, width, 102);
        TextRenderer.DrawText(g, $"{info.Earthquake.OriginTime:d日H時mm分}頃発生", font9, new Point(0, 55), Color.White);
        TextRenderer.DrawText(g, "震源地", font10, new Point(150, 0), secondary);
        var epicenter = info.Earthquake.Hypocenter!.Name;
        TextRenderer.DrawText(g, epicenter, epicenter.Length < 8 ? font16 : font14,
            new Rectangle(160, 15, 200, 27), Color.White, TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, "深さ", font10, new Point(150, 51), secondary);
        var depth = info.Earthquake.Hypocenter!.Depth switch
        {
            null => "----",
            600 => "≧ 600km",
            0 => "ごく浅い",
            { } x => $"約{x}km"
        };
        TextRenderer.DrawText(g, depth, font14, new Point(180, 45), Color.White);
        TextRenderer.DrawText(g, "規模", font10, new Point(270, 51), secondary);
        if (isHuge)
        {
            var clr = Color.FromArgb(255, 90, 90);
            TextRenderer.DrawText(g, "M8超の巨大地震", font14, new Point(303, 44), clr);
        }
        else
        {
            TextRenderer.DrawText(g, "M", font11, new Point(303, 50), Color.White);
            TextRenderer.DrawText(g, isHuge ? "M8超 巨大地震" : info.Earthquake.Magnitude ?? "---", font16, new Point(317, 42),  Color.White);
        }
        var tsunamiMessage = "";
        var tsunamiColor = Color.Yellow;
        var tsunamiFont = font16;

        if (info.ForecastCommentCodes.Contains("0215"))
        {
            tsunamiColor = Color.White;
            tsunamiMessage = "津波の心配なし";
        }
        else if (info.ForecastCommentCodes.Contains("0212"))
        {
            tsunamiMessage = "若干の海面変動 被害の心配なし";
        }
        else if (info.ForecastCommentCodes.Contains("0211"))
        {
            tsunamiFont = font13;
            tsunamiMessage = "津波に関する情報（津波警報等）を発表中";
        }

        TextRenderer.DrawText(g, tsunamiMessage, tsunamiFont, new Rectangle(0, 68, 360, 32),
            tsunamiColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Right);

        if (info.Type == EarthquakeInformationType.HypocenterAndSeismicIntensityInformation)
        {
            var color = Form1.Colors[info.MaxIntensity.EnumOrder];
            var textColor = info.MaxIntensity.EnumOrder is -1 or >= 3 and <= 5 ? Color.Black : Color.White;
            g.FillRectangle(new SolidBrush(color), 0, 0, 147, 55);
            var s = info.MaxIntensity.ShortString[0].ToString();
            g.DrawString("最大震度", font14, new SolidBrush(textColor), 0, 29);
            if (info.MaxIntensity.Equals(Intensity.Unknown))
            {
                g.DrawString("不明", font20, Brushes.Black, new Point(80, 67));
            }
            else if (info.MaxIntensity.ShortString.Length == 2)
            {
                g.DrawString(s, robotoI, new SolidBrush(textColor),
                    new Rectangle(79, 6, 35, 53), sfCenterCenter);
                var pmChar = info.MaxIntensity.LongString.Last().ToString();
                g.DrawString(pmChar, font20B, new SolidBrush(textColor),
                    new Rectangle(104, 6, 45, 53), sfCenterFar);
            }
            else
            {
                g.DrawString(s, robotoI, new SolidBrush(textColor),
                    new Rectangle(74, 6, 80, 53), sfCenterCenter);
            }
        }
        else
        {
            g.DrawRectangle(Pens.White, new Rectangle(8, 5, 131, 45));
            TextRenderer.DrawText(g, "震源情報", font20, new Rectangle(2, 0, 145, 55),
                Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }

        return img;
    }
}