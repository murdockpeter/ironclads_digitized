using System.Collections.Generic;
using UnityEngine;
namespace Ironclads
{
    public static partial class Shipwright
    {
        static Vector3 SailingInteriorPoint(CloseActionSpec spec,float z,float across,int tier,float inset=.20f)
        {
            var lines=CloseActionHullLines.Get(spec.Length,spec.Beam);
            float y=tier<0?lines.DeckHeight(z)-.08f:lines.PlankHeight(z,1.55f+tier*2.05f)-.85f;
            var p=SailingShellPoint(1,z,y,spec.Length,spec.Beam,spec.Deck);
            p.x=across*Mathf.Max(0,p.x-inset);p.y+=.10f*(1-across*across);return p;
        }
        static void SailingInterior(Transform root,CloseActionSpec spec)
        {
            var interior=new GameObject("Closed hull interior").transform;interior.SetParent(root,false);
            int decks=spec.rate==120?3:2;
            for(int tier=0;tier<decks;tier++)SailingInteriorDeck(interior,spec,tier);
            SailingInteriorDeck(interior,spec,-1);
            var frames=new GameObject("Interior deck beams and pillars").transform;frames.SetParent(interior,false);
            for(int tier=0;tier<decks;tier++)for(int n=0;n<12;n++){
                float z=Mathf.Lerp(-spec.Length*.40f,spec.Length*.40f,n/11f);int ceiling=tier==decks-1?-1:tier+1;
                Vector3 p=SailingInteriorPoint(spec,z,0,ceiling);float half=SailingInteriorPoint(spec,z,1,ceiling).x;
                Box(frames,"Transverse gun-deck beam",p+Vector3.down*.24f,new Vector3(half*2,.18f,.22f),caOak);
                if(n%2==0){Vector3 bottom=SailingInteriorPoint(spec,z,0,tier);SailingRod(frames,"Gun-deck support pillar",bottom,p+Vector3.down*.3f,.12f,caOak,sides:8);}
            }
            Combine(frames,6);
        }
        static void SailingInteriorDeck(Transform root,CloseActionSpec spec,int tier)
        {
            const int along=64,across=12;int stride=across+1,layer=(along+1)*stride;
            var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();
            for(int face=0;face<2;face++)for(int z=0;z<=along;z++)for(int x=0;x<=across;x++){
                Vector3 p=SailingInteriorPoint(spec,Mathf.Lerp(-spec.Length*.5f,spec.Length*.5f,z/(float)along),Mathf.Lerp(-1,1,x/(float)across),tier,face==0?0:.20f);
                p.y-=face*.16f;v.Add(p);uv.Add(new Vector2(x/(float)across,z/(float)along));
            }
            for(int z=0;z<along;z++)for(int x=0;x<across;x++){
                int a=z*stride+x,b=a+1,c=a+stride,d=c+1;
                t.AddRange(new[]{a,c,b,b,c,d,a+layer,b+layer,c+layer,b+layer,d+layer,c+layer});
            }
            void Edge(int a,int b){t.AddRange(new[]{a,b,b+layer,a,b+layer,a+layer,b,a,a+layer,b,a+layer,b+layer});}
            for(int z=0;z<along;z++){Edge(z*stride,(z+1)*stride);Edge((z+1)*stride+across,z*stride+across);}
            for(int x=0;x<across;x++){Edge(x+1,x);Edge(along*stride+x,along*stride+x+1);}
            Part(tier<0?"Weather-deck inner ceiling":"Solid gun deck "+tier,root,Mesh("Timber deck with closed top, underside and edges",v,t,uv),caOak);
        }
    }
}
