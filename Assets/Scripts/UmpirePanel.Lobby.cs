using System.Linq;
using UnityEngine;

namespace Ironclads
{
    public partial class UmpirePanel
    {
        bool hostUmpire=true;
        int assignmentPlayer;
        string[] SideNames=>scene.scenario.Pantelleria?new[]{"Britain / Italy","France / Russia"}:scene.scenario.CloseAction?new[]{"British","French"}:scene.scenario.DoB?new[]{"Russia","Japan"}:new[]{"Union","Confederacy"};
        void MultiplayerLobby()
        {
            var net=scene.network;
            Heading("MULTIPLAYER / DIRECT TCP LOBBY");Text(net.ConnectionStatus);
            if(!net.Active){
                GUILayout.Label("Your captain name",small);net.PlayerName=GUILayout.TextField(net.PlayerName,24,field);
                Heading("HOST A TABLE");
                hostUmpire=GUILayout.Toggle(hostUmpire,"Host as separate umpire (no ship)",toggle);
                if(!hostUmpire){hostShip=Mathf.Clamp(hostShip,0,scene.scenario.ShipSlots-1);hostShip=GUILayout.SelectionGrid(hostShip,Enumerable.Range(0,scene.scenario.ShipSlots).Select(scene.scenario.ShortName).ToArray(),2,button);}
                GUILayout.Label("TCP port",small);networkPort=GUILayout.TextField(networkPort,5,field);
                GUILayout.Label("Session code",small);networkCode=GUILayout.TextField(networkCode,64,field);
                if(Button("New session code"))networkCode=MultiplayerSession.NewCode();
                if(Button("Open lobby")){if(int.TryParse(networkPort,out int port))net.Host(port,hostUmpire?-1:hostShip,networkCode);else scene.status="Enter a TCP port from 1 to 65535.";}
                Heading("JOIN A HOST");GUILayout.Label("Host IPv4 address",small);networkAddress=GUILayout.TextField(networkAddress,45,field);
                if(Button("Join IP / port / code above")){if(int.TryParse(networkPort,out int port))net.Join(networkAddress.Trim(),port,networkCode);else scene.status="Enter the host's TCP port.";}
                Text("Capacity: "+net.PlayerCapacity+" players including host. Use matching protocol "+MultiplayerSession.Protocol+" builds. Players connect first, then choose ships in this lobby.");
                Text("Use the host's LAN IP on your network, or public IP over the internet. The host usually needs TCP port forwarding and a firewall allowance. Players normally need no port forwarding. No public server browser or relay service is required.");
                Text("Direct TCP is unencrypted. A shared VPN can provide connectivity where direct hosting is unavailable.");return;
            }
            if(net.IsHost)Text($"Host LAN IPv4: {net.HostAddresses}\nTCP {net.Port} / code {net.SessionCode}\nFor internet guests, share your public IP.");
            if(!net.IsHost&&!net.Connected){if(Button("Cancel connection"))net.Stop();return;}
            var lobby=net.Lobby;
            Heading(lobby.running?"IN MATCH / CONTROL ASSIGNMENTS":"LOBBY / SHIP ASSIGNMENTS");Text(lobby.message);
            Heading("PENDING CONTROL REQUESTS");
            foreach(var request in lobby.requests.ToArray()){
                bool cross=lobby.players.FirstOrDefault(p=>p.id==request.player)?.side>=0&&lobby.players.First(p=>p.id==request.player).side!=net.ShipSide(request.ship);
                Text(net.PlayerLabel(request.player)+" requests "+scene.scenario.ShortName(request.ship)+"\nCurrent: "+net.PlayerLabel(net.OwnerFor(request.ship))+(cross?"\nCross-side request":""));
                if(net.IsHost){GUILayout.BeginHorizontal();if(Button("Approve"))net.DecideRequest(request.id,true);if(Button("Decline"))net.DecideRequest(request.id,false);GUILayout.EndHorizontal();}
            }
            Text("You: "+net.PlayerLabel(net.LocalPlayer));
            var local=lobby.players.FirstOrDefault(p=>p.id==net.LocalPlayer);
            int oldSide=local==null?2:local.side<0?2:local.side;
            int chosenSide=GUILayout.SelectionGrid(oldSide,new[]{SideNames[0],SideNames[1],"Umpire / open"},1,button);
            if(chosenSide!=oldSide)net.SetSide(chosenSide==2?-1:chosenSide);
            Text("Side is a preference. The host can approve any ship, including cross-side transfers.");
            Heading("PLAYERS");
            foreach(var p in lobby.players.ToArray()){
                Text(net.PlayerLabel(p.id)+" / "+(p.side<0?"open":SideNames[p.side])+" / "+(!p.connected?"disconnected":p.ready?"ready":"not ready"));
                if(net.IsHost&&p.id>0&&Button("Remove "+p.name+" / release ships"))net.KickPlayer(p.id);
            }
            if(!lobby.running&&local!=null&&Button(local.ready?"Mark not ready":"Ready to start"))net.SetReady(!local.ready);
            if(net.IsHost){
                if(!lobby.running){if(Button("Start match / all connected players ready"))net.StartMatch();}
                else if(Button("Pause orders / return to lobby"))net.ReturnToLobby();
            }
            if(net.IsHost){
                Heading("ASSIGN SHIPS TO");var players=lobby.players.Where(p=>p.connected).ToArray();
                assignmentPlayer=Mathf.Clamp(assignmentPlayer,0,players.Length);
                assignmentPlayer=GUILayout.SelectionGrid(assignmentPlayer,new[]{"Unassigned"}.Concat(players.Select(p=>net.PlayerLabel(p.id))).ToArray(),1,button);
            }
            Heading("SHIPS / ONE COMMANDER PER SHIP");
            for(int i=0;i<scene.scenario.ShipSlots&&i<lobby.owners.Length;i++){
                Text(scene.scenario.ShortName(i)+" / "+SideNames[net.ShipSide(i)]+"\nCommander: "+net.PlayerLabel(net.OwnerFor(i)));
                if(Button("Inspect "+scene.scenario.ShortName(i))){if(scene.scenario.Hampton&&i==2)scene.support.Inspect(2);else if(scene.scenario.Hampton&&i==3)scene.support.InspectPatrick();else scene.Focus(i);}
                if(net.OwnerFor(i)!=net.LocalPlayer&&Button(net.IsHost?"Take command as host":"Request control"))net.RequestControl(i);
                if(net.IsHost){var players=lobby.players.Where(p=>p.connected).ToArray();int target=assignmentPlayer>0&&assignmentPlayer<=players.Length?players[assignmentPlayer-1].id:-1;if(target!=net.OwnerFor(i)&&Button("Assign to "+net.PlayerLabel(target)))net.AssignShip(i,target);}
            }
            Text("Requests never grant control until the host approves. One captain may command several ships. Disconnected ships remain reserved; the host may reassign them.");
            if(Button(net.IsHost?"Stop hosting / retain table offline":"Disconnect / retain table offline"))net.Stop();
        }
    }
}
