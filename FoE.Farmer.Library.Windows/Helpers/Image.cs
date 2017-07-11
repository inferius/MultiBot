using System;
using System.Drawing;
using System.IO;

namespace FoE.Farmer.Library.Windows.Helpers
{
    public static class ImageHelpers
    {
        public static string ToBase64(string path)
        {
            using (var image = Image.FromFile(path))
            {
                using (var m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    var imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    var base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
        }
    }
}
