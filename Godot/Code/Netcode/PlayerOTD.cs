﻿using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futzim
{
    public struct PlayerOTD
    {
        public string UserId { get; set; }
        public MatchStats Stats { get; set; }
        public PlayerDTOSide Side { get; set; }
        public int MMRIncr { get; set; }

        public PlayerOTD(MatchSlot p, int mmrIncr)
        {
            UserId = p.Source.Player.UserId;
            Stats = p.Stats;
            Side = p.Team.IsHome ? PlayerDTOSide.HOME: PlayerDTOSide.AWAY;
            MMRIncr = mmrIncr;
        }
    }
}
