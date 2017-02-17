﻿using System;

namespace YorkDevelopers.Meetup.Models
{
    public class Event
    {
        public string name { get; set; }
        public string description { get; set; }
        public string link { get; set; }

        public Group group { get; set; }
        public Venue venue { get; set; }

        public Fee fee { get; set; }

        public string time { get; set; }

        public string duration { get; set; }
    }
}
