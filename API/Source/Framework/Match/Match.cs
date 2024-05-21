﻿
using SRkMatchSimAPI.Framework.DTO;

namespace SRkMatchSimAPI.Framework;

public enum MatchBreakpoint { Goal, Shot, ThroughBall }
public enum MatchState { Pre, Half1, Interval, Half2, Post }

public class Match
{
    public float Minutes { get; private set; }
    public float Ball { get; set; } = 0.5f;

    public MatchState State { get; private set; }

    //public MatchDebugger Debugger;
    public List<MatchEvent> Events { get; } = new List<MatchEvent>();

    public MatchTeam Home { get; }
    public MatchTeam Away { get; }

    public MatchTeam PT { get; private set; }
    public MatchTeam OT { get; private set; }

    public MatchTeam[] Teams;

    public string Id {  get; set; }

    public event Action<MatchBreakpoint> Breakpoint;

    public void EmitSignal(MatchBreakpoint breakpoint)
    {
        Breakpoint?.Invoke(breakpoint);
    }

    public List<Player> Players = new List<Player>();

    public static Match FromDTO(MatchDTO dto)
    {

        var b = new Club("Black", "#000");
        var w = new Club("White", "#fff");

        b.ProSquad = new Squad(b);
        w.ProSquad = new Squad(w);

        /*
        Console.WriteLine($"---------match({dto.Id})");

        Console.WriteLine($"--home");
        foreach (var p in dto.Players.Where(p => p.Side == PlayerDTOSide.Home))
        {
            Console.WriteLine($"{p.User.Email} (id: {p.User.Id}, pos: {p.Pos}), fin: {p.Attrib.Finishing}, mar: {p.Attrib.Marking}");
        }
        Console.WriteLine($"--away");
        foreach (var p in dto.Players.Where(p => p.Side == PlayerDTOSide.Away))
        {
            Console.WriteLine($"{p.User.Email} (id: {p.User.Id}, pos: {p.Pos}), fin: {p.Attrib.Finishing}, mar: {p.Attrib.Marking}");
        }

        Console.WriteLine($"--------------------------------");
        */

        var homePlayers = dto.Players.Where(p => p.Side == PlayerDTOSide.HOME).OrderBy(p => p.Pos);
        var awayPlayers = dto.Players.Where(p => p.Side == PlayerDTOSide.AWAY).OrderBy(p => p.Pos);

        if (homePlayers.Count() != 11 || awayPlayers.Count() != 11)
        {
            throw new Exception($"Teams must have 11 players each. They have {homePlayers.Count()} and {awayPlayers.Count()}.");
        }

        foreach (var hp in dto.Players)
        {
            var player = new Player(hp.User.Id, hp.User.Email, hp.Pos, hp.Attrib, hp.MMR);
            (hp.Side == PlayerDTOSide.HOME ? b : w).ProSquad.AddPlayer(player);
        }

        

        var m = new Match(dto.Id, b.ProSquad, w.ProSquad);

        


        return m;
    }

    public Match(string id, Squad home, Squad away)
    {
        Id = id;
        Home = new MatchTeam(this, home, true);
        Away = new MatchTeam(this, away, false);

        Teams = new MatchTeam[2] { Home, Away };

        Home.Opp = Away;
        Away.Opp = Home;

        GiveBallTo(Home);
        //this.Debugger = debugger;
    }

    public void GiveBallTo(MatchTeam t)
    {
        //Console.WriteLine("give ball to ", t.IsHome ? "Home" : "Away");
        PT = t;
        OT = t.Opp;
    }

    public void GiveBallToOpp()
    {
        GiveBallTo(OT);
    }

    public void AdvanceTillEnd()
    {
        //var d = Debugger;
        //Debugger = null;
        while (State != MatchState.Post)
        {
            Advance(false);
        }


        //Debugger = d;

        //OnMatchAdvanced();
    }

    public void OnAdvance(bool emit = true)
    {
        if (State == MatchState.Post)
            return;

        if (Minutes >= 90f)
        {
            End();
            return;
        }

        PT.Play();

        Minutes += 0.15f;

        Home.CalcAttrib();
        Away.CalcAttrib();

        if (emit)
            HasAdvanced?.Invoke();
    }

    public void Advance(bool emit = true)
    {
        //Console.WriteLine("---adv");
        if (emit)
            PreAdvance?.Invoke();

        OnAdvance(emit);
    }

    public event Action HasEnded, EndOf1stHalf, EndOf2ndHalf, HasAdvanced, PreAdvance;

    public void End()
    {
        State = MatchState.Post;

        // foreach(var ev in Events)
        // {
        //     Console.WriteLine(ev);
        // }
        
        Console.WriteLine($"<end of {Id}> home {Home.Slots.Count} x away {Away.Slots.Count}");

        HasEnded?.Invoke();
    }
}
