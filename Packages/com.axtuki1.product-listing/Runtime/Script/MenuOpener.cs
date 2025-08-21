
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace AX.ProductListing
{
    public class MenuOpener : UdonSharpBehaviour
    {

        [SerializeField]
        private LayoutElement content;

        [SerializeField]
        private Transform triangle;
        
        [SerializeField]
        private float openSpeed = 10f;

        [SerializeField]
        private float openRotate = 90f;

        private bool isOpen;

        [SerializeField]
        private float closeRotate;

        private void Start()
        {
            closeRotate = triangle.localRotation.eulerAngles.z;
        }

        public override void Interact()
        {
            isOpen = !isOpen;
            
        }

        public void Update()
        {
            content.gameObject.SetActive(isOpen);
            
            Quaternion currentRot = triangle.localRotation;
            Quaternion targetRot = Quaternion.Euler(0, 0, isOpen ? openRotate : closeRotate);
            triangle.localRotation = Quaternion.Lerp(currentRot, targetRot, Time.deltaTime * openSpeed);
            
        }
    }
}
