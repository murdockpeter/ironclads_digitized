using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Ironclads.Editor
{
    public static class PantelleriaReview
    {
        static int count;
        static void Check(bool ok,string text){if(!ok)throw new Exception("Pantelleria verification: "+text);count++;}
        public static void Run()
        {
            count=0;Directory.CreateDirectory("Artifacts/Pantelleria");var s=PantelleriaScenario.Create();
            Check(s.Pantelleria&&s.DoB&&s.ShipSlots==42&&s.ships.Length==42,"42-ship DoB module");Check(PantelleriaScenario.Ships.Select(x=>x.name).Distinct().Count()==42,"unique ship names");
            int[] expected={13,8,13,8};for(int n=0;n<4;n++)Check(PantelleriaScenario.Ships.Count(x=>x.nation==n)==expected[n],"national count "+n);
            Check(PantelleriaScenario.Ships.Count(x=>x.Side==0)==21&&PantelleriaScenario.Ships.Count(x=>x.Side==1)==21,"21 per alliance");
            Check(s.Date.AddHours(1).Hour==14&&s.Date.AddHours(1).Minute==30&&s.Date.Day==14&&s.Date.Month==2&&s.Date.Year==1901,"scenario clock / UTC conversion");
            for(int n=0;n<4;n++){var p=s.ships[PantelleriaScenario.Leaders[n]];Check(Mathf.Abs(p.Position.magnitude-32000*.9144f)<.02f,"leading ship range "+n);Check(Mathf.Abs(Mathf.DeltaAngle(GunneryTools.Bearing(p.Position),PantelleriaScenario.BearingsFromX[n]))<.01f,"reciprocal opening bearing "+n);Check(p.heading==PantelleriaScenario.Courses[n]&&p.speed==15,"course and initial speed "+n);}
            Check(s.windKnots==20&&s.windFrom==260&&s.dobVisibilityPercent==60,"printed weather preset");
            var csv=new StringBuilder("pass,ship,nation,division,class,vertices,meshes,modeled_guns,funnels\n");
            var baseline=File.ReadAllLines("Documentation/Pantelleria-Polish-Baseline.csv").Skip(1).Where(x=>!string.IsNullOrWhiteSpace(x)).Select(x=>x.Split(',')).ToArray();
            var comparison=new StringBuilder("ship,baseline_vertices,detail_vertices,vertex_ratio,baseline_meshes,detail_meshes,mesh_ratio\n");
            var budgets=new System.Collections.Generic.List<(bool ok,string message)>();
            for(int i=0;i<42;i++) {
                var spec=PantelleriaScenario.Ships[i];var go=Shipwright.CreatePantelleriaShip(i);
                try {
                    var colours=go.GetComponent<NavalColours>();var allMeshes=go.GetComponentsInChildren<MeshFilter>(true);var meshes=allMeshes.Where(m=>!colours.IsRigging(m.transform)).ToArray();int vertices=meshes.Sum(m=>m.sharedMesh.vertexCount);Check(meshes.Length>=8&&vertices>600,"model baseline "+i);
                    int oldVertices=int.Parse(baseline[i][5]),oldMeshes=int.Parse(baseline[i][6]);
                    budgets.Add((vertices==oldVertices&&meshes.Length==oldMeshes,$"polish preserves geometry {i}: {vertices}/{oldVertices} vertices, {meshes.Length}/{oldMeshes} meshes"));
                    var mainGuns=go.GetComponentsInChildren<GunMount>().Where(g=>g.GetComponentInParent<TurretAssembly>()!=null).ToArray();
                    Check(mainGuns.Length==spec.model.MainGuns,"all main tubes retained "+i);
                    float visualDeck=spec.model.name=="Hood"||spec.model.name=="Jemmapes"?3.2f:4.8f;
                    foreach(var assembly in go.GetComponentsInChildren<TurretAssembly>()) {
                        float bottom=assembly.GetComponentsInChildren<MeshFilter>().SelectMany(f=>f.sharedMesh.vertices.Select(v=>go.transform.InverseTransformPoint(f.transform.TransformPoint(v)).y)).Min();
                        Check(bottom<=visualDeck+.01f&&bottom>=visualDeck-.10f,"barbette foundation reaches deck "+i+" / "+assembly.name);
                    }
                    foreach(var gun in mainGuns)Check(spec.model.open?HasCarriageContact(gun):gun.transform.localPosition.y>.78f&&gun.transform.localPosition.y<2.4f,"supported main gun "+i+" / "+gun.name);
                    Check(colours!=null&&colours.Nation==spec.nation&&colours.Ensign!=null&&colours.Pennant!=null,"national cloth attachments "+i);
                    Check(colours.DressingRoot.GetComponentsInChildren<Renderer>(true).Count(r=>r.sharedMaterial.shader.name=="Ironclads/Naval Cloth")==NavalColours.DressFlagCount&&colours.SignalRoot.GetComponentsInChildren<Renderer>(true).Count(r=>r.sharedMaterial.shader.name=="Ironclads/Naval Cloth")==NavalColours.SignalFlagCount,"complete dressing and signal hoists "+i);
                    foreach(var group in new[]{colours.DressingRoot,colours.SignalRoot}) {
                        float bottom=group.GetComponentsInChildren<MeshFilter>(true).SelectMany(m=>m.sharedMesh.vertices.Select(v=>go.transform.InverseTransformPoint(m.transform.TransformPoint(v)).y)).Min();
                        Check(bottom<=visualDeck+.06f&&bottom>=visualDeck-.10f,"rigging anchored at deck "+i+" / "+group.name);
                    }
                    colours.SetRiggingMode(0);Check(!colours.DressingRoot.gameObject.activeSelf&&!colours.SignalRoot.gameObject.activeSelf&&colours.Ensign.gameObject.activeSelf,"colours-only display "+i);
                    colours.SetRiggingMode(1);Check(!colours.DressingRoot.gameObject.activeSelf&&colours.SignalRoot.gameObject.activeSelf,"yardarm hoists display "+i);
                    colours.SetRiggingMode(2);Check(colours.DressingRoot.gameObject.activeSelf&&!colours.SignalRoot.gameObject.activeSelf,"full dress display "+i);
                    colours.Refresh(90,10,270,20,0);Check(Vector3.Distance(colours.ApparentWind,Vector3.right*10)<.001f&&Vector3.Dot(colours.Ensign.right,Vector3.right)>.999f,"apparent wind subtracts ship velocity "+i);
                    colours.Refresh(180,10,270,20,0);Check(Vector3.Distance(colours.ApparentWind,new Vector3(20,0,10))<.001f,"course change redirects cloth "+i);
                    colours.Refresh(0,0,0,0,0);Check(float.IsFinite(colours.Ensign.rotation.x)&&colours.ApparentWind==Vector3.zero,"calm cloth orientation finite "+i);
                    comparison.AppendLine(FormattableString.Invariant($"{spec.name},{oldVertices},{vertices},{vertices/(float)oldVertices:F3},{oldMeshes},{meshes.Length},{meshes.Length/(float)oldMeshes:F3}"));
                    Check(allMeshes.All(m=>m.sharedMesh.vertices.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z))),"finite model and rigging geometry "+i);
                    Check(go.GetComponentsInChildren<GunMount>().Length==spec.model.MainGuns+spec.model.secondary+6,"working battery count "+i);
                    Check(go.GetComponentsInChildren<CruiserFunnel>().Length==spec.model.funnels,"funnel outlets "+i);
                    Check(go.GetComponentsInChildren<GunMount>().All(g=>float.IsFinite(g.Muzzle.x)&&g.Direction.sqrMagnitude>.99f),"muzzle bindings "+i);
                    csv.AppendLine($"8,{spec.name},{PantelleriaScenario.Nations[spec.nation]},{PantelleriaScenario.Divisions[spec.division]},{spec.model.name},{vertices},{meshes.Length},{spec.model.MainGuns+spec.model.secondary+6},{spec.model.funnels}");
                }finally{UnityEngine.Object.DestroyImmediate(go);}
            }
            File.WriteAllText("Artifacts/Pantelleria/rigging-base-model-counts.csv",csv.ToString());
            File.WriteAllText("Documentation/Pantelleria-Rigging-Base-Comparison.csv",comparison.ToString());
            foreach(var budget in budgets)Check(budget.ok,budget.message);
            Check(PantelleriaAppearance.ExhaustRate(false,1,0,0)==0,"extinguished boilers emit no new smoke");
            Check(PantelleriaAppearance.ExhaustRate(true,1,0,0)>PantelleriaAppearance.ExhaustRate(true,0,0,0),"smoke rate follows engine load");
            Check(PantelleriaAppearance.ExhaustShade(1,0)<PantelleriaAppearance.ExhaustShade(0,0),"loaded boilers produce darker smoke");
            Check(Mathf.Abs(PantelleriaAppearance.ExhaustPulse(0,0)-PantelleriaAppearance.ExhaustPulse(1,0))>.05f,"funnels have distinct pulse phases");
            s.pantelleria.divisionOrders[13]="Maintain contact";s.pantelleria.damagePercent[41]=52;s.pantelleria.sunk[40]=true;s.ships[41].dobOrderedHeading=12;
            var copy=Scenario.Parse(JsonUtility.ToJson(s));Check(copy.ships.Length==42&&copy.pantelleria.divisionOrders[13]=="Maintain contact"&&copy.pantelleria.damagePercent[41]==52&&copy.pantelleria.sunk[40]&&copy.ships[41].dobOrderedHeading==12,"full fleet orders and result save roundtrip");
            var packet=new TablePacket{scenarioId=PantelleriaScenario.Id,rulesetId="dob",ship=41,load=.5f,heading=1,orderedHeading=20,speed=15,orderedSpeed=15,turnClass=3};Check(MultiplayerSession.ValidShip(packet),"last fleet slot accepted over wire");packet.ship=42;Check(!MultiplayerSession.ValidShip(packet),"out-of-range fleet slot rejected");packet.ship=41;packet.orderedHeading=float.NaN;Check(!MultiplayerSession.ValidShip(packet),"nonfinite plotted course rejected");
            Check(Scenario.Parse(JsonUtility.ToJson(UlsanScenario.Create())).ShipSlots==7,"Ulsan serialization regression");Check(Scenario.Parse(JsonUtility.ToJson(JamesScenario.Create())).ShipSlots==6,"Stonewall serialization regression");
            Check(!s.TableSavePath.Equals(Scenario.SavePath),"separate save slot");File.WriteAllText("Artifacts/Pantelleria/editor-verification.txt","PASS: "+count+" checks: roster, all 42 models/batteries, national ensigns and pennants, apparent wind/course/calm response, preserved model budgets, persistence and protocol bounds.");Debug.Log("PANTELLERIA PASS: "+count+" editor checks.");
        }
        static bool HasCarriageContact(GunMount gun)
        {
            var assembly=gun.GetComponentInParent<TurretAssembly>().transform;var axis=gun.transform.localPosition;
            foreach(var filter in assembly.GetComponentsInChildren<MeshFilter>()) {
                var matrix=assembly.worldToLocalMatrix*filter.transform.localToWorldMatrix;var vertices=filter.sharedMesh.vertices;var triangles=filter.sharedMesh.triangles;
                for(int i=0;i<triangles.Length;i+=3)for(int edge=0;edge<3;edge++) {
                    var p=matrix.MultiplyPoint3x4(vertices[triangles[i+edge]]);var q=matrix.MultiplyPoint3x4(vertices[triangles[i+(edge+1)%3]]);
                    if(Mathf.Min(p.y,q.y)<=.91f&&Mathf.Max(p.y,q.y)>=1.60f&&Mathf.Abs(p.x-axis.x)<.65f&&Mathf.Abs(q.x-axis.x)<.65f&&Mathf.Abs(p.z-axis.z)<1.3f&&Mathf.Abs(q.z-axis.z)<1.3f)return true;
                }
            }
            return false;
        }
        public static void VerifyTransportCapacity()
        {
            var clients=new System.Collections.Generic.List<DirectTcpTransport>();using(var host=new DirectTcpTransport())try {
                host.Listen(0,true);for(int i=0;i<10;i++){var client=new DirectTcpTransport();clients.Add(client);client.Connect("127.0.0.1",host.Port);}
                int connected=0;var limit=DateTime.UtcNow.AddSeconds(10);while(connected<10&&DateTime.UtcNow<limit){while(host.Poll(out var notice))if(notice.kind=="connected")connected++;System.Threading.Thread.Sleep(10);}
                Check(connected==10,"transport admits more than eight captain connections");File.WriteAllText("Artifacts/Pantelleria/transport-capacity.txt","PASS: 10 simultaneous loopback peer connections accepted (transport cap 31 guests; lobby cap 32 including host).");
            } finally {foreach(var client in clients)client.Dispose();}
        }
    }
}
