
using UdonSharp;
using UnityEngine;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;

namespace AX.ProductListing
{
    public class AttentionWindow : UdonSharpBehaviour
    {
        public string listingId;
    
        public override void Interact()
        {
            Store.OpenListing(listingId);
        }
    }

}
