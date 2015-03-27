using System;
using KSP;
using UnityEngine;
using KSPPluginFramework;

namespace RealScience
{
    public class UserSettings : ConfigNodeStorage
    {
        public UserSettings(String FilePath) : base(FilePath) {
        }


        [Persistent] public bool debugLog = false;
        [Persistent] public bool favorLowPowerAntenna = true;

        [Persistent] public int kscWindowPage = 0;
        [Persistent] public PersistentVector2 currentResearchScrollPositionStored = new PersistentVector2();
        public Vector2 currentResearchScrollPosition = new Vector2(0,0);

        // Unity/KSP can't store some more complex data types so we provide classes to convert
        [Persistent] public PersistentRect kscWindowPositionStored = new PersistentRect();
        public Rect kscWindowPosition = new Rect(0,0,0,0);
        [Persistent] public PersistentRect flightWindowPositionStored = new PersistentRect();
        public Rect flightWindowPosition = new Rect(250,100,0,0);

        public override void OnDecodeFromConfigNode()
        {
            kscWindowPosition = kscWindowPositionStored.ToRect();
            flightWindowPosition = flightWindowPositionStored.ToRect();
        }

        public override void OnEncodeToConfigNode()
        {
            kscWindowPositionStored = kscWindowPositionStored.FromRect(kscWindowPosition);
            flightWindowPositionStored = flightWindowPositionStored.FromRect(flightWindowPosition);
        }

    }


    public class PersistentVector2 : ConfigNodeStorage
    {
        [Persistent] public float x;
        [Persistent] public float y;

        public Vector2 ToVector2() 
        {
            return new Vector2(x, y);
        }

        public PersistentVector2 FromVector2(Vector2 vectorToStore)
        {
            this.x = vectorToStore.x;
            this.y = vectorToStore.y;
            return this;
        }
    }

    public class PersistentRect : ConfigNodeStorage
    {
        [Persistent] public float x;
        [Persistent] public float y;
        [Persistent] public float width;
        [Persistent] public float height;

        public Rect ToRect()
        { 
            return new Rect(x, y, width, height); 
        }
        public PersistentRect FromRect(Rect rectToStore)
        {
            this.x = rectToStore.x;
            this.y = rectToStore.y;
            this.width = rectToStore.width;
            this.height = rectToStore.height;
            return this;
        }
    }
}

