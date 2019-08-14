using System.IO;
using RGB.NET.Core.Layout;

namespace LayoutEditor.UI.Models
{
    public class LayoutEditModel
    {
        public string BasePath { get; set; }
        public DeviceLayout DeviceLayout { get; set; }
        public string DeviceLayoutSource { get; set; }

        public string DeviceImage => Path.Combine(BasePath, DeviceLayout.ImageBasePath, DeviceLayout.DeviceImage);

        public string GetAbsoluteImageDirectory(string image)
        {
            if (BasePath != null && DeviceLayout.ImageBasePath != null && image != null)
                return Path.Combine(BasePath, DeviceLayout.ImageBasePath, image);

            return null;
        }
    }
}