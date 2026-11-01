using System.Globalization;
using UnityEngine;

namespace Ironclads
{
    public partial class UmpirePanel
    {
        string minnesotaEast="-650",minnesotaNorth="-450";
        void UnionSupport()
        {
            if(!scene.scenario.Hampton){Text("The additional Union ships belong to the Hampton Roads scenario.");return;}
            Heading("USS MINNESOTA · AGROUND");
            var ship=scene.scenario.minnesota;var effects=GetComponent<BatteryEffects>();
            Text("Three-masted steam frigate, single funnel. Grounded on the shoals; its position stays fixed as turns advance. Battery availability is set by the umpire.");
            if(Button("Inspect Minnesota"))scene.support.Inspect(2);
            Heading("MINNESOTA'S BATTERIES");
            GUI.enabled=ManageTable;
            scene.scenario.minnesotaPortGuns=Mathf.RoundToInt(Slider("Usable PORT guns",scene.scenario.minnesotaPortGuns,0,21));
            scene.scenario.minnesotaStarboardGuns=Mathf.RoundToInt(Slider("Usable STARBOARD guns",scene.scenario.minnesotaStarboardGuns,0,21));
            bool bow=GUILayout.Toggle(scene.scenario.minnesotaBowGuns!=0,"Ten-inch bow pivot available",toggle);scene.scenario.minnesotaBowGuns=bow?1:0;
            GUI.enabled=!scene.globeMode&&!effects.firing[2]&&(scene.network==null||scene.network.CanControl(2));
            if(Button("Fire PORT · "+effects.Battery(2,-1).Length+" available guns"))effects.Fire(2,-1);
            if(Button("Fire STARBOARD · "+effects.Battery(2,1).Length+" available guns"))effects.Fire(2,1);
            if(Button("Fire BOW · ten-inch pivot"))effects.Fire(2,0);
            if(Button("All available guns · visual display"))effects.Fire(2,2);
            GUI.enabled=true;
            if(scene.network!=null&&scene.network.Active)Text("The Monitor player commands Minnesota's batteries. The host sets damage availability and the grounded position.");
            Text("Nominal outfit from the supplied card: 28 nine-inch, 14 eight-inch, one ten-inch. The default leaves 11 of 21 port guns available, approximating the Iron & Fire scenario's 50% port-battery loss. Select any availability you adjudicate; disabled guns remain visible on the model.");
            Vector3 delta=scene.scenario.ships[1].Position-ship.Position;
            Text($"Minnesota → Virginia: {delta.magnitude/.9144f:0} yd / {delta.magnitude/RuleChart.HexMeters:0.00} tabletop inches\nBearing {GunneryTools.Bearing(delta):0.0}° true; from bow {Mathf.DeltaAngle(ship.heading,GunneryTools.Bearing(delta)):+0.0;-0.0;0}°");
            if(Button("Measure from Minnesota / custom gun guide")){var guns=GetComponent<GunneryTools>();guns.minnesotaOrigin=true;guns.preset=3;guns.visible=true;ShowTab(4);}
            Heading("GROUNDED POSITION · UMPIRE SETUP");GUI.enabled=ManageTable;
            GUILayout.BeginHorizontal();minnesotaEast=GUILayout.TextField(minnesotaEast,field);minnesotaNorth=GUILayout.TextField(minnesotaNorth,field);GUILayout.EndHorizontal();
            if(Button("Set east / north meters")){
                if(float.TryParse(minnesotaEast,NumberStyles.Float,CultureInfo.InvariantCulture,out float e)&&float.TryParse(minnesotaNorth,NumberStyles.Float,CultureInfo.InvariantCulture,out float n)&&CourseMovement.Inside(new Vector3(e,0,n))){chart.Remember();ship.east=e;ship.north=n;scene.UpdateWorld();}else scene.status="Use finite east/north meters within ±20,000.";
            }
            ship.heading=Slider("Grounded heading",ship.heading,0,359.9f,"°");
            ship.boilersLit=GUILayout.Toggle(ship.boilersLit,"Minnesota boilers lit",toggle);ship.engineLoad=Slider("Minnesota visual engine effort",ship.engineLoad,0,1);
            GUI.enabled=true;
            Heading("WRECKS OFF NEWPORT NEWS");
            if(Button("Inspect USS Congress wreck"))scene.support.Inspect(3);
            Text("Congress burned and exploded overnight: charred, ruptured hull, fallen beams and short mast remnants. No usable battery.");
            if(Button("Inspect USS Cumberland wreck"))scene.support.Inspect(4);
            Text("Cumberland sank on March 8: submerged hull with exposed leaning masts and surviving rigging. No usable battery.");
            Text("Ship and wreck positions, damage, rigging and small fittings are reconstructions. Wrecks are scenery and manual obstacles; the app does not enforce collision, grounding or line of sight.");
            SaveLoad();
        }
    }
}
