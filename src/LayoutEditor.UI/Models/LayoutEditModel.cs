using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RGB.NET.Core;
using RGB.NET.Core.Layout;

namespace LayoutEditor.UI.Models
{
    public class LayoutEditModel
    {
        private readonly List<string> _ledIds;

        public string BasePath { get; set; }
        public DeviceLayout DeviceLayout { get; set; }
        public string DeviceLayoutSource { get; set; }

        public string DeviceImage => Path.Combine(BasePath, DeviceLayout.ImageBasePath, DeviceLayout.DeviceImage);

        public LayoutEditModel()
        {
            _ledIds = Enum.GetValues(typeof(LedId)).Cast<LedId>().Select(v => v.ToString()).ToList();
        }

        public string GetAbsoluteImageDirectory(string image)
        {
            if (BasePath != null && DeviceLayout.ImageBasePath != null && image != null)
                return Path.Combine(BasePath, DeviceLayout.ImageBasePath, image);

            return null;
        }

        public IEnumerable<string> GetAvailableLedIds(string ignore = null)
        {
            var leds = DeviceLayout.Leds;
            if (ignore != null)
                leds = leds.Where(l => !string.Equals(l.Id, ignore)).ToList();

            return _ledIds.Except(leds.Select(l => l.Id));
        }
    }
}