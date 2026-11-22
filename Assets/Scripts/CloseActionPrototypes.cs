using System.Linq;
namespace Ironclads
{
    public sealed class CloseActionPrototype
    {
        public readonly string key,label;
        public readonly int side,rate,representative;
        public readonly float length,beam,deck,sternWidth,arch,quarterProjection,camber;
        public readonly int lights;
        public CloseActionPrototype(string key,string label,int side,int rate,int representative,float length,float beam,float deck,float width,float arch,float quarter,float camber,int lights)
        {this.key=key;this.label=label;this.side=side;this.rate=rate;this.representative=representative;this.length=length;this.beam=beam;this.deck=deck;sternWidth=width;this.arch=arch;quarterProjection=quarter;this.camber=camber;this.lights=lights;}
    }
    public static class CloseActionPrototypes
    {
        // Measured-looking proportions are representative reconstructions; source limits
        // and exact / related-family assignments are recorded in the visual guide.
        public static readonly CloseActionPrototype[] All={
            new CloseActionPrototype("British-Slade-74","Bellona / Arrogant family 74",0,74,0,51.2f,14.3f,6,.36f,.48f,.12f,.20f,7),
            new CloseActionPrototype("British-Culloden-74","Culloden family 74",0,74,6,52.4f,14.6f,6.1f,.35f,.40f,.105f,.18f,7),
            new CloseActionPrototype("British-Alfred-74","Alfred class 74",0,74,8,51.2f,14.5f,6.1f,.375f,.62f,.13f,.24f,7),
            new CloseActionPrototype("British-Portland-50","Portland class 50",0,50,4,44.5f,12.2f,5.1f,.37f,.64f,.12f,.22f,7),
            new CloseActionPrototype("French-Temeraire-74","Temeraire family 74",1,74,10,55.9f,14.9f,6.2f,.36f,.85f,.14f,.34f,9),
            new CloseActionPrototype("French-Centaure-74","Centaure class 74",1,74,18,54.9f,14.4f,6.1f,.35f,.67f,.125f,.28f,7),
            new CloseActionPrototype("French-Citoyen-74","Citoyen family 74",1,74,17,55.1f,14.1f,6,.37f,.74f,.15f,.31f,7),
            new CloseActionPrototype("French-Souverain-74","Souverain / earlier 74 family",1,74,15,53.3f,14.1f,6,.375f,.94f,.155f,.38f,7),
            new CloseActionPrototype("French-Tonnant-80","Tonnant class 80",1,80,12,59.3f,15.5f,6.5f,.37f,1.05f,.15f,.38f,9),
            new CloseActionPrototype("French-Ocean-120","Ocean family 120 / Orient",1,120,16,65.2f,16.8f,8.1f,.38f,1.2f,.16f,.44f,11)
        };
        public static CloseActionPrototype For(CloseActionSpec ship)
        {
            if(ship.side==0){if(ship.rate==50)return All[3];if(ship.name=="Alexander")return All[2];if(ship.name=="Culloden"||ship.name=="Theseus"||ship.name=="Minotaur")return All[1];return All[0];}
            if(ship.rate==120)return All[9];if(ship.rate==80)return All[8];
            if(ship.name=="Peuple Souverain"||ship.name=="Guerrier")return All[7];
            if(ship.name=="Conquerant")return All[6];if(ship.name=="Heureux")return All[5];return All[4];
        }
        public static CloseActionSpec[] Representatives=>All.Select(p=>CloseActionScenario.BaseFleet[p.representative]).ToArray();
    }
}
