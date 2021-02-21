using System;
using System.Collections.Generic;
using System.Linq;
using RGB.NET.Core;
using RGB.NET.Layout;

namespace LayoutEditor.UI.Models
{
    public class LayoutEditModel
    {
        private readonly List<string> _ledIds;

        public LayoutEditModel()
        {
            _ledIds = Enum.GetValues(typeof(LedId)).Cast<LedId>().Select(v => v.ToString()).ToList();
        }

        public string FilePath { get; set; }
        public DeviceLayout DeviceLayout { get; set; }
        public KeyboardLayoutType PhysicalLayout { get; set; }

        public IEnumerable<string> GetAvailableLedIds(string ignore = null)
        {
            var leds = DeviceLayout.Leds;
            if (ignore != null)
                leds = leds.Where(l => !string.Equals(l.Id, ignore)).ToList();

            return _ledIds.Except(leds.Select(l => l.Id));
        }
    }
}