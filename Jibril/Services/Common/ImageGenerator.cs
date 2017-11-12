﻿using System;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using Discord.WebSocket;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace Jibril.Services.Common
{
    public static class ImageGenerator
    {
        public static async Task<string> AvatarGenerator(SocketUser user, string randomString)
        {
            var httpClient = new HttpClient();
            HttpResponseMessage response = null;

            var rand = new Random();
            var avaMutate = rand.Next(0, 60);

            var aviPath = $"Data/Images/Welcome/Cache/Avatar/{randomString}.png";
            try
            {
                response = await httpClient.GetAsync(user.GetAvatarUrl());
            }
            catch
            {
                response = await httpClient.GetAsync(
                    "https://discordapp.com/assets/1cbd08c76f8af6dddce02c5138971129.png");
            }
            var inputStream = await response.Content.ReadAsStreamAsync();
            using (var img = Image.Load(inputStream))
            {
                img.Mutate(x => x.ConvertToAvatar(new Size(60, 60), avaMutate));
                img.Save(aviPath);
                return aviPath;
            }
        }

        private static IImageProcessingContext<Rgba32> ConvertToAvatar(
            this IImageProcessingContext<Rgba32> processingContext, Size size, float cornerRadius)
        {
            return processingContext.Resize(new ResizeOptions
            {
                Size = size,
                Mode = ResizeMode.Crop
            }).Apply(i => ApplyRoundedCorners(i, cornerRadius));
        }

        private static Image<Rgba32> CloneAndConvertToAvatarWithoutApply(this Image<Rgba32> image, Size size,
            float cornerRadius)
        {
            var result = image.Clone(
                ctx => ctx.Resize(
                    new ResizeOptions
                    {
                        Size = size,
                        Mode = ResizeMode.Crop
                    }));

            ApplyRoundedCorners(result, cornerRadius);
            return result;
        }

        public static void ApplyRoundedCorners(Image<Rgba32> img, float cornerRadius)
        {
            var corners = BuildCorners(img.Width, img.Height, cornerRadius);


            img.Mutate(x => x.Fill(Rgba32.Transparent, corners, new GraphicsOptions(true)
            {
                BlenderMode = PixelBlenderMode.Src
            }));
        }

        public static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            var rect = new RectangularePolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

            var cornerToptLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            var center = new Vector2(imageWidth / 2F, imageHeight / 2F);

            var rightPos = imageWidth - cornerToptLeft.Bounds.Width + 1;
            var bottomPos = imageHeight - cornerToptLeft.Bounds.Height + 1;

            var cornerTopRight = cornerToptLeft.RotateDegree(90).Translate(rightPos, 0);
            var cornerBottomLeft = cornerToptLeft.RotateDegree(-90).Translate(0, bottomPos);
            var cornerBottomRight = cornerToptLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerToptLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }
    }
}