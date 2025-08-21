
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace AX.ProductListing
{
    public class ItemProduct : UdonSharpBehaviour
    {
        
        [SerializeField]
        private UdonProduct product;
        
        [SerializeField]
        private Transform[] itemTransforms;

        [SerializeField]
        private GameObject purchasedLabel;

        [SerializeField]
        private string listingId;
        
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private AudioClip purchaseSound, expiredSound;

        [SerializeField]
        private AttentionWindow attention;
        
        [SerializeField]
        private AudioSource attentionAudioSource; 
        
        [SerializeField]
        private AudioClip attentionSound;
        
        /**
         * 一人でも購入してたらインスタンス内全員が利用可能にする
         * falseの場合は購入者のみPickup可能
         */
        [SerializeField]
        [Tooltip("一人でも購入してたらインスタンス内全員が利用可能にする\nfalseの場合は購入者のみPickup可能")]
        private bool isPurchaseShared = true;
        
        private Vector3[] positions;
        private Quaternion[] rotations;
        private bool isInitialized = false;
        
        void Start()
        {
            positions = new Vector3[itemTransforms.Length];
            rotations = new Quaternion[itemTransforms.Length];
            for (int i = 0; i < itemTransforms.Length; i++)
            {
                if (Utilities.IsValid(itemTransforms[i]))
                {
                    positions[i] = itemTransforms[i].position;
                    rotations[i] = itemTransforms[i].rotation;
                }
                else
                {
                    positions[i] = Vector3.zero;
                    rotations[i] = Quaternion.identity;
                }
            }
            CheckPermission();
            SendCustomEventDelayedSeconds(nameof(Initialized), 60f * 1);
        }

        public void OpenPurchasePage()
        {
            if (Utilities.IsValid(attention))
            {
                attention.listingId = listingId;
                attention.gameObject.SetActive(true);
                if (Utilities.IsValid(attentionSound) && Utilities.IsValid(attentionAudioSource))
                {
                    attentionAudioSource.PlayOneShot(attentionSound);
                }
            }
            else
            {
                Store.OpenListing(listingId);
            }
            
        }

        public void ResetPositions()
        {
            for (int i = 0; i < itemTransforms.Length; i++)
            {
                if (Utilities.IsValid(itemTransforms[i]))
                {
                    if (!Networking.IsOwner(Networking.LocalPlayer, itemTransforms[i].gameObject)) continue;
                    if (Utilities.IsValid(positions) && i < positions.Length)
                    {
                        itemTransforms[i].position = positions[i];
                    }
                    else
                    {
                        itemTransforms[i].position = Vector3.zero;
                    }

                    if (Utilities.IsValid(rotations) && i < rotations.Length)
                    {
                        itemTransforms[i].rotation = rotations[i];
                    }
                    else
                    {
                        itemTransforms[i].rotation = Quaternion.identity;
                    }
                }
            }
        }

        public void CheckPermission()
        {
            bool isProductOwned = false, isProductAnyOwned = false;
            
            // VRC側の購入情報の読み出しまで読まないようにする
            if (isInitialized)
            {
                isProductAnyOwned = Store.DoesAnyPlayerOwnProduct(product);;
                if (isPurchaseShared) // 共有購入の場合は、インスタンス内の誰かが購入していれば有効
                {
                    isProductOwned = isProductAnyOwned;
                }
                else // 個人購入の場合は、ローカルプレイヤーが購入しているかどうかを確認
                {
                    isProductOwned = Store.DoesPlayerOwnProduct(Networking.LocalPlayer, product);
                }
            }
            
            for (int i = 0; i < itemTransforms.Length; i++)
            {
                if (Utilities.IsValid(itemTransforms[i]))
                {
                    var col = itemTransforms[i].GetComponent<Collider>();
                    if (Utilities.IsValid(col))
                    {
                        col.enabled = isProductOwned;
                    }
                    var pickup = itemTransforms[i].GetComponent<VRC_Pickup>();
                    if (Utilities.IsValid(pickup))
                    {
                        if(!isProductOwned) pickup.Drop();
                    }
                }
            }
            if (!isProductAnyOwned)
            {
                ResetPositions();
            }
            
            // 権利が有効の場合は購入済みということを示す
            purchasedLabel.SetActive(isProductOwned);
            
            // 定期実行する
            SendCustomEventDelayedSeconds(nameof(CheckPermission), 1f);
        }

        public void Initialized()
        {
            isInitialized = true;
        }
        
        public override void OnPurchaseConfirmed(IProduct product, VRCPlayerApi player, bool isNowPurchase)
        {
            Initialized();
            if (!player.isLocal || !this.product.Equals(product)) return;
            if(isNowPurchase) 
            {
                if (Utilities.IsValid(purchaseSound) && Utilities.IsValid(audioSource))
                {
                    audioSource.PlayOneShot(purchaseSound);
                }
            }
        }

        public override void OnPurchaseExpired(IProduct product, VRCPlayerApi player)
        {
            Initialized();
            if (!this.product.Equals(product)) return;
            if (Utilities.IsValid(expiredSound) && Utilities.IsValid(audioSource)) 
            { 
                audioSource.PlayOneShot(expiredSound);
            }
        }

        public override void OnPurchasesLoaded(IProduct[] products, VRCPlayerApi player)
        {
            Initialized();
        }

    }

}