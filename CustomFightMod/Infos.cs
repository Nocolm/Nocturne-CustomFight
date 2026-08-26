using System;
using System.Collections.Generic;

namespace CustomFightMod
{
    public class Infos
    {
        public string songname;
        public string icon;
        public int icon_size_x;
        public int icon_size_y;
        public float pixel_per_unit;
        public string beatmap;
        public string animation_placeholder;
        public int animation_size_x;
        public int animation_size_y;
        public float animation_pixel_per_unit;
        public Dictionary<string, string> animation;
        public string soundbank;
        public uint playevent_id;
        public uint resumeevent_id;
        public uint pauseevent_id;
        public uint stopevent_id;
        public bool five_columns;
    }
}
