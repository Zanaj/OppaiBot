/*
 
        using (Graphics g = Graphics.FromImage(profile))
        {
            
            string name = ctx.Member.DisplayName + "#" + ctx.Member.Discriminator;
            Font font = new Font("Arial", 9);
            brush.Color = Color.Black;
            g.DrawImage(middleground, new PointF(0, 0));
            g.DrawString(name, font, brush, new PointF(245, 110));

            using (Font f = new Font("Ariel", 7, FontStyle.Italic, GraphicsUnit.Point))
            {

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                g.DrawString(expStr, f, brush, rect, sf);
            }

            int a = 170;
            brush.Color = Color.FromArgb(a, 254, 134, 134);
            g.FillRectangle(brush, levelRect);

            brush.Color = Color.Black;
            
            using (Font f = new Font(family, 11, FontStyle.Bold, GraphicsUnit.Point))
            {
                StringFormat sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;

                g.DrawString(currentLevel.ToString(), f, brush, levelRect, sf);
            }
        }

 */

using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using OppaiBot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public static class ImageProcesser
{
    public static string BASE_PATH = "";
    public const int AVATAR_HEIGHT = 85;
    public const int AVATAR_OFFSET = 10;
    public static Rectangle expBarRect;
    public static Rectangle levelRect;
    public static PointF usernamePoint;
    public static float levelFontSize = 16;
    public static float usernameFontSize = 11;
    public static float expFontSize = 7;
    public static string levelFontName = "Noteworthy";
    public static string usernameFontName = "Noteworthy";
    public static string expFontName = "Noteworthy";
    public static Font levelFont { get { return new Font(levelFontName,levelFontSize, FontStyle.Bold); } }
    public static Font usernameFont { get { return new Font(usernameFontName, usernameFontSize); } }
    public static Font expFont { get { return new Font(expFontName, expFontSize); } }

    public static SolidBrush brush;

    public static void Initialize()
    {
        BASE_PATH = System.Environment.CurrentDirectory + "\\Data\\";

        expBarRect = new Rectangle(235, 165, 755, 65);
        usernamePoint = new PointF(250, 95);
        SolidBrush brush = new SolidBrush(Color.Gray);
        levelRect = new Rectangle(850, 3, 200, 85);
    }

    /// <summary>
    /// Resize the image to the specified width and height.
    /// </summary>
    /// <param name="image">The image to resize.</param>
    /// <param name="width">The width to resize to.</param>
    /// <param name="height">The height to resize to.</param>
    /// <returns>The resized image.</returns>
    public static Bitmap ResizeImage(Image image, int width, int height)
    {
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height);

        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using (var graphics = Graphics.FromImage(destImage))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var wrapMode = new ImageAttributes())
            {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
        }

        return destImage;
    }

    public async static Task<DiscordMessage> GetLevelupProfile(CommandContext ctx)
    {
        DiscordMessage holdonMsg = await ctx.RespondAsync("Processing picture...");

        Bitmap template = new Bitmap(BASE_PATH+"template.png");

        Bitmap profile = DrawBackground(ctx, template);
        profile = DrawForeground(ctx, template);

        string filePath = BASE_PATH + "temp.png";
        profile.Save(filePath, ImageFormat.Png);

        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            await holdonMsg.DeleteAsync();
            return await ctx.RespondWithFileAsync(fs);
        }
    }

    public static Bitmap GetAvatar(CommandContext ctx)
    {
        WebClient wc = new WebClient();
        byte[] bytes = wc.DownloadData(ctx.Member.AvatarUrl);
        MemoryStream ms = new MemoryStream(bytes);
        Image avatar = Image.FromStream(ms);

        Bitmap bmAvatar = ResizeImage(avatar, AVATAR_HEIGHT, AVATAR_HEIGHT);

        return bmAvatar;
    }
    public static Bitmap DrawForeground(CommandContext ctx, Bitmap template)
    {
        using (Graphics g = Graphics.FromImage(template))
        {
            g.DrawImage(template, 0, 0);
            g.DrawImage(Image.FromFile(BASE_PATH + "template.png"), 0, 0);

            string name = ctx.Member.DisplayName + "#" + ctx.Member.Discriminator;
            SolidBrush b = new SolidBrush(Color.Black);
            g.DrawString(name, usernameFont, b, usernamePoint);

            b.Color = Color.White;
            User user = Bot.GetUserByID(ctx.Member);
            g.DrawString((user.lastRecordedLevel*10).ToString(), levelFont, b, levelRect);

            DrawExpString(ctx, g);
        }

        return template;
    }

    public static void DrawExpString(CommandContext ctx, Graphics g)
    {
        SolidBrush b = new SolidBrush(Color.Black);

        User user = Bot.GetUserByID(ctx.Member);
        float currentExp = user.exp;
        float nextLvlExp = ConfigHandler.levelConfig.GetExpForLevel(user.level + 1);

        currentExp = (float)Math.Ceiling(currentExp);
        nextLvlExp = (float)Math.Ceiling(nextLvlExp);

        string expStr = currentExp + "/" + nextLvlExp;

        StringFormat format = new StringFormat();
        format.LineAlignment = StringAlignment.Center;
        format.Alignment = StringAlignment.Center;

        Font f = new Font(expFont.FontFamily, expFontSize);
        g.DrawString(expStr, expFont, b, expBarRect, format);
    }
    public static Bitmap DrawBackground(CommandContext ctx, Bitmap background)
    {
        using (Graphics g = Graphics.FromImage(background))
        {
            SolidBrush brush = new SolidBrush(Color.Gray);
            g.DrawImage(GetAvatar(ctx), new PointF(AVATAR_OFFSET - 2, AVATAR_OFFSET));

            g.FillRectangle(brush, expBarRect);
            DrawExpBar(ctx,g);
        }

        return background;
    }
    public static void DrawExpBar(CommandContext ctx, Graphics g)
    {
        User user = Bot.GetUserByID(ctx.Member);

        float currenctExp = user.exp;;
        float nextExp = ConfigHandler.levelConfig.GetExpForLevel(user.level + 1);

        float procentage = currenctExp / nextExp;

        SolidBrush sbrush = new SolidBrush(Color.LightBlue);
        int newWith = (int)Math.Floor(procentage * expBarRect.Width);
        g.FillRectangle(sbrush, expBarRect.X, expBarRect.Y, newWith, expBarRect.Height);
    }
}
