using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Ironclads {
 public static class CampaignOrderOfBattle {
  [Serializable] class Packet { public CampaignVessel[] records; }
  public static List<CampaignVessel> Markers() {
   var source=Resources.Load<TextAsset>("Campaign/Hampton1864OOB");if(source==null)throw new InvalidOperationException("KS order of battle missing");
   var rows=JsonUtility.FromJson<Packet>(source.text).records.ToList();int csa=0,usa=0;
   foreach(var v in rows){int i=v.side==CampaignSide.Confederate?csa++:usa++;var p=CampaignGeography.Staging(i,v.side);v.x=p.x;v.z=p.z;v.station=-1;v.readiness="Not supplied";v.supplies="Not supplied";v.condition="Not supplied";v.notes="";v.task="";}
   return rows;
  }
  public static List<CampaignVessel> Legacy() {
   var rows=new List<CampaignVessel>();string[] css={"Virginia II","Richmond","Fredericksburg","Hampton","Nansemond","Drewry","Beaufort","Torpedo","Scorpion"};string[] uss={"Onondaga","Hunchback","Eutaw","Daylight","Miami","Mendota","Commodore Perry","Western World","Henry Brinker","Commodore Morris"};
   for(int i=0;i<css.Length;i++)rows.Add(new CampaignVessel{id="CSS-"+i,name="CSS "+css[i],side=CampaignSide.Confederate,type=i<3?"Ironclad":i<6?"Gunboat":"Supporting vessel"});
   for(int i=0;i<uss.Length;i++)rows.Add(new CampaignVessel{id="USS-"+i,name="USS "+uss[i],side=CampaignSide.Union,type=i==0?"Double-turret monitor":"Supporting vessel"});return rows;
  }
  public static string Briefing(CampaignSide side) {
   string mission=side==CampaignSide.Union?"Maintain control of the James River, protect Federal shipping and installations, and prevent Confederate penetration.":"Break through Federal defenses and get a viable Confederate naval force into the open Atlantic, preserving strength for further operations.";
   var rows=Markers().Where(v=>v.side==side);return mission+"\nScenario operational window: 22 Dec 1864 1800 to 23 Dec 1864 1800. Planning precedes T0; operational execution starts at T0.\n\n"+string.Join("\n",rows.Select(v=>v.name+" / "+v.type+" / "+v.guideStation+" / "+v.guideArmament+(string.IsNullOrEmpty(v.guideRestrictions)?"":" / "+v.guideRestrictions)))+"\n\nHistorical officers and wider command responsibilities follow the supplied OOB v1.0. Army and distant support require coordination. Group resources have no preset quantity. Readiness, supplies and precise coordinates require umpire confirmation.";
  }
 }
}
