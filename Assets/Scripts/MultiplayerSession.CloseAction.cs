using System;
using System.Linq;
using UnityEngine;
namespace Ironclads
{
    public partial class MultiplayerSession
    {
        public bool SubmitCloseAction(string kind,int ship,string payload)
        {
            if(!Active||IsHost||!CanControl(ship)||payload==null||payload.Length>4096)return false;
            return Send(new TablePacket{type=kind,ship=ship,state=payload,epoch=Lobby.epochs[ship],generation=Lobby.generation,turnNumber=scene.scenario.turn,scenarioId=scene.scenario.scenarioId,rulesetId=scene.scenario.rulesetId});
        }
        void ReceiveCloseAction(Client client,TablePacket packet)
        {
            if(!scene.scenario.CloseAction||!Authorized(client,packet)||packet.state==null||packet.state.Length>4096)return;
            bool okay=false;var table=scene.closeAction;
            if(packet.type=="ca-order"){
                var order=JsonUtility.FromJson<CloseActionOrder>(packet.state);
                if(order!=null&&order.ship==packet.ship&&order.turn==packet.turnNumber&&order.epoch==packet.epoch&&order.generation==packet.generation)okay=table.ReceiveOrder(order);
            }else if(packet.type=="ca-fire"){
                var fire=JsonUtility.FromJson<CloseActionFire>(packet.state);
                if(fire!=null&&fire.ship==packet.ship){fire.resolved=false;fire.hit=null;fire.result="";okay=table.ReceiveFire(fire);}
            }else{
                var signal=JsonUtility.FromJson<CloseActionSignal>(packet.state);
                if(signal!=null&&signal.sender==packet.ship)okay=table.ReceiveSignal(signal,client.player);
            }
            SendTo(client.peer,new TablePacket{type="ca-ack",name=(okay?"Accepted: ":"Rejected: ")+scene.status});
        }
        public void CloseActionDisplay(int ship,int mask)
        {
            scene.GetComponent<BatteryEffects>().FireCloseActionLocal(ship,mask);
            if(Active&&IsHost)Broadcast(new TablePacket{type="ca-display",ship=ship,side=mask});
        }
        public void DeliverCloseActionSignal(CloseActionSignal signal)
        {
            if(!Active||!IsHost)return;
            foreach(var client in clients.Values.Where(c=>c.player>=0&&c.rejectAt==0)){
                bool friendly=Enumerable.Range(0,Lobby.owners.Length).Any(i=>OwnerFor(i)==client.player&&ShipSide(i)==signal.side);
                if(friendly)SendTo(client.peer,new TablePacket{type="ca-signal-delivery",state=JsonUtility.ToJson(signal)});
            }
        }
    }
}
