using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ironclads
{
    // Numerical game mechanics transcribed from the published sheets photographed at BGG:
    // movement 518058, BGN 518054, results 518048, auxiliaries 518070.
    // Original chart artwork is not bundled in the public source or public player.
    public static class CloseActionCharts
    {
        // For each WS, C/B/R rows contain 4-section FS/MS/PS, 3-section FS/MS,
        // 2-section FS and 1-section FS. All hulls in scenario 6 are L-f or L-s.
        static readonly int[,,,] Movement={
            {
                {{1,2,4,0,1,0,0},{3,5,7,2,3,1,0},{2,4,6,1,2,1,0}},
                {{2,3,5,1,2,0,0},{4,6,8,3,5,2,1},{3,5,7,2,4,1,1}},
                {{3,4,6,2,3,1,0},{5,7,9,4,6,3,2},{4,6,8,3,5,2,1}},
                {{3,4,6,2,3,1,0},{5,7,9,4,6,3,2},{5,7,9,4,6,3,2}}
            },
            {
                {{1,2,3,0,1,0,0},{3,4,6,2,3,1,0},{2,3,5,1,2,1,0}},
                {{2,3,4,1,2,0,0},{4,5,7,3,4,2,1},{3,4,6,2,3,1,1}},
                {{3,4,5,2,3,1,0},{5,6,8,4,5,3,2},{4,5,7,3,4,2,1}},
                {{3,4,5,2,3,1,0},{5,6,8,4,6,3,2},{5,6,8,4,6,3,2}}
            }
        };
        public static int Allowance(string type,int wind,int sections,int sail,int facing)
        {
            if(wind==0||sections==0||sail==0||facing==0)return 0;
            int column=sections==4?sail-1:sections==3?3+Math.Min(2,sail)-1:sections==2?5:6;
            return Movement[type=="L-s"?1:0,wind-1,facing-1,column];
        }
        public static int TurnSpacing(char maneuver,bool same,int mp)
        {
            if(mp<=2)return 1;
            if(!same)return mp<=5?1:mp<=7?2:3;
            if(maneuver=='A')return mp<=5?2:3;
            return mp<=3?1:mp<=7?2:3;
        }
        static readonly int[] Bands={1,2,3,4,5,6,7,8,9,10,11,12,14,16,18,20,22,24,27,30,int.MaxValue};
        static readonly int[,] Bgn={
            {3,2,2,1,1,0,0,-99,-99,-99,-99,-99,-99,-99,-99},
            {8,7,6,5,4,3,2,1,0,-99,-99,-99,-99,-99,-99},
            {11,10,9,8,7,5,4,3,2,0,-99,-99,-99,-99,-99},
            {13,12,11,10,9,7,5,4,3,2,0,-99,-99,-99,-99},
            {15,14,13,12,11,9,7,5,4,3,2,0,-99,-99,-99},
            {17,16,15,14,13,11,9,7,5,4,3,2,0,-99,-99},
            {18,17,16,15,14,12,10,8,6,5,4,3,2,0,-99},
            {19,18,17,16,15,13,11,9,7,6,5,4,3,1,0},
            {20,19,18,17,16,14,12,10,8,7,6,5,4,2,0},
            {21,20,19,18,17,15,13,11,9,8,7,6,5,3,1},
            {22,21,20,19,18,16,14,12,10,9,8,7,5,4,2},
            {23,22,21,20,19,17,15,13,11,10,9,7,6,5,3},
            {24,23,22,21,20,18,16,14,12,11,9,8,7,6,4},
            {25,24,23,22,21,19,17,15,13,12,10,9,8,7,5},
            {26,25,24,23,22,20,18,16,14,13,11,10,9,7,6},
            {27,26,25,24,23,21,19,17,15,14,12,11,10,8,7},
            {28,27,26,25,24,22,20,18,16,15,13,12,11,9,8},
            {29,28,27,26,25,23,21,19,17,16,14,13,12,10,8},
            {30,29,28,27,26,24,22,20,18,17,15,14,13,11,9},
            {31,30,29,28,27,25,23,21,19,18,16,15,14,12,10},
            {32,31,30,29,28,26,24,22,20,19,17,16,15,13,11}
        };
        public static int BasicGunnery(CloseActionSpec spec,int range)
        {
            if(range<1||range>15)return -99;
            float cr=range<=3?2:range==4?1.5f:range==5?1:range==6?.5f:0;
            int factor=Mathf.CeilToInt(spec.longGuns+spec.carronades*cr),row=Array.FindIndex(Bands,n=>factor<=n);
            int value=Bgn[row,range-1];return value<0?value:value+(range>=11&&spec.side==1?1:0);
        }
        // FS result | added MS | added PS; / denotes a fresh independent d6 choice.
        static readonly string[] Low={
            ".|.|.",".|.|.","h/.|.|.",".|.|.","r/.|.|.",".|.|.",
            "h/.|.|r/.","c/.|.|.","h/.|r/.|.","h/.|.|.","r/.|.|.","r/.|.|.",
            "h/.|.|r/.","h/c|.|.","h|r/.|.","h/c|.|.","r|.|.","r/h|.|.",
            "h/c|.|r","r/h-c/.|.|.","h-r/.|r/.|.","h-c/.|.|.","r-h|.|.","r-c|r/.|.",
            "r-h|.|r","h-c-r/.|.|.","r-h-c|r|.","2h-c|.|r","2r-2h|.|.","2r-h-c|r|.",
            "r-2h-c|.|r","2r-2h-c|.|r","r-3h-c|r|.","r-2h-2c|r|.","3r-3h-c|.|r","3r-2h-2c|r|.",
            "2r-4h-c|r|r","3r-3h-2c|.|2r","3r-4h-2c|r|.","3r-4h-3c|r|2r","4r-5h-2c|r|2r","4r-5h-3c|r|r",
            "4r-6h-3c|2r|r","5r-6h-3c|r|2r","5r-7h-3c|r|2r","6r-7h-4c|2r|3r","6r-8h-5c|2r|3r","7r-8h-4c|2r|2r",
            "7r-10h-5c|2r|3r","8r-11h-5c|2r|3r","8r-11h-6c|2r|3r","9r-12h-6c|3r|4r"
        };
        static readonly string[] High={
            ".|.|.",".|.|.","r/.|.|r/.",".|.|.","r/.|.|.",".|r/.|.",
            "h/.|.|.",".|.|.","c/.|.|r/.","r/.|.|.","r/.|.|.","r/.|r/.|.",
            "h/.|.|.","r/.|.|.","c/.|.|r","r/h|.|.","r|.|.","r/c|r/.|.",
            "r/h|r/.|.","r|.|.","r/c|.|r","r-h/.|.|r","r-h/.|.|.","r-c/.|r/.|.",
            "r-h/.|r|r","2r|.|.","r-h/c|r|r","2r-h/.|.|r","2r-h|.|r","2r-c|r|r",
            "2r-h|.|r","3r-c|.|r","3r-h|r|r","3r-h|r|2r","3r-h-c|r|r","4r-c|r|2r",
            "4r-2h|r|r","4r-h-c|2r|r","5r-h-c|r|2r","5r-h-c|2r|2r","6r-2h|r|3r","6r-h-2c|2r|3r",
            "7r-2h-c|2r|2r","7r-2h-2c|2r|2r","8r-2h-2c|2r|3r","9r-2h-c|2r|4r","9r-3h-2c|3r|4r","10r-3h-2c|3r|4r",
            "11r-3h-2c|3r|4r","12r-3h-2c|3r|4r","13r-4h-2c|3r|5r","14r-3h-3c|4r|5r"
        };
        public static int CriticalThreshold(int fgn)=>fgn<=6?0:fgn<=14?1:fgn<=22?2:fgn<=30?3:fgn<=38?4:fgn<=46?5:6;
        public static string RawResult(int fgn,bool high,int sail)
        {
            if(fgn<1)return ".";string[] row=(high?High:Low)[Math.Min(52,fgn)-1].Split('|');
            return string.Join(" + ",row.Take(Math.Max(1,sail)));
        }
        public static CloseActionHit RollDamage(Scenario scene,CloseActionFire fire,Func<int> d6,out string detail)
        {
            int range=CloseActionRules.Range(scene.ships[fire.ship],scene.ships[fire.target]);
            int fgn=fire.bgn+CloseActionRules.GunneryModifier(scene,fire)+fire.die;
            var target=scene.closeAction.logs[fire.target];var hit=new CloseActionHit{target=fire.target};
            detail=RawResult(fgn,fire.high,target.sail);
            if(fgn<1)return hit;
            string[] row=(fire.high?High:Low)[Math.Min(52,fgn)-1].Split('|');int crew=0;
            for(int i=0;i<Math.Max(1,target.sail);i++)foreach(var part in row[i].Split('-')){
                string term=part;
                if(part.Contains('/')){var options=part.Split('/');term=options[d6()<=3?0:1];}
                if(term=="."||term.Length==0)continue;
                char kind=term[term.Length-1];int count=term.Length==1?1:int.Parse(term.Substring(0,term.Length-1));
                if(kind=='r')hit.rig+=count;else if(kind=='h')hit.hull+=count;else if(kind=='c')crew+=count;
            }
            if(target.RigSections==0)hit.rig=0;else if(target.sail==0)hit.rig=hit.rig/2+(hit.rig%2!=0&&d6()<=3?1:0);
            if(fire.dismantling)hit.hull=0;
            if(CloseActionRules.Rake(scene,fire.ship,fire.target)==2)crew*=2;
            for(int i=0;i<crew;i++){
                if(target.crew.Take(4).Sum()-hit.sailors<=0)hit.marines++;
                else if(target.crew[4]-hit.marines<=0)hit.sailors++;
                else if(d6()<=(fire.high||fire.dismantling?3:5))hit.sailors++;else hit.marines++;
            }
            int check=CriticalThreshold(fgn);
            if(fire.die==6){int roll=d6();if(roll+CloseActionScenario.Spec(scene,fire.target).critical<=check){int critical=d6()+d6();hit.note=(fire.high?"Rigging":"Hull")+" critical roll "+critical+"; umpire resolves yellow sheet before maintenance.";}}
            hit.note="FGN "+fgn+": "+hit.rig+"R "+hit.hull+"H "+hit.sailors+"S "+hit.marines+"M. "+hit.note;return hit;
        }
        public static int TackTarget(char maneuver,int wind)=>new int[,]{{2,4,6,5},{3,5,7,6}}[maneuver=='A'?0:1,wind-1];
        public static int PoorTurnTarget(char maneuver,int quality)=>new int[,]{{6,7},{5,6},{5,5},{4,5},{4,4},{4,4},{4,4}}[quality,maneuver=='A'?0:1];
        public static int DriftTarget(int sail,int wind,bool dismasted)=>dismasted?new[]{99,10,9,8,6}[wind]:new int[,]{{99,8,7,6,4},{99,7,6,5,3},{99,6,5,4,2},{99,4,3,2,0}}[sail,wind];
    }
}
